using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorProfiles;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml.Linq;

namespace Scrblr.Rtx
{
    internal static class Png
    {
        public static void Save(string path, int width, int height, Vector3d[] rgb)
        {
            var bufferIndex = 0;
            var rgbIndex = 0;

            var buffer = new byte[width * height * 3];

            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    buffer[bufferIndex++] = (byte)(255 * Math.Clamp(rgb[rgbIndex].X, 0.0, 1.0));
                    buffer[bufferIndex++] = (byte)(255 * Math.Clamp(rgb[rgbIndex].Y, 0.0, 1.0));
                    buffer[bufferIndex++] = (byte)(255 * Math.Clamp(rgb[rgbIndex].Z, 0.0, 1.0));
                    rgbIndex++;
                }
            }

            Save(path, width, height, buffer);
        }

        public static void Save(string path, int width, int height, byte[] rgb)
        {
            using (var image = SixLabors.ImageSharp.Image.LoadPixelData<Rgb24>(rgb, width, height))
            {
                image.SaveAsPng(path);
            }
        }
    }
}
