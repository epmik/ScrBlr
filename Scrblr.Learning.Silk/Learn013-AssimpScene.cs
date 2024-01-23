using Silk.NET.OpenGL;
using Silk.NET.Input;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using System.Numerics;
using System.Xml.Linq;
using System.Xml.Serialization;
using System;
using System.Drawing;
using System.Reflection;

namespace Scrblr.Learning
{
    // Be warned, there is a LOT of stuff here. It might seem complicated, but just take it slow and you'll be fine.
    // OpenGL's initial hurdle is quite large, but once you get past that, things will start making more sense.
    public class Learn013AssimpScene : SilkSketch
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

        // We need an instance of the new camera class so it can manage the view and projection matrix code.
        // We also need a boolean set to true to detect whether or not the mouse has been moved for the first time.
        // Finally, we add the last position of the mouse so we can calculate the mouse offset easily.
        private Camera _camera;

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

        float[] _vertices;
        uint _verticesIndex;
        uint[] _indices;
        uint _indicesIndex;

        public unsafe Learn013AssimpScene()
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

            //var sceneGLTF = new Assimp.AssimpContext().ImportFile($".resources/plane-001.gltf");
            //var sceneGLTF = new Assimp.AssimpContext().ImportFile($".resources/cube-001.gltf");
            var sceneGLTF = new Assimp.AssimpContext().ImportFile($".resources/scene-005.gltf");

            var vertexCount = sceneGLTF.TotalMeshVertexCount();
            var indiceCount = sceneGLTF.TotalMeshIndexCount();

            _vertices = new float[vertexCount * 3];
            _indices = new uint[indiceCount];

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();

            GL.BindBuffer(GLEnum.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertexCount * 3 * sizeof(float)), IntPtr.Zero, BufferUsageARB.StaticDraw); //Setting buffer data.

            _elementBufferObject = GL.GenBuffer();

            GL.BindBuffer(GLEnum.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indiceCount * sizeof(uint)), IntPtr.Zero, BufferUsageARB.StaticDraw); //Setting buffer data.




            _shader = new Shader(GL, vertexShaderSource, fragmentShaderSource);
            _shader.Use();

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

            uint arrayBufferBytesWritten = 0;
            uint elementArrayBufferBytesWritten = 0;
            uint index = 0;

            //WriteNodeToBuffersRecursive(
            //    sceneGLTF,
            //    _vertexArrayObject,
            //    _vertexBufferObject,
            //    _elementBufferObject,
            //    index,
            //    ref arrayBufferBytesWritten,
            //    ref elementArrayBufferBytesWritten);

            //foreach(var mesh in sceneGLTF.Meshes)
            //{
            //    WriteMeshToBuffers(
            //        mesh,
            //        _vertexArrayObject,
            //        _vertexBufferObject,
            //        _elementBufferObject,
            //        arrayBufferBytesWritten,
            //        elementArrayBufferBytesWritten,
            //        out arrayBufferBytesWritten,
            //        out elementArrayBufferBytesWritten);
            //}


            fixed (void* v = &_vertices[0])
            {
                var size = _vertices.Length * sizeof(float);

                GL.BufferSubData(GLEnum.ArrayBuffer, (nint)0, (nuint)size, v);
            }

            fixed (void* i = &_indices[0])
            {
                var size = _indices.Length * sizeof(uint);

                GL.BufferSubData(GLEnum.ElementArrayBuffer, (nint)0, (nuint)size, i);
            }


            var vertexLocation = _shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, GLEnum.Float, false, 3 * sizeof(float), (void*)0);

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
            _camera = new Camera(Vector3.UnitZ * 9, window.Size.X / (float)window.Size.Y);

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

            foreach(var b in _VAOBatch.Batches)
            {
                b.Model = Matrix4x4.CreateRotationX((float)Utility.DegreesToRadians(angle)) * b.Origin;
            }

            //_VAOBatch.Batches[0].Model = Matrix4x4.CreateRotationX((float)Utility.DegreesToRadians(angle));
            //_VAOBatch.Batches[1].Model = Matrix4x4.CreateRotationY((float)Utility.DegreesToRadians(angle)) * Matrix4x4.CreateTranslation(new Vector3(2, 0, 0));
            //_VAOBatch.Batches[2].Model = Matrix4x4.CreateRotationZ((float)Utility.DegreesToRadians(angle)) * Matrix4x4.CreateTranslation(new Vector3(-2, 0, 0));
        }

        protected unsafe void OnRenderFrame(double elapsedTime)
        {
            GL.Enable(GLEnum.DepthTest);
            GL.Enable(GLEnum.CullFace);
            GL.CullFace(GLEnum.Back);
            GL.FrontFace(FrontFaceDirection.Ccw);   // FrontFaceDirection.Ccw == default

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // see binding/ubinding https://www.songho.ca/opengl/gl_vbo.html
            GL.BindVertexArray(_VAOBatch.VAO);
            GL.BindBuffer(GLEnum.ArrayBuffer, _VAOBatch.VBO);
            GL.BindBuffer(GLEnum.ElementArrayBuffer, _VAOBatch.EBO);

            foreach (var atribute in _VAOBatch.Attributes)
            {
                var vertexLocation = _shader.GetAttribLocation(atribute.Name);
                GL.EnableVertexAttribArray(vertexLocation);
                GL.VertexAttribPointer(vertexLocation, atribute.Count, atribute.Type, false, atribute.Stride, (void*)atribute.Offset);
            }

            _view = _camera.GetViewMatrix();

            //_texture.Use(TextureUnit.Texture0);
            //_texture2.Use(TextureUnit.Texture1);

            _shader.Use();

            var i = Matrix4x4.Identity;

            _shader.SetMatrix4("view", _view);
            _shader.SetMatrix4("projection", _projection);
            _shader.SetMatrix4("model", i);
            _shader.SetVector4("uColor", new Vector4(1f, 0f, 0f, 1f));

            GL.DrawElements(GLEnum.Triangles, (uint)_indices.Length, GLEnum.UnsignedInt, (void*)0);

            //foreach (var batch in _VAOBatch.Batches)
            //{
            //    _shader.Uniform("model", batch.Model);

            //    foreach (var uniform in batch.Uniforms)
            //    {
                    //_shader.Uniform(uniform.Key, uniform.Value);
            //    }

            //    GL.DrawRangeElements(GLEnum.Triangles, (uint)batch.Start, (uint)(batch.Start + batch.Count), (uint)batch.Count, GLEnum.UnsignedInt, (void*)0);
            //}

            //// see binding/ubinding https://www.songho.ca/opengl/gl_vbo.html

            //foreach (var atribute in _VAOBatch.Attributes)
            //{
            //    var vertexLocation = _shader.GetAttribLocation(atribute.Name);
            //    GL.DisableVertexAttribArray(vertexLocation);
            //}

            GL.BindVertexArray(0);
            GL.BindBuffer(GLEnum.ArrayBuffer, 0);
            GL.BindBuffer(GLEnum.ElementArrayBuffer, 0);
        }

        private Matrix4x4 ToMatrix4x4(Assimp.Matrix4x4 m)
        {
            //return new Matrix4x4(
            //    m.A1, m.A2, m.A3, m.A4,
            //    m.B1, m.B2, m.B3, m.B4,
            //    m.C1, m.C2, m.C3, m.C4,
            //    m.D1, m.D2, m.D3, m.D4);

            return new Matrix4x4(
                m.A1, m.B1, m.C1, m.D1,
                m.A2, m.B2, m.C2, m.D2,
                m.A3, m.B3, m.C3, m.D3,
                m.A4, m.B4, m.C4, m.D4);
        }

        private uint WriteNodeToBuffersRecursive(Assimp.Scene scene,
            uint vao,
            uint vbo,
            uint ebo,
            uint index,
            ref uint arrayBufferOffset,
            ref uint elementArrayBufferOffset)
        {
            index = WriteNodeToBuffersRecursive(
                    scene.RootNode,
                    scene,
                    vao,
                    vbo,
                    ebo,
                    index,
                    ref arrayBufferOffset,
                    ref elementArrayBufferOffset);

            return index;
        }

        private uint WriteNodeToBuffersRecursive(
            Assimp.Node node,
            Assimp.Scene scene,
            uint vao,
            uint vbo,
            uint ebo,
            uint index,
            ref uint arrayBufferOffset,
            ref uint elementArrayBufferOffset)
        {
            if (node == null)
            {
                return index;
            }

            if(node.HasMeshes)
            {
                index = WriteNodeToBuffers(
                    node, 
                    scene, 
                    vao, 
                    vbo, 
                    ebo,
                    index,
                    ref arrayBufferOffset,
                    ref elementArrayBufferOffset);
            }

            if(node.HasChildren)
            {
                foreach(var child in node.Children)
                {
                    index = WriteNodeToBuffersRecursive(
                        child,
                        scene,
                        vao,
                        vbo,
                        ebo,
                        index,
                        ref arrayBufferOffset,
                        ref elementArrayBufferOffset);
                }
            }

            return index;
        }

        private uint WriteNodeToBuffers(
            Assimp.Node node, 
            Assimp.Scene scene,
            uint vao,
            uint vbo,
            uint ebo,
            uint indiceIndex,
            ref uint arrayBufferOffset,
            ref uint elementArrayBufferOffset)
        {
            if (!node.HasMeshes)
            {
                return indiceIndex;
            }

            foreach (var i in node.MeshIndices)
            {
                var m = scene.Meshes[i];

                WriteMeshToBuffers(
                    m,
                    vao,
                    vbo,
                    ebo,
                    indiceIndex,
                    ref arrayBufferOffset,
                    ref elementArrayBufferOffset);

                var b = new Batch
                {
                    Start = indiceIndex,
                    Count = m.TotalMeshIndexCount(),
                };

                _VAOBatch.Batches.Add(b);

                b.Origin = ToMatrix4x4(node.Transform);

                var random = new Random();

                b.Uniforms.Add("uColor", new Vector4(
                    (float)random.NextDouble(),
                    (float)random.NextDouble(),
                    (float)random.NextDouble(),
                    1f));

                indiceIndex += b.Count;
            }

            return indiceIndex;
        }

        private unsafe void WriteMeshToBuffers(
            Assimp.Mesh mesh,
            uint vao,
            uint vbo,
            uint ebo,
            uint indiceIndex,
            ref uint arrayBufferOffset,
            ref uint elementArrayBufferOffset)
        {
            GL.BindVertexArray(vao);

            GL.BindBuffer(GLEnum.ArrayBuffer, vbo);
            GL.BindBuffer(GLEnum.ElementArrayBuffer, ebo);

            var floatSize = (uint)sizeof(float);

            // README Instancing
            // https://learnopengl.com/Advanced-OpenGL/Instancing


            // README GL.MapBuffer example
            // https://github.com/Airstriker/VBO-OpenTK-Example/blob/master/VBO/T08_VBO.cs

            // README OpenTK read depth buffer to array
            // https://stackoverflow.com/questions/69883630/opentk-read-depth-buffer-to-array


            // README efficiently writing/streaming data to the GPU
            // see https://gamedev.stackexchange.com/a/87078
            // or
            // https://www.khronos.org/opengl/wiki/Buffer_Object_Streaming

            for (var i = 0; i < mesh.Vertices.Count; i++)
            {
                GL.BufferSubData(GLEnum.ArrayBuffer, (nint)arrayBufferOffset, (nuint)floatSize, mesh.Vertices[i].X);
                arrayBufferOffset += floatSize;
                GL.BufferSubData(GLEnum.ArrayBuffer, (nint)arrayBufferOffset, (nuint)floatSize, mesh.Vertices[i].X);
                _vertices[_verticesIndex++] = mesh.Vertices[i].X;

                GL.BufferSubData(GLEnum.ArrayBuffer, (nint)arrayBufferOffset, (nuint)floatSize, mesh.Vertices[i].Y);
                arrayBufferOffset += floatSize;
                _vertices[_verticesIndex++] = mesh.Vertices[i].Y;

                GL.BufferSubData(GLEnum.ArrayBuffer, (nint)arrayBufferOffset, (nuint)floatSize, mesh.Vertices[i].Z);
                arrayBufferOffset += floatSize;
                _vertices[_verticesIndex++] = mesh.Vertices[i].Z;
            }

            var uintSize = (uint)sizeof(uint);

            for (var f = 0; f < mesh.Faces.Count; f++)
            {
                for (var i = 0; i < mesh.Faces[f].Indices.Count; i++)
                {
                    GL.BufferSubData(GLEnum.ElementArrayBuffer, (nint)elementArrayBufferOffset, (nuint)uintSize, indiceIndex + mesh.Faces[f].Indices[i]);

                    elementArrayBufferOffset += uintSize;

                    _indices[_indicesIndex++] = (uint)(indiceIndex + mesh.Faces[f].Indices[i]);
                }
            }
        }

        private void MeasureExecutionTimeProperties(
            out bool isHighResolution,
            out long frequency,
            out long nanosecPerTick)
        {
            isHighResolution = System.Diagnostics.Stopwatch.IsHighResolution;
            frequency = System.Diagnostics.Stopwatch.Frequency;
            nanosecPerTick = (1000L * 1000L * 1000L) / frequency;
        }

        private void MeasureExecutionTimeStart()
        {
            System.Diagnostics.Stopwatch.StartNew();
        }

        private void MeasureExecutionTimeStop()
        {

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

            public Matrix4x4 Origin = Matrix4x4.Identity;

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