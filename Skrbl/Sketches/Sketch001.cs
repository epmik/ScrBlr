
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Skrbl
{
    public class Sketch001 : GameWindow
    {
        //private readonly float[] _verticesTextured =
        //{
        //    // Position         Texture coordinates
        //     0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top right
        //     0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
        //    -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
        //    -0.5f,  0.5f, 0.0f, 0.0f, 1.0f  // top left
        //};

        private readonly float[] _verticesTextured =
        {
            // Position             Texture coordinates
             250f,  250f, 0.0f,     1.0f, 1.0f, // top right
             250f, -250f, 0.0f,     1.0f, 0.0f, // bottom right
            -250f, -250f, 0.0f,     0.0f, 0.0f, // bottom left
            -250f,  250f, 0.0f,     0.0f, 1.0f  // top left
        };

        private readonly uint[] _indicesTextured =
        {
            0, 1, 3,
            1, 2, 3
        };

        // Two triangles forming a quad which covers the whole display area
        float[] _postprocessingVertices =
        {
             // position             texture coords
             1.0f,  1.0f, 0.0f,   1.0f, 1.0f,     // top right
             1.0f, -1.0f, 0.0f,   1.0f, 0.0f,     // bottom right
            -1.0f, -1.0f, 0.0f,   0.0f, 0.0f,     // bottom left
            -1.0f,  1.0f, 0.0f,   0.0f, 1.0f      // top left
        };

        readonly uint[] _postprocessingIndices =
        {
            0, 1, 3,
            1, 2, 3
        };

        private int _elementBufferObjectTextured;

        private int _vertexBufferObjectTextured;

        private int _vertexArrayObjectTextured;

        private Shader _shaderTextured;

        private Texture _texture;

        private Texture _texture2;

        private Shader _shader;
        private Shader _orangeShader;
        Shader _postprocessingShader;

        // The view and projection matrices have been removed as we don't need them here anymore.
        // They can now be found in the new camera class.

        // We need an instance of the new camera class so it can manage the view and projection matrix code.
        // We also need a boolean set to true to detect whether or not the mouse has been moved for the first time.
        // Finally, we add the last position of the mouse so we can calculate the mouse offset easily.
        private Camera01 _camera;

        private bool _mouseButtonDown = false;

        private Vector2 _lastPos;

        Color4 _backgroundColor = new(00.2f, 0.3f, 0.3f, 1.0f);

        public Sketch001(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            Diagnostics.EnableOpenGlDebugMessages();

            GL.ClearColor(_backgroundColor);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.Enable(EnableCap.DepthTest);

            _vertexArrayObjectTextured = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObjectTextured);

            _vertexBufferObjectTextured = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObjectTextured);
            GL.BufferData(BufferTarget.ArrayBuffer, _verticesTextured.Length * sizeof(float), _verticesTextured, BufferUsageHint.StaticDraw);

            _elementBufferObjectTextured = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObjectTextured);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indicesTextured.Length * sizeof(uint), _indicesTextured, BufferUsageHint.StaticDraw);

            _shaderTextured = new Shader(".shaders/with-camera.vert", ".shaders/with-camera.frag");
            _shaderTextured.Use();

            var vertexLocation = _shaderTextured.GetAttribLocation("i_pos");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            var texCoordLocation = _shaderTextured.GetAttribLocation("i_uv0");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            _texture = Texture.LoadFromFile(".resources/.textures./container.png");
            _texture.Use(TextureUnit.Texture0);

            _texture2 = Texture.LoadFromFile(".resources/.textures./awesomeface.png");
            _texture2.Use(TextureUnit.Texture1);

            _shaderTextured.SetInt("u_tex0", 0);
            _shaderTextured.SetInt("u_tex1", 1);


            _shader = new (".shaders/simple.vert", ".shaders/simple.frag");
            _orangeShader = new (".shaders/orange.vert", ".shaders/orange.frag");
            _postprocessingShader = new ("_postprocessing");
            //_shader.Use();

                
            // --------------------------------------------
            // Postprocessing start

            //postprocessingVertexArrayObject = GL.GenVertexArray();
            //GL.BindVertexArray(postprocessingVertexArrayObject);

            //postprocessingVertexBufferObject = GL.GenBuffer();
            //GL.BindBuffer(BufferTarget.ArrayBuffer, postprocessingVertexBufferObject);
            //GL.BufferData(BufferTarget.ArrayBuffer, _postprocessingVertices.Length * sizeof(float), _postprocessingVertices, BufferUsageHint.StaticDraw);

            //postprocessingElementBufferObject = GL.GenBuffer();
            //GL.BindBuffer(BufferTarget.ElementArrayBuffer, postprocessingElementBufferObject);
            //GL.BufferData(BufferTarget.ElementArrayBuffer, _postprocessingIndices.Length * sizeof(uint), _postprocessingIndices, BufferUsageHint.StaticDraw);

            //var locationVertices = _postprocessingShader.GetAttribLocation("vertices");
            //GL.EnableVertexAttribArray(locationVertices);
            //GL.VertexAttribPointer(locationVertices, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            ////                                       ^ 3 vertex is 3 floats                   ^ 5 per row        ^ 0 offset per row

            //var locationTexCoords = _postprocessingShader.GetAttribLocation("vertexTexCoords");
            //GL.EnableVertexAttribArray(locationTexCoords);
            //GL.VertexAttribPointer(locationTexCoords, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            ////                                        ^ tex coords is 2 floats                 ^ 5 per row        ^ 4th and 5th float in each row

            // Postprocessing end
            // --------------------------------------------

            // We initialize the camera so that it is 3 units back from where the rectangle is.
            // We also give it the proper aspect ratio.
            _camera = new Camera01(ClientSize.X / (float)ClientSize.Y, ProjectionType.Perspective);
            //_camera = new Camera((float)ClientSize.X, (float)ClientSize.Y, ProjectionType.Orthographic);
            _camera = new Camera01((float)ClientSize.X, (float)ClientSize.Y, MathHelper.PiOver3, ProjectionType.Perspective);

            //_camera.Position = Vector3.UnitZ * 100f;

            // We make the mouse cursor invisible and captured so we can have proper FPS-camera movement.
            CursorState = CursorState.Grabbed;


            MousePosition = new Vector2(ClientSize.X/ 2, ClientSize.Y / 2);

            _lastPos = new Vector2(MouseState.X, MouseState.Y);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(_vertexArrayObjectTextured);

            _texture.Use(TextureUnit.Texture0);
            _texture2.Use(TextureUnit.Texture1);
            _shaderTextured.Use();

            var model = Matrix4.Identity;// * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(_time));
            _shaderTextured.SetMatrix4("u_model", model);
            _shaderTextured.SetMatrix4("u_view", _camera.GetViewMatrix());
            _shaderTextured.SetMatrix4("u_projection", _camera.GetProjectionMatrix());


            _orangeShader.Use();
            _orangeShader.SetMatrix4("u_model", model);
            _orangeShader.SetMatrix4("u_view", _camera.GetViewMatrix());
            _orangeShader.SetMatrix4("u_projection", _camera.GetProjectionMatrix());

            GL.DrawElements(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, _indicesTextured.Length, DrawElementsType.UnsignedInt, 0);

            //_texture.Disable(TextureUnit.Texture0);
            //_texture2.Disable(TextureUnit.Texture1);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (!IsFocused) // Check to see if the window is focused
            {
                return;
            }

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.2f;

            if (input.IsKeyDown(Keys.W))
            {
                _camera.Position += _camera.Front * cameraSpeed * (float)e.Time; // Forward
            }

            if (input.IsKeyDown(Keys.S))
            {
                _camera.Position -= _camera.Front * cameraSpeed * (float)e.Time; // Backwards
            }
            if (input.IsKeyDown(Keys.A))
            {
                _camera.Position -= _camera.Right * cameraSpeed * (float)e.Time; // Left
            }
            if (input.IsKeyDown(Keys.D))
            {
                _camera.Position += _camera.Right * cameraSpeed * (float)e.Time; // Right
            }
            if (input.IsKeyDown(Keys.Space))
            {
                _camera.Position += _camera.Up * cameraSpeed * (float)e.Time; // Up
            }
            if (input.IsKeyDown(Keys.LeftShift))
            {
                _camera.Position -= _camera.Up * cameraSpeed * (float)e.Time; // Down
            }

            // Get the mouse state
            var mouse = MouseState;

            if(mouse.IsButtonDown(MouseButton.Left))
            {
                if (!_mouseButtonDown)
                {
                    _lastPos = new Vector2(mouse.X, mouse.Y);
                    _mouseButtonDown = true;
                }

                // Calculate the offset of the mouse position
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                _camera.Yaw += deltaX * sensitivity;
                _camera.Pitch -= deltaY * sensitivity; // Reversed since y-coordinates range from bottom to top
            }
            else
            {
                _mouseButtonDown = false;
            }

        }

        // In the mouse wheel function, we manage all the zooming of the camera.
        // This is simply done by changing the FOV of the camera.
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            //_camera.Fov -= e.OffsetY;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
            // We need to update the aspect ratio once the window has been resized.
            _camera.AspectRatio = ClientSize.X / (float)ClientSize.Y;
        }
    }
}   