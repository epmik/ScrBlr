
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
    public class Sketch002TexturedVertexBuffer : GameWindow
    {

        private IRandomGenerator _defaultRandomGenerator;
        public IRandomGenerator Random { get { return _defaultRandomGenerator; } }

        float[] _vertices =
        {
            // Position             Texture coordinates
             250f,  250f, 0.0f,     1.0f, 1.0f, // top right
             250f, -250f, 0.0f,     1.0f, 0.0f, // bottom right
            -250f, -250f, 0.0f,     0.0f, 0.0f, // bottom left
            -250f,  250f, 0.0f,     0.0f, 1.0f  // top left
        };

        uint[] _indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        private int _elementBufferObjectTextured;

        private int _vertexBufferObjectTextured;

        private int _vertexArrayObjectTextured;

        private Shader _shaderTextured;

        private Texture _texture0;

        private Texture _texture1;

        private Camera01 _camera;

        private bool _mouseButtonDown = false;

        private Vector2 _lastPos;

        Color4 _backgroundColor = new(00.2f, 0.3f, 0.3f, 1.0f);
        private int UsedVertexBytes;
        private int TotalVertexBytes;
        private int TotalIndexBytes;
        private int UsedIndexBytes;

        public int FrameCount { get; private set; }

        public double ElapsedTime { get; private set; }
        public double TotalTime { get; private set; }

        public int FramesPerSecond { get; private set; }

        private long _lastFramesPerSecondTimeStamp { get; set; }

        private readonly Stopwatch _timeStopWatch = new Stopwatch();

        public Sketch002TexturedVertexBuffer(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
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

            _vertexArrayObjectTextured = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObjectTextured);

            var quadcount = 1024;

            TotalVertexBytes = sizeof(float) * 5 * 4 * quadcount;   // 1024 quads containing 4 vertices containing 5 float elements (x,y,z,u,v)
            TotalIndexBytes = sizeof(uint) * 6 * quadcount;         // 1024 quads containing 6 uint indices (2 triangles per quad)

            _vertexBufferObjectTextured = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObjectTextured);
            //GL.BufferData(BufferTarget.ArrayBuffer, _verticesTextured.Length * sizeof(float), _verticesTextured, BufferUsageHint.StaticDraw);
            GL.BufferData(BufferTarget.ArrayBuffer, TotalVertexBytes, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            _elementBufferObjectTextured = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObjectTextured);
            //GL.BufferData(BufferTarget.ElementArrayBuffer, _indicesTextured.Length * sizeof(uint), _indicesTextured, BufferUsageHint.StaticDraw);
            GL.BufferData(BufferTarget.ElementArrayBuffer, TotalIndexBytes, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            ClearAndWriteRandomizedVertices();

            _shaderTextured = new Shader("Sketch002TexturedDynamicVertexBuffer");
            _shaderTextured.Use();

            var elements = 5;
            var stride = elements * sizeof(float);
            var offset = 0;

            var vertexLocation = _shaderTextured.GetAttribLocation("i_pos");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, stride, offset);

            offset += 3 * sizeof(float);
            var texCoordLocation = _shaderTextured.GetAttribLocation("i_uv0");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, stride, offset);

            _texture0 = Texture.LoadFromFile(".resources/.textures./container.png");
            _texture0.Use(TextureUnit.Texture0);

            _texture1 = Texture.LoadFromFile(".resources/.textures./awesomeface.png");
            _texture1.Use(TextureUnit.Texture1);

            _shaderTextured.SetInt("u_tex0", 0);
            _shaderTextured.SetInt("u_tex1", 1);

            //_camera = new Camera(ClientSize.X / (float)ClientSize.Y, ProjectionType.Perspective);
            //_camera = new Camera((float)ClientSize.X, (float)ClientSize.Y, ProjectionType.Orthographic);
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

            //GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(_vertexArrayObjectTextured);

            _texture0.Use(TextureUnit.Texture0);
            _texture1.Use(TextureUnit.Texture1);

            _shaderTextured.Use();

            var model = Matrix4.Identity;// * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(_time));

            _shaderTextured.SetMatrix4("u_model", model);
            _shaderTextured.SetMatrix4("u_view", _camera.GetViewMatrix());
            _shaderTextured.SetMatrix4("u_projection", _camera.GetProjectionMatrix());

            GL.DrawElements(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();

            //_texture0.Disable(TextureUnit.Texture0);
            //_texture1.Disable(TextureUnit.Texture1);
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
                ClearAndWriteRandomizedVertices();
            }

        }

        private void ClearAndWriteRandomizedVertices()
        {
            ClearBuffers();

            var top = Random.Value(1, ClientSize.Y / 2);
            var left = Random.Value(-1, -ClientSize.X / 2);
            var right = Random.Value(1, ClientSize.X / 2);
            var bottom = Random.Value(-1, -ClientSize.Y / 2);

            _vertices = 
            [
                // Position                   Texture coordinates
                right,  top,        0.0f,     1.0f,     1.0f, // top right
                right,  bottom,     0.0f,     1.0f,     0.0f, // bottom right
                left,   bottom,     0.0f,     0.0f,     0.0f, // bottom left
                left,   top,        0.0f,     0.0f,     1.0f  // top left
            ];

            WriteRaw(ref _vertices, ref _indices);
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

        public void WriteRaw(ref float[] vertexdata, ref uint[] indexdata)
        {
            var vertexsize = vertexdata.Length * sizeof(float);
            var indexsize = indexdata.Length * sizeof(uint);

            if (UsedVertexBytes + vertexsize > TotalVertexBytes)
            {
                throw new Exception("VertexBuffer<T>.Write(ref T[] data) failed. The VertexBuffer isn't large enough to hold this vertex data.");
            }

            if (UsedIndexBytes + indexsize > TotalIndexBytes)
            {
                throw new Exception("VertexBuffer<T>.Write(ref float[] vertexdata, ref uint[] indexdata) failed. The VertexBuffer isn't large enough to hold this index data.");
            }

            GL.BindVertexArray(_vertexArrayObjectTextured);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObjectTextured);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)UsedVertexBytes, vertexsize, vertexdata);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObjectTextured);
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)UsedIndexBytes, indexsize, indexdata);

            UsedVertexBytes += vertexsize;
            UsedIndexBytes += indexsize;
        }

        public void ClearBuffers()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObjectTextured);

            //GL.BufferData(BufferTarget.ArrayBuffer, BufferSize, IntPtr.Zero, (BufferUsageHint)VertexBufferUsage);

            //GL.ClearBufferSubData(BufferTarget.ArrayBuffer, (IntPtr)UsedCount, size, data);

            GL.InvalidateBufferData(_vertexBufferObjectTextured);


            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObjectTextured);

            GL.InvalidateBufferData(_elementBufferObjectTextured);

            UsedVertexBytes = 0;
            UsedIndexBytes = 0;
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