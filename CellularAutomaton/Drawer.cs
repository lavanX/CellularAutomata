using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;

namespace CellularAutomata
{
    static class BitmapExtension
    {
        public static void SetPixelValue(this WriteableBitmap bitmap, int column, int row, Color color)
        {
            Int32Rect rect = new Int32Rect(
                column,
                row,
                1,
                1);
            bitmap.WritePixels(rect, new byte[] {color.B, color.G, color.R, 0}, 4, 0);
        }
    }
}
