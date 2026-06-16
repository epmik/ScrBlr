
using Microsoft.Extensions.Configuration;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Skrbl;
using System.Runtime.Versioning;

namespace OpenGlTutorialOrg
{
    public class Tutorial14WindowExtented04 : IDisposable
    {
        GameWindow _internalWindow;

        bool CaptureInput = false;
        Vector2 CapturedMousePosition;
        protected string ResourceDirectory = string.Empty;
        bool _renderToFullScreenQuad = false;
        double TotalTime;
        int FrameCount;

        int _suzanneVertexArray;
        int _suzanneRenderPass_Program;
        int _suzanneVertexbuffer;
        int _suzanneUvBuffer;
        int _suzanneNormalBuffer;
        int _suzanneElementBuffer;
        int _suzannePass_Program_ModelViewProjectionMatrixLocation;
        int _suzannePass_Program_ViewMatrixLocation;
        int _suzannePass_Program_ModelMatrixLocation;
        int _suzannePass_Texture;
        int _suzannePass_Program_TextureLocation;
        int _suzanneIndexCount;
        int _suzannePass_Program_LightLocation;
        int _suzannePass_FrameBuffer;
        int _suzanne_FrameBuffer_Texture;
        int _suzanne_FrameBuffer_DepthBuffer;

        int _desaturatePass_frameBuffer;
        int _desaturatePass_FrameBufferTexture;
        int _desaturatePass_FrameBufferDepthBuffer;

        int _fullscreenquad_vao;
        int _fullscreenquad_vertexbuffer, _fullscreenquad_uvbuffer;
        int _fullscreenquad_programID;
        int _fullscreenquad_programTexLocation;

        //int _quad_vao;
        //int _quad_vertexbuffer;
        //int _quad_uvbuffer;
        //int _quad_vertexbuffer_index;

        int _box_vertexbuffer;
        int _box_uvbuffer;
        int _box_programID;
        int _box_matrixID;
        int _box_verticeCount = 6;

        int _desaturatePass_Program;
        int _desaturatePass_ProgramSourceLocation;

        //int _filmicPass_Framebuffer;
        //int _filmicPass_FramebufferTexture;
        //int _filmicPass_FramebufferDepthBuffer;
        //int _filmicPass_Program;
        //int _filmicPass_Program_SourceLocation;
        //int _filmicPass_Program_TimeLocation;

        int _horizontalBlurPass_Framebuffer;
        int _horizontalBlurPass_FramebufferTexture;
        int _horizontalBlurPass_FramebufferDepthBuffer;
        int _horizontalBlurPass_Program;
        int _horizontalBlurPass_Program_SourceLocation;
        int _horizontalBlurPass_Program_ResolutionLocation;
        int _horizontalBlurPass_Program_RadiusLocation;
        int _horizontalBlurPass_Program_ModelViewProjectionMatrixMatrixLocation;

        int _windowWidth;
        int _windowHeight;

        int _frustumWidth = 100;
        int _frustumHeight = 100;

        float _frameBufferScale = 2.0f;

        int _frameBufferWidth;
        int _frameBufferHeight;

        public Matrix4 CameraViewMatrix = Matrix4.Identity;
        public Matrix4 CameraProjectionMatrix = Matrix4.Identity;

        public ProjectionType ProjectionType = ProjectionType.Perspective;

        // Initial position : on +Z
        private Vector3 _cameraPosition;
        // Initial horizontal angle : toward -Z
        float _cameraHorizontalAngle = 3.14f;
        // Initial vertical angle : none
        float _cameraVerticalAngle = 0.0f;

        float _cameraForwardSpeed = DefaultCameraForwardSpeed;
        float _cameraRotationSpeed = 0.005f;

        public float CameraAspectRatio;
        public float CameraNear = DefaultCameraNear;
        public float CameraFar = DefaultCameraFar;

        private const float DefaultCameraNear = 0.01f;
        private const float DefaultCameraFar = 10000.0f;

        private const float DefaultCameraForwardSpeed = 3.0f; // 3 units / second

        private float _cameraFovInRadians = MathHelper.DegreesToRadians(66.0f);

        internal class SketchSettings
        {
            public Version OpenGlVersion { get; set; } = new Version(4, 6);
            public float FrustumWidth { get; set; } = 1024.0f;
            public float FrustumHeight { get; set; } = 1024.0f;
            public float FrameBufferScale { get; set; } = 4.0f;
        }

        public float CameraFovInDegrees
        {
            get
            {
                return MathHelper.RadiansToDegrees(_cameraFovInRadians);
            }
            set
            {
                _cameraFovInRadians = MathHelper.DegreesToRadians(value);
            }
        }

        // A simple constructor to let us set properties like window size, title, FPS, etc. on the window.
        public Tutorial14WindowExtented04(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
        {
            ResourceDirectory = "opengl-tutorial.org/tutorial14_render_to_texture/";

            CalculateWindowSize();

            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile(ResourcePath($"{GetType().Name}.json"), optional: false, reloadOnChange: true)
                .Build();

            // Get a configuration section

            var sketchSettings = new SketchSettings();

            configuration.GetSection("Sketch").Bind(sketchSettings);

            gameWindowSettings = gameWindowSettings != null ? gameWindowSettings : GameWindowSettings.Default;
            nativeWindowSettings = nativeWindowSettings != null ? nativeWindowSettings : new NativeWindowSettings()
            {
                APIVersion = new System.Version(4, 6),
                Title = "Tutorial 14 - Render To Texture - Extented 4",
                Flags = ContextFlags.ForwardCompatible | ContextFlags.Debug,     // needed to run on macos
                NumberOfSamples = 4,
                DepthBits = 32,
                StencilBits = 0,
            };

            nativeWindowSettings.ClientSize = (_windowWidth, _windowHeight);

            _internalWindow = new GameWindow(gameWindowSettings, nativeWindowSettings);

            _internalWindow.Load += LoadInternal;
            _internalWindow.Unload += UnLoadInternal;
            _internalWindow.UpdateFrame += UpdateFrameInternal;
            _internalWindow.RenderFrame += RenderFrameInternal;
            _internalWindow.Resize += ResizeInternal;

            //_internalWindow.MouseWheel += MouseWheelInternal;
            //_internalWindow.MouseDown += MouseDownInternal;
            //_internalWindow.MouseUp += MouseUpInternal;
            //_internalWindow.MouseEnter += MouseEnterInternal;
            //_internalWindow.MouseLeave += MouseLeaveInternal;
            //_internalWindow.MouseMove += MouseMoveInternal;

            //_internalWindow.KeyDown += KeyDownInternal;
            //_internalWindow.KeyUp += KeyUpInternal;

            _internalWindow.CenterWindow();
            //_internalWindow.Location = new Vector2i(_internalWindow.Location.X, 30);

            //if (CaptureMouse)
            //{
            //    _internalWindow.CursorState = CursorState.Grabbed;
            //}

            if (CaptureInput)
            {
                _internalWindow.MousePosition = new Vector2(_internalWindow.ClientSize.X / 2, _internalWindow.ClientSize.Y / 2);
            }

            CameraAspectRatio = (float)_internalWindow.ClientSize.X / (float)_internalWindow.ClientSize.Y;

            var z = _frustumWidth / 2 / MathF.Tan(_cameraFovInRadians * 0.5f);

            _cameraPosition = (0, 0, z);

            //float defaultWidth = DefaultCameraNear * MathF.Tan(hfov) * CameraAspectRatio;

            //float scale = _frustumWidth / defaultWidth;

            //var n = DefaultCameraNear * scale;
            //var f = DefaultCameraFar * scale;
            //var s = DefaultCameraForwardSpeed * scale;
        }

        private void LoadInternal()
        {
            Diagnostics.EnableOpenGlDebugMessages();

            // retrieve the framebuffer size
            // if the screen has a resolution of 1024x768 pixels then we can expect the framebuffer to have the same size but on
            // MacOS X with a retina screen it'll be 1024*2 and 768*2
            // Needs 'unsafe' compiler flag
            //GLFW.GetFramebufferSize(WindowPtr, out _frameBufferWidth, out _frameBufferHeight);

            _frameBufferWidth = (int)(_windowWidth * _frameBufferScale);
            _frameBufferHeight = (int)(_windowHeight * _frameBufferScale);

            // Dark blue background
            GL.ClearColor(0.0f, 0.0f, 0.4f, 0.0f);

            // Enable depth test
            GL.Enable(EnableCap.DepthTest);
            // Accept fragment if it is closer to the camera than the former one
            GL.DepthFunc(DepthFunction.Less);

            // Cull triangles which normal is not towards the camera
            GL.Enable(EnableCap.CullFace);

            // ------------------------------------------------------------------------------------
            // suzanne render pass

            _suzanneVertexArray = GL.GenVertexArray();
            GL.BindVertexArray(_suzanneVertexArray);

            // Create and compile our GLSL program from the shaders
            _suzanneRenderPass_Program = Shaders.Load(
                ResourcePath("StandardShadingRTT.vertexshader"),
                ResourcePath("StandardShadingRTT.fragmentshader"));

            // Get a handle for our "MVP" uniform
            _suzannePass_Program_ModelViewProjectionMatrixLocation = GL.GetUniformLocation(_suzanneRenderPass_Program, "MVP");
            _suzannePass_Program_ViewMatrixLocation = GL.GetUniformLocation(_suzanneRenderPass_Program, "V");
            _suzannePass_Program_ModelMatrixLocation = GL.GetUniformLocation(_suzanneRenderPass_Program, "M");

            // Load the texture
            _suzannePass_Texture = Textures.Load(ResourcePath("uvmap.png"));

            // Get a handle for our "myTextureSampler" uniform
            _suzannePass_Program_TextureLocation = GL.GetUniformLocation(_suzanneRenderPass_Program, "myTextureSampler");

            // Read our .obj file
            ObjLoader.Load(ResourcePath("suzanne.obj"), out var vertices, out var uvs, out var normals);

            VboIndexer.IndexTriangles(
                vertices, uvs, normals,
                out ushort[] indices,
                out Vector3[] indexed_vertices,
                out Vector2[] indexed_uvs,
                out Vector3[] indexed_normals);

            _suzanneIndexCount = indices.Length;
            // Load it into a VBO

            _suzanneVertexbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _suzanneVertexbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, indexed_vertices.Length * 3 * sizeof(float), indexed_vertices, BufferUsageHint.StaticDraw);

            _suzanneUvBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _suzanneUvBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, indexed_uvs.Length * 2 * sizeof(float), indexed_uvs, BufferUsageHint.StaticDraw);

            _suzanneNormalBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _suzanneNormalBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, indexed_normals.Length * 3 * sizeof(float), indexed_normals, BufferUsageHint.StaticDraw);

            // Generate a buffer for the indices as well
            _suzanneElementBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _suzanneElementBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(ushort), indices, BufferUsageHint.StaticDraw);

            // Get a handle for our "LightPosition" uniform
            GL.UseProgram(_suzanneRenderPass_Program);
            _suzannePass_Program_LightLocation = GL.GetUniformLocation(_suzanneRenderPass_Program, "LightPosition_worldspace");

            // The framebuffer, which regroups 0, 1, or more textures, and 0 or 1 depth buffer.
            _suzannePass_FrameBuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _suzannePass_FrameBuffer);

            // The texture we're going to render to
            _suzanne_FrameBuffer_Texture = GL.GenTexture();

            // "Bind" the newly created texture : all future texture functions will modify this texture
            GL.BindTexture(TextureTarget.Texture2D, _suzanne_FrameBuffer_Texture);

            // Give an empty image to OpenGL ( the last "0" means "empty" )
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, _frameBufferWidth, _frameBufferHeight, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);

            // Poor filtering
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            // The depth buffer
            _suzanne_FrameBuffer_DepthBuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _suzanne_FrameBuffer_DepthBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, _frameBufferWidth, _frameBufferHeight);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _suzanne_FrameBuffer_DepthBuffer);

            //// Alternative : Depth texture. Slower, but you can sample it later in your shader
            ////GLuint depthTexture;
            ////glGenTextures(1, &depthTexture);
            ////glBindTexture(GL_TEXTURE_2D, depthTexture);
            ////glTexImage2D(GL_TEXTURE_2D, 0,GL_DEPTH_COMPONENT24, _frameBufferWidth, _frameBufferHeight, 0,GL_DEPTH_COMPONENT, GL_FLOAT, 0);
            ////glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
            ////glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST); 
            ////glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
            ////glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
            //depthTexture = GL.GenTexture();
            //GL.BindTexture(TextureTarget.Texture2D, depthTexture);
            //GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent24, windowWidth, windowHeight, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            // Set "renderedTexture" as our colour attachement #0
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, _suzanne_FrameBuffer_Texture, 0);

            //// Depth texture alternative : 
            ////glFramebufferTexture(GL_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, depthTexture, 0);
            //GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, depthTexture, 0);


            // Set the list of draw buffers.
            //GLenum DrawBuffers[1] = { GL_COLOR_ATTACHMENT0 };
            //glDrawBuffers(1, DrawBuffers); // "1" is the size of DrawBuffers
            //GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                throw new Exception("Framebuffer not complete");

            // end suzanne render pass
            // ------------------------------------------------------------------------------------

            // ------------------------------------------------------------------------------------
            // desaturate render pass

            _desaturatePass_frameBuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _desaturatePass_frameBuffer);

            _desaturatePass_FrameBufferTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _desaturatePass_FrameBufferTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, _frameBufferWidth, _frameBufferHeight, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            _desaturatePass_FrameBufferDepthBuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _desaturatePass_FrameBufferDepthBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, _frameBufferWidth, _frameBufferHeight);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _desaturatePass_FrameBufferDepthBuffer);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, _desaturatePass_FrameBufferTexture, 0);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                throw new Exception("Framebuffer not complete");

            _desaturatePass_Program = Shaders.Load(
                ResourcePath("Desaturate.vertexshader"),
                ResourcePath("Desaturate.fragmentshader"));
            _desaturatePass_ProgramSourceLocation = GL.GetUniformLocation(_desaturatePass_Program, "u_source");

            // end desaturate render pass
            // ------------------------------------------------------------------------------------

            // ------------------------------------------------------------------------------------
            // filmic render pass

            //_filmicPass_Framebuffer = GL.GenFramebuffer();
            //GL.BindFramebuffer(FramebufferTarget.Framebuffer, _filmicPass_Framebuffer);

            //_filmicPass_FramebufferTexture = GL.GenTexture();
            //GL.BindTexture(TextureTarget.Texture2D, _filmicPass_FramebufferTexture);
            //GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, _frameBufferWidth, _frameBufferHeight, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            //_filmicPass_FramebufferDepthBuffer = GL.GenRenderbuffer();
            //GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _filmicPass_FramebufferDepthBuffer);
            //GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, _frameBufferWidth, _frameBufferHeight);
            //GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _filmicPass_FramebufferDepthBuffer);

            //GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, _filmicPass_FramebufferTexture, 0);

            //if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            //    throw new Exception("Framebuffer not complete");

            //_filmicPass_Program = Shaders.Load(
            //    ResourcePath("Filmic.vert"),
            //    ResourcePath("Filmic.frag"));

            //_filmicPass_Program_SourceLocation = GL.GetUniformLocation(_filmicPass_Program, "u_source");
            ////_filmicPass_Program_ProjectionMatrixLocation = GL.GetUniformLocation(_filmicPass_Program, "projectionMatrix");
            ////_filmicPass_Program_ModelViewMatrixLocation = GL.GetUniformLocation(_filmicPass_Program, "modelViewMatrix");
            //_filmicPass_Program_TimeLocation = GL.GetUniformLocation(_filmicPass_Program, "u_time");
            ////_filmicPass_Program_DiffuseLocation = GL.GetUniformLocation(_filmicPass_Program, "u_diffuse");

            // end filmic render pass
            // ------------------------------------------------------------------------------------

            // ------------------------------------------------------------------------------------
            // horizontal blur render pass

            _horizontalBlurPass_Framebuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _horizontalBlurPass_Framebuffer);

            _horizontalBlurPass_FramebufferTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _horizontalBlurPass_FramebufferTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, _frameBufferWidth, _frameBufferHeight, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, _horizontalBlurPass_FramebufferTexture, 0);

            _horizontalBlurPass_FramebufferDepthBuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _horizontalBlurPass_FramebufferDepthBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, _frameBufferWidth, _frameBufferHeight);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _horizontalBlurPass_FramebufferDepthBuffer);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                throw new Exception("Framebuffer not complete");

            _horizontalBlurPass_Program = Shaders.Load(
                ResourcePath("HorizontalGaussianBlurShader.vert"),
                ResourcePath("HorizontalGaussianBlurShader.frag"));

            _horizontalBlurPass_Program_SourceLocation = GL.GetUniformLocation(_horizontalBlurPass_Program, "u_source");
            _horizontalBlurPass_Program_ResolutionLocation = GL.GetUniformLocation(_horizontalBlurPass_Program, "u_resolution");
            _horizontalBlurPass_Program_RadiusLocation = GL.GetUniformLocation(_horizontalBlurPass_Program, "u_radius");
            _horizontalBlurPass_Program_ModelViewProjectionMatrixMatrixLocation = GL.GetUniformLocation(_horizontalBlurPass_Program, "u_modelViewProjectionMatrix");

            // end horizontal blur render pass
            // ------------------------------------------------------------------------------------

            // ------------------------------------------------------------------------------------
            // box render pass

            _box_verticeCount = 36;

            float[] g_box_vertex_buffer_data = [
                // back
               1.0f, -1.0f,  -1.0f, // left bottom
               -1.0f,  1.0f,  -1.0f, // right top
               1.0f,  1.0f,  -1.0f, // left top
               1.0f, -1.0f,  -1.0f, // left bottom
               -1.0f, -1.0f,  -1.0f, // right bottom
               -1.0f,  1.0f,  -1.0f, // right top

                // front
               -1.0f, -1.0f,  1.0f, // left bottom
                1.0f,  1.0f,  1.0f, // right top
               -1.0f,  1.0f,  1.0f, // left top
               -1.0f, -1.0f,  1.0f, // left bottom
                1.0f, -1.0f,  1.0f, // right bottom
                1.0f,  1.0f,  1.0f, // right top

                // right side
                1.0f, -1.0f,  1.0f, // left bottom
                1.0f,  1.0f,  -1.0f, // right top
                1.0f,  1.0f,  1.0f, // left top
                1.0f, -1.0f,  1.0f, // left bottom
                1.0f, -1.0f,  -1.0f, // right bottom
                1.0f,  1.0f,  -1.0f, // right top

                // left side
                -1.0f, -1.0f,  -1.0f, // left bottom
                -1.0f,  1.0f,  1.0f, // right top
                -1.0f,  1.0f,  -1.0f, // left top
                -1.0f, -1.0f,  -1.0f, // left bottom
                -1.0f, -1.0f,  1.0f, // right bottom
                -1.0f,  1.0f,  1.0f, // right top

                // top
               -1.0f, 1.0f,  1.0f, // left bottom
                1.0f,  1.0f,  -1.0f, // right top
               -1.0f,  1.0f,  -1.0f, // left top
               -1.0f, 1.0f,  1.0f, // left bottom
                1.0f, 1.0f,  1.0f, // right bottom
                1.0f,  1.0f,  -1.0f, // right top

                // bottom
               -1.0f, -1.0f,  -1.0f, // left bottom
                1.0f,  -1.0f,  1.0f, // right top
               -1.0f,  -1.0f,  1.0f, // left top
               -1.0f, -1.0f,  -1.0f, // left bottom
                1.0f, -1.0f,  -1.0f, // right bottom
                1.0f,  -1.0f,  1.0f, // right top
            ];

            float[] g_box_uv_buffer_data = [

                0.0f,  0.0f, // left bottom
                1.0f,  1.0f, // right top
                0.0f,  1.0f, // left top
                0.0f,  0.0f, // left bottom
                1.0f,  0.0f, // right bottom
                1.0f,  1.0f, // right top

                0.0f,  0.0f, // left bottom
                1.0f,  1.0f, // right top
                0.0f,  1.0f, // left top
                0.0f,  0.0f, // left bottom
                1.0f,  0.0f, // right bottom
                1.0f,  1.0f, // right top

                0.0f,  0.0f, // left bottom
                1.0f,  1.0f, // right top
                0.0f,  1.0f, // left top
                0.0f,  0.0f, // left bottom
                1.0f,  0.0f, // right bottom
                1.0f,  1.0f, // right top

                0.0f,  0.0f, // left bottom
                1.0f,  1.0f, // right top
                0.0f,  1.0f, // left top
                0.0f,  0.0f, // left bottom
                1.0f,  0.0f, // right bottom
                1.0f,  1.0f, // right top

                0.0f,  0.0f, // left bottom
                1.0f,  1.0f, // right top
                0.0f,  1.0f, // left top
                0.0f,  0.0f, // left bottom
                1.0f,  0.0f, // right bottom
                1.0f,  1.0f, // right top

                0.0f,  0.0f, // left bottom
                1.0f,  1.0f, // right top
                0.0f,  1.0f, // left top
                0.0f,  0.0f, // left bottom
                1.0f,  0.0f, // right bottom
                1.0f,  1.0f, // right top
            ];

            _box_vertexbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _box_vertexbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, g_box_vertex_buffer_data.Length * sizeof(float), g_box_vertex_buffer_data, BufferUsageHint.StaticDraw);

            _box_uvbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _box_uvbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, g_box_uv_buffer_data.Length * sizeof(float), g_box_uv_buffer_data, BufferUsageHint.StaticDraw);

            _box_programID = Shaders.Load(
                ResourcePath("TransformVertexShader.vertexshader"),
                ResourcePath("Passthrough.fragmentshader"));

            _box_matrixID = GL.GetUniformLocation(_box_programID, "MVP");

            // end box render pass
            // ------------------------------------------------------------------------------------

            // ------------------------------------------------------------------------------------
            // fullscreen quad render pass

            // The fullscreen quad's FBO
            //float[] g_quad_vertex_buffer_data = [
            //    -1.0f, -1.0f, 0.0f,
            //     1.0f, -1.0f, 0.0f,
            //    -1.0f,  1.0f, 0.0f,
            //    -1.0f,  1.0f, 0.0f,
            //     1.0f, -1.0f, 0.0f,
            //     1.0f,  1.0f, 0.0f,
            //];

            //float[] g_quad_uv_buffer_data = [
            //     0.0f,  0.0f, //  + vec2(1,1)) / 2.0
            //     1.0f,  0.0f,
            //     0.0f,  1.0f,
            //     0.0f,  1.0f,
            //     1.0f,  0.0f,
            //     1.0f,  1.0f,
            //];

            //
            // see https://github.com/mrdoob/three.js/blob/master/examples/jsm/postprocessing/Pass.js
            // and
            // https://github.com/mrdoob/three.js/pull/21358
            //
            float[] g_quad_vertex_buffer_data = [
                -1,  3, 0,
                -1, -1, 0,
                 3, -1, 0,
            ];

            float[] g_quad_uv_buffer_data = [
                0, 2,
                0, 0,
                2, 0
            ];

            //GLuint quad_vertexbuffer;
            _fullscreenquad_vertexbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _fullscreenquad_vertexbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, g_quad_vertex_buffer_data.Length * sizeof(float), g_quad_vertex_buffer_data, BufferUsageHint.StaticDraw);

            _fullscreenquad_uvbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _fullscreenquad_uvbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, g_quad_uv_buffer_data.Length * sizeof(float), g_quad_uv_buffer_data, BufferUsageHint.StaticDraw);

            //// Create and compile our GLSL program from the shaders
            _fullscreenquad_programID = Shaders.Load(
                ResourcePath("FullScreenQuad.vertexshader"),
                ResourcePath("FullScreenQuad.fragmentshader"));
            _fullscreenquad_programTexLocation = GL.GetUniformLocation(_fullscreenquad_programID, "u_source");

            // end fullscreen quad render pass
            // ------------------------------------------------------------------------------------
        }

        // This function runs on every update frame.
        protected void RenderFrameInternal(FrameEventArgs e)
        {
            //base.OnRenderFrame(e);

            // ------------------------------------------------------------------------------------
            // render suzanne to frame buffer 0

            // Render to our framebuffer
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _suzannePass_FrameBuffer);

            GL.Viewport(0, 0, _frameBufferWidth, _frameBufferHeight); // Render on the whole framebuffer, complete from the lower left corner to the upper right

            // Dark blue background
            GL.ClearColor(0.0f, 0.0f, 0.4f, 0.0f);

            // Clear the screen
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Use our shader
            GL.UseProgram(_suzanneRenderPass_Program);

            // Compute the MVP matrix from keyboard and mouse input
            var modelMatrix = Matrix4.Identity;
            var viewMatrix = Matrix4.LookAt(
                new Vector3(4, 3, 3), // Camera is at (4,3,3), in World Space
                Vector3.Zero,         // and looks at the origin
                Vector3.UnitY         // Head is up (set to 0,-1,0 to look upside-down)
            );
            var projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), CameraAspectRatio, 0.1f, 100.0f);
            var mvp = modelMatrix * viewMatrix * projectionMatrix;

            // Send our transformation to the currently bound shader, 
            // in the "MVP" uniform
            GL.UniformMatrix4(_suzannePass_Program_ModelViewProjectionMatrixLocation, false, ref mvp);
            GL.UniformMatrix4(_suzannePass_Program_ModelMatrixLocation, false, ref modelMatrix);
            GL.UniformMatrix4(_suzannePass_Program_ViewMatrixLocation, false, ref viewMatrix);

            var lightPos = new Vector3(4, 4, 4);
            GL.Uniform3(_suzannePass_Program_LightLocation, lightPos.X, lightPos.Y, lightPos.Z);

            // Bind our texture in Texture Unit 0
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _suzannePass_Texture);

            // Set our "myTextureSampler" sampler to use Texture Unit 0
            //glUniform1i(TextureID, 0);
            GL.Uniform1(_suzannePass_Program_TextureLocation, 0);

            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _suzanneVertexbuffer);
            GL.VertexAttribPointer(
                0,                  // attribute 0. No particular reason for 0, but must match the layout in the shader.
                3,                  // size
                VertexAttribPointerType.Float,           // type
                false,              // normalized?
                0,                  // stride
                0                   // array buffer offset 
            );

            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _suzanneUvBuffer);
            GL.VertexAttribPointer(
                1,                  // attribute. No particular reason for 1, but must match the layout in the shader.
                2,                  // size
                VertexAttribPointerType.Float,           // type
                false,              // normalized?
                0,                  // stride
                0                   // array buffer offset
            );

            GL.EnableVertexAttribArray(2);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _suzanneNormalBuffer);
            GL.VertexAttribPointer(
                2,                  // attribute. No particular reason for 1, but must match the layout in the shader.
                3,                  // size
                VertexAttribPointerType.Float,           // type
                false,              // normalized?
                0,                  // stride
                0                   // array buffer offset
            );

            // Index buffer
            //glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, elementbuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _suzanneElementBuffer);

            GL.DrawElements(
                OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,            // mode
                _suzanneIndexCount,                         // count
                DrawElementsType.UnsignedShort,     // type
                0);                                 // element array buffer offset

            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(2);

            // end render suzanne to frame buffer 0
            // ------------------------------------------------------------------------------------


            // ------------------------------------------------------------------------------------
            // render desaturate pass to frame buffer 1

            //GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _framebuffer0);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _desaturatePass_frameBuffer);
            GL.Viewport(0, 0, _frameBufferWidth, _frameBufferHeight);
            GL.ClearColor(255.0f / 255.0f, 255.0f / 255.0f, 255.0f / 255.0f, 0.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _suzanne_FrameBuffer_Texture);

            GL.UseProgram(_desaturatePass_Program);
            GL.Uniform1(_desaturatePass_ProgramSourceLocation, 0);
            //GL.Uniform1(_fullscreenquad_programTexLocation, 0);

            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _fullscreenquad_vertexbuffer);
            GL.VertexAttribPointer(
                0,                  // layout 0
                3,                  // size
                VertexAttribPointerType.Float,           // type
                false,              // normalized?
                0,                  // stride
                0                   // array buffer offset
            );

            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _fullscreenquad_uvbuffer);
            GL.VertexAttribPointer(
                1,                  // layout 1
                2,                  // size
                VertexAttribPointerType.Float,           // type
                false,              // normalized?
                0,                  // stride
                0                   // array buffer offset
            );

            GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, 3); // 2*3 indices starting at 0 -> 2 triangles

            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);

            // end render desaturate pass to frame buffer 1
            // ------------------------------------------------------------------------------------


            // ------------------------------------------------------------------------------------
            // render filmic pass

            //GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _filmicPass_Framebuffer);
            //GL.Viewport(0, 0, windowWidth, windowHeight);
            //GL.ClearColor(255.0f / 255.0f, 255.0f / 255.0f, 255.0f / 255.0f, 0.0f);
            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //GL.ActiveTexture(TextureUnit.Texture0);
            //GL.BindTexture(TextureTarget.Texture2D, _desaturatePass_FrameBufferTexture);

            //GL.UseProgram(_filmicPass_Program);
            //GL.Uniform1(_filmicPass_Program_SourceLocation, 0);
            //GL.Uniform1(_filmicPass_Program_TimeLocation, (float)TotalTime);

            //GL.EnableVertexAttribArray(0);
            //GL.BindBuffer(BufferTarget.ArrayBuffer, _fullscreenquad_vertexbuffer);
            //GL.VertexAttribPointer(
            //    0,                  // layout 0
            //    3,                  // size
            //    VertexAttribPointerType.Float,           // type
            //    false,              // normalized?
            //    0,                  // stride
            //    0                   // array buffer offset
            //);

            //GL.EnableVertexAttribArray(1);
            //GL.BindBuffer(BufferTarget.ArrayBuffer, _fullscreenquad_uvbuffer);
            //GL.VertexAttribPointer(
            //    1,                  // layout 1
            //    2,                  // size
            //    VertexAttribPointerType.Float,           // type
            //    false,              // normalized?
            //    0,                  // stride
            //    0                   // array buffer offset
            //);

            //GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            //GL.DisableVertexAttribArray(0);
            //GL.DisableVertexAttribArray(1);

            // end render filmic pass
            // ------------------------------------------------------------------------------------


            // ------------------------------------------------------------------------------------
            // horizontal blur pass

            //GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _framebuffer0);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _horizontalBlurPass_Framebuffer);
            GL.Viewport(0, 0, _frameBufferWidth, _frameBufferHeight);
            GL.ClearColor(255.0f / 255.0f, 255.0f / 255.0f, 255.0f / 255.0f, 0.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _desaturatePass_FrameBufferTexture);

            var identity = Matrix4.Identity;

            GL.UseProgram(_horizontalBlurPass_Program);
            GL.Uniform1(_horizontalBlurPass_Program_SourceLocation, 0);
            GL.Uniform1(_horizontalBlurPass_Program_RadiusLocation, 8.0f);
            GL.Uniform2(_horizontalBlurPass_Program_ResolutionLocation, new Vector2((float)_windowWidth, (float)_windowHeight));
            GL.UniformMatrix4(_horizontalBlurPass_Program_ModelViewProjectionMatrixMatrixLocation, false, ref identity);

            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _fullscreenquad_vertexbuffer);
            GL.VertexAttribPointer(
                0,                  // layout 0
                3,                  // size
                VertexAttribPointerType.Float,           // type
                false,              // normalized?
                0,                  // stride
                0                   // array buffer offset
            );

            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _fullscreenquad_uvbuffer);
            GL.VertexAttribPointer(
                1,                  // layout 1
                2,                  // size
                VertexAttribPointerType.Float,           // type
                false,              // normalized?
                0,                  // stride
                0                   // array buffer offset
            );

            GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, 3);

            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);

            // end horizontal blur pass
            // ------------------------------------------------------------------------------------

            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);

            GL.Viewport(0, 0, _windowWidth, _windowHeight);

            GL.ClearColor(251.0f / 255.0f, 204.0f / 255.0f, 122.0f / 255.0f, 0.0f);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.ActiveTexture(TextureUnit.Texture0);
            //GL.BindTexture(TextureTarget.Texture2D, _desaturatePass_FrameBufferTexture);
            //GL.BindTexture(TextureTarget.Texture2D, _filmicPass_FramebufferTexture);
            GL.BindTexture(TextureTarget.Texture2D, _horizontalBlurPass_FramebufferTexture);

            if (_renderToFullScreenQuad)
            {
                GL.UseProgram(_fullscreenquad_programID);

                GL.Uniform1(_fullscreenquad_programTexLocation, 0);

                GL.EnableVertexAttribArray(0);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _fullscreenquad_vertexbuffer);
                GL.VertexAttribPointer(
                    0,                  // attribute 0. No particular reason for 0, but must match the layout in the shader.
                    3,                  // size
                    VertexAttribPointerType.Float,           // type
                    false,              // normalized?
                    0,                  // stride
                    0                   // array buffer offset
                );

                GL.EnableVertexAttribArray(1);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _fullscreenquad_uvbuffer);
                GL.VertexAttribPointer(
                    1,                  // attribute 1.
                    2,                  // size
                    VertexAttribPointerType.Float,           // type
                    false,              // normalized?
                    0,                  // stride
                    0                   // array buffer offset
                );

                GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, 3);

                GL.DisableVertexAttribArray(0);
                GL.DisableVertexAttribArray(1);
            }
            else
            {
                GL.UseProgram(_box_programID);

                modelMatrix = Matrix4.Identity;

                mvp = modelMatrix * CameraViewMatrix * CameraProjectionMatrix;

                GL.UniformMatrix4(_box_matrixID, false, ref mvp);

                GL.EnableVertexAttribArray(0);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _box_vertexbuffer);
                GL.VertexAttribPointer(
                    0,                  // attribute 0. No particular reason for 0, but must match the layout in the shader.
                    3,                  // size
                    VertexAttribPointerType.Float,           // type
                    false,              // normalized?
                    0,                  // stride
                    0                   // array buffer offset
                );

                GL.EnableVertexAttribArray(1);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _box_uvbuffer);
                GL.VertexAttribPointer(
                    1,                  // attribute. No particular reason for 1, but must match the layout in the shader.
                    2,                  // size
                    VertexAttribPointerType.Float,           // type
                    false,              // normalized?
                    0,                  // stride
                    0                   // array buffer offset
                );

                GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, _box_verticeCount);

                GL.DisableVertexAttribArray(0);
                GL.DisableVertexAttribArray(1);
            }

            if (FrameCount == 1 && OperatingSystem.IsWindows())
            {
                Save(_desaturatePass_frameBuffer, _frameBufferWidth, _frameBufferHeight);
                //Save(_filmicPass_Framebuffer, _frameBufferWidth, _frameBufferHeight);
                Save(_horizontalBlurPass_Framebuffer, _frameBufferWidth, _frameBufferHeight);
            }

            // Swap buffers
            _internalWindow.SwapBuffers();
        }

        // This function runs on every update frame.
        protected void UpdateFrameInternal(FrameEventArgs e)
        {
            if (!_internalWindow.IsFocused)
            {
                return;
            }

            FrameCount++;
            TotalTime += e.Time;

            // Check if the ESC key was pressed or the window was closed
            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                _internalWindow.Close();
            }

            if (KeyboardState.IsKeyPressed(Keys.Enter))
            {
                _renderToFullScreenQuad = !_renderToFullScreenQuad;
                Diagnostics.Log("Render to screen quad: " + _renderToFullScreenQuad);
            }

            if (MouseState.IsButtonPressed(MouseButton.Right))
            {
                CaptureInput = !CaptureInput;
                if (CaptureInput)
                {
                    CapturedMousePosition = new Vector2(_internalWindow.MouseState.X, _internalWindow.MouseState.Y);
                }
                _internalWindow.CursorState = CaptureInput ? CursorState.Hidden : CursorState.Normal;
                Diagnostics.Log("CaptureMouse: " + CaptureInput);
            }

            UpdateControls(e);
        }


        public void UpdateControls(FrameEventArgs e)
        {
            var deltaTime = (float)e.Time;

            if (CaptureInput)
            {
                // Compute new orientation
                _cameraHorizontalAngle += _cameraRotationSpeed * (float)(CapturedMousePosition.X - _internalWindow.MouseState.X);
                _cameraVerticalAngle += _cameraRotationSpeed * (float)(CapturedMousePosition.Y - _internalWindow.MouseState.Y);

                // Reset mouse position for next frame
                _internalWindow.MousePosition = new Vector2(CapturedMousePosition.X, CapturedMousePosition.Y);
            }

            // Direction : Spherical coordinates to Cartesian coordinates conversion
            var direction = new Vector3(
                MathF.Cos(_cameraVerticalAngle) * MathF.Sin(_cameraHorizontalAngle),
                MathF.Sin(_cameraVerticalAngle),
                MathF.Cos(_cameraVerticalAngle) * MathF.Cos(_cameraHorizontalAngle)
            );

            // Right vector
            var right = new Vector3(
                MathF.Sin(_cameraHorizontalAngle - 3.14f / 2.0f),
                0,
                MathF.Cos(_cameraHorizontalAngle - 3.14f / 2.0f)
            );

            // Up vector
            var up = Vector3.Cross(right, direction);

            if (CaptureInput)
            {
                // Move forward
                if (KeyboardState.IsKeyDown(Keys.Up))
                {
                    _cameraPosition += direction * deltaTime * _cameraForwardSpeed;
                }
                // Move backward
                if (KeyboardState.IsKeyDown(Keys.Down))
                {
                    _cameraPosition -= direction * deltaTime * _cameraForwardSpeed;
                }
                // Strafe right
                if (KeyboardState.IsKeyDown(Keys.Right))
                {
                    _cameraPosition += right * deltaTime * _cameraForwardSpeed;
                }
                // Strafe left
                if (KeyboardState.IsKeyDown(Keys.Left))
                {
                    _cameraPosition -= right * deltaTime * _cameraForwardSpeed;
                }
                if (KeyboardState.IsKeyDown(Keys.PageUp))
                {
                    _cameraPosition += up * deltaTime * _cameraForwardSpeed;
                }
                if (KeyboardState.IsKeyDown(Keys.PageDown))
                {
                    _cameraPosition -= up * deltaTime * _cameraForwardSpeed;
                }
            }

            CameraProjectionMatrix = ProjectionType == ProjectionType.Perspective
                ? Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(CameraFovInDegrees), CameraAspectRatio, CameraNear, CameraFar)
                : Matrix4.CreateOrthographic(_frustumWidth, _frustumHeight, CameraNear, CameraFar);

            CameraViewMatrix = Matrix4.LookAt(
                _cameraPosition,   // Camera is here
                _cameraPosition + direction,         // and looks here : at the same position, plus "direction"
                up         // Head is up (set to 0,-1,0 to look upside-down)
            );
        }


        public KeyboardState KeyboardState => _internalWindow.KeyboardState;
        public MouseState MouseState => _internalWindow.MouseState;

        protected void UnLoadInternal()
        {
            //base.OnUnload();

            GL.DeleteBuffer(_suzanneVertexbuffer);
            GL.DeleteBuffer(_suzanneUvBuffer);
            GL.DeleteBuffer(_suzanneNormalBuffer);
            GL.DeleteBuffer(_suzanneElementBuffer);
            GL.DeleteProgram(_suzanneRenderPass_Program);
            GL.DeleteTexture(_suzannePass_Texture);
            GL.DeleteVertexArray(_suzanneVertexArray);

            GL.DeleteFramebuffer(_suzannePass_FrameBuffer);
            GL.DeleteTexture(_suzanne_FrameBuffer_Texture);
            GL.DeleteRenderbuffer(_suzanne_FrameBuffer_DepthBuffer);
            GL.DeleteBuffer(_fullscreenquad_vertexbuffer);
            GL.DeleteBuffer(_fullscreenquad_uvbuffer);

            GL.DeleteBuffer(_box_vertexbuffer);
            GL.DeleteBuffer(_box_uvbuffer);
            GL.DeleteProgram(_box_programID);
        }

        private void ResizeInternal(ResizeEventArgs a)
        {
            GL.Viewport(0, 0, _internalWindow.ClientSize.X, _internalWindow.ClientSize.Y);

            CameraAspectRatio = (float)_internalWindow.ClientSize.X / (float)_internalWindow.ClientSize.Y;
            //Controls.AspectRatio = (float)_internalWindow.ClientSize.X / (float)_internalWindow.ClientSize.Y;
        }

        [SupportedOSPlatform("windows")]
        public void Save(int handle, int width, int height, string? destination = null)
        {
            if (string.IsNullOrEmpty(destination))
            {
                destination = ResourcePath($".saves/{DateTime.Now.ToString("yyyyMMdd.HHmmss.ffff")}.png");
            }
            else if (!destination.EndsWith(".png"))
            {
                destination += ".png";
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, handle);

            using (var bitmap = new System.Drawing.Bitmap(
                width,
                height,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                var data = bitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, width, height),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.PixelStore(PixelStoreParameter.PackRowLength, data.Stride / 4);

                GL.ReadPixels(
                    0, 0,
                    width, height,
                    OpenTK.Graphics.OpenGL4.PixelFormat.Bgra,
                    PixelType.UnsignedByte,
                    data.Scan0);

                bitmap.UnlockBits(data);

                bitmap.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);

                bitmap.Save(destination, System.Drawing.Imaging.ImageFormat.Png);
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        private readonly int[] AvailableSizes = { 3520, 2920, 2320, 1920, 1520, 1120, 920, 720, 560, 400 };

        private Vector2i CalculateWindowSize(float frustumWidth, float frustumHeight)
        {
            var primaryMonitor = Monitors.GetPrimaryMonitor();

            var targetWindowWidth = AvailableSizes.First(o => o < primaryMonitor.HorizontalResolution);
            var targetWindowHeight = AvailableSizes.First(o => o < primaryMonitor.VerticalResolution);

            var windowWidth = (int)(targetWindowHeight * frustumWidth / frustumHeight);
            var windowHeight = targetWindowHeight;

            if (windowWidth > targetWindowWidth || windowHeight > targetWindowWidth)
            {
                windowWidth = targetWindowWidth;
                windowHeight = (int)(targetWindowWidth * frustumHeight / frustumWidth);
            }

            return new Vector2i(windowWidth, windowHeight);
        }

        //private Vector2i CalculateWindowSize(float frustumWidth, float frustumHeight)
        //{
        //    var primaryMonitor = Monitors.GetPrimaryMonitor();

        //    var hscale = primaryMonitor.HorizontalResolution / frustumWidth;
        //    var vscale = primaryMonitor.VerticalResolution / frustumHeight;

        //    var scale = MathF.Min(hscale, vscale);

        //    return new Vector2i((int)MathF.Floor(frustumWidth * scale), (int)MathF.Floor(frustumHeight * scale));
        //}

        private void CalculateWindowSize()
        {
            var size = CalculateWindowSize(_frustumWidth, _frustumHeight);

            _windowWidth = size.X;
            _windowHeight = size.Y;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _internalWindow?.CursorState = CursorState.Normal;
                // dispose managed objects
                _internalWindow?.Dispose();
            }

            // free unmanaged resources (unmanaged objects)
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected string ResourcePath(string resource)
        {
            return System.IO.Path.Combine(ResourceDirectory, resource);
        }

        public void Focus()
        {
            _internalWindow?.Focus();
        }

        public void Run()
        {
            _internalWindow?.Run();
        }
    }
}