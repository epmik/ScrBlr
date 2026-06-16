
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
    public class Tutorial03Window : GameWindow
    {
        int VertexArrayID;
        int programID;
        int vertexbuffer;
        int MatrixID;
        Matrix4 MVP;

        // A simple constructor to let us set properties like window size, title, FPS, etc. on the window.
        public Tutorial03Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            Title = "Tutorial 03 - Matrices";
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            // Dark blue background
            GL.ClearColor(0.0f, 0.0f, 0.4f, 0.0f);

            VertexArrayID = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayID);

            // Create and compile our GLSL program from the shaders
            programID = Shaders.Load(
                "opengl-tutorial.org/tutorial03_matrices/SimpleTransform.vertexshader",
                "opengl-tutorial.org/tutorial03_matrices/SingleColor.fragmentshader");

            // Get a handle for our "MVP" uniform
            MatrixID = GL.GetUniformLocation(programID, "MVP");

            // Projection matrix : 45� Field of View, 4:3 ratio, display range : 0.1 unit <-> 100 units
            // glm::mat4 Projection = glm::perspective(glm::radians(45.0f), 4.0f / 3.0f, 0.1f, 100.0f);
            var Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), 4.0f / 3.0f, 0.1f, 100.0f);
            // Or, for an ortho camera :
            //glm::mat4 Projection = glm::ortho(-10.0f,10.0f,-10.0f,10.0f,0.0f,100.0f); // In world coordinates
            //var Projection = Matrix4.CreateOrthographicOffCenter(-10.0f, 10.0f, -10.0f, 10.0f, 0.0f, 100.0f);

            // Camera matrix
            //   glm::mat4 View = glm::lookAt(
            // glm::vec3(4, 3, 3), // Camera is at (4,3,3), in World Space
            // glm::vec3(0, 0, 0), // and looks at the origin
            // glm::vec3(0, 1, 0)  // Head is up (set to 0,-1,0 to look upside-down)
            //);
            var View = Matrix4.LookAt(
                new Vector3(4, 3, 3), // Camera is at (4,3,3), in World Space
                Vector3.Zero,         // and looks at the origin
                Vector3.UnitY         // Head is up (set to 0,-1,0 to look upside-down)
            );

            // Model matrix : an identity matrix (model will be at the origin)
            //glm::mat4 Model = glm::mat4(1.0f);
            var Model = Matrix4.Identity;
            // Our ModelViewProjection : multiplication of our 3 matrices
            //glm::mat4 MVP = Projection * View * Model; // Remember, matrix multiplication is the other way around
            //MVP = Projection * View * Model;
            MVP = Model * View * Projection;

            //static const GLfloat g_vertex_buffer_data[] = {
            //	-1.0f, -1.0f, 0.0f,
            //	 1.0f, -1.0f, 0.0f,
            //	 0.0f,  1.0f, 0.0f,
            //};
            float[] g_vertex_buffer_data = [
                -1.0f, -1.0f, 0.0f,
                 1.0f, -1.0f, 0.0f,
                 0.0f,  1.0f, 0.0f,
            ];

            //GLuint vertexbuffer;
            //glGenBuffers(1, &vertexbuffer);
            //glBindBuffer(GL_ARRAY_BUFFER, vertexbuffer);
            //glBufferData(GL_ARRAY_BUFFER, sizeof(g_vertex_buffer_data), g_vertex_buffer_data, GL_STATIC_DRAW);
            vertexbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, g_vertex_buffer_data.Length * sizeof(float), g_vertex_buffer_data, BufferUsageHint.StaticDraw);
        }

        // This function runs on every update frame.
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            // Clear the screen
            //glClear(GL_COLOR_BUFFER_BIT);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Use our shader
            //glUseProgram(programID);
            GL.UseProgram(programID);

            // Send our transformation to the currently bound shader, 
            // in the "MVP" uniform
            //glUniformMatrix4fv(MatrixID, 1, GL_FALSE, &MVP[0][0]);
            GL.UniformMatrix4(MatrixID, false, ref MVP);
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

            // Draw the triangle !
            //glDrawArrays(GL_TRIANGLES, 0, 3); // 3 indices starting at 0 -> 1 triangle
            GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, 3); // 3 indices starting at 0 -> 1 triangle

            //glDisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(0);

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
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Cleanup VBO and shader
            //glDeleteBuffers(1, &vertexbuffer);
            //glDeleteProgram(programID);
            //glDeleteVertexArrays(1, &VertexArrayID);

            GL.DeleteBuffer(vertexbuffer);
            GL.DeleteProgram(programID);
            GL.DeleteVertexArray(VertexArrayID);

            base.OnClosing(e);
        }
    }
}