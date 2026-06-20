using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml.Linq;

namespace Scrblr.Rtx
{
    internal static class Ppm
    {
        public static void Save(string path, int width, int height, float[] buffer)
        {
            using (StreamWriter writer = new StreamWriter(path, false, Encoding.ASCII))
            {
                writer.WriteLine("P3");
                writer.WriteLine("# ");
                writer.WriteLine($"{width} {height}");
                writer.WriteLine("255");

                for (int j = 0; j < height; j++)
                {
                    for (int i = 0; i < width; i++)
                    {

                        Write(new Vector3d((double)(i) / (width - 1), (double)(j) / (height - 1), 0), writer);

                        //var r = (double)(i) / (width - 1);
                        //var g = (double)(j) / (height - 1);
                        //var b = 0.0;

                        //int ir = (int)(255.999 * r);
                        //int ig = (int)(255.999 * g);
                        //int ib = (int)(255.999 * b);

                        //var s = $"{ir} {ig} {ib}\n";

                        //writer.Write($"{ir} {ig} {ib}\n");
                    }
                }
            }
        }

        private static void Write(Vector3d color , StreamWriter writer)
        {
            var ir = (int)(255.999 * color.X);
            var ig = (int)(255.999 * color.Y);
            var ib = (int)(255.999 * color.Z);

            writer.Write($"{ir} {ig} {ib}\n");
        }
    }

    internal class PpmWriter : IDisposable
    {
        StreamWriter? _writer;
        string _path;
        int _width;
        int _height;

        // Linear space handles light with physically accurate math (matching how light behaves in the real world), while Gamma space
        // compresses brightness values to match how the human eye perceives light and how traditional monitors display it.
        // Physics
        // Linear Space: Accurately models how light blends, falls off, and bounces in the real world.
        // Gamma (sRGB) Space: Physically inaccurate; light addition or blending results in unnaturally bright or washed-out images.
        // Human Perception
        // Linear Space: Treats all mathematical values equally, which conflicts with our eyes (we are much more sensitive to dark tones than bright ones).
        // Gamma (sRGB) Space: Compresses digital brightness values (using a power curve, usually ~2.2) to match human vision perfectly.
        // Use Case
        // Linear Space: Ideal for 3D rendering, shading calculations, and compositing (where math must equal real-world light).
        // Gamma (sRGB) Space: Ideal for storing, displaying, and transmitting images on screens (saves file size and prevents banding).
        public bool OutputLinearColorSpace = false;

        public PpmWriter(string path, int width, int height)
        {
            _path = path;
            _width = width;
            _height = height;
        }

        public void Dispose()
        {
            Close();
        }

        public void Open()
        {
            _writer = new StreamWriter(_path, false, Encoding.ASCII);

            _writer.WriteLine("P3");
            _writer.WriteLine("# ");
            _writer.WriteLine($"{_width} {_height}");
            _writer.WriteLine("255");
        }

        static readonly Interval Intensity = new Interval(0.000, 0.999);

        public void Write(Vector3d color)
        {
            if(_writer == null)
            {
                Open();
            }

            var r = color.X;
            var g = color.Y;
            var b = color.Z;

            if (!OutputLinearColorSpace)
            {
                // Apply a linear to gamma transform for gamma 2
                r = linear_to_gamma(r);
                g = linear_to_gamma(g);
                b = linear_to_gamma(b);
            }

            var rbyte = (byte)(255.999 * Intensity.Clamp(r));
            var gbyte = (byte)(255.999 * Intensity.Clamp(g));
            var bbyte = (byte)(255.999 * Intensity.Clamp(b));

            Write(rbyte, gbyte, bbyte);
        }

        public void Write(byte r, byte g, byte b)
        {
            if (_writer == null)
            {
                Open();
            }
            _writer.Write($"{r} {g} {b}\n");
        }

        private double linear_to_gamma(double linear_component)
        {
            if (linear_component > 0)
                return Math.Sqrt(linear_component);

            return 0;
        }

        public void Close()
        {
            _writer?.Dispose();
        }
    }
}
