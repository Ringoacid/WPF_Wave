using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Numerics;

namespace WPF_Wave.Models;

public class Vcd
{
    /// <summary>
    /// 時間
    /// </summary>
    public DateTime? DateTime { get; set; }

    /// <summary>
    /// バージョン(例："Icarus Verilog")
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// トップモジュール
    /// </summary>
    public Module? TopModule { get; set; }

    /// <summary>
    /// 1nsに対するタイムスケール
    /// 1nsなら1
    /// 1psなら1000
    /// 1msなら1000000
    /// 1sなら1000000000
    /// </summary>
    public long TimeScale { get; set; } = 1;

    /// <summary>
    /// 時間をキー、IDと値のペアを値とする辞書
    /// </summary>
    public Dictionary<long, Dictionary<string, VariableValue>> Time_IDValue_Pairs { get; set; } = [];

    /// <summary>
    /// IDをキー、Variableオブジェクトを値とする辞書
    /// </summary>
    public Dictionary<string, Variable> ID_Variable_Pairs { get; set; } = [];

    /// <summary>
    /// シミュレーション全体の時間
    /// </summary>
    public long SimulationTime { get; private set; } = 0;

    /// <summary>
    /// ロードされているか
    /// </summary>
    public bool IsLoaded { get; private set; } = false;


    private void InitProperties()
    {
        DateTime = null;
        Version = null;
        TopModule = null;
        TimeScale = 1; // デフォルトは1ns
        Time_IDValue_Pairs.Clear();
        SimulationTime = 0;
        IsLoaded = false;
    }


    public void LoadFromFile(string filePath)
    {
        InitProperties();
        using var reader = new StreamReader(filePath);
        string? line;

        long currentTime = 0;
        while ((line = reader.ReadLine()) != null)
        {
            if (line == "$date")
            {
                ReadDate(reader);
                continue;
            }
            if (line == "$version")
            {
                ReadVersion(reader);
                continue;
            }
            if (line == "$timescale")
            {
                ReadTimeScale(reader);
                continue;
            }

            if (line.StartsWith("$scope"))
            {
                ReadScope(line, reader, true);
                continue;
            }

            if (line.StartsWith("#"))
            {
                line = line.Substring(1).Trim();
                if(!long.TryParse(line, out long time))
                {
                    throw new FormatException($"Invalid time format : {line}. Expected a valid number.");
                }

                currentTime = time * TimeScale; // タイムスケールに基づいて時間を調整
                SimulationTime = Math.Max(SimulationTime, currentTime); // 最後に読み込んだ時間をシミュレーション時間として保存
                continue;
            }

            if (line.StartsWith("b"))
            {
                line = line.Substring(1).Trim();
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if(parts.Length < 2)
                {
                    throw new FormatException($"Invalid valueString value format : {line}. Expected at least 2 parts.");
                }

                var id = parts[1];
                var value = StringToVariableValue(id ,parts[0]);

                SetTime_IDValue_Pair(currentTime, id, value);
                continue;
            }

            if (line.StartsWith("0") || line.StartsWith("1"))
            {
                VariableValue value;
                if (line.StartsWith("0"))
                {
                    value = VariableValue.SingleZero;
                }
                else
                {
                    value = VariableValue.SingleOne;
                }

                var id = line.Substring(1).Trim();

                SetTime_IDValue_Pair(currentTime, id, value);
                continue;
            }
        }

        IsLoaded = true;

        void SetTime_IDValue_Pair(long time, string id, VariableValue value)
        {
            if (Time_IDValue_Pairs.TryGetValue(time, out var dict))
            {
                // 既存の時間に対応する辞書がある場合、値を更新
                dict[id] = value;
            }
            else
            {
                // 新しい時間に対応する辞書を作成
                Time_IDValue_Pairs[time] = new Dictionary<string, VariableValue> { { id, value } };
            }
        }
    }



    [MemberNotNull(nameof(DateTime))]
    private void ReadDate(StreamReader reader)
    {
        var line = reader.ReadLine();
        if (line is null) throw new FormatException("Expected date after $date");

        // 例: "Mon Jun 09 21:46:49 2025"
        string dateString = line.Trim();

        // フォーマット文字列を定義
        // "ddd": 曜日 (Mon)
        // "MMM": 月の省略名 (Jun)
        // "dd": 日 (09)
        // "HH": 時 (21)
        // "mm": 分 (46)
        // "ss": 秒 (49)
        // "yyyy": 年 (2025)
        string format = "ddd MMM dd HH:mm:ss yyyy";

        // カルチャ情報としてInvariantCultureを指定することで、
        // 曜日や月の省略名がシステム設定に依存しないようにします。
        DateTime = System.DateTime.ParseExact(dateString, format, CultureInfo.InvariantCulture);

        line = reader.ReadLine();
        if (line is null || line.Trim() != "$end") throw new FormatException("Expected $end after date");
    }

    [MemberNotNull(nameof(Version))]
    private void ReadVersion(StreamReader reader)
    {
        var line = reader.ReadLine();
        if (line is null) throw new FormatException("Expected version after $version");

        // 例: "Icarus Verilog"
        Version = line.Trim();

        line = reader.ReadLine();
        if (line is null || line.Trim() != "$end") throw new FormatException("Expected $end after version");
    }

    [MemberNotNull(nameof(TimeScale))]
    private void ReadTimeScale(StreamReader reader)
    {
        var line = reader.ReadLine();
        if (line is null) throw new FormatException("Expected timescale after $timescale");
        // 例: "1ns"
        line = line.Trim();

        // 正規表現を使って数値と単位を分離
        var match = System.Text.RegularExpressions.Regex.Match(line, @"^(\d+)([a-zA-Z]+)$");
        if (!match.Success)
        {
            throw new FormatException($"Invalid timescale format : {line}. Expected format like '1ns', '1ps', etc.");
        }

        // 数値部分を取得
        if (!long.TryParse(match.Groups[1].Value, out long value))
        {
            throw new FormatException($"Invalid timescale value : {match.Groups[1]}. Must be a valid number.");
        }
        // 単位部分を取得
        string unit = match.Groups[2].Value.ToLowerInvariant();

        // 単位に応じてTimeScaleを設定
        TimeScale = unit switch
        {
            "ns" => value,
            "ps" => value * 1000,
            "us" => value * 1000000,
            "ms" => value * 1000000000,
            "s" => value * 1000000000000,
            _ => throw new FormatException($"Unknown timescale unit : {unit}. Valid units are 'ns', 'ps', 'us', 'ms', 's'.")
        };

        line = reader.ReadLine();
        if (line is null || line.Trim() != "$end")
        {
            throw new FormatException("Expected $end after timescale");
        }
    }

    private Module ReadScope(string line, StreamReader reader, bool isTop = false)
    {
        // lineは"$scope module t_Main $end"のような形式
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 4)
        {
            throw new FormatException("Invalid scope declaration format. Line must contain at least 4 parts.");
        }
        if (parts[0] != "$scope" || parts[^1] != "$end")
        {
            throw new FormatException("Invalid scope declaration format. Line must start with '$scope' and end with '$end'.");
        }

        string scopeType = parts[1];
        string scopeName = parts[2];
        if (scopeType != "module" && scopeType != "function")
        {
            throw new FormatException($"Unsupported scope type : {scopeType}. Only 'module' and 'function' are supported.");
        }

        var module = new Module(scopeName);
        if (isTop)
        {
            TopModule = module;
        }

        while (true)
        {
            var newLine = reader.ReadLine();

            if (newLine is null)
            {
                return module;
            }

            if (newLine.StartsWith("$upscope"))
            {
                return module;
            }

            if (newLine.StartsWith("$var"))
            {
                var variable = new Variable(newLine);
                ID_Variable_Pairs[variable.Id] = variable;
                module.ID_Variable_Pairs[variable.Id] = variable;
                continue;
            }

            if (newLine.StartsWith("$scope"))
            {
                module.SubModules.Add(ReadScope(newLine, reader, false));
            }
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="valueString">先頭の"b"が取り除かれた変数の値を示す文字列</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public VariableValue StringToVariableValue(string id, string valueString)
    {
        if (string.IsNullOrEmpty(valueString))
            throw new ArgumentException("Binary string cannot be null or empty.", nameof(valueString));

        var bitWidth = ID_Variable_Pairs[id].BitWidth;
        var val = new VariableValue(valueString);
        return val;
    }
}
