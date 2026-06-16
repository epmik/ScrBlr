
using ObjParser;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Skrbl;
using System;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace OpenGlTutorialOrg
{
    // This is where all OpenGL code will be written.
    // OpenToolkit allows for several functions to be overriden to extend functionality; this is how we'll be writing code.
    public class Tutorial14WindowExtented03 : AbstractTutorialWindow
    {
        bool _renderToFullScreenQuad = false;
        int VertexArrayID;
        int _suzanneRenderPass_Program;
        int vertexbuffer;
        int uvbuffer;
        int normalbuffer;
        int elementbuffer;
        int _suzanneRenderPass_Program_ModelViewProjectionMatrixLocation;
        int _suzanneRenderPass_Program_ViewMatrixLocation;
        int _suzanneRenderPass_Program_ModelMatrixLocation;
        int _suzanneRenderPass_Texture;
        int _suzanneRenderPass_Program_TextureLocation;
        Controls Controls = new Controls();
        int indexCount;
        int _suzanneRenderPass_Program_LightLocation;
        int _suzanneRenderPass_FrameBuffer;
        int _framebuffer0Texture;
        int _framebuffer0DepthBuffer;

        int _desaturatePass_frameBuffer;
        int _desaturatePass_FrameBufferTexture;
        int _desaturatePass_FrameBufferDepthBuffer;

        int _fullscreenquad_vertexbuffer, _fullscreenquad_uvbuffer;
        int _fullscreenquad_programID;
        int _fullscreenquad_programTexLocation;

        int windowWidth = 1024;
        int windowHeight = 768;

        double TotalTime;

        int box_vertexbuffer;
        int box_uvbuffer;
        int box_programID;
        int box_matrixID;
        int box_verticeCount = 6;

        int _desaturatePass_Program;
        int _desaturatePass_ProgramSourceLocation;
        int _frameCount;

        int _filmicPass_Framebuffer;
        int _filmicPass_FramebufferTexture;
        int _filmicPass_FramebufferDepthBuffer;
        int _filmicPass_Program;
        int _filmicPass_Program_SourceLocation;
        int _filmicPass_Program_TimeLocation;

        int _horizontalBlurPass_Framebuffer;
        int _horizontalBlurPass_FramebufferTexture;
        int _horizontalBlurPass_FramebufferDepthBuffer;
        int _horizontalBlurPass_Program;
        int _horizontalBlurPass_Program_SourceLocation;
        int _horizontalBlurPass_Program_ResolutionLocation;
        int _horizontalBlurPass_Program_RadiusLocation;
        int _horizontalBlurPass_Program_ModelViewProjectionMatrixMatrixLocation;

        // A simple constructor to let us set properties like window size, title, FPS, etc. on the window.
        public Tutorial14WindowExtented03(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            Title = "Tutorial 14 - Render To Texture - Extented 3";
            ResourceDirectory = "opengl-tutorial.org/tutorial14_render_to_texture/";
        }

        unsafe protected override void OnLoad()
        {
            base.OnLoad();

            Diagnostics.EnableOpenGlDebugMessages();

            // We would expect width and height to be 1024 and 768
            windowWidth = 1024;
            windowHeight = 768;
            // But on MacOS X with a retina screen it'll be 1024*2 and 768*2, so we get the actual framebuffer size:
            // Needs 'unsafe' compiler flag
            //glfwGetFramebufferSize(window, &windowWidth, &windowHeight);
            GLFW.GetFramebufferSize(WindowPtr, out windowWidth, out windowHeight);

            MousePosition = new Vector2(1024 / 2, 768 / 2);

            // Dark blue background
            //glClearColor(0.0f, 0.0f, 0.4f, 0.0f);
            GL.ClearColor(0.0f, 0.0f, 0.4f, 0.0f);

            // Enable depth test
            //glEnable(GL_DEPTH_TEST);
            GL.Enable(EnableCap.DepthTest);
            // Accept fragment if it is closer to the camera than the former one
            //glDepthFunc(GL_LESS);
            GL.DepthFunc(DepthFunction.Less);

            // Cull triangles which normal is not towards the camera
            //glEnable(GL_CULL_FACE);
            GL.Enable(EnableCap.CullFace);

            // ------------------------------------------------------------------------------------
            // suzanne render pass

            VertexArrayID = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayID);

            // Create and compile our GLSL program from the shaders
            _suzanneRenderPass_Program = Shaders.Load(
                ResourcePath("StandardShadingRTT.vertexshader"),
                ResourcePath("StandardShadingRTT.fragmentshader"));

            // Get a handle for our "MVP" uniform
            _suzanneRenderPass_Program_ModelViewProjectionMatrixLocation = GL.GetUniformLocation(_suzanneRenderPass_Program, "MVP");
            _suzanneRenderPass_Program_ViewMatrixLocation = GL.GetUniformLocation(_suzanneRenderPass_Program, "V");
            _suzanneRenderPass_Program_ModelMatrixLocation = GL.GetUniformLocation(_suzanneRenderPass_Program, "M");

            // Load the texture
            _suzanneRenderPass_Texture = Textures.Load(ResourcePath("uvmap.png"));

            // Get a handle for our "myTextureSampler" uniform
            _suzanneRenderPass_Program_TextureLocation = GL.GetUniformLocation(_suzanneRenderPass_Program, "myTextureSampler");

            // Read our .obj file
            ObjLoader.Load(ResourcePath("suzanne.obj"), out var vertices, out var uvs, out var normals);

            VboIndexer.IndexTriangles(
                vertices, uvs, normals,
                out ushort[] indices,
                out Vector3[] indexed_vertices,
                out Vector2[] indexed_uvs,
                out Vector3[] indexed_normals);

            indexCount = indices.Length;
            // Load it into a VBO

            vertexbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, indexed_vertices.Length * 3 * sizeof(float), indexed_vertices, BufferUsageHint.StaticDraw);

            uvbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, uvbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, indexed_uvs.Length * 2 * sizeof(float), indexed_uvs, BufferUsageHint.StaticDraw);

            normalbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, normalbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, indexed_normals.Length * 3 * sizeof(float), indexed_normals, BufferUsageHint.StaticDraw);

            // Generate a buffer for the indices as well
            elementbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementbuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(ushort), indices, BufferUsageHint.StaticDraw);

            // Get a handle for our "LightPosition" uniform
            GL.UseProgram(_suzanneRenderPass_Program);
            _suzanneRenderPass_Program_LightLocation = GL.GetUniformLocation(_suzanneRenderPass_Program, "LightPosition_worldspace");

            // The framebuffer, which regroups 0, 1, or more textures, and 0 or 1 depth buffer.
            _suzanneRenderPass_FrameBuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _suzanneRenderPass_FrameBuffer);

            // The texture we're going to render to
            _framebuffer0Texture = GL.GenTexture();

            // "Bind" the newly created texture : all future texture functions will modify this texture
            GL.BindTexture(TextureTarget.Texture2D, _framebuffer0Texture);

            // Give an empty image to OpenGL ( the last "0" means "empty" )
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, windowWidth, windowHeight, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);

            // Poor filtering
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            // The depth buffer
            _framebuffer0DepthBuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _framebuffer0DepthBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, windowWidth, windowHeight);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _framebuffer0DepthBuffer);

            //// Alternative : Depth texture. Slower, but you can sample it later in your shader
            ////GLuint depthTexture;
            ////glGenTextures(1, &depthTexture);
            ////glBindTexture(GL_TEXTURE_2D, depthTexture);
            ////glTexImage2D(GL_TEXTURE_2D, 0,GL_DEPTH_COMPONENT24, 1024, 768, 0,GL_DEPTH_COMPONENT, GL_FLOAT, 0);
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
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, _framebuffer0Texture, 0);

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
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, windowWidth, windowHeight, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            _desaturatePass_FrameBufferDepthBuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _desaturatePass_FrameBufferDepthBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, windowWidth, windowHeight);
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

            _filmicPass_Framebuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _filmicPass_Framebuffer);

            _filmicPass_FramebufferTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _filmicPass_FramebufferTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, windowWidth, windowHeight, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            _filmicPass_FramebufferDepthBuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _filmicPass_FramebufferDepthBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, windowWidth, windowHeight);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _filmicPass_FramebufferDepthBuffer);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, _filmicPass_FramebufferTexture, 0);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                throw new Exception("Framebuffer not complete");

            _filmicPass_Program = Shaders.Load(
                ResourcePath("Filmic.vert"),
                ResourcePath("Filmic.frag"));

            _filmicPass_Program_SourceLocation = GL.GetUniformLocation(_filmicPass_Program, "u_source");
            //_filmicPass_Program_ProjectionMatrixLocation = GL.GetUniformLocation(_filmicPass_Program, "projectionMatrix");
            //_filmicPass_Program_ModelViewMatrixLocation = GL.GetUniformLocation(_filmicPass_Program, "modelViewMatrix");
            _filmicPass_Program_TimeLocation = GL.GetUniformLocation(_filmicPass_Program, "u_time");
            //_filmicPass_Program_DiffuseLocation = GL.GetUniformLocation(_filmicPass_Program, "u_diffuse");

            // end filmic render pass
            // ------------------------------------------------------------------------------------

            // ------------------------------------------------------------------------------------
            // horizontal blur render pass

            _horizontalBlurPass_Framebuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _horizontalBlurPass_Framebuffer);

            _horizontalBlurPass_FramebufferTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _horizontalBlurPass_FramebufferTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, windowWidth, windowHeight, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, _horizontalBlurPass_FramebufferTexture, 0);

            _horizontalBlurPass_FramebufferDepthBuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _horizontalBlurPass_FramebufferDepthBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, windowWidth, windowHeight);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _horizontalBlurPass_FramebufferDepthBuffer);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                throw new Exception("Framebuffer not complete");

            _horizontalBlurPass_Program = Shaders.Load(
                ResourcePath("HorizontalGaussianBlurShader.vert"),
                ResourcePath("HorizontalGaussianBlurShader.frag"));

            _horizontalBlurPass_Program_SourceLocation = GL.GetUniformLocation(_horizontalBlurPass_Program, "u_source");
            _horizontalBlurPass_Program_ResolutionLocation = GL.GetUniformLocation(_horizontalBlurPass_Program, "u_resolution");
            _horizontalBlurPass_Program_RadiusLocation = GL.GetUniformLocation(_horizontalBlurPass_Program, "u_radius");
            //_horizontalBlurPass_Program_ProjectionMatrixLocation = GL.GetUniformLocation(_horizontalBlurPass_Program, "projectionMatrix");
            //_horizontalBlurPass_Program_ModelMatrixLocation = GL.GetUniformLocation(_horizontalBlurPass_Program, "modelMatrix");
            //_horizontalBlurPass_Program_ViewMatrixLocation = GL.GetUniformLocation(_horizontalBlurPass_Program, "viewMatrix");
            _horizontalBlurPass_Program_ModelViewProjectionMatrixMatrixLocation = GL.GetUniformLocation(_horizontalBlurPass_Program, "u_modelViewProjectionMatrix");

            // end horizontal blur render pass
            // ------------------------------------------------------------------------------------

            // ------------------------------------------------------------------------------------
            // box render pass

            box_verticeCount = 36;

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

            box_vertexbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, box_vertexbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, g_box_vertex_buffer_data.Length * sizeof(float), g_box_vertex_buffer_data, BufferUsageHint.StaticDraw);

            box_uvbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, box_uvbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, g_box_uv_buffer_data.Length * sizeof(float), g_box_uv_buffer_data, BufferUsageHint.StaticDraw);

            box_programID = Shaders.Load(
                ResourcePath("TransformVertexShader.vertexshader"),
                ResourcePath("Passthrough.fragmentshader"));

            box_matrixID = GL.GetUniformLocation(box_programID, "MVP");

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
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            // ------------------------------------------------------------------------------------
            // render suzanne to frame buffer 0

            // Render to our framebuffer
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _suzanneRenderPass_FrameBuffer);

            GL.Viewport(0, 0, windowWidth, windowHeight); // Render on the whole framebuffer, complete from the lower left corner to the upper right

            // Dark blue background
            GL.ClearColor(0.0f, 0.0f, 0.4f, 0.0f);

            // Clear the screen
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Use our shader
            GL.UseProgram(_suzanneRenderPass_Program);

            // Compute the MVP matrix from keyboard and mouse input
            var ModelMatrix = Matrix4.Identity;
            var ViewMatrix = Matrix4.LookAt(
                new Vector3(4, 3, 3), // Camera is at (4,3,3), in World Space
                Vector3.Zero,         // and looks at the origin
                Vector3.UnitY         // Head is up (set to 0,-1,0 to look upside-down)
            );
            var MVP = ModelMatrix * ViewMatrix * Controls.ProjectionMatrix;

            // Send our transformation to the currently bound shader, 
            // in the "MVP" uniform
            GL.UniformMatrix4(_suzanneRenderPass_Program_ModelViewProjectionMatrixLocation, false, ref MVP);
            GL.UniformMatrix4(_suzanneRenderPass_Program_ModelMatrixLocation, false, ref ModelMatrix);
            GL.UniformMatrix4(_suzanneRenderPass_Program_ViewMatrixLocation, false, ref ViewMatrix);

            var lightPos = new Vector3(4, 4, 4);
            GL.Uniform3(_suzanneRenderPass_Program_LightLocation, lightPos.X, lightPos.Y, lightPos.Z);

            // Bind our texture in Texture Unit 0
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _suzanneRenderPass_Texture);

            // Set our "myTextureSampler" sampler to use Texture Unit 0
            //glUniform1i(TextureID, 0);
            GL.Uniform1(_suzanneRenderPass_Program_TextureLocation, 0);

            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexbuffer);
            GL.VertexAttribPointer(
                0,                  // attribute 0. No particular reason for 0, but must match the layout in the shader.
                3,                  // size
                VertexAttribPointerType.Float,           // type
                false,              // normalized?
                0,                  // stride
                0                   // array buffer offset 
            );

            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, uvbuffer);
            GL.VertexAttribPointer(
                1,                  // attribute. No particular reason for 1, but must match the layout in the shader.
                2,                  // size
                VertexAttribPointerType.Float,           // type
                false,              // normalized?
                0,                  // stride
                0                   // array buffer offset
            );

            GL.EnableVertexAttribArray(2);
            GL.BindBuffer(BufferTarget.ArrayBuffer, normalbuffer);
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
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementbuffer);

            GL.DrawElements(
                OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,            // mode
                indexCount,                         // count
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
            GL.Viewport(0, 0, windowWidth, windowHeight);
            GL.ClearColor(255.0f / 255.0f, 255.0f / 255.0f, 255.0f / 255.0f, 0.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(_desaturatePass_Program);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _framebuffer0Texture);
            
            GL.Uniform1(_fullscreenquad_programTexLocation, 0);

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

            //GL.UseProgram(_filmicPass_Program);
            //GL.ActiveTexture(TextureUnit.Texture0);
            //GL.BindTexture(TextureTarget.Texture2D, _desaturatePass_FrameBufferTexture);

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
            GL.Viewport(0, 0, windowWidth, windowHeight);
            GL.ClearColor(255.0f / 255.0f, 255.0f / 255.0f, 255.0f / 255.0f, 0.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(_horizontalBlurPass_Program);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _desaturatePass_FrameBufferTexture);

            var identity = Matrix4.Identity;

            GL.Uniform1(_horizontalBlurPass_Program_SourceLocation, 0);
            GL.Uniform1(_horizontalBlurPass_Program_RadiusLocation, 8.0f);
            GL.Uniform2(_horizontalBlurPass_Program_ResolutionLocation, new Vector2((float)windowWidth, (float)windowHeight));
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

            GL.Viewport(0, 0, windowWidth, windowHeight);

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
                GL.UseProgram(box_programID);

                MVP = ModelMatrix * Controls.ViewMatrix * Controls.ProjectionMatrix;

                GL.UniformMatrix4(box_matrixID, false, ref MVP);

                GL.EnableVertexAttribArray(0);
                GL.BindBuffer(BufferTarget.ArrayBuffer, box_vertexbuffer);
                GL.VertexAttribPointer(
                    0,                  // attribute 0. No particular reason for 0, but must match the layout in the shader.
                    3,                  // size
                    VertexAttribPointerType.Float,           // type
                    false,              // normalized?
                    0,                  // stride
                    0                   // array buffer offset
                );

                GL.EnableVertexAttribArray(1);
                GL.BindBuffer(BufferTarget.ArrayBuffer, box_uvbuffer);
                GL.VertexAttribPointer(
                    1,                  // attribute. No particular reason for 1, but must match the layout in the shader.
                    2,                  // size
                    VertexAttribPointerType.Float,           // type
                    false,              // normalized?
                    0,                  // stride
                    0                   // array buffer offset
                );

                GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, box_verticeCount);

                GL.DisableVertexAttribArray(0);
                GL.DisableVertexAttribArray(1);
            }

            if(_frameCount == 1 && OperatingSystem.IsWindows())
            {
                Save(_desaturatePass_frameBuffer, windowWidth, windowHeight);
                //Save(_filmicPass_Framebuffer, windowWidth, windowHeight);
                Save(_horizontalBlurPass_Framebuffer, windowWidth, windowHeight);
            }

            // Swap buffers
            SwapBuffers();
        }

        // This function runs on every update frame.
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            _frameCount++;

            base.OnUpdateFrame(e);

            if (!IsFocused) 
            {
                return;
            }

            TotalTime += e.Time;

            // Check if the ESC key was pressed or the window was closed
            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            if (KeyboardState.IsKeyPressed(Keys.Enter))
            {
                _renderToFullScreenQuad = !_renderToFullScreenQuad;
                Console.WriteLine("Render to screen quad: " + _renderToFullScreenQuad);
            }

            Controls.Update(this, (float)e.Time);
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            GL.DeleteBuffer(vertexbuffer);
            GL.DeleteBuffer(uvbuffer);
            GL.DeleteBuffer(normalbuffer);
            GL.DeleteBuffer(elementbuffer);
            GL.DeleteProgram(_suzanneRenderPass_Program);
            GL.DeleteTexture(_suzanneRenderPass_Texture);
            GL.DeleteVertexArray(VertexArrayID);

            GL.DeleteFramebuffer(_suzanneRenderPass_FrameBuffer);
            GL.DeleteTexture(_framebuffer0Texture);
            GL.DeleteRenderbuffer(_framebuffer0DepthBuffer);
            GL.DeleteBuffer(_fullscreenquad_vertexbuffer);
            GL.DeleteBuffer(_fullscreenquad_uvbuffer);
            GL.DeleteVertexArray(VertexArrayID);

            GL.DeleteBuffer(box_vertexbuffer);
            GL.DeleteBuffer(box_uvbuffer);
            GL.DeleteProgram(box_programID);
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
    }
}