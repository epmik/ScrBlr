﻿using OpenTK.Graphics.OpenGL4;
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
    public class GraphicsSettings20220413
    {
        #region Fields and Properties

        public const int DefaultColorBits = 32;
        public const int DefaultDepthBits = 32;
        public const int DefaultStencilBits = 0;
        public const int DefaultSamples = 8;

        /// <summary>
        /// default == 0
        /// </summary
        public int Width { get; private set; }

        /// <summary>
        /// default == 0
        /// </summary>
        public int Height { get; private set; }

        private int _colorBits = GraphicsContext20220413.DefaultColorBits;

        /// <summary>
        /// default == 32
        /// </summary>
        /// set to 0 to disable depth buffer creation
        /// <para>
        /// possible values are 32
        /// </para>
        public int ColorBits
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

        private int _depthBits = GraphicsContext20220413.DefaultDepthBits;

        /// <summary>
        /// default == 32
        /// set to 0 to disable depth buffer creation
        /// <para>
        /// possible values are 16, 24 or 32
        /// </para>
        /// </summary>
        public int DepthBits
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

        private int _stencilBits = GraphicsContext20220413.DefaultStencilBits;

        /// <summary>
        /// default == 24
        /// set to 0 to disable stencil buffer creation
        /// <para>
        /// possible values are 16 or 24
        /// </para>
        /// </summary>
        public int StencilBits
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

        private int _samples = GraphicsContext20220413.DefaultSamples;

        /// <summary>
        /// default == 8
        /// set to 0 or 1 to disable sampling
        /// </summary>
        public int Samples
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

        #endregion Fields and Properties

        public GraphicsSettings20220413(
            int width, 
            int height, 
            int colorBits = GraphicsContext20220413.DefaultColorBits,
            int depthBits = GraphicsContext20220413.DefaultDepthBits,
            int stencilBits = GraphicsContext20220413.DefaultStencilBits,
            int samples = GraphicsContext20220413.DefaultSamples)
        {
            Width = width;
            Height = height;
            ColorBits = colorBits;
            DepthBits = depthBits;
            StencilBits = stencilBits;
            Samples = samples;
        }

        public GraphicsSettings20220413(GraphicsSettings20220413 graphicsSettings)
            : this(
                  graphicsSettings.Width,
                  graphicsSettings.Height,
                  graphicsSettings.ColorBits,
                  graphicsSettings.DepthBits,
                  graphicsSettings.StencilBits,
                  graphicsSettings.Samples)
        {
        }
    }
}
