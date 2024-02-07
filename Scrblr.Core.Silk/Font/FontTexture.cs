using Silk.NET.OpenGL;
using System;
using System.Drawing;

namespace Scrblr.Core
{
    public unsafe class FontTexture : IDisposable
    {
        private readonly uint _handle;

        public readonly int Width;
        public readonly int Height;

        public FontTexture(int width, int height)
        {
            Width = width;
            Height = height;

            _handle = Context.GL.GenTexture();
            GLUtility.CheckError();
            Bind();

            //Reserve enough memory from the gpu for the whole image
            Context.GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
            GLUtility.CheckError();

            SetParameters();
        }

        private void SetParameters()
        {
            //Setting some texture perameters so the texture behaves as expected.
            Context.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
            GLUtility.CheckError();

            Context.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
            GLUtility.CheckError();

            Context.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
            GLUtility.CheckError();

            Context.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
            GLUtility.CheckError();

            //Generating mipmaps.
            Context.GL.GenerateMipmap(TextureTarget.Texture2D);
            GLUtility.CheckError();
        }

        public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
        {
            //When we bind a texture we can choose which textureslot we can bind it to.
            Context.GL.ActiveTexture(textureSlot);
            GLUtility.CheckError();

            Context.GL.BindTexture(TextureTarget.Texture2D, _handle);
            GLUtility.CheckError();
        }

        public void Dispose()
        {
            //In order to dispose we need to delete the opengl handle for the texure.
            Context.GL.DeleteTexture(_handle);
            GLUtility.CheckError();
        }

        public void SetData(Rectangle bounds, byte[] data)
        {
            Bind();
            fixed (byte* ptr = data)
            {
                Context.GL.TexSubImage2D(
                    target: TextureTarget.Texture2D,
                    level: 0,
                    xoffset: bounds.Left,
                    yoffset: bounds.Top,
                    width: (uint)bounds.Width,
                    height: (uint)bounds.Height,
                    format: PixelFormat.Rgba,
                    type: PixelType.UnsignedByte,
                    pixels: ptr
                );
                GLUtility.CheckError();
            }
        }
    }
}
