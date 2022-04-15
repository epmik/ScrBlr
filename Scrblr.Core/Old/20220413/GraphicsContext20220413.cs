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
    public class GraphicsContext20220413 : GraphicsSettings20220413, IDisposable
    {
        private static int GraphicsContextCount { get; set; }
        public static GraphicsContext20220413 Default { get; set; }

        /// <summary>
        /// default == Color4.White
        /// </summary>
        protected Color4 _clearColor = Color4.White;

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

        private FrameBuffer20220413 _frameBuffer;

        public GraphicsContext20220413(
            int width, 
            int height, 
            int colorBits = GraphicsContext20220413.DefaultColorBits,
            int depthBits = GraphicsContext20220413.DefaultDepthBits,
            int stencilBits = GraphicsContext20220413.DefaultStencilBits,
            int samples = GraphicsContext20220413.DefaultSamples)
            : base(width, height, colorBits, depthBits, stencilBits, samples)
        {
            GraphicsContextCount++;

            if(GraphicsContextCount == 1)
            {
                // default OpenGl context
                GraphicsContext20220413.Default = this;
            }
            else
            {
                // custom OpenGl context, create a framebuffer to render to
                _frameBuffer = new FrameBuffer20220413(Width, Height, ColorBits, DepthBits, StencilBits, Samples);
            }
        }

        public GraphicsContext20220413(GraphicsSettings20220413 graphicsSettings)
            : this(
                  graphicsSettings.Width,
                  graphicsSettings.Height,
                  graphicsSettings.ColorBits,
                  graphicsSettings.DepthBits,
                  graphicsSettings.StencilBits,
                  graphicsSettings.Samples)
        {
        }

        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);

            GL.Viewport(0, 0, Width, Height);
        }

        public void Enable(EnableFlag enableFlag)
        {
            GL.Enable((EnableCap)enableFlag);
        }

        public void Disable(EnableFlag enableFlag)
        {
            GL.Disable((EnableCap)enableFlag);
        }

        public void ClearBuffers()
        {
            var clearFlag = ClearFlag.None;

            if (ColorBits != 0)
            {
                clearFlag = clearFlag.AddFlag(ClearFlag.ColorBuffer);
            }

            if (DepthBits != 0)
            {
                clearFlag = clearFlag.AddFlag(ClearFlag.DepthBuffer);
            }

            if (StencilBits != 0)
            {
                clearFlag = clearFlag.AddFlag(ClearFlag.StencilBuffer);
            }

            ClearBuffers(clearFlag);
        }

        public void ClearBuffers(ClearFlag clearFlag)
        {
            GL.Clear((ClearBufferMask)clearFlag);
        }

        public void ClearColor(float r, float g, float b, float a = 1f)
        {
            _clearColor.R = r;
            _clearColor.G = g;
            _clearColor.B = b;
            _clearColor.A = a;

            GL.ClearColor(_clearColor);
        }

        public void ClearColor(int r, int g, int b, int a = 255)
        {
            ClearColor(Utility.ToUnitSingle(r), Utility.ToUnitSingle(g), Utility.ToUnitSingle(b), Utility.ToUnitSingle(a));
        }

        public virtual void Dispose()
        {
            if(_frameBuffer != null)
            {
                _frameBuffer.Dispose();
                _frameBuffer = null;
            }
        }
    }
}
