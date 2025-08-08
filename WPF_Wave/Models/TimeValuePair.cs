using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Wave.Models;

/// <summary>
/// 時間と値のペアを表すクラス
/// </summary>
public class TimeValuePair
{
    /// <summary>
    /// 時間
    /// </summary>
    public long Time { get; set; }

    /// <summary>
    /// 値
    /// </summary>
    public VariableValue Value { get; set; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="time">時間</param>
    /// <param name="value">値</param>
    public TimeValuePair(long time, VariableValue value)
    {
        Time = time;
        Value = value;
    }
}