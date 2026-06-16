
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace Skrbl
{
    public class Sketch004Quad : GameWindow
    {
        private IRandomGenerator _defaultRandomGenerator;

        public IRandomGenerator Random { get { return _defaultRandomGenerator; } }

        struct FrameBufferObject
        {
            public int Width;
            public int Height;
            public int Handle;
            public int ColorBufferHandle;
            public int RenderBufferHandle;
        }

        struct VertexBufferObject
        {
            public int Handle;
            public int VertexArrayHandle;
            public int IndexArrayHandle;

            public int UsedVertexBytes;
            public int TotalVertexBytes;
            public int TotalIndexBytes;
            public int UsedIndexBytes;

            internal void Clear()
            {
                GL.BindVertexArray(Handle);


                GL.BindBuffer(BufferTarget.ArrayBuffer, VertexArrayHandle);
                //GL.BufferData(BufferTarget.ArrayBuffer, BufferSize, IntPtr.Zero, (BufferUsageHint)VertexBufferUsage);
                //GL.ClearBufferSubData(BufferTarget.ArrayBuffer, (IntPtr)UsedCount, size, data);
                GL.InvalidateBufferData(VertexArrayHandle);


                GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexArrayHandle);
                GL.InvalidateBufferData(IndexArrayHandle);

                UsedVertexBytes = 0;
                UsedIndexBytes = 0;
            }

            public void Write(ref float[] vertexdata, ref uint[] indexdata)
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

                GL.BindVertexArray(Handle);

                GL.BindBuffer(BufferTarget.ArrayBuffer, VertexArrayHandle);
                GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)UsedVertexBytes, vertexsize, vertexdata);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexArrayHandle);
                GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)UsedIndexBytes, indexsize, indexdata);

                UsedVertexBytes += vertexsize;
                UsedIndexBytes += indexsize;
            }
        }

        float[] _verticesTextured;

        uint[] _indicesTextured =
        {
            0, 1, 3,
            2,
        };

        private Shader _shader;

        private Texture _texture0;
        private Texture _texture1;

        private Camera01 _camera;

        private bool _mouseButtonDown = false;

        private Vector2 _lastPos;

        Color4 _backgroundColor = new(00.2f, 0.3f, 0.3f, 0.0f);

        bool _isRightMouseButtonPressed;

        public int FrameCount { get; private set; }

        public double ElapsedTime { get; private set; }
        public double TotalTime { get; private set; }

        public int FramesPerSecond { get; private set; }

        private long _lastFramesPerSecondTimeStamp { get; set; }

        private readonly Stopwatch _timeStopWatch = new Stopwatch();

        private FrameBufferObject _framebufferObject;
        private VertexBufferObject _vertexbufferObject;

        public Sketch004Quad(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
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

            var quadcount = 1024;

            var totalVertexBytes = sizeof(float) * 5 * 4 * quadcount;   // 1024 quads containing 4 vertices containing 5 float elements (x,y,z,u,v)
            var totalIndexBytes = sizeof(uint) * 6 * quadcount;         // 1024 quads containing 6 uint indices (2 triangles per quad)

            _vertexbufferObject = CreateVertexBufferObject(totalVertexBytes, totalIndexBytes);

            ClearAndWriteRandomizedVertices(false);

            GL.BindVertexArray(_vertexbufferObject.Handle);

            _shader = new Shader("Sketch004Quad");
            _shader.Use();

            var stride = 8 * sizeof(float);
            var offset = 0;

            var xyzLocation = _shader.GetAttribLocation("i_pos");
            GL.EnableVertexAttribArray(xyzLocation);
            GL.VertexAttribPointer(xyzLocation, 3, VertexAttribPointerType.Float, false, stride, offset);

            offset += 3 * sizeof(float);
            var rgbLocation = _shader.GetAttribLocation("i_rgb");
            GL.EnableVertexAttribArray(rgbLocation);
            GL.VertexAttribPointer(rgbLocation, 3, VertexAttribPointerType.Float, false, stride, offset);

            offset += 3 * sizeof(float);
            var uvLocation = _shader.GetAttribLocation("i_uv0");
            GL.EnableVertexAttribArray(uvLocation);
            GL.VertexAttribPointer(uvLocation, 2, VertexAttribPointerType.Float, false, stride, offset);

            _texture0 = Texture.LoadFromFile(".resources/.textures./container.png");
            _texture0.Use(TextureUnit.Texture0);

            _texture1 = Texture.LoadFromFile(".resources/.textures./awesomeface.png");
            _texture1.Use(TextureUnit.Texture1);

            _shader.SetInt("u_tex0", 0);
            _shader.SetInt("u_tex1", 1);

            _framebufferObject = CreateFrameBufferObject(ClientSize.X, ClientSize.Y);

            _camera = new Camera01((float)ClientSize.X, (float)ClientSize.Y, ProjectionType.Orthographic);

            // We make the mouse cursor invisible and captured so we can have proper FPS-camera movement.
            CursorState = CursorState.Grabbed;

            MousePosition = new Vector2(ClientSize.X / 2, ClientSize.Y / 2);

            _lastPos = new Vector2(MouseState.X, MouseState.Y);

            _timeStopWatch.Start();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            // bind the framebuffer
            // all commands from here on will be rendered to the framebuffer
            // until we bind the default framebuffer again by calling: GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _framebufferObject.Handle);

            //GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(_vertexbufferObject.Handle);

            _texture0.Use(TextureUnit.Texture0);
            _texture1.Use(TextureUnit.Texture1);

            _shader.Use();

            var model = Matrix4.Identity;// * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(_time));
            _shader.SetMatrix4("u_model", model);
            _shader.SetMatrix4("u_view", _camera.GetViewMatrix());
            _shader.SetMatrix4("u_projection", _camera.GetProjectionMatrix());

            GL.DrawElements(OpenTK.Graphics.OpenGL4.PrimitiveType.TriangleStrip, _vertexbufferObject.UsedIndexBytes / sizeof(uint), DrawElementsType.UnsignedInt, 0);

            //_texture0.Disable(TextureUnit.Texture0);
            //_texture1.Disable(TextureUnit.Texture1);

            // set the default framebuffer as the draw or render target
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            // set the offscreen framebuffer as the read source
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _framebufferObject.Handle);
            GL.BlitFramebuffer(
                0, 0, _framebufferObject.Width, _framebufferObject.Height,
                0, 0, ClientSize.X, ClientSize.Y,
                ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);

            // bind the default framebuffer
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);

            SwapBuffers();
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

            var keyboard = KeyboardState;

            if (keyboard.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.2f;

            if (keyboard.IsKeyDown(Keys.W))
            {
                _camera.Position += _camera.Front * cameraSpeed * (float)e.Time; // Forward
            }

            if (keyboard.IsKeyDown(Keys.S))
            {
                _camera.Position -= _camera.Front * cameraSpeed * (float)e.Time; // Backwards
            }
            if (keyboard.IsKeyDown(Keys.A))
            {
                _camera.Position -= _camera.Right * cameraSpeed * (float)e.Time; // Left
            }
            if (keyboard.IsKeyDown(Keys.D))
            {
                _camera.Position += _camera.Right * cameraSpeed * (float)e.Time; // Right
            }
            if (keyboard.IsKeyDown(Keys.Space))
            {
                _camera.Position += _camera.Up * cameraSpeed * (float)e.Time; // Up
            }
            if (keyboard.IsKeyDown(Keys.LeftShift))
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

            if (mouse.IsButtonDown(MouseButton.Right))
            {
                _isRightMouseButtonPressed = true;
            }
            else
            {
                if (_isRightMouseButtonPressed)
                {
                    ClearAndWriteRandomizedVertices();
                }

                _isRightMouseButtonPressed = false;
            }

            if (keyboard.IsKeyDown(Keys.PrintScreen) || keyboard.IsKeyDown(Keys.F5))
            {
                SaveFrameBufferObject(_framebufferObject);
            }
        }

        private void ClearAndWriteRandomizedVertices(bool randomize = true)
        {
            var top0 = Random.Value(1, ClientSize.Y / 2);
            var top1 = Random.Value(1, ClientSize.Y / 2);
            var left0 = Random.Value(-1, -ClientSize.X / 2);
            var left1 = Random.Value(-1, -ClientSize.X / 2);
            var right0 = Random.Value(1, ClientSize.X / 2);
            var right1 = Random.Value(1, ClientSize.X / 2);
            var bottom0 = Random.Value(-1, -ClientSize.Y / 2);
            var bottom1 = Random.Value(-1, -ClientSize.Y / 2);

            _verticesTextured = new float[]
            {
                // Position                Colors              Texture coordinates
                    right0,  top0,    0.0f,     1.0f, 0.0f, 0.0f,   1.0f, 1.0f, // top right - red
                    right1,  bottom0, 0.0f,     0.0f, 1.0f, 0.0f,   1.0f, 0.0f, // bottom right - green
                    left0,   bottom1, 0.0f,     0.0f, 0.0f, 1.0f,   0.0f, 0.0f, // bottom left - blue
                    left1,   top1,    0.0f,     1.0f, 1.0f, 0.0f,   0.0f, 1.0f  // top left - yellow
            };

            _vertexbufferObject.Clear();

            _vertexbufferObject.Write(ref _verticesTextured, ref _indicesTextured);
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

        public long CurrentTimeStamp()
        {
            return _timeStopWatch.ElapsedMilliseconds;
        }

        private VertexBufferObject CreateVertexBufferObject(int totalVertexBytes, int totalIndexBytes)
        {
            var vertexBufferObject = new VertexBufferObject
            {
                TotalVertexBytes = totalVertexBytes,
                TotalIndexBytes = totalIndexBytes,
            };

            vertexBufferObject.Handle = GL.GenVertexArray();

            GL.BindVertexArray(vertexBufferObject.Handle);

            vertexBufferObject.VertexArrayHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject.VertexArrayHandle);
            //GL.BufferData(BufferTarget.ArrayBuffer, _verticesTextured.Length * sizeof(float), _verticesTextured, BufferUsageHint.StaticDraw);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexBufferObject.TotalVertexBytes, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            vertexBufferObject.IndexArrayHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vertexBufferObject.IndexArrayHandle);
            //GL.BufferData(BufferTarget.ElementArrayBuffer, _indicesTextured.Length * sizeof(uint), _indicesTextured, BufferUsageHint.StaticDraw);
            GL.BufferData(BufferTarget.ElementArrayBuffer, vertexBufferObject.TotalIndexBytes, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            return vertexBufferObject;
        }

        private FrameBufferObject CreateFrameBufferObject(int? width = null, int? height = null)
        {
            var frameBufferObject = new FrameBufferObject
            {
                Width = width == null ? ClientSize.X : width.Value,
                Height = height == null ? ClientSize.Y : height.Value,
            };

            // Create Framebuffer
            frameBufferObject.Handle = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBufferObject.Handle);

            // Create texture to hold color buffer
            frameBufferObject.ColorBufferHandle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, frameBufferObject.ColorBufferHandle);
            //GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, frameBufferObject.Width, frameBufferObject.Height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, frameBufferObject.Width, frameBufferObject.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, frameBufferObject.ColorBufferHandle, 0);

            // Create Renderbuffer Object for depth and stencil attachment (we won't be sampling these)
            frameBufferObject.RenderBufferHandle = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, frameBufferObject.RenderBufferHandle);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, frameBufferObject.Width, frameBufferObject.Height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, frameBufferObject.RenderBufferHandle);

            // Now that we actually created the framebuffer and added all attachments we want to check if it is complete
            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);

            if (!status.Equals(FramebufferErrorCode.FramebufferComplete) && !status.Equals(FramebufferErrorCode.FramebufferCompleteExt))
            {
                Console.WriteLine($"Error creating framebuffer: {status}");
                Thread.Sleep(250);
                Environment.Exit(-1);
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            return frameBufferObject;
        }

        [SupportedOSPlatform("windows")]
        private void SaveFrameBufferObject(FrameBufferObject frameBufferObject, string? destination = null)
        {
            if (string.IsNullOrEmpty(destination))
            {
                destination = $".saves/{DateTime.Now.ToString("yyyyMMdd.HHmmss.ffff")}.png";
            }
            else if (!destination.EndsWith(".png"))
            {
                destination += ".png";
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBufferObject.Handle);

            using (var bitmap = new System.Drawing.Bitmap(
                frameBufferObject.Width,
                frameBufferObject.Height,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                var data = bitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, frameBufferObject.Width, frameBufferObject.Height),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.PixelStore(PixelStoreParameter.PackRowLength, data.Stride / 4);

                GL.ReadPixels(
                    0, 0,
                    frameBufferObject.Width, frameBufferObject.Height,
                    OpenTK.Graphics.OpenGL4.PixelFormat.Bgra,
                    PixelType.UnsignedByte,
                    data.Scan0);

                bitmap.UnlockBits(data);

                bitmap.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);

                bitmap.Save(destination, System.Drawing.Imaging.ImageFormat.Png);
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        private void QueryGraphicsCardCapabilities()
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

            GL.GetInteger(GetPName.MaxRenderbufferSize, out int maxRenderbufferSize);
            Diagnostics.Log($"Maximum render buffer size supported: {maxRenderbufferSize}");

            var maxViewportDims = new int[2];
            GL.GetInteger(GetPName.MaxViewportDims, maxViewportDims);
            Diagnostics.Log($"Maximum viewport dimensions: {maxViewportDims[0]}x{maxViewportDims[1]}");

            GL.GetInteger(GetPName.MaxSamples, out int maxSamples);
            Diagnostics.Log($"Maximum samples: {maxSamples}");

            //GL.GetInteger(GetPName.MaxClipDistances, out int maxClipDistances);
            //Diagnostics.Log($"Maximum clip distance: {maxClipDistances}");
        }
    }
}