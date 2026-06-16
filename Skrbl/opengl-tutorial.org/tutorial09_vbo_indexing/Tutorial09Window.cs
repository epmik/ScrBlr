
using ObjParser;
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
    public class Tutorial09Window : AbstractTutorialWindow
    {
        int VertexArrayID;
        int programID;
        int vertexbuffer;
        int uvbuffer;
        int normalbuffer;
        int elementbuffer;
        int MatrixID;
        int ViewMatrixID;
        int ModelMatrixID;
        Matrix4 MVP;
        int Texture;
        int TextureID;
        Controls Controls = new Controls();
        int indexCount;
        int LightID;

        // A simple constructor to let us set properties like window size, title, FPS, etc. on the window.
        public Tutorial09Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            Title = "Tutorial 09 - VBO Indexing";
            ResourceDirectory = "opengl-tutorial.org/tutorial09_vbo_indexing/";
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
            //GLuint programID = LoadShaders("StandardShading.vertexshader", "StandardShading.fragmentshader");
            programID = Shaders.Load(
                ResourcePath("StandardShading.vertexshader"),
                ResourcePath("StandardShading.fragmentshader"));

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
            //glUniformMatrix4fv(ModelMatrixID, 1, GL_FALSE, &ModelMatrix[0][0]);
            //glUniformMatrix4fv(ViewMatrixID, 1, GL_FALSE, &ViewMatrix[0][0]);
            GL.UniformMatrix4(MatrixID, false, ref MVP);
            GL.UniformMatrix4(ModelMatrixID, false, ref ModelMatrix);
            GL.UniformMatrix4(ViewMatrixID, false, ref Controls.ViewMatrix);

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
            //glDeleteBuffers(1, &normalbuffer);
            //glDeleteBuffers(1, &elementbuffer);
            //glDeleteProgram(programID);
            //glDeleteTextures(1, &Texture);
            //glDeleteVertexArrays(1, &VertexArrayID);

            GL.DeleteBuffer(vertexbuffer);
            GL.DeleteBuffer(uvbuffer);
            GL.DeleteBuffer(normalbuffer);
            GL.DeleteBuffer(elementbuffer);
            GL.DeleteProgram(programID);
            GL.DeleteTexture(Texture);
            GL.DeleteVertexArray(VertexArrayID);

            base.OnClosing(e);
        }
    }
}