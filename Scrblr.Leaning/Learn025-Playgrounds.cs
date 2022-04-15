using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Scrblr.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Scrblr.Leaning
{
    [Sketch(Name = "Learn025-Playgrounds")]
    public class Learn025 : AbstractSketch
    {
        public Learn025()
            : base(600, 600)
        {
            LoadAction += Load;
            UnLoadAction += UnLoad;
            RenderAction += Render;
            UpdateAction += Update;
            MouseWheelAction += MouseWheel;
            ResizeAction += Resize;

            Samples = 8;
        }

        const string vertexShaderSource = @"
#version 330 core

uniform mat4 uModelMatrix;
uniform mat4 uViewMatrix;
uniform mat4 uProjectionMatrix;

layout(location = 0) in vec3 iPosition0;
layout(location = 1) in vec4 iColor0;
layout(location = 2) in vec2 iUv0;

out vec2 ioUv0;
out vec4 ioColor0;

void main(void)
{
    ioUv0 = iUv0;
    ioColor0 = iColor0;

    gl_Position = vec4(iPosition0, 1.0) * uModelMatrix * uViewMatrix * uProjectionMatrix;
}
";

        const string fragmentShaderSource = @"
#version 330

uniform sampler2D uTexture0;
uniform sampler2D uTexture1;

in vec4 ioColor0;
in vec2 ioUv0;

out vec4 oColor0;

void main()
{
    oColor0 = mix(texture(uTexture0, ioUv0), texture(uTexture1, ioUv0), 0.5) * ioColor0; 
}
";

        private float[] _vertices =
        {
            // Position             // colors               // texture
             0.5f, -0.5f, 0.0f,     1.0f, 0.0f, 0.0f, 1.0f,       1.0f, 0.0f,        1.0f, 0.0f, // bottom right
             0.5f,  0.5f, 0.0f,     0.0f, 0.0f, 1.0f, 1.0f,       1.0f, 1.0f,        1.0f, 1.0f, // top right
            -0.5f, -0.5f, 0.0f,     0.0f, 1.0f, 0.0f, 1.0f,       0.0f, 0.0f,        0.0f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f,     0.0f, 0.0f, 1.0f, 1.0f,       0.0f, 1.0f,        0.0f, 1.0f, // top left
        };

        private bool _enableLocalVariables = false;

        //private uint[] _indices =
        //{
        //    0, 1, 2, 
        //    1, 3, 2,
        //};

        //private int _elementBufferObject;

        //private int _vertexBufferObject;

        //private int _vertexArrayObject;

        private Shader _shader;

        private Texture _texture;

        private Texture _texture2;

        private Camera _camera;

        private bool _firstMove = true;

        private Vector2 _lastPos;

        private VertexBuffer _vertexBuffer;

        private Random _random = new Random(0);

        private ObjectInfo[] _objects = new ObjectInfo[20];

        private class ObjectInfo
        {
            public Vector3 Position;
            public Vector3 Scale;
            public Axis Axis;
            public float RotateSpeed;
            public float Angle;
        }


        public void Load()
        {
            for (var i = 0; i < _objects.Length; i++)
            {
                var scale = (float)(0.5 + _random.NextDouble() * 4);

                _objects[i] = new ObjectInfo
                {
                    Position = new Vector3((float)_random.Next(-10, 10), (float)_random.Next(-10, 10), (float)_random.Next(-10, 10)),
                    Scale = new Vector3(scale, scale, scale),
                    Axis = (Axis)_random.Next((int)Axis.X, (int)(Axis.Z) + 1),
                    RotateSpeed = (float)_random.NextDouble() * 360f,
                    Angle = (float)_random.NextDouble() * 360f,
                };
            }

            if (_enableLocalVariables)
            { 
                _vertexBuffer = new VertexBuffer(
                    12,
                    new[] {
                        new VertexBufferLayout.Part { VertexFlag = VertexFlag.Position0, ElementType = VertexBufferLayout.ElementType.Single, Count = 3 },
                        new VertexBufferLayout.Part { VertexFlag = VertexFlag.Color0, ElementType = VertexBufferLayout.ElementType.Single, Count = 4 },
                        new VertexBufferLayout.Part { VertexFlag = VertexFlag.Uv0, ElementType = VertexBufferLayout.ElementType.Single, Count = 2 },
                        new VertexBufferLayout.Part { VertexFlag = VertexFlag.Uv1, ElementType = VertexBufferLayout.ElementType.Single, Count = 2 },
                    },
                    VertexBufferUsage.StaticDraw);

                _vertexBuffer.Bind();
                _vertexBuffer.Write(ref _vertices);
            }
            else
            {
                Graphics.VertexBuffer.Bind();
                Graphics.VertexBuffer.Write(ref _vertices);
            }

            if(_enableLocalVariables)
            {
                _shader = new Shader(vertexShaderSource, fragmentShaderSource);
            }
            else
            {
                _shader = new Shader(vertexShaderSource, fragmentShaderSource);

                Graphics.ActiveShader(_shader);
            }

            _texture = new Texture("resources/textures/orange-transparent-1024x1024.png");

            _texture2 = new Texture("resources/textures/smiley-transparent-1024x1024.png");

            _camera = new Camera(Vector3.UnitZ * 5, (float)WindowWidth, (float)WindowHeight);

            Graphics.ClearColor(1f, 1f, 1f, 1.0f);
        }

        public void UnLoad()
        {
            _texture.Dispose();
            _texture2.Dispose();
        }

        public void Render()
        {
            if (_enableLocalVariables)
            {
                GL.Enable(EnableCap.DepthTest);

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                _vertexBuffer.Bind();
                _vertexBuffer.EnableElements(_shader);
            }
            else
            {
                Graphics.Enable(EnableFlag.DepthTest);

                Graphics.ClearBuffers();

                Graphics.ActiveRenderPrimitiveType = PrimitiveType.TriangleStrip;

            }

            _texture.UnitAndBind(TextureUnit.Texture0);
            _texture2.UnitAndBind(TextureUnit.Texture1);

            if (_enableLocalVariables)
            {
                _shader.Use();
                _shader.Uniform("uViewMatrix", _camera.GetViewMatrix());
                _shader.Uniform("uProjectionMatrix", _camera.GetProjectionMatrix());
                _shader.Uniform("uTexture0", 0);
                _shader.Uniform("uTexture1", 1);
            }
            else
            {
                Graphics.ActiveShader().Use();
                Graphics.ActiveShader().Uniform("uViewMatrix", _camera.GetViewMatrix());
                Graphics.ActiveShader().Uniform("uProjectionMatrix", _camera.GetProjectionMatrix());
                Graphics.ActiveShader().Uniform("uTexture0", 0);
                Graphics.ActiveShader().Uniform("uTexture1", 1);
            }


            Graphics.PushMatrix();

            for (var i = 0; i < _objects.Length; i++)
            {
                var model = Matrix4.CreateScale(_objects[i].Scale);

                model *= _objects[i].Axis == Axis.X
                    ? Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(_objects[i].Angle))
                    : _objects[i].Axis == Axis.Y
                        ? Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(_objects[i].Angle))
                        : Matrix4.CreateRotationZ((float)MathHelper.DegreesToRadians(_objects[i].Angle));

                model *= Matrix4.CreateTranslation(_objects[i].Position);

                if (_enableLocalVariables)
                {
                    _shader.Uniform("uModelMatrix", model);

                    GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
                }
                else
                {
                    Graphics.EnableVertexBuffer(VertexFlag.Position0 | VertexFlag.Color0 | VertexFlag.Uv0);

                    Graphics.ActiveShader().Uniform("uModelMatrix", model);

                    Graphics.Render(0, 4);
                }
            }

            Graphics.PopMatrix();


            //Graphics.PushMatrix();

            //Graphics.Quad();

            //Graphics.PopMatrix();

        }

        public void Update()
        {
            for (var i = 0; i < _objects.Length; i++)
            {
                _objects[i].Angle += (float)(_objects[i].RotateSpeed * ElapsedTime);
            }

            var input =  KeyboardState;

            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.2f;

            if (input.IsKeyDown(Keys.W))
            {
                _camera.Position += _camera.Front * cameraSpeed * (float)ElapsedTime; // Forward
            }

            if (input.IsKeyDown(Keys.S))
            {
                _camera.Position -= _camera.Front * cameraSpeed * (float)ElapsedTime; // Backwards
            }
            if (input.IsKeyDown(Keys.A))
            {
                _camera.Position -= _camera.Right * cameraSpeed * (float)ElapsedTime; // Left
            }
            if (input.IsKeyDown(Keys.D))
            {
                _camera.Position += _camera.Right * cameraSpeed * (float)ElapsedTime; // Right
            }
            if (input.IsKeyDown(Keys.Space))
            {
                _camera.Position += _camera.Up * cameraSpeed * (float)ElapsedTime; // Up
            }
            if (input.IsKeyDown(Keys.LeftShift))
            {
                _camera.Position -= _camera.Up * cameraSpeed * (float)ElapsedTime; // Down
            }

            // Get the mouse state
            var mouse = MouseState;

            if (_firstMove) // This bool variable is initially set to true.
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                // Calculate the offset of the mouse position
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                _camera.Yaw += deltaX * sensitivity;
                _camera.Pitch -= deltaY * sensitivity; // Reversed since y-coordinates range from bottom to top
            }

            //_angle += (float)(45f * ElapsedTime);
        }

        public void Resize(ResizeEventArgs a)
        {
            _camera.AspectRatio = a.Size.X / (float)a.Size.Y;
        }

        public void MouseWheel(MouseWheelEventArgs a)
        {
            _camera.Fov -= a.Offset.Y;
        }
    }
}
