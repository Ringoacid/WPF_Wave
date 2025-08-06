using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Wave.Models;

/// <summary>
/// Icarus Verilogで作成されたワイヤ・レジスタ・パラメーターなどの値を保持するクラス
/// </summary>
public class VariableValue
{
    public static VariableValue SingleZero => new VariableValue([BitType.Zero]);
    public static VariableValue SingleOne => new VariableValue([BitType.One]);
    public static VariableValue SingleX => new VariableValue([BitType.X]);
    public static VariableValue SingleZ => new VariableValue([BitType.Z]);

    public enum BitType : byte
    {
        Zero = 0, // 0
        One = 1, // 1
        X = 2, // 不定値
        Z = 3, // ハイインピーダンス
    }

    private BitType[] _data;

    public BitType this[int index]
    {
        get => _data[index];
        set
        {
            _data[index] = value;
        }
    }

    public int BitWidth => _data.Length;
    public long BitLongWidth => _data.LongLength;

    public bool IsUndefined => _data.Any(bit => bit == BitType.X);
    public bool IsHighImpedance => _data.Any(bit => bit == BitType.Z);
    public bool HasValue => !_data.Any(bit => bit == BitType.X || bit == BitType.Z);

    public VariableValue(int bitWidth)
    {
        if (bitWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bitWidth), "Bit width must be greater than zero.");
        }
        _data = new BitType[bitWidth];

        for(int i = 0; i < bitWidth; i++)
        {
            _data[i] = BitType.X; // 初期値は不定値
        }
    }

    public VariableValue(BitType[] data)
    {
        _data = data;
    }

    public VariableValue(string binaryString)
    {
        if (binaryString.StartsWith("0b"))
        {
            binaryString = binaryString.Substring(2); // "0b"を除去
        }

        _data = new BitType[binaryString.Length];

        for (int i=binaryString.Length - 1, j = 0; i >= 0; i--, j++)
        {
            switch (binaryString[i])
            {
                case '0':
                    _data[j] = BitType.Zero;
                    break;
                case '1':
                    _data[j] = BitType.One;
                    break;
                case 'x':
                case 'X':
                    _data[j] = BitType.X;
                    break;
                case 'z':
                case 'Z':
                    _data[j] = BitType.Z;
                    break;
                default:
                    throw new FormatException("Invalid character in binary string.");
            }
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new();

        foreach (var bit in _data)
        {
            switch (bit)
            {
                case BitType.Zero:
                    sb.Append('0');
                    break;
                case BitType.One:
                    sb.Append('1');
                    break;
                case BitType.X:
                    sb.Append('X');
                    break;
                case BitType.Z:
                    sb.Append('Z');
                    break;
                default:
                    throw new FormatException("Invalid bit type in VariableValue.");
            }
        }

        return sb.ToString();
    }

    public BigInteger ToBigInteger()
    {
        if (!HasValue) throw new FormatException("This variable does not have valid value");

        string binaryString = ToString();

        var bigint = BigInteger.Parse(binaryString, System.Globalization.NumberStyles.BinaryNumber);

        return bigint;
    }
}
