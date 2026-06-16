
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection.Metadata;
using System.Runtime.Versioning;
using System.Xml.Linq;
using static Skrbl.Sketch008BoxGeometry;

namespace Skrbl
{
    public class Sketch009RenderToTexture : GameWindow
    {

        private IRandomGenerator _defaultRandomGenerator;
        public IRandomGenerator Random { get { return _defaultRandomGenerator; } }

        FrameBufferObject _framebufferObject;

        VertexArrayObject _vertexArrayObject0;
        VertexArrayObject _vertexArrayObject1;
        VertexArrayObject _vertexArrayObject2;

        private Shader _shader0;
        //private Shader _shader1;
        //private Shader _shader2;

        private Texture _texture0;
        private Texture _texture1;
        private Texture _texture2;

        private Camera01 _camera;

        private bool _mouseButtonDown = false;

        private Vector2 _lastPos;

        Color4 _backgroundColor = new(00.2f, 0.3f, 0.3f, 1.0f);

        private int TotalVertexBytes;
        private int TotalIndexBytes;

        private float _frameBufferScale = 1.0f;

        public int FrameCount { get; private set; }

        public double ElapsedTime { get; private set; }
        public double TotalTime { get; private set; }

        public int FramesPerSecond { get; private set; }

        private long _lastFramesPerSecondTimeStamp { get; set; }

        private readonly Stopwatch _timeStopWatch = new Stopwatch();

        public Sketch009RenderToTexture(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
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

            _framebufferObject = new FrameBufferObject((int)(ClientSize.X * _frameBufferScale), (int)(ClientSize.Y * _frameBufferScale));

            // ------------------------------------------------------------------------------------

            var quadcount = 1024;
            var elements = 0;
            var stride = 0;
            var offset = 0;

            // ------------------------------------------------------------------------------------

            _shader0 = new Shader("Sketch009RenderToTexture");
            //_shader1 = new Shader("Sketch009RenderToTexture");
            //_shader2 = new Shader("Sketch009RenderToTexture");

            // ------------------------------------------------------------------------------------

            TotalVertexBytes = sizeof(float) * 5 * 4 * quadcount;   // 1024 quads containing 4 vertices containing 5 float elements (x,y,z,u,v)
            TotalIndexBytes = sizeof(uint) * 6 * quadcount;         // 1024 quads containing 6 uint indices (2 triangles per quad)

            _vertexArrayObject0 = new VertexArrayObject(TotalVertexBytes, TotalIndexBytes);
            _vertexArrayObject0.Attributes.Add(new Float32BufferAttribute("i_pos", 3));
            _vertexArrayObject0.Attributes.Add(new Float32BufferAttribute("i_uv0", 2));
            _vertexArrayObject0.Attributes.Enable(_shader0);

            ClearAndWriteRandomizedVertices(_vertexArrayObject0);

            // ------------------------------------------------------------------------------------

            _vertexArrayObject1 = new VertexArrayObject(TotalVertexBytes, TotalIndexBytes);
            _vertexArrayObject1.Attributes.Add(new Float32BufferAttribute("i_pos", 3));
            _vertexArrayObject1.Attributes.Add(new Float32BufferAttribute("i_uv0", 2));
            _vertexArrayObject1.Attributes.Enable(_shader0);

            ClearAndWriteRandomizedVertices(_vertexArrayObject1);

            // ------------------------------------------------------------------------------------

            _vertexArrayObject2 = new VertexArrayObject(TotalVertexBytes, TotalIndexBytes);
            _vertexArrayObject2.Attributes.Add(new Float32BufferAttribute("i_pos", 3));
            _vertexArrayObject2.Attributes.Add(new Float32BufferAttribute("i_uv0", 2));
            _vertexArrayObject2.Attributes.Enable(_shader0);

            ClearAndWriteRandomizedVertices(_vertexArrayObject2);

            // ------------------------------------------------------------------------------------

            _texture0 = Texture.LoadFromFile(".resources/.textures./ca5266d55389fd7b73173b6a74f57017d0af95d007bec063f652dfc96b4014c1.jpg");
            _texture1 = Texture.LoadFromFile(".resources/.textures./UpsideDown_ROW12246886594_1920x1080.jpg");
            _texture2 = Texture.LoadFromFile(".resources/.textures./MatterhornVideo_ROW11549990163_1920x1080-v.jpg");

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

            // generates an error
            //GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _framebufferObject.Handle);

            GL.ClearColor(new Color4(251.0f / 255.0f, 161.0f / 255.0f, 122.0f / 255.0f, 0.0f));

            GL.Viewport(0, 0, _framebufferObject.Width, _framebufferObject.Height);

            _camera.AspectRatio = _framebufferObject.Width / (float)_framebufferObject.Height;
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // ------------------------------------------------------------------------------------

            var model = Matrix4.Identity;

            // ------------------------------------------------------------------------------------

            _vertexArrayObject0.Bind();

            _texture0.Use(TextureUnit.Texture0);

            _shader0.Use();

            _shader0.SetMatrix4("u_model", model);
            _shader0.SetMatrix4("u_view", _camera.GetViewMatrix());
            _shader0.SetMatrix4("u_projection", _camera.GetProjectionMatrix());
            _shader0.SetInt("u_tex0", 0);

            _vertexArrayObject0.Draw();

            // ------------------------------------------------------------------------------------

            _vertexArrayObject1.Bind();

            _texture1.Use(TextureUnit.Texture0);

            _vertexArrayObject1.Draw();

            // ------------------------------------------------------------------------------------

            //GL.DrawBuffer(DrawBufferMode.Back);

            // set the default framebuffer as the draw or render target
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);

            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

            _vertexArrayObject2.Bind();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _framebufferObject.ColorBufferHandle);

            _vertexArrayObject2.Draw();

            // ------------------------------------------------------------------------------------

            //// set the default framebuffer as the draw or render target
            //GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            //// set the offscreen framebuffer as the read source
            //GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _framebufferObject.Handle);
            //// https://registry.khronos.org/OpenGL-Refpages/gl4/html/glBlitFramebuffer.xhtml
            //GL.BlitFramebuffer(
            //    0, 0, _framebufferObject.Width, _framebufferObject.Height,
            //    0, 0, ClientSize.X, ClientSize.Y,
            //    ClearBufferMask.ColorBufferBit,
            //    BlitFramebufferFilter.Linear);

            // bind the default framebuffer
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);

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
                ClearAndWriteRandomizedVertices(_vertexArrayObject0);
                ClearAndWriteRandomizedVertices(_vertexArrayObject1);
                ClearAndWriteRandomizedVertices(_vertexArrayObject2);
            }

        }

        private void ClearAndWriteRandomizedVertices(VertexArrayObject vertexArrayObject)
        {
            var top = Random.Value(1, ClientSize.Y / 2);
            var left = Random.Value(-1, -ClientSize.X / 2);
            var right = Random.Value(1, ClientSize.X / 2);
            var bottom = Random.Value(-1, -ClientSize.Y / 2);

            float[] _vertices = 
            [
                // Position                   Texture coordinates
                right,  top,        0.0f,     1.0f,     1.0f, // top right
                right,  bottom,     0.0f,     1.0f,     0.0f, // bottom right
                left,   bottom,     0.0f,     0.0f,     0.0f, // bottom left
                left,   top,        0.0f,     0.0f,     1.0f  // top left
            ];

            uint[] _indices =
            {
                0, 1, 3,
                1, 2, 3
            };

            vertexArrayObject.Clear();
            vertexArrayObject.Write(ref _vertices, ref _indices);

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

        class FrameBufferObject
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

        class VertexArrayObject
        {
            public int Handle;
            public int VertexArrayHandle;
            public int IndexArrayHandle;

            public int UsedVertexBytes;
            public int TotalVertexBytes;
            public int TotalIndexBytes;
            public int UsedIndexBytes;
            public int TotalIndices;

            public VertexBufferAttributes Attributes = new VertexBufferAttributes();

            public VertexArrayObject(int totalVertexBytes, int totalIndexBytes = 0)
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

            internal void Bind()
            {
                GL.BindVertexArray(Handle);
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
                TotalIndices = 0;
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
                TotalIndices += indexdata.Length;
            }

            public void Draw()
            {
                GL.BindVertexArray(Handle);
                GL.DrawElements(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, TotalIndices, DrawElementsType.UnsignedInt, 0);
            }
        }

        public class VertexBufferAttributes
        {
            List<AbstractBufferAttribute> _attributes = new List<AbstractBufferAttribute>();

            public int Stride { get { return _attributes.Sum(o => o.Size); } }

            public void Add<TBufferAttribute>(TBufferAttribute attribute) where TBufferAttribute : AbstractBufferAttribute
            {
                _attributes.Add(attribute);
            }

            public void Enable(Shader shader)
            {
                var offset = 0;

                foreach (var attribute in _attributes)
                {
                    var location = shader.GetAttribLocation(attribute.Name);

                    GL.EnableVertexAttribArray(location);
                    GL.VertexAttribPointer(location, attribute.Count, VertexAttribPointerType.Float, false, Stride, offset);

                    offset += attribute.Size;
                }
            }

            //public void Disable(Shader shader)
            //{
            //    foreach (var attribute in _attributes)
            //    {
            //        var location = shader.GetAttribLocation(attribute.Name);

            //        GL.DisableVertexAttribArray(location);
            //    }
            //}
        }

        public abstract class AbstractBufferAttribute
        {
            public int Size;
            public string Name;
            public int Count;
        }

        public class Float32BufferAttribute : AbstractBufferAttribute
        {
            public Float32BufferAttribute(string name, int count)
            {
                Name = name;
                Count = count;
                Size = sizeof(float) * Count;
            }
        }
    }
}