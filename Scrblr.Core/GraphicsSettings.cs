using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Scrblr.Core
{
    public class GraphicsSettings
    {
        public const int DefaultColorBits = 32;
        public const int DefaultDepthBits = 32;
        public const int DefaultStencilBits = 24;
        public const int DefaultSamples = 8;

        /// <summary>
        /// default == 0
        /// </summary
        public int Width { get; private set; }

        /// <summary>
        /// default == 0
        /// </summary>
        public int Height { get; private set; }

        private int? _colorBits = GraphicsContext.DefaultColorBits;

        /// <summary>
        /// default == 32
        /// </summary>
        public int? ColorBits
        {
            get { return _colorBits; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("Samples failed. Samples cannot be less then 0.");
                }
                else if (value > 32)
                {
                    throw new ArgumentOutOfRangeException("Samples failed. Samples cannot be greater then 32.");
                }
                _colorBits = value;
            }
        }

        private int? _depthBits = GraphicsContext.DefaultDepthBits;

        /// <summary>
        /// default == 32
        /// </summary>
        public int? DepthBits
        {
            get { return _depthBits; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("Samples failed. Samples cannot be less then 0.");
                }
                else if (value > 32)
                {
                    throw new ArgumentOutOfRangeException("Samples failed. Samples cannot be greater then 32.");
                }
                _depthBits = value;
            }
        }

        private int? _stencilBits = GraphicsContext.DefaultStencilBits;

        /// <summary>
        /// default == 24
        /// </summary>
        public int? StencilBits
        {
            get { return _stencilBits; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("Samples failed. Samples cannot be less then 0.");
                }
                else if (value > 32)
                {
                    throw new ArgumentOutOfRangeException("Samples failed. Samples cannot be greater then 32.");
                }
                _stencilBits = value;
            }
        }

        private int? _samples = GraphicsContext.DefaultSamples;

        /// <summary>
        /// default == 8
        /// </summary>
        public int? Samples
        {
            get { return _samples; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("Samples failed. Samples cannot be less then 0.");
                }
                _samples = value;
            }
        }

        public GraphicsSettings(
            int width, 
            int height, 
            int? colorBits = GraphicsContext.DefaultColorBits,
            int? depthBits = GraphicsContext.DefaultDepthBits,
            int? stencilBits = GraphicsContext.DefaultStencilBits,
            int? samples = GraphicsContext.DefaultSamples)
        {
            Width = width;
            Height = height;
            ColorBits = colorBits;
            DepthBits = depthBits;
            StencilBits = stencilBits;
            Samples = samples;
        }
    }
}
