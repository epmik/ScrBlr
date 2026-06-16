
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
using System.Runtime.InteropServices;

namespace OpenGlTutorialOrg
{
    // This is where all OpenGL code will be written.
    // OpenToolkit allows for several functions to be overriden to extend functionality; this is how we'll be writing code.
    public class Tutorial14WindowExtented02 : AbstractTutorialWindow
    {
        bool _renderToFullScreenQuad = false;
        int VertexArrayID;
        int programID;
        int vertexbuffer;
        int uvbuffer;
        int normalbuffer;
        int elementbuffer;
        int MatrixID;
        int ViewMatrixID;
        int ModelMatrixID;
        int Texture;
        int TextureID;
        Controls Controls = new Controls();
        int indexCount;
        int LightID;
        int _framebuffer0;
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
        int box_vertexbuffer;
        int box_uvbuffer;
        int box_programID;
        int box_matrixID;
        int box_verticeCount = 6;
        int _desaturatePass_Program;
        int _desaturatePass_ProgramSourceLocation;
        int _filmicPass_Framebuffer;
        int _filmicPass_FramebufferTexture;
        int _filmicPass_FramebufferDepthBuffer;
        int _filmicPass_Program;
        int _filmicPass_ProgramProjectionMatrixLocation;
        int _filmicPass_ProgramModelViewMatrixLocation;
        int _filmicPass_ProgramTimeLocation;
        int _filmicPass_ProgramDiffuseLocation;

        // A simple constructor to let us set properties like window size, title, FPS, etc. on the window.
        public Tutorial14WindowExtented02(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            Title = "Tutorial 14 - Render To Texture - Extented 2";
            ResourceDirectory = "opengl-tutorial.org/tutorial14_render_to_texture/";
        }

        unsafe protected override void OnLoad()
        {
            base.OnLoad();

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

            //GLuint VertexArrayID;
            //glGenVertexArrays(1, &VertexArrayID);
            //glBindVertexArray(VertexArrayID);
            VertexArrayID = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayID);

            // Create and compile our GLSL program from the shaders
            //GLuint programID = LoadShaders("StandardShading.vertexshader", "StandardShading.fragmentshader");
            programID = Shaders.Load(
                ResourcePath("StandardShadingRTT.vertexshader"),
                ResourcePath("StandardShadingRTT.fragmentshader"));

            // Get a handle for our "MVP" uniform
            //GLuint MatrixID = glGetUniformLocation(programID, "MVP");
            //GLuint ViewMatrixID = glGetUniformLocation(programID, "V");
            //GLuint ModelMatrixID = glGetUniformLocation(programID, "M");
            MatrixID = GL.GetUniformLocation(programID, "MVP");
            ViewMatrixID = GL.GetUniformLocation(programID, "V");
            ModelMatrixID = GL.GetUniformLocation(programID, "M");

            // Load the texture
            //GLuint Texture = loadDDS("uvtemplate.DDS");
            Texture = Textures.Load(ResourcePath("uvmap.png"));

            // Get a handle for our "myTextureSampler" uniform
            //GLuint TextureID = glGetUniformLocation(programID, "myTextureSampler");
            TextureID = GL.GetUniformLocation(programID, "myTextureSampler");

            // Read our .obj file
            //std::vector<glm::vec3> vertices;
            //std::vector<glm::vec2> uvs;
            //std::vector<glm::vec3> normals; // Won't be used at the moment.
            //bool res = loadOBJ("suzanne.obj", vertices, uvs, normals);
            ObjLoader.Load(ResourcePath("suzanne.obj"), out var vertices, out var uvs, out var normals);

            //std::vector < unsigned short> indices;
            //std::vector<glm::vec3> indexed_vertices;
            //std::vector<glm::vec2> indexed_uvs;
            //std::vector<glm::vec3> indexed_normals;
            //indexVBO(vertices, uvs, normals, indices, indexed_vertices, indexed_uvs, indexed_normals);
            VboIndexer.IndexTriangles(
                vertices, uvs, normals, 
                out ushort[] indices, 
                out Vector3[] indexed_vertices, 
                out Vector2[] indexed_uvs, 
                out Vector3[] indexed_normals);
            indexCount = indices.Length;
            // Load it into a VBO

            //GLuint vertexbuffer;
            //glGenBuffers(1, &vertexbuffer);
            //glBindBuffer(GL_ARRAY_BUFFER, vertexbuffer);
            //glBufferData(GL_ARRAY_BUFFER, indexed_vertices.size() * sizeof(glm::vec3), &indexed_vertices[0], GL_STATIC_DRAW);
            vertexbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, indexed_vertices.Length * 3 * sizeof(float), indexed_vertices, BufferUsageHint.StaticDraw);

            //GLuint uvbuffer;
            //glGenBuffers(1, &uvbuffer);
            //glBindBuffer(GL_ARRAY_BUFFER, uvbuffer);
            //glBufferData(GL_ARRAY_BUFFER, indexed_uvs.size() * sizeof(glm::vec2), &indexed_uvs[0], GL_STATIC_DRAW);
            uvbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, uvbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, indexed_uvs.Length * 2 * sizeof(float), indexed_uvs, BufferUsageHint.StaticDraw);

            //GLuint normalbuffer;
            //glGenBuffers(1, &normalbuffer);
            //glBindBuffer(GL_ARRAY_BUFFER, normalbuffer);
            //glBufferData(GL_ARRAY_BUFFER, indexed_normals.size() * sizeof(glm::vec3), &indexed_normals[0], GL_STATIC_DRAW);
            normalbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, normalbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, indexed_normals.Length * 3 * sizeof(float), indexed_normals, BufferUsageHint.StaticDraw);

            // Generate a buffer for the indices as well
            //GLuint elementbuffer;
            //glGenBuffers(1, &elementbuffer);
            //glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, elementbuffer);
            //glBufferData(GL_ELEMENT_ARRAY_BUFFER, indices.size() * sizeof(unsigned short), &indices[0], GL_STATIC_DRAW);
            elementbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementbuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(ushort), indices, BufferUsageHint.StaticDraw);

            // Get a handle for our "LightPosition" uniform
            //glUseProgram(programID);
            //GLuint LightID = glGetUniformLocation(programID, "LightPosition_worldspace");
            GL.UseProgram(programID);
            LightID = GL.GetUniformLocation(programID, "LightPosition_worldspace");



            // ------------------------------------------------------------------------------------
            // first render pass

            // The framebuffer, which regroups 0, 1, or more textures, and 0 or 1 depth buffer.
            //GLuint FramebufferName = 0;
            //glGenFramebuffers(1, &FramebufferName);
            //glBindFramebuffer(GL_FRAMEBUFFER, FramebufferName);
            _framebuffer0 = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer0);

            // The texture we're going to render to
            //GLuint renderedTexture;
            //glGenTextures(1, &renderedTexture);
            _framebuffer0Texture = GL.GenTexture();

            // "Bind" the newly created texture : all future texture functions will modify this texture
            //glBindTexture(GL_TEXTURE_2D, renderedTexture);
            GL.BindTexture(TextureTarget.Texture2D, _framebuffer0Texture);

            // Give an empty image to OpenGL ( the last "0" means "empty" )
            //glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, windowWidth, windowHeight, 0, GL_RGB, GL_UNSIGNED_BYTE, 0);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, windowWidth, windowHeight, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);

            // Poor filtering
            //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
            //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
            //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
            //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            // The depth buffer
            //GLuint depthrenderbuffer;
            //glGenRenderbuffers(1, &depthrenderbuffer);
            //glBindRenderbuffer(GL_RENDERBUFFER, depthrenderbuffer);
            //glRenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH_COMPONENT, windowWidth, windowHeight);
            //glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, GL_RENDERBUFFER, depthrenderbuffer);
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
            //glFramebufferTexture(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, renderedTexture, 0);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, _framebuffer0Texture, 0);

            //// Depth texture alternative : 
            ////glFramebufferTexture(GL_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, depthTexture, 0);
            //GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, depthTexture, 0);


            // Set the list of draw buffers.
            //GLenum DrawBuffers[1] = { GL_COLOR_ATTACHMENT0 };
            //glDrawBuffers(1, DrawBuffers); // "1" is the size of DrawBuffers
            //GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            // Always check that our framebuffer is ok
            //if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
            //    return false;
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                throw new Exception("Framebuffer not complete");

            // end first render pass
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
            _desaturatePass_ProgramSourceLocation = GL.GetUniformLocation(_fullscreenquad_programID, "u_source");

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
                ResourcePath("Filmic.vertexshader"),
                ResourcePath("Filmic.fragmentshader"));
            _filmicPass_ProgramProjectionMatrixLocation = GL.GetUniformLocation(_filmicPass_Program, "projectionMatrix");
            _filmicPass_ProgramModelViewMatrixLocation = GL.GetUniformLocation(_filmicPass_Program, "modelViewMatrix");
            _filmicPass_ProgramTimeLocation = GL.GetUniformLocation(_filmicPass_Program, "u_time");
            _filmicPass_ProgramDiffuseLocation = GL.GetUniformLocation(_filmicPass_Program, "u_diffuse");

            // end filmic render pass
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
            float[] g_quad_vertex_buffer_data = [
                -1.0f, -1.0f, 0.0f,
                 1.0f, -1.0f, 0.0f,
                -1.0f,  1.0f, 0.0f,
                -1.0f,  1.0f, 0.0f,
                 1.0f, -1.0f, 0.0f,
                 1.0f,  1.0f, 0.0f,
            ];

            float[] g_quad_uv_buffer_data = [
                 0.0f,  0.0f,
                 1.0f,  0.0f,
                 0.0f,  1.0f,
                 0.0f,  1.0f,
                 1.0f,  0.0f,
                 1.0f,  1.0f,
            ];

            //GLuint quad_vertexbuffer;
            //glGenBuffers(1, &quad_vertexbuffer);
            //glBindBuffer(GL_ARRAY_BUFFER, quad_vertexbuffer);
            //glBufferData(GL_ARRAY_BUFFER, sizeof(g_quad_vertex_buffer_data), g_quad_vertex_buffer_data, GL_STATIC_DRAW);
            _fullscreenquad_vertexbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _fullscreenquad_vertexbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, g_quad_vertex_buffer_data.Length * sizeof(float), g_quad_vertex_buffer_data, BufferUsageHint.StaticDraw);
            
            _fullscreenquad_uvbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _fullscreenquad_uvbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, g_quad_uv_buffer_data.Length * sizeof(float), g_quad_uv_buffer_data, BufferUsageHint.StaticDraw);

            //// Create and compile our GLSL program from the shaders
            //GLuint quad_programID = LoadShaders("Passthrough.vertexshader", "WobblyTexture.fragmentshader");
            //GLuint texID = glGetUniformLocation(quad_programID, "renderedTexture");
            //GLuint timeID = glGetUniformLocation(quad_programID, "time");
            _fullscreenquad_programID = Shaders.Load(
                ResourcePath("Passthrough.vertexshader"),
                ResourcePath("Passthrough.fragmentshader"));
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
            //glBindFramebuffer(GL_FRAMEBUFFER, FramebufferName);
            //glViewport(0, 0, windowWidth, windowHeight); // Render on the whole framebuffer, complete from the lower left corner to the upper right
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer0);

            GL.Viewport(0, 0, windowWidth, windowHeight); // Render on the whole framebuffer, complete from the lower left corner to the upper right

            // Dark blue background
            //glClearColor(0.0f, 0.0f, 0.4f, 0.0f);
            GL.ClearColor(0.0f, 0.0f, 0.4f, 0.0f);

            // Clear the screen
            //glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Use our shader
            //glUseProgram(programID);
            GL.UseProgram(programID);

            // Compute the MVP matrix from keyboard and mouse input
            //computeMatricesFromInputs();
            //glm::mat4 ProjectionMatrix = getProjectionMatrix();
            //glm::mat4 ViewMatrix = getViewMatrix();
            //glm::mat4 ModelMatrix = glm::mat4(1.0);
            //glm::mat4 MVP = ProjectionMatrix * ViewMatrix * ModelMatrix;
            var ModelMatrix = Matrix4.Identity;
            var ViewMatrix = Matrix4.LookAt(
                new Vector3(4, 3, 3), // Camera is at (4,3,3), in World Space
                Vector3.Zero,         // and looks at the origin
                Vector3.UnitY         // Head is up (set to 0,-1,0 to look upside-down)
            );
            var MVP = ModelMatrix * ViewMatrix * Controls.ProjectionMatrix;

            // Send our transformation to the currently bound shader, 
            // in the "MVP" uniform
            //glUniformMatrix4fv(MatrixID, 1, GL_FALSE, &MVP[0][0]);
            //glUniformMatrix4fv(ModelMatrixID, 1, GL_FALSE, &ModelMatrix[0][0]);
            //glUniformMatrix4fv(ViewMatrixID, 1, GL_FALSE, &ViewMatrix[0][0]);
            GL.UniformMatrix4(MatrixID, false, ref MVP);
            GL.UniformMatrix4(ModelMatrixID, false, ref ModelMatrix);
            GL.UniformMatrix4(ViewMatrixID, false, ref ViewMatrix);

            //glm::vec3 lightPos = glm::vec3(4, 4, 4);
            var lightPos = new Vector3(4, 4, 4);
            //glUniform3f(LightID, lightPos.x, lightPos.y, lightPos.z);
            GL.Uniform3(LightID, lightPos.X, lightPos.Y, lightPos.Z);


            // Bind our texture in Texture Unit 0
            //glActiveTexture(GL_TEXTURE0);
            //glBindTexture(GL_TEXTURE_2D, Texture);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, Texture);

            // Set our "myTextureSampler" sampler to use Texture Unit 0
            //glUniform1i(TextureID, 0);
            GL.Uniform1(TextureID, 0);

            // 1rst attribute buffer : vertices
            //glEnableVertexAttribArray(0);
            //glBindBuffer(GL_ARRAY_BUFFER, vertexbuffer);
            //glVertexAttribPointer(
            //    0,                  // attribute. No particular reason for 0, but must match the layout in the shader.
            //    3,                  // size
            //    GL_FLOAT,           // type
            //    GL_FALSE,           // normalized?
            //    0,                  // stride
            //    (void*)0            // array buffer offset
            //);
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

            // 2nd attribute buffer : UVs
            //glEnableVertexAttribArray(1);
            //glBindBuffer(GL_ARRAY_BUFFER, uvbuffer);
            //glVertexAttribPointer(
            //    1,                                // attribute. No particular reason for 1, but must match the layout in the shader.
            //    2,                                // size : U+V => 2
            //    GL_FLOAT,                         // type
            //    GL_FALSE,                         // normalized?
            //    0,                                // stride
            //    (void*)0                          // array buffer offset
            //);
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

            // 3rd attribute buffer : normals
            //glEnableVertexAttribArray(2);
            //glBindBuffer(GL_ARRAY_BUFFER, normalbuffer);
            //glVertexAttribPointer(
            //    2,                                // attribute
            //    3,                                // size
            //    GL_FLOAT,                         // type
            //    GL_FALSE,                         // normalized?
            //    0,                                // stride
            //    (void*)0                          // array buffer offset
            //);
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

            // Draw the triangles !
            //glDrawElements(
            //    GL_TRIANGLES,      // mode
            //    indices.size(),    // count
            //    GL_UNSIGNED_SHORT,   // type
            //    (void*)0           // element array buffer offset
            //);
            GL.DrawElements(
                OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,            // mode
                indexCount,                         // count
                DrawElementsType.UnsignedShort,     // type
                0);                                 // element array buffer offset

            //glDisableVertexAttribArray(0);
            //glDisableVertexAttribArray(1);
            //glDisableVertexAttribArray(2);
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
                0,                  // attribute 0. No particular reason for 0, but must match the layout in the shader.
                3,                  // size
                VertexAttribPointerType.Float,           // type
                false,              // normalized?
                0,                  // stride
                0                   // array buffer offset
            );

            GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, 6); // 2*3 indices starting at 0 -> 2 triangles

            GL.DisableVertexAttribArray(0);

            // end render desaturate pass to frame buffer 1
            // ------------------------------------------------------------------------------------

            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);

            GL.Viewport(0, 0, windowWidth, windowHeight);

            GL.ClearColor(251.0f / 255.0f, 204.0f / 255.0f, 122.0f / 255.0f, 0.0f);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _desaturatePass_FrameBufferTexture);

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

                //GL.EnableVertexAttribArray(1);
                //GL.BindBuffer(BufferTarget.ArrayBuffer, _fullscreenquad_uvbuffer);
                //GL.VertexAttribPointer(
                //    0,                  // attribute 0. No particular reason for 0, but must match the layout in the shader.
                //    2,                  // size
                //    VertexAttribPointerType.Float,           // type
                //    false,              // normalized?
                //    0,                  // stride
                //    0                   // array buffer offset
                //);

                GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, 6); // 2*3 indices starting at 0 -> 2 triangles

                GL.DisableVertexAttribArray(0);
                //GL.DisableVertexAttribArray(1);
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

            // Swap buffers
            SwapBuffers();
        }

        // This function runs on every update frame.
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // Check if the ESC key was pressed or the window was closed
            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            base.OnUpdateFrame(e);

            if (KeyboardState.IsKeyPressed(Keys.Enter))
            {
                _renderToFullScreenQuad = !_renderToFullScreenQuad;
                Console.WriteLine("Render to screen quad: " + _renderToFullScreenQuad);
            }

            Controls.Update(this, (float)e.Time);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Cleanup VBO and shader
            //glDeleteBuffers(1, &vertexbuffer);
            //glDeleteBuffers(1, &uvbuffer);
            //glDeleteBuffers(1, &normalbuffer);
            //glDeleteBuffers(1, &elementbuffer);
            //glDeleteProgram(programID);
            //glDeleteTextures(1, &Texture);

            //glDeleteFramebuffers(1, &FramebufferName);
            //glDeleteTextures(1, &renderedTexture);
            //glDeleteRenderbuffers(1, &depthrenderbuffer);
            //glDeleteBuffers(1, &quad_vertexbuffer);
            //glDeleteVertexArrays(1, &VertexArrayID);

            GL.DeleteBuffer(vertexbuffer);
            GL.DeleteBuffer(uvbuffer);
            GL.DeleteBuffer(normalbuffer);
            GL.DeleteBuffer(elementbuffer);
            GL.DeleteProgram(programID);
            GL.DeleteTexture(Texture);
            GL.DeleteVertexArray(VertexArrayID);

            GL.DeleteFramebuffer(_framebuffer0);
            GL.DeleteTexture(_framebuffer0Texture);
            GL.DeleteRenderbuffer(_framebuffer0DepthBuffer);
            GL.DeleteBuffer(_fullscreenquad_vertexbuffer);
            GL.DeleteBuffer(_fullscreenquad_uvbuffer);
            GL.DeleteVertexArray(VertexArrayID);

            GL.DeleteBuffer(box_vertexbuffer);
            GL.DeleteBuffer(box_uvbuffer);
            GL.DeleteProgram(box_programID);

            base.OnClosing(e);
        }
    }
}