﻿using OpenTK.Graphics.OpenGL4;
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
    public class GraphicsContext2d20220413 : GraphicsContext20220413
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
        private VertexBuffer20220413 _vertexBuffer { get; set; }

        /// <summary>
        /// default == 256
        /// </summary>
        private int _maxRenderChunks = 256;

        private int _renderChunkCount;

        private RenderChunk20220413[] _renderChunks;


        /// <summary>
        /// default == 4096
        /// </summary>
        private int _maxGeometryCount = 4096;

        private int _geometryCount;

        private Geometry20220413[] _geometry;



        private Shader20220413 _currentShader { get; set; }

        public Shader20220413 CurrentShader { get { return _currentShader; } set { _currentShader = value; } }


        private ICamera _currentCamera { get; set; }

        public ICamera CurrentCamera{ get { return _currentCamera; } set { _currentCamera = value; } }

        public GraphicsContext2d20220413(
            int width,
            int height,
            int colorBits = GraphicsContext20220413.DefaultColorBits,
            int depthBits = GraphicsContext20220413.DefaultDepthBits,
            int stencilBits = GraphicsContext20220413.DefaultStencilBits,
            int samples = GraphicsContext20220413.DefaultSamples)
            : base(width, height, colorBits, depthBits, stencilBits, samples)
        {
            CurrentModelMatrix = Matrix4.Identity;
        }

        public GraphicsContext2d20220413(GraphicsSettings20220413 graphicsSettings)
            : this(
                  graphicsSettings.Width, 
                  graphicsSettings.Height, 
                  graphicsSettings.ColorBits, 
                  graphicsSettings.DepthBits, 
                  graphicsSettings.StencilBits, 
                  graphicsSettings.Samples)
        {

        }

        #region Load

        public void Load()
        {
            _renderChunks = new RenderChunk20220413[_maxRenderChunks];

            for (var i = 0; i < _maxRenderChunks; i++)
            {
                _renderChunks[i] = new RenderChunk20220413();
            }

            _geometry = new Geometry20220413[_maxGeometryCount];

            _vertexBuffer = new VertexBuffer20220413(
                _vertexBufferElementCount,
                new[] {
                    new VertexBufferLayout20220413.Part { Identifier = VertexBufferLayout20220413.PartIdentifier.Position0, Type = VertexBufferLayout20220413.ElementType.Single, Count = 3 },
                    //new VertexBufferLayout.Part { Identifier = VertexBufferLayout.PartIdentifier.Normal0, Type = VertexBufferLayout.ElementType.Single, Count = 3 },
                    new VertexBufferLayout20220413.Part { Identifier = VertexBufferLayout20220413.PartIdentifier.Color0, Type = VertexBufferLayout20220413.ElementType.Single, Count = 4 },
                    //new VertexBufferLayout.Part { Identifier = VertexBufferLayout.PartIdentifier.Uv0, Type = VertexBufferLayout.ElementType.Single, Count = 2 },
                    //new VertexBufferLayout.Part { Identifier = VertexBufferLayout.PartIdentifier.Uv1, Type = VertexBufferLayout.ElementType.Single, Count = 2 },
                },
                VertexBufferUsage.DynamicDraw);

            // disable depth testing by default for 2d
            Disable(EnableFlag.DepthTest);
        }

        #endregion Load

        #region Reset

        public void Reset()
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

        #endregion Reset

        #region Flush

        public void Flush()
        {
            if(_renderChunkCount < 1)
            {
                return;
            }

            FlushChunks();

            Reset();
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

        public override void Dispose()
        {
            if(_vertexBuffer != null)
            {
                _vertexBuffer.Dispose();
                _vertexBuffer = null;
            }

            GC.SuppressFinalize(this);
        }

        #endregion Dispose

        protected void InsertRenderChunk(GeometryType20220413 geometryType, int elementCount)
        {
            if (!_vertexBuffer.CanWriteElements(elementCount) || _renderChunkCount + 1 >= _maxRenderChunks)
            {
                Flush();
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

        public Geometry20220413 CreateGeometry(GeometryType20220413 shapeKind)
        {
            var g = new Geometry20220413(shapeKind, CurrentShader, CurrentModelMatrix);

            AddGeometry(g);

            return g;
        }

        public void AddGeometry(Geometry20220413 geometry)
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
            InsertRenderChunk(GeometryType20220413.TriangleStrip, 4);

            _vertexBuffer.Bind();

            var halfWidth = width * 0.5f;
            var halfHeight = height * 0.5f;

            x -= halfWidth;
            y -= halfHeight;

            _vertexBuffer.Write(x + width, y + height, 0f);           // top right
            _vertexBuffer.Write(_currentFillColor);

            _vertexBuffer.Write(x, y + height, 0f);                   // top left
            _vertexBuffer.Write(_currentFillColor);

            _vertexBuffer.Write(x + width, y, 0f);                    // bottom right
            _vertexBuffer.Write(_currentFillColor);

            _vertexBuffer.Write(x, y, 0f);                            // bottom left
            _vertexBuffer.Write(_currentFillColor);
        }

        #endregion Rectangle

        #region Quad

        public void Quad(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            InsertRenderChunk(GeometryType20220413.TriangleStrip, 4);

            _vertexBuffer.Bind();

            _vertexBuffer.Write(x1, y1, 0f);
            _vertexBuffer.Write(_currentFillColor);

            _vertexBuffer.Write(x2, y2, 0f);
            _vertexBuffer.Write(_currentFillColor);

            _vertexBuffer.Write(x3, y3, 0f);
            _vertexBuffer.Write(_currentFillColor);

            _vertexBuffer.Write(x4, y4, 0f);
            _vertexBuffer.Write(_currentFillColor);
        }

        #endregion Quad

        #region Line Functions

        public void Lines(params float[] coordinates)
        {
            InsertRenderChunk(GeometryType20220413.Lines, 4);

            _vertexBuffer.Bind();

            for (var i = 0; i < coordinates.Length;)
            {
                _vertexBuffer.Write(coordinates[i++], coordinates[i++], 0f);
                _vertexBuffer.Write(_currentFillColor);
            }
        }

        public void LineLoop(params float[] coordinates)
        {
            InsertRenderChunk(GeometryType20220413.LineLoop, 4);

            _vertexBuffer.Bind();

            for (var i = 0; i < coordinates.Length;)
            {
                _vertexBuffer.Write(coordinates[i++], coordinates[i++], 0f);
                _vertexBuffer.Write(_currentFillColor);
            }
        }

        public void LineStrip(params float[] coordinates)
        {
            InsertRenderChunk(GeometryType20220413.LineStrip, 4);

            _vertexBuffer.Bind();

            for (var i = 0; i < coordinates.Length;)
            {
                _vertexBuffer.Write(coordinates[i++], coordinates[i++], 0f);
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

        public void Fill(int grey, int a = 255)
        {
            var g = Utility.ToUnitSingle(grey);

            Fill(g, g, g, Utility.ToUnitSingle(a));
        }

        public void Fill(float grey, float a = 1f)
        {
            Fill(grey, grey, grey, a);
        }

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
