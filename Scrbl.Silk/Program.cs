using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;
using System.Linq;
using System.Numerics;
using Silk.NET.Maths;

namespace Scrbl
{
    class Program
    {
        private static void Main(string[] args)
        {
            //new Scrbl.Tutorials._001_Hello_Window().Run(args);
            //new Scrbl.Tutorials._002_Hello_Quad().Run(args);
            //new Scrbl.Tutorials._003_Hello_Colored_Quad().Run(args);
            new Scrbl.Tutorials._004_Hello_Colored_Quad_Non_Interleaved().Run(args);
        }
    }

    //class Program
    //{
    //    private static IWindow window;
    //    private static GL Gl;
    //    private static IKeyboard primaryKeyboard;

    //    private static BufferObject<float> Vbo;
    //    private static BufferObject<uint> Ebo;
    //    private static VertexArrayObject<float, uint> VaoCube;
    //    private static Shader LightingShader;
    //    private static Shader LampShader;
    //    private static Vector3 LampPosition = new Vector3(1.2f, 1.0f, 2.0f);

    //    private static Camera Camera;

    //    //Used to track change in mouse movement to allow for moving of the Camera
    //    private static Vector2 LastMousePosition;

    //    //Track when the window started so we can use the time elapsed to rotate the cube
    //    private static DateTime StartTime;

    //    private static readonly float[] Vertices =
    //    {
    //        //X    Y      Z       Normals
    //        -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
    //         0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
    //         0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
    //         0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
    //        -0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
    //        -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,

    //        -0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
    //         0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
    //         0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
    //         0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
    //        -0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
    //        -0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,

    //        -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,
    //        -0.5f,  0.5f, -0.5f, -1.0f,  0.0f,  0.0f,
    //        -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,
    //        -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,
    //        -0.5f, -0.5f,  0.5f, -1.0f,  0.0f,  0.0f,
    //        -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,

    //         0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,
    //         0.5f,  0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
    //         0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
    //         0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
    //         0.5f, -0.5f,  0.5f,  1.0f,  0.0f,  0.0f,
    //         0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,

    //        -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,
    //         0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,
    //         0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,
    //         0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,
    //        -0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,
    //        -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,

    //        -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,
    //         0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,
    //         0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,
    //         0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,
    //        -0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,
    //        -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f
    //    };

    //    private static readonly uint[] Indices =
    //    {
    //        0, 1, 3,
    //        1, 2, 3
    //    };

    //    private static void Main(string[] args)
    //    {
    //        var options = WindowOptions.Default;
    //        options.Size = new Vector2D<int>(800, 600);
    //        options.Title = "LearnOpenGL with Silk.NET";
    //        window = Window.Create(options);

    //        window.Load += OnLoad;
    //        window.Update += OnUpdate;
    //        window.Render += OnRender;
    //        window.FramebufferResize += OnFramebufferResize;
    //        window.Closing += OnClose;

    //        window.Run();

    //        window.Dispose();
    //    }

    //    private static void OnLoad()
    //    {
    //        StartTime = DateTime.UtcNow;
    //        IInputContext input = window.CreateInput();
    //        primaryKeyboard = input.Keyboards.FirstOrDefault();
    //        if (primaryKeyboard != null)
    //        {
    //            primaryKeyboard.KeyDown += KeyDown;
    //        }
    //        for (int i = 0; i < input.Mice.Count; i++)
    //        {
    //            input.Mice[i].Cursor.CursorMode = CursorMode.Raw;
    //            input.Mice[i].MouseMove += OnMouseMove;
    //            input.Mice[i].Scroll += OnMouseWheel;
    //        }

    //        Gl = GL.GetApi(window);

    //        Ebo = new BufferObject<uint>(Gl, Indices, BufferTargetARB.ElementArrayBuffer);
    //        Vbo = new BufferObject<float>(Gl, Vertices, BufferTargetARB.ArrayBuffer);
    //        VaoCube = new VertexArrayObject<float, uint>(Gl, Vbo, Ebo);

    //        VaoCube.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 6, 0);
    //        VaoCube.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, 6, 3);

    //        //The lighting shader will give our main cube it's colour multiplied by the light's intensity
    //        LightingShader = new Shader(Gl, "shader.vert", "lighting.frag");
    //        //The Lamp shader uses a fragment shader that just colours it solid white so that we know it is the light source
    //        LampShader = new Shader(Gl, "shader.vert", "shader.frag");

    //        //Start a camera at position 3 on the Z axis, looking at position -1 on the Z axis
    //        var size = window.FramebufferSize;
    //        Camera = new Camera(Vector3.UnitZ * 6, Vector3.UnitZ * -1, Vector3.UnitY, (float)size.X / size.Y);
    //    }

    //    private static unsafe void OnUpdate(double deltaTime)
    //    {
    //        var moveSpeed = 2.5f * (float)deltaTime;

    //        if (primaryKeyboard.IsKeyPressed(Key.W))
    //        {
    //            //Move forwards
    //            Camera.Position += moveSpeed * Camera.Front;
    //        }
    //        if (primaryKeyboard.IsKeyPressed(Key.S))
    //        {
    //            //Move backwards
    //            Camera.Position -= moveSpeed * Camera.Front;
    //        }
    //        if (primaryKeyboard.IsKeyPressed(Key.A))
    //        {
    //            //Move left
    //            Camera.Position -= Vector3.Normalize(Vector3.Cross(Camera.Front, Camera.Up)) * moveSpeed;
    //        }
    //        if (primaryKeyboard.IsKeyPressed(Key.D))
    //        {
    //            //Move right
    //            Camera.Position += Vector3.Normalize(Vector3.Cross(Camera.Front, Camera.Up)) * moveSpeed;
    //        }
    //    }

    //    private static unsafe void OnRender(double deltaTime)
    //    {
    //        Gl.Enable(EnableCap.DepthTest);
    //        Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

    //        VaoCube.Bind();
    //        LightingShader.Use();

    //        //Slightly rotate the cube to give it an angled face to look at
    //        LightingShader.SetUniform("uModel", Matrix4x4.CreateRotationY(MathHelper.DegreesToRadians(25f)));
    //        LightingShader.SetUniform("uView", Camera.GetViewMatrix());
    //        LightingShader.SetUniform("uProjection", Camera.GetProjectionMatrix());
    //        LightingShader.SetUniform("viewPos", Camera.Position);
    //        LightingShader.SetUniform("material.ambient", new Vector3(1.0f, 0.5f, 0.31f));
    //        LightingShader.SetUniform("material.diffuse", new Vector3(1.0f, 0.5f, 0.31f));
    //        LightingShader.SetUniform("material.specular", new Vector3(0.5f, 0.5f, 0.5f));
    //        LightingShader.SetUniform("material.shininess", 32.0f);

    //        //Track the difference in time so we can manipulate variables as time changes
    //        var difference = (float)(DateTime.UtcNow - StartTime).TotalSeconds;
    //        var lightColor = Vector3.Zero;
    //        lightColor.X = MathF.Sin(difference * 2.0f);
    //        lightColor.Y = MathF.Sin(difference * 0.7f);
    //        lightColor.Z = MathF.Sin(difference * 1.3f);

    //        var diffuseColor = lightColor * new Vector3(0.5f);
    //        var ambientColor = diffuseColor * new Vector3(0.2f);

    //        LightingShader.SetUniform("light.ambient", ambientColor);
    //        LightingShader.SetUniform("light.diffuse", diffuseColor); // darkened
    //        LightingShader.SetUniform("light.specular", new Vector3(1.0f, 1.0f, 1.0f));
    //        LightingShader.SetUniform("light.position", LampPosition);

    //        //We're drawing with just vertices and no indicies, and it takes 36 verticies to have a six-sided textured cube
    //        Gl.DrawArrays(PrimitiveType.Triangles, 0, 36);

    //        LampShader.Use();

    //        //The Lamp cube is going to be a scaled down version of the normal cubes verticies moved to a different screen location
    //        var lampMatrix = Matrix4x4.Identity;
    //        lampMatrix *= Matrix4x4.CreateScale(0.2f);
    //        lampMatrix *= Matrix4x4.CreateTranslation(LampPosition);

    //        LampShader.SetUniform("uModel", lampMatrix);
    //        LampShader.SetUniform("uView", Camera.GetViewMatrix());
    //        LampShader.SetUniform("uProjection", Camera.GetProjectionMatrix());

    //        Gl.DrawArrays(PrimitiveType.Triangles, 0, 36);
    //    }

    //    private static void OnFramebufferResize(Vector2D<int> newSize)
    //    {
    //        Gl.Viewport(newSize);
    //        Camera.AspectRatio = (float)newSize.X / newSize.Y;
    //    }

    //    private static unsafe void OnMouseMove(IMouse mouse, Vector2 position)
    //    {
    //        var lookSensitivity = 0.1f;
    //        if (LastMousePosition == default) { LastMousePosition = position; }
    //        else
    //        {
    //            var xOffset = (position.X - LastMousePosition.X) * lookSensitivity;
    //            var yOffset = (position.Y - LastMousePosition.Y) * lookSensitivity;
    //            LastMousePosition = position;

    //            Camera.ModifyDirection(xOffset, yOffset);
    //        }
    //    }

    //    private static unsafe void OnMouseWheel(IMouse mouse, ScrollWheel scrollWheel)
    //    {
    //        Camera.ModifyZoom(scrollWheel.Y);
    //    }

    //    private static void OnClose()
    //    {
    //        Vbo.Dispose();
    //        Ebo.Dispose();
    //        VaoCube.Dispose();
    //        LightingShader.Dispose();
    //    }

    //    private static void KeyDown(IKeyboard keyboard, Key key, int arg3)
    //    {
    //        if (key == Key.Escape)
    //        {
    //            window.Close();
    //        }
    //    }
    //}

    //class Program
    //{
    //    private static IWindow window;
    //    private static GL Gl;
    //    private static IKeyboard primaryKeyboard;

    //    private static Texture Texture;
    //    private static Shader Shader;
    //    private static Model Model;

    //    //Setup the camera's location, directions, and movement speed
    //    private static Vector3 CameraPosition = new Vector3(0.0f, 0.0f, 3.0f);
    //    private static Vector3 CameraFront = new Vector3(0.0f, 0.0f, -1.0f);
    //    private static Vector3 CameraUp = Vector3.UnitY;
    //    private static Vector3 CameraDirection = Vector3.Zero;
    //    private static float CameraYaw = -90f;
    //    private static float CameraPitch = 0f;
    //    private static float CameraZoom = 45f;

    //    //Used to track change in mouse movement to allow for moving of the Camera
    //    private static Vector2 LastMousePosition;

    //    private static void Main(string[] args)
    //    {
    //        var options = WindowOptions.Default;
    //        options.Size = new Vector2D<int>(800, 600);
    //        options.Title = "LearnOpenGL with Silk.NET";
    //        window = Window.Create(options);

    //        window.Load += OnLoad;
    //        window.Update += OnUpdate;
    //        window.Render += OnRender;
    //        window.FramebufferResize += OnFramebufferResize;
    //        window.Closing += OnClose;

    //        window.Run();

    //        window.Dispose();
    //    }

    //    private static void OnLoad()
    //    {
    //        IInputContext input = window.CreateInput();
    //        primaryKeyboard = input.Keyboards.FirstOrDefault();
    //        if (primaryKeyboard != null)
    //        {
    //            primaryKeyboard.KeyDown += KeyDown;
    //        }
    //        for (int i = 0; i < input.Mice.Count; i++)
    //        {
    //            input.Mice[i].Cursor.CursorMode = CursorMode.Raw;
    //            input.Mice[i].MouseMove += OnMouseMove;
    //            input.Mice[i].Scroll += OnMouseWheel;
    //        }

    //        Gl = GL.GetApi(window);

    //        Shader = new Shader(Gl, "shader.vert", "shader.frag");
    //        Texture = new Texture(Gl, "silk.png");
    //        Model = new Model(Gl, "cube.model");
    //    }

    //    private static unsafe void OnUpdate(double deltaTime)
    //    {
    //        var moveSpeed = 2.5f * (float) deltaTime;

    //        if (primaryKeyboard.IsKeyPressed(Key.W))
    //        {
    //            //Move forwards
    //            CameraPosition += moveSpeed * CameraFront;
    //        }
    //        if (primaryKeyboard.IsKeyPressed(Key.S))
    //        {
    //            //Move backwards
    //            CameraPosition -= moveSpeed * CameraFront;
    //        }
    //        if (primaryKeyboard.IsKeyPressed(Key.A))
    //        {
    //            //Move left
    //            CameraPosition -= Vector3.Normalize(Vector3.Cross(CameraFront, CameraUp)) * moveSpeed;
    //        }
    //        if (primaryKeyboard.IsKeyPressed(Key.D))
    //        {
    //            //Move right
    //            CameraPosition += Vector3.Normalize(Vector3.Cross(CameraFront, CameraUp)) * moveSpeed;
    //        }
    //    }

    //    private static unsafe void OnRender(double deltaTime)
    //    {
    //        Gl.Enable(EnableCap.DepthTest);
    //        Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

    //        Texture.Bind();
    //        Shader.Use();
    //        Shader.SetUniform("uTexture0", 0);

    //        //Use elapsed time to convert to radians to allow our cube to rotate over time
    //        var difference = (float) (window.Time * 100);

    //        var size = window.FramebufferSize;

    //        var model = Matrix4x4.CreateRotationY(MathHelper.DegreesToRadians(difference)) * Matrix4x4.CreateRotationX(MathHelper.DegreesToRadians(difference));
    //        var view = Matrix4x4.CreateLookAt(CameraPosition, CameraPosition + CameraFront, CameraUp);
    //        //Note that the apsect ratio calculation must be performed as a float, otherwise integer division will be performed (truncating the result).
    //        var projection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(CameraZoom), (float)size.X / size.Y, 0.1f, 100.0f);

    //        foreach (var mesh in Model.Meshes)
    //        {
    //            mesh.Bind();
    //            Shader.Use();
    //            Texture.Bind();
    //            Shader.SetUniform("uTexture0", 0);
    //            Shader.SetUniform("uModel", model);
    //            Shader.SetUniform("uView", view);
    //            Shader.SetUniform("uProjection", projection);

    //            Gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)mesh.Vertices.Length);
    //        }
    //    }

    //    private static void OnFramebufferResize(Vector2D<int> newSize)
    //    {
    //        Gl.Viewport(newSize);
    //    }

    //    private static unsafe void OnMouseMove(IMouse mouse, Vector2 position)
    //    {
    //        var lookSensitivity = 0.1f;
    //        if (LastMousePosition == default)
    //        {
    //            LastMousePosition = position;
    //        }
    //        else
    //        {
    //            var xOffset = (position.X - LastMousePosition.X) * lookSensitivity;
    //            var yOffset = (position.Y - LastMousePosition.Y) * lookSensitivity;
    //            LastMousePosition = position;

    //            CameraYaw += xOffset;
    //            CameraPitch -= yOffset;

    //            //We don't want to be able to look behind us by going over our head or under our feet so make sure it stays within these bounds
    //            CameraPitch = Math.Clamp(CameraPitch, -89.0f, 89.0f);

    //            CameraDirection.X = MathF.Cos(MathHelper.DegreesToRadians(CameraYaw)) * MathF.Cos(MathHelper.DegreesToRadians(CameraPitch));
    //            CameraDirection.Y = MathF.Sin(MathHelper.DegreesToRadians(CameraPitch));
    //            CameraDirection.Z = MathF.Sin(MathHelper.DegreesToRadians(CameraYaw)) * MathF.Cos(MathHelper.DegreesToRadians(CameraPitch));
    //            CameraFront = Vector3.Normalize(CameraDirection);
    //        }
    //    }

    //    private static unsafe void OnMouseWheel(IMouse mouse, ScrollWheel scrollWheel)
    //    {
    //        //We don't want to be able to zoom in too close or too far away so clamp to these values
    //        CameraZoom = Math.Clamp(CameraZoom - scrollWheel.Y, 1.0f, 45f);
    //    }

    //    private static void OnClose()
    //    {
    //        Model.Dispose();
    //        Shader.Dispose();
    //        Texture.Dispose();
    //    }

    //    private static void KeyDown(IKeyboard keyboard, Key key, int arg3)
    //    {
    //        if (key == Key.Escape)
    //        {
    //            window.Close();
    //        }
    //    }
    //}
}
