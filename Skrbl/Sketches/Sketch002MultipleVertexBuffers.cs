
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using System.Drawing;
using System.Reflection.Metadata;

namespace Skrbl
{
    public class Sketch002MultipleVertexBuffers : GameWindow
    {

        private IRandomGenerator _defaultRandomGenerator;
        public IRandomGenerator Random { get { return _defaultRandomGenerator; } }

        float[] _vertices0 =
        {
            // Position             Texture coordinates
             250f,  250f, 0.0f,     1.0f, 1.0f, // top right
             250f,    0f, 0.0f,     1.0f, 0.0f, // bottom right
               0f,    0f, 0.0f,     0.0f, 0.0f, // bottom left
               0f,  250f, 0.0f,     0.0f, 1.0f  // top left
        };

        uint[] _indices0 =
        {
            0, 1, 3,
            1, 2, 3
        };

        float[] _vertices1 =
        {
            // Position             Color                 Texture coordinates
             250f,    0f, 0.0f,     1.0f, 0.0f, 0.0f,     1.0f, 1.0f, // top right
             250f, -250f, 0.0f,     0.0f, 1.0f, 0.0f,     1.0f, 0.0f, // bottom right
               0f,   0f, 0.0f,      0.0f, 0.0f, 1.0f,     0.0f, 0.0f, // bottom left
               0f,  250f, 0.0f,     1.0f, 1.0f, 0.0f,     0.0f, 1.0f  // top left
        };

        uint[] _indices1 =
        {
            0, 1, 3,
            1, 2, 3
        };

        float[] _vertices2 =
        {
            // Position             Color
               0f,  250f, 0.0f,     1.0f, 0.0f, 0.0f, // top right
               0f,    0f, 0.0f,     0.0f, 1.0f, 0.0f, // bottom right
            -250f,    0f, 0.0f,     0.0f, 0.0f, 1.0f, // bottom left
            -250f,  250f, 0.0f,     1.0f, 1.0f, 0.0f, // top left
        };

        uint[] _indices2 =
        {
            0, 1, 3,
            1, 2, 3
        };

        private int _vertexArrayObject0;
        private int _vertexArrayObject1;
        private int _vertexArrayObject2;

        private int _elementBuffer0;
        private int _elementBuffer1;
        private int _elementBuffer2;

        private int _vertexBuffer0;
        private int _vertexBuffer1;
        private int _vertexBuffer2;

        private Shader _shader0;
        private Shader _shader1;
        private Shader _shader2;

        private Texture _texture0;

        private Texture _texture1;

        private Camera01 _camera;

        private bool _mouseButtonDown = false;

        private Vector2 _lastPos;

        Color4 _backgroundColor = new(00.2f, 0.3f, 0.3f, 1.0f);

        private int UsedVertexBytes0;
        private int TotalVertexBytes0;
        private int TotalIndexBytes0;
        private int UsedIndexBytes0;

        private int UsedVertexBytes1;
        private int TotalVertexBytes1;
        private int TotalIndexBytes1;
        private int UsedIndexBytes1;

        private int UsedVertexBytes2;
        private int TotalVertexBytes2;
        private int TotalIndexBytes2;
        private int UsedIndexBytes2;

        public int FrameCount { get; private set; }

        public double ElapsedTime { get; private set; }
        public double TotalTime { get; private set; }

        public int FramesPerSecond { get; private set; }

        private long _lastFramesPerSecondTimeStamp { get; set; }

        private readonly Stopwatch _timeStopWatch = new Stopwatch();

        public Sketch002MultipleVertexBuffers(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            _defaultRandomGenerator = new RandomGenerator();
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            QueryGraphicsCardCapabilities();

            Diagnostics.EnableOpenGlDebugMessages();

            GL.ClearColor(_backgroundColor);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.Enable(EnableCap.DepthTest);

            // ------------------------------------------------------------------------------------

            var quadcount = 1024;
            var elements = 0;
            var stride = 0;
            var offset = 0;

            // ------------------------------------------------------------------------------------

            _vertexArrayObject0 = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject0);

            TotalVertexBytes0 = sizeof(float) * 5 * 4 * quadcount;   // 1024 quads containing 4 vertices containing 5 float elements (x,y,z,u,v)
            TotalIndexBytes0 = sizeof(uint) * 6 * quadcount;         // 1024 quads containing 6 uint indices (2 triangles per quad)

            _vertexBuffer0 = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer0);
            GL.BufferData(BufferTarget.ArrayBuffer, TotalVertexBytes0, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            _elementBuffer0 = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBuffer0);
            GL.BufferData(BufferTarget.ElementArrayBuffer, TotalIndexBytes0, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            ClearAndWriteRandomizedVertices0();

            _shader0 = new Shader("Sketch002MultipleDynamicVertexBuffers0");

            elements = 5;
            stride = elements * sizeof(float);
            offset = 0;

            var vertexLocation = _shader0.GetAttribLocation("i_pos");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, stride, offset);

            offset += 3 * sizeof(float);
            var texCoordLocation = _shader0.GetAttribLocation("i_uv0");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, stride, offset);

            // ------------------------------------------------------------------------------------

            _vertexArrayObject1 = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject1);

            TotalVertexBytes1 = sizeof(float) * 5 * 4 * quadcount;   // 1024 quads containing 4 vertices containing 5 float elements (x,y,z,u,v)
            TotalIndexBytes1 = sizeof(uint) * 6 * quadcount;         // 1024 quads containing 6 uint indices (2 triangles per quad)

            _vertexBuffer1 = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer1);
            GL.BufferData(BufferTarget.ArrayBuffer, TotalVertexBytes0, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            _elementBuffer1 = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBuffer1);
            GL.BufferData(BufferTarget.ElementArrayBuffer, TotalIndexBytes0, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            ClearAndWriteRandomizedVertices1();

            _shader1 = new Shader("Sketch002MultipleDynamicVertexBuffers1");

            elements = 8;
            stride = elements * sizeof(float);
            offset = 0;

            vertexLocation = _shader1.GetAttribLocation("i_pos");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, stride, offset);

            offset += 3 * sizeof(float);
            var colorLocation = _shader1.GetAttribLocation("i_rgb");
            GL.EnableVertexAttribArray(colorLocation);
            GL.VertexAttribPointer(colorLocation, 3, VertexAttribPointerType.Float, false, stride, offset);

            offset += 3 * sizeof(float);
            texCoordLocation = _shader1.GetAttribLocation("i_uv0");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, stride, offset);

            // ------------------------------------------------------------------------------------

            _vertexArrayObject2 = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject2);

            TotalVertexBytes2 = sizeof(float) * 6 * 4 * quadcount;   // 1024 quads containing 4 vertices containing 5 float elements (x,y,z,u,v)
            TotalIndexBytes2 = sizeof(uint) * 6 * quadcount;         // 1024 quads containing 6 uint indices (2 triangles per quad)

            _vertexBuffer2 = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer2);
            GL.BufferData(BufferTarget.ArrayBuffer, TotalVertexBytes2, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            _elementBuffer2 = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBuffer2);
            GL.BufferData(BufferTarget.ElementArrayBuffer, TotalIndexBytes2, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            ClearAndWriteRandomizedVertices2();

            _shader2 = new Shader("Sketch002MultipleDynamicVertexBuffers2");

            elements = 6;
            stride = elements * sizeof(float);
            offset = 0;

            vertexLocation = _shader2.GetAttribLocation("i_pos");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, stride, offset);

            offset += 3 * sizeof(float);
            colorLocation = _shader2.GetAttribLocation("i_rgb");
            GL.EnableVertexAttribArray(colorLocation);
            GL.VertexAttribPointer(colorLocation, 3, VertexAttribPointerType.Float, false, stride, offset);

            // ------------------------------------------------------------------------------------

            _texture0 = Texture.LoadFromFile(".resources/.textures./container.png");

            _texture1 = Texture.LoadFromFile(".resources/.textures./awesomeface.png");

            _camera = new Camera01((float)ClientSize.X, (float)ClientSize.Y, MathHelper.PiOver3, ProjectionType.Perspective);

            // We make the mouse cursor invisible and captured so we can have proper FPS-camera movement.
            CursorState = CursorState.Grabbed;

            MousePosition = new Vector2(ClientSize.X / 2, ClientSize.Y / 2);

            _lastPos = new Vector2(MouseState.X, MouseState.Y);

            _timeStopWatch.Start();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // ------------------------------------------------------------------------------------

            var model = Matrix4.Identity;

            // ------------------------------------------------------------------------------------

            GL.BindVertexArray(_vertexArrayObject1);

            _texture1.Use(TextureUnit.Texture0);

            _shader1.Use();

            _shader1.SetMatrix4("u_model", model);
            _shader1.SetMatrix4("u_view", _camera.GetViewMatrix());
            _shader1.SetMatrix4("u_projection", _camera.GetProjectionMatrix());
            _shader1.SetInt("u_tex0", 0);

            GL.DrawElements(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, _indices1.Length, DrawElementsType.UnsignedInt, 0);

            // ------------------------------------------------------------------------------------

            GL.BindVertexArray(_vertexArrayObject2);

            _shader2.Use();

            _shader2.SetMatrix4("u_model", model);
            _shader2.SetMatrix4("u_view", _camera.GetViewMatrix());
            _shader2.SetMatrix4("u_projection", _camera.GetProjectionMatrix());

            GL.DrawElements(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, _indices2.Length, DrawElementsType.UnsignedInt, 0);

            // ------------------------------------------------------------------------------------

            GL.BindVertexArray(_vertexArrayObject0);

            _texture0.Use(TextureUnit.Texture0);
            _texture1.Use(TextureUnit.Texture1);

            _shader0.Use();

            _shader0.SetMatrix4("u_model", model);
            _shader0.SetMatrix4("u_view", _camera.GetViewMatrix());
            _shader0.SetMatrix4("u_projection", _camera.GetProjectionMatrix());
            _shader0.SetInt("u_tex0", 0);
            _shader0.SetInt("u_tex1", 1);

            GL.DrawElements(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, _indices0.Length, DrawElementsType.UnsignedInt, 0);

            // ------------------------------------------------------------------------------------

            SwapBuffers();

            // ------------------------------------------------------------------------------------
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (!IsFocused) // Check to see if the window is focused
            {
                return;
            }

            FrameCount++;
            FramesPerSecond++;
            TotalTime += e.Time;
            ElapsedTime = e.Time;

            var currentTimeStamp = CurrentTimeStamp();

            if (_lastFramesPerSecondTimeStamp + 2000 <= currentTimeStamp)
            {
                Diagnostics.Log($"FPS: {FramesPerSecond}");

                _lastFramesPerSecondTimeStamp = currentTimeStamp;
                FramesPerSecond = 0;
            }

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.2f;

            if (input.IsKeyDown(Keys.W))
            {
                _camera.Position += _camera.Front * cameraSpeed * (float)e.Time; // Forward
            }

            if (input.IsKeyDown(Keys.S))
            {
                _camera.Position -= _camera.Front * cameraSpeed * (float)e.Time; // Backwards
            }
            if (input.IsKeyDown(Keys.A))
            {
                _camera.Position -= _camera.Right * cameraSpeed * (float)e.Time; // Left
            }
            if (input.IsKeyDown(Keys.D))
            {
                _camera.Position += _camera.Right * cameraSpeed * (float)e.Time; // Right
            }
            if (input.IsKeyDown(Keys.Space))
            {
                _camera.Position += _camera.Up * cameraSpeed * (float)e.Time; // Up
            }
            if (input.IsKeyDown(Keys.LeftShift))
            {
                _camera.Position -= _camera.Up * cameraSpeed * (float)e.Time; // Down
            }

            // Get the mouse state
            var mouse = MouseState;

            if (mouse.IsButtonDown(MouseButton.Left))
            {
                if (!_mouseButtonDown)
                {
                    _lastPos = new Vector2(mouse.X, mouse.Y);
                    _mouseButtonDown = true;
                }

                // Calculate the offset of the mouse position
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                _camera.Yaw += deltaX * sensitivity;
                _camera.Pitch -= deltaY * sensitivity; // Reversed since y-coordinates range from bottom to top
            }
            else
            {
                _mouseButtonDown = false;
            }

            if (mouse.IsButtonPressed(MouseButton.Right))
            {
                ClearAndWriteRandomizedVertices0();
                ClearAndWriteRandomizedVertices1();
                ClearAndWriteRandomizedVertices2();
            }

        }

        private void ClearAndWriteRandomizedVertices0()
        {
            ClearBuffers0();

            var top = Random.Value(1, ClientSize.Y / 2);
            var left = Random.Value(-1, -ClientSize.X / 2);
            var right = Random.Value(1, ClientSize.X / 2);
            var bottom = Random.Value(-1, -ClientSize.Y / 2);

            _vertices0 = 
            [
                // Position                   Texture coordinates
                right,  top,        0.0f,     1.0f,     1.0f, // top right
                right,  bottom,     0.0f,     1.0f,     0.0f, // bottom right
                left,   bottom,     0.0f,     0.0f,     0.0f, // bottom left
                left,   top,        0.0f,     0.0f,     1.0f  // top left
            ];

            WriteRaw0(ref _vertices0, ref _indices0);
        }

        private void ClearAndWriteRandomizedVertices1()
        {
            ClearBuffers1();

            var top = Random.Value(1, ClientSize.Y / 2);
            var left = Random.Value(-1, -ClientSize.X / 2);
            var right = Random.Value(1, ClientSize.X / 2);
            var bottom = Random.Value(-1, -ClientSize.Y / 2);

            _vertices1 =
            [
                // Position                 Color                 Texture coordinates
                 right,  top, 0.0f,         1.0f, 0.0f, 0.0f,     1.0f, 1.0f, // top right
                 right,  bottom, 0.0f,      0.0f, 1.0f, 0.0f,     1.0f, 0.0f, // bottom right
                  left,  bottom, 0.0f,      0.0f, 0.0f, 1.0f,     0.0f, 0.0f, // bottom left
                  left,  top, 0.0f,         1.0f, 1.0f, 0.0f,     0.0f, 1.0f,  // top left
            ];

            WriteRaw1(ref _vertices1, ref _indices1);
        }

        private void ClearAndWriteRandomizedVertices2()
        {
            ClearBuffers2();

            var top = Random.Value(1, ClientSize.Y / 2);
            var left = Random.Value(-1, -ClientSize.X / 2);
            var right = Random.Value(1, ClientSize.X / 2);
            var bottom = Random.Value(-1, -ClientSize.Y / 2);

            _vertices2 =
            [
                // Position             Color
                right,  top, 0.0f,        1.0f, 0.0f, 0.0f, // top right
                right,  bottom, 0.0f,     0.0f, 1.0f, 0.0f, // bottom right
                left,   bottom, 0.0f,     0.0f, 0.0f, 1.0f, // bottom left
                left,   top, 0.0f,        1.0f, 1.0f, 0.0f, // top left
            ];

            WriteRaw2(ref _vertices2, ref _indices2);
        }

        // In the mouse wheel function, we manage all the zooming of the camera.
        // This is simply done by changing the FOV of the camera.
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            //_camera.Fov -= e.OffsetY;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
            // We need to update the aspect ratio once the window has been resized.
            _camera.AspectRatio = ClientSize.X / (float)ClientSize.Y;
        }

        public void WriteRaw0(ref float[] vertexdata, ref uint[] indexdata)
        {
            var vertexsize = vertexdata.Length * sizeof(float);
            var indexsize = indexdata.Length * sizeof(uint);

            if (UsedVertexBytes0 + vertexsize > TotalVertexBytes0)
            {
                throw new Exception("VertexBuffer<T>.Write(ref T[] data) failed. The VertexBuffer isn't large enough to hold this vertex data.");
            }

            if (UsedIndexBytes0 + indexsize > TotalIndexBytes0)
            {
                throw new Exception("VertexBuffer<T>.Write(ref float[] vertexdata, ref uint[] indexdata) failed. The VertexBuffer isn't large enough to hold this index data.");
            }

            GL.BindVertexArray(_vertexArrayObject0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer0);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)UsedVertexBytes0, vertexsize, vertexdata);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBuffer0);
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)UsedIndexBytes0, indexsize, indexdata);

            UsedVertexBytes0 += vertexsize;
            UsedIndexBytes0 += indexsize;
        }

        public void WriteRaw1(ref float[] vertexdata, ref uint[] indexdata)
        {
            var vertexsize = vertexdata.Length * sizeof(float);
            var indexsize = indexdata.Length * sizeof(uint);

            if (UsedVertexBytes1 + vertexsize > TotalVertexBytes1)
            {
                throw new Exception("VertexBuffer<T>.Write(ref T[] data) failed. The VertexBuffer isn't large enough to hold this vertex data.");
            }

            if (UsedIndexBytes1 + indexsize > TotalIndexBytes1)
            {
                throw new Exception("VertexBuffer<T>.Write(ref float[] vertexdata, ref uint[] indexdata) failed. The VertexBuffer isn't large enough to hold this index data.");
            }

            GL.BindVertexArray(_vertexArrayObject1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer1);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)UsedVertexBytes1, vertexsize, vertexdata);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBuffer1);
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)UsedIndexBytes1, indexsize, indexdata);

            UsedVertexBytes1 += vertexsize;
            UsedIndexBytes1 += indexsize;
        }

        public void WriteRaw2(ref float[] vertexdata, ref uint[] indexdata)
        {
            var vertexsize = vertexdata.Length * sizeof(float);
            var indexsize = indexdata.Length * sizeof(uint);

            if (UsedVertexBytes2 + vertexsize > TotalVertexBytes2)
            {
                throw new Exception("VertexBuffer<T>.Write(ref T[] data) failed. The VertexBuffer isn't large enough to hold this vertex data.");
            }

            if (UsedIndexBytes2 + indexsize > TotalIndexBytes2)
            {
                throw new Exception("VertexBuffer<T>.Write(ref float[] vertexdata, ref uint[] indexdata) failed. The VertexBuffer isn't large enough to hold this index data.");
            }

            GL.BindVertexArray(_vertexArrayObject2);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer2);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)UsedVertexBytes2, vertexsize, vertexdata);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBuffer2);
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)UsedIndexBytes2, indexsize, indexdata);

            UsedVertexBytes2 += vertexsize;
            UsedIndexBytes2 += indexsize;
        }

        public void ClearBuffers0()
        {
            GL.BindVertexArray(_vertexArrayObject0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer0);
            GL.InvalidateBufferData(_vertexBuffer0);


            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBuffer0);
            GL.InvalidateBufferData(_elementBuffer0);

            UsedVertexBytes0 = 0;
            UsedIndexBytes0 = 0;
        }

        public void ClearBuffers1()
        {
            GL.BindVertexArray(_vertexArrayObject1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer1);
            GL.InvalidateBufferData(_vertexBuffer1);


            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBuffer1);
            GL.InvalidateBufferData(_elementBuffer1);

            UsedVertexBytes1 = 0;
            UsedIndexBytes1 = 0;
        }

        public void ClearBuffers2()
        {
            GL.BindVertexArray(_vertexArrayObject2);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer2);
            GL.InvalidateBufferData(_vertexBuffer2);


            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBuffer2);
            GL.InvalidateBufferData(_elementBuffer2);

            UsedVertexBytes2 = 0;
            UsedIndexBytes2 = 0;
        }

        public long CurrentTimeStamp()
        {
            return _timeStopWatch.ElapsedMilliseconds;
        }

        protected void QueryGraphicsCardCapabilities()
        {
            GL.GetInteger(GetPName.MajorVersion, out int majorVersion);
            GL.GetInteger(GetPName.MinorVersion, out int minorVersion);
            Diagnostics.Log($"OpenGL version: {majorVersion}.{minorVersion}");

            GL.GetInteger(GetPName.MaxVertexAttribs, out int maxAttributeCount);
            Diagnostics.Log($"Maximum number of vertex attributes supported: {maxAttributeCount}");

            GL.GetInteger(GetPName.MaxTextureUnits, out int maxTextureUnits);
            Diagnostics.Log($"Maximum number of texture units supported: {maxTextureUnits}");

            GL.GetInteger(GetPName.MaxTextureSize, out int maxTextureSize);
            Diagnostics.Log($"Maximum texture size supported: {maxTextureSize}");

            GL.GetInteger(GetPName.MaxClipDistances, out int maxClipDistances);
            Diagnostics.Log($"Maximum clip distance: {maxClipDistances}");

            GL.GetInteger(GetPName.MaxSamples, out int maxSamples);
            Diagnostics.Log($"Maximum samples: {maxSamples}");
        }
    }
}