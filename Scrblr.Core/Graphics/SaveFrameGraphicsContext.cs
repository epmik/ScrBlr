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
    public class SaveFrameGraphicsContext : GraphicsContext
    {
        private GraphicsContext _offscreenGraphicsContext;

        //public GraphicsContext Offscreen => _offscreenGraphicsContext;

        /// <summary>
        /// A scale applied to the saved image. default == 8f
        /// <para>
        /// i.e. if the window is 600 x 800 pixels
        /// a scale will of 1 will result in a 600 x 800 pixels image
        /// a scale will of 2 will result in a 1200 x 1600 pixels image
        /// a scale will of 0.25 will result in a 150 x 200 pixels image
        /// </para>
        /// <para>
        /// Irrespective of the scale, the dimensions of the image can never be less then 1 and never be greater then the texture size limit defined by OpenGl and your graphics device.
        /// And will also be limited by the <see cref="SaveFrameMaxWidth"/> and <see cref="SaveFrameMaxHeight"/> properties.
        /// </para>
        /// </summary>
        public float SaveFrameScale = 8f;

        /// <summary>
        /// default == 8192
        /// </summary>
        public int SaveFrameMaxWidth = 1024 * 8;

        /// <summary>
        /// default == 8192
        /// </summary>
        public int SaveFrameMaxHeight = 1024 * 8;

        /// <summary>
        /// 
        /// </summary>
        public Color4 SaveFrameClearColor = new Color4(1f, 1f, 1f, 0f);

        #region Constructors

        public SaveFrameGraphicsContext(
            int width,
            int height,
            int colorBits = GraphicsSettings.DefaultColorBits,
            int depthBits = GraphicsSettings.DefaultDepthBits,
            int stencilBits = GraphicsSettings.DefaultStencilBits,
            int samples = GraphicsSettings.DefaultSamples)
            : base(width, height, colorBits, depthBits, stencilBits, samples)
        {
        }

        public SaveFrameGraphicsContext(GraphicsSettings graphicsSettings)
            : this(
                  graphicsSettings.Width,
                  graphicsSettings.Height,
                  graphicsSettings.ColorBits,
                  graphicsSettings.DepthBits,
                  graphicsSettings.StencilBits,
                  graphicsSettings.Samples)
        {
        }

        #endregion Constructors

        public void CreateOffscreenContext()
        {
            if(_offscreenGraphicsContext != null )
            {
                throw new InvalidOperationException("_offscreenGraphicsContext is not null");
            }

            var width = (int)(Width * SaveFrameScale);
            var height = (int)(Height * SaveFrameScale);

            var max = GL.GetInteger(GetPName.MaxTextureSize);
            var maxwidth = Math.Min(max, SaveFrameMaxWidth);
            var maxheight = Math.Min(max, SaveFrameMaxHeight);

            if (width > maxwidth)
            {
                var factor = (float)maxwidth / (float)width;
                width = (int)(width * factor);
                height = (int)(height * factor);
            }

            if (height > maxheight)
            {
                var factor = (float)maxheight / (float)height;
                width = (int)(width * factor);
                height = (int)(height * factor);
            }

            _offscreenGraphicsContext = new GraphicsContext(
                width,
                height,
                ColorBits,
                DepthBits,
                StencilBits,
                1);

            _offscreenGraphicsContext.ClearColor(SaveFrameClearColor);

            _offscreenGraphicsContext.ActiveCamera(ActiveCamera());
            _offscreenGraphicsContext.ModelMatrix(ModelMatrix());

            //if(Graphics.Samples > 1)
            //{
            //    // to save multi sampled images, we need 2 frame buffers
            //    // one multi sampled frame buffer to render the scene to
            //    // another normal frame buffer to blit the multi sampled frame buffer onto
            //    _saveMultiSampleFrameGraphicsContext = new GraphicsContext(
            //        width,
            //        height,
            //        Graphics.ColorBits,
            //        Graphics.DepthBits,
            //        Graphics.StencilBits,
            //        Graphics.Samples);

            //    _saveMultiSampleFrameGraphicsContext.ActiveCamera(Graphics.ActiveCamera());
            //    _saveMultiSampleFrameGraphicsContext.ModelMatrix(Graphics.ModelMatrix());
            //}
        }

        /// <summary>
        /// <para>
        /// Binds the offscreen frame buffer
        /// </para>
        /// <para>
        /// Sets the viewport and clear color
        /// </para>
        /// </summary>
        /// <param name="bindFlag">default == BindFlag.Default</param>
        public void BindOffscreenContext(BindFlag bindFlag = BindFlag.Default)
        {
            _offscreenGraphicsContext.Bind(bindFlag);
        }

        /// <summary>
        /// Binds and flushes the current frame buffer and saves it's color buffer to disk as a .png file
        /// </summary>
        /// <param name="pathAndName">can be null</param>
        public void SaveOffscreenContextFrame(string pathAndName = null)
        {
            _offscreenGraphicsContext.SaveFrame(pathAndName);
        }

        public void DisposeOffscreenContext()
        {
            _offscreenGraphicsContext.Dispose();
            _offscreenGraphicsContext = null;
        }

        /// <summary>
        /// <para>
        /// Binds the frame buffer (0 for the default frame buffer)
        /// </para>
        /// <para>
        /// Sets the viewport and clear color
        /// </para>
        /// </summary>
        /// <param name="bindFlag">default == BindFlag.Default</param>
        public override void Bind(BindFlag bindFlag = BindFlag.Default)
        {
            if(_offscreenGraphicsContext != null) 
            {
                _offscreenGraphicsContext.Bind(bindFlag);
            }
            else
            {
                base.Bind(bindFlag);
            }
        }

        #region Dispose

        public override void Dispose()
        {
            _offscreenGraphicsContext?.Dispose();
            _offscreenGraphicsContext = null;

            base.Dispose();
        }

        #endregion Dispose
    }
}
