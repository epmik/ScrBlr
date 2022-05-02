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
    [Sketch(Name = "Learn019-VertexBufferClass-TextureClass")]
    public class Learn019 : AbstractSketch20220410
    {
        public Learn019()
            : base(10, 10)
        {
            LoadAction += Load;
            UnLoadAction += UnLoad;
            RenderAction += Render;
            UpdateAction += Update;

            Samples = 8;
        }

        const string vertexShaderSource = @"
#version 330 core

uniform mat4 uModelMatrix;
uniform mat4 uViewMatrix;
uniform mat4 uProjectionMatrix;

layout(location = 0) in vec3 iPosition;
layout(location = 1) in vec3 iColor;
layout(location = 2) in vec2 iUv1;

out vec2 ioUv1;
out vec3 ioColor;

void main(void)
{
    ioUv1 = iUv1;
    ioColor = iColor;

    gl_Position = vec4(iPosition, 1.0) * uModelMatrix * uViewMatrix * uProjectionMatrix;
}
";

        const string fragmentShaderSource = @"
#version 330

uniform sampler2D uTexture0;
uniform sampler2D uTexture1;

in vec3 ioColor;
in vec2 ioUv1;

out vec4 oColor;

void main()
{
    //oColor = mix(texture(uTexture0, ioUv1), texture(uTexture1, ioUv1), 0.2);
    oColor = mix(texture(uTexture0, ioUv1), texture(uTexture1, ioUv1), 0.5) * vec4(ioColor, 1.0); 
}
";

        private float[] _vertices =
        {
            // Position             // colors               // texture
             0.5f, -0.5f, 0.0f,     1.0f, 0.0f, 0.0f,       1.0f, 0.0f, // bottom right
             0.5f,  0.5f, 0.0f,     0.0f, 0.0f, 1.0f,       1.0f, 1.0f, // top right
            -0.5f, -0.5f, 0.0f,     0.0f, 1.0f, 0.0f,       0.0f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f,     0.0f, 0.0f, 1.0f,       0.0f, 1.0f  // top left
        };

        private uint[] _indices =
        {
            0, 1, 2, 
            1, 3, 2,
        };

        private int _elementBufferObject;

        //private int _vertexBufferObject;

        private int _vertexArrayObject;

        private Shader20220413 _shader;

        private Texture20220413 _texture;

        private Texture20220413 _texture2;

        private FirstPersonCamera _camera;

        private VertexBuffer20220413 _vertexBuffer;

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
                    Position = new Vector3((float)_random.Next(-10, 10), (float)_random.Next(-10, 10), (float)_random.Next(-16, 2)),
                    Scale = new Vector3(scale, scale, scale),
                    Axis = (Axis)_random.Next((int)Axis.X, (int)(Axis.Z) + 1),
                    RotateSpeed = (float)_random.NextDouble() * 360f,
                    Angle = (float)_random.NextDouble() * 360f,
                };
            }

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBuffer = new VertexBuffer20220413(
             12,
             new[] {
                    new VertexBufferLayout20220413.Part { Identifier = VertexBufferLayout20220413.PartIdentifier.Position0, Type = VertexBufferLayout20220413.ElementType.Single, Count = 3 },
                    new VertexBufferLayout20220413.Part { Identifier = VertexBufferLayout20220413.PartIdentifier.Color0, Type = VertexBufferLayout20220413.ElementType.Single, Count = 3 },
                    new VertexBufferLayout20220413.Part { Identifier = VertexBufferLayout20220413.PartIdentifier.Uv0, Type = VertexBufferLayout20220413.ElementType.Single, Count = 2 },
             },
             VertexBufferUsage.StaticDraw);

            _vertexBuffer.Bind();

            _vertexBuffer.Write(ref _vertices);



            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            _shader = new Shader20220413(vertexShaderSource, fragmentShaderSource);
            _shader.Use();

            var vertexLocation = _shader.AttributeLocation("iPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);

            var colorLocation = _shader.AttributeLocation("iColor");
            GL.EnableVertexAttribArray(colorLocation);
            GL.VertexAttribPointer(colorLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));

            var texCoordLocation = _shader.AttributeLocation("iUv1");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));

            _texture = new Texture20220413("resources/textures/orange-transparent-1024x1024.png");
            _texture.UnitAndBind(TextureUnit.Texture0);

            _texture2 = new Texture20220413("resources/textures/smiley-transparent-1024x1024.png");
            _texture2.UnitAndBind(TextureUnit.Texture1);

            _shader.Uniform("uTexture0", 0);
            _shader.Uniform("uTexture1", 1);

            _camera = new FirstPersonCamera
            {
                Fov = 90f,
                AspectRatio = SketchWidth / SketchHeight,
                Near = 1f,
                Far = 100f,
                Position = new Vector3(0, 0, 2),
            };

            UpdateAction += _camera.Update;
            MouseWheelAction += _camera.MouseWheel;
            ResizeAction += _camera.Resize;

            // We make the mouse cursor invisible and captured so we can have proper FPS-camera movement.
            //CursorGrabbed = true;
        }

        public void UnLoad()
        {
            _texture.Dispose();
            _texture2.Dispose();
        }

        public void Render()
        {
            ClearColor(1f, 1f, 1f, 1.0f);

            GL.Enable(EnableCap.DepthTest);
            //GL.CullFace(CullFaceMode.Back);
            //GL.Enable(EnableCap.CullFace);

            Clear(ClearFlag.ColorBuffer);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(_vertexArrayObject);

            _vertexBuffer.Bind();

            _vertexBuffer.EnableElements();


            _texture.UnitAndBind(TextureUnit.Texture0);
            _texture2.UnitAndBind(TextureUnit.Texture1);
            _shader.Use();

            _shader.Uniform("uViewMatrix", _camera.ViewMatrix());

            for(var i = 0; i < _objects.Length; i++)
            {
                // see https://stackoverflow.com/a/55009832/527843
                var distance = (_objects[i].Position - Camera.Position).Length;

                var size_y = _camera.DepthRatio * distance;
                var size_x = _camera.DepthRatio * distance * _camera.AspectRatio;

                var ortho = Matrix4.CreateOrthographicOffCenter(-size_x, size_x, -size_y, size_y, 0.0f, 2.0f * distance);

                _shader.Uniform("uProjectionMatrix", _camera.ProjectionMatrix());

                var model = Matrix4.CreateScale(_objects[i].Scale);

                model *= _objects[i].Axis == Axis.X
                    ? Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(_objects[i].Angle))
                    : _objects[i].Axis == Axis.Y
                        ? Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(_objects[i].Angle))
                        : Matrix4.CreateRotationZ((float)MathHelper.DegreesToRadians(_objects[i].Angle));

                model *= Matrix4.CreateTranslation(_objects[i].Position);

                _shader.Uniform("uModelMatrix", model);

                GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
            }





            //_shader.Use();

            //_vertexBuffer.Bind();

            //_vertexBuffer.EnableElements();

            //GL.ActiveTexture(TextureUnit.Texture0);
            //_texture0.Bind();

            //GL.ActiveTexture(TextureUnit.Texture1);
            //_texture1.Bind();

            //var model = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(_angle));
            //model *= Matrix4.CreateTranslation(100.0f, 0.0f, 0.0f);

            //_shader.Uniform("model", model);
            //_shader.Uniform("view", ViewMatrix);
            //_shader.Uniform("projection", ProjectionMatrix);
            //_shader.Uniform("uTexture0", 0);
            //_shader.Uniform("uTexture1", 1);

            //GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }

        public void Update(FrameEventArgs a)
        {
            _camera.ElapsedTime = ElapsedTime;
            _camera.KeyboardState = KeyboardState;
            _camera.MouseState = MouseState;

            for (var i = 0; i < _objects.Length; i++)
            {
                _objects[i].Angle += (float)(_objects[i].RotateSpeed * ElapsedTime);
            }
        }
    }
}
