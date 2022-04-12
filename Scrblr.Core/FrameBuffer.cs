using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Scrblr.Core
{
    [Flags]
    public enum FrameBufferAttachments
    {
        None = 0,
        Color = 1,
        Depth = 2,
        //Stencil = 4,
    }

    public class FrameBuffer : IDisposable
    {
        #region Fields and Properties

        public int Handle;

        private int _colorAttachmentHandle;
        private int _depthAttachmentHandle;
        private int _stencilAttachmentHandle;

        public FrameBufferAttachments Attachments { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public int Samples { get; private set; }


        private FramebufferTarget _framebufferTarget = FramebufferTarget.Framebuffer;

        //public int TotalBytes { get; private set; }

        //public int UsedBytes { get; private set; }

        //public VertexBufferUsage VertexBufferUsage { get; private set; }

        //public VertexBufferLayout Layout { get; private set; }

        #endregion Fields and Properties

        #region Constructors

        public FrameBuffer(
            FrameBufferAttachments attachments,
            int width, int height, int samples)
        {
            Attachments = attachments;
            Width = width;
            Height = height;

            // todo fix multisampling for framebuffers
            // see https://stackoverflow.com/a/42882506/527843
            //Samples = 1; 
            Samples = samples;

            Handle = GL.GenFramebuffer();

            GL.BindFramebuffer(_framebufferTarget, Handle);

            if (Attachments.HasFlag(FrameBufferAttachments.Color))
            {
                _colorAttachmentHandle = GL.GenTexture();

                GL.BindTexture(OpenGlTextureTarget(), _colorAttachmentHandle);

                if (Samples == 1)
                {
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
                }
                else
                {
                    GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, Samples, PixelInternalFormat.Rgba8, Width, Height, false);
                }

                GL.TexParameter(OpenGlTextureTarget(), TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(OpenGlTextureTarget(), TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);

                GL.TexParameter(OpenGlTextureTarget(), TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(OpenGlTextureTarget(), TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

                GL.BindTexture(OpenGlTextureTarget(), 0);

                GL.FramebufferTexture2D(_framebufferTarget, FramebufferAttachment.ColorAttachment0, OpenGlTextureTarget(), _colorAttachmentHandle, 0);
                // or
                //GL.FramebufferTexture(_framebufferTarget, FramebufferAttachment.ColorAttachment0, _colorAttachmentHandle, 0);
            }

            if (Attachments.HasFlag(FrameBufferAttachments.Depth))
            {
                // todo fix this for multisampled depth buffer?
                // does it need fixing?

                _depthAttachmentHandle = GL.GenRenderbuffer();

                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthAttachmentHandle);

                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, Width, Height);

                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

                GL.FramebufferRenderbuffer(_framebufferTarget, FramebufferAttachment.StencilAttachment, RenderbufferTarget.Renderbuffer, _depthAttachmentHandle);
            }

            GL.BindFramebuffer(_framebufferTarget, 0);
        }

        #endregion Constructors

        private TextureTarget OpenGlTextureTarget()
        {
            return Samples < 2 ? TextureTarget.Texture2D : TextureTarget.Texture2DMultisample;
        }

        public bool CheckStatus()
        {
            GL.BindFramebuffer(_framebufferTarget, Handle);

            var status = GL.CheckFramebufferStatus(_framebufferTarget) == FramebufferErrorCode.FramebufferComplete;

            GL.BindFramebuffer(_framebufferTarget, 0);
            
            return status;
        }


        public void Clear()
        {
            GL.BindFramebuffer(_framebufferTarget, 0);

            //GL.InvalidateFramebuffer(FramebufferTarget.Framebuffer,);
        }

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
            GL.DeleteFramebuffer(Handle);
        }
    }
}