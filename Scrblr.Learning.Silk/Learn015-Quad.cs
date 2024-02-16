using Silk.NET.OpenGL;
using Silk.NET.Input;
using Silk.NET.Maths;
using System.Numerics;
using Scrblr.Core;

namespace Scrblr.Learning
{
    // Be warned, there is a LOT of stuff here. It might seem complicated, but just take it slow and you'll be fine.
    // OpenGL's initial hurdle is quite large, but once you get past that, things will start making more sense.
    public class Learn015Quad : Sketch
    {

        private uint _vertexBufferObject;

        private uint _vertexArrayObject;

        private Core.Shader _shader;

        private Core.Texture _texture;

        private Core.Texture _texture2;

        bool _animate = false;

        float _angle = 0f;
        float _angleRotationPerSecond = 45f;

        float _textureMix = 0.5f;
        private float _textureMixOffsetPerSecond = 0.2f;

        private Vector3 _modelPosition = new Vector3(0, 0, 0);
        private Vector3 _modelPositionOffsetPerSecond = new Vector3(1, 0, 0);

        private Vector3 _viewPosition = new Vector3(0, 0, -3f);
        private Vector3 _viewPositionOffsetPerSecond = new Vector3(0, 0, 2);

        string _vertexShaderSource = @"
#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec4 vColor;

out vec2 texCoord;
out vec4 fColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main(void)
{
    texCoord = aTexCoord;
    fColor = vColor;

    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
}
";

        string _fragmentShaderSource = @"
#version 330

out vec4 outputColor;

in vec2 texCoord;
in vec4 fColor;

uniform sampler2D texture0;
uniform sampler2D texture1;
uniform float textureMix;

void main()
{
    outputColor = mix(texture(texture0, texCoord), texture(texture1, texCoord), textureMix) * fColor;
}
";

        private static float _rads = 0.0f;

        unsafe void Load()
        {
            // clockwise winding
            float[] _vertices =
            {
                 // xyz             uv          rgba
                 0.5f,  0.5f, 0.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f, // top right red
                 0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f, // bottom right green
                -0.5f,  0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f, // top left blue

                 0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 0.5f, 0.5f, 0.5f, 1.0f, // bottom right grey
                -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, // bottom left black
                -0.5f,  0.5f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, // top left white
            };

            // background color
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            // enable clockwise winding
            GL.FrontFace(FrontFaceDirection.CW);

            // cull the backface
            GL.CullFace(GLEnum.Back);

            // enable culling
            GL.Enable(EnableCap.CullFace);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(GLEnum.ArrayBuffer, _vertexBufferObject);
            fixed (void* v = &_vertices[0])
                GL.BufferData(GLEnum.ArrayBuffer, (nuint)(_vertices.Length * sizeof(float)), v, GLEnum.StaticDraw);

            _shader = new Core.Shader(GL, _vertexShaderSource, _fragmentShaderSource); _shader.Use();

            var vertexLocation = _shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, GLEnum.Float, false, 9 * sizeof(float), (void*)0);

            var texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, GLEnum.Float, false, 9 * sizeof(float), (void*)(3 * sizeof(float)));

            var colorLocation = _shader.GetAttribLocation("vColor");
            GL.EnableVertexAttribArray(colorLocation);
            GL.VertexAttribPointer(colorLocation, 4, GLEnum.Float, false, 9 * sizeof(float), (void*)(5 * sizeof(float)));

            _texture = Core.Texture.LoadFromFile(GL, ".resources/container.png");
            _texture.Use(TextureUnit.Texture0);

            _texture2 = Core.Texture.LoadFromFile(GL, ".resources/awesomeface.png");
            _texture2.Use(TextureUnit.Texture1);

            _shader.SetInt("texture0", 0);
            _shader.SetInt("texture1", 1);

            UpdateProjectionMatrix();

            var fovRadians = (float)Utility.DegreesToRadians(Fov);

            float extent = Width * 0.5f;
            var z = extent / (float)Math.Sin(fovRadians * 0.5f);

            _viewPosition = new Vector3(0, 0, -(z - Near));
        }

        void Update()
        {
            if (_animate)
            {
                _angle += (float)(_angleRotationPerSecond * ElapsedTime);

                if (_angle >= 360f)
                {
                    _angle -= 360f;
                }

                _modelPosition += _modelPositionOffsetPerSecond * (float)ElapsedTime;

                if (_modelPosition.X > 1.5 || _modelPosition.X < -1.5)
                {
                    _modelPositionOffsetPerSecond *= -1;
                    _modelPosition.X = -1.5f * _modelPositionOffsetPerSecond.X;
                }

                _textureMix += _textureMixOffsetPerSecond * (float)ElapsedTime;

                if (_textureMix > 1 || _textureMix < 0)
                {
                    _textureMixOffsetPerSecond *= -1;
                }
            }

            _rads += 0.01f;
        }

        void Render()
        {
            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Quad(0.5f)
                //.Translate(0.25f, -0.25f)
                .Color(1f, 0f, 0f);

            //Quad(0.5f)
            //    .Color(1f, 0f, 0f)
            //    .Texture(".resources/container.png")
            //    .Position(0.25f, -0.25f)
            //    .Scale(0.5f)
            //    .Rotate(45f);

            GL.BindVertexArray(_vertexArrayObject);

            // Note: The matrices we'll use for transformations are all 4x4.

            // We start with an identity matrix. This is just a simple matrix that doesn't move the vertices at all.
            ModelMatrix = Matrix4x4.Identity;

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

            ////// Next, we scale the matrix. This will make the rectangle slightly larger.
            //model = model * Matrix4x4.CreateScale(1.0f, 1f, 1f);

            ////// Then, we translate the matrix, which will move it slightly towards the top-right.
            ////// Note that we aren't using a full coordinate system yet, so the translation is in normalized device coordinates.
            ////// The next tutorial will be about how to set one up so we can use more human-readable numbers.
            ////model = model * Matrix4x4.CreateTranslation(0.1f, 0.1f, 0.0f);

            //// Finally, we have the model matrix. This determines the position of the model.
            //model *= Matrix4x4.CreateRotationZ((float)Utility.DegreesToRadians(_angle));

            ModelMatrix *= Matrix4x4.CreateTranslation(_modelPosition);

            // For the view, we don't do too much here. Next tutorial will be all about a Camera class that will make it much easier to manipulate the view.
            // For now, we move it backwards three units on the Z axis.
            ViewMatrix = Matrix4x4.CreateTranslation(_viewPosition);

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
            _shader.SetMatrix4("model", ModelMatrix);
            _shader.SetMatrix4("view", ViewMatrix);
            _shader.SetMatrix4("projection", ProjectionMatrix);

            _shader.SetFloat("textureMix", _textureMix);

            // And that's it for now! In the next tutorial, we'll see how to setup a full coordinates system.

            //GL.DrawElements(GLEnum.Triangles, (uint)(6), GLEnum.UnsignedInt, (void*)0);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            WriteText("The quick いろは brown\nfox にほへ jumps over\nt🙌h📦e l👏a👏zy dog", 20, 20);
        }

        void Closing()
        {
            GL.DeleteBuffer(_vertexBufferObject);
            //GL.DeleteBuffer(_elementBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);

            _shader.Delete();
        }


        void KeyDown(IKeyboard keyboard, Key key, int code)
        {
            switch (key)
            {
                case Key.Space:
                    _animate = !_animate;
                    break;
                case Key.P:
                    ProjectionMode = ProjectionMode.Next();
                    Console.WriteLine($"Switched ProjectionMode to: {ProjectionMode}");
                    UpdateProjectionMatrix();
                    break;
            }
        }

        
        void Resize(Vector2D<int> size)
        {

        }
    }
}