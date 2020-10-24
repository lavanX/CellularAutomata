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
        //Copied from https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.imaging.writeablebitmap?view=netcore-3.1
        public static unsafe void SetPixelValue(this WriteableBitmap bitmap, int column, int row, Color color)
        {
            try
            {
                // Reserve the back buffer for updates.
                bitmap.Lock();

                unsafe
                {
                    // Get a pointer to the back buffer.
                    IntPtr pBackBuffer = bitmap.BackBuffer;

                    // Find the address of the pixel to draw.
                    pBackBuffer += row * bitmap.BackBufferStride;
                    pBackBuffer += column * 4;

                    // Compute the pixel's color.
                    int colorValueRGB = color.R << 16; // R
                    colorValueRGB |= color.G << 8;   // G
                    colorValueRGB |= color.B << 0;   // B


                    // Assign the color data to the pixel.
                    *((int*)pBackBuffer) = colorValueRGB;
                }

                // Specify the area of the bitmap that changed.
                bitmap.AddDirtyRect(new Int32Rect(column, row, 1, 1));
            }
            finally
            {
                // Release the back buffer and make it available for display.
                bitmap.Unlock();
            }
        }
    }
}
