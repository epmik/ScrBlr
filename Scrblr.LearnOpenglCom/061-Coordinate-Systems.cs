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
    public class Learn061 : AbstractSketch
    {
        private static readonly string VertexShaderSource = @"
#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoord;

out vec2 TexCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
	gl_Position = projection * view * model * vec4(aPos, 1.0);
	TexCoord = vec2(aTexCoord.x, aTexCoord.y);
}
";

        //Fragment shaders are run on each fragment/pixel of the geometry.
        private static readonly string FragmentShaderSource = @"
#version 330 core
out vec4 FragColor;

in vec2 TexCoord;

// texture samplers
// uniform sampler2D texture1;
// uniform sampler2D texture2;

void main()
{
	// linearly interpolate between both textures (80% container, 20% awesomeface)
	FragColor = vec4(1.0f, 1.0f, 0.0f, 1.0f); // mix(texture(texture1, TexCoord), texture(texture2, TexCoord), 0.2);
}
";

        //Vertex data, uploaded to the VBO.
        private static readonly float[] vertices =
        {
            // positions          // texture coords
             0.5f,  0.5f, 0.0f,   1.0f, 1.0f,   // top right
             0.5f, -0.5f, 0.0f,   1.0f, 0.0f,   // bottom right
            -0.5f, -0.5f, 0.0f,   0.0f, 0.0f,   // bottom left
            -0.5f,  0.5f, 0.0f,   0.0f, 1.0f,   // top left 
        };

        private static readonly uint[] indices = 
        {
            0, 1, 3, // first triangle
            1, 2, 3  // second triangle
        };

        uint VBO, VAO, EBO, Shader;


        private unsafe void Load()
        {
            // see https://learnopengl.com/code_viewer_gh.php?code=src/1.getting_started/6.1.coordinate_systems/coordinate_systems.cpp

            Gl.GenVertexArrays(1, out VAO); // or VAO = Gl.GenVertexArray();
            Gl.GenBuffers(1, out VBO);      // or VBO = Gl.GenBuffer();
            Gl.GenBuffers(1, out EBO);      // or EBO = Gl.GenBuffer();

            Gl.BindVertexArray(VAO);

            Gl.BindBuffer(GLEnum.ArrayBuffer, VBO);
            fixed (void* v = &vertices[0])
                Gl.BufferData(GLEnum.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);

            Gl.BindBuffer(GLEnum.ElementArrayBuffer, EBO);
            fixed (void* i = &indices[0])
                Gl.BufferData(GLEnum.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);

            // position attribute
            Gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 5 * sizeof(float), (void*)0);
            Gl.EnableVertexAttribArray(0);

            // texture coord attribute
            Gl.VertexAttribPointer(1, 2, GLEnum.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float)));
            Gl.EnableVertexAttribArray(1);



            uint vertexShader = Gl.CreateShader(ShaderType.VertexShader);
            Gl.ShaderSource(vertexShader, VertexShaderSource);
            Gl.CompileShader(vertexShader);

            //Checking the shader for compilation errors.
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

            //Combining the shaders under one shader program.
            Shader = Gl.CreateProgram();
            Gl.AttachShader(Shader, vertexShader);
            Gl.AttachShader(Shader, fragmentShader);
            Gl.LinkProgram(Shader);

            //Checking the linking for errors.
            Gl.GetProgram(Shader, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(Shader)}");
            }

            //Delete the no longer useful individual shaders;
            Gl.DetachShader(Shader, vertexShader);
            Gl.DetachShader(Shader, fragmentShader);
            Gl.DeleteShader(vertexShader);
            Gl.DeleteShader(fragmentShader);
        }

        private unsafe void Render()
        {
            // render
            // ------
            Gl.ClearColor(System.Drawing.Color.CornflowerBlue);
            Gl.Clear((uint)ClearBufferMask.ColorBufferBit);

            // bind textures on corresponding texture units
            //glActiveTexture(GL_TEXTURE0);
            //glBindTexture(GL_TEXTURE_2D, texture1);
            //glActiveTexture(GL_TEXTURE1);
            //glBindTexture(GL_TEXTURE_2D, texture2);

            // activate shader
            Gl.UseProgram(Shader);

            // create transformations
            //glm::mat4 model = glm::mat4(1.0f); // make sure to initialize matrix to identity matrix first
            //var model = Matrix4x4.Identity;
            //glm::mat4 view = glm::mat4(1.0f);
            //var view = Matrix4x4.Identity;
            //glm::mat4 projection = glm::mat4(1.0f);
            //var projection = Matrix4x4.Identity;

            var model = Matrix4x4.CreateRotationX((float)Utility.DegreesToRadians(-55));
            //model = glm::rotate(model, glm::radians(-55.0f), glm::vec3(1.0f, 0.0f, 0.0f));
            //view = glm::translate(view, glm::vec3(0.0f, 0.0f, -3.0f));
            var view = Matrix4x4.CreateTranslation(0.0f, 0.0f, -3f);
            //projection = glm::perspective(glm::radians(45.0f), (float)SCR_WIDTH / (float)SCR_HEIGHT, 0.1f, 100.0f);
            var projection = Matrix4x4.CreatePerspectiveFieldOfView((float)Utility.DegreesToRadians(45f), (float)window.Size.X / (float)window.Size.Y, 0.1f, 100f);
            // retrieve the matrix uniform locations
            //unsigned int modelLoc = glGetUniformLocation(ourShader.ID, "model");
            var modelLoc = Gl.GetUniformLocation(Shader, "model");
            //unsigned int viewLoc = glGetUniformLocation(ourShader.ID, "view");
            var viewLoc = Gl.GetUniformLocation(Shader, "view");
            // pass them to the shaders (3 different ways)
            //glUniformMatrix4fv(modelLoc, 1, GL_FALSE, glm::value_ptr(model));
            Gl.UniformMatrix4(Gl.GetUniformLocation(Shader, "model"), 1, false, (float*)&model);
            //glUniformMatrix4fv(viewLoc, 1, GL_FALSE, &view[0][0]);
            Gl.UniformMatrix4(Gl.GetUniformLocation(Shader, "view"), 1, false, (float*)&view);
            // note: currently we set the projection matrix each frame, but since the projection matrix rarely changes it's often best practice to set it outside the main loop only once.
            //ourShader.setMat4("projection", projection);
            Gl.UniformMatrix4(Gl.GetUniformLocation(Shader, "projection"), 1, false, (float*)&projection);


            // render container
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

            Gl.DeleteProgram(Shader);
        }

        private void Resize(uint width, uint height)
        {
            Gl.Viewport(0, 0, width, height);
        }
    }
}
