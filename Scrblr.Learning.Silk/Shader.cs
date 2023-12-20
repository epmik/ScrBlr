using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Silk.NET.OpenGL;
using System.Numerics;

namespace Scrblr.LearnOpenTK
{
    // A simple class meant to help create shaders.
    public class Shader
    {
        public readonly uint Handle;

        private readonly Dictionary<string, int> _uniformLocations;

        public static GL GL;

        // This is how you create a simple shader.
        // Shaders are written in GLSL, which is a language very similar to C in its semantics.
        // The GLSL source is compiled *at runtime*, so it can optimize itself for the graphics card it's currently being used on.
        // A commented example of GLSL can be found in shader.vert.
        public Shader(GL gl, string vertexShaderSource, string fragmentShaderSource)
        {
            GL = gl;

            // There are several different types of shaders, but the only two you need for basic rendering are the vertex and fragment shaders.
            // The vertex shader is responsible for moving around vertices, and uploading that data to the fragment shader.
            //   The vertex shader won't be too important here, but they'll be more important later.
            // The fragment shader is responsible for then converting the vertices to "fragments", which represent all the data OpenGL needs to draw a pixel.
            //   The fragment shader is what we'll be using the most here.

            // Load vertex shader and compile

            // GL.CreateShader will create an empty shader (obviously). The ShaderType enum denotes which type of shader will be created.
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);

            // Now, bind the GLSL source code
            GL.ShaderSource(vertexShader, vertexShaderSource);

            // And then compile
            CompileShader(vertexShader);

            // We do the same for the fragment shader.
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            CompileShader(fragmentShader);

            // These two shaders must then be merged into a shader program, which can then be used by OpenGL.
            // To do this, create a program...
            Handle = GL.CreateProgram();

            // Attach both shaders...
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            // And then link them together.
            LinkProgram(Handle);

            // When the shader program is linked, it no longer needs the individual shaders attached to it; the compiled code is copied into the shader program.
            // Detach them, and then delete them.
            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            // The shader is now ready to go, but first, we're going to cache all the shader uniform locations.
            // Querying this from the shader is very slow, so we do it once on initialization and reuse those values
            // later.

            // First, we have to get the number of active uniforms in the shader.
            GL.GetProgram(Handle, GLEnum.ActiveUniforms, out var numberOfUniforms);

            // Next, allocate the dictionary to hold the locations.
            _uniformLocations = new Dictionary<string, int>();

            // Loop over all the uniforms,
            for (uint i = 0; i < numberOfUniforms; i++)
            {
                // get the name of this uniform,
                var key = GL.GetActiveUniform(Handle, i, out _, out _);

                // get the location,
                var location = GL.GetUniformLocation(Handle, key);

                // and then add it to the dictionary.
                _uniformLocations.Add(key, location);
            }
        }

        private static void CompileShader(uint shader)
        {
            // Try to compile the shader
            GL.CompileShader(shader);

            // Check for compilation errors
            GL.GetShader(shader, GLEnum.CompileStatus, out var code);
            if (code != 1)
            {
                // We can use `GL.GetShaderInfoLog(shader)` to get information about the error.
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
            }
        }

        private static void LinkProgram(uint program)
        {
            // We link the program
            GL.LinkProgram(program);

            // Check for linking errors
            GL.GetProgram(program, GLEnum.LinkStatus, out var code);
            if (code != 1)
            {
                // We can use `GL.GetProgramInfoLog(program)` to get information about the error.
                throw new Exception($"Error occurred whilst linking Program({program})");
            }
        }

        // A wrapper function that enables the shader program.
        public void Use()
        {
            GL.UseProgram(Handle);
        }

        // The shader sources provided with this project use hardcoded layout(location)-s. If you want to do it dynamically,
        // you can omit the layout(location=X) lines in the vertex shader, and use this in VertexAttribPointer instead of the hardcoded values.
        public uint GetAttribLocation(string attribName)
        {
            return (uint)GL.GetAttribLocation(Handle, attribName);
        }

        // Uniform setters
        // Uniforms are variables that can be set by user code, instead of reading them from the VBO.
        // You use VBOs for vertex-related data, and uniforms for almost everything else.

        // Setting a uniform is almost always the exact same, so I'll explain it here once, instead of in every method:
        //     1. Bind the program you want to set the uniform on
        //     2. Get a handle to the location of the uniform with GL.GetUniformLocation.
        //     3. Use the appropriate GL.Uniform* function to set the uniform.

        /// <summary>
        /// Set a uniform int on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetInt(string name, int data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform float on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetFloat(string name, float data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform Matrix4 on this shader
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        /// <remarks>
        ///   <para>
        ///   Positive rotation is counterclockwise about the axis of rotation
        ///   The matrix is transposed before being sent to the shader.
        ///   </para>
        /// </remarks>
        public unsafe void SetMatrix4(string name, Matrix4x4 data)
        {
            GL.UseProgram(Handle);
            GL.UniformMatrix4(_uniformLocations[name], 1, true, (float*)&data);
        }

        /// <summary>
        /// Set a uniform Vector3 on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(Handle);
            GL.Uniform3(_uniformLocations[name], data);
        }
        public void Delete()
        {
            GL.DeleteProgram(Handle);
        }

        private Dictionary<Type, int> _uniformTypeDictionary = new Dictionary<Type, int>
        {
            { typeof(int), 0 },
            { typeof(uint), 1 },
            { typeof(float), 2 },
            { typeof(double), 3 },
            { typeof(short), 4 },
            { typeof(ushort), 5 },
            { typeof(byte), 6 },
            { typeof(Vector3), 7 },
            { typeof(Vector4), 8 },
            { typeof(Matrix4x4), 9 },
        };

        internal unsafe void Uniform(string key, object value)
        {
            GL.UseProgram(Handle);

            switch (_uniformTypeDictionary[value.GetType()])
            {
                case 0:
                    GL.Uniform1(_uniformLocations[key], (int)value);
                    break;
                case 1:
                    GL.Uniform1(_uniformLocations[key], (uint)value);
                    break;
                case 2:
                    GL.Uniform1(_uniformLocations[key], (float)value);
                    break;
                case 3:
                    GL.Uniform1(_uniformLocations[key], (double)value);
                    break;
                case 4:
                    GL.Uniform1(_uniformLocations[key], (short)value);
                    break;
                case 5:
                    GL.Uniform1(_uniformLocations[key], (ushort)value);
                    break;
                case 6:
                    GL.Uniform1(_uniformLocations[key], (byte)value);
                    break;
                case 7:
                    GL.Uniform3(_uniformLocations[key], (Vector3)value);
                    break;
                case 8:
                    GL.Uniform4(_uniformLocations[key], (Vector4)value);
                    break;
                case 9:
                    SetMatrix4(key, (Matrix4x4)value);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}