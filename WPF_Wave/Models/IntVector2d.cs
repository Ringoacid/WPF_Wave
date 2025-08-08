using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Wave.Models;

public class IntVector2d
{
    public int X { get; set; }
    public int Y { get; set; }

    public IntVector2d(int x, int y)
    {
        X = x;
        Y = y;
    }

    public IntVector2d(double x, double y)
    {
        X = (int)x;
        Y = (int)y;
    }

    public IntVector2d() : this(0, 0) { }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }

    public static IntVector2d operator +(IntVector2d a, IntVector2d b)
    {
        return new IntVector2d(a.X + b.X, a.Y + b.Y);
    }

    public static IntVector2d operator -(IntVector2d a, IntVector2d b)
    {
        return new IntVector2d(a.X - b.X, a.Y - b.Y);
    }

    public static IntVector2d operator *(IntVector2d a, int scalar)
    {
        return new IntVector2d(a.X * scalar, a.Y * scalar);
    }
}
