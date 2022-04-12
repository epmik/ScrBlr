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
    public class GraphicsContext2d : IDisposable
    {
        #region Shader Sources

        const string NdcVertexShaderSource = @"
#version 330 core

in vec3 iPosition; 

void main(void)
{
    gl_Position = vec4(iPosition, 1.0);
}";

        const string NdcFragmentShaderSource = @"
#version 330 core

in vec4 ioColor;

out vec4 oColor;

void main()
{
    oColor = ioColor;
}";

        #endregion Shader Sources

        private int _vertexBufferElementCount = 32768;
        private VertexBuffer _vertexBuffer { get; set; }

        /// <summary>
        /// default == 256
        /// </summary>
        private int _maxRenderChunks = 256;

        private int _renderChunkCount;

        private RenderChunk[] _renderChunks;


        /// <summary>
        /// default == 4096
        /// </summary>
        private int _maxGeometryCount = 4096;

        private int _geometryCount;

        private Geometry[] _geometry;



        private Shader _currentShader { get; set; }

        public Shader CurrentShader { get { return _currentShader; } set { _currentShader = value; } }


        private ICamera _currentCamera { get; set; }

        public ICamera CurrentCamera{ get { return _currentCamera; } set { _currentCamera = value; } }


        public GraphicsContext2d()
        {
            CurrentModelMatrix = Matrix4.Identity;
        }

        #region Load

        public void Load()
        {
            _renderChunks = new RenderChunk[_maxRenderChunks];

            for (var i = 0; i < _maxRenderChunks; i++)
            {
                _renderChunks[i] = new RenderChunk();
            }

            _geometry = new Geometry[_maxGeometryCount];

            _vertexBuffer = new VertexBuffer(
                _vertexBufferElementCount,
                new[] {
                    new VertexBufferLayout.Part { Identifier = VertexBufferLayout.PartIdentifier.Position1, Type = VertexBufferLayout.ElementType.Single, Count = 3 },
                    new VertexBufferLayout.Part { Identifier = VertexBufferLayout.PartIdentifier.Color1, Type = VertexBufferLayout.ElementType.Single, Count = 4 },
                },
                VertexBufferUsage.DynamicDraw);
        }

        #endregion Load

        #region Clear

        public void Clear()
        {
            _vertexBuffer.Clear();

            _renderChunkCount = 0;

            for (var i = 0; i < _geometryCount; i++)
            {
                _geometry[i].Dispose();
                _geometry[i] = null;
            }

            _geometryCount = 0;
        }

        #endregion Clear

        #region Flush

        public void Flush()
        {
            if(_renderChunkCount < 1)
            {
                return;
            }

            FlushChunks();

            Clear();
        }

        private void FlushChunks()
        {
            for (var c = 0; c < _renderChunkCount; c++)
            {
                var renderChunk = _renderChunks[c];

                renderChunk.Shader.Use();

                renderChunk.Shader.Uniform("uViewMatrix", renderChunk.ViewMatrix);
                renderChunk.Shader.Uniform("uProjectionMatrix", renderChunk.ProjectionMatrix);
                renderChunk.Shader.Uniform("uModelMatrix", renderChunk.ModelMatrix);

                renderChunk.VertexBuffer.Bind();
                renderChunk.VertexBuffer.EnableElements();

                GL.DrawArrays((PrimitiveType)renderChunk.GeometryType, renderChunk.ElementIndex, renderChunk.ElementCount);
            }
        }

        #endregion Flush

        #region Matrix Stack Stuff

        private const int _modelMatrixStackSize = 128;
        private int _currentModelMatrixStackIndex = 0;

        private Matrix4[] _modelMatrixStack = new Matrix4[_modelMatrixStackSize];

        public Matrix4 CurrentModelMatrix
        {
            get
            {
                return _modelMatrixStack[_currentModelMatrixStackIndex];
            }
            set
            {
                _modelMatrixStack[_currentModelMatrixStackIndex] = value;
            }
        }

        public void ClearMatrixStack()
        {
            _currentModelMatrixStackIndex = 0;
            _modelMatrixStack[_currentModelMatrixStackIndex] = Matrix4.Identity;
        }

        public void PushMatrix()
        {
            if (_currentModelMatrixStackIndex + 1 >= _modelMatrixStackSize)
            {
                throw new InvalidOperationException($"PushMatrix() failed. _currentModelMatrixStackIndex has been reached: {_currentModelMatrixStackIndex}");
            }

            _modelMatrixStack[_currentModelMatrixStackIndex + 1] = _modelMatrixStack[_currentModelMatrixStackIndex];
            _currentModelMatrixStackIndex++;
        }

        public void PopMatrix()
        {
            if (_currentModelMatrixStackIndex <= 0)
            {
                throw new InvalidOperationException($"PopMatrix() failed. _currentModelMatrixStackIndex is {_currentModelMatrixStackIndex}. There are more PopMatrix() calls then PushMatrix() calls.");
            }

            _currentModelMatrixStackIndex--;
        }

        #endregion Matrix Stack Stuff

        #region Dispose

        public void Dispose()
        {
            _vertexBuffer.Dispose();

            GC.SuppressFinalize(this);
        }

        #endregion Dispose

        protected void InsertRenderChunk(GeometryType geometryType, int elementCount)
        {
            if (!_vertexBuffer.CanWriteElements(elementCount) || _renderChunkCount + 1 >= _maxRenderChunks)
            {
                Flush();

                //if (!_vertexBuffer.CanWriteElements(elementCount))
                //{
                //    throw new 
                //}
            }

            _renderChunks[_renderChunkCount].Shader = CurrentShader;
            _renderChunks[_renderChunkCount].VertexBuffer = _vertexBuffer;
            _renderChunks[_renderChunkCount].ViewMatrix = CurrentCamera.ViewMatrix();
            _renderChunks[_renderChunkCount].ProjectionMatrix = CurrentCamera.ProjectionMatrix();
            _renderChunks[_renderChunkCount].ModelMatrix = CurrentModelMatrix;
            _renderChunks[_renderChunkCount].GeometryType = geometryType;
            _renderChunks[_renderChunkCount].ElementIndex = _vertexBuffer.UsedElements();
            _renderChunks[_renderChunkCount].ElementCount = elementCount;

            _renderChunkCount++;
        }

        #region Geometry Functions

        public Geometry CreateGeometry(GeometryType shapeKind)
        {
            var g = new Geometry(shapeKind, CurrentShader, CurrentModelMatrix);

            AddGeometry(g);

            return g;
        }

        public void AddGeometry(Geometry geometry)
        {
            if (_geometryCount + 1 >= _maxGeometryCount)
            {
                throw new InvalidOperationException($"AddGeometry(Geometry geometry) failed. _maxGeometryCount had been reached: {_geometryCount}");
            }

            _geometry[_geometryCount++] = geometry;
        }

        #endregion Geometry Functions

        #region Shape Functions

        #region Rectangle

        public void Rectangle(float x, float y, float width, float height)
        {
            InsertRenderChunk(GeometryType.TriangleStrip, 4);

            _vertexBuffer.Bind();

            var halfWidth = width * 0.5f;
            var halfHeight = height * 0.5f;

            x -= halfWidth;
            y -= halfHeight;

            _vertexBuffer.Write(new[] { x + width, y + height, 0f });           // top right
            _vertexBuffer.Write(_currentFillColor);

            _vertexBuffer.Write(new[] { x, y + height, 0f });                   // top left
            _vertexBuffer.Write(_currentFillColor);

            _vertexBuffer.Write(new[] { x + width, y, 0f });                    // bottom right
            _vertexBuffer.Write(_currentFillColor);

            _vertexBuffer.Write(new[] { x, y, 0f });                            // bottom left
            _vertexBuffer.Write(_currentFillColor);
        }

        #endregion Rectangle

        #region Quad

        public void Quad(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            InsertRenderChunk(GeometryType.TriangleStrip, 4);

            _vertexBuffer.Bind();

            _vertexBuffer.Write(new[] { x1, y1, 0f });
            _vertexBuffer.Write(_currentFillColor);

            _vertexBuffer.Write(new[] { x2, y2, 0f });
            _vertexBuffer.Write(_currentFillColor);

            _vertexBuffer.Write(new[] { x3, y3, 0f });
            _vertexBuffer.Write(_currentFillColor);

            _vertexBuffer.Write(new[] { x4, y4, 0f });
            _vertexBuffer.Write(_currentFillColor);
        }

        #endregion Quad

        #region Line Functions

        public void Lines(params float[] coordinates)
        {
            InsertRenderChunk(GeometryType.Lines, 4);

            _vertexBuffer.Bind();

            for (var i = 0; i < coordinates.Length;)
            {
                _vertexBuffer.Write(new[] { coordinates[i++], coordinates[i++], 0f });
                _vertexBuffer.Write(_currentFillColor);
            }
        }

        public void LineLoop(params float[] coordinates)
        {
            InsertRenderChunk(GeometryType.LineLoop, 4);

            _vertexBuffer.Bind();

            for (var i = 0; i < coordinates.Length;)
            {
                _vertexBuffer.Write(new[] { coordinates[i++], coordinates[i++], 0f });
                _vertexBuffer.Write(_currentFillColor);
            }
        }

        public void LineStrip(params float[] coordinates)
        {
            InsertRenderChunk(GeometryType.LineStrip, 4);

            _vertexBuffer.Bind();

            for (var i = 0; i < coordinates.Length;)
            {
                _vertexBuffer.Write(new[] { coordinates[i++], coordinates[i++], 0f });
                _vertexBuffer.Write(_currentFillColor);
            }
        }

        #endregion Line Functions

        #endregion Shape Functions

        #region Fill Functions

        /// <summary>
        /// default == Color4.OrangeRed
        /// </summary>
        private Color4 _currentFillColor = Color4.Black;

        public void Fill(int r, int g, int b, int a = 255)
        {
            Fill(Utility.ToUnitSingle(r), Utility.ToUnitSingle(g), Utility.ToUnitSingle(b), Utility.ToUnitSingle(a));
        }

        public void Fill(float r, float g, float b, float a = 1f)
        {
            _currentFillColor.R = r;
            _currentFillColor.G = g;
            _currentFillColor.B = b;
            _currentFillColor.A = a;
        }

        public void Fill(Color4 color)
        {
            Fill(color.R, color.G, color.B, color.A);
        }

        #endregion Fill Functions

        #region Stroke Functions

        ///// <summary>
        ///// default == Color4.Black
        ///// </summary>
        //protected Color4 _strokeColor = Color4.Black;

        //protected void Stroke(int r, int g, int b, int a = 255)
        //{
        //    Stroke(Utility.ToUnitSingle(r), Utility.ToUnitSingle(g), Utility.ToUnitSingle(b), Utility.ToUnitSingle(a));
        //}

        //protected void Stroke(float r, float g, float b, float a = 1f)
        //{
        //    _strokeColor.R = r;
        //    _strokeColor.G = g;
        //    _strokeColor.B = b;
        //    _strokeColor.A = a;
        //}

        #endregion Stroke Functions

    }
}
