using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Scrblr.Core
{
    public enum TextureWrap
    {
        Repeat = TextureWrapMode.Repeat,
        Clamp = TextureWrapMode.ClampToEdge,
    }

    public enum TextureFilter
    {
        Nearest = TextureMagFilter.Nearest,
        Linear = TextureMagFilter.Linear,
    }

    public class Texture : IDisposable
    {
        #region Fields and Properties

        public int Handle;
        
        public int Width { get; private set; }

        public int Height { get; private set; }

        public TextureWrap TextureWrap { get; private set; } = TextureWrap.Repeat;

        public TextureFilter TextureFilter { get; private set; } = TextureFilter.Linear;

        public bool UseMipMaps { get; set; } = true;


        #endregion Fields and Properties

        #region Constructors

        public Texture(string path)
        {
            Handle = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, Handle);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrap);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrap);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureFilter);

            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(path))
            {
                Width = bitmap.Width;
                Height = bitmap.Height;

                var pixelFormat = bitmap.RawFormat == System.Drawing.Imaging.ImageFormat.Png 
                    ? System.Drawing.Imaging.PixelFormat.Format32bppArgb 
                    : System.Drawing.Imaging.PixelFormat.Format32bppRgb;

                var pixelInternalFormat = bitmap.RawFormat == System.Drawing.Imaging.ImageFormat.Png
                    ? PixelInternalFormat.Rgba
                    : PixelInternalFormat.Rgb;

                bitmap.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);

                var bits = bitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    pixelFormat);

                GL.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                    pixelInternalFormat,    // Specifies the number of color components in the texture.
                    Width,
                    Height,
                    0,
                    PixelFormat.Bgra,       // Specifies the format of the pixel data.
                    PixelType.UnsignedByte, // Specifies the data type of the pixel data.
                    bits.Scan0);

                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

                bitmap.UnlockBits(bits);
            }
        }

        #endregion Constructors

        public void Clear()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, Handle);

            GL.InvalidateBufferData(Handle);
        }

        public void Unit(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
        }

        public void UnitAndBind(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }

        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }

        public void UnBind()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Dispose()
        {
            UnBind();
            GL.DeleteTexture(Handle);
        }
    }
}