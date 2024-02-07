using Silk.NET.OpenGL;
using Silk.NET.Input;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using System.Numerics;

namespace Scrblr.Learning
{
    // Be warned, there is a LOT of stuff here. It might seem complicated, but just take it slow and you'll be fine.
    // OpenGL's initial hurdle is quite large, but once you get past that, things will start making more sense.
    public class Learn007Transformations : SilkSketch20240207
    {
        private uint _elementBufferObject;

        private uint _vertexBufferObject;

        private uint _vertexArrayObject;

        private Shader _shader;

        private Texture _texture;

        private Texture _texture2;

        float angle = 20f;
        float angleRotationPerSecond = 45f;

        string vertexShaderSource = @"
#version 330 core

layout(location = 0) in vec3 aPosition;

layout(location = 1) in vec2 aTexCoord;

out vec2 texCoord;

// Add a uniform for the transformation matrix.
uniform mat4 transform;

void main(void)
{
    texCoord = aTexCoord;

    // Then all you have to do is multiply the vertices by the transformation matrix, and you'll see your transformation in the scene!
    gl_Position = vec4(aPosition, 1.0) * transform;
}
";

        string fragmentShaderSource = @"
#version 330

out vec4 outputColor;

in vec2 texCoord;

uniform sampler2D texture0;
uniform sampler2D texture1;

void main()
{
    outputColor = mix(texture(texture0, texCoord), texture(texture1, texCoord), 0.2);
}
";

        public Learn007Transformations()
        {
            var options = Silk.NET.Windowing.WindowOptions.Default;
            options.Size = new Vector2D<int>(800, 680);
            options.Title = "LearnOpenGL with Silk.NET";
            window = Window.Create(options);

            window.Load += OnLoad;
            window.Render += OnRenderFrame;
            window.Update += OnUpdateFrame;
            window.Resize += OnResize;
            window.Closing += OnUnLoad;
        }

        protected unsafe void OnLoad()
        {
            GL = GL.GetApi(window);

            inputContext = window.CreateInput();

            for (int i = 0; i < inputContext.Keyboards.Count; i++)
            {
                inputContext.Keyboards[i].KeyDown += KeyDown;
            }

            float[] _vertices =
                    {
                // Position         Texture coordinates
                 0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top right
                 0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
                -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
                -0.5f,  0.5f, 0.0f, 0.0f, 1.0f  // top left
            };

            uint[] _indices =
            {
                0, 1, 3,
                1, 2, 3
            };


            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(GLEnum.ArrayBuffer, _vertexBufferObject);
            fixed (void* v = &_vertices[0]) 
                GL.BufferData(GLEnum.ArrayBuffer, (nuint)(_vertices.Length * sizeof(float)), v, GLEnum.StaticDraw);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(GLEnum.ElementArrayBuffer, _elementBufferObject);
            fixed (void* i = &_indices[0])
                GL.BufferData(GLEnum.ElementArrayBuffer, (nuint)(_indices.Length * sizeof(uint)), i, GLEnum.StaticDraw);

            _shader = new Shader(GL, vertexShaderSource, fragmentShaderSource); _shader.Use();

            var vertexLocation = _shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, GLEnum.Float, false, 5 * sizeof(float), (void*)0);

            var texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, GLEnum.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float)));

            _texture = Texture.LoadFromFile(GL, ".resources/container.png");
            _texture.Use(TextureUnit.Texture0);

            _texture2 = Texture.LoadFromFile(GL, ".resources/awesomeface.png");
            _texture2.Use(TextureUnit.Texture1);

            _shader.SetInt("texture0", 0);
            _shader.SetInt("texture1", 1);
        }

        protected unsafe void OnRenderFrame(double elapsedTime)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.BindVertexArray(_vertexArrayObject);

            // Note: The matrices we'll use for transformations are all 4x4.

            // We start with an identity matrix. This is just a simple matrix that doesn't move the vertices at all.
            var transform = Matrix4x4.Identity;

            // The next few steps just show how to use OpenTK's matrix functions, and aren't necessary for the transform matrix to actually work.
            // If you want, you can just pass the identity matrix to the shader, though it won't affect the vertices at all.

            // A fact to note about matrices is that the order of multiplications matter. "matrixA * matrixB" and "matrixB * matrixA" mean different things.
            // A VERY important thing to know is that OpenTK matrices are so called row-major. We won't go into the full details here, but here is a good place to read more about it:
            // https://www.scratchapixel.com/lessons/mathematics-physics-for-computer-graphics/geometry/row-major-vs-column-major-vector
            // What it means for us is that we can think of matrix multiplication as going left to right.
            // So "rotate * translate" means rotate (around the origin) first and then translate, as opposed to "translate * rotate" which means translate and then rotate (around the origin).

            // To combine two matrices, you multiply them. Here, we combine the transform matrix with another one created by OpenTK to rotate it by 20 degrees.
            // Note that all Matrix4.CreateRotation functions take radians, not degrees. Use MathHelper.DegreesToRadians() to convert to radians, if you want to use degrees.
            transform = transform * Matrix4x4.CreateRotationZ((float)Utility.DegreesToRadians(angle));

            // Next, we scale the matrix. This will make the rectangle slightly larger.
            transform = transform * Matrix4x4.CreateScale(1.1f);

            // Then, we translate the matrix, which will move it slightly towards the top-right.
            // Note that we aren't using a full coordinate system yet, so the translation is in normalized device coordinates.
            // The next tutorial will be about how to set one up so we can use more human-readable numbers.
            transform = transform * Matrix4x4.CreateTranslation(0.1f, 0.1f, 0.0f);

            _texture.Use(TextureUnit.Texture0);
            _texture2.Use(TextureUnit.Texture1);
            _shader.Use();

            // Now that the matrix is finished, pass it to the vertex shader.
            // Go over to shader.vert to see how we finally apply this to the vertices.
            _shader.SetMatrix4("transform", transform);

            // And that's it for now! In the next tutorial, we'll see how to setup a full coordinates system.

            GL.DrawElements(GLEnum.Triangles, (uint)(6), GLEnum.UnsignedInt, (void*)0);
        }

        protected void OnUpdateFrame(double elapsedTime)
        {
            angle += (float)(angleRotationPerSecond * elapsedTime);

            if(angle >= 360f)
            {
                angle -= 360f;
            }
        }

        protected unsafe void OnUnLoad()
        {
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteBuffer(_elementBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);

            _shader.Delete();
        }

        private void KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            if (key == Key.Escape)
            {
                window.Close();
            }
        }

        protected void OnResize(Vector2D<int> size)
        {
            GL.Viewport(0, 0, (uint)size.X, (uint)size.Y);
        }
    }
}