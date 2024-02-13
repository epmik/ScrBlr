using Silk.NET.OpenGL;
using Silk.NET.Input;
using Silk.NET.Windowing;
using Silk.NET.Maths;

namespace Scrblr.Learning
{
    // Be warned, there is a LOT of stuff here. It might seem complicated, but just take it slow and you'll be fine.
    // OpenGL's initial hurdle is quite large, but once you get past that, things will start making more sense.
    public class Learn003Silk : SilkSketch20240207
    {
        // We modify the vertex array to include four vertices for our rectangle.
        private readonly float[] _vertices =
        {
             0.5f,  0.5f, 0.0f, // top right
             0.5f, -0.5f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f, // top left
        };

        // Then, we create a new array: indices.
        // This array controls how the EBO will use those vertices to create triangles
        private readonly uint[] _indices =
        {
            // Note that indices start at 0!
            0, 1, 3, // The first triangle will be the top-right half of the triangle
            1, 2, 3  // Then the second will be the bottom-left half of the triangle
        };

        private uint _vertexBufferObject;

        private uint _vertexArrayObject;

        private Core.Shader _shader;

        // Add a handle for the EBO
        private uint _elementBufferObject;

        public Learn003Silk()
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

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(GLEnum.ArrayBuffer, _vertexBufferObject);
            fixed (void* v = &_vertices[0])
                GL.BufferData(GLEnum.ArrayBuffer, (nuint)(_vertices.Length * sizeof(float)), v, GLEnum.StaticDraw);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            GL.VertexAttribPointer(0, 3, GLEnum.Float, false, 3 * sizeof(float), (void*)0);
            GL.EnableVertexAttribArray(0);

            // We create/bind the Element Buffer Object EBO the same way as the VBO, except there is a major difference here which can be REALLY confusing.
            // The binding spot for ElementArrayBuffer is not actually a global binding spot like ArrayBuffer is. 
            // Instead it's actually a property of the currently bound VertexArrayObject, and binding an EBO with no VAO is undefined behaviour.
            // This also means that if you bind another VAO, the current ElementArrayBuffer is going to change with it.
            // Another sneaky part is that you don't need to unbind the buffer in ElementArrayBuffer as unbinding the VAO is going to do this,
            // and unbinding the EBO will remove it from the VAO instead of unbinding it like you would for VBOs or VAOs.
            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(GLEnum.ElementArrayBuffer, _elementBufferObject);
            // We also upload data to the EBO the same way as we did with VBOs.
            fixed (void* i = &_indices[0])
                GL.BufferData(GLEnum.ElementArrayBuffer, (nuint)(_indices.Length * sizeof(uint)), i, GLEnum.StaticDraw);
            // The EBO has now been properly setup. Go to the Render function to see how we draw our rectangle now!


            var vertexShaderSource = @"
#version 330 core

layout(location = 0) in vec3 aPosition;

void main(void)
{
    gl_Position = vec4(aPosition, 1.0);
}
";

            var fragmentShaderSource = @"
#version 330

out vec4 outputColor;

void main()
{
    outputColor = vec4(1.0, 1.0, 0.0, 1.0);
}
";

            _shader = new Core.Shader(GL, vertexShaderSource, fragmentShaderSource);
            _shader.Use();
        }

        protected unsafe void OnUnLoad()
        {
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteBuffer(_elementBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);

            _shader.Delete();
        }

        protected unsafe void OnRenderFrame(double elapsedTime)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            _shader.Use();

            // Because ElementArrayObject is a property of the currently bound VAO,
            // the buffer you will find in the ElementArrayBuffer will change with the currently bound VAO.
            GL.BindVertexArray(_vertexArrayObject);

            // Then replace your call to DrawTriangles with one to DrawElements
            // Arguments:
            //   Primitive type to draw. Triangles in this case.
            //   How many indices should be drawn. Six in this case.
            //   Data type of the indices. The indices are an unsigned int, so we want that here too.
            //   Offset in the EBO. Set this to 0 because we want to draw the whole thing.
            GL.DrawElements(GLEnum.Triangles, (uint)_indices.Length, GLEnum.UnsignedInt, (void*)0);
        }

        protected void OnUpdateFrame(double elapsedTime)
        {
            
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