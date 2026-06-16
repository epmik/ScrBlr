
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Skrbl;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace OpenGlTutorialOrg
{
    // This is where all OpenGL code will be written.
    // OpenToolkit allows for several functions to be overriden to extend functionality; this is how we'll be writing code.
    public class Tutorial06Window : AbstractTutorialWindow
    {
        int VertexArrayID;
        int programID;
        int vertexbuffer;
        int uvbuffer;
        int MatrixID;
        Matrix4 MVP;
        int Texture;
        int TextureID;
        Controls Controls = new Controls();

        // A simple constructor to let us set properties like window size, title, FPS, etc. on the window.
        public Tutorial06Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            Title = "Tutorial 06 - Keyboard and Mouse";
            ResourceDirectory = "opengl-tutorial.org/tutorial06_keyboard_and_mouse/";
        }

        protected override void OnLoad()
        {
            base.OnLoad();

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
            //GLuint programID = LoadShaders("TransformVertexShader.vertexshader", "TextureFragmentShader.fragmentshader");
            programID = Shaders.Load(
                ResourcePath("TransformVertexShader.vertexshader"),
                ResourcePath("TextureFragmentShader.fragmentshader"));

            // Get a handle for our "MVP" uniform
            //GLuint MatrixID = glGetUniformLocation(programID, "MVP");
            MatrixID = GL.GetUniformLocation(programID, "MVP");

            // Load the texture
            //GLuint Texture = loadDDS("uvtemplate.DDS");
            Texture = Textures.Load(ResourcePath("uvtemplate.png"));

            // Get a handle for our "myTextureSampler" uniform
            //GLuint TextureID = glGetUniformLocation(programID, "myTextureSampler");
            TextureID = GL.GetUniformLocation(programID, "myTextureSampler");

            // Our vertices. Tree consecutive floats give a 3D vertex; Three consecutive vertices give a triangle.
            // A cube has 6 faces with 2 triangles each, so this makes 6*2=12 triangles, and 12*3 vertices
            float[] g_vertex_buffer_data = [
                -1.0f,-1.0f,-1.0f,
                -1.0f,-1.0f, 1.0f,
                -1.0f, 1.0f, 1.0f,
                 1.0f, 1.0f,-1.0f,
                -1.0f,-1.0f,-1.0f,
                -1.0f, 1.0f,-1.0f,
                 1.0f,-1.0f, 1.0f,
                -1.0f,-1.0f,-1.0f,
                 1.0f,-1.0f,-1.0f,
                 1.0f, 1.0f,-1.0f,
                 1.0f,-1.0f,-1.0f,
                -1.0f,-1.0f,-1.0f,
                -1.0f,-1.0f,-1.0f,
                -1.0f, 1.0f, 1.0f,
                -1.0f, 1.0f,-1.0f,
                 1.0f,-1.0f, 1.0f,
                -1.0f,-1.0f, 1.0f,
                -1.0f,-1.0f,-1.0f,
                -1.0f, 1.0f, 1.0f,
                -1.0f,-1.0f, 1.0f,
                 1.0f,-1.0f, 1.0f,
                 1.0f, 1.0f, 1.0f,
                 1.0f,-1.0f,-1.0f,
                 1.0f, 1.0f,-1.0f,
                 1.0f,-1.0f,-1.0f,
                 1.0f, 1.0f, 1.0f,
                 1.0f,-1.0f, 1.0f,
                 1.0f, 1.0f, 1.0f,
                 1.0f, 1.0f,-1.0f,
                -1.0f, 1.0f,-1.0f,
                 1.0f, 1.0f, 1.0f,
                -1.0f, 1.0f,-1.0f,
                -1.0f, 1.0f, 1.0f,
                 1.0f, 1.0f, 1.0f,
                -1.0f, 1.0f, 1.0f,
                 1.0f,-1.0f, 1.0f
            ];

            // Two UV coordinatesfor each vertex. They were created with Blender.
            float[] g_uv_buffer_data = [
                0.000059f, 0.000004f,
                0.000103f, 0.336048f,
                0.335973f, 0.335903f,
                1.000023f, 0.000013f,
                0.667979f, 0.335851f,
                0.999958f, 0.336064f,
                0.667979f, 0.335851f,
                0.336024f, 0.671877f,
                0.667969f, 0.671889f,
                1.000023f, 0.000013f,
                0.668104f, 0.000013f,
                0.667979f, 0.335851f,
                0.000059f, 0.000004f,
                0.335973f, 0.335903f,
                0.336098f, 0.000071f,
                0.667979f, 0.335851f,
                0.335973f, 0.335903f,
                0.336024f, 0.671877f,
                1.000004f, 0.671847f,
                0.999958f, 0.336064f,
                0.667979f, 0.335851f,
                0.668104f, 0.000013f,
                0.335973f, 0.335903f,
                0.667979f, 0.335851f,
                0.335973f, 0.335903f,
                0.668104f, 0.000013f,
                0.336098f, 0.000071f,
                0.000103f, 0.336048f,
                0.000004f, 0.671870f,
                0.336024f, 0.671877f,
                0.000103f, 0.336048f,
                0.336024f, 0.671877f,
                0.335973f, 0.335903f,
                0.667969f, 0.671889f,
                1.000004f, 0.671847f,
                0.667979f, 0.335851f
            ];

            //GLuint vertexbuffer;
            //glGenBuffers(1, &vertexbuffer);
            //glBindBuffer(GL_ARRAY_BUFFER, vertexbuffer);
            //glBufferData(GL_ARRAY_BUFFER, sizeof(g_vertex_buffer_data), g_vertex_buffer_data, GL_STATIC_DRAW);
            vertexbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, g_vertex_buffer_data.Length * sizeof(float), g_vertex_buffer_data, BufferUsageHint.StaticDraw);

            //GLuint uvbuffer;
            //glGenBuffers(1, &uvbuffer);
            //glBindBuffer(GL_ARRAY_BUFFER, uvbuffer);
            //glBufferData(GL_ARRAY_BUFFER, sizeof(g_uv_buffer_data), g_uv_buffer_data, GL_STATIC_DRAW);
            uvbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, uvbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, g_uv_buffer_data.Length * sizeof(float), g_uv_buffer_data, BufferUsageHint.StaticDraw);
        }

        // This function runs on every update frame.
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
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
            var MVP = ModelMatrix * Controls.ViewMatrix * Controls.ProjectionMatrix;

            // Send our transformation to the currently bound shader, 
            // in the "MVP" uniform
            //glUniformMatrix4fv(MatrixID, 1, GL_FALSE, &MVP[0][0]);
            GL.UniformMatrix4(MatrixID, false, ref MVP);

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

            // Draw the triangle !
            //glDrawArrays(GL_TRIANGLES, 0, 12 * 3); // 12*3 indices starting at 0 -> 12 triangles
            GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, 12 * 3); // 3 indices starting at 0 -> 1 triangle

            //glDisableVertexAttribArray(0);
            //glDisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);

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

            Controls.Update(this, (float)e.Time);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Cleanup VBO and shader
            //glDeleteBuffers(1, &vertexbuffer);
            //glDeleteBuffers(1, &uvbuffer);
            //glDeleteProgram(programID);
            //glDeleteTextures(1, &TextureID);
            //glDeleteVertexArrays(1, &VertexArrayID);

            GL.DeleteBuffer(vertexbuffer);
            GL.DeleteBuffer(uvbuffer);
            GL.DeleteProgram(programID);
            GL.DeleteTexture(Texture);
            GL.DeleteVertexArray(VertexArrayID);

            base.OnClosing(e);
        }
    }
}