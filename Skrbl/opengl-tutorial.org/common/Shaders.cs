using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace OpenGlTutorialOrg
{
    internal static class Shaders
    {
        public static int Load(string vertex_file_path, string fragment_file_path)
        {
            // Create the shaders
            var VertexShaderID = GL.CreateShader(ShaderType.VertexShader);
            var FragmentShaderID = GL.CreateShader(ShaderType.FragmentShader);

            // Read the Vertex Shader code from the file
            var VertexShaderCode = File.ReadAllText(vertex_file_path);

            // Read the Fragment Shader code from the file
            var FragmentShaderCode = File.ReadAllText(fragment_file_path);

            int Result;
            int InfoLogLength;

            // Compile Vertex Shader
            Console.WriteLine($"Compiling shader : {vertex_file_path}");
            GL.ShaderSource(VertexShaderID, VertexShaderCode);
            GL.CompileShader(VertexShaderID);

            // Check Vertex Shader
            GL.GetShader(VertexShaderID, ShaderParameter.CompileStatus, out Result);
            GL.GetShader(VertexShaderID, ShaderParameter.InfoLogLength, out InfoLogLength);
            if (InfoLogLength > 0)
            {
                GL.GetShaderInfoLog(VertexShaderID, InfoLogLength, out _, out string VertexShaderErrorMessage);
                Console.WriteLine($"{VertexShaderErrorMessage}");
                Debug.WriteLine($"{VertexShaderErrorMessage}");
            }

            // Compile Fragment Shader
            Console.WriteLine($"Compiling shader : {fragment_file_path}");
            GL.ShaderSource(FragmentShaderID, FragmentShaderCode);
            GL.CompileShader(FragmentShaderID);

            // Check Fragment Shader
            GL.GetShader(FragmentShaderID, ShaderParameter.CompileStatus, out Result);
            GL.GetShader(FragmentShaderID, ShaderParameter.InfoLogLength, out InfoLogLength);
            if (InfoLogLength > 0)
            {
                GL.GetShaderInfoLog(FragmentShaderID, InfoLogLength, out _, out string FragmentShaderErrorMessage);
                Console.WriteLine($"{FragmentShaderErrorMessage}");
                Debug.WriteLine($"{FragmentShaderErrorMessage}");
            }

            // Link the program
            Console.WriteLine("Linking program");
            var ProgramID = GL.CreateProgram();
            GL.AttachShader(ProgramID, VertexShaderID);
            GL.AttachShader(ProgramID, FragmentShaderID);
            GL.LinkProgram(ProgramID);

            // Check the program
            GL.GetProgram(ProgramID, GetProgramParameterName.LinkStatus, out Result);
            GL.GetProgram(ProgramID, GetProgramParameterName.InfoLogLength, out InfoLogLength);
            if (InfoLogLength > 0)
            {
                GL.GetProgramInfoLog(ProgramID, InfoLogLength, out _, out string ProgramErrorMessage);
                Console.WriteLine($"{ProgramErrorMessage}");
                Debug.WriteLine($"{ProgramErrorMessage}");
            }


            GL.DetachShader(ProgramID, VertexShaderID);
            GL.DetachShader(ProgramID, FragmentShaderID);

            GL.DeleteShader(VertexShaderID);
            GL.DeleteShader(FragmentShaderID);

            return ProgramID;
        }
    }
}
