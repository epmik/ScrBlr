using Scrblr.Core;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Drawing;
using System.Xml.Linq;
using Silk.NET.GLFW;
using System.Reflection.Metadata;
using System.Numerics;
using Silk.NET.SDL;

namespace Scrblr.LearnOpenglCom
{
    //[Sketch(Name = "Learn006-BufferSubData-DrawArrays-Position-Color-Seperated")]
    public class Learn022HelloTriangleIndexed : AbstractSketch
    {
        uint shaderProgram;
        uint VBO, VAO, EBO;

        private static readonly string VertexShaderSource = @"
#version 330 core
layout (location = 0) in vec3 aPos;
void main()
{
    gl_Position = vec4(aPos.x, aPos.y, aPos.z, 1.0);
}";

        //Fragment shaders are run on each fragment/pixel of the geometry.
        private static readonly string FragmentShaderSource = @"
#version 330 core
out vec4 FragColor;
void main()
{
    FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);
}
";

        private unsafe void Load()
        {
            // build and compile our shader program
            // ------------------------------------
            // vertex shader
            uint vertexShader = Gl.CreateShader(GLEnum.VertexShader);
            Gl.ShaderSource(vertexShader, VertexShaderSource);
            Gl.CompileShader(vertexShader);
            // check for shader compile errors
            string infoLog = Gl.GetShaderInfoLog(vertexShader);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                Console.WriteLine($"Error compiling vertex shader {infoLog}");
            }
            //Creating a fragment shader.
            uint fragmentShader = Gl.CreateShader(ShaderType.FragmentShader);
            Gl.ShaderSource(fragmentShader, FragmentShaderSource);
            Gl.CompileShader(fragmentShader);
            //Checking the shader for compilation errors.
            infoLog = Gl.GetShaderInfoLog(fragmentShader);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                Console.WriteLine($"Error compiling fragment shader {infoLog}");
            }
            // link shaders
            //Combining the shaders under one shader program.
            shaderProgram = Gl.CreateProgram();
            Gl.AttachShader(shaderProgram, vertexShader);
            Gl.AttachShader(shaderProgram, fragmentShader);
            Gl.LinkProgram(shaderProgram);
            // check for linking errors
            Gl.GetProgram(shaderProgram, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(shaderProgram)}");
            }
            //Delete the no longer useful individual shaders;
            Gl.DetachShader(shaderProgram, vertexShader);
            Gl.DetachShader(shaderProgram, fragmentShader);
            Gl.DeleteShader(vertexShader);
            Gl.DeleteShader(fragmentShader);


            // set up vertex data (and buffer(s)) and configure vertex attributes
            // ------------------------------------------------------------------
            float[] vertices = {
                 0.5f,  0.5f, 0.0f,  // top right
                 0.5f, -0.5f, 0.0f,  // bottom right
                -0.5f, -0.5f, 0.0f,  // bottom left
                -0.5f,  0.5f, 0.0f   // top left 
            };
            uint[] indices = {  // note that we start from 0!
                0, 1, 3,  // first Triangle
                1, 2, 3   // second Triangle
            };

            Gl.GenVertexArrays(1, out VAO); // or VAO = Gl.GenVertexArray();
            Gl.GenBuffers(1, out VBO);      // or VBO = Gl.GenBuffer();
            Gl.GenBuffers(1, out EBO);      // or EBO = Gl.GenBuffer();
            // bind the Vertex Array Object first, then bind and set vertex buffer(s), and then configure vertex attributes(s).
            Gl.BindVertexArray(VAO);

            Gl.BindBuffer(GLEnum.ArrayBuffer, VBO);
            fixed (void* v = &vertices[0])
                Gl.BufferData(GLEnum.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);

            Gl.BindBuffer(GLEnum.ElementArrayBuffer, EBO);
            fixed (void* i = &indices[0])
                Gl.BufferData(GLEnum.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);

            // position attribute
            Gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 3 * sizeof(float), (void*)null);
            Gl.EnableVertexAttribArray(0);

            // note that this is allowed, the call to glVertexAttribPointer registered VBO as the vertex attribute's bound vertex buffer object so afterwards we can safely unbind
            //Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            // remember: do NOT unbind the EBO while a VAO is active as the bound element buffer object IS stored in the VAO; keep the EBO bound.
            //glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);

            // You can unbind the VAO afterwards so other VAO calls won't accidentally modify this VAO, but this rarely happens. Modifying other
            // VAOs requires a call to glBindVertexArray anyways so we generally don't unbind VAOs (nor VBOs) when it's not directly necessary.
            //Gl.BindVertexArray(0);


            // uncomment this call to draw in wireframe polygons.
            //glPolygonMode(GL_FRONT_AND_BACK, GL_LINE);







        }

        private unsafe void Render()
        {
            // render
            // ------
            Gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            Gl.Clear((uint)ClearBufferMask.ColorBufferBit);

            Gl.UseProgram(shaderProgram);

            Gl.BindVertexArray(VAO);

            //glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, 0);
            Gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
        }

        private void Update()
        {
        }

        private void UnLoad()
        {
            Gl.DeleteVertexArray(VAO);
            Gl.DeleteBuffer(VBO);
            Gl.DeleteBuffer(EBO);

            Gl.DeleteProgram(shaderProgram);
        }

        private void Resize(uint width, uint height)
        {
            Gl.Viewport(0, 0, width, height);
        }
    }
}
