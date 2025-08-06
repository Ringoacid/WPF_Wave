using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Wave.Models;

/// <summary>
/// 宣言されたワイヤもしくはレジスタの情報
/// </summary>
public class Variable
{
    /// <summary>
    /// 変数の種類
    /// </summary>
    public enum VariableType
    {
        Wire, // ワイヤ
        Reg, // レジスタ
        Integer, // 整数
        Parameter, // パラメータ
    }

    /// <summary>
    /// この変数の種類
    /// </summary>
    public VariableType Type { get; set; }

    /// <summary>
    /// ビット幅
    /// </summary>
    public int BitWidth { get; set; }

    /// <summary>
    /// 識別子
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// ソースファイル内の名前
    /// </summary>
    public string Name { get; set; }

    public Variable(VariableType type, int bitWidth, string id, string name)
    {
        Type = type;
        BitWidth = bitWidth;
        Id = id;
        Name = name;
    }

    public Variable(Variable other)
    {
        Type = other.Type;
        BitWidth = other.BitWidth;
        Id = other.Id;
        Name = other.Name;
    }

    public Variable(string line)
    {
        LoadFromString(line);
    }

    /// <summary>
    /// vcdファイル1行分の文字列から、ワイヤもしくはレジスタの情報を読み込む
    /// MemberNotNull属性は、指定されたメンバー（プロパティやフィールド）がnullではなくなることを保証
    /// </summary>
    /// <param name="line">vcdファイル1行分の文字列</param>
    /// <exception cref="FormatException">文字列のフォーマットに不備がある場合</exception>
    [MemberNotNull(nameof(Type), nameof(BitWidth) ,nameof(Id), nameof(Name))]
    public void LoadFromString(string line)
    {
        // 例: "$var reg 1 hoge piyo $end"
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 6)
        {
            throw new FormatException("Invalid wire/reg declaration format. Line must contain at least 6 parts.");
        }
        if (parts[0] != "$var" || parts[^1] != "$end")
        {
            throw new FormatException("Invalid wire/reg declaration format. Line must start with '$var' and end with '$end'.");
        }

        Type = parts[1] switch
        {
            "wire" => VariableType.Wire,
            "reg" => VariableType.Reg,
            "integer" => VariableType.Integer,
            "parameter" => VariableType.Parameter,
            _ => throw new FormatException($"Unknown variable type : \"{parts[1]}\". Valid types are 'wire' or 'reg'.")
        };

        if (!int.TryParse(parts[2], out int bitWidth))
        {
            throw new FormatException($"Failed to parse bit width : \"{parts[2]}\". It must be an integer.");
        }
        if (bitWidth < 1)
        {
            throw new FormatException($"Invalid bit width : \"{parts[2]}\". It must be a positive integer.");
        }
        BitWidth = bitWidth;

        Id = parts[3];

        Name = parts[4];
    }
}
