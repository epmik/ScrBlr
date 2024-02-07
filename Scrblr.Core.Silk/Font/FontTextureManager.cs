using FontStashSharp.Interfaces;
using System.Drawing;

namespace Scrblr.Core
{
    internal class FontTextureManager : ITexture2DManager
    {
        public FontTextureManager()
        {
        }

        public object CreateTexture(int width, int height) => new FontTexture(width, height);

        public Point GetTextureSize(object texture)
        {
            var t = (FontTexture)texture;
            return new Point(t.Width, t.Height);
        }

        public void SetTextureData(object texture, Rectangle bounds, byte[] data)
        {
            var t = (FontTexture)texture;
            t.SetData(bounds, data);
        }
    }
}
