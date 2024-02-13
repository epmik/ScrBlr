using Silk.NET.OpenGL;
using Silk.NET.Input;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using System.Numerics;
using Scrblr.Core;
using FontStashSharp;

namespace Scrblr.Learning
{
    // Be warned, there is a LOT of stuff here. It might seem complicated, but just take it slow and you'll be fine.
    // OpenGL's initial hurdle is quite large, but once you get past that, things will start making more sense.
    public class Learn014CoordinatesSystems : SilkSketch20240207
    {
        //private uint _elementBufferObject;

        /// <summary>
        /// default == ProjectionMode.Perspective
        /// </summary>
        public ProjectionMode ProjectionMode { get; set; } = ProjectionMode.Perspective;

        private uint _vertexBufferObject;

        private uint _vertexArrayObject;

        private Core.Shader _shader;

        private Core.Texture _texture;

        private Core.Texture _texture2;

        private static FontRenderer _fontRenderer;
        private static FontSystem _fontSystem;

        bool _animate = false;

        float _angle = 0f;
        float _angleRotationPerSecond = 45f;

        float _textureMix = 0.5f;
        private float _textureMixOffsetPerSecond = 0.2f;

        // Then, we create two matrices to hold our view and projection. They're initialized at the bottom of OnLoad.
        // The view matrix is what you might consider the "camera". It represents the current viewport in the window.
        private Matrix4x4 _view;

        // This represents how the vertices will be projected. It's hard to explain through comments,
        // so check out the web version for a good demonstration of what this does.
        private Matrix4x4 _projection;

        private Vector3 _modelPosition = new Vector3(0, 0, 0);
        private Vector3 _modelPositionOffsetPerSecond = new Vector3(1, 0, 0);

        private Vector3 _viewPosition = new Vector3(0, 0, -3f);
        private Vector3 _viewPositionOffsetPerSecond = new Vector3(0, 0, 2);

        public float CanvasWidth = 1;
        public float CanvasHeight = 1;
        public float Near = 0.1f;
        public float Far = 100f;

        /// <summary>
        /// field of view in degrees
        /// <para>
        /// this can be the vertical (default) or horizontal field of view
        /// </para>
        /// <para>
        /// if the window is higher than it's width, then the Fov is considered horizontal
        /// </para>
        /// </summary>
        public float Fov = 45f;

        private int _windowWidth = 640;
        private int _windowHeight = 640;

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

        public Learn014CoordinatesSystems()
        {

            var options = Silk.NET.Windowing.WindowOptions.Default;
            //options.Size = new Vector2D<int>(_windowWidth, _windowHeight);
            options.Size = CalculateWindowSize(CanvasWidth, CanvasHeight);
            options.Title = "LearnOpenGL with Silk.NET";
            Context.Window = window = Window.Create(options);

            window.Load += OnLoad;
            window.Render += OnRenderFrame;
            window.Update += OnUpdateFrame;
            window.Resize += OnResize;
            window.Closing += OnUnLoad;

            Console.WriteLine($"Window size is: {options.Size.X}x{options.Size.Y}");
        }

        protected unsafe void OnLoad()
        {
            Context.GL = GL = GL.GetApi(window);

            inputContext = window.CreateInput();

            for (int i = 0; i < inputContext.Keyboards.Count; i++)
            {
                inputContext.Keyboards[i].KeyDown += KeyDown;
            }

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

            //uint[] _indices =
            //{
            //    0, 1, 3,
            //    1, 2, 3
            //};

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

            //_elementBufferObject = GL.GenBuffer();
            //GL.BindBuffer(GLEnum.ElementArrayBuffer, _elementBufferObject);
            //fixed (void* i = &_indices[0])
            //    GL.BufferData(GLEnum.ElementArrayBuffer, (nuint)(_indices.Length * sizeof(uint)), i, GLEnum.StaticDraw);

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

            _texture =  Core.Texture.LoadFromFile(GL, ".resources/container.png");
            _texture.Use(TextureUnit.Texture0);

            _texture2 = Core.Texture.LoadFromFile(GL, ".resources/awesomeface.png");
            _texture2.Use(TextureUnit.Texture1);

            _shader.SetInt("texture0", 0);
            _shader.SetInt("texture1", 1);

            UpdateProjectionMatrix();

            var fovRadians = (float)Utility.DegreesToRadians(Fov);

            float extent = CanvasWidth * 0.5f;
            var z = extent / (float)Math.Sin(fovRadians * 0.5f);

            _viewPosition = new Vector3(0, 0, -(z - Near));


            _fontRenderer = new FontRenderer();

            //var settings = new FontSystemSettings
            //{
            //    FontResolutionFactor = 2,
            //    KernelWidth = 2,
            //    KernelHeight = 2
            //};

            //_fontSystem = new FontSystem(settings);
            _fontSystem = new FontSystem();
            //_fontSystem.AddFont(File.ReadAllBytes(@".resources/.fonts/droidsans.ttf"));
            //_fontSystem.AddFont(File.ReadAllBytes(@".resources/.fonts/Roboto-Black.ttf"));
            _fontSystem.AddFont(File.ReadAllBytes(@".resources/.fonts/Ubuntu-Regular.ttf"));
            _fontSystem.AddFont(File.ReadAllBytes(@".resources/.fonts/droidsansjapanese.ttf"));
            _fontSystem.AddFont(File.ReadAllBytes(@".resources/.fonts/symbola-emoji.ttf"));

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

            ////// Next, we scale the matrix. This will make the rectangle slightly larger.
            //model = model * Matrix4x4.CreateScale(1.0f, 1f, 1f);

            ////// Then, we translate the matrix, which will move it slightly towards the top-right.
            ////// Note that we aren't using a full coordinate system yet, so the translation is in normalized device coordinates.
            ////// The next tutorial will be about how to set one up so we can use more human-readable numbers.
            ////model = model * Matrix4x4.CreateTranslation(0.1f, 0.1f, 0.0f);

            //// Finally, we have the model matrix. This determines the position of the model.
            //model *= Matrix4x4.CreateRotationZ((float)Utility.DegreesToRadians(_angle));

            model *= Matrix4x4.CreateTranslation(_modelPosition);

            // For the view, we don't do too much here. Next tutorial will be all about a Camera class that will make it much easier to manipulate the view.
            // For now, we move it backwards three units on the Z axis.
            _view = Matrix4x4.CreateTranslation(_viewPosition);

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

            _shader.SetFloat("textureMix", _textureMix);

            // And that's it for now! In the next tutorial, we'll see how to setup a full coordinates system.

            //GL.DrawElements(GLEnum.Triangles, (uint)(6), GLEnum.UnsignedInt, (void*)0);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);




            var text = "The quick いろは brown\nfox にほへ jumps over\nt🙌h📦e l👏a👏zy dog";
            //var scale = new Vector2(2, 2);

            var font = _fontSystem.GetFont(18);

            //var size = font.MeasureString(text, scale);
            //var origin = new Vector2(size.X / 2.0f, size.Y / 2.0f);

            _fontRenderer.Begin();

            //font.DrawText(_fontRenderer, text, new Vector2(400, 400), FSColor.Yellow, _rads, origin, scale);

            font.DrawText(_fontRenderer, text, new Vector2(20, 20), FSColor.White);

            _fontRenderer.End();

        }

        protected void OnUpdateFrame(double elapsedTime)
        {
            if(_animate)
            {
                _angle += (float)(_angleRotationPerSecond * elapsedTime);

                if (_angle >= 360f)
                {
                    _angle -= 360f;
                }

                _modelPosition += _modelPositionOffsetPerSecond * (float)elapsedTime;

                if (_modelPosition.X > 1.5 || _modelPosition.X < -1.5)
                {
                    _modelPositionOffsetPerSecond *= -1;
                    _modelPosition.X = -1.5f * _modelPositionOffsetPerSecond.X;
                }

                _textureMix += _textureMixOffsetPerSecond * (float)elapsedTime;

                if (_textureMix > 1 || _textureMix < 0)
                {
                    _textureMixOffsetPerSecond *= -1;
                }
            }

            _rads += 0.01f;

            //viewPosition += viewPositionOffsetPerSecond * (float)elapsedTime;

            //if (viewPosition.Z > 0 || viewPosition.Z < -6)
            //{
            //    viewPositionOffsetPerSecond *= -1;
            //}
        }

        protected unsafe void OnUnLoad()
        {
            GL.DeleteBuffer(_vertexBufferObject);
            //GL.DeleteBuffer(_elementBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);

            _shader.Delete();

            _fontRenderer.Dispose();
        }

        private void KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            switch(key)
            {
                case Key.Escape:
                    window.Close();
                    break;
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

        protected void OnResize(Vector2D<int> size)
        {
            GL.Viewport(0, 0, (uint)size.X, (uint)size.Y);

            UpdateProjectionMatrix();
        }

        protected void UpdateProjectionMatrix()
        {
            var fovRadians = (float)Utility.DegreesToRadians(Fov);

            var aspectRatioHorizontal = ((float)window.Size.X / (float)window.Size.Y);
            var aspectRatioVertical = ((float)window.Size.Y / (float)window.Size.X);

            switch (ProjectionMode)
            {
                case ProjectionMode.Orthographic:
                    if (aspectRatioHorizontal >= aspectRatioVertical)
                    {
                        _projection = Matrix4x4.CreateOrthographic(CanvasWidth * aspectRatioHorizontal, CanvasHeight, Near, Far);
                    }
                    else
                    {
                        _projection = Matrix4x4.CreateOrthographic(CanvasWidth, CanvasHeight * aspectRatioVertical, Near, Far);
                    }
                    break;
                case ProjectionMode.Perspective:
                    if (aspectRatioHorizontal >= aspectRatioVertical)
                    {
                        var top = (float)Math.Tan(fovRadians * 0.5f) * Near;
                        var bottom = -top;
                        var right = top * aspectRatioHorizontal;
                        var left = -right;
                        _projection = Matrix4x4.CreatePerspective(right - left, top - bottom, Near, Far);
                    }
                    else
                    {
                        var right = (float)Math.Tan(fovRadians * 0.5f) * Near;
                        var left = -right;
                        var top = right * aspectRatioVertical;
                        var bottom = -top;
                        _projection = Matrix4x4.CreatePerspective(right - left, top - bottom, Near, Far);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }


        private readonly int[] AvailableSizes = { 3840, 3440, 2560, 2160, 2048, 1920, 1600, 1440, 1360, 1280, 1152, 1024, 800, 720, 640, 480, 360 };

        private Vector2D<int> CalculateWindowSize(float canvasWidth, float canvasHeight)
        {
            var primaryMonitor = Silk.NET.Windowing.Monitor.GetMainMonitor(null);

            var targetWindowWidth = AvailableSizes.First(o => o < primaryMonitor.Bounds.Size.X);
            var targetWindowHeight = AvailableSizes.First(o => o < primaryMonitor.Bounds.Size.Y);

            var windowWidth = (int)(targetWindowHeight * canvasWidth / canvasHeight);
            var windowHeight = targetWindowHeight;

            if (windowWidth > targetWindowWidth || windowHeight > targetWindowWidth)
            {
                windowWidth = targetWindowWidth;
                windowHeight = (int)(targetWindowWidth * canvasHeight / canvasWidth);
            }

            return new Vector2D<int>(windowWidth, windowHeight);
        }
    }
}