using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Scrblr.Core
{
    //[Flags]
    //public enum FrameBufferAttachments
    //{
    //    None = 0,
    //    Color = 1,
    //    Depth = 2,
    //    Stencil = 4,
    //}

    public class FrameBuffer : GraphicsSettings, IDisposable
    {
        #region Fields and Properties

        public int Handle;

        private int _colorAttachmentHandle;
        private int _depthAttachmentHandle;
        private int _stencilAttachmentHandle;

        //public FrameBufferAttachments Attachments { get; private set; }

        private FramebufferTarget _framebufferTarget = FramebufferTarget.Framebuffer;

        #endregion Fields and Properties

        #region Constructors

        public FrameBuffer(
            int width, 
            int height,
            int? colorBits = GraphicsContext.DefaultColorBits,
            int? depthBits = GraphicsContext.DefaultDepthBits,
            int? stencilBits = GraphicsContext.DefaultStencilBits,
            int? samples = GraphicsContext.DefaultSamples)
            : base(width, height, colorBits, depthBits, stencilBits, samples)
        {
            Handle = GL.GenFramebuffer();

            GL.BindFramebuffer(_framebufferTarget, Handle);

            if (ColorBits != null && ColorBits > 0)
            {
                _colorAttachmentHandle = GL.GenTexture();

                GL.BindTexture(OpenGlTextureTarget(), _colorAttachmentHandle);

                if (Samples == null || Samples <= 1)
                {
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
                }
                else
                {
                    GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, Samples.Value, PixelInternalFormat.Rgba8, Width, Height, false);
                }

                GL.TexParameter(OpenGlTextureTarget(), TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(OpenGlTextureTarget(), TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);

                GL.TexParameter(OpenGlTextureTarget(), TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(OpenGlTextureTarget(), TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

                GL.BindTexture(OpenGlTextureTarget(), 0);

                GL.FramebufferTexture2D(_framebufferTarget, FramebufferAttachment.ColorAttachment0, OpenGlTextureTarget(), _colorAttachmentHandle, 0);
            }

            if (DepthBits != null && DepthBits > 0)
            {
                _depthAttachmentHandle = GL.GenRenderbuffer();

                GL.BindRenderbuffer(RenderbufferTarget.RenderbufferExt, _depthAttachmentHandle);

                if (Samples == null || Samples <= 1)
                {
                    GL.RenderbufferStorage(RenderbufferTarget.RenderbufferExt, RenderbufferStorageDepthComponent(), Width, Height);
                }
                else
                {
                    GL.RenderbufferStorageMultisample(RenderbufferTarget.RenderbufferExt, Samples.Value, RenderbufferStorageDepthComponent(), Width, Height);
                }
            }

            if (StencilBits != null && StencilBits > 0)
            {
                _stencilAttachmentHandle = GL.GenRenderbuffer();

                GL.BindRenderbuffer(RenderbufferTarget.RenderbufferExt, _stencilAttachmentHandle);

                if (Samples == null || Samples <= 1)
                {
                    GL.RenderbufferStorage(RenderbufferTarget.RenderbufferExt, RenderbufferStorageStencil(), Width, Height);
                }
                else
                {
                    GL.RenderbufferStorageMultisample(RenderbufferTarget.RenderbufferExt, Samples.Value, RenderbufferStorageStencil(), Width, Height);
                }
            }

            GL.BindFramebuffer(_framebufferTarget, 0);
        }

        #endregion Constructors

        private RenderbufferStorage RenderbufferStorageDepthComponent()
        {
            switch(DepthBits)
            {
                case 16:
                    return RenderbufferStorage.DepthComponent16;
                case 24:
                    return RenderbufferStorage.DepthComponent24;
                case 32:
                    return RenderbufferStorage.DepthComponent32;
                default:
                    throw new NotImplementedException($"RenderbufferStorageDepthComponent() failed. found unknown DepthBits: {DepthBits}");
            }
        }

        private RenderbufferStorage RenderbufferStorageStencil()
        {
            switch (StencilBits)
            {
                case 24:
                    return RenderbufferStorage.Depth24Stencil8;
                case 32:
                    return RenderbufferStorage.Depth32fStencil8;
                default:
                    throw new NotImplementedException($"RenderbufferStorageStencil() failed. found unknown StencilBits: {StencilBits}");
            }
        }

        private TextureTarget OpenGlTextureTarget()
        {
            return Samples < 2 ? TextureTarget.Texture2D : TextureTarget.Texture2DMultisample;
        }

        public FramebufferErrorCode Status()
        {
            GL.BindFramebuffer(_framebufferTarget, Handle);

            var status = GL.CheckFramebufferStatus(_framebufferTarget);

            GL.BindFramebuffer(_framebufferTarget, 0);
            
            return status;
        }


        //public void Clear()
        //{
        //    GL.BindFramebuffer(_framebufferTarget, 0);

        //    //GL.InvalidateFramebuffer(FramebufferTarget.Framebuffer,);
        //}

        public void Bind()
        {
            GL.BindFramebuffer(_framebufferTarget, Handle);

            GL.Viewport(0, 0, Width, Height);
        }

        public void UnBind()
        {
            GL.BindFramebuffer(_framebufferTarget, 0);
        }

        public void Dispose()
        {
            UnBind();

            if (_colorAttachmentHandle != 0)
            {
                GL.DeleteTexture(_colorAttachmentHandle);
            }

            if (_depthAttachmentHandle != 0)
            {
                GL.DeleteRenderbuffer(_depthAttachmentHandle);
            }

            if (_stencilAttachmentHandle != 0)
            {
                GL.DeleteRenderbuffer(_stencilAttachmentHandle);
            }

            GL.DeleteFramebuffer(Handle);
        }
    }
}