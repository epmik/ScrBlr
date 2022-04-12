using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Scrblr.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Scrblr.Leaning
{
    [Sketch(Name = "Learn001-BufferData-Position")]
    public class Learn001 : AbstractSketch20220317
    {
        public Learn001()
            : base(3, 3)
        {
            LoadAction += Load;
            UnLoadAction += UnLoad;
            RenderAction += Render;
            UpdateAction += Update;
        }

        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private Shader _shader;

        // Create the vertices for our triangle. These are listed in normalized device coordinates (NDC)
        // In NDC, (0, 0) is the center of the screen.
        // Negative X coordinates move to the left, positive X move to the right.
        // Negative Y coordinates move to the bottom, positive Y move to the top.
        // OpenGL only supports rendering in 3D, so to create a flat triangle, the Z coordinate will be kept as 0.
        //private readonly float[] _position =
        //{
        //    -0.5f, -0.5f, 0.0f, // Bottom-left vertex
        //     0.5f, -0.5f, 0.0f, // Bottom-right vertex
        //     0.0f,  0.5f, 0.0f  // Top vertex
        //};

        // We're assigning three different colors at the asscoiate vertex position:
        // blue for the top, green for the bottom left and red for the bottom right.
        private readonly float[] _position =
        {
             // positions
             1f, -1f, 0.0f,    // bottom right
            -1f, -1f, 0.0f,    // bottom left
             1f,  1f, 0.0f,    // top right
            -1f, -1f, 0.0f,    // bottom left
            -1f,  1f, 0.0f,    // top left
             1f,  1f, 0.0f,    // top right
        };

        public void Load()
        {
            ClearColor(1f, 1f, 1f, 1f);
            ClearColor(1f, 1f, 1f);
            ClearColor(255, 255, 255, 255);
            ClearColor(255, 255, 255);

            // We need to send our vertices over to the graphics card so OpenGL can use them.
            // To do this, we need to create what's called a Vertex Buffer Object (VBO).
            // These allow you to upload a bunch of data to a buffer, and send the buffer to the graphics card.
            // This effectively sends all the vertices at the same time.

            // First, we need to create a buffer. This function returns a handle to it, but as of right now, it's empty.
            _vertexBufferObject = GL.GenBuffer();

            // Now, bind the buffer. OpenGL uses one global state, so after calling this,
            // all future calls that modify the VBO will be applied to this buffer until another buffer is bound instead.
            // The first argument is an enum, specifying what type of buffer we're binding. A VBO is an ArrayBuffer.
            // There are multiple types of buffers, but for now, only the VBO is necessary.
            // The second argument is the handle to our buffer.
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            // Finally, upload the vertices to the buffer.
            // Arguments:
            //   Which buffer the data should be sent to.
            //   How much data is being sent, in bytes. You can generally set this to the length of your array, multiplied by sizeof(array type).
            //   The vertices themselves.
            //   How the buffer will be used, so that OpenGL can write the data to the proper memory space on the GPU.
            //   There are three different BufferUsageHints for drawing:
            //     StaticDraw: This buffer will rarely, if ever, update after being initially uploaded.
            //     DynamicDraw: This buffer will change frequently after being initially uploaded.
            //     StreamDraw: This buffer will change on every frame.
            //   Writing to the proper memory space is important! Generally, you'll only want StaticDraw,
            //   but be sure to use the right one for your use case.
            //GL.BufferData(BufferTarget.ArrayBuffer, _position.Length * sizeof(float), _position, BufferUsageHint.StaticDraw);
            GL.BufferData(BufferTarget.ArrayBuffer, _position.Length * sizeof(float), _position, BufferUsageHint.StaticDraw);

            // One notable thing about the buffer we just loaded data into is that it doesn't have any structure to it. It's just a bunch of floats (which are actaully just bytes).
            // The opengl driver doesn't know how this data should be interpreted or how it should be divided up into vertices. To do this opengl introduces the idea of a 
            // Vertex Array Obejct (VAO) which has the job of keeping track of what parts or what buffers correspond to what data. In this example we want to set our VAO up so that 
            // it tells opengl that we want to interpret 12 bytes as 3 floats and divide the buffer into vertices using that.
            // To do this we generate and bind a VAO (which looks deceptivly similar to creating and binding a VBO, but they are different!).
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            // Now, we need to setup how the vertex shader will interpret the VBO data; you can send almost any C datatype (and a few non-C ones too) to it.
            // While this makes them incredibly flexible, it means we have to specify how that data will be mapped to the shader's input variables.

            // To do this, we use the GL.VertexAttribPointer function
            // This function has two jobs, to tell opengl about the format of the data, but also to associate the current array buffer with the VAO.
            // This means that after this call, we have setup this attribute to source data from the current array buffer and interpret it in the way we specified.
            // Arguments:
            //   Location of the input variable in the shader. the layout(location = 0) line in the vertex shader explicitly sets it to 0.
            //   How many elements will be sent to the variable. In this case, 3 floats for every vertex.
            //   The data type of the elements set, in this case float.
            //   Whether or not the data should be converted to normalized device coordinates. In this case, false, because that's already done.
            //   The stride; this is how many bytes are between the last element of one vertex and the first element of the next. 3 * sizeof(float) in this case.
            //   The offset; this is how many bytes it should skip to find the first element of the first vertex. 0 as of right now.
            // Stride and Offset are just sort of glossed over for now, but when we get into texture coordinates they'll be shown in better detail.
            //GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            // Just like before, we create a pointer for the 3 position components of our vertices.
            // The only difference here is that we need to account for the 3 color values in the stride variable.
            // Therefore, the stride contains the size of 6 floats instead of 3.
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            // Enable variable 0 in the shader.
            GL.EnableVertexAttribArray(0);

            // Vertex attributes are the data we send as input into the vertex shader from the main program.
            // So here we're checking to see how many vertex attributes our hardware can handle.
            // OpenGL at minimum supports 16 vertex attributes. This only needs to be called 
            // when your intensive attribute work and need to know exactly how many are available to you.
            GL.GetInteger(GetPName.MaxVertexAttribs, out int maxAttributeCount);
            Debug.WriteLine($"Maximum number of vertex attributes supported: {maxAttributeCount}");

            const string vertexShaderSource = @"
#version 330 core

// the position variable has attribute position 0
layout(location = 0) in vec3 aPosition;  

// This is where the color values we assigned in the main program goes to
layout(location = 1) in vec3 aColor;

out vec3 ioColor; // output a color to the fragment shader

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main(void)
{
    gl_Position = vec4(aPosition, 1.0) * model * view * projection;

	// We use the ioColor variable to pass on the color information to the frag shader
	ioColor = aColor;
}";

            const string fragmentShaderSource = @"
#version 330 core

out vec4 oColor;

void main()
{
    oColor = vec4(1.0, 1.0, 0.0, 1.0);
}";

            _shader = new Shader(vertexShaderSource, fragmentShaderSource);
        }

        public void UnLoad()
        {
            
        }

        public void Render()
        {
            Clear(ClearFlag.Color);

            //PushTransform();

            //Rectangle()
            //    .Position(0, 0)
            //    .Size(1, 1)
            //    .Translate(10f, 10f)
            //    .Fill(255, 0, 0);

            //PopTransform();

            // To draw an object in OpenGL, it's typically as simple as binding your shader,
            // setting shader uniforms (not done here, will be shown in a future tutorial)
            // binding the VAO,
            // and then calling an OpenGL function to render.

            // Now, enable the shader.
            // Just like the VBO, this is global, so every function that uses a shader will modify this one until a new one is bound instead.
            _shader.Use();

            // Bind the VAO
            GL.BindVertexArray(_vertexArrayObject);

            // We start with an identity matrix. This is just a simple matrix that doesn't move the vertices at all.
            var model = Matrix4.Identity;

            // The next few steps just show how to use OpenTK's matrix functions, and aren't necessary for the transform matrix to actually work.
            // If you want, you can just pass the identity matrix to the shader, though it won't affect the vertices at all.

            // A fact to note about matrices is that the order of multiplications matter. "matrixA * matrixB" and "matrixB * matrixA" mean different things.
            // A VERY important thing to know is that OpenTK matrices are so called row-major. We won't go into the full details here, but here is a good place to read more about it:
            // https://www.scratchapixel.com/lessons/mathematics-physics-for-computer-graphics/geometry/row-major-vs-column-major-vector
            // What it means for us is that we can think of matrix multiplication as going left to right.
            // So "rotate * translate" means rotate (around the origin) first and then translate, as opposed to "translate * rotate" which means translate and then rotate (around the origin).

            // To combine two matrices, you multiply them. Here, we combine the transform matrix with another one created by OpenTK to rotate it by 20 degrees.
            // Note that all Matrix4.CreateRotation functions take radians, not degrees. Use MathHelper.DegreesToRadians() to convert to radians, if you want to use degrees.
            //model = model * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(45f));

            // Next, we scale the matrix. This will make the rectangle slightly larger.
            //model = model * Matrix4.CreateScale(1.1f);

            // Then, we translate the matrix, which will move it slightly towards the top-right.
            // Note that we aren't using a full coordinate system yet, so the translation is in normalized device coordinates.
            // The next tutorial will be about how to set one up so we can use more human-readable numbers.
            model = model * Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f);


            //// Now that the matrix is finished, pass it to the vertex shader.
            //// Go over to shader.vert to see how we finally apply this to the vertices.
            //_shader.SetMatrix4("transform", transform);

            // IMPORTANT: OpenTK's matrix types are transposed from what OpenGL would expect - rows and columns are reversed.
            // They are then transposed properly when passed to the shader. 
            // This means that we retain the same multiplication order in both OpenTK c# code and GLSL shader code.
            // If you pass the individual matrices to the shader and multiply there, you have to do in the order "model * view * projection".
            // You can think like this: first apply the modelToWorld (aka model) matrix, then apply the worldToView (aka view) matrix, 
            // and finally apply the viewToProjectedSpace (aka projection) matrix.
            _shader.Uniform("model", model);
            _shader.Uniform("view", ViewMatrix);
            _shader.Uniform("projection", ProjectionMatrix);


            // And then call our drawing function.
            // For this tutorial, we'll use GL.DrawArrays, which is a very simple rendering function.
            // Arguments:
            //   Primitive type; What sort of geometric primitive the vertices represent.
            //     OpenGL used to support many different primitive types, but almost all of the ones still supported
            //     is some variant of a triangle. Since we just want a single triangle, we use Triangles.
            //   Starting index; this is just the start of the data you want to draw. 0 here.
            //   How many vertices you want to draw. 3 for a triangle.
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        }

        public void Update()
        {
            
        }
    }
}
