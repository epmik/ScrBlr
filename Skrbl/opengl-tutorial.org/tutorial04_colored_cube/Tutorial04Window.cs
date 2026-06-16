
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
    public class Tutorial04Window : AbstractTutorialWindow
    {
        int VertexArrayID;
        int programID;
        int vertexbuffer;
        int colorbuffer;
        int MatrixID;
        Matrix4 MVP;

        // A simple constructor to let us set properties like window size, title, FPS, etc. on the window.
        public Tutorial04Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            Title = "Tutorial 03 - Matrices";
            ResourceDirectory = "opengl-tutorial.org/tutorial04_colored_cube/";
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            // Dark blue background
            //glClearColor(0.0f, 0.0f, 0.4f, 0.0f);
            GL.ClearColor(0.0f, 0.0f, 0.4f, 0.0f);

            // Enable depth test
            //glEnable(GL_DEPTH_TEST);
            GL.Enable(EnableCap.DepthTest);
            // Accept fragment if it is closer to the camera than the former one
            //glDepthFunc(GL_LESS);
            GL.DepthFunc(DepthFunction.Less);

            //GLuint VertexArrayID;
            //glGenVertexArrays(1, &VertexArrayID);
            //glBindVertexArray(VertexArrayID);
            VertexArrayID = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayID);

            // Create and compile our GLSL program from the shaders
            //GLuint programID = LoadShaders("TransformVertexShader.vertexshader", "ColorFragmentShader.fragmentshader");
            programID = Shaders.Load(
                ResourcePath("TransformVertexShader.vertexshader"),
                ResourcePath("ColorFragmentShader.fragmentshader"));

            // Get a handle for our "MVP" uniform
            //GLuint MatrixID = glGetUniformLocation(programID, "MVP");
            MatrixID = GL.GetUniformLocation(programID, "MVP");

            // Projection matrix : 45� Field of View, 4:3 ratio, display range : 0.1 unit <-> 100 units
            //glm::mat4 Projection = glm::perspective(glm::radians(45.0f), 4.0f / 3.0f, 0.1f, 100.0f);
            var Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), 4.0f / 3.0f, 0.1f, 100.0f);

            // Camera matrix
            //glm::mat4 View = glm::lookAt(
            //    glm::vec3(4, 3, -3), // Camera is at (4,3,-3), in World Space
            //    glm::vec3(0, 0, 0), // and looks at the origin
            //    glm::vec3(0, 1, 0)  // Head is up (set to 0,-1,0 to look upside-down)
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
            MVP = Model * View * Projection;

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

            // One color for each vertex. They were generated randomly.
            float[] g_color_buffer_data = [
                0.583f,  0.771f,  0.014f,
                0.609f,  0.115f,  0.436f,
                0.327f,  0.483f,  0.844f,
                0.822f,  0.569f,  0.201f,
                0.435f,  0.602f,  0.223f,
                0.310f,  0.747f,  0.185f,
                0.597f,  0.770f,  0.761f,
                0.559f,  0.436f,  0.730f,
                0.359f,  0.583f,  0.152f,
                0.483f,  0.596f,  0.789f,
                0.559f,  0.861f,  0.639f,
                0.195f,  0.548f,  0.859f,
                0.014f,  0.184f,  0.576f,
                0.771f,  0.328f,  0.970f,
                0.406f,  0.615f,  0.116f,
                0.676f,  0.977f,  0.133f,
                0.971f,  0.572f,  0.833f,
                0.140f,  0.616f,  0.489f,
                0.997f,  0.513f,  0.064f,
                0.945f,  0.719f,  0.592f,
                0.543f,  0.021f,  0.978f,
                0.279f,  0.317f,  0.505f,
                0.167f,  0.620f,  0.077f,
                0.347f,  0.857f,  0.137f,
                0.055f,  0.953f,  0.042f,
                0.714f,  0.505f,  0.345f,
                0.783f,  0.290f,  0.734f,
                0.722f,  0.645f,  0.174f,
                0.302f,  0.455f,  0.848f,
                0.225f,  0.587f,  0.040f,
                0.517f,  0.713f,  0.338f,
                0.053f,  0.959f,  0.120f,
                0.393f,  0.621f,  0.362f,
                0.673f,  0.211f,  0.457f,
                0.820f,  0.883f,  0.371f,
                0.982f,  0.099f,  0.879f
            ];


            //GLuint vertexbuffer;
            //glGenBuffers(1, &vertexbuffer);
            //glBindBuffer(GL_ARRAY_BUFFER, vertexbuffer);
            //glBufferData(GL_ARRAY_BUFFER, sizeof(g_vertex_buffer_data), g_vertex_buffer_data, GL_STATIC_DRAW);
            vertexbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, g_vertex_buffer_data.Length * sizeof(float), g_vertex_buffer_data, BufferUsageHint.StaticDraw);

            //GLuint colorbuffer;
            //glGenBuffers(1, &colorbuffer);
            //glBindBuffer(GL_ARRAY_BUFFER, colorbuffer);
            //glBufferData(GL_ARRAY_BUFFER, sizeof(g_color_buffer_data), g_color_buffer_data, GL_STATIC_DRAW);
            colorbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, colorbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, g_color_buffer_data.Length * sizeof(float), g_color_buffer_data, BufferUsageHint.StaticDraw);
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

            // 2nd attribute buffer : colors
            //glEnableVertexAttribArray(1);
            //glBindBuffer(GL_ARRAY_BUFFER, colorbuffer);
            //glVertexAttribPointer(
            //    1,                                // attribute. No particular reason for 1, but must match the layout in the shader.
            //    3,                                // size
            //    GL_FLOAT,                         // type
            //    GL_FALSE,                         // normalized?
            //    0,                                // stride
            //    (void*)0                          // array buffer offset
            //);
            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, colorbuffer);
            GL.VertexAttribPointer(
                1,                  // attribute. No particular reason for 1, but must match the layout in the shader.
                3,                  // size
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
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Cleanup VBO and shader
            //glDeleteBuffers(1, &vertexbuffer);
            //glDeleteBuffers(1, &colorbuffer);
            //glDeleteProgram(programID);
            //glDeleteVertexArrays(1, &VertexArrayID);

            GL.DeleteBuffer(vertexbuffer);
            GL.DeleteBuffer(colorbuffer);
            GL.DeleteProgram(programID);
            GL.DeleteVertexArray(VertexArrayID);

            base.OnClosing(e);
        }
    }
}