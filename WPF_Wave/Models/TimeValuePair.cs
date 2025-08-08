using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Wave.Models;

/// <summary>
/// ���Ԃƒl�̃y�A��\���N���X
/// </summary>
public class TimeValuePair
{
    /// <summary>
    /// ����
    /// </summary>
    public long Time { get; set; }

    /// <summary>
    /// �l
    /// </summary>
    public VariableValue Value { get; set; }

    /// <summary>
    /// �R���X�g���N�^
    /// </summary>
    /// <param name="time">����</param>
    /// <param name="value">�l</param>
    public TimeValuePair(long time, VariableValue value)
    {
        Time = time;
        Value = value;
    }
}