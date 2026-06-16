
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;

namespace Skrbl
{
    [Sketch(Name = "Toggle frame buffers")]
    public class Sketch007ToggleFrameBuffers : GameWindow
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

            public FrameBufferObject(int width, int height)
            {
                Width = width;
                Height = height;

                // Create Framebuffer
                Handle = GL.GenFramebuffer();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);

                // Create texture to hold color buffer
                ColorBufferHandle = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, ColorBufferHandle);
                //GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, frameBufferObject.Width, frameBufferObject.Height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, ColorBufferHandle, 0);

                // Create Renderbuffer Object for depth and stencil attachment (we won't be sampling these)
                RenderBufferHandle = GL.GenRenderbuffer();
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RenderBufferHandle);
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, Width, Height);
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, RenderBufferHandle);

                // Now that we actually created the framebuffer and added all attachments we want to check if it is complete
                var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);

                if (!status.Equals(FramebufferErrorCode.FramebufferComplete) && !status.Equals(FramebufferErrorCode.FramebufferCompleteExt))
                {
                    Console.WriteLine($"Error creating framebuffer: {status}");
                    Thread.Sleep(250);
                    Environment.Exit(-1);
                }

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }

            [SupportedOSPlatform("windows")]
            public void Save(string? destination = null)
            {
                if (string.IsNullOrEmpty(destination))
                {
                    destination = $".saves/{DateTime.Now.ToString("yyyyMMdd.HHmmss.ffff")}.png";
                }
                else if (!destination.EndsWith(".png"))
                {
                    destination += ".png";
                }

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);

                using (var bitmap = new System.Drawing.Bitmap(
                    Width,
                    Height,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    var data = bitmap.LockBits(
                        new System.Drawing.Rectangle(0, 0, Width, Height),
                        System.Drawing.Imaging.ImageLockMode.WriteOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    GL.PixelStore(PixelStoreParameter.PackRowLength, data.Stride / 4);

                    GL.ReadPixels(
                        0, 0,
                        Width, Height,
                        OpenTK.Graphics.OpenGL4.PixelFormat.Bgra,
                        PixelType.UnsignedByte,
                        data.Scan0);

                    bitmap.UnlockBits(data);

                    bitmap.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);

                    bitmap.Save(destination, System.Drawing.Imaging.ImageFormat.Png);
                }

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }
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

            public VertexBufferObject(int totalVertexBytes, int totalIndexBytes = 0)
            {
                TotalVertexBytes = totalVertexBytes;
                TotalIndexBytes = totalIndexBytes;

                Handle = GL.GenVertexArray();

                GL.BindVertexArray(Handle);

                VertexArrayHandle = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, VertexArrayHandle);
                GL.BufferData(BufferTarget.ArrayBuffer, TotalVertexBytes, IntPtr.Zero, BufferUsageHint.DynamicDraw);

                if(TotalIndexBytes > 0)
                {
                    IndexArrayHandle = GL.GenBuffer();
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexArrayHandle);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, TotalIndexBytes, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                }
            }

            internal void Clear()
            {
                GL.BindVertexArray(Handle);

                GL.BindBuffer(BufferTarget.ArrayBuffer, VertexArrayHandle);
                GL.InvalidateBufferData(VertexArrayHandle);

                if (TotalIndexBytes > 0)
                {
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexArrayHandle);
                    GL.InvalidateBufferData(IndexArrayHandle);
                }

                UsedVertexBytes = 0;
                UsedIndexBytes = 0;
            }

            internal void Write(ref float[] vertexdata)
            {
                Write(ref vertexdata, out int vertexByteSize);
            }

            internal void Write(ref float[] vertexdata, out int vertexByteSize)
            {
                vertexByteSize = vertexdata.Length * sizeof(float);

                if (UsedVertexBytes + vertexByteSize > TotalVertexBytes)
                {
                    throw new Exception("VertexBuffer<T>.Write(ref T[] data) failed. The VertexBuffer isn't large enough to hold this vertex data.");
                }

                GL.BindVertexArray(Handle);

                GL.BindBuffer(BufferTarget.ArrayBuffer, VertexArrayHandle);
                GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)UsedVertexBytes, vertexByteSize, vertexdata);

                UsedVertexBytes += vertexByteSize;
            }

            internal void Write(ref float[] vertexdata, ref uint[] indexdata)
            {
                Write(ref vertexdata, ref indexdata, out int vertexByteSize, out int indexByteSize);
            }

            internal void Write(ref float[] vertexdata, ref uint[] indexdata, out int vertexByteSize, out int indexByteSize)
            {
                vertexByteSize = vertexdata.Length * sizeof(float);
                indexByteSize = indexdata.Length * sizeof(uint);

                if (UsedVertexBytes + vertexByteSize > TotalVertexBytes)
                {
                    throw new Exception("VertexBuffer<T>.Write(ref T[] data) failed. The VertexBuffer isn't large enough to hold this vertex data.");
                }

                if (UsedIndexBytes + indexByteSize > TotalIndexBytes)
                {
                    throw new Exception("VertexBuffer<T>.Write(ref float[] vertexdata, ref uint[] indexdata) failed. The VertexBuffer isn't large enough to hold this index data.");
                }

                GL.BindVertexArray(Handle);

                GL.BindBuffer(BufferTarget.ArrayBuffer, VertexArrayHandle);
                GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)UsedVertexBytes, vertexByteSize, vertexdata);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexArrayHandle);
                GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)UsedIndexBytes, indexByteSize, indexdata);

                UsedVertexBytes += vertexByteSize;
                UsedIndexBytes += indexByteSize;
            }
        }

        bool _drawToFramebuffer = true;

        // must be set to 1.0f since GL.BlitFramebuffer generates an error if the source and destination frame buffers are of different sizes
        private float _frameBufferScale = 1.0f;

        float[] _lineVertices;
        int _vertexCount;

        private Shader _shader;

        private Camera01 _camera;

        private bool _mouseButtonDown = false;

        private Vector2 _lastPos;

        Color4 _backgroundColor = new(251.0f / 255.0f, 204.0f / 255.0f, 122.0f / 255.0f, 0.0f);

        bool _isRightMouseButtonPressed;

        public int FrameCount { get; private set; }

        public double ElapsedTime { get; private set; }
        public double TotalTime { get; private set; }

        public int FramesPerSecond { get; private set; }

        private long _lastFramesPerSecondTimeStamp { get; set; }

        private readonly Stopwatch _timeStopWatch = new Stopwatch();

        private FrameBufferObject _framebufferObject;
        private VertexBufferObject _vertexbufferObject;

        public Sketch007ToggleFrameBuffers(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            _defaultRandomGenerator = new RandomGenerator();

            if (string.IsNullOrEmpty(Title))
            {
                var typeInfo = GetType().GetTypeInfo();
                var sketchAttribute = typeInfo.GetCustomAttribute<SketchAttribute>();

                if (sketchAttribute != null && !string.IsNullOrEmpty(sketchAttribute.Name))
                {
                    Title = sketchAttribute.Name;
                }
                else
                {
                    Title = GetType().Name;
                }
            }
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            QueryGraphicsCardCapabilities();

            Diagnostics.EnableOpenGlDebugMessages();

            GL.ClearColor(_backgroundColor);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.Enable(EnableCap.DepthTest);

            var linecount = 4096;

            var totalVertexBytes = linecount * 2 * 6 * sizeof(float);   // 4096 lines containing 2 vertices each containing 6 float elements (x,y,z,r,g,b)

            _vertexbufferObject = new(totalVertexBytes);

            ClearAndBindGridVertices(200, 20, Color4.White);

            GL.BindVertexArray(_vertexbufferObject.Handle);

            _shader = new Shader("Sketch007RenderToTexture");
            _shader.Use();

            var elements = 6;
            var stride = elements * sizeof(float);
            var offset = 0;

            var xyzLocation = _shader.GetAttribLocation("i_pos");
            GL.EnableVertexAttribArray(xyzLocation);
            GL.VertexAttribPointer(xyzLocation, 3, VertexAttribPointerType.Float, false, stride, offset);

            offset += 3 * sizeof(float);
            var rgbLocation = _shader.GetAttribLocation("i_rgb");
            GL.EnableVertexAttribArray(rgbLocation);
            GL.VertexAttribPointer(rgbLocation, 3, VertexAttribPointerType.Float, false, stride, offset);


            _framebufferObject = new FrameBufferObject((int)(ClientSize.X * _frameBufferScale), (int)(ClientSize.Y * _frameBufferScale));

            _camera = new Camera01((float)ClientSize.X, (float)ClientSize.Y, ProjectionType.Perspective);

            _camera.Position += new Vector3(0, 50, 50);
            _camera.Pitch = -30.0f;

            // We make the mouse cursor invisible and captured so we can have proper FPS-camera movement.
            CursorState = CursorState.Grabbed;

            MousePosition = new Vector2(ClientSize.X / 2, ClientSize.Y / 2);

            _lastPos = new Vector2(MouseState.X, MouseState.Y);

            _timeStopWatch.Start();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            //base.OnRenderFrame(e);

            if(_drawToFramebuffer)
            {
                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _framebufferObject.Handle);

                // DrawBuffer call must be placed after binding the framebuffer
                //GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

                GL.ClearColor(new Color4(251.0f / 255.0f, 161.0f / 255.0f, 122.0f / 255.0f, 0.0f));

                GL.Viewport(0, 0, _framebufferObject.Width, _framebufferObject.Height);

                _camera.AspectRatio = _framebufferObject.Width / (float)_framebufferObject.Height;

            }
            else
            {
                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);

                // DrawBuffer call must be placed after binding the framebuffer
                //GL.DrawBuffer(DrawBufferMode.Front);

                GL.ClearColor(_backgroundColor);

                GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

                _camera.AspectRatio = ClientSize.X / (float)ClientSize.Y;
            }

            // Multiple color buffers can be cleared simultaneously by selecting more than one buffer at a time using GL.DrawBuffer(...)
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(_vertexbufferObject.Handle);

            _shader.Use();

            var model = Matrix4.Identity;// * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(_time));
            _shader.SetMatrix4("u_model", model);
            _shader.SetMatrix4("u_view", _camera.GetViewMatrix());
            _shader.SetMatrix4("u_projection", _camera.GetProjectionMatrix());

            GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Lines, 0, _vertexCount);

            if(_drawToFramebuffer)
            {
                // set the default framebuffer as the draw or render target
                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
                // set the offscreen framebuffer as the read source
                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _framebufferObject.Handle);
                // https://registry.khronos.org/OpenGL-Refpages/gl4/html/glBlitFramebuffer.xhtml
                GL.BlitFramebuffer(
                    0, 0, _framebufferObject.Width, _framebufferObject.Height,
                    0, 0, ClientSize.X, ClientSize.Y,
                    ClearBufferMask.ColorBufferBit, 
                    BlitFramebufferFilter.Linear);

                //var error = GL.GetError();

                //if (error != ErrorCode.NoError)
                //{
                //    Diagnostics.Log($"Error blitting framebuffer: {error}");
                //}
            }

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
                //if (_isRightMouseButtonPressed)
                //{
                //    ClearAndBindGridVertices();
                //}

                _isRightMouseButtonPressed = false;
            }

            if (keyboard.IsKeyPressed(Keys.PrintScreen) || keyboard.IsKeyPressed(Keys.F5))
            {
                if (OperatingSystem.IsWindows())
                {
                    _framebufferObject.Save();
                }
            }

            if (keyboard.IsKeyPressed(Keys.Enter))
            {
                _drawToFramebuffer = !_drawToFramebuffer;
                Diagnostics.Log($"Draw to framebuffer: {_drawToFramebuffer}");
            }
        }

        private void ClearAndBindGridVertices(int size, int divisions, Color4 color)
        {
            _vertexbufferObject.Clear();

            var center = divisions / 2;
            var step = size / divisions;
            var halfSize = size / 2;

            _vertexCount = (divisions + 1) * 4;
            _lineVertices = new float[_vertexCount * 6];    // 6 floats per vertex (x,y,z,r,g,b)

            var j = 0;
            var k = -halfSize;

            for (var i = 0; i <= divisions; i++, k += step)
            {
                _lineVertices[j++] = -halfSize;
                _lineVertices[j++] = 0;
                _lineVertices[j++] = k;
                // color
                _lineVertices[j++] = color.R;
                _lineVertices[j++] = color.G;
                _lineVertices[j++] = color.B;

                _lineVertices[j++] = halfSize;
                _lineVertices[j++] = 0;
                _lineVertices[j++] = k;
                // color
                _lineVertices[j++] = color.R;
                _lineVertices[j++] = color.G;
                _lineVertices[j++] = color.B;


                _lineVertices[j++] = k;
                _lineVertices[j++] = 0;
                _lineVertices[j++] = -halfSize;
                // color
                _lineVertices[j++] = color.R;
                _lineVertices[j++] = color.G;
                _lineVertices[j++] = color.B;

                _lineVertices[j++] = k;
                _lineVertices[j++] = 0;
                _lineVertices[j++] = halfSize;
                // color
                _lineVertices[j++] = color.R;
                _lineVertices[j++] = color.G;
                _lineVertices[j++] = color.B;
            }

            _vertexbufferObject.Write(ref _lineVertices);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
        }

        //protected override void OnResize(ResizeEventArgs e)
        //{
        //    base.OnResize(e);

        //    GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

        //    _camera.AspectRatio = ClientSize.X / (float)ClientSize.Y;
        //}

        public long CurrentTimeStamp()
        {
            return _timeStopWatch.ElapsedMilliseconds;
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

        private static DebugProc DebugMessageDelegate = DebugMessageHandler;

        private static void DebugMessageHandler(
            DebugSource source,     // Source of the debugging message.
            DebugType type,         // Type of the debugging message.
            int id,                 // ID associated with the message.
            DebugSeverity severity, // Severity of the message.
            int length,             // Length of the string in pMessage.
            IntPtr pMessage,        // Pointer to message string.
            IntPtr pUserParam)      // The pointer you gave to OpenGL, explained later.
        {
            // In order to access the string pointed to by pMessage, you can use Marshal
            // class to copy its contents to a C# string without unsafe code. You can
            // also use the new function Marshal.PtrToStringUTF8 since .NET Core 1.1.
            string message = Marshal.PtrToStringAnsi(pMessage, length);

            // The rest of the function is up to you to implement, however a debug output
            // is always useful.
            Diagnostics.Log($"[{severity} source={source} type={type} id={id}] {message}");

            //// Potentially, you may want to throw from the function for certain severity
            //// messages.
            //if (type == DebugType.DebugTypeError)
            //{
            //    throw new Exception(message);
            //}
        }
    }
}