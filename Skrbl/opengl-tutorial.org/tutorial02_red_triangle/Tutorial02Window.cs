
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Skrbl;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace OpenGlTutorialOrg
{
    // This is where all OpenGL code will be written.
    // OpenToolkit allows for several functions to be overriden to extend functionality; this is how we'll be writing code.
    public class Tutorial02Window : GameWindow
    {
        int VertexArrayID;
        int programID;
        int vertexbuffer;

        // A simple constructor to let us set properties like window size, title, FPS, etc. on the window.
        public Tutorial02Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            Title = "Tutorial 02 - Red triangle";
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
                "opengl-tutorial.org/tutorial02_red_triangle/SimpleVertexShader.vertexshader", 
                "opengl-tutorial.org/tutorial02_red_triangle/SimpleFragmentShader.fragmentshader");

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

            // 1rst attribute buffer : vertices
            //glEnableVertexAttribArray(0);
            //glBindBuffer(GL_ARRAY_BUFFER, vertexbuffer);
            //glVertexAttribPointer(
            //    0,                  // attribute 0. No particular reason for 0, but must match the layout in the shader.
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
            // Cleanup VBO
            //glDeleteBuffers(1, &vertexbuffer);
            //glDeleteVertexArrays(1, &VertexArrayID);
            //glDeleteProgram(programID);

            GL.DeleteBuffer(vertexbuffer);
            GL.DeleteVertexArray(VertexArrayID);
            GL.DeleteProgram(programID);

            base.OnClosing(e);
        }
    }
}