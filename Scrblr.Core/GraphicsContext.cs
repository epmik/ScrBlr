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
    public class GraphicsContext : GraphicsSettings, IDisposable
    {
        private static int GraphicsContextCount { get; set; }

        /// <summary>
        /// the OpenGl Handle to the intenal framebuffer if any was created, or 0 if this is the default framebuffer
        /// default == 0 or the default framebuffer
        /// </summary>
        public int Handle 
        { 
            get 
            { 
                return _frameBuffer == null ? 0 : _frameBuffer.Handle; 
            } 
        }

        public bool IsDefault
        {
            get { return _frameBuffer == null; }
        }

        private FrameBuffer _frameBuffer;

        public GraphicsContext(
            int width, 
            int height, 
            int? colorBits = GraphicsContext.DefaultColorBits,
            int? depthBits = GraphicsContext.DefaultDepthBits,
            int? stencilBits = GraphicsContext.DefaultStencilBits,
            int? samples = GraphicsContext.DefaultSamples)
            : base(width, height, colorBits, depthBits, stencilBits, samples)
        {
            GraphicsContextCount++;

            if(GraphicsContextCount > 1)
            {
                _frameBuffer = new FrameBuffer(Width, Height, ColorBits, DepthBits, StencilBits, Samples);
            }
        }

        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);

            GL.Viewport(0, 0, Width, Height);
        }

        public void Dispose()
        {
            if(_frameBuffer != null)
            {
                _frameBuffer.Dispose();
                _frameBuffer = null;
            }
        }
    }
}
