using Silk.NET.OpenGL;
using Silk.NET.Input;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using System.Numerics;
using Scrblr.Leaning;
using static Scrblr.LearnOpenTK.Learn999BlenderFileReader;
using System.Xml.Linq;
using System.Xml.Serialization;
using System;


namespace Scrblr.LearnOpenTK
{
    // Be warned, there is a LOT of stuff here. It might seem complicated, but just take it slow and you'll be fine.
    // OpenGL's initial hurdle is quite large, but once you get past that, things will start making more sense.
    public class Learn999BlenderFileReader : SilkSketch
    {
        private uint _elementBufferObject;

        private uint _vertexBufferObject;

        private uint _vertexArrayObject;

        private Shader _shader;

        //private Texture _texture;

        //private Texture _texture2;

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

        private Vector3 viewPosition = new Vector3(0, 0, -3);
        private Vector3 viewPositionOffsetPerSecond = new Vector3(0, 0, -2);

        // We need an instance of the new camera class so it can manage the view and projection matrix code.
        // We also need a boolean set to true to detect whether or not the mouse has been moved for the first time.
        // Finally, we add the last position of the mouse so we can calculate the mouse offset easily.
        private Camera _camera;

        private bool _firstMove = true;

        private Vector2 _lastPos;

        private Vector2D<int> WindowCenter = new Vector2D<int>();

        private VAOBatch _VAOBatch = new VAOBatch();

        string vertexShaderSource = @"
#version 330 core

layout(location = 0) in vec3 aPosition;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main(void)
{
    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
}
";

        string fragmentShaderSource = @"
#version 330

uniform vec4 uColor; 

out vec4 outputColor;

void main()
{
    outputColor = uColor;
    //outputColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);
    //outputColor = mix(texture(texture0, texCoord), texture(texture1, texCoord), 0.2);
}
";

        public unsafe Learn999BlenderFileReader()
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

            //var b = BlendFileParser.Parse("resources/test-block.v365.blend");

            //var b1 = ObjFileReader.Parse("resources/quad-001.obj");
            //var b2 = ObjFileReader.Parse("resources/cube-001.obj");
            //var b3 = ObjFileReader.Parse("resources/test-objects-001.obj");
            //var b4 = ObjFileReader.Parse("resources/test-objects-002.obj");

            var importerContext = new Assimp.AssimpContext();

            var assimpApi = Silk.NET.Assimp.Assimp.GetApi();

            var name = "plane-001";
            //var name = "cube-001";
            //var name = "scene-001";

            var sceneDAE = importerContext.ImportFile($".resources/{name}.dae");
            var sceneFBX = importerContext.ImportFile($".resources/{name}.fbx");
            var sceneGLB = importerContext.ImportFile($".resources/{name}.glb");
            var sceneGLTF = importerContext.ImportFile($".resources/{name}.gltf");
            //var sceneOBJ = importerContext.ImportFile($".resources/{name}.obj");

            var sceneDAE2 = assimpApi.ImportFile($".resources/{name}.dae", 0);
            var sceneFBX2 = assimpApi.ImportFile($".resources/{name}.fbx", 0);
            var sceneGLB2 = assimpApi.ImportFile($".resources/{name}.glb", 0);
            var sceneGLTF2 = assimpApi.ImportFile($".resources/{name}.gltf", 0);
            //var sceneOBJ2 = assimpApi.ImportFile($".resources/{name}.obj", 0);
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

            //var sceneGLTF = new Assimp.AssimpContext().ImportFile($".resources/plane-001.gltf");
            var sceneGLTF = new Assimp.AssimpContext().ImportFile($".resources/cube-001.gltf");

            var mesh = sceneGLTF.Meshes[0];

            var indiceCount = (uint)(mesh.FaceCount * 3);

            var _vertices = new float[mesh.VertexCount * 3];

            var index = 0;
            foreach(var v in mesh.Vertices)
            {
                _vertices[index++] = v.X;
                _vertices[index++] = v.Y;
                _vertices[index++] = v.Z;
            }

            var _indices = new uint[indiceCount];

            index = 0;
            foreach (var f in mesh.Faces)
            {
                foreach (var indice in f.Indices)
                {
                    _indices[index++] = (uint)indice;
                }
            }

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();

            var size = _vertices.Length * sizeof(float) + _vertices.Length * sizeof(float) + _vertices.Length * sizeof(float);
            var offset = 0;

            GL.BindBuffer(GLEnum.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTargetARB.ArrayBuffer, (nuint)size, IntPtr.Zero, BufferUsageARB.StaticDraw); //Setting buffer data.

            fixed (void* v = &_vertices[0])
            {
                size = _vertices.Length * sizeof(float);

                GL.BufferSubData(GLEnum.ArrayBuffer, (nint)offset, (nuint)size, v);
                offset += size;

                GL.BufferSubData(GLEnum.ArrayBuffer, (nint)offset, (nuint)size, v);
                offset += size;

                GL.BufferSubData(GLEnum.ArrayBuffer, (nint)offset, (nuint)size, v);
                offset += size;
            }

            _elementBufferObject = GL.GenBuffer();

            size = _indices.Length * sizeof(uint) + _indices.Length * sizeof(uint) + _indices.Length * sizeof(uint);
            offset = 0;

            GL.BindBuffer(GLEnum.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)size, IntPtr.Zero, BufferUsageARB.StaticDraw); //Setting buffer data.

            fixed (void* i = &_indices[0])
            {
                size = _indices.Length * sizeof(uint);

                GL.BufferSubData(GLEnum.ElementArrayBuffer, (nint)offset, (nuint)size, i);
                offset += size;

                GL.BufferSubData(GLEnum.ElementArrayBuffer, (nint)offset, (nuint)size, i);
                offset += size;

                GL.BufferSubData(GLEnum.ElementArrayBuffer, (nint)offset, (nuint)size, i);
                offset += size;
            }

            _shader = new Shader(GL, vertexShaderSource, fragmentShaderSource); _shader.Use();

            //var texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            //GL.EnableVertexAttribArray(texCoordLocation);
            //GL.VertexAttribPointer(texCoordLocation, 2, GLEnum.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float)));

            //_texture = Texture.LoadFromFile(GL, "Resources/container.png");
            //_texture.Use(TextureUnit.Texture0);

            //_texture2 = Texture.LoadFromFile(GL, "Resources/awesomeface.png");
            //_texture2.Use(TextureUnit.Texture1);

            //_shader.SetInt("texture0", 0);
            //_shader.SetInt("texture1", 1);

            _VAOBatch.VAO = _vertexArrayObject;
            _VAOBatch.VBO = _vertexBufferObject;
            _VAOBatch.EBO = _elementBufferObject;

            _VAOBatch.Attributes.Add(new VertexAttribute
            {
                Name = "aPosition",
                Count = 3,
                Type = GLEnum.Float,
                Stride = sizeof(float) * 3,
                Offset = 0
            });

            _VAOBatch.Batches.Add(new Batch
            {
                Start = 0,
                Count = indiceCount,
            });

            _VAOBatch.Batches[_VAOBatch.Batches.Count - 1].Uniforms.Add("uColor", new Vector4(1f, 0f, 0f, 1f));

            _VAOBatch.Batches.Add(new Batch
            {
                Start = indiceCount,
                Count = indiceCount,
            });

            _VAOBatch.Batches[_VAOBatch.Batches.Count - 1].Uniforms.Add("uColor", new Vector4(1f, 1f, 0f, 1f));

            _VAOBatch.Batches.Add(new Batch
            {
                Start = indiceCount + indiceCount,
                Count = indiceCount,
            });

            _VAOBatch.Batches[_VAOBatch.Batches.Count - 1].Uniforms.Add("uColor", new Vector4(1f, 0f, 1f, 1f));

            GL.BindVertexArray(0);
            GL.BindBuffer(GLEnum.ArrayBuffer, 0);
            GL.BindBuffer(GLEnum.ElementArrayBuffer, 0);

            // For the matrix, we use a few parameters.
            //   Field of view. This determines how much the viewport can see at once. 45 is considered the most "realistic" setting, but most video games nowadays use 90
            //   Aspect ratio. This should be set to Width / Height.
            //   Near-clipping. Any vertices closer to the camera than this value will be clipped.
            //   Far-clipping. Any vertices farther away from the camera than this value will be clipped.
            _projection = Matrix4x4.CreatePerspectiveFieldOfView((float)Utility.DegreesToRadians(45f), (float)window.Size.X / (float)window.Size.Y, 0.1f, 100.0f);


            // We initialize the camera so that it is 3 units back from where the rectangle is.
            // We also give it the proper aspect ratio.
            _camera = new Camera(Vector3.UnitZ * 3, window.Size.X / (float)window.Size.Y);

            HideCursor();

            //ConfineCursor();

            CenterCursor();
        }

        protected void OnUpdateFrame(double elapsedTime)
        {
            angle += (float)(angleRotationPerSecond * elapsedTime);

            if (angle >= 360f)
            {
                angle -= 360f;
            }

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

            // Calculate the offset of the mouse position
            var deltaX = Mouse.Position.X - WindowCenter.X;
            var deltaY = Mouse.Position.Y - WindowCenter.Y;

            Mouse.Position = new Vector2(WindowCenter.X, WindowCenter.Y);

            // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
            _camera.Yaw += deltaX * sensitivity;
            _camera.Pitch -= deltaY * sensitivity; // Reversed since y-coordinates range from bottom to top
            //}

            _VAOBatch.Batches[0].Model = Matrix4x4.CreateRotationX((float)Utility.DegreesToRadians(angle));
            _VAOBatch.Batches[1].Model = Matrix4x4.CreateRotationY((float)Utility.DegreesToRadians(angle)) * Matrix4x4.CreateTranslation(new Vector3(3, 0, 0));
            _VAOBatch.Batches[2].Model = Matrix4x4.CreateRotationZ((float)Utility.DegreesToRadians(angle)) * Matrix4x4.CreateTranslation(new Vector3(-3, 0, 0));
        }

        protected unsafe void OnRenderFrame(double elapsedTime)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // see binding/ubinding https://www.songho.ca/opengl/gl_vbo.html
            GL.BindVertexArray(_VAOBatch.VAO);
            GL.BindBuffer(GLEnum.ArrayBuffer, _VAOBatch.VBO);
            GL.BindBuffer(GLEnum.ElementArrayBuffer, _VAOBatch.EBO);

            foreach(var atribute in _VAOBatch.Attributes)
            {
                var vertexLocation = _shader.GetAttribLocation(atribute.Name);
                GL.EnableVertexAttribArray(vertexLocation);
                GL.VertexAttribPointer(vertexLocation, atribute.Count, atribute.Type, false, atribute.Stride, (void*)atribute.Offset);
            }

            _view = _camera.GetViewMatrix();

            //_texture.Use(TextureUnit.Texture0);
            //_texture2.Use(TextureUnit.Texture1);
            _shader.Use();

            _shader.SetMatrix4("view", _view);
            _shader.SetMatrix4("projection", _projection);

            foreach(var batch in _VAOBatch.Batches)
            {
                _shader.Uniform("model", batch.Model);

                foreach (var uniform in batch.Uniforms)
                {
                    _shader.Uniform(uniform.Key, uniform.Value);
                }

                GL.DrawRangeElements(GLEnum.Triangles, (uint)batch.Start, (uint)(batch.Start + batch.Count), (uint)batch.Count, GLEnum.UnsignedInt, (void*)0);
            }



            //var model = Matrix4x4.CreateRotationX((float)Utility.DegreesToRadians(angle));
            //model *= Matrix4x4.CreateTranslation(modelPosition);

            //_shader.SetMatrix4("model", model);
            //_shader.SetVector4("uColor", new Vector4(1f, 0f, 0f, 1f));

            ////GL.DrawElements(GLEnum.Triangles, (uint)(3), GLEnum.UnsignedInt, (void*)0);
            //GL.DrawRangeElements(GLEnum.Triangles, (uint)(0), (uint)(6), (uint)(6), GLEnum.UnsignedInt, (void*)0);



            //model = Matrix4x4.Identity * Matrix4x4.CreateRotationZ((float)Utility.DegreesToRadians(angle));
            //model *= Matrix4x4.CreateTranslation(new Vector3(2, 0, 0));

            //_shader.SetMatrix4("model", model); 
            //_shader.SetVector4("uColor", new Vector4(1f, 0f, 1f, 1f));

            //GL.DrawRangeElements(GLEnum.Triangles, (uint)(6), (uint)(12), (uint)(6), GLEnum.UnsignedInt, (void*)0);



            //model = Matrix4x4.Identity * Matrix4x4.CreateRotationY((float)Utility.DegreesToRadians(angle));
            //model *= Matrix4x4.CreateTranslation(new Vector3(-2, 0, 0));

            //_shader.SetMatrix4("model", model);
            //_shader.SetVector4("uColor", new Vector4(1f, 1f, 0f, 1f));

            //GL.DrawRangeElements(GLEnum.Triangles, (uint)(12), (uint)(18), (uint)(3), GLEnum.UnsignedInt, (void*)0);

            // see binding/ubinding https://www.songho.ca/opengl/gl_vbo.html

            foreach (var atribute in _VAOBatch.Attributes)
            {
                var vertexLocation = _shader.GetAttribLocation(atribute.Name);
                GL.DisableVertexAttribArray(vertexLocation);
            }

            GL.BindVertexArray(0);
            GL.BindBuffer(GLEnum.ArrayBuffer, 0);
            GL.BindBuffer(GLEnum.ElementArrayBuffer, 0);
        }

        public class VertexAttribute
        {
            public string Name;
            public uint Stride;
            public uint Offset;
            public int Count;
            public GLEnum Type = GLEnum.Float;
        }

        public class VAOBatch
        {
            public uint VAO;
            public uint VBO;
            public uint EBO;

            public List<Batch> Batches = new List<Batch>();

            public List<VertexAttribute> Attributes = new List<VertexAttribute>();
        }

        public class Batch
        {
            public uint Start;
            //public uint End;
            public uint Count;

            public Matrix4x4 Model = Matrix4x4.Identity;

            public List<VertexAttribute> Attributes = new List<VertexAttribute>();

            public Dictionary<string, object> Uniforms = new Dictionary<string, object>();
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

        protected void OnResize(Vector2D<int> size)
        {
            WindowCenter.X = (int)(window.Size.X / 2);
            WindowCenter.Y = (int)(window.Size.Y / 2);

            GL.Viewport(0, 0, (uint)size.X, (uint)size.Y);
        }
    }
}