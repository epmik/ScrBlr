
using Microsoft.Extensions.Configuration;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Skrbl;
using System;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.Versioning;
using static Skrbl.Gdi32;
using static System.Net.Mime.MediaTypeNames;

namespace OpenGlTutorialOrg
{
    public class Tutorial14WindowExtented07 : IDisposable
    {
        public class RenderTarget
        {
            public GeometryBuffer Lines;

            public RenderTarget()
            {
                Lines = new GeometryBuffer();
            }
        }

        public class GeometryBuffer
        {
            public GeometryBuffer Camera()
            {
                return this;
            }

            public GeometryBuffer Transform(ref Matrix4 modelMatrix)
            {
                return this;
            }

            public GeometryBuffer Color(ReadOnlySpan<float> color)
            {
                return this;
            }

            public GeometryBuffer From(ReadOnlySpan<float> position)
            {
                return this;
            }

            public GeometryBuffer To(ReadOnlySpan<float> position)
            {
                return this;
            }

            public GeometryBuffer Close()
            {
                return this;
            }
        }

        RenderTarget Graphics;

        GameWindow _internalWindow;

        Resources Resources;

        bool CaptureInput = false;
        Vector2 CapturedMousePosition;
        protected string ResourceDirectory = string.Empty;
        protected string ShaderDirectory = string.Empty;
        protected string FontDirectory = string.Empty;
        bool _renderToFullScreenQuad = true;
        bool _renderRandomPass = false;
        bool _renderNoisePass = false;
        bool _enablePostProcessing = false;
        bool _enableClearingFrameBuffer = true;
        double TotalTime;
        int FrameCount;

        private IRandomGenerator _defaultRandomGenerator = new RandomGenerator();   
        
        public IRandomGenerator Random { get { return _defaultRandomGenerator; } }

        int _render_vao;
        int _render_frameBuffer;
        int _render_frameBuffer_texture;
        int _render_frameBuffer_depthBuffer;
        float _renderNewQuadTimeOut = 0.5f;
        float _renderNewQuadTimer = 1f;

        int _noise_frameBuffer;
        int _noise_frameBuffer_texture;
        int _noise_program;
        int _noise_program_octavesLocation;
        int _noise_program_amplitudeLocation;
        int _noise_program_frequencyLocation;
        int _noise_program_lacunarityLocation;
        int _noise_program_persistenceLocation;
        int _noise_program_inputScaleLocation;
        int _noise_program_inputOffsetLocation;
        int _noise_program_biasLocation;
        int _noise_program_noiseTypeLocation;

        int _random_frameBuffer;
        int _random_frameBuffer_texture;
        int _random_program;

        //int _suzanne_xyzBuffer;
        //int _suzanne_uvBuffer;
        //int _suzanne_normalBuffer;
        //int _suzanne_elementBuffer;
        //int _suzanne_Texture;
        //int _suzanne_program;
        //int _suzanne_program_textureLocation;
        //int _suzanne_program_modelViewProjectionMatrixLocation;
        //int _suzanne_program_viewMatrixLocation;
        //int _suzanne_program_modelMatrixLocation;
        //int _suzanne_program_lightLocation;
        //int _suzanne_IndexCount;

        int _desaturate_frameBuffer;
        int _desaturate_frameBuffer_texture;
        int _desaturate_frameBuffer_depthBuffer;
        int _desaturate_program;
        int _desaturate_program_sourceLocation;

        int _fullscreenTriangle_vao;
        int _fullscreenTriangle_xyzBffer;
        int _fullscreenTriangle_uvBuffer;
        int _fullscreenTriangle_program;
        int _fullscreenTriangle_program_texLocation;

        int _quads_vao;
        int _quads_vertexbuffer;
        // the number of elements per vertex
        int _quads_vertexbuffer_elementCount;
        // the byte offset between consecutive vertices
        int _quads_vertexbuffer_elementStride;
        // the total size of the vertex buffer in bytes
        int _quads_vertexbuffer_totalBytes;
        int _quads_vertexbuffer_totalVertices;
        // the number of used bytes in the vertex buffer
        int _quads_vertexbuffer_usedBytes;
        int _quads_vertexbuffer_usedVertices;
        int _quads_program;
        int _quads_program_modelViewProjectionMatrixLocation;

        int _font_vao;
        int _font_texture;
        int _font_vertexbuffer;
        // the number of elements per vertex
        int _font_vertexbuffer_elementCount;
        // the byte offset between consecutive vertices
        int _font_vertexbuffer_elementStride;
        // the total size of the vertex buffer in bytes
        int _font_vertexbuffer_totalBytes;
        int _font_vertexbuffer_totalVertices;
        // the number of used bytes in the vertex buffer
        int _font_vertexbuffer_usedBytes;
        int _font_vertexbuffer_usedVertices;
        int _font_program;
        int _font_program_modelViewProjectionMatrixLocation;
        FontSettings _font_Settings = new FontSettings();
        int _font_texture_2;


        struct FontDictionaryEntry
        {
            public float width;
            public float height;
            public float u;
            public float v;
            public float uu;
            public float vv;
        }

        int _font_Size;
        Dictionary<Char, FontDictionaryEntry> _font_Dictionary = new Dictionary<Char, FontDictionaryEntry>();
        Dictionary<Char, FontDictionaryEntry> _font_Dictionary_2 = new Dictionary<Char, FontDictionaryEntry>();

        int _lines_vao;
        int _lines_vertexbuffer;
        // the number of elements per vertex
        int _lines_vertexbuffer_elementCount;
        // the byte offset between consecutive vertices
        int _lines_vertexbuffer_elementStride;
        // the total size of the vertex buffer in bytes
        int _lines_vertexbuffer_totalBytes;
        int _lines_vertexbuffer_totalVertices;
        // the number of used bytes in the vertex buffer
        int _lines_vertexbuffer_usedBytes;
        int _lines_vertexbuffer_usedVertices;
        int _lines_program;
        int _lines_program_modelViewProjectionMatrixLocation;
        float _renderNewLineTimeOut = 0.5f;
        float _renderNewLineTimer = 0.5f;

        //int _box_vao;
        //int _box_xyzbuffer;
        //int _box_uvbuffer;
        //int _box_programID;
        //int _box_matrixID;
        //int _box_verticeCount = 6;
        //float _box_scale = 200;

        int _horizontalBlur_framebuffer;
        int _horizontalBlur_framebuffer_texture;
        int _horizontalBlur_framebuffer_depthBuffer;
        int _horizontalBlur_program;
        int _horizontalBlur_program_sourceLocation;
        int _horizontalBlur_program_resolutionLocation;
        int _horizontalBlur_program_radiusLocation;
        int _horizontalBlur_program_modelViewProjectionMatrixMatrixLocation;

        int _windowWidth;
        int _windowHeight;

        int _frustumWidth = 1024;
        int _frustumHeight = 1024;

        float _frameBufferScale = 4.0f;

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

        private const float DefaultCameraForwardSpeed = 300.0f; // units / second

        float _cameraForwardSpeed = DefaultCameraForwardSpeed;
        float _cameraRotationSpeed = 0.005f;

        private const float DefaultCameraNear = 0.01f;
        private const float DefaultCameraFar = 10000.0f;

        public float CameraAspectRatio;
        public float CameraNear = DefaultCameraNear;
        public float CameraFar = DefaultCameraFar;

        private float _cameraFovInRadians = MathHelper.DegreesToRadians(66.0f);

        SketchOrigin _origin = SketchOrigin.Center;
        Matrix4 _originMatrix = Matrix4.Identity;
        private int _noiseType;

        internal enum SketchOrigin
        {
            Center,
            LeftTop,
            LeftBottom,
        }

        internal class SketchSettings
        {
            public float FrustumWidth { get; set; } = 1024.0f;
            public float FrustumHeight { get; set; } = 1024.0f;
            public float FrameBufferScale { get; set; } = 4.0f;
            public SketchOrigin Origin { get; set; } = SketchOrigin.Center;
        }

        internal class OpenGlSettings
        {
            /// <para>
            /// OpenGL 3.3 is selected by default, and runs on almost any hardware made within the last ten years.
            /// This will run on Windows, Mac OS, and Linux.
            /// </para>
            /// <para>
            /// OpenGL 4.1 is suggested for modern apps meant to run on more modern hardware.
            /// This will run on Windows, Mac OS, and Linux.
            /// </para>
            /// <para>
            /// OpenGL 4.6 is suggested for modern apps that only intend to run on Windows and Linux;
            /// Mac OS doesn't support it.
            /// </para>
            public Version Version { get; set; } = new Version(3, 3);
            public ContextFlags Flags { get; set; } = ContextFlags.ForwardCompatible | ContextFlags.Debug;
        }

        internal class WindowSettings
        {
            public string Title { get; set; } = string.Empty;
            public bool IsFullScreen { get; set; } = false;
            public bool VSync { get; set; } = false;
            public WindowBorder WindowBorder { get; set; } = WindowBorder.Fixed;
            public float Width { get; set; } = 1024.0f;
            public float Height { get; set; } = 1024.0f;
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

        public Tutorial14WindowExtented07(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
        {
            Resources = new Resources([
                "C:\\Steven\\Atelier\\Scrblr\\Skrbl\\opengl-tutorial.org\\tutorial14_render_to_texture",
                "C:\\Steven\\Atelier\\Scrblr\\Skrbl"]);

            //var importer = new Skrbl.Blender.Importer(new Skrbl.Blender.ImporterSettings
            //{
            //    FilePath = Resources.Scene("default-box.blend"),
            //});

            //importer.Parse();

            //var modelGltfTriangle = Skrbl.Gltf.Import(new Skrbl.Gltf.ImportSettings
            //{
            //    FilePath = Resources.Scene("TriangleWithoutIndices/glTF/TriangleWithoutIndices.gltf"),
            //});

            //var modelGltf = Skrbl.Gltf.Import(new Skrbl.Gltf.ImportSettings
            //{
            //    FilePath = Resources.Scene("default-scene.gltf"),
            //});

            //var modelGltfMonkey = Skrbl.Gltf.Import(new Skrbl.Gltf.ImportSettings
            //{
            //    FilePath = Resources.Scene("monkey.gltf"),
            //});

            //var modelGlb = Skrbl.Gltf.Import(new Skrbl.Gltf.ImportSettings
            //{
            //    FilePath = Resources.Scene("default-scene.glb"),
            //});

            //var modelGlbMonkey = Skrbl.Gltf.Import(new Skrbl.Gltf.ImportSettings
            //{
            //    FilePath = Resources.Scene("monkey.glb"),
            //});

            CalculateWindowSize();

            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile(Resources.Path($"{GetType().Name}.json"), optional: true, reloadOnChange: true)
                .Build();

            var sketchSettings = new SketchSettings();
            var windowSettings = new WindowSettings();
            var openGlSettings = new OpenGlSettings();

            configuration.GetSection("Sketch").Bind(sketchSettings);
            configuration.GetSection("Window").Bind(windowSettings);
            configuration.GetSection("OpenGl").Bind(openGlSettings);

            gameWindowSettings = gameWindowSettings != null ? gameWindowSettings : GameWindowSettings.Default;
            nativeWindowSettings = nativeWindowSettings != null ? nativeWindowSettings : new NativeWindowSettings()
            {
                APIVersion = new System.Version(4, 6),
                Title = "Tutorial 14 - Render To Texture - Extented 7",
                Flags = ContextFlags.ForwardCompatible | ContextFlags.Debug,     // needed to run on macos
                WindowBorder = WindowBorder.Hidden,
                NumberOfSamples = 4,
                DepthBits = 32,
                StencilBits = 0,
            };

            _origin = sketchSettings.Origin;

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

            GenerateFontTextureAndDictionaryAndBindTexture(_font_Settings);

            _font_Dictionary_2 = GenerateFontTextureAndDictionaryAndBindTexture(new FontSettings
            {
                TextureWidth = 1024,
                TextureHeight = 1024,
                FontName = Resources.Font("Roboto-Regular.ttf"),
                FontSize = 32,
            }, out _font_texture_2);
        }

        private void LoadInternal()
        {
            Diagnostics.EnableOpenGlDebugMessages([DebugSeverityControl.DebugSeverityHigh, DebugSeverityControl.DebugSeverityMedium, DebugSeverityControl.DebugSeverityLow]);

            Graphics = new RenderTarget();

            //ImGui.CreateContext();
            //ImGuiIOPtr io = ImGui.GetIO();
            //io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
            //io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;
            //io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            //io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;

            //ImGui.StyleColorsDark();

            //ImGuiStylePtr style = ImGui.GetStyle();
            //if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
            //{
            //    style.WindowRounding = 0.0f;
            //    style.Colors[(int)ImGuiCol.WindowBg].W = 1.0f;
            //}


            switch (_origin)
            {
                case SketchOrigin.Center:
                    _originMatrix = Matrix4.Identity;
                    break;
                case SketchOrigin.LeftTop:
                    _originMatrix = Matrix4.CreateTranslation(-_frustumWidth / 2.0f, _frustumHeight / 2.0f, 0.0f);
                    break;
                case SketchOrigin.LeftBottom:
                    _originMatrix = Matrix4.CreateTranslation(-_frustumWidth / 2.0f, -_frustumHeight / 2.0f, 0.0f);
                    break;
            }

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

            _render_vao = GL.GenVertexArray();
            GL.BindVertexArray(_render_vao);

            //// a vertex contains 12 floats (position, normal, color, tex)
            //_render_vertexbuffer_elementCount = 3 + 3 + 4 + 2;
            //// size in bytes of a vertex
            //_render_vertexbuffer_elementStride = _render_vertexbuffer_elementCount * sizeof(float);
            //// total vertices
            //_render_vertexbuffer_totalVertices = 1024 * 1024;
            //_render_vertexbuffer_totalBytes = _render_vertexbuffer_totalVertices * _render_vertexbuffer_elementStride;
            //_render_vertexbuffer_usedVertices = 0;
            //_render_vertexbuffer_usedBytes = 0;

            //// Specifies the number of components per generic vertex attribute. Must be 1, 2, 3, 4
            //var size = 3;
            //var offset = 0;

            //_render_vertexBuffer = GL.GenBuffer();
            //GL.BindBuffer(BufferTarget.ArrayBuffer, _render_vertexBuffer);
            //GL.BufferData(BufferTarget.ArrayBuffer, _render_vertexbuffer_totalBytes, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            //GL.EnableVertexAttribArray(0);
            //GL.VertexAttribPointer(
            //    0,                  // layout 0
            //    size,               // the number of components per generic vertex attribute
            //    VertexAttribPointerType.Float,           // type
            //    false,              // specifies whether fixed-point data values should be normalized (true) or converted directly as fixed-point values (false) when they are accessed
            //    _quads_vertexbuffer_elementStride,                  // the byte offset between consecutive generic vertex attributes
            //    offset              // offset of the first component of the first generic vertex attribute
            //);

            //offset += size * sizeof(float);
            //size = 3;
            //GL.EnableVertexAttribArray(1);
            //GL.VertexAttribPointer(
            //    1,                  // layout 1
            //    size,                  // the number of components per generic vertex attribute
            //    VertexAttribPointerType.Float,           // type
            //    false,              // specifies whether fixed-point data values should be normalized (true) or converted directly as fixed-point values (false) when they are accessed
            //    _quads_vertexbuffer_elementStride,                  // the byte offset between consecutive generic vertex attributes
            //    offset               // offset of the first component of the first generic vertex attribute
            //);

            //offset += size * sizeof(float);
            //size = 4;
            //GL.EnableVertexAttribArray(2);
            //GL.VertexAttribPointer(
            //    2,                  // layout 2
            //    size,                  // the number of components per generic vertex attribute
            //    VertexAttribPointerType.Float,           // type
            //    false,              // specifies whether fixed-point data values should be normalized (true) or converted directly as fixed-point values (false) when they are accessed
            //    _quads_vertexbuffer_elementStride,                  // the byte offset between consecutive generic vertex attributes
            //    offset               // offset of the first component of the first generic vertex attribute
            //);

            //offset += size * sizeof(float);
            //size = 2;
            //GL.EnableVertexAttribArray(3);
            //GL.VertexAttribPointer(
            //    3,                  // layout 2
            //    size,               // the number of components per generic vertex attribute
            //    VertexAttribPointerType.Float,           // type
            //    false,              // specifies whether fixed-point data values should be normalized (true) or converted directly as fixed-point values (false) when they are accessed
            //    _quads_vertexbuffer_elementStride,             // the byte offset between consecutive generic vertex attributes
            //    offset              // offset of the first component of the first generic vertex attribute
            //);




            //// Create and compile our GLSL program from the shaders
            //_suzanne_program = Shaders.Load(
            //    ResourcePath("StandardShadingRTT.vert"),
            //    ResourcePath("StandardShadingRTT.frag"));

            //// Get a handle for our "MVP" uniform
            //_suzanne_program_modelViewProjectionMatrixLocation = GL.GetUniformLocation(_suzanne_program, "MVP");
            //_suzanne_program_viewMatrixLocation = GL.GetUniformLocation(_suzanne_program, "V");
            //_suzanne_program_modelMatrixLocation = GL.GetUniformLocation(_suzanne_program, "M");
            //_suzanne_program_textureLocation = GL.GetUniformLocation(_suzanne_program, "myTextureSampler");

            //// Load the texture
            //_suzanne_Texture = Textures.Load(ResourcePath("uvmap.png"));

            //// Read our .obj file
            //ObjLoader.Load(ResourcePath("suzanne.obj"), out var vertices, out var uvs, out var normals);

            //VboIndexer.IndexTriangles(
            //    vertices, uvs, normals,
            //    out ushort[] indices,
            //    out Vector3[] indexed_vertices,
            //    out Vector2[] indexed_uvs,
            //    out Vector3[] indexed_normals);

            //_suzanne_IndexCount = indices.Length;
            //// Load it into a VBO

            //_suzanne_xyzBuffer = GL.GenBuffer();
            //GL.EnableVertexAttribArray(0);
            //GL.BindBuffer(BufferTarget.ArrayBuffer, _suzanne_xyzBuffer);
            //GL.BufferData(BufferTarget.ArrayBuffer, indexed_vertices.Length * 3 * sizeof(float), indexed_vertices, BufferUsageHint.StaticDraw);
            //GL.VertexAttribPointer(
            //    0,                  // attribute 0. No particular reason for 0, but must match the layout in the shader.
            //    3,                  // size
            //    VertexAttribPointerType.Float,           // type
            //    false,              // normalized?
            //    0,                  // stride
            //    0                   // array buffer offset 
            //);

            //_suzanne_uvBuffer = GL.GenBuffer();
            //GL.EnableVertexAttribArray(1);
            //GL.BindBuffer(BufferTarget.ArrayBuffer, _suzanne_uvBuffer);
            //GL.BufferData(BufferTarget.ArrayBuffer, indexed_uvs.Length * 2 * sizeof(float), indexed_uvs, BufferUsageHint.StaticDraw);
            //GL.VertexAttribPointer(
            //    1,                  // attribute. No particular reason for 1, but must match the layout in the shader.
            //    2,                  // size
            //    VertexAttribPointerType.Float,           // type
            //    false,              // normalized?
            //    0,                  // stride
            //    0                   // array buffer offset
            //);

            //_suzanne_normalBuffer = GL.GenBuffer();
            //GL.EnableVertexAttribArray(2);
            //GL.BindBuffer(BufferTarget.ArrayBuffer, _suzanne_normalBuffer);
            //GL.BufferData(BufferTarget.ArrayBuffer, indexed_normals.Length * 3 * sizeof(float), indexed_normals, BufferUsageHint.StaticDraw);
            //GL.VertexAttribPointer(
            //    2,                  // attribute. No particular reason for 1, but must match the layout in the shader.
            //    3,                  // size
            //    VertexAttribPointerType.Float,           // type
            //    false,              // normalized?
            //    0,                  // stride
            //    0                   // array buffer offset
            //);

            //// Generate a buffer for the indices as well
            //_suzanne_elementBuffer = GL.GenBuffer();
            //GL.BindBuffer(BufferTarget.ElementArrayBuffer, _suzanne_elementBuffer);
            //GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(ushort), indices, BufferUsageHint.StaticDraw);



            //// Get a handle for our "LightPosition" uniform
            //GL.UseProgram(_suzanne_program);
            //_suzanne_program_lightLocation = GL.GetUniformLocation(_suzanne_program, "LightPosition_worldspace");

            // The framebuffer, which regroups 0, 1, or more textures, and 0 or 1 depth buffer.
            _render_frameBuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _render_frameBuffer);

            // The texture we're going to render to
            _render_frameBuffer_texture = GL.GenTexture();

            // "Bind" the newly created texture : all future texture functions will modify this texture
            GL.BindTexture(TextureTarget.Texture2D, _render_frameBuffer_texture);

            // Give an empty image to OpenGL ( the last "0" means "empty" )
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, _frameBufferWidth, _frameBufferHeight, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);

            // Poor filtering
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, _render_frameBuffer_texture, 0);

            // The depth buffer
            _render_frameBuffer_depthBuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _render_frameBuffer_depthBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, _frameBufferWidth, _frameBufferHeight);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _render_frameBuffer_depthBuffer);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                throw new Exception("Framebuffer not complete");

            // end suzanne render pass
            // ------------------------------------------------------------------------------------


            // ------------------------------------------------------------------------------------
            // setup noise frame buffer

            _noise_frameBuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _noise_frameBuffer);

            // The texture we're going to render to
            _noise_frameBuffer_texture = GL.GenTexture();

            // "Bind" the newly created texture : all future texture functions will modify this texture
            GL.BindTexture(TextureTarget.Texture2D, _noise_frameBuffer_texture);

            // Give an empty image to OpenGL ( the last "0" means "empty" )
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, _frameBufferWidth, _frameBufferHeight, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);

            // Poor filtering
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, _noise_frameBuffer_texture, 0);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                throw new Exception("Framebuffer not complete");

            _noise_program = Shaders.Load(
                Resources.Shader("pass-through.vert"),
                Resources.Shader("noise-visualize.frag"));

            _noise_program_octavesLocation = GL.GetUniformLocation(_noise_program, "u_octaves");
            _noise_program_amplitudeLocation = GL.GetUniformLocation(_noise_program, "u_amplitude");
            _noise_program_frequencyLocation = GL.GetUniformLocation(_noise_program, "u_frequency");
            _noise_program_lacunarityLocation = GL.GetUniformLocation(_noise_program, "u_lacunarity");
            _noise_program_persistenceLocation = GL.GetUniformLocation(_noise_program, "u_persistence");
            _noise_program_inputScaleLocation = GL.GetUniformLocation(_noise_program, "u_inputScale");
            _noise_program_inputOffsetLocation = GL.GetUniformLocation(_noise_program, "u_inputOffset");
            _noise_program_biasLocation = GL.GetUniformLocation(_noise_program, "u_bias");
            _noise_program_noiseTypeLocation = GL.GetUniformLocation(_noise_program, "u_noiseType");

            // end setup noise frame buffer
            // ------------------------------------------------------------------------------------


            // ------------------------------------------------------------------------------------
            // setup random frame buffer

            _random_frameBuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _random_frameBuffer);

            // The texture we're going to render to
            _random_frameBuffer_texture = GL.GenTexture();

            // "Bind" the newly created texture : all future texture functions will modify this texture
            GL.BindTexture(TextureTarget.Texture2D, _random_frameBuffer_texture);

            // Give an empty image to OpenGL ( the last "0" means "empty" )
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, _frameBufferWidth, _frameBufferHeight, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);

            // Poor filtering
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, _random_frameBuffer_texture, 0);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                throw new Exception("Framebuffer not complete");

            _random_program = Shaders.Load(
                Resources.Shader("pass-through.vert"),
                Resources.Shader("random.frag"));

            // end setup random frame buffer
            // ------------------------------------------------------------------------------------


            // ------------------------------------------------------------------------------------
            // quads vao

            _quads_program = Shaders.Load(
                Resources.Path("MvpXyzRgbaUv.vert"),
                Resources.Path("MvpXyzRgbaUv.frag"));

            _quads_program_modelViewProjectionMatrixLocation = GL.GetUniformLocation(_quads_program, "u_modelViewProjectionMatrix");

            // a vertex contains 9 floats (xyz, rgba, uv)
            _quads_vertexbuffer_elementCount = 3 + 4 + 2;
            // size in bytes of a single vertex
            _quads_vertexbuffer_elementStride = _quads_vertexbuffer_elementCount * sizeof(float);
            // 1024 * 1024 total vertices: 
            _quads_vertexbuffer_totalVertices = 1024 * 1024;
            // total size in bytes of the vertex buffer
            _quads_vertexbuffer_totalBytes = _quads_vertexbuffer_totalVertices * _quads_vertexbuffer_elementStride;

            // Specifies the number of components per generic vertex attribute. Must be 1, 2, 3, 4
            var size = 3;
            var offset = 0;

            _quads_vao = GL.GenVertexArray();
            GL.BindVertexArray(_quads_vao);

            _quads_vertexbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _quads_vertexbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, _quads_vertexbuffer_totalBytes, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(
                0,                  // layout 0
                size,               // the number of components per generic vertex attribute
                VertexAttribPointerType.Float,           // type
                false,              // specifies whether fixed-point data values should be normalized (true) or converted directly as fixed-point values (false) when they are accessed
                _quads_vertexbuffer_elementStride,                  // the byte offset between consecutive generic vertex attributes
                offset              // offset of the first component of the first generic vertex attribute
            );

            offset += size * sizeof(float);
            size = 4;
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(
                1,                  // layout 1
                size,                  // the number of components per generic vertex attribute
                VertexAttribPointerType.Float,           // type
                false,              // specifies whether fixed-point data values should be normalized (true) or converted directly as fixed-point values (false) when they are accessed
                _quads_vertexbuffer_elementStride,                  // the byte offset between consecutive generic vertex attributes
                offset               // offset of the first component of the first generic vertex attribute
            );

            offset += size * sizeof(float);
            size = 2;
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(
                2,                  // layout 2
                size,               // the number of components per generic vertex attribute
                VertexAttribPointerType.Float,           // type
                false,              // specifies whether fixed-point data values should be normalized (true) or converted directly as fixed-point values (false) when they are accessed
                _quads_vertexbuffer_elementStride,             // the byte offset between consecutive generic vertex attributes
                offset              // offset of the first component of the first generic vertex attribute
            );

            WriteQuad(0, 0, _frustumWidth / 2, _frustumHeight / 2, new Vector4(1.0f, 0.0f, 0.0f, 1.0f), out int storedQuadBytes);

            // end quads vao
            // ------------------------------------------------------------------------------------


            // ------------------------------------------------------------------------------------
            // fonts vao

            _font_program = Shaders.Load(
                Resources.Shader("font.vert"),
                Resources.Shader("font.frag"));

            _font_program_modelViewProjectionMatrixLocation = GL.GetUniformLocation(_font_program, "u_modelViewProjectionMatrix");

            // a vertex contains 9 floats (xyz, rgba, uv)
            _font_vertexbuffer_elementCount = 3 + 4 + 2;
            // size in bytes of a single vertex
            _font_vertexbuffer_elementStride = _font_vertexbuffer_elementCount * sizeof(float);
            // 1024 * 1024 total vertices: 
            _font_vertexbuffer_totalVertices = 1024 * 1024;
            // total size in bytes of the vertex buffer
            _font_vertexbuffer_totalBytes = _font_vertexbuffer_totalVertices * _font_vertexbuffer_elementStride;

            // Specifies the number of components per generic vertex attribute. Must be 1, 2, 3, 4
            size = 3;
            offset = 0;

            _font_vao = GL.GenVertexArray();
            GL.BindVertexArray(_font_vao);

            _font_vertexbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _font_vertexbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, _font_vertexbuffer_totalBytes, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(
                0,                  // layout 0
                size,               // the number of components per generic vertex attribute
                VertexAttribPointerType.Float,           // type
                false,              // specifies whether fixed-point data values should be normalized (true) or converted directly as fixed-point values (false) when they are accessed
                _font_vertexbuffer_elementStride,                  // the byte offset between consecutive generic vertex attributes
                offset              // offset of the first component of the first generic vertex attribute
            );

            offset += size * sizeof(float);
            size = 4;
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(
                1,                  // layout 1
                size,                  // the number of components per generic vertex attribute
                VertexAttribPointerType.Float,           // type
                false,              // specifies whether fixed-point data values should be normalized (true) or converted directly as fixed-point values (false) when they are accessed
                _font_vertexbuffer_elementStride,                  // the byte offset between consecutive generic vertex attributes
                offset               // offset of the first component of the first generic vertex attribute
            );

            offset += size * sizeof(float);
            size = 2;
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(
                2,                  // layout 2
                size,               // the number of components per generic vertex attribute
                VertexAttribPointerType.Float,           // type
                false,              // specifies whether fixed-point data values should be normalized (true) or converted directly as fixed-point values (false) when they are accessed
                _font_vertexbuffer_elementStride,             // the byte offset between consecutive generic vertex attributes
                offset              // offset of the first component of the first generic vertex attribute
            );

            WriteText("abcmdpziD,;:= %MdpzRPz@-+*/*1233", 10, 10, new Vector4(1.0f, 0.0f, 0.0f, 1.0f), out int storedFontBytes);

            // end fonts vao
            // ------------------------------------------------------------------------------------



            // ------------------------------------------------------------------------------------
            // quads vao

            _lines_program = Shaders.Load(
                Resources.Path("MvpXyzRgbaUv.vert"),
                Resources.Path("MvpXyzRgbaUv.frag"));

            _lines_program_modelViewProjectionMatrixLocation = GL.GetUniformLocation(_lines_program, "u_modelViewProjectionMatrix");

            // a vertex contains 7 floats (xyz, rgba)
            _lines_vertexbuffer_elementCount = 3 + 4;
            // size in bytes of a single vertex
            _lines_vertexbuffer_elementStride = _lines_vertexbuffer_elementCount * sizeof(float);
            // 1024 * 1024 total vertices: 
            _lines_vertexbuffer_totalVertices = 1024 * 1024;
            // total size in bytes of the vertex buffer
            _lines_vertexbuffer_totalBytes = _lines_vertexbuffer_totalVertices * _lines_vertexbuffer_elementStride;

            // Specifies the number of components per generic vertex attribute. Must be 1, 2, 3, 4
            size = 3;
            offset = 0;

            _lines_vao = GL.GenVertexArray();
            GL.BindVertexArray(_lines_vao);

            _lines_vertexbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _lines_vertexbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, _lines_vertexbuffer_totalBytes, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(
                0,                  // layout 0
                size,               // the number of components per generic vertex attribute
                VertexAttribPointerType.Float,           // type
                false,              // specifies whether fixed-point data values should be normalized (true) or converted directly as fixed-point values (false) when they are accessed
                _lines_vertexbuffer_elementStride,                  // the byte offset between consecutive generic vertex attributes
                offset              // offset of the first component of the first generic vertex attribute
            );

            offset += size * sizeof(float);
            size = 4;
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(
                1,                  // layout 1
                size,                  // the number of components per generic vertex attribute
                VertexAttribPointerType.Float,           // type
                false,              // specifies whether fixed-point data values should be normalized (true) or converted directly as fixed-point values (false) when they are accessed
                _lines_vertexbuffer_elementStride,                  // the byte offset between consecutive generic vertex attributes
                offset               // offset of the first component of the first generic vertex attribute
            );

            WriteLine(0, 0, _frustumWidth / 2, _frustumHeight / 2, new Vector4(1.0f, 0.0f, 0.0f, 1.0f), out int storedLineBytes);

            // end quads vao
            // ------------------------------------------------------------------------------------



            // ------------------------------------------------------------------------------------
            // desaturate render pass

            _desaturate_frameBuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _desaturate_frameBuffer);

            _desaturate_frameBuffer_texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _desaturate_frameBuffer_texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, _frameBufferWidth, _frameBufferHeight, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            _desaturate_frameBuffer_depthBuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _desaturate_frameBuffer_depthBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, _frameBufferWidth, _frameBufferHeight);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _desaturate_frameBuffer_depthBuffer);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, _desaturate_frameBuffer_texture, 0);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                throw new Exception("Framebuffer not complete");

            _desaturate_program = Shaders.Load(
                Resources.Path("Desaturate.vert"),
                Resources.Path("Desaturate.frag"));
            _desaturate_program_sourceLocation = GL.GetUniformLocation(_desaturate_program, "u_source");

            // end desaturate render pass
            // ------------------------------------------------------------------------------------

            // ------------------------------------------------------------------------------------
            // horizontal blur render pass

            _horizontalBlur_framebuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _horizontalBlur_framebuffer);

            _horizontalBlur_framebuffer_texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _horizontalBlur_framebuffer_texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, _frameBufferWidth, _frameBufferHeight, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, _horizontalBlur_framebuffer_texture, 0);

            _horizontalBlur_framebuffer_depthBuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _horizontalBlur_framebuffer_depthBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, _frameBufferWidth, _frameBufferHeight);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _horizontalBlur_framebuffer_depthBuffer);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                throw new Exception("Framebuffer not complete");

            _horizontalBlur_program = Shaders.Load(
                Resources.Path("HorizontalGaussianBlurShader.vert"),
                Resources.Path("HorizontalGaussianBlurShader.frag"));

            _horizontalBlur_program_sourceLocation = GL.GetUniformLocation(_horizontalBlur_program, "u_source");
            _horizontalBlur_program_resolutionLocation = GL.GetUniformLocation(_horizontalBlur_program, "u_resolution");
            _horizontalBlur_program_radiusLocation = GL.GetUniformLocation(_horizontalBlur_program, "u_radius");
            _horizontalBlur_program_modelViewProjectionMatrixMatrixLocation = GL.GetUniformLocation(_horizontalBlur_program, "u_modelViewProjectionMatrix");

            // end horizontal blur render pass
            // ------------------------------------------------------------------------------------

            // ------------------------------------------------------------------------------------
            // box render pass

            //_box_verticeCount = 36;

            //float[] g_box_vertex_buffer_data = [
            //    // back
            //   1.0f, -1.0f,  -1.0f, // left bottom
            //   -1.0f,  1.0f,  -1.0f, // right top
            //   1.0f,  1.0f,  -1.0f, // left top
            //   1.0f, -1.0f,  -1.0f, // left bottom
            //   -1.0f, -1.0f,  -1.0f, // right bottom
            //   -1.0f,  1.0f,  -1.0f, // right top

            //    // front
            //   -1.0f, -1.0f,  1.0f, // left bottom
            //    1.0f,  1.0f,  1.0f, // right top
            //   -1.0f,  1.0f,  1.0f, // left top
            //   -1.0f, -1.0f,  1.0f, // left bottom
            //    1.0f, -1.0f,  1.0f, // right bottom
            //    1.0f,  1.0f,  1.0f, // right top

            //    // right side
            //    1.0f, -1.0f,  1.0f, // left bottom
            //    1.0f,  1.0f,  -1.0f, // right top
            //    1.0f,  1.0f,  1.0f, // left top
            //    1.0f, -1.0f,  1.0f, // left bottom
            //    1.0f, -1.0f,  -1.0f, // right bottom
            //    1.0f,  1.0f,  -1.0f, // right top

            //    // left side
            //    -1.0f, -1.0f,  -1.0f, // left bottom
            //    -1.0f,  1.0f,  1.0f, // right top
            //    -1.0f,  1.0f,  -1.0f, // left top
            //    -1.0f, -1.0f,  -1.0f, // left bottom
            //    -1.0f, -1.0f,  1.0f, // right bottom
            //    -1.0f,  1.0f,  1.0f, // right top

            //    // top
            //   -1.0f, 1.0f,  1.0f, // left bottom
            //    1.0f,  1.0f,  -1.0f, // right top
            //   -1.0f,  1.0f,  -1.0f, // left top
            //   -1.0f, 1.0f,  1.0f, // left bottom
            //    1.0f, 1.0f,  1.0f, // right bottom
            //    1.0f,  1.0f,  -1.0f, // right top

            //    // bottom
            //   -1.0f, -1.0f,  -1.0f, // left bottom
            //    1.0f,  -1.0f,  1.0f, // right top
            //   -1.0f,  -1.0f,  1.0f, // left top
            //   -1.0f, -1.0f,  -1.0f, // left bottom
            //    1.0f, -1.0f,  -1.0f, // right bottom
            //    1.0f,  -1.0f,  1.0f, // right top
            //];

            //float[] g_box_uv_buffer_data = [

            //    0.0f,  0.0f, // left bottom
            //    1.0f,  1.0f, // right top
            //    0.0f,  1.0f, // left top
            //    0.0f,  0.0f, // left bottom
            //    1.0f,  0.0f, // right bottom
            //    1.0f,  1.0f, // right top

            //    0.0f,  0.0f, // left bottom
            //    1.0f,  1.0f, // right top
            //    0.0f,  1.0f, // left top
            //    0.0f,  0.0f, // left bottom
            //    1.0f,  0.0f, // right bottom
            //    1.0f,  1.0f, // right top

            //    0.0f,  0.0f, // left bottom
            //    1.0f,  1.0f, // right top
            //    0.0f,  1.0f, // left top
            //    0.0f,  0.0f, // left bottom
            //    1.0f,  0.0f, // right bottom
            //    1.0f,  1.0f, // right top

            //    0.0f,  0.0f, // left bottom
            //    1.0f,  1.0f, // right top
            //    0.0f,  1.0f, // left top
            //    0.0f,  0.0f, // left bottom
            //    1.0f,  0.0f, // right bottom
            //    1.0f,  1.0f, // right top

            //    0.0f,  0.0f, // left bottom
            //    1.0f,  1.0f, // right top
            //    0.0f,  1.0f, // left top
            //    0.0f,  0.0f, // left bottom
            //    1.0f,  0.0f, // right bottom
            //    1.0f,  1.0f, // right top

            //    0.0f,  0.0f, // left bottom
            //    1.0f,  1.0f, // right top
            //    0.0f,  1.0f, // left top
            //    0.0f,  0.0f, // left bottom
            //    1.0f,  0.0f, // right bottom
            //    1.0f,  1.0f, // right top
            //];

            //_box_vao = GL.GenVertexArray();
            //GL.BindVertexArray(_box_vao);

            //_box_xyzbuffer = GL.GenBuffer();
            //GL.EnableVertexAttribArray(0);
            //GL.BindBuffer(BufferTarget.ArrayBuffer, _box_xyzbuffer);
            //GL.BufferData(BufferTarget.ArrayBuffer, g_box_vertex_buffer_data.Length * sizeof(float), g_box_vertex_buffer_data, BufferUsageHint.StaticDraw);
            //GL.VertexAttribPointer(
            //    0,                  // layout 0
            //    3,                  // size
            //    VertexAttribPointerType.Float,           // type
            //    false,              // normalized?
            //    0,                  // stride
            //    0                   // array buffer offset
            //);

            //_box_uvbuffer = GL.GenBuffer();
            //GL.EnableVertexAttribArray(1);
            //GL.BindBuffer(BufferTarget.ArrayBuffer, _box_uvbuffer);
            //GL.BufferData(BufferTarget.ArrayBuffer, g_box_uv_buffer_data.Length * sizeof(float), g_box_uv_buffer_data, BufferUsageHint.StaticDraw);
            //GL.VertexAttribPointer(
            //    1,                  // layout 1
            //    2,                  // size
            //    VertexAttribPointerType.Float,           // type
            //    false,              // normalized?
            //    0,                  // stride
            //    0                   // array buffer offset
            //);

            //_box_programID = Shaders.Load(
            //    ResourcePath("TransformVertexShader.vert"),
            //    ResourcePath("Passthrough.frag"));

            //_box_matrixID = GL.GetUniformLocation(_box_programID, "MVP");

            // end box render pass
            // ------------------------------------------------------------------------------------

            // ------------------------------------------------------------------------------------
            // fullscreen quad render pass

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

            _fullscreenTriangle_vao = GL.GenVertexArray();
            GL.BindVertexArray(_fullscreenTriangle_vao);

            //GLuint quad_vertexbuffer;
            _fullscreenTriangle_xyzBffer = GL.GenBuffer();
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _fullscreenTriangle_xyzBffer);
            GL.BufferData(BufferTarget.ArrayBuffer, g_quad_vertex_buffer_data.Length * sizeof(float), g_quad_vertex_buffer_data, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(
                0,                  // layout 0
                3,                  // size
                VertexAttribPointerType.Float,           // type
                false,              // normalized?
                0,                  // stride
                0                   // array buffer offset
            );

            _fullscreenTriangle_uvBuffer = GL.GenBuffer();
            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _fullscreenTriangle_uvBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, g_quad_uv_buffer_data.Length * sizeof(float), g_quad_uv_buffer_data, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(
                1,                  // layout 1
                2,                  // size
                VertexAttribPointerType.Float,           // type
                false,              // normalized?
                0,                  // stride
                0                   // array buffer offset
            );

            // Create and compile our GLSL program from the shaders
            _fullscreenTriangle_program = Shaders.Load(
                Resources.Path("FullScreenQuad.vert"),
                Resources.Path("FullScreenQuad.frag"));
            _fullscreenTriangle_program_texLocation = GL.GetUniformLocation(_fullscreenTriangle_program, "u_source");

            // end fullscreen quad render pass
            // ------------------------------------------------------------------------------------
        }

        // This function runs on every update frame.
        protected void RenderFrameInternal(FrameEventArgs e)
        {
            // ------------------------------------------------------------------------------------


            // ------------------------------------------------------------------------------------
            // render suzanne to frame buffer 0

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _render_frameBuffer);

            GL.Viewport(0, 0, _frameBufferWidth, _frameBufferHeight);

            GL.ClearColor(1.0f, 168.0f / 225.0f, 0.0f, 0.0f);

            if(_enableClearingFrameBuffer)
            {
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            }

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            // Use our shader
            //GL.UseProgram(_suzanne_program);

            // Compute the MVP matrix from keyboard and mouse input
            var modelMatrix = Matrix4.Identity;
            var viewMatrix = Matrix4.LookAt(
                new Vector3(4, 3, 3), // Camera is at (4,3,3), in World Space
                Vector3.Zero,         // and looks at the origin
                Vector3.UnitY         // Head is up (set to 0,-1,0 to look upside-down)
            );
            var projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), CameraAspectRatio, 0.1f, 100.0f);
            var mvp = modelMatrix * viewMatrix * projectionMatrix;

            //// Send our transformation to the currently bound shader, 
            //// in the "MVP" uniform
            //GL.UniformMatrix4(_suzanne_program_modelViewProjectionMatrixLocation, false, ref mvp);
            //GL.UniformMatrix4(_suzanne_program_modelMatrixLocation, false, ref modelMatrix);
            //GL.UniformMatrix4(_suzanne_program_viewMatrixLocation, false, ref viewMatrix);

            //var lightPos = new Vector3(4, 4, 4);
            //GL.Uniform3(_suzanne_program_lightLocation, lightPos.X, lightPos.Y, lightPos.Z);

            //GL.ActiveTexture(TextureUnit.Texture0);
            //GL.BindTexture(TextureTarget.Texture2D, _suzanne_Texture);

            //GL.Uniform1(_suzanne_program_textureLocation, 0);

            //GL.BindVertexArray(_suzanne_vao);

            //GL.DrawElements(
            //    PrimitiveType.Triangles,            // mode
            //    _suzanne_IndexCount,                         // count
            //    DrawElementsType.UnsignedShort,     // type
            //    0);                                 // element array buffer offset

            // end render suzanne to frame buffer 0
            // ------------------------------------------------------------------------------------

            // ------------------------------------------------------------------------------------
            // render quads

            modelMatrix = _originMatrix * Matrix4.Identity;
            viewMatrix = Matrix4.LookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.UnitY);
            projectionMatrix = Matrix4.CreateOrthographic(_frustumWidth, _frustumWidth, CameraNear, CameraFar);
            mvp = modelMatrix * viewMatrix * projectionMatrix;

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _render_frameBuffer);

            GL.Viewport(0, 0, _frameBufferWidth, _frameBufferHeight);

            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            GL.BindVertexArray(_quads_vao);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.DisableVertexAttribArray(2);

            GL.UseProgram(_quads_program);
            GL.UniformMatrix4(_quads_program_modelViewProjectionMatrixLocation, false, ref mvp);

            GL.DrawArrays(
                OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,
                0,
                _quads_vertexbuffer_usedVertices);

            // end render quads
            // ------------------------------------------------------------------------------------


            // ------------------------------------------------------------------------------------
            // render lines

            modelMatrix = _originMatrix * Matrix4.Identity;
            viewMatrix = Matrix4.LookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.UnitY);
            projectionMatrix = Matrix4.CreateOrthographic(_frustumWidth, _frustumWidth, CameraNear, CameraFar);
            mvp = modelMatrix * viewMatrix * projectionMatrix;

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _render_frameBuffer);

            GL.Viewport(0, 0, _frameBufferWidth, _frameBufferHeight);

            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            GL.BindVertexArray(_lines_vao);

            //GL.EnableVertexAttribArray(0);
            //GL.EnableVertexAttribArray(1);

            GL.UseProgram(_lines_program);
            GL.UniformMatrix4(_lines_program_modelViewProjectionMatrixLocation, false, ref mvp);

            GL.DrawArrays(
                OpenTK.Graphics.OpenGL4.PrimitiveType.Lines,
                0,
                _lines_vertexbuffer_usedVertices);

            // end render quads
            // ------------------------------------------------------------------------------------


            // ------------------------------------------------------------------------------------
            // render lines

            modelMatrix = _originMatrix * Matrix4.Identity;
            viewMatrix = Matrix4.LookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.UnitY);
            projectionMatrix = Matrix4.CreateOrthographic(_frustumWidth, _frustumWidth, CameraNear, CameraFar);
            mvp = modelMatrix * viewMatrix * projectionMatrix;

            var color = new [] { 1f, 0f, 0f };

            Graphics.Lines
                .Camera()
                .Transform(ref modelMatrix)
                .Color(color)  
                .Color(new [] { 1f, 0f, 0f, 1f })
                .From(new[] { 0f, 0f, 0f })
                .To(new[] { 1f, 0f, 0f })
                .From(new[] { 0f, 1f, 0f })
                .To(new[] { 1f, 1f, 0f })
                .From(new[] { -1f, 1f, 0f })
                .To(new[] { 1f, 1f, 0f })
                .Color(new[] { 1f, 0f, 1f, 1f })
                .To(new[] { 1f, 0f, 0f }) 
                .To(new[] { 0f, 0f, 0f })
                .Close()
                .To(new[] { 1f, 0f, 0f })
                .To(new[] { 0f, 0f, 0f });

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _render_frameBuffer);

            GL.Viewport(0, 0, _frameBufferWidth, _frameBufferHeight);

            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            GL.BindVertexArray(_lines_vao);

            //GL.EnableVertexAttribArray(0);
            //GL.EnableVertexAttribArray(1);

            GL.UseProgram(_lines_program);
            GL.UniformMatrix4(_lines_program_modelViewProjectionMatrixLocation, false, ref mvp);

            GL.DrawArrays(
                OpenTK.Graphics.OpenGL4.PrimitiveType.Lines,
                0,
                _lines_vertexbuffer_usedVertices);

            // ------------------------------------------------------------------------------------

            var frameBuffer_texture = _render_frameBuffer_texture;

            // ------------------------------------------------------------------------------------
            // random pass

            if(_renderRandomPass)
            {
                frameBuffer_texture = _random_frameBuffer_texture;

                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _random_frameBuffer);

                GL.Viewport(0, 0, _frameBufferWidth, _frameBufferHeight);
                GL.ClearColor(255.0f / 255.0f, 255.0f / 255.0f, 255.0f / 255.0f, 0.0f);
                GL.Clear(ClearBufferMask.ColorBufferBit);

                GL.Disable(EnableCap.DepthTest);
                GL.Disable(EnableCap.CullFace);

                GL.UseProgram(_random_program);
                //GL.Uniform1(_desaturate_program_sourceLocation, 0);

                GL.BindVertexArray(_fullscreenTriangle_vao);

                GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, 3);


                if (FrameCount == 8 && OperatingSystem.IsWindows())
                {
                    Save(_random_frameBuffer, _frameBufferWidth, _frameBufferHeight);
                }

            }

            // end random pass
            // ------------------------------------------------------------------------------------


            // ------------------------------------------------------------------------------------
            // noise pass

            if (_renderNoisePass)
            {
                frameBuffer_texture = _noise_frameBuffer_texture;

                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _noise_frameBuffer);

                GL.Viewport(0, 0, _frameBufferWidth, _frameBufferHeight);
                GL.ClearColor(255.0f / 255.0f, 255.0f / 255.0f, 255.0f / 255.0f, 0.0f);
                GL.Clear(ClearBufferMask.ColorBufferBit);

                GL.Disable(EnableCap.DepthTest);
                GL.Disable(EnableCap.CullFace);

                GL.UseProgram(_noise_program);
                GL.Uniform1(_noise_program_octavesLocation, 4);
                GL.Uniform1(_noise_program_amplitudeLocation, 1.0f);
                GL.Uniform1(_noise_program_frequencyLocation, 1.0f);
                GL.Uniform1(_noise_program_lacunarityLocation, 2.0f);
                GL.Uniform1(_noise_program_persistenceLocation, 0.5f);
                GL.Uniform4(_noise_program_inputScaleLocation, new Vector4(4.782981f, 4.782981f, 4.782981f, 4.782981f));
                GL.Uniform4(_noise_program_inputOffsetLocation, new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
                GL.Uniform1(_noise_program_biasLocation, 0.5f);
                GL.Uniform1(_noise_program_noiseTypeLocation, _noiseType);

                GL.BindVertexArray(_fullscreenTriangle_vao);

                GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, 3);


                if (FrameCount == 8 && OperatingSystem.IsWindows())
                {
                    Save(_noise_frameBuffer, _frameBufferWidth, _frameBufferHeight);
                }

            }

            // end noise pass
            // ------------------------------------------------------------------------------------


            // ------------------------------------------------------------------------------------
            // post processing passes

            if (_enablePostProcessing)
            {

                // ------------------------------------------------------------------------------------
                // render desaturate pass to frame buffer 1

                frameBuffer_texture = _desaturate_frameBuffer;

                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _desaturate_frameBuffer);

                GL.Viewport(0, 0, _frameBufferWidth, _frameBufferHeight);
                GL.ClearColor(255.0f / 255.0f, 255.0f / 255.0f, 255.0f / 255.0f, 0.0f);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, _render_frameBuffer_texture);

                GL.UseProgram(_desaturate_program);
                GL.Uniform1(_desaturate_program_sourceLocation, 0);

                GL.BindVertexArray(_fullscreenTriangle_vao);

                GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, 3);

                // end render desaturate pass to frame buffer 1
                // ------------------------------------------------------------------------------------

                // ------------------------------------------------------------------------------------
                // horizontal blur pass

                frameBuffer_texture = _horizontalBlur_framebuffer;

                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _horizontalBlur_framebuffer);

                GL.Viewport(0, 0, _frameBufferWidth, _frameBufferHeight);
                GL.ClearColor(255.0f / 255.0f, 255.0f / 255.0f, 255.0f / 255.0f, 0.0f);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, _desaturate_frameBuffer_texture);

                var identity = Matrix4.Identity;

                GL.UseProgram(_horizontalBlur_program);
                GL.Uniform1(_horizontalBlur_program_sourceLocation, 0);
                GL.Uniform1(_horizontalBlur_program_radiusLocation, 8.0f);
                GL.Uniform2(_horizontalBlur_program_resolutionLocation, new Vector2((float)_windowWidth, (float)_windowHeight));
                GL.UniformMatrix4(_horizontalBlur_program_modelViewProjectionMatrixMatrixLocation, false, ref identity);

                GL.BindVertexArray(_fullscreenTriangle_vao);

                GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, 3);

                // end horizontal blur pass
                // ------------------------------------------------------------------------------------
            }

            // end post processing passes
            // ------------------------------------------------------------------------------------


            // ------------------------------------------------------------------------------------
            // render offscreen framebuffers to the screen

            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);

            GL.Viewport(0, 0, _windowWidth, _windowHeight);

            // no need to set the clear color or clear the buffer since every pixel will be overwritten en depth testing isn't needed
            //GL.ClearColor(251.0f / 255.0f, 204.0f / 255.0f, 122.0f / 255.0f, 0.0f);

            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, frameBuffer_texture);

            GL.UseProgram(_fullscreenTriangle_program);

            GL.Uniform1(_fullscreenTriangle_program_texLocation, 0);

            GL.BindVertexArray(_fullscreenTriangle_vao);

            GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, 3);

            // ------------------------------------------------------------------------------------


            // ------------------------------------------------------------------------------------
            // render fonts

            modelMatrix = _originMatrix * Matrix4.Identity;
            viewMatrix = Matrix4.LookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.UnitY);
            projectionMatrix = Matrix4.CreateOrthographic(_frustumWidth, _frustumWidth, CameraNear, CameraFar);
            mvp = modelMatrix * viewMatrix * projectionMatrix;

            GL.BindVertexArray(_font_vao);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _font_texture_2);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.UseProgram(_font_program);
            GL.UniformMatrix4(_font_program_modelViewProjectionMatrixLocation, false, ref mvp);

            GL.DrawArrays(
                OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,
                0,
                _font_vertexbuffer_usedVertices);

            GL.Disable(EnableCap.Blend);

            // end fonts
            // ------------------------------------------------------------------------------------


            // ------------------------------------------------------------------------------------
            // Swap buffers
            _internalWindow.SwapBuffers();

            // ------------------------------------------------------------------------------------
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

            if (KeyboardState.IsKeyPressed(Keys.C))
            {
                _enableClearingFrameBuffer = !_enableClearingFrameBuffer;
                Diagnostics.Log("Clear frame buffer: " + _enableClearingFrameBuffer);
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

            if (KeyboardState.IsKeyPressed(Keys.S) && OperatingSystem.IsWindows())
            {
                Save(_render_frameBuffer, _frameBufferWidth, _frameBufferHeight);
            }

            if (KeyboardState.IsKeyPressed(Keys.KeyPadAdd))
            {
                _noiseType++;

                if (_noiseType > 5)
                {
                    _noiseType = 0;
                }
            }

            if (KeyboardState.IsKeyPressed(Keys.KeyPadSubtract))
            {
                _noiseType--;

                if(_noiseType < 0)
                {
                    _noiseType = 5;
                }

            }

            _renderNewQuadTimer -= (float)e.Time;

            if(_renderNewQuadTimer <= 0f)
            { 
                _renderNewQuadTimer = _renderNewQuadTimeOut;

                ClearQuadVertexBuffer();

                var count = _defaultRandomGenerator.Value(1, 5);

                for(int i = 0; i < count; i++)
                {
                    var width = _defaultRandomGenerator.Value(0, _frustumWidth / 2);
                    var height = _defaultRandomGenerator.Value(0, _frustumHeight / 2);
                    var x = _defaultRandomGenerator.Value(0, _frustumWidth - width);
                    var y = _defaultRandomGenerator.Value(0, _frustumHeight - height);

                    var color = new Vector4(
                        (float)_defaultRandomGenerator.Value(),
                        (float)_defaultRandomGenerator.Value(),
                        (float)_defaultRandomGenerator.Value(),
                        1.0f);

                    WriteQuad(x, y, width, height, color, out int storedBytes);
                }

            }

            _renderNewLineTimer -= (float)e.Time;

            if (_renderNewLineTimer <= 0f)
            {
                _renderNewLineTimer = _renderNewLineTimeOut;

                var random = new Random();

                var xFrom = _defaultRandomGenerator.Value(0, _frustumWidth / 2);
                var yFrom = _defaultRandomGenerator.Value(0, _frustumHeight);
                var xTo = _defaultRandomGenerator.Value(_frustumWidth / 2, _frustumHeight);
                var yTo = _defaultRandomGenerator.Value(0, _frustumHeight);

                var color = new Vector4(
                    (float)random.NextDouble(),
                    (float)random.NextDouble(),
                    (float)random.NextDouble(),
                    1.0f);

                WriteLine(xFrom, yFrom, xTo, yTo, color, out int storedBytes);
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

            //GL.DeleteBuffer(_suzanne_xyzBuffer);
            //GL.DeleteBuffer(_suzanne_uvBuffer);
            //GL.DeleteBuffer(_suzanne_normalBuffer);
            //GL.DeleteBuffer(_suzanne_elementBuffer);
            //GL.DeleteProgram(_suzanne_program);
            //GL.DeleteTexture(_suzanne_Texture);

            GL.DeleteVertexArray(_render_vao);
            GL.DeleteFramebuffer(_render_frameBuffer);
            GL.DeleteTexture(_render_frameBuffer_texture);
            GL.DeleteRenderbuffer(_render_frameBuffer_depthBuffer);

            GL.DeleteVertexArray(_fullscreenTriangle_vao);
            GL.DeleteBuffer(_fullscreenTriangle_xyzBffer);
            GL.DeleteBuffer(_fullscreenTriangle_uvBuffer);

            //GL.DeleteVertexArray(_box_vao);
            //GL.DeleteBuffer(_box_xyzbuffer);
            //GL.DeleteBuffer(_box_uvbuffer);
            //GL.DeleteProgram(_box_programID);
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
                destination = Resources.Save($"{DateTime.Now.ToString("yyyyMMdd.HHmmss.ffff")}.png");
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

        //protected string ResourcePath(string resource)
        //{
        //    return System.IO.Path.Combine(ResourceDirectory, resource);
        //}

        //protected string ResourceShaderPath(string resource)
        //{
        //    if(string.IsNullOrEmpty(ShaderDirectory))
        //    {
        //        ShaderDirectory = Path.Combine(ResourceDirectory, ".shaders");
        //    }

        //    return System.IO.Path.Combine(ShaderDirectory, resource);
        //}

        //protected string ResourceFontPath(string resource)
        //{
        //    if (string.IsNullOrEmpty(FontDirectory))
        //    {
        //        FontDirectory = Path.Combine(ResourceDirectory, ".fonts");
        //    }

        //    return System.IO.Path.Combine(FontDirectory, resource);
        //}

        public void Focus()
        {
            _internalWindow?.Focus();
        }

        public void Run()
        {
            _internalWindow?.Run();
        }

        internal void ClearFontVertexBuffer()
        {
            GL.BindVertexArray(_font_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _font_vertexbuffer);
            GL.InvalidateBufferData(_font_vertexbuffer);

            _font_vertexbuffer_usedVertices = 0;
            _font_vertexbuffer_usedBytes = 0;
        }

        internal void ClearQuadVertexBuffer()
        {
            GL.BindVertexArray(_quads_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _quads_vertexbuffer);
            GL.InvalidateBufferData(_quads_vertexbuffer);

            _quads_vertexbuffer_usedVertices = 0;
            _quads_vertexbuffer_usedBytes = 0;
        }

        internal void ClearLineVertexBuffer()
        {
            GL.BindVertexArray(_lines_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _lines_vertexbuffer);
            GL.InvalidateBufferData(_lines_vertexbuffer);

            _lines_vertexbuffer_usedVertices = 0;
            _lines_vertexbuffer_usedBytes = 0;
        }

        internal void WriteQuad(float x, float y, float width, float height, Vector4 color, out int storedBytes)
        {
            float[] vertices = [
                // xyz          // rgba             // uv
                x,              y,          0.0f,   color.X, color.Y, color.Z, color.W,     0.0f, 0.0f, // left bottom
                x + width,      y,          0.0f,   color.X, color.Y, color.Z, color.W,     1.0f, 0.0f, // right bottom
                x + width,      y + height, 0.0f,   color.X, color.Y, color.Z, color.W,     1.0f, 1.0f, // right top

                x,              y,          0.0f,   color.X, color.Y, color.Z, color.W,     0.0f, 0.0f, // left bottom
                x + width,      y + height, 0.0f,   color.X, color.Y, color.Z, color.W,     1.0f, 1.0f, // right top
                x,              y + height, 0.0f,   color.X, color.Y, color.Z, color.W,     0.0f, 1.0f, // left top
            ];

            WriteQuad(ref vertices, out storedBytes);
        }

        internal void WriteQuad(ref float[] vertexdata, out int storedBytes)
        {
            storedBytes = vertexdata.Length * sizeof(float);

            if (_quads_vertexbuffer_usedBytes + storedBytes > _quads_vertexbuffer_totalBytes)
            {
                throw new Exception("WriteQuad(ref float[] vertexdata, out int vertexByteSize) failed. The vertex buffer isn't large enough to hold this vertex data.");
            }

            GL.BindVertexArray(_quads_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _quads_vertexbuffer);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)_quads_vertexbuffer_usedBytes, storedBytes, vertexdata);

            _quads_vertexbuffer_usedVertices += vertexdata.Length / _quads_vertexbuffer_elementCount;
            _quads_vertexbuffer_usedBytes += storedBytes;
        }

        internal void WriteLine(float xFrom, float yFrom, float xTo, float yTo, Vector4 color, out int storedBytes)
        {
            float[] vertices = [
                // xyz                  // rgba
                xFrom,  yFrom,  0.0f,   color.X, color.Y, color.Z, color.W,
                xTo,    yTo,    0.0f,   color.X, color.Y, color.Z, color.W,
            ];

            WriteLine(ref vertices, out storedBytes);
        }

        internal void WriteLine(ref float[] vertexdata, out int storedBytes)
        {
            storedBytes = vertexdata.Length * sizeof(float);

            if (_lines_vertexbuffer_usedBytes + storedBytes > _lines_vertexbuffer_totalBytes)
            {
                throw new Exception("WriteQuad(ref float[] vertexdata, out int vertexByteSize) failed. The vertex buffer isn't large enough to hold this vertex data.");
            }

            GL.BindVertexArray(_lines_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _lines_vertexbuffer);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)_lines_vertexbuffer_usedBytes, storedBytes, vertexdata);

            _lines_vertexbuffer_usedVertices += vertexdata.Length / _lines_vertexbuffer_elementCount;
            _lines_vertexbuffer_usedBytes += storedBytes;
        }

        internal void WriteText(
            string text, 
            float x, float y, 
            Vector4 color, 
            out int storedBytes)
        {
            storedBytes = 0;

            for (int n = 0; n < text.Length; n++)
            {
                var c = text[n];
                var entry = _font_Dictionary_2[c];

                float[] vertices = [
                    // xyz                                                                      // rgba                              // uv
                    x,                      y,                  0.0f,   color.X, color.Y, color.Z, color.W,     entry.u,    entry.v,     // left bottom
                    x + entry.width,        y,                  0.0f,   color.X, color.Y, color.Z, color.W,     entry.uu,   entry.v,     // right bottom
                    x + entry.width,        y + entry.height,   0.0f,   color.X, color.Y, color.Z, color.W,     entry.uu,   entry.vv,    // right top

                    x,                      y,                  0.0f,   color.X, color.Y, color.Z, color.W,     entry.u,    entry.v,     // left bottom
                    x + entry.width,        y + entry.height,   0.0f,   color.X, color.Y, color.Z, color.W,     entry.uu,   entry.vv,    // right top
                    x,                      y + entry.height,   0.0f,   color.X, color.Y, color.Z, color.W,     entry.u,    entry.vv,    // left top
                ];

                WriteText(ref vertices, out storedBytes);

                x += entry.width;
            }
        }

        internal void WriteText(ref float[] vertexdata, out int storedBytes)
        {
            storedBytes = vertexdata.Length * sizeof(float);

            if (_font_vertexbuffer_usedBytes + storedBytes > _font_vertexbuffer_totalBytes)
            {
                throw new Exception("WriteQuad(ref float[] vertexdata, out int vertexByteSize) failed. The vertex buffer isn't large enough to hold this vertex data.");
            }

            GL.BindVertexArray(_font_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _font_vertexbuffer);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)_font_vertexbuffer_usedBytes, storedBytes, vertexdata);

            _font_vertexbuffer_usedVertices += vertexdata.Length / _font_vertexbuffer_elementCount;
            _font_vertexbuffer_usedBytes += storedBytes;
        }

        public class FontSettings
        {
            public int MinCharacterIndex = 0;
            public int MaxCharacterIndex = 255;

            public int GlyphsPerLine = 32;
            public int GlyphLineCount = 16;
            public int GlyphWidth = 11;
            public int GlyphHeight = 22;

            public int CharXSpacing = 11;

            public static string TextLine1 = @"abcdefghijklmnopqrtstuvwxyz";
            public static string TextLine2 = @"ABCDEFGHIJKLMNOPQRTSTUVWXYZ";
            public static string TextLine3 = @".,;:?!()[]{}/|+-=_*""'$€£%@&";

            // Used to offset rendering glyphs to bitmap
            public int AtlasOffsetX = -3, AtlassOffsetY = -1;
            public int FontSize = 14;
            public string FontName = "Consolas";

            public int TextureWidth;
            public int TextureHeight;

        }

        [SupportedOSPlatform("windows")]
        void GenerateFontTextureAndDictionaryAndBindTexture(FontSettings settings)
        {
            _font_Dictionary.Clear();
            _font_Size = settings.FontSize;

            settings.TextureWidth = settings.GlyphsPerLine * settings.GlyphWidth;
            //settings.TextureHeight = settings.GlyphLineCount * settings.GlyphHeight;
            settings.TextureHeight = settings.GlyphHeight * (int)MathF.Ceiling((float)settings.MaxCharacterIndex / (float)settings.GlyphsPerLine);

            float u_step = (float)_font_Settings.GlyphWidth / (float)_font_Settings.TextureWidth;
            float v_step = u_step * ((float)_font_Settings.GlyphHeight / (float)_font_Settings.GlyphWidth);

            using (var bitmap = new System.Drawing.Bitmap(settings.TextureWidth, settings.TextureHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var font = new System.Drawing.Font(new System.Drawing.FontFamily(settings.FontName), settings.FontSize))
                {
                    using (var g = System.Drawing.Graphics.FromImage(bitmap))
                    {
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                        for (int p = 0; p < settings.GlyphLineCount; p++)
                        {
                            for (int n = 0; n < settings.GlyphsPerLine; n++)
                            {
                                if (p * settings.GlyphsPerLine + n >= settings.MaxCharacterIndex)
                                {
                                    break;
                                }

                                var index = n + p * settings.GlyphsPerLine;

                                char c = (char)index;

                                float u = (float)(index % settings.GlyphsPerLine) * u_step;
                                float v = (float)(index / settings.GlyphsPerLine) * v_step;

                                var entry = new FontDictionaryEntry
                                {
                                    width = settings.GlyphWidth,
                                    height = settings.GlyphHeight,
                                    u = u,
                                    v = v,
                                    uu = u_step,
                                    vv = v_step,
                                };

                                _font_Dictionary.Add(c, entry);

                                g.DrawString(c.ToString(), font, System.Drawing.Brushes.White, n * settings.GlyphWidth + settings.AtlasOffsetX, p * settings.GlyphHeight + settings.AtlassOffsetY);
                            }
                        }
                    }

                    _font_texture = GL.GenTexture();
                    GL.BindTexture(TextureTarget.Texture2D, _font_texture);
                    var data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                    bitmap.UnlockBits(data);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

                }

                bitmap.Save(Resources.Save(settings.FontName + ".png"));
            }
        }

        [SupportedOSPlatform("windows")]
        private Dictionary<Char, FontDictionaryEntry> GenerateFontTextureAndDictionaryAndBindTexture(FontSettings settings, out int textureHandle)
        {
            var fontDictionary = new Dictionary<Char, FontDictionaryEntry>();

            using (var fontCollection = new PrivateFontCollection())
            {
                System.Drawing.FontFamily fontFamily;

                if (System.IO.Path.HasExtension(settings.FontName))
                {
                    fontCollection.AddFontFile(settings.FontName);

                    fontFamily = fontCollection.Families[0];
                }
                else
                {
                    fontFamily = new System.Drawing.FontFamily(settings.FontName);
                }

                using (var font = new System.Drawing.Font(fontFamily, settings.FontSize))
                {
                    var additionalGlyphMarginLeftRight = 4;
                    var additionalGlyphMarginTopBottom = 8;
                    
                    var pixelToUvRatio = 1 / (float)settings.TextureWidth;

                    using (var bitmap = new System.Drawing.Bitmap(settings.TextureWidth, settings.TextureHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                    {
                        using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
                        {
                            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                            int index = 0;

                            for (int y = settings.TextureHeight - additionalGlyphMarginTopBottom; y > 0; )
                            {
                                var maxGlyphHeight = 0;

                                for (int x = additionalGlyphMarginLeftRight; x < settings.TextureWidth - additionalGlyphMarginLeftRight; )
                                {
                                    if (index > settings.MaxCharacterIndex)
                                    {
                                        break;
                                    }

                                    using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                                    {
                                        char c = (char)index;

                                        if (c == 'm')
                                        {
                                            var f = 0;
                                        }

                                        path.AddString(c.ToString(), fontFamily, 0, settings.FontSize, Point.Empty, StringFormat.GenericTypographic);

                                        var rectangle = Rectangle.Ceiling(path.GetBounds());

                                        if(maxGlyphHeight != 0 && rectangle.Height + rectangle.Y != maxGlyphHeight)
                                        {
                                            var t = 0;
                                        }

                                        maxGlyphHeight = Math.Max(maxGlyphHeight, rectangle.Height + rectangle.Y);

                                        if (x + rectangle.X + rectangle.Width + additionalGlyphMarginLeftRight >= settings.TextureWidth - additionalGlyphMarginLeftRight)
                                        {
                                            x = settings.TextureWidth;

                                            continue;
                                        }

                                        graphics.TranslateTransform(x, settings.TextureHeight - y);

                                        graphics.FillPath(Brushes.White, path);

                                        graphics.ResetTransform();

                                        var u = (float)x * pixelToUvRatio;
                                        var uu = (float)(x + rectangle.Width + rectangle.X) * pixelToUvRatio;
                                        var v = (float)(y - rectangle.Height - rectangle.Y) * pixelToUvRatio;
                                        var vv = (float)y * pixelToUvRatio;

                                        var entry = new FontDictionaryEntry
                                        {
                                            width = rectangle. X + rectangle.Width,
                                            height = rectangle.Y + rectangle.Height,
                                            u = u,
                                            v = v,
                                            uu = uu,
                                            vv = vv,
                                        };

                                        fontDictionary.Add(c, entry);

                                        x += rectangle.X + rectangle.Width + additionalGlyphMarginLeftRight;

                                        index++;
                                    }
                                }

                                y -= maxGlyphHeight - additionalGlyphMarginTopBottom;
                            }
                        }

                        bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

                        textureHandle = GL.GenTexture();
                        GL.BindTexture(TextureTarget.Texture2D, textureHandle);
                        var data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                        bitmap.UnlockBits(data);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

                        bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                        bitmap.Save(Resources.Texture(font.Name + "-add.png"));
                    }
                }
            }

            return fontDictionary;
        }
    }
}