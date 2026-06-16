
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.Versioning;
using static Skrbl.Sketch008BoxGeometry;

namespace Skrbl
{
    public class Sketch008BoxGeometry : GameWindow
    {
        private IRandomGenerator _defaultRandomGenerator;

        public IRandomGenerator Random { get { return _defaultRandomGenerator; } }

        bool _drawGeometry = false;

        float[] _lineVertices;
        int _vertexCount;

        private Shader _lineShader;
        private Shader _quadShader;

        private Camera01 _camera;

        private bool _mouseButtonDown = false;

        private Vector2 _lastPos;

        Color4 _primaryBackgroundColor = new(251.0f / 255.0f, 204.0f / 255.0f, 122.0f / 255.0f, 0.0f);
        Color4 _secundaryBackgroundColor = new(251.0f / 255.0f, 161.0f / 255.0f, 122.0f / 255.0f, 0.0f);

        bool _isRightMouseButtonPressed;

        public int FrameCount { get; private set; }

        public double ElapsedTime { get; private set; }
        public double TotalTime { get; private set; }

        public int FramesPerSecond { get; private set; }

        private long _lastFramesPerSecondTimeStamp { get; set; }

        private readonly Stopwatch _timeStopWatch = new Stopwatch();

        private FrameBufferObject _framebufferObject;
        private VertexBufferObject _vertexbufferObject;
        private LineGeometry _lineGeometry;
        private QuadGeometry _quadGeometry;

        private Texture _texture0;
        private Texture _texture1;

        public Sketch008BoxGeometry(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            _defaultRandomGenerator = new RandomGenerator();
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            QueryGraphicsCardCapabilities();

            Diagnostics.EnableOpenGlDebugMessages();

            GL.ClearColor(_drawGeometry ? _secundaryBackgroundColor : _primaryBackgroundColor);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.Enable(EnableCap.DepthTest);

            _framebufferObject = new FrameBufferObject(ClientSize.X, ClientSize.Y);

            var linecount = 4096;

            var totalVertexBytes = linecount * 2 * 6 * sizeof(float);   // 4096 lines containing 2 vertices each containing 6 float elements (x,y,z,r,g,b)

            _vertexbufferObject = new(totalVertexBytes);

            ClearAndBindGridVertices(200, 20, Color4.White);

            _lineShader = new Shader("Sketch008BoxGeometryLine");
            _quadShader = new Shader("Sketch008BoxGeometryQuad");

            _lineGeometry = new LineGeometry(200, 20, Color4.White, _lineShader);
            
            //_quadGeometry = new QuadGeometry(50, _secundaryBackgroundColor, _quadShader);
            //_quadGeometry.Attributes.Disable(_quadShader);


            _texture0 = Texture.LoadFromFile(".resources/.textures./container.png");
            _texture0.Use(TextureUnit.Texture0);

            _texture1 = Texture.LoadFromFile(".resources/.textures./awesomeface.png");
            _texture1.Use(TextureUnit.Texture1);

            _quadShader.SetInt("u_tex0", 0);
            _quadShader.SetInt("u_tex1", 1);


            var elements = 6;
            var stride = elements * sizeof(float);
            var offset = 0;

            GL.BindVertexArray(_vertexbufferObject.Handle);

            var xyzLocation = _lineShader.GetAttribLocation("i_pos");
            GL.EnableVertexAttribArray(xyzLocation);
            GL.VertexAttribPointer(xyzLocation, 3, VertexAttribPointerType.Float, false, stride, offset);

            offset += 3 * sizeof(float);
            var rgbLocation = _lineShader.GetAttribLocation("i_rgb");
            GL.EnableVertexAttribArray(rgbLocation);
            GL.VertexAttribPointer(rgbLocation, 3, VertexAttribPointerType.Float, false, stride, offset);

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
            base.OnRenderFrame(e);

            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _framebufferObject.Handle);

            GL.ClearColor(_drawGeometry ? _secundaryBackgroundColor : _primaryBackgroundColor);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // ------------------------------------------------------------------------------------
            // line geometry

            if (_drawGeometry)
            {
                _lineGeometry.Bind();
            }
            else
            {
                GL.BindVertexArray(_vertexbufferObject.Handle);

                var xyzLocation = _lineShader.GetAttribLocation("i_pos");
                GL.EnableVertexAttribArray(xyzLocation);

                var rgbLocation = _lineShader.GetAttribLocation("i_rgb");
                GL.EnableVertexAttribArray(rgbLocation);
            }

            _lineShader.Use();

            var model = Matrix4.Identity;// * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(_time));
            _lineShader.SetMatrix4("u_model", model);
            _lineShader.SetMatrix4("u_view", _camera.GetViewMatrix());
            _lineShader.SetMatrix4("u_projection", _camera.GetProjectionMatrix());

            GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Lines, 0, _vertexCount);

            // ------------------------------------------------------------------------------------

            // ------------------------------------------------------------------------------------
            // quad geometry
            
            //_quadGeometry.Bind();
            //_quadGeometry.Attributes.Enable(_quadShader);

            //_quadShader.Use();


            //_quadShader.SetMatrix4("u_model", model);
            //_quadShader.SetMatrix4("u_view", _camera.GetViewMatrix());
            //_quadShader.SetMatrix4("u_projection", _camera.GetProjectionMatrix());

            //GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

            //_quadGeometry.Attributes.Disable(_quadShader);

            // ------------------------------------------------------------------------------------

            // ------------------------------------------------------------------------------------
            // box geometry

            // ------------------------------------------------------------------------------------

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
                _drawGeometry = !_drawGeometry;
                Diagnostics.Log($"Draw line geometry: {_drawGeometry}");
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

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

            _camera.AspectRatio = ClientSize.X / (float)ClientSize.Y;
        }

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

            public VertexBufferAttributes Attributes = new VertexBufferAttributes();

            public VertexBufferObject(int totalVertexBytes, int totalIndexBytes = 0)
            {
                TotalVertexBytes = totalVertexBytes;
                TotalIndexBytes = totalIndexBytes;

                Handle = GL.GenVertexArray();

                GL.BindVertexArray(Handle);

                VertexArrayHandle = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, VertexArrayHandle);
                GL.BufferData(BufferTarget.ArrayBuffer, TotalVertexBytes, IntPtr.Zero, BufferUsageHint.DynamicDraw);

                if (TotalIndexBytes > 0)
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

        public class VertexBufferAttributes
        {
            List<AbstractBufferAttribute> _attributes = new List<AbstractBufferAttribute>();

            public void Add<TBufferAttribute>(TBufferAttribute attribute) where TBufferAttribute : AbstractBufferAttribute
            {
                _attributes.Add(attribute);
            }

            public void Enable(Shader shader)
            {
                var offset = 0;

                foreach (var attribute in _attributes)
                {
                    offset += 3 * sizeof(float);
                    
                    var location = shader.GetAttribLocation(attribute.Name);

                    GL.EnableVertexAttribArray(location);
                    GL.VertexAttribPointer(location, attribute.Count, VertexAttribPointerType.Float, false, attribute.Stride, offset);
                }
            }

            public void Disable(Shader shader)
            {
                foreach (var attribute in _attributes)
                {
                    var location = shader.GetAttribLocation(attribute.Name);

                    GL.DisableVertexAttribArray(location);
                }
            }
        }

        public abstract class AbstractBufferAttribute
        {
            public int Size;
            public string Name;
            public int Count;
            public int Stride;
        }

        public class Float32BufferAttribute : AbstractBufferAttribute
        {
            public Float32BufferAttribute(string name, int count)
            {
                Size = sizeof(float);
                Name = name;
                Count = count;
                Stride = Size * Count;
            }
        }

        internal class LineGeometry
        {
            int _vertexCount;

            VertexBufferObject _vertexbufferObject;
            Shader _shader;

            internal LineGeometry(float size, Shader shader)
                : this(size, Math.Max(1, (int)size), shader)
            {

            }

            internal LineGeometry(float size, int divisions, Shader shader)
                : this(size, divisions, Color4.White, shader)
            {

            }

            internal LineGeometry(float size, int divisions, Color4 color, Shader shader)
            {
                _shader = shader;

                _vertexCount = (divisions + 1) * 4;

                var lineVertices = new float[_vertexCount * 6];    // 6 floats per vertex (x,y,z,r,g,b)

                var totalVertexBytes = _vertexCount * 6 * sizeof(float);   // a vertex contains 6 float elements (x,y,z,r,g,b)

                _vertexbufferObject = new VertexBufferObject(totalVertexBytes);

                GL.BindVertexArray(_vertexbufferObject.Handle);

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

                var center = divisions / 2;
                var step = size / divisions;
                var halfSize = size / 2;

                var j = 0;
                var k = -halfSize;

                for (var i = 0; i <= divisions; i++, k += step)
                {
                    lineVertices[j++] = -halfSize;
                    lineVertices[j++] = 0;
                    lineVertices[j++] = k;
                    // color
                    lineVertices[j++] = color.R;
                    lineVertices[j++] = color.G;
                    lineVertices[j++] = color.B;

                    lineVertices[j++] = halfSize;
                    lineVertices[j++] = 0;
                    lineVertices[j++] = k;
                    // color
                    lineVertices[j++] = color.R;
                    lineVertices[j++] = color.G;
                    lineVertices[j++] = color.B;


                    lineVertices[j++] = k;
                    lineVertices[j++] = 0;
                    lineVertices[j++] = -halfSize;
                    // color
                    lineVertices[j++] = color.R;
                    lineVertices[j++] = color.G;
                    lineVertices[j++] = color.B;

                    lineVertices[j++] = k;
                    lineVertices[j++] = 0;
                    lineVertices[j++] = halfSize;
                    // color
                    lineVertices[j++] = color.R;
                    lineVertices[j++] = color.G;
                    lineVertices[j++] = color.B;
                }

                _vertexbufferObject.Write(ref lineVertices);
            }

            internal void Clear()
            {
                _vertexbufferObject.Clear();
            }

            internal void Bind()
            {
                GL.BindVertexArray(_vertexbufferObject.Handle);
            }
        }

        internal class QuadGeometry
        {
            int _vertexCount;

            VertexBufferObject _vertexbufferObject;
            Shader _shader;

            public VertexBufferAttributes Attributes { get { return _vertexbufferObject.Attributes; } }

            internal QuadGeometry(float size, Shader shader)
                : this(size, size, shader)
            {

            }

            internal QuadGeometry(float size, Color4 color, Shader shader)
                : this(size, size, color, shader)
            {

            }
            internal QuadGeometry(float width, float height, Shader shader)
                : this(width, height, Color4.White, shader)
            {
            }

            internal QuadGeometry(float width, float height, Color4 color, Shader shader)
            {
                _shader = shader;

                float hw = width * 0.5f;
                float hh = height * 0.5f;


                var stride = 8 * sizeof(float);
                var offset = 0;


                var totalVertexBytes = 4 * 8 * sizeof(float);   // 4 vertices containing 8 floats (xyz/rgb/uv)
                var totalIndexBytes = 2 * 3 * sizeof(uint);     // 2 triangles containing 3 uint indices

                _vertexbufferObject = new VertexBufferObject(totalVertexBytes, totalIndexBytes);

                _vertexbufferObject.Attributes.Add(new Float32BufferAttribute("i_pos", 3));
                _vertexbufferObject.Attributes.Add(new Float32BufferAttribute("i_rgb", 3));
                _vertexbufferObject.Attributes.Add(new Float32BufferAttribute("i_uv0", 2));

                //var xyzLocation = _shader.GetAttribLocation("i_pos");
                //GL.EnableVertexAttribArray(xyzLocation);
                //GL.VertexAttribPointer(xyzLocation, 3, VertexAttribPointerType.Float, false, stride, offset);

                //offset += 3 * sizeof(float);
                //var rgbLocation = _shader.GetAttribLocation("i_rgb");
                //GL.EnableVertexAttribArray(rgbLocation);
                //GL.VertexAttribPointer(rgbLocation, 3, VertexAttribPointerType.Float, false, stride, offset);

                //offset += 3 * sizeof(float);
                //var uvLocation = _shader.GetAttribLocation("i_uv0");
                //GL.EnableVertexAttribArray(uvLocation);
                //GL.VertexAttribPointer(uvLocation, 2, VertexAttribPointerType.Float, false, stride, offset);

                float[] vertices =
                [
                     // Position    Colors                      Texture coordinates
                     hw, hh, 0.0f,  color.R, color.G, color.B,  1.0f, 1.0f, // top right
                     hw, -hh, 0.0f, color.R, color.G, color.B,  1.0f, 0.0f, // bottom right
                    -hw, -hh, 0.0f, color.R, color.G, color.B,  0.0f, 0.0f, // bottom left
                    -hw, hh, 0.0f,  color.R, color.G, color.B,  0.0f, 1.0f  // top left
                ];

                uint[] indices = 
                [
                    0, 1, 3,
                    1, 2, 3
                ];

                _vertexbufferObject.Write(ref vertices, ref indices);
            }

            internal void Clear()
            {
                _vertexbufferObject.Clear();
            }

            internal void Bind()
            {
                GL.BindVertexArray(_vertexbufferObject.Handle);
            }
        }

        internal class BoxGeometry
        {
            int _vertexCount;

            VertexBufferObject _vertexbufferObject;
            Shader _shader;

            internal BoxGeometry(Shader shader, float size = 1.0f)
                : this(shader, size, size, size)
            {

            }

            internal BoxGeometry(Shader shader, float width = 1.0f, float height = 1.0f, float depth = 1.0f)
                : this(shader, width, height, depth, 1, 1, 1)
            {

            }

            internal BoxGeometry(Shader shader, float width = 1.0f, float height = 1.0f, float depth = 1.0f, int widthSegments = 1, int heightSegments = 1, int depthSegments = 1)
            {
                _shader = shader;

                widthSegments = (int)MathF.Floor(widthSegments);
                heightSegments = (int)MathF.Floor(heightSegments);
                depthSegments = (int)MathF.Floor(depthSegments);

                var totalVertexBytes = 1024 * 1024 * 8 * sizeof(uint);
                var totalIndexBytes = 1024 * 1024 * sizeof(uint);

                int vertexBufferIndex = 0;
                int indexBufferIndex = 0;
                int numberOfVertices = ((widthSegments + 1) * (widthSegments + 1)) + ((heightSegments + 1) * (heightSegments + 1)) + ((depthSegments + 1) * (depthSegments + 1));
                int numberOfIndices = numberOfVertices;

                var vertices = new float[numberOfVertices];
                var indices = new uint[numberOfIndices];

                _vertexbufferObject = new VertexBufferObject(totalVertexBytes, totalIndexBytes);

                GL.BindVertexArray(_vertexbufferObject.Handle);

                var elements = 8;
                var stride = elements * sizeof(float);
                var offset = 0;

                var xyzLocation = _shader.GetAttribLocation("i_pos");
                GL.EnableVertexAttribArray(xyzLocation);
                GL.VertexAttribPointer(xyzLocation, 3, VertexAttribPointerType.Float, false, stride, offset);

                offset += 3 * sizeof(float);
                var norLocation = _shader.GetAttribLocation("i_nor");
                GL.EnableVertexAttribArray(norLocation);
                GL.VertexAttribPointer(norLocation, 3, VertexAttribPointerType.Float, false, stride, offset);

                offset += 3 * sizeof(float);
                var rgbLocation = _shader.GetAttribLocation("i_rgb");
                GL.EnableVertexAttribArray(rgbLocation);
                GL.VertexAttribPointer(rgbLocation, 3, VertexAttribPointerType.Float, false, stride, offset);

                // build each side of the box geometry

                BuildPlane(ref vertexBufferIndex, ref indexBufferIndex, numberOfVertices, numberOfIndices, ref vertices, ref indices, 2, 1, 0, -1, -1, depth, height, width, depthSegments, heightSegments, 0); // px
                BuildPlane(ref vertexBufferIndex, ref indexBufferIndex, numberOfVertices, numberOfIndices, ref vertices, ref indices, 2, 1, 0, 1, -1, depth, height, -width, depthSegments, heightSegments, 1); // nx
                BuildPlane(ref vertexBufferIndex, ref indexBufferIndex, numberOfVertices, numberOfIndices, ref vertices, ref indices, 0, 2, 1, 1, 1, width, depth, height, widthSegments, depthSegments, 2); // py
                BuildPlane(ref vertexBufferIndex, ref indexBufferIndex, numberOfVertices, numberOfIndices, ref vertices, ref indices, 0, 2, 1, 1, -1, width, depth, -height, widthSegments, depthSegments, 3); // ny
                BuildPlane(ref vertexBufferIndex, ref indexBufferIndex, numberOfVertices, numberOfIndices, ref vertices, ref indices, 0, 1, 2, 1, -1, width, height, depth, widthSegments, heightSegments, 4); // pz
                BuildPlane(ref vertexBufferIndex, ref indexBufferIndex, numberOfVertices, numberOfIndices, ref vertices, ref indices, 0, 1, 2, -1, -1, width, height, -depth, widthSegments, heightSegments, 5); // nz

                _vertexbufferObject.Write(ref vertices, ref indices);
            }

            private void BuildPlane(
                ref int vertexBufferIndex,
                ref int indexBufferIndex,
                int numberOfVertices,
                int numberOfIndices,
                ref float[] vertices,
                ref uint[] indices,
                int u, int v, int w,
                float udir, float vdir,
                float width, float height, float depth,
                int gridX, int gridY,
                int materialIndex)
            {

                var segmentWidth = width / gridX;
                var segmentHeight = height / gridY;

                var widthHalf = width / 2;
                var heightHalf = height / 2;
                var depthHalf = depth / 2;

                var gridX1 = gridX + 1;
                var gridY1 = gridY + 1;

                var vertexCounter = 0;
                var groupCount = 0;

                var vector = new float[3];

                // generate vertices, normals and uvs

                for (var iy = 0; iy < gridY1; iy++)
                {

                    var y = iy * segmentHeight - heightHalf;

                    for (var ix = 0; ix < gridX1; ix++)
                    {

                        var x = ix * segmentWidth - widthHalf;

                        // set values to correct vector component

                        vector[u] = x * udir;
                        vector[v] = y * vdir;
                        vector[w] = depthHalf;

                        // now apply vector to vertex buffer

                        //vertices.push(vector.x, vector.y, vector.z);
                        vertices[vertexBufferIndex++] = vector[0];
                        vertices[vertexBufferIndex++] = vector[1];
                        vertices[vertexBufferIndex++] = vector[2];

                        // set values to correct vector component

                        vector[u] = 0;
                        vector[v] = 0;
                        vector[w] = depth > 0 ? 1 : -1;

                        // now apply vector to normal buffer

                        //normals.push(vector.x, vector.y, vector.z);
                        vertices[vertexBufferIndex++] = vector[0];
                        vertices[vertexBufferIndex++] = vector[1];
                        vertices[vertexBufferIndex++] = vector[2];

                        // uvs

                        //uvs.push(ix / gridX);
                        //uvs.push(1 - (iy / gridY));
                        vertices[vertexBufferIndex++] = ix / gridX;
                        vertices[vertexBufferIndex++] = 1 - (iy / gridY);

                        // counters

                        vertexCounter += 1;

                    }

                }

                // indices

                // 1. you need three indices to draw a single face
                // 2. a single segment consists of two faces
                // 3. so we need to generate six (2*3) indices per segment

                for (uint iy = 0; iy < gridY; iy++)
                {

                    for (uint ix = 0; ix < gridX; ix++)
                    {

                        var a = numberOfVertices + ix + gridX1 * iy;
                        var b = numberOfVertices + ix + gridX1 * (iy + 1);
                        var c = numberOfVertices + (ix + 1) + gridX1 * (iy + 1);
                        var d = numberOfVertices + (ix + 1) + gridX1 * iy;

                        // faces

                        //indices.push(a, b, d);
                        //indices.push(b, c, d);
                        indices[indexBufferIndex++] = (uint)a;
                        indices[indexBufferIndex++] = (uint)b;
                        indices[indexBufferIndex++] = (uint)d;

                        indices[indexBufferIndex++] = (uint)b;
                        indices[indexBufferIndex++] = (uint)c;
                        indices[indexBufferIndex++] = (uint)d;

                        // increase counter

                        groupCount += 6;

                    }

                }
            }

            internal void Clear()
            {
                _vertexbufferObject.Clear();
            }

            internal void Bind()
            {
                GL.BindVertexArray(_vertexbufferObject.Handle);
            }
        }
    }
}