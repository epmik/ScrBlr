using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using LearnOpenTK.Common;
using System.Diagnostics;

namespace LearnOpenTK
{
    // So you've drawn the first triangle. But what about drawing multiple?
    // You may consider just adding more vertices to the array, and that would technically work, but say you're drawing a rectangle.
    // It only needs four vertices, but since OpenGL works in triangles, you'd need to define 6.
    // Not a huge deal, but it quickly adds up when you get to more complex models. For example, a cube only needs 8 vertices, but
    // doing it that way would need 36 vertices!

    // OpenGL provides a way to reuse vertices, which can heavily reduce memory usage on complex objects.
    // This is called an Element Buffer Object. This tutorial will be all about how to set one up.
    public class LearnOpenTK004InsAndOuts : GameWindow
    {

        private readonly float[] _vertices =
        {
            -0.5f, -0.5f, 0.0f, // Bottom-left vertex
             0.5f, -0.5f, 0.0f, // Bottom-right vertex
             0.0f,  0.5f, 0.0f  // Top vertex
        };

        private int _vertexBufferObject;

        private int _vertexArrayObject;

        private Shader _shader;

        public LearnOpenTK004InsAndOuts(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            _vertexBufferObject = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Vertex attributes are the data we send as input into the vertex shader from the main program.
            // So here we're checking to see how many vertex attributes our hardware can handle.
            // OpenGL at minimum supports 16 vertex attributes. This only needs to be called 
            // when your intensive attribute work and need to know exactly how many are available to you.
            GL.GetInteger(GetPName.MaxVertexAttribs, out int maxAttributeCount);
            Debug.WriteLine($"Maximum number of vertex attributes supported: {maxAttributeCount}");

            _shader = new Shader("Shaders/LearnOpenTK0044ShadersMoreAttributes.vert", "Shaders/LearnOpenTK0044ShadersMoreAttributes.frag");
            _shader.Use();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            _shader.Use();

            GL.BindVertexArray(_vertexArrayObject);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);
        }
    }
}
