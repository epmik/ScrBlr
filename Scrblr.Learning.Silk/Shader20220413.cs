using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace Scrblr.Leaning
{
    public class Shader20220413
    {
        private static GL Gl;

        public readonly uint Handle;

        private readonly Dictionary<string, int> _uniformLocations;
        private readonly Dictionary<string, int> _attributeLocations;

        // This is how you create a simple shader.
        // Shaders are written in GLSL, which is a language very similar to C in its semantics.
        // The GLSL source is compiled *at runtime*, so it can optimize itself for the graphics card it's currently being used on.
        // A commented example of GLSL can be found in shader.vert.
        public Shader20220413(GL gl, string vertexShaderPathOrSource, string fragmentShaderPathOrSource)
        {
            Gl = gl;

            // There are several different types of shaders, but the only two you need for basic rendering are the vertex and fragment shaders.
            // The vertex shader is responsible for moving around vertices, and uploading that data to the fragment shader.
            //   The vertex shader won't be too important here, but they'll be more important later.
            // The fragment shader is responsible for then converting the vertices to "fragments", which represent all the data OpenGL needs to draw a pixel.
            //   The fragment shader is what we'll be using the most here.

            var shaderSource = File.Exists(vertexShaderPathOrSource) ? File.ReadAllText(vertexShaderPathOrSource) : vertexShaderPathOrSource;

            // Gl.CreateShader will create an empty shader (obviously). The ShaderType enum denotes which type of shader will be created.
            var vertexShader = Gl.CreateShader(ShaderType.VertexShader);

            // Now, bind the GLSL source code
            Gl.ShaderSource(vertexShader, shaderSource);

            // And then compile
            Compile(vertexShader);

            // We do the same for the fragment shader.
            shaderSource = File.Exists(fragmentShaderPathOrSource) ? File.ReadAllText(fragmentShaderPathOrSource) : fragmentShaderPathOrSource;

            var fragmentShader = Gl.CreateShader(ShaderType.FragmentShader);
            Gl.ShaderSource(fragmentShader, shaderSource);
            Compile(fragmentShader);

            // These two shaders must then be merged into a shader program, which can then be used by OpenGl.
            // To do this, create a program...
            Handle = Gl.CreateProgram();

            // Attach both shaders...
            Gl.AttachShader(Handle, vertexShader);
            Gl.AttachShader(Handle, fragmentShader);

            // And then link them together.
            Link(Handle);

            // When the shader program is linked, it no longer needs the individual shaders attached to it; the compiled code is copied into the shader program.
            // Detach them, and then delete them.
            Gl.DetachShader(Handle, vertexShader);
            Gl.DetachShader(Handle, fragmentShader);
            Gl.DeleteShader(fragmentShader);
            Gl.DeleteShader(vertexShader);

            // The shader is now ready to go, but first, we're going to cache all the shader uniform locations.
            // Querying this from the shader is very slow, so we do it once on initialization and reuse those values
            // later.

            // see for a 'newer' way to do this
            // https://stackoverflow.com/questions/440144/in-opengl-is-there-a-way-to-get-a-list-of-all-uniforms-attribs-used-by-a-shade/12611619#12611619

            // First, we have to get the number of active uniforms in the shader.
            Gl.GetProgram(Handle, GLEnum.ActiveUniforms, out var numberOfUniforms);

            // Next, allocate the dictionary to hold the locations.
            _uniformLocations = new Dictionary<string, int>();

            // Loop over all the uniforms,
            for (uint i = 0; i < numberOfUniforms; i++)
            {
                // get the name of this uniform,
                var key = Gl.GetActiveUniform(Handle, i, out _, out _);

                // get the location,
                var location = Gl.GetUniformLocation(Handle, key);

                // and then add it to the dictionary.
                _uniformLocations.Add(key, location);
            }



            Gl.GetProgram(Handle, GLEnum.ActiveAttributes, out var numberOfAttributes);

            // Next, allocate the dictionary to hold the locations.
            _attributeLocations = new Dictionary<string, int>();

            // Loop over all the uniforms,
            for (uint i = 0; i < numberOfAttributes; i++)
            {
                // get the name of this uniform,
                var key = Gl.GetActiveAttrib(Handle, i, out _, out _);

                // get the location,
                var location = Gl.GetAttribLocation(Handle, key);

                // and then add it to the dictionary.
                _attributeLocations.Add(key, location);
            }
        }

        private static void Compile(uint shader)
        {
            // Try to compile the shader
            Gl.CompileShader(shader);

            var infoLog = Gl.GetShaderInfoLog(shader);

            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
            }

            //// Check for compilation errors
            //Gl.GetShader(shader, GLEnum.CompileStatus, out var code);
            //if (code != (int)All.True)
            //{
            //    // We can use `Gl.GetShaderInfoLog(shader)` to get information about the error.
            //    var infoLog = Gl.GetShaderInfoLog(shader);
            //    throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
            //}
        }

        private static void Link(uint program)
        {
            // We link the program
            Gl.LinkProgram(program);

            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);

            if (status == 0)
            {
                throw new Exception($"Error occurred whilst linking Program({program})");
            }

            //// Check for linking errors
            //Gl.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            //if (code != (int)All.True)
            //{
            //    // We can use `Gl.GetProgramInfoLog(program)` to get information about the error.
            //    throw new Exception($"Error occurred whilst linking Program({program})");
            //}
        }

        // A wrapper function that enables the shader program.
        public void Use()
        {
            Gl.UseProgram(Handle);
        }

        // The shader sources provided with this project use hardcoded layout(location)-s. If you want to do it dynamically,
        // you can omit the layout(location=X) lines in the vertex shader, and use this in VertexAttribPointer instead of the hardcoded values.
        public int AttributeLocation(string name)
        {
            return _attributeLocations[name];

            // return Gl.GetAttribLocation(Handle, name);
        }

        // Uniform setters
        // Uniforms are variables that can be set by user code, instead of reading them from the VBO.
        // You use VBOs for vertex-related data, and uniforms for almost everything else.

        // Setting a uniform is almost always the exact same, so I'll explain it here once, instead of in every method:
        //     1. Bind the program you want to set the uniform on
        //     2. Get a handle to the location of the uniform with Gl.GetUniformLocation.
        //     3. Use the appropriate Gl.Uniform* function to set the uniform.

        /// <summary>
        /// Set a uniform int on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void Uniform(string name, int data)
        {
            //Gl.UseProgram(Handle);
            Gl.Uniform1(_uniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform float on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void Uniform(string name, float data)
        {
            //Gl.UseProgram(Handle);
            Gl.Uniform1(_uniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform Matrix4 on this shader
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        /// <remarks>
        ///   <para>
        ///   The matrix is transposed before being sent to the shader.
        ///   </para>
        /// </remarks>
        public unsafe void Uniform(string name, Matrix4x4 data)
        {
            //Gl.UseProgram(Handle);
            Gl.UniformMatrix4(_uniformLocations[name], 1, true, (float*)&data);
        }

        /// <summary>
        /// Set a uniform Vector3 on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void Uniform(string name, Vector3 data)
        {
            //Gl.UseProgram(Handle);
            Gl.Uniform3(_uniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform Vector3 on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void Uniform(string name, float x, float y, float z)
        {
            //Gl.UseProgram(Handle);
            Gl.Uniform3(_uniformLocations[name], x, y, z);
        }

        /// <summary>
        /// Set a uniform Vector3 on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void Uniform(string name, Vector4 data)
        {
            //Gl.UseProgram(Handle);
            Gl.Uniform4(_uniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform Vector3 on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void Uniform(string name, float x, float y, float z, float w)
        {
            //Gl.UseProgram(Handle);
            Gl.Uniform4(_uniformLocations[name], x, y, z, w);
        }
    }
}