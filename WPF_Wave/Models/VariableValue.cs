using System.Collections;
using System.Numerics;
using System.Text;

namespace WPF_Wave.Models;

public enum StringFormat
{
    Binary, // 2進数
    Decimal, // 10進数
    Hexadecimal, // 16進数
}

/// <summary>
/// Icarus Verilogで作成されたワイヤ・レジスタ・パラメーターなどの値を保持するクラス
/// </summary>
public class VariableValue : IEnumerable<VariableValue.BitType>
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

        for (int i = 0; i < bitWidth; i++)
        {
            _data[i] = BitType.X; // 初期値は不定値
        }
    }

    public VariableValue(BitType[] data)
    {
        _data = data;
    }

    public VariableValue(string binaryString, int bitWidth)
    {
        if (binaryString.StartsWith("0b"))
        {
            binaryString = binaryString.Substring(2); // "0b"を除去
        }

        _data = new BitType[bitWidth];

        for (int i = binaryString.Length - 1, j = 0; i >= 0; i--, j++)
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

    public VariableValue(string binaryString) : this(binaryString, binaryString.StartsWith("0b") ? binaryString.Length - 2 : binaryString.Length)
    {
        
    }

    public VariableValue(VariableValue other)
    {
        _data = new BitType[other.BitWidth];
        Array.Copy(other._data, _data, other.BitWidth);
    }

    public override string ToString()
    {
        StringBuilder sb = new();

        for (int i = _data.Length - 1; i >= 0; i--)
        {
            var bit = _data[i];
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

    public string ToString(StringFormat format)
    {
        if (format == StringFormat.Binary)
        {
            return ToString();
        }
        else if (format == StringFormat.Decimal)
        {
            if (HasValue)
            {
                return ToBigInteger().ToString();
            }
            else
            {
                return "XXX";
            }
        }
        else if (format == StringFormat.Hexadecimal)
        {
            // 16進数表現に変換
            // 4bitごとに区切り、1bitでも不定値xの場合は"X"
            // ハイインピーダンスZの場合は"Z"
            // そうでなければ"0"～"F"までの文字列を生成

            StringBuilder sb = new();

            // ビット幅を4の倍数に調整（上位ビットを0でパディング）
            int paddedBitWidth = ((BitWidth + 3) / 4) * 4;

            // 4bitずつ処理（上位ビットから）
            for (int i = paddedBitWidth - 4; i >= 0; i -= 4)
            {
                bool hasX = false;
                bool hasZ = false;
                int nibbleValue = 0;

                // 4bitを処理（LSB→MSBの重みで構築）
                for (int k = 0; k < 4; k++)
                {
                    int bitIndex = i + k; // nibble内のLSBから順に0..3
                    BitType bit;

                    if (bitIndex >= BitWidth)
                    {
                        // パディング部分は0として扱う
                        bit = BitType.Zero;
                    }
                    else
                    {
                        bit = _data[bitIndex];
                    }

                    switch (bit)
                    {
                        case BitType.Zero:
                            break;
                        case BitType.One:
                            nibbleValue |= (1 << k);
                            break;
                        case BitType.X:
                            hasX = true;
                            break;
                        case BitType.Z:
                            hasZ = true;
                            break;
                    }
                }

                // 4bitの結果を文字に変換
                if (hasX)
                {
                    sb.Append('X');
                }
                else if (hasZ)
                {
                    sb.Append('Z');
                }
                else
                {
                    // 0-15を16進数文字に変換
                    sb.Append(nibbleValue.ToString("X"));
                }
            }

            return sb.ToString();
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(format), "Unsupported format type.");
        }
    }


    public BigInteger ToBigInteger()
    {
        if (!HasValue) throw new FormatException("This variable does not have valid value");

        string binaryString = ToString();

        var bigint = BigInteger.Parse(binaryString, System.Globalization.NumberStyles.BinaryNumber);

        return bigint;
    }

    public IEnumerator<BitType> GetEnumerator()
    {
        return _data.AsEnumerable().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
