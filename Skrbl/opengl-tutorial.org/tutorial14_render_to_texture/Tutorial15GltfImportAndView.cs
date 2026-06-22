
using Microsoft.Extensions.Configuration;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Skrbl;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace OpenGlTutorialOrg
{
    public class Tutorial15GltfImportAndView : IDisposable
    {
        GameWindow _internalWindow;

        Resources Resources;

        bool CaptureInput = false;
        Vector2 CapturedMousePosition;

        //bool _renderToFullScreenQuad = true;
        //bool _enablePostProcessing = true;
        double TotalTime;
        int FrameCount;

        int _suzanne_vao;
        int _suzanne_xyzBuffer;
        int _suzanne_uvBuffer;
        int _suzanne_normalBuffer;
        int _suzanne_elementBuffer;
        int _suzanne_Texture;
        int _suzanne_program;
        int _suzanne_program_textureLocation;
        int _suzanne_program_modelViewProjectionMatrixLocation;
        int _suzanne_program_viewMatrixLocation;
        int _suzanne_program_modelMatrixLocation;
        int _suzanne_program_lightLocation;
        //int _suzanne_frameBuffer;
        //int _suzanne_frameBuffer_texture;
        //int _suzanne_frameBuffer_depthBuffer;
        int _suzanne_IndexCount;

        private Mesh _mesh;
        private Camera _camera;

        int _box_vao;
        int _box_xyzbuffer;
        int _box_uvbuffer;
        int _box_programID;
        int _box_matrixID;
        int _box_verticeCount = 6;
        float _box_scale = 200;

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
        private float _suzanne_rotation_angle;
        private float _suzanne_rotation_speed = MathF.PI * 0.25f;

        internal class SketchSettings
        {
            public float FrustumWidth { get; set; } = 1024.0f;
            public float FrustumHeight { get; set; } = 1024.0f;
            public float FrameBufferScale { get; set; } = 4.0f;
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

        internal class Graphics
        {
            internal List<RenderMesh> _renderMeshes = new List<RenderMesh>();
            internal List<RenderMaterial> _renderMaterials = new List<RenderMaterial>();

            public int Width { get; private set; }
            public int Height { get; private set; }

            internal Graphics(int width, int height)
            {
                Width = width;
                Height = height;
            }

            internal class RenderMesh
            {
                internal Mesh _mesh { get; private set; }

                internal RenderMaterial _renderMaterial { get; set; }

                internal int _vao { get; private set; }

                internal int[] _buffers;

                public RenderMesh(Mesh mesh, Graphics graphics)
                {
                    _mesh = mesh;

                    _vao = GL.GenVertexArray();

                    GL.BindVertexArray(_vao);

                    var layout = 0;

                    _buffers = new int[_mesh.Attributes.Count];

                    foreach (var attribute in _mesh.Attributes)
                    {
                        _buffers[layout] = GL.GenBuffer();
                        
                        GL.EnableVertexAttribArray(layout);

                        GL.BindBuffer(BufferTarget.ArrayBuffer, _buffers[layout]);
                        GL.BufferData(BufferTarget.ArrayBuffer, attribute.Array.Length, attribute.Array, BufferUsageHint.StaticDraw);

                        GL.VertexAttribPointer(
                            layout,                         // layout 0
                            attribute.Count,                // size
                            ToVertexAttribPointerType(attribute.Type),  // type
                            attribute.Normalized,           // normalized?
                            0,                              // stride
                            0                               // array buffer offset
                        );

                        layout++;
                    }

                    if(_mesh.Material != null)
                    {
                        _renderMaterial = new RenderMaterial(_mesh.Material);
                        graphics._renderMaterials.Add(_renderMaterial);
                    }
                }
            }
            internal class RenderMaterial
            {
                internal Material _material { get; private set; }

                internal int _handle { get; private set; }

                private readonly Dictionary<string, int> _uniformLocations;
                private readonly Dictionary<string, int> _attributeLocations;

                public RenderMaterial(Material material)
                {
                    _material = material;

                    _handle = Shaders.Load(material.VertexShaderUri, material.FragmentShaderUri);


                    // First, we have to get the number of active uniforms in the shader.
                    GL.GetProgram(_handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

                    // Next, allocate the dictionary to hold the locations.
                    _uniformLocations = new Dictionary<string, int>();

                    // Loop over all the uniforms,
                    for (var i = 0; i < numberOfUniforms; i++)
                    {
                        // get the name of this uniform,
                        var key = GL.GetActiveUniform(_handle, i, out _, out _);

                        // get the location,
                        var location = GL.GetUniformLocation(_handle, key);

                        // and then add it to the dictionary.
                        _uniformLocations.Add(key, location);
                    }

                    GL.GetProgram(_handle, GetProgramParameterName.ActiveAttributes, out var numberOfAttributes);

                    // Next, allocate the dictionary to hold the locations.
                    _attributeLocations = new Dictionary<string, int>();

                    // Loop over all the uniforms,
                    for (var i = 0; i < numberOfAttributes; i++)
                    {
                        // get the name of this uniform,
                        var key = GL.GetActiveAttrib(_handle, i, out _, out _);

                        // get the location,
                        var location = GL.GetAttribLocation(_handle, key);

                        // and then add it to the dictionary.
                        _attributeLocations.Add(key, location);
                    }


                    //_box_matrixID = GL.GetUniformLocation(_handle, "MVP");
                }

                /// <summary>
                /// Set a uniform int on this shader.
                /// </summary>
                /// <param name="name">The name of the uniform</param>
                /// <param name="data">The data to set</param>
                public void Uniform(string name, int data)
                {
                    if (!_uniformLocations.ContainsKey(name))
                    {
                        return;
                    }
                    GL.Uniform1(_uniformLocations[name], data);
                }

                /// <summary>
                /// Set a uniform float on this shader.
                /// </summary>
                /// <param name="name">The name of the uniform</param>
                /// <param name="data">The data to set</param>
                public void Uniform(string name, float data)
                {
                    if (!_uniformLocations.ContainsKey(name))
                    {
                        return;
                    }
                    GL.Uniform1(_uniformLocations[name], data);
                }

                /// <summary>
                /// Set a uniform Matrix4 on this shader
                /// </summary>
                /// <param name="name">The name of the uniform</param>
                /// <param name="data">The data to set</param>
                /// <remarks>
                /// </remarks>
                public void Uniform(string name, Matrix4 data)
                {
                    if (!_uniformLocations.ContainsKey(name))
                    {
                        return;
                    }
                    GL.UniformMatrix4(_uniformLocations[name], true, ref data);
                }

                /// <summary>
                /// Set a uniform Matrix4 on this shader
                /// </summary>
                /// <param name="name">The name of the uniform</param>
                /// <param name="data">The data to set</param>
                /// <remarks>
                /// </remarks>
                public void Uniform(string name, ref Matrix4 data)
                {
                    if (!_uniformLocations.ContainsKey(name))
                    {
                        return;
                    }
                    GL.UniformMatrix4(_uniformLocations[name], true, ref data);
                }

                /// <summary>
                /// Set a uniform Matrix4 on this shader
                /// </summary>
                /// <param name="name">The name of the uniform</param>
                /// <param name="data">The data to set</param>
                /// <remarks>
                /// </remarks>
                public void Uniform(string name, Matrix3 data)
                {
                    if (!_uniformLocations.ContainsKey(name))
                    {
                        return;
                    }
                    GL.UniformMatrix3(_uniformLocations[name], true, ref data);
                }

                /// <summary>
                /// Set a uniform Matrix4 on this shader
                /// </summary>
                /// <param name="name">The name of the uniform</param>
                /// <param name="data">The data to set</param>
                /// <remarks>
                /// </remarks>
                public void Uniform(string name, ref Matrix3 data)
                {
                    if (!_uniformLocations.ContainsKey(name))
                    {
                        return;
                    }
                    GL.UniformMatrix3(_uniformLocations[name], true, ref data);
                }

                /// <summary>
                /// Set a uniform Vector3 on this shader.
                /// </summary>
                /// <param name="name">The name of the uniform</param>
                /// <param name="data">The data to set</param>
                public void Uniform(string name, Vector3 data)
                {
                    if (!_uniformLocations.ContainsKey(name))
                    {
                        return;
                    }
                    GL.Uniform3(_uniformLocations[name], data);
                }

                /// <summary>
                /// Set a uniform Vector3 on this shader.
                /// </summary>
                /// <param name="name">The name of the uniform</param>
                /// <param name="data">The data to set</param>
                public void Uniform(string name, ref Vector3 data)
                {
                    if (!_uniformLocations.ContainsKey(name))
                    {
                        return;
                    }
                    GL.Uniform3(_uniformLocations[name], data);
                }

                /// <summary>
                /// Set a uniform Vector3 on this shader.
                /// </summary>
                /// <param name="name">The name of the uniform</param>
                /// <param name="data">The data to set</param>
                public void Uniform(string name, float x, float y, float z)
                {
                    if (!_uniformLocations.ContainsKey(name))
                    {
                        return;
                    }
                    GL.Uniform3(_uniformLocations[name], x, y, z);
                }

                /// <summary>
                /// Set a uniform Vector3 on this shader.
                /// </summary>
                /// <param name="name">The name of the uniform</param>
                /// <param name="data">The data to set</param>
                public void Uniform(string name, Vector4 data)
                {
                    if (!_uniformLocations.ContainsKey(name))
                    {
                        return;
                    }
                    GL.Uniform4(_uniformLocations[name], data);
                }

                /// <summary>
                /// Set a uniform Vector3 on this shader.
                /// </summary>
                /// <param name="name">The name of the uniform</param>
                /// <param name="data">The data to set</param>
                public void Uniform(string name, ref Vector4 data)
                {
                    if (!_uniformLocations.ContainsKey(name))
                    {
                        return;
                    }
                    GL.Uniform4(_uniformLocations[name], data);
                }

                /// <summary>
                /// Set a uniform Vector3 on this shader.
                /// </summary>
                /// <param name="name">The name of the uniform</param>
                /// <param name="data">The data to set</param>
                public void Uniform(string name, float x, float y, float z, float w)
                {
                    if (!_uniformLocations.ContainsKey(name))
                    {
                        return;
                    }
                    GL.Uniform4(_uniformLocations[name], x, y, z, w);
                }

                internal void Use()
                {
                    GL.UseProgram(_handle);
                }
            }

            private static VertexAttribPointerType ToVertexAttribPointerType(Skrbl.AttributeType attributeType)
            {
                switch (attributeType)
                {
                    case Skrbl.AttributeType.Float:
                        return VertexAttribPointerType.Float;
                    case Skrbl.AttributeType.Int32:
                        return VertexAttribPointerType.Int;
                    case Skrbl.AttributeType.Int13:
                        return VertexAttribPointerType.Short;
                    case Skrbl.AttributeType.UInt32:
                        return VertexAttribPointerType.UnsignedInt;
                    case Skrbl.AttributeType.UInt16:
                        return VertexAttribPointerType.UnsignedShort;
                    default:
                        throw new Exception($"Unsupported attribute type {attributeType}");
                }
            }

            public void Add(Scene scene)
            {

            }

            public void Add(Mesh mesh)
            {
                var m = new RenderMesh(mesh, this);

                _renderMeshes.Add(m);
            }

            public void Render(Scene scene, Camera camera)
            {

            }


            public void Render(Mesh mesh, Camera camera)
            {
                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);

                GL.Viewport(0, 0, Width, Height);

                GL.ClearColor(251.0f / 255.0f, 204.0f / 255.0f, 122.0f / 255.0f, 0.0f);

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                GL.Enable(EnableCap.DepthTest);
                GL.Enable(EnableCap.CullFace);

                var _renderMesh = _renderMeshes[0];
                var _renderMaterial = _renderMesh._renderMaterial;

                var modelMatrix = mesh.Transform.Matrix();

                var mvp = modelMatrix * camera.ViewMatrix() * camera.ProjectionMatrix();

                _renderMaterial.Use();
                _renderMaterial.Uniform("MVP", ref mvp);

                GL.BindVertexArray(_renderMesh._vao);

                GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, (int)mesh.Count);
            }

            public void Remove(Scene scene)
            {

            }

            public void Remove(Mesh mesh)
            {

            }
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

        internal Graphics _graphics;

        // A simple constructor to let us set properties like window size, title, FPS, etc. on the window.
        public Tutorial15GltfImportAndView(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
        {
            Resources = new Resources([
                "C:\\Steven\\Atelier\\Scrblr\\Skrbl", 
                "C:\\Steven\\Atelier\\Scrblr\\Skrbl\\opengl-tutorial.org\\tutorial14_render_to_texture"]);

            CalculateWindowSize();

            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile(Resources.Path($"{GetType().Name}.json"), optional: false, reloadOnChange: true)
                .Build();

            // Get a configuration section

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
                Title = "Tutorial 14 - Render To Texture - Extented 5",
                Flags = ContextFlags.ForwardCompatible | ContextFlags.Debug,     // needed to run on macos
                WindowBorder = WindowBorder.Hidden,
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

            _graphics = new Graphics(_internalWindow.ClientSize.X, _internalWindow.ClientSize.Y);

            _camera = new Camera();
            _camera.Position = (0, 0, z);
            _camera.AspectRatio = (float)_internalWindow.ClientSize.X / (float)_internalWindow.ClientSize.Y;
        }

        private void LoadInternal()
        {
            Diagnostics.EnableOpenGlDebugMessages([DebugSeverityControl.DebugSeverityHigh, DebugSeverityControl.DebugSeverityMedium, DebugSeverityControl.DebugSeverityLow]);

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

            _suzanne_vao = GL.GenVertexArray();
            GL.BindVertexArray(_suzanne_vao);

            // Create and compile our GLSL program from the shaders
            _suzanne_program = Shaders.Load(
                Resources.Path("StandardShadingRTT.vert"),
                Resources.Path("StandardShadingRTT.frag"));

            // Get a handle for our "MVP" uniform
            _suzanne_program_modelViewProjectionMatrixLocation = GL.GetUniformLocation(_suzanne_program, "MVP");
            _suzanne_program_viewMatrixLocation = GL.GetUniformLocation(_suzanne_program, "V");
            _suzanne_program_modelMatrixLocation = GL.GetUniformLocation(_suzanne_program, "M");
            _suzanne_program_textureLocation = GL.GetUniformLocation(_suzanne_program, "myTextureSampler");

            // Load the texture
            _suzanne_Texture = Textures.Load(Resources.Path("uvmap.png"));

            // Read our .obj file
            ObjLoader.Load(Resources.Path("suzanne.obj"), out var vertices, out var uvs, out var normals);

            VboIndexer.IndexTriangles(
                vertices, uvs, normals,
                out ushort[] indices,
                out Vector3[] indexed_vertices,
                out Vector2[] indexed_uvs,
                out Vector3[] indexed_normals);

            _suzanne_IndexCount = indices.Length;
            // Load it into a VBO

            _suzanne_xyzBuffer = GL.GenBuffer();
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _suzanne_xyzBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, indexed_vertices.Length * 3 * sizeof(float), indexed_vertices, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(
                0,                  // attribute 0. No particular reason for 0, but must match the layout in the shader.
                3,                  // size
                VertexAttribPointerType.Float,           // type
                false,              // normalized?
                0,                  // stride
                0                   // array buffer offset 
            );

            _suzanne_uvBuffer = GL.GenBuffer();
            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _suzanne_uvBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, indexed_uvs.Length * 2 * sizeof(float), indexed_uvs, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(
                1,                  // attribute. No particular reason for 1, but must match the layout in the shader.
                2,                  // size
                VertexAttribPointerType.Float,           // type
                false,              // normalized?
                0,                  // stride
                0                   // array buffer offset
            );

            _suzanne_normalBuffer = GL.GenBuffer();
            GL.EnableVertexAttribArray(2);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _suzanne_normalBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, indexed_normals.Length * 3 * sizeof(float), indexed_normals, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(
                2,                  // attribute. No particular reason for 1, but must match the layout in the shader.
                3,                  // size
                VertexAttribPointerType.Float,           // type
                false,              // normalized?
                0,                  // stride
                0                   // array buffer offset
            );

            // Generate a buffer for the indices as well
            _suzanne_elementBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _suzanne_elementBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(ushort), indices, BufferUsageHint.StaticDraw);



            // Get a handle for our "LightPosition" uniform
            GL.UseProgram(_suzanne_program);
            _suzanne_program_lightLocation = GL.GetUniformLocation(_suzanne_program, "LightPosition_worldspace");

            // The framebuffer, which regroups 0, 1, or more textures, and 0 or 1 depth buffer.
            //_suzanne_frameBuffer = GL.GenFramebuffer();
            //GL.BindFramebuffer(FramebufferTarget.Framebuffer, _suzanne_frameBuffer);

            // The texture we're going to render to
            //_suzanne_frameBuffer_texture = GL.GenTexture();

            // "Bind" the newly created texture : all future texture functions will modify this texture
            //GL.BindTexture(TextureTarget.Texture2D, _suzanne_frameBuffer_texture);

            // Give an empty image to OpenGL ( the last "0" means "empty" )
            //GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, _frameBufferWidth, _frameBufferHeight, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);

            // Poor filtering
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            // The depth buffer
            //_suzanne_frameBuffer_depthBuffer = GL.GenRenderbuffer();
            //GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _suzanne_frameBuffer_depthBuffer);
            //GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, _frameBufferWidth, _frameBufferHeight);
            //GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _suzanne_frameBuffer_depthBuffer);

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
            //GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, _suzanne_frameBuffer_texture, 0);

            //// Depth texture alternative : 
            ////glFramebufferTexture(GL_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, depthTexture, 0);
            //GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, depthTexture, 0);


            // Set the list of draw buffers.
            //GLenum DrawBuffers[1] = { GL_COLOR_ATTACHMENT0 };
            //glDrawBuffers(1, DrawBuffers); // "1" is the size of DrawBuffers
            //GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            //if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            //    throw new Exception("Framebuffer not complete");

            // end suzanne render pass
            // ------------------------------------------------------------------------------------

            //var gltfModel = Skrbl.Gltf.Import(new Skrbl.Gltf.ImportSettings
            //{
            //    FilePath = Resources.Scene("TriangleWithoutIndices/glTF/TriangleWithoutIndices.gltf"),
            //});


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

            byte[] g_box_vertex_buffer_byte_data = MemoryMarshal.AsBytes(g_box_vertex_buffer_data.AsSpan()).ToArray();
            byte[] g_box_uv_buffer_byte_data = MemoryMarshal.AsBytes(g_box_uv_buffer_data.AsSpan()).ToArray();

            _box_vao = GL.GenVertexArray();
            GL.BindVertexArray(_box_vao);

            _box_xyzbuffer = GL.GenBuffer();
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _box_xyzbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, g_box_vertex_buffer_byte_data.Length, g_box_vertex_buffer_byte_data, BufferUsageHint.StaticDraw);
            //GL.BufferData(BufferTarget.ArrayBuffer, g_box_vertex_buffer_data.Length * sizeof(float), g_box_vertex_buffer_data, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(
                0,                  // layout 0
                3,                  // size
                VertexAttribPointerType.Float,           // type
                false,              // normalized?
                0,                  // stride
                0                   // array buffer offset
            );

            _box_uvbuffer = GL.GenBuffer();
            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _box_uvbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, g_box_uv_buffer_byte_data.Length, g_box_uv_buffer_byte_data, BufferUsageHint.StaticDraw);
            //GL.BufferData(BufferTarget.ArrayBuffer, g_box_uv_buffer_data.Length * sizeof(float), g_box_uv_buffer_data, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(
                1,                  // layout 1
                2,                  // size
                VertexAttribPointerType.Float,           // type
                false,              // normalized?
                0,                  // stride
                0                   // array buffer offset
            );

            _box_programID = Shaders.Load(
                Resources.Path("TransformVertexShader.vert"),
                Resources.Path("Passthrough.frag"));

            _box_matrixID = GL.GetUniformLocation(_box_programID, "MVP");

            _mesh = new Mesh(g_box_vertex_buffer_data.Length, Skrbl.PrimitiveType.Triangles, new Material
            {
                VertexShaderUri = Resources.Path("TransformVertexShader.vert"),
                FragmentShaderUri = Resources.Path("Passthrough.frag"),
            });
            _mesh.AddAttribute(new Skrbl.Attribute("position", Skrbl.AttributeType.Float, g_box_vertex_buffer_byte_data, 3));
            _mesh.AddAttribute(new Skrbl.Attribute("uv0", Skrbl.AttributeType.Float, g_box_uv_buffer_byte_data, 2));

            _graphics.Add(_mesh);


            // end box render pass
            // ------------------------------------------------------------------------------------
        }

        // This function runs on every update frame.
        protected void RenderFrameInternal(FrameEventArgs e)
        {
            // ------------------------------------------------------------------------------------
            // render suzanne to frame buffer 0

            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);

            GL.Viewport(0, 0, _windowWidth, _windowHeight);

            GL.ClearColor(251.0f / 255.0f, 204.0f / 255.0f, 122.0f / 255.0f, 0.0f);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            //// Use our shader
            //GL.UseProgram(_suzanne_program);

            //// Compute the MVP matrix from keyboard and mouse input
            //var modelMatrix = Matrix4.CreateRotationY(_suzanne_rotation_angle);
            //var viewMatrix = Matrix4.LookAt(
            //    new Vector3(4, 3, 3), // Camera is at (4,3,3), in World Space
            //    Vector3.Zero,         // and looks at the origin
            //    Vector3.UnitY         // Head is up (set to 0,-1,0 to look upside-down)
            //);
            //var projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), CameraAspectRatio, 0.1f, 100.0f);
            //var mvp = modelMatrix * viewMatrix * projectionMatrix;

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
            //    OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,            // mode
            //    _suzanne_IndexCount,                         // count
            //    DrawElementsType.UnsignedShort,     // type
            //    0);                                 // element array buffer offset

            // end render suzanne to frame buffer 0
            // ------------------------------------------------------------------------------------

            GL.UseProgram(_box_programID);

            var modelMatrix = Matrix4.CreateScale(_box_scale) * Matrix4.CreateRotationY(_suzanne_rotation_angle);

            var mvp = modelMatrix * CameraViewMatrix * CameraProjectionMatrix;

            GL.UniformMatrix4(_box_matrixID, false, ref mvp);

            GL.BindVertexArray(_box_vao);

            GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, _box_verticeCount);

            _mesh.Transform.Scale.X = _box_scale;
            _mesh.Transform.Scale.Y = _box_scale;
            _mesh.Transform.Scale.Z = _box_scale;

            _mesh.Transform.Rotation.Y = _suzanne_rotation_angle;

            _graphics.Render(_mesh, _camera);

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

            //if (KeyboardState.IsKeyPressed(Keys.S) && OperatingSystem.IsWindows())
            //{
            //    Save(_suzanne_frameBuffer, _frameBufferWidth, _frameBufferHeight);
            //}

            _suzanne_rotation_angle += (float)e.Time * _suzanne_rotation_speed;

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

            GL.DeleteBuffer(_suzanne_xyzBuffer);
            GL.DeleteBuffer(_suzanne_uvBuffer);
            GL.DeleteBuffer(_suzanne_normalBuffer);
            GL.DeleteBuffer(_suzanne_elementBuffer);
            GL.DeleteProgram(_suzanne_program);
            GL.DeleteTexture(_suzanne_Texture);

            GL.DeleteVertexArray(_suzanne_vao);
            //GL.DeleteFramebuffer(_suzanne_frameBuffer);
            //GL.DeleteTexture(_suzanne_frameBuffer_texture);
            //GL.DeleteRenderbuffer(_suzanne_frameBuffer_depthBuffer);

            GL.DeleteVertexArray(_box_vao);
            GL.DeleteBuffer(_box_xyzbuffer);
            GL.DeleteBuffer(_box_uvbuffer);
            GL.DeleteProgram(_box_programID);
        }

        private void ResizeInternal(ResizeEventArgs a)
        {
            GL.Viewport(0, 0, _internalWindow.ClientSize.X, _internalWindow.ClientSize.Y);

            _camera.AspectRatio = (float)_internalWindow.ClientSize.X / (float)_internalWindow.ClientSize.Y;

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