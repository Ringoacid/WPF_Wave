using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WPF_Wave.Models;

namespace WPF_Wave.Helpers;

public static class WriteableBitmapDrawLine
{
    public static void DrawLine(this WriteableBitmap wb, IntVector2d from, IntVector2d to, Color lineColor)
    {
        wb.DrawLine(from.X, from.Y, to.X, to.Y, lineColor);
    }
}
