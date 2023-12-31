﻿using Silk.NET.OpenGL;
using Silk.NET.Input;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using System.Numerics;
using System.Drawing;
using Silk.NET.GLFW;

namespace Scrblr.Learning
{
    // Be warned, there is a LOT of stuff here. It might seem complicated, but just take it slow and you'll be fine.
    // OpenGL's initial hurdle is quite large, but once you get past that, things will start making more sense.
    public class Learn009Camera : SilkSketch
    {
        private uint _elementBufferObject;

        private uint _vertexBufferObject;

        private uint _vertexArrayObject;

        private Shader _shader;

        private Texture _texture;

        private Texture _texture2;

        float angle = 20f;
        float angleRotationPerSecond = 45f;

        // Then, we create two matrices to hold our view and projection. They're initialized at the bottom of OnLoad.
        // The view matrix is what you might consider the "camera". It represents the current viewport in the window.
        private Matrix4x4 _view;

        // This represents how the vertices will be projected. It's hard to explain through comments,
        // so check out the web version for a good demonstration of what this does.
        private Matrix4x4 _projection;

        private Vector3 modelPosition = new Vector3(0, 0, 0);
        private Vector3 modelPositionOffsetPerSecond = new Vector3(1, 0, 0);

        private Vector3 cameraPosition = new Vector3(0, 0, -3);

        // We need an instance of the new camera class so it can manage the view and projection matrix code.
        // We also need a boolean set to true to detect whether or not the mouse has been moved for the first time.
        // Finally, we add the last position of the mouse so we can calculate the mouse offset easily.
        private Camera _camera;

        private bool _firstMove = true;

        private Vector2 _lastPos;

        private Vector2D<int> WindowCenter = new Vector2D<int>();

        string vertexShaderSource = @"
#version 330 core

layout(location = 0) in vec3 aPosition;

layout(location = 1) in vec2 aTexCoord;

out vec2 texCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main(void)
{
    texCoord = aTexCoord;

    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
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

        public Learn009Camera()
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

            WindowCenter.X = (int)(window.Size.X / 2);
            WindowCenter.Y = (int)(window.Size.Y / 2);
        }

        protected unsafe void OnLoad()
        {
            GL = GL.GetApi(window);

            inputContext = window.CreateInput();

            Keyboard = inputContext.Keyboards[0];

            Mouse = inputContext.Mice[0];

            Keyboard.KeyDown += KeyDown;
            Keyboard.KeyUp += KeyUp;
            Keyboard.KeyChar += KeyChar;

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

            _texture = Texture.LoadFromFile(GL, ".resources//container.png");
            _texture.Use(TextureUnit.Texture0);

            _texture2 = Texture.LoadFromFile(GL, ".resources/awesomeface.png");
            _texture2.Use(TextureUnit.Texture1);

            _shader.SetInt("texture0", 0);
            _shader.SetInt("texture1", 1);

            // For the matrix, we use a few parameters.
            //   Field of view. This determines how much the viewport can see at once. 45 is considered the most "realistic" setting, but most video games nowadays use 90
            //   Aspect ratio. This should be set to Width / Height.
            //   Near-clipping. Any vertices closer to the camera than this value will be clipped.
            //   Far-clipping. Any vertices farther away from the camera than this value will be clipped.
            _projection = Matrix4x4.CreatePerspectiveFieldOfView((float)Utility.DegreesToRadians(45f), (float)window.Size.X / (float)window.Size.Y, 0.1f, 100.0f);


            // We initialize the camera so that it is 3 units back from where the rectangle is.
            // We also give it the proper aspect ratio.
            _camera = new Camera(cameraPosition, window.Size.X / (float)window.Size.Y);

            HideCursor();

            ConfineCursor();

            CenterCursor();
        }

        private void HideCursor()
        {
            Mouse.Cursor.CursorMode = CursorMode.Hidden;
        }

        private void HideAllCursors()
        {
            for (int i = 0; i < inputContext.Mice.Count; i++)
            {
                inputContext.Mice[i].Cursor.CursorMode = CursorMode.Hidden;
            }
        }

        private void ConfineCursor()
        {
            for (int i = 0; i < inputContext.Mice.Count; i++)
            {
                inputContext.Mice[i].Cursor.IsConfined = true;
            }
        }

        private void CenterCursor()
        {
            for (int i = 0; i < inputContext.Mice.Count; i++)
            {
                inputContext.Mice[i].Position = new Vector2((int)(window.Size.X / 2), (int)(window.Size.Y / 2));
            }
        }

        protected unsafe void OnRenderFrame(double elapsedTime)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(_vertexArrayObject);

            // Note: The matrices we'll use for transformations are all 4x4.

            // We start with an identity matrix. This is just a simple matrix that doesn't move the vertices at all.
            var model = Matrix4x4.Identity;

            //// The next few steps just show how to use OpenTK's matrix functions, and aren't necessary for the transform matrix to actually work.
            //// If you want, you can just pass the identity matrix to the shader, though it won't affect the vertices at all.

            //// A fact to note about matrices is that the order of multiplications matter. "matrixA * matrixB" and "matrixB * matrixA" mean different things.
            //// A VERY important thing to know is that OpenTK matrices are so called row-major. We won't go into the full details here, but here is a good place to read more about it:
            //// https://www.scratchapixel.com/lessons/mathematics-physics-for-computer-graphics/geometry/row-major-vs-column-major-vector
            //// What it means for us is that we can think of matrix multiplication as going left to right.
            //// So "rotate * translate" means rotate (around the origin) first and then translate, as opposed to "translate * rotate" which means translate and then rotate (around the origin).

            //// To combine two matrices, you multiply them. Here, we combine the transform matrix with another one created by OpenTK to rotate it by 20 degrees.
            //// Note that all Matrix4.CreateRotation functions take radians, not degrees. Use MathHelper.DegreesToRadians() to convert to radians, if you want to use degrees.
            //model = model * Matrix4x4.CreateRotationZ((float)Utility.DegreesToRadians(angle));

            //// Next, we scale the matrix. This will make the rectangle slightly larger.
            //model = model * Matrix4x4.CreateScale(1.1f);

            //// Then, we translate the matrix, which will move it slightly towards the top-right.
            //// Note that we aren't using a full coordinate system yet, so the translation is in normalized device coordinates.
            //// The next tutorial will be about how to set one up so we can use more human-readable numbers.
            //model = model * Matrix4x4.CreateTranslation(0.1f, 0.1f, 0.0f);

            // Finally, we have the model matrix. This determines the position of the model.
            model = Matrix4x4.Identity * Matrix4x4.CreateRotationX((float)Utility.DegreesToRadians(angle));

            model = model * Matrix4x4.CreateTranslation(modelPosition);

            // For the view, we don't do too much here. Next tutorial will be all about a Camera class that will make it much easier to manipulate the view.
            // For now, we move it backwards three units on the Z axis.
            //_view = Matrix4x4.CreateTranslation(viewPosition);
            _view = _camera.GetViewMatrix();

            // Then, we pass all of these matrices to the vertex shader.
            // You could also multiply them here and then pass, which is faster, but having the separate matrices available is used for some advanced effects.

            // IMPORTANT: OpenTK's matrix types are transposed from what OpenGL would expect - rows and columns are reversed.
            // They are then transposed properly when passed to the shader. 
            // This means that we retain the same multiplication order in both OpenTK c# code and GLSL shader code.
            // If you pass the individual matrices to the shader and multiply there, you have to do in the order "model * view * projection".
            // You can think like this: first apply the modelToWorld (aka model) matrix, then apply the worldToView (aka view) matrix, 
            // and finally apply the viewToProjectedSpace (aka projection) matrix.

            _texture.Use(TextureUnit.Texture0);
            _texture2.Use(TextureUnit.Texture1);
            _shader.Use();

            // Now that the matrix is finished, pass it to the vertex shader.
            // Go over to shader.vert to see how we finally apply this to the vertices.
            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", _view);
            _shader.SetMatrix4("projection", _projection);

            // And that's it for now! In the next tutorial, we'll see how to setup a full coordinates system.

            GL.DrawElements(GLEnum.Triangles, (uint)(6), GLEnum.UnsignedInt, (void*)0);
        }

        protected void OnUpdateFrame(double elapsedTime)
        {

            //if (!IsFocused) // Check to see if the window is focused
            //{
            //    return;
            //}

            angle += (float)(angleRotationPerSecond * elapsedTime);

            if(angle >= 360f)
            {
                angle -= 360f;
            }

            //modelPosition += modelPositionOffsetPerSecond * (float)elapsedTime;

            //if (modelPosition.X > 1.5 || modelPosition.X < -1.5)
            //{
            //    modelPositionOffsetPerSecond *= -1;
            //}

            //viewPosition += viewPositionOffsetPerSecond * (float)elapsedTime;

            //if (viewPosition.Z > 0 || viewPosition.Z < -6)
            //{
            //    viewPositionOffsetPerSecond *= -1;
            //}




            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.2f;

            if (IsKeyDown(Key.W))
            {
                _camera.Position += _camera.Front * cameraSpeed * (float)elapsedTime; // Forward
            }

            if (IsKeyDown(Key.S))
            {
                _camera.Position -= _camera.Front * cameraSpeed * (float)elapsedTime; // Backwards
            }
            if (IsKeyDown(Key.A))
            {
                _camera.Position -= _camera.Right * cameraSpeed * (float)elapsedTime; // Left
            }
            if (IsKeyDown(Key.D))
            {
                _camera.Position += _camera.Right * cameraSpeed * (float)elapsedTime; // Right
            }
            if (IsKeyDown(Key.Space))
            {
                _camera.Position += _camera.Up * cameraSpeed * (float)elapsedTime; // Up
            }
            if (IsKeyDown(Key.ShiftLeft))
            {
                _camera.Position -= _camera.Up * cameraSpeed * (float)elapsedTime; // Down
            }

            //// Get the mouse state
            //var mouse = MouseState;

            //if (_firstMove) // This bool variable is initially set to true.
            //{
            //    _lastPos = new Vector2(mouse.X, mouse.Y);
            //    _firstMove = false;
            //}
            //else
            //{
            // Calculate the offset of the mouse position
            var deltaX = Mouse.Position.X - WindowCenter.X;
            var deltaY = Mouse.Position.Y - WindowCenter.Y;

            Mouse.Position = new Vector2(WindowCenter.X, WindowCenter.Y);

            // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
            _camera.Yaw += deltaX * sensitivity;
            _camera.Pitch -= deltaY * sensitivity; // Reversed since y-coordinates range from bottom to top
            //}
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

        private void KeyUp(IKeyboard keyboard, Key key, int arg3)
        {
        }

        private void KeyChar(IKeyboard keyboard, char character)
        {
        }

        private bool IsKeyDown(Key key)
        {
            return Keyboard.IsKeyPressed(key);
        }

        protected void OnResize(Vector2D<int> size)
        {
            WindowCenter.X = (int)(window.Size.X / 2);
            WindowCenter.Y = (int)(window.Size.Y / 2);

            GL.Viewport(0, 0, (uint)size.X, (uint)size.Y);
        }
    }
}