using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using FontStashSharp.Interfaces;
using Silk.NET.OpenGL;

namespace Scrblr.Core.Silk.Font
{
    internal class FontRenderer : IFontStashRenderer2, IDisposable
    {
        private const int MAX_SPRITES = 2048;
        private const int MAX_VERTICES = MAX_SPRITES * 4;
        private const int MAX_INDICES = MAX_SPRITES * 6;

        private readonly FontShader _shader;
        private readonly FontBufferObject<VertexPositionColorTexture> _vertexBuffer;
        private readonly FontBufferObject<short> _indexBuffer;
        private readonly FontVertexArrayObject _vao;
        private readonly VertexPositionColorTexture[] _vertexData = new VertexPositionColorTexture[MAX_VERTICES];
        private object _lastTexture;
        private int _vertexIndex = 0;

        private readonly FontTextureManager _textureManager;

        public ITexture2DManager TextureManager => _textureManager;

        private static readonly short[] indexData = GenerateIndexArray();

        public unsafe FontRenderer()
        {
            _textureManager = new FontTextureManager();

            _vertexBuffer = new FontBufferObject<VertexPositionColorTexture>(MAX_VERTICES, BufferTargetARB.ArrayBuffer, true);
            _indexBuffer = new FontBufferObject<short>(indexData.Length, BufferTargetARB.ElementArrayBuffer, false);
            _indexBuffer.SetData(indexData, 0, indexData.Length);

            _shader = new FontShader("fontshader.vert", "fontshader.frag");
            _shader.Use();

            _vao = new FontVertexArrayObject(sizeof(VertexPositionColorTexture));
            _vao.Bind();

            var location = _shader.GetAttribLocation("a_position");
            _vao.VertexAttribPointer(location, 3, VertexAttribPointerType.Float, false, 0);

            location = _shader.GetAttribLocation("a_color");
            _vao.VertexAttribPointer(location, 4, VertexAttribPointerType.UnsignedByte, true, 12);

            location = _shader.GetAttribLocation("a_texCoords0");
            _vao.VertexAttribPointer(location, 2, VertexAttribPointerType.Float, false, 16);
        }

        ~FontRenderer() => Dispose(false);

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            _vao.Dispose();
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            _shader.Dispose();
        }

        public void Begin()
        {
            Context.GL.Disable(EnableCap.DepthTest);
            GLUtility.CheckError();
            Context.GL.Enable(EnableCap.Blend);
            GLUtility.CheckError();
            Context.GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
            GLUtility.CheckError();

            _shader.Use();
            _shader.SetUniform("TextureSampler", 0);

            var transform = Matrix4x4.CreateOrthographicOffCenter(0, 1200, 800, 0, 0, -1);
            _shader.SetUniform("MatrixTransform", transform);

            _vao.Bind();
            _indexBuffer.Bind();
            _vertexBuffer.Bind();
        }

        public void DrawQuad(object texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight, ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight)
        {
            if (_lastTexture != texture)
            {
                FlushBuffer();
            }

            _vertexData[_vertexIndex++] = topLeft;
            _vertexData[_vertexIndex++] = topRight;
            _vertexData[_vertexIndex++] = bottomLeft;
            _vertexData[_vertexIndex++] = bottomRight;

            _lastTexture = texture;
        }

        public void End()
        {
            FlushBuffer();
        }

        private unsafe void FlushBuffer()
        {
            if (_vertexIndex == 0 || _lastTexture == null)
            {
                return;
            }

            _vertexBuffer.SetData(_vertexData, 0, _vertexIndex);

            var texture = (FontTexture)_lastTexture;
            texture.Bind();

            Context.GL.DrawElements(PrimitiveType.Triangles, (uint)(_vertexIndex * 6 / 4), DrawElementsType.UnsignedShort, null);
            _vertexIndex = 0;
        }

        private static short[] GenerateIndexArray()
        {
            short[] result = new short[MAX_INDICES];
            for (int i = 0, j = 0; i < MAX_INDICES; i += 6, j += 4)
            {
                result[i] = (short)j;
                result[i + 1] = (short)(j + 1);
                result[i + 2] = (short)(j + 2);
                result[i + 3] = (short)(j + 3);
                result[i + 4] = (short)(j + 2);
                result[i + 5] = (short)(j + 1);
            }
            return result;
        }
    }
}
