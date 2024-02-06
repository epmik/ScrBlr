using System;
using System.IO;
using System.Numerics;
using Silk.NET.OpenGL;

namespace Scrblr.Core.Silk.Font
{
    public class FontShader : IDisposable
    {
        private uint _handle;

        public FontShader(string vertexPath, string fragmentPath)
        {
            uint vertex = LoadShader(ShaderType.VertexShader, vertexPath);
            uint fragment = LoadShader(ShaderType.FragmentShader, fragmentPath);
            _handle = Context.GL.CreateProgram();
            GLUtility.CheckError();

            Context.GL.AttachShader(_handle, vertex);
            GLUtility.CheckError();

            Context.GL.AttachShader(_handle, fragment);
            GLUtility.CheckError();

            Context.GL.LinkProgram(_handle);
            Context.GL.GetProgram(_handle, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                throw new Exception($"Program failed to link with error: {Context.GL.GetProgramInfoLog(_handle)}");
            }

            Context.GL.DetachShader(_handle, vertex);
            Context.GL.DetachShader(_handle, fragment);

            Context.GL.DeleteShader(vertex);
            Context.GL.DeleteShader(fragment);
        }

        public void Use()
        {
            Context.GL.UseProgram(_handle);
            GLUtility.CheckError();
        }

        public void SetUniform(string name, int value)
        {
            int location = Context.GL.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                throw new Exception($"{name} uniform not found on shader.");
            }
            Context.GL.Uniform1(location, value);
            GLUtility.CheckError();
        }

        public void SetUniform(string name, float value)
        {
            int location = Context.GL.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                throw new Exception($"{name} uniform not found on shader.");
            }
            Context.GL.Uniform1(location, value);
            GLUtility.CheckError();
        }

        public unsafe void SetUniform(string name, Matrix4x4 value)
        {
            int location = Context.GL.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                throw new Exception($"{name} uniform not found on shader.");
            }

            Context.GL.UniformMatrix4(location, 1, false, (float*)&value);
            GLUtility.CheckError();
        }

        public void Dispose()
        {
            Context.GL.DeleteProgram(_handle);
        }

        private uint LoadShader(ShaderType type, string path)
        {
            string src = File.ReadAllText(path);
            uint handle = Context.GL.CreateShader(type);
            GLUtility.CheckError();

            Context.GL.ShaderSource(handle, src);
            GLUtility.CheckError();

            Context.GL.CompileShader(handle);
            string infoLog = Context.GL.GetShaderInfoLog(handle);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");
            }

            return handle;
        }

        public int GetAttribLocation(string attribName)
        {
            var result = Context.GL.GetAttribLocation(_handle, attribName);
            GLUtility.CheckError();
            return result;
        }
    }
}
