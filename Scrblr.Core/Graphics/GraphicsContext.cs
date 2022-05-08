using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Scrblr.Core
{
    public class GraphicsContext : GraphicsSettings, IDisposable
    {
        #region Shader Sources




        internal const string Po0Vss = @"
#version 330 core

// location 0 - name iPosition0
// location 1 - name iNormal0
// location 2 - name iNormal1
// location 3 - name iColor0
// location 4 - name iColor1
// location 5 - name iUv0
// location 6 - name iUv1
// location 7 - name iUv2
// location 8 - name iUv3

out vec3 ioPosition0;
out vec4 ioColor0;

layout(location = 0) in vec3 iPosition0;

uniform mat4 uModelMatrix;
uniform mat4 uViewMatrix;
uniform mat4 uProjectionMatrix;
uniform mat4 uModelViewMatrix;
uniform mat4 uModelViewProjectionMatrix;
uniform mat3 uNormalMatrix;

void main(void)
{
    gl_Position = vec4(iPosition0, 1.0) * uModelViewProjectionMatrix;

    // position in view space, used for specular lighting
    ioPosition0 = vec3(uModelMatrix * vec4(iPosition0, 1.0));

	ioColor0 = vec4(0, 0, 0, 1);
}";

        internal const string Po0Fss = @"
#version 330 core

in vec3 ioPosition0;
in vec4 ioColor0;

out vec4 oColor0;

void main()
{
    oColor0 = ioColor0;
}";





        internal const string Po0No0Vss = @"
#version 330 core

// location 0 - name iPosition0
// location 1 - name iNormal0
// location 2 - name iNormal1
// location 3 - name iColor0
// location 4 - name iColor1
// location 5 - name iUv0
// location 6 - name iUv1
// location 7 - name iUv2
// location 8 - name iUv3

layout(location = 0) in vec3 iPosition0;  
layout(location = 1) in vec3 iNormal0;

out vec3 ioPosition0;
out vec3 ioNormal0;
out vec4 ioColor0;

uniform mat4 uModelMatrix;
uniform mat4 uViewMatrix;
uniform mat4 uProjectionMatrix;
uniform mat4 uModelViewMatrix;
uniform mat4 uModelViewProjectionMatrix;
uniform mat3 uNormalMatrix;

void main(void)
{
    gl_Position = vec4(iPosition0, 1.0) * uModelViewProjectionMatrix;

    // position in view space, used for specular lighting
    ioPosition0 = vec3(uModelMatrix * vec4(iPosition0, 1.0));

    ioNormal0 = normalize(iNormal0 * uNormalMatrix);

	ioColor0 = vec4(0, 0, 0, 1);
}";





        internal const string Po0Co0Vss = @"
#version 330 core

// location 0 - name iPosition0
// location 1 - name iNormal0
// location 2 - name iNormal1
// location 3 - name iColor0
// location 4 - name iColor1
// location 5 - name iUv0
// location 6 - name iUv1
// location 7 - name iUv2
// location 8 - name iUv3

layout(location = 0) in vec3 iPosition0;  
layout(location = 3) in vec4 iColor0;

out vec4 ioColor0;

uniform mat4 uModelMatrix;
uniform mat4 uViewMatrix;
uniform mat4 uProjectionMatrix;

void main(void)
{
    gl_Position = vec4(iPosition0, 1.0) * uModelMatrix * uViewMatrix * uProjectionMatrix;

	ioColor0 = iColor0;
}";

        internal const string Po0Co0Fss = @"
#version 330 core

in vec4 ioColor0;
in vec2 ioUv0;

out vec4 oColor0;

void main()
{
    oColor0 = ioColor0;
}";





        internal const string Po0No0Co0Vss = @"
#version 330 core

// location 0 - name iPosition0
// location 1 - name iNormal0
// location 2 - name iNormal1
// location 3 - name iColor0
// location 4 - name iColor1
// location 5 - name iUv0
// location 6 - name iUv1
// location 7 - name iUv2
// location 8 - name iUv3

layout(location = 0) in vec3 iPosition0;  
layout(location = 1) in vec3 iNormal0;  
layout(location = 3) in vec4 iColor0;

out vec3 ioPosition0;
out vec3 ioNormal0;
out vec4 ioColor0;

uniform mat4 uModelMatrix;
uniform mat4 uViewMatrix;
uniform mat4 uProjectionMatrix;
uniform mat4 uModelViewMatrix;
uniform mat4 uModelViewProjectionMatrix;
uniform mat3 uNormalMatrix;

void main(void)
{
    gl_Position = vec4(iPosition0, 1.0) * uModelViewProjectionMatrix;

    // position in view space, used for specular lighting
    ioPosition0 = vec3(uModelMatrix * vec4(iPosition0, 1.0));

    ioNormal0 = normalize(iNormal0 * uNormalMatrix);

	ioColor0 = iColor0;
}";

        internal const string Po0No0Co0Fss = @"
#version 330 core

in vec3 ioPosition0;
in vec3 ioNormal0;
in vec4 ioColor0;

out vec4 oColor0;

uniform int uLightCount;
uniform vec4 uLightPosition;
uniform vec3 uLightDiffuseColor;
uniform float uLightAmbientStrength;
uniform vec3 uLightAmbientColor;

uniform vec3 uViewPosition;

//const float zero_float = 0.0;
//const float one_float = 1.0;
//const vec3 zero_vec3 = vec3(0);

void main()
{
    if(uLightCount > 0)
    {
        float specularStrength = 0.20;
    
        int shininess = 256;

        vec3 ambient = uLightAmbientStrength * uLightAmbientColor;

        bool lightIsDirectional = uLightPosition.w < 1.0;

        // vector from the fragment towards the light
        vec3 lightDirection = normalize(lightIsDirectional ? vec3(-uLightPosition) : vec3(uLightPosition) - ioPosition0);  

        float diffuseFactor = max(dot(ioNormal0, lightDirection), 0.0);

        vec3 diffuse = diffuseFactor * uLightDiffuseColor;

        vec3 viewDirection = normalize(uViewPosition - ioPosition0);
    
        // negate the light direction so the vector points from the light to the fragment
        vec3 reflectionDirection = reflect(-lightDirection, ioNormal0);

        float specularFactor = pow(max(dot(viewDirection, reflectionDirection), 0.0), shininess);
        vec3 specular = specularStrength * specularFactor * uLightDiffuseColor;

        oColor0 = vec4(ambient + diffuse + specular, 1) * ioColor0;
    }
    else
    {
        oColor0 = ioColor0;
    }
}";





        internal const string Po0No0Uv0Vss = @"
#version 330 core

// location 0 - name iPosition0
// location 1 - name iNormal0
// location 2 - name iNormal1
// location 3 - name iColor0
// location 4 - name iColor1
// location 5 - name iUv0
// location 6 - name iUv1
// location 7 - name iUv2
// location 8 - name iUv3

layout(location = 0) in vec3 iPosition0;  
layout(location = 1) in vec3 iNormal0;  
layout(location = 5) in vec2 iUv0;

out vec3 ioPosition0;
out vec3 ioNormal0;
out vec4 ioColor0;
out vec2 ioUv0;

uniform mat4 uModelMatrix;
uniform mat4 uViewMatrix;
uniform mat4 uProjectionMatrix;
uniform mat4 uModelViewMatrix;
uniform mat4 uModelViewProjectionMatrix;
uniform mat3 uNormalMatrix;

void main(void)
{
    gl_Position = vec4(iPosition0, 1.0) * uModelViewProjectionMatrix;

    // position in view space, used for specular lighting
    ioPosition0 = vec3(uModelMatrix * vec4(iPosition0, 1.0));

    ioNormal0 = normalize(iNormal0 * uNormalMatrix);

	ioColor0 = vec4(1, 1, 1, 1);
    ioUv0 = iUv0;
}";





        internal const string Po0No0Co0Uv0Vss = @"
#version 330 core

// location 0 - name iPosition0
// location 1 - name iNormal0
// location 2 - name iNormal1
// location 3 - name iColor0
// location 4 - name iColor1
// location 5 - name iUv0
// location 6 - name iUv1
// location 7 - name iUv2
// location 8 - name iUv3

layout(location = 0) in vec3 iPosition0;  
layout(location = 1) in vec3 iNormal0;  
layout(location = 3) in vec4 iColor0;
layout(location = 5) in vec2 iUv0;

out vec3 ioPosition0;
out vec3 ioNormal0;
out vec4 ioColor0;
out vec2 ioUv0;

uniform mat4 uModelMatrix;
uniform mat4 uViewMatrix;
uniform mat4 uProjectionMatrix;
uniform mat4 uModelViewMatrix;
uniform mat4 uModelViewProjectionMatrix;
uniform mat3 uNormalMatrix;

void main(void)
{
    gl_Position = vec4(iPosition0, 1.0) * uModelViewProjectionMatrix;

    // position in view space, used for specular lighting
    ioPosition0 = vec3(uModelMatrix * vec4(iPosition0, 1.0));

    ioNormal0 = normalize(iNormal0 * uNormalMatrix);

	ioColor0 = iColor0;
    ioUv0 = iUv0;
}";

        internal const string Po0No0Co0Uv0Fss = @"
#version 330 core

in vec3 ioPosition0;
in vec3 ioNormal0;
in vec4 ioColor0;
in vec2 ioUv0;

out vec4 oColor0;

uniform sampler2D uTexture0;

uniform int uLightCount;
uniform vec4 uLightPosition;
uniform vec3 uLightDiffuseColor;
uniform float uLightAmbientStrength;
uniform vec3 uLightAmbientColor;

uniform vec3 uViewPosition;

void main()
{
    if(uLightCount > 0)
    {
        float specularStrength = 0.20;
    
        int shininess = 256;

        vec3 ambient = uLightAmbientStrength * uLightAmbientColor;

        bool lightIsDirectional = uLightPosition.w < 1.0;

        // vector from the fragment towards the light
        vec3 lightDirection = normalize(lightIsDirectional ? vec3(-uLightPosition) : vec3(uLightPosition) - ioPosition0);  

        float diffuseFactor = max(dot(ioNormal0, lightDirection), 0.0);

        vec3 diffuse = diffuseFactor * uLightDiffuseColor;

        vec3 viewDirection = normalize(uViewPosition - ioPosition0);
    
        // negate the light direction so the vector points from the light to the fragment
        vec3 reflectionDirection = reflect(-lightDirection, ioNormal0);

        float specularFactor = pow(max(dot(viewDirection, reflectionDirection), 0.0), shininess);
        vec3 specular = specularStrength * specularFactor * uLightDiffuseColor;

        oColor0 = vec4(ambient + diffuse + specular, 1) * texture(uTexture0, ioUv0) * ioColor0;
    }
    else
    {
        oColor0 = texture(uTexture0, ioUv0) * ioColor0; 
    }
}";




        internal const string Po0Uv0Vss = @"
#version 330 core

// location 0 - name iPosition0
// location 1 - name iNormal0
// location 2 - name iNormal1
// location 3 - name iColor0
// location 4 - name iColor1
// location 5 - name iUv0
// location 6 - name iUv1
// location 7 - name iUv2
// location 8 - name iUv3

layout(location = 0) in vec3 iPosition0;  
layout(location = 5) in vec2 iUv0;

out vec2 ioUv0;

uniform mat4 uModelMatrix;
uniform mat4 uViewMatrix;
uniform mat4 uProjectionMatrix;

void main(void)
{
    gl_Position = vec4(iPosition0, 1.0) * uModelMatrix * uViewMatrix * uProjectionMatrix;

	ioUv0 = iUv0;
}";

        internal const string Po0Uv0Fss = @"
#version 330 core

uniform sampler2D uTexture0;

in vec2 ioUv0;

out vec4 oColor0;

void main()
{
    oColor0 = texture(uTexture0, ioUv0); 
}";




        internal const string Po0Co0Uv0Vss = @"
#version 330 core

// location 0 - name iPosition0
// location 1 - name iNormal0
// location 2 - name iNormal1
// location 3 - name iColor0
// location 4 - name iColor1
// location 5 - name iUv0
// location 6 - name iUv1
// location 7 - name iUv2
// location 8 - name iUv3

layout(location = 0) in vec3 iPosition0;  
layout(location = 3) in vec4 iColor0;
layout(location = 5) in vec2 iUv0;

out vec4 ioColor0;
out vec2 ioUv0;

uniform mat4 uModelMatrix;
uniform mat4 uViewMatrix;
uniform mat4 uProjectionMatrix;

void main(void)
{
    gl_Position = vec4(iPosition0, 1.0) * uModelMatrix * uViewMatrix * uProjectionMatrix;

	ioColor0 = iColor0;
	ioUv0 = iUv0;
}";

        internal const string Po0Co0Uv0Fss = @"
#version 330 core

uniform sampler2D uTexture0;

in vec4 ioColor0;
in vec2 ioUv0;

out vec4 oColor0;

void main()
{
    oColor0 = texture(uTexture0, ioUv0) * ioColor0; 
}";




        internal const string Po0Uv0Uv1Vss = @"
#version 330 core

// location 0 - name iPosition0
// location 1 - name iNormal0
// location 2 - name iNormal1
// location 3 - name iColor0
// location 4 - name iColor1
// location 5 - name iUv0
// location 6 - name iUv1
// location 7 - name iUv2
// location 8 - name iUv3

layout(location = 0) in vec3 iPosition0;  
layout(location = 5) in vec2 iUv0;
layout(location = 6) in vec2 iUv1;

out vec2 ioUv0;
out vec2 ioUv1;

uniform mat4 uModelMatrix;
uniform mat4 uViewMatrix;
uniform mat4 uProjectionMatrix;

void main(void)
{
    gl_Position = vec4(iPosition0, 1.0) * uModelMatrix * uViewMatrix * uProjectionMatrix;

	ioUv0 = iUv0;
	ioUv1 = iUv1;
}";

        internal const string Po0Uv0Uv1Fss = @"
#version 330 core

uniform sampler2D uTexture0;
uniform sampler2D uTexture1;

in vec2 ioUv0;
in vec2 ioUv1;

out vec4 oColor0;

void main()
{
    oColor0 = mix(texture(uTexture0, ioUv0), texture(uTexture1, ioUv1), 0.5);
}";




        internal const string Po0Co0Uv0Uv1Vss = @"
#version 330 core

// location 0 - name iPosition0
// location 1 - name iNormal0
// location 2 - name iNormal1
// location 3 - name iColor0
// location 4 - name iColor1
// location 5 - name iUv0
// location 6 - name iUv1
// location 7 - name iUv2
// location 8 - name iUv3

layout(location = 0) in vec3 iPosition0;  
layout(location = 3) in vec4 iColor0;
layout(location = 5) in vec2 iUv0;
layout(location = 6) in vec2 iUv1;

out vec4 ioColor0;
out vec2 ioUv0;
out vec2 ioUv1;

uniform mat4 uModelMatrix;
uniform mat4 uViewMatrix;
uniform mat4 uProjectionMatrix;

void main(void)
{
    gl_Position = vec4(iPosition0, 1.0) * uModelMatrix * uViewMatrix * uProjectionMatrix;

	ioColor0 = iColor0;
	ioUv0 = iUv0;
	ioUv1 = iUv1;
}";

        internal const string Po0Co0Uv0Uv1Fss = @"
#version 330 core

uniform sampler2D uTexture0;
uniform sampler2D uTexture1;

in vec4 ioColor0;
in vec2 ioUv0;
in vec2 ioUv1;

out vec4 oColor0;

void main()
{

    
    oColor0 = mix(texture(uTexture0, ioUv0), texture(uTexture1, ioUv1), 0.5) * ioColor0; 
}";




        #endregion Shader Sources

        //private void FlushRenderChunks()
        //{
        //    if (!State.IsEnabled(EnableFlag.Rendering))
        //    {
        //        return;
        //    }

        //    for (var c = 0; c < _renderChunkCount; c++)
        //    {
        //        var renderChunk = _renderChunks[c];

        //        var modelViewMatrix = Matrix4.Mult(renderChunk.ModelMatrix, renderChunk.ViewMatrix);
        //        var modelViewProjectionMatrix = Matrix4.Mult(modelViewMatrix, renderChunk.ProjectionMatrix);

        //        renderChunk.Shader.Use();

        //        renderChunk.Shader.Uniform("uModelMatrix", renderChunk.ModelMatrix);
        //        renderChunk.Shader.Uniform("uViewMatrix", renderChunk.ViewMatrix);
        //        renderChunk.Shader.Uniform("uProjectionMatrix", renderChunk.ProjectionMatrix);
        //        renderChunk.Shader.Uniform("uModelViewMatrix", modelViewMatrix);
        //        renderChunk.Shader.Uniform("uModelViewProjectionMatrix", modelViewProjectionMatrix);
        //        renderChunk.Shader.Uniform("uViewPosition", renderChunk.ViewPosition);

        //        if (renderChunk.VertexFlag.HasFlag(VertexFlag.Normal0) || renderChunk.VertexFlag.HasFlag(VertexFlag.Normal1))
        //        {
        //            // remove any scaling from the model matrix by transposing the inverted matrix and taking only the upper 3x3 part
        //            // see: http://www.lighthouse3d.com/tutorials/glsl-12-tutorial/the-normal-matrix/
        //            renderChunk.Shader.Uniform("uNormalMatrix", new Matrix3(Matrix4.Transpose(Matrix4.Invert(renderChunk.ModelMatrix))));
        //        }

        //        if (renderChunk.VertexFlag.HasFlag(VertexFlag.Uv0))
        //        {
        //            renderChunk.Texture0.UnitAndBind(TextureUnit.Texture0);
        //            renderChunk.Shader.Uniform("uTexture0", 0);
        //        }

        //        if (renderChunk.VertexFlag.HasFlag(VertexFlag.Uv1))
        //        {
        //            renderChunk.Texture1.UnitAndBind(TextureUnit.Texture1);
        //            renderChunk.Shader.Uniform("uTexture1", 1);
        //        }

        //        if (renderChunk.VertexFlag.HasFlag(VertexFlag.Uv2) || renderChunk.VertexFlag.HasFlag(VertexFlag.Uv3))
        //        {
        //            Diagnostics.Warn("Waring FlushRenderChunks(). VertexFlag.Uv2 and VertexFlag.Uv3 are not yet supported.");
        //        }

        //        renderChunk.VertexBuffer.Bind();
        //        renderChunk.VertexBuffer.EnableElements(renderChunk.VertexFlag);

        //        GL.DrawArrays((PrimitiveType)renderChunk.GeometryType, renderChunk.ElementIndex, renderChunk.ElementCount);
        //    }
        //}

        private void FlushRenderBatches()
        {
            for (var c = 0; c < _renderBatchCount; c++)
            {
                var renderBatch = _renderBatches[c];

                if (!renderBatch.State.IsEnabled(EnableFlag.Rendering))
                {
                    continue;
                }

                renderBatch.State.SetState();

                var modelViewMatrix = Matrix4.Mult(renderBatch.ModelMatrix, renderBatch.ViewMatrix);
                var modelViewProjectionMatrix = Matrix4.Mult(modelViewMatrix, renderBatch.ProjectionMatrix);

                renderBatch.Shader.Use();

                renderBatch.Shader.Uniform("uModelMatrix", renderBatch.ModelMatrix);
                renderBatch.Shader.Uniform("uViewMatrix", renderBatch.ViewMatrix);
                renderBatch.Shader.Uniform("uProjectionMatrix", renderBatch.ProjectionMatrix);
                renderBatch.Shader.Uniform("uModelViewMatrix", modelViewMatrix);
                renderBatch.Shader.Uniform("uModelViewProjectionMatrix", modelViewProjectionMatrix);
                renderBatch.Shader.Uniform("uViewPosition", renderBatch.ViewPosition);

                if (renderBatch.VertexFlags.HasFlag(VertexFlag.Normal0) || renderBatch.VertexFlags.HasFlag(VertexFlag.Normal1))
                {
                    // remove any scaling from the model matrix by transposing the inverted matrix and taking only the upper 3x3 part
                    // see: http://www.lighthouse3d.com/tutorials/glsl-12-tutorial/the-normal-matrix/
                    renderBatch.Shader.Uniform("uNormalMatrix", new Matrix3(Matrix4.Transpose(Matrix4.Invert(renderBatch.ModelMatrix))));
                }

                if (renderBatch.VertexFlags.HasFlag(VertexFlag.Uv0))
                {
                    renderBatch.Texture0.UnitAndBind(TextureUnit.Texture0);
                    renderBatch.Shader.Uniform("uTexture0", 0);
                }

                if (renderBatch.VertexFlags.HasFlag(VertexFlag.Uv1))
                {
                    renderBatch.Texture1.UnitAndBind(TextureUnit.Texture1);
                    renderBatch.Shader.Uniform("uTexture1", 1);
                }

                if (renderBatch.VertexFlags.HasFlag(VertexFlag.Uv2) || renderBatch.VertexFlags.HasFlag(VertexFlag.Uv3))
                {
                    Diagnostics.Warn("Waring FlushRenderChunks(). VertexFlag.Uv2 and VertexFlag.Uv3 are not yet supported.");
                }

                renderBatch.VertexBuffer.Bind();
                renderBatch.VertexBuffer.EnableElements(renderBatch.VertexFlags);

                GL.DrawArrays((PrimitiveType)renderBatch.GeometryType, renderBatch.ElementIndex, renderBatch.ElementCount);
            }

            _renderBatchCount = 0;
        }

        //protected void InsertRenderBatchAndWriteToVertexBuffer(AbstractGeometry geometry)
        //{
        //    var shader = QueryShader(geometry);
        //    var vertexBuffer = QueryVertexBuffer(geometry);

        //    var camera = ActiveCamera();

        //    var vertexCount = geometry.VertexCount(geometry.ModelMatrix(), camera.ViewMatrix(), camera.ProjectionMatrix());

        //    if (!vertexBuffer.CanWriteElements(vertexCount) || _renderChunkCount + 1 >= _maxRenderChunks)
        //    {
        //        Flush();
        //    }

        //    geometry.WriteToVertexBuffer(vertexBuffer);

        //    // todo: is this all needed? why not store just the Geometry object?
        //    _renderChunks[_renderChunkCount].Shader = shader;
        //    _renderChunks[_renderChunkCount].VertexBuffer = vertexBuffer;
        //    _renderChunks[_renderChunkCount].ViewMatrix = camera.ViewMatrix();
        //    _renderChunks[_renderChunkCount].ViewPosition = camera.Position;
        //    _renderChunks[_renderChunkCount].ProjectionMatrix = camera.ProjectionMatrix();
        //    _renderChunks[_renderChunkCount].ModelMatrix = geometry.ModelMatrix();
        //    _renderChunks[_renderChunkCount].GeometryType = geometry.GeometryType;
        //    _renderChunks[_renderChunkCount].ElementCount = vertexCount;
        //    _renderChunks[_renderChunkCount].ElementIndex = vertexBuffer.UsedElements() - vertexCount;
        //    _renderChunks[_renderChunkCount].Texture0 = geometry._texture0;
        //    _renderChunks[_renderChunkCount].Texture1 = geometry._texture1;
        //    _renderChunks[_renderChunkCount].Texture2 = geometry._texture2;
        //    _renderChunks[_renderChunkCount].Texture3 = geometry._texture3;
        //    _renderChunks[_renderChunkCount].VertexFlag = geometry.VertexFlags;

        //    _renderChunkCount++;
        //}

        protected void CreateRenderBatchesAndWriteToVertexBuffer(AbstractGeometry geometry)
        {
            var shader = QueryShader(geometry);
            var vertexBuffer = QueryVertexBuffer(geometry);
            var camera = ActiveCamera();

            var renderBatches = geometry.ToRenderBatch(this, State, shader, vertexBuffer, camera);

            for(var i = 0; i < renderBatches.Length; i++)
            {
                _renderBatches[_renderBatchCount++] = renderBatches[i];

                if(_renderBatchCount == _maxRenderBatchCount)
                {
                    FlushRenderBatches();
                }
            }



            //var vertexCount = geometry.VertexCount(geometry.ModelMatrix(), camera.ViewMatrix(), camera.ProjectionMatrix());

            //if (!vertexBuffer.CanWriteElements(vertexCount) || _renderBatchCount + 1 >= _maxRenderBatchCount)
            //{
            //    Flush();
            //}

            //geometry.WriteToVertexBuffer(vertexBuffer);

            //_renderBatches[_renderBatchCount].State = State;
            //_renderBatches[_renderBatchCount].Shader = shader;
            //_renderBatches[_renderBatchCount].VertexBuffer = vertexBuffer;
            //_renderBatches[_renderBatchCount].ViewMatrix = camera.ViewMatrix();
            //_renderBatches[_renderBatchCount].ViewPosition = camera.Position;
            //_renderBatches[_renderBatchCount].ProjectionMatrix = camera.ProjectionMatrix();
            //_renderBatches[_renderBatchCount].ModelMatrix = geometry.ModelMatrix();
            //_renderBatches[_renderBatchCount].GeometryType = geometry.GeometryType;
            //_renderBatches[_renderBatchCount].ElementCount = vertexCount;
            //_renderBatches[_renderBatchCount].ElementIndex = vertexBuffer.UsedElements() - vertexCount;
            //_renderBatches[_renderBatchCount].Texture0 = geometry._texture0;
            //_renderBatches[_renderBatchCount].Texture1 = geometry._texture1;
            //_renderBatches[_renderBatchCount].Texture2 = geometry._texture2;
            //_renderBatches[_renderBatchCount].Texture3 = geometry._texture3;
            //_renderBatches[_renderBatchCount].VertexFlag = geometry.VertexFlags;

            //_renderBatchCount++;
        }

        private void InitializeStandardShaderDictionary()
        {
            if (_standardShaderDictionary == null)
            {
                _standardShaderDictionary = new Dictionary<string, Shader>();
            }

            _standardShaderDictionary.Clear();

            InitializeStandardShaderDictionary("Po0", GraphicsContext.Po0Vss, GraphicsContext.Po0Fss);
            InitializeStandardShaderDictionary("Po0-No0", GraphicsContext.Po0No0Vss, GraphicsContext.Po0No0Co0Fss);
            InitializeStandardShaderDictionary("Po0-Co0", GraphicsContext.Po0Co0Vss, GraphicsContext.Po0Co0Fss);
            InitializeStandardShaderDictionary("Po0-No0-Co0", GraphicsContext.Po0No0Co0Vss, GraphicsContext.Po0No0Co0Fss);
            InitializeStandardShaderDictionary("Po0-Uv0", GraphicsContext.Po0Uv0Vss, GraphicsContext.Po0Uv0Fss);
            InitializeStandardShaderDictionary("Po0-Co0-Uv0", GraphicsContext.Po0Co0Uv0Vss, GraphicsContext.Po0Co0Uv0Fss);
            InitializeStandardShaderDictionary("Po0-No0-Uv0", GraphicsContext.Po0No0Uv0Vss, GraphicsContext.Po0No0Co0Uv0Fss);
            InitializeStandardShaderDictionary("Po0-No0-Co0-Uv0", GraphicsContext.Po0No0Co0Uv0Vss, GraphicsContext.Po0No0Co0Uv0Fss);
            InitializeStandardShaderDictionary("Po0-Uv0-Uv1", GraphicsContext.Po0Uv0Uv1Vss, GraphicsContext.Po0Uv0Uv1Fss);
            InitializeStandardShaderDictionary("Po0-Co0-Uv0-Uv1", GraphicsContext.Po0Co0Uv0Uv1Vss, GraphicsContext.Po0Co0Uv0Uv1Fss);
        }

        public string QueryStandardVertexShaderSource(VertexFlag vertexFlag)
        {
            var key = vertexFlag.StandardShaderDictionaryKey();

            switch (key)
            {
                case "po0":
                    return GraphicsContext.Po0Vss;
                case "po0-no0":
                    return GraphicsContext.Po0No0Vss;
                case "po0-co0":
                    return GraphicsContext.Po0Co0Vss;
                case "po0-no0-co0":
                    return GraphicsContext.Po0No0Co0Vss;
                case "po0-uv0":
                    return GraphicsContext.Po0Uv0Vss;
                case "po0-co0-uv0":
                    return GraphicsContext.Po0Co0Uv0Vss;
                case "po0-no0-uv0":
                    return GraphicsContext.Po0No0Uv0Vss;
                case "po0-no0-co0-uv0":
                    return GraphicsContext.Po0No0Co0Uv0Vss;
                case "po0-uv0-uv1":
                    return GraphicsContext.Po0Uv0Uv1Vss;
                case "po0-co0-uv0-uv1":
                    return GraphicsContext.Po0Co0Uv0Uv1Vss;
                default:
                    throw new InvalidOperationException($"QueryStandardVertexShaderSource(VertexFlag) failed. Could not find a vertex shader source for key: {key}.");

            }
        }

        private void InitializeStandardVertexBuffers()
        {
            _standardVertexBufferArray[0] = new VertexBuffer(
                DefaultVertexBufferElementCount,
                new[] {
                    new VertexMapping.Map { VertexFlag = VertexFlag.Position0, ElementType = VertexMapping.ElementType.Single, Count = 3 }, // location 0 - name iPosition0
                    new VertexMapping.Map { VertexFlag = VertexFlag.Normal0, ElementType = VertexMapping.ElementType.Single, Count = 3 },   // location 1 - name iNormal0
                    new VertexMapping.Map { VertexFlag = VertexFlag.Normal1, ElementType = VertexMapping.ElementType.Single, Count = 3 },   // location 2 - name iNormal1
                    new VertexMapping.Map { VertexFlag = VertexFlag.Color0, ElementType = VertexMapping.ElementType.Single, Count = 4 },    // location 3 - name iColor0
                    new VertexMapping.Map { VertexFlag = VertexFlag.Color1, ElementType = VertexMapping.ElementType.Single, Count = 4 },    // location 4 - name iColor1
                    new VertexMapping.Map { VertexFlag = VertexFlag.Uv0, ElementType = VertexMapping.ElementType.Single, Count = 2 },       // location 5 - name iUv0
                    new VertexMapping.Map { VertexFlag = VertexFlag.Uv1, ElementType = VertexMapping.ElementType.Single, Count = 2 },       // location 6 - name iUv1
                    new VertexMapping.Map { VertexFlag = VertexFlag.Uv2, ElementType = VertexMapping.ElementType.Single, Count = 2 },       // location 7 - name iUv2
                    new VertexMapping.Map { VertexFlag = VertexFlag.Uv3, ElementType = VertexMapping.ElementType.Single, Count = 2 },       // location 8 - name iUv3
                },
                VertexBufferUsage.DynamicDraw);

            _standardVertexBufferArrayCount++;
        }

        #region Fields and Properties

        ///// <summary>
        ///// default == 256
        ///// </summary>
        //private int _maxRenderChunks = 256;

        //private int _renderChunkCount;

        //private RenderChunk[] _renderChunks;

        /// <summary>
        /// default == 256
        /// </summary>
        private int _maxRenderBatchCount = 256;

        private int _renderBatchCount;

        private RenderBatch[] _renderBatches;


        /// <summary>
        /// default == 4096
        /// </summary>
        private int _maxGeometryCount = 4096;

        private int _geometryCount;

        private AbstractGeometry[] _geometry;


        ///// <summary>
        ///// default == 4096
        ///// </summary>
        //private int _maxTesselatedGeometryCount = 4096;

        //private int _tesselatedGeometryCount;

        //private AbstractGeometry[] _tesselatedGeometry;

        // todo implement default camera instead of relying on the camera provided by AbstractSketch
        private ICamera _activeCamera { get; set; }

        public ICamera ActiveCamera() 
        { 
            return _activeCamera; 
        }

        public void ActiveCamera(ICamera camera)
        {
            _activeCamera = camera;
        }

        private static int GraphicsContextCount { get; set; }

        public static GraphicsContext Default { get; set; }

        /// <summary>
        /// default == Color4.White
        /// </summary>
        protected Color4 _clearColor = Color4.White;

        /// <summary>
        /// the OpenGl Handle to the intenal framebuffer if any was created, or 0 if this is the default framebuffer
        /// default == 0 or the default framebuffer
        /// </summary>
        public int Handle
        {
            get
            {
                return _frameBuffer == null ? 0 : _frameBuffer.Handle;
            }
        }

        public bool IsDefault
        {
            get { return _frameBuffer == null; }
        }

        private FrameBuffer _frameBuffer;

        private Shader _attachedShader { get; set; }

        public void AttachShader(Shader shader)
        {
            _attachedShader = shader;
        }

        public Shader AttachedShader()
        {
            return _attachedShader;
        }

        private Dictionary<string, Shader> _standardShaderDictionary;



        private const int DefaultVertexBufferElementCount = 32768;

        //private VertexBuffer _defaultVertexBuffer { get; set; }

        private VertexBuffer _attachedVertexBuffer { get; set; }

        public VertexBuffer AttachedVertexBuffer()
        {
            return _attachedVertexBuffer;
        }

        public void AttachVertexBuffer(VertexBuffer vertexBuffer)
        {
            _attachedVertexBuffer = vertexBuffer;
        }

        private const int _standardVertexBufferArraySize = 8;

        private int _standardVertexBufferArrayCount = 0;

        private VertexBuffer[] _standardVertexBufferArray = new VertexBuffer[_standardVertexBufferArraySize];

        #endregion Fields and Properties

        #region Constructors

        public GraphicsContext(
            int width,
            int height,
            int colorBits = GraphicsSettings.DefaultColorBits,
            int depthBits = GraphicsSettings.DefaultDepthBits,
            int stencilBits = GraphicsSettings.DefaultStencilBits,
            int samples = GraphicsSettings.DefaultSamples)
            : base(width, height, colorBits, depthBits, stencilBits, samples)
        {
            _modelMatrixStack[_modelMatrixStackIndex] = Matrix4.Identity;

            GraphicsContextCount++;

            if (GraphicsContextCount == 1)
            {
                // default OpenGl context
                GraphicsContext.Default = this;
            }
            else
            {
                // custom OpenGl context, create a framebuffer to render to
                _frameBuffer = new FrameBuffer(Width, Height, ColorBits, DepthBits, StencilBits, Samples);
            }
        }

        public GraphicsContext(GraphicsSettings graphicsSettings)
            : this(
                  graphicsSettings.Width,
                  graphicsSettings.Height,
                  graphicsSettings.ColorBits,
                  graphicsSettings.DepthBits,
                  graphicsSettings.StencilBits,
                  graphicsSettings.Samples)
        {
        }

        #endregion Constructors

        #region Matrix Stack Stuff

        private const int _modelMatrixStackSize = 128;
        private int _modelMatrixStackIndex = 0;

        private Matrix4[] _modelMatrixStack = new Matrix4[_modelMatrixStackSize];

        public Matrix4 ModelMatrix()
        {
            return _modelMatrixStack[_modelMatrixStackIndex];
        }

        public Matrix4 ModelMatrix(Matrix4 modelMatrix)
        {
            _modelMatrixStack[_modelMatrixStackIndex] = modelMatrix;

            return ModelMatrix();
        }

        public void ClearMatrixStack()
        {
            _modelMatrixStackIndex = 0;
            _modelMatrixStack[_modelMatrixStackIndex] = Matrix4.Identity;
        }

        public void PushMatrix()
        {
            if (_modelMatrixStackIndex + 1 >= _modelMatrixStackSize)
            {
                throw new InvalidOperationException($"PushMatrix() failed. _currentModelMatrixStackIndex has been reached: {_modelMatrixStackIndex}");
            }

            _modelMatrixStack[_modelMatrixStackIndex + 1] = _modelMatrixStack[_modelMatrixStackIndex];
            _modelMatrixStackIndex++;
        }

        public void PopMatrix()
        {
            if (_modelMatrixStackIndex == 0)
            {
                return;
                // throw new InvalidOperationException($"PopMatrix() failed. _currentModelMatrixStackIndex is {_currentModelMatrixStackIndex}. There are more PopMatrix() calls then PushMatrix() calls.");
            }

            _modelMatrixStackIndex--;
        }

        public void Translate(float x, float y)
        {
            // assume 2d translation along the x-axis and y-axis
            Translate(x, y, 0f);
        }

        public void Translate(ref Vector2 vector)
        {
            // assume 2d translation along the x-axis and y-axis
            Translate(vector.X, vector.Y, 0f);
        }

        public void Translate(float x, float y, float z)
        {
            // todo test this out
            //Matrix4.Mult(_modelMatrixStack[_currentModelMatrixStackIndex], Matrix4.CreateTranslation(x, y, z), out _modelMatrixStack[_currentModelMatrixStackIndex]);

            _modelMatrixStack[_modelMatrixStackIndex] = _modelMatrixStack[_modelMatrixStackIndex] * Matrix4.CreateTranslation(x, y, z);
        }

        public void Translate(ref Vector3 vector)
        {
            // assume 2d translation along the x-axis and y-axis
            Translate(vector.X, vector.Y, vector.Z);
        }

        public void Transform(Matrix4 transform)
        {
            Transform(ref transform);
        }

        public void Transform(ref Matrix4 transform)
        {
            _modelMatrixStack[_modelMatrixStackIndex] = transform;
        }

        public void Scale(float scale)
        {
            Scale(scale, scale, scale);
        }

        public void Scale(float x, float y)
        {
            // assume 2d scaling along the x-axis and y-axis
            Scale(x, y, 1f);
        }

        public void Scale(float x, float y, float z)
        {
            // todo test this out
            //Matrix4.Mult(_modelMatrixStack[_currentModelMatrixStackIndex], Matrix4.CreateScale(x, y, z), out _modelMatrixStack[_currentModelMatrixStackIndex]);

            _modelMatrixStack[_modelMatrixStackIndex] = _modelMatrixStack[_modelMatrixStackIndex] * Matrix4.CreateScale(x, y, z);
        }

        public void Scale(ref Vector3 v)
        {
            Scale(v.X, v.Y, v.Z);
        }

        public void Scale(ref Vector2 v)
        {
            Scale(v.X, v.Y, 1f);
        }

        public void RotateTo(Vector3 target)
        {
            var translation = _modelMatrixStack[_modelMatrixStackIndex].ExtractTranslation();

            _modelMatrixStack[_modelMatrixStackIndex] = Utility.ObjectLookAtMatrix(translation, target);
        }

        public void Rotate(float degrees, Vector3 axis)
        {
            // todo test this out
            //Matrix4.Mult(_modelMatrixStack[_currentModelMatrixStackIndex], Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(degrees)), out _modelMatrixStack[_currentModelMatrixStackIndex]);

            _modelMatrixStack[_modelMatrixStackIndex] = _modelMatrixStack[_modelMatrixStackIndex] * Matrix4.CreateFromAxisAngle(axis, MathHelper.DegreesToRadians(degrees));
        }

        /// <summary>
        /// default == PrimitiveType.Triangles
        /// </summary>
        public PrimitiveType ActiveRenderPrimitiveType { get; set; } = PrimitiveType.Triangles;

        //public void Render()
        //{
        //    Render(ActiveRenderPrimitiveType, 0, _defaultVertexBuffer.TotelElements());
        //}

        //public void Render(int index, int count)
        //{
        //    Render(ActiveRenderPrimitiveType, index, count);
        //}

        //public void Render(PrimitiveType primitiveType, int index, int count)
        //{
        //    _defaultVertexBuffer.Bind();

        //    var shader = ActiveOrStandardShader();

        //    shader.Use();

        //    GL.DrawArrays(primitiveType, index, count);
        //}

        //private Shader ActiveOrStandardShader()
        //{
        //    var shader = AttachedShader();

        //    if (shader == null)
        //    {
        //        shader = QueryStandardShader(_defaultVertexBuffer.VertexFlags);
        //    }

        //    return shader;
        //}

        /// <summary>
        /// default == VertexFlag.Position0 | VertexFlag.Color0
        /// </summary>
        //private VertexFlag ActiveVertexBufferFlags = VertexFlag.Position0 | VertexFlag.Color0;

        //public void EnableVertexBuffer(VertexFlag vertexFlags)
        //{
        //    EnableVertexBuffer(vertexFlags, AttachedShader());
        //}

        //public void EnableVertexBuffer(VertexFlag vertexFlags, Shader shader)
        //{
        //    _defaultVertexBuffer.ToggleElements(shader, vertexFlags);
        //}

        public void Rotate(float degrees)
        {
            // assume 2d rotation along the z-axis
            Rotate(degrees, Vector3.UnitZ);
        }

        public void Rotate(float degrees, Axis axis)
        {
            Rotate(degrees, axis.ToVector());
        }

        #endregion Matrix Stack Stuff

        #region Load

        public virtual void Load()
        {
            InitializeStandardShaderDictionary();

            //_renderChunks = new RenderChunk[_maxRenderChunks];

            //for (var i = 0; i < _maxRenderChunks; i++)
            //{
            //    _renderChunks[i] = new RenderChunk();
            //}

            _renderBatches = new RenderBatch[_maxRenderBatchCount];

            for (var i = 0; i < _maxRenderBatchCount; i++)
            {
                _renderBatches[i] = new RenderBatch();
            }

            _geometry = new AbstractGeometry[_maxGeometryCount];

            //_tesselatedGeometry = new AbstractGeometry[_maxTesselatedGeometryCount];

            InitializeStandardVertexBuffers();

            GL.ClearColor(_clearColor);
        }

        #endregion Load

        #region Reset

        public void Reset()
        {
            for(var i = 0; i < _standardVertexBufferArrayCount; i++)
            {
                _standardVertexBufferArray[i].Clear();
            }

            //ResetRenderChunks();

            _renderBatchCount = 0;

            ResetGeometry();

            //ResetTesselatedGeometry();
        }

        //private void ResetRenderChunks()
        //{
        //    _renderChunkCount = 0;
        //}

        #endregion Reset

        #region Flush

        public void Flush()
        {
            //TesselateGeometry();

            //GeometryToRenderChunks();

            GeometryToRenderBatches();

            //FlushRenderChunks();

            FlushRenderBatches();

            Reset();
        }

        #endregion Flush

        #region Standard Shaders Functions

        private void InitializeStandardShaderDictionary(string key, string vertexShaderSource, string fragmentShaderSource)
        {
            var shader = new Shader(vertexShaderSource, fragmentShaderSource);

            _standardShaderDictionary.Add(key.ToLowerInvariant(), shader);
        }

        //private void InitializeStandardShaderDictionary(string key)
        //{
        //    var vertexShaderSource = QueryGraphicsContextShaderSource(key, "vss");
        //    var fragmentShaderSource = QueryGraphicsContextShaderSource(key, "fss");

        //    if (vertexShaderSource != null && fragmentShaderSource != null)
        //    {
        //        var shader = new Shader(vertexShaderSource, fragmentShaderSource);

        //        _standardShaderDictionary.Add(key.ToLowerInvariant(), shader);

        //        return;
        //    }

        //    if (vertexShaderSource == null && fragmentShaderSource == null)
        //    {
        //        return;
        //    }

        //    if (vertexShaderSource != null)
        //    {
        //        throw new NotImplementedException($"InitializeStandardShaderDictionary(string key) failed. could not find vertex shader source for key: {key}. The GraphicsContext class should contain a const property with the name {key}vss (case incensitive)");
        //    }

        //    if (fragmentShaderSource != null)
        //    {
        //        throw new NotImplementedException($"InitializeStandardShaderDictionary(string key) failed. could not find flagment shader source for key: {key}. The GraphicsContext class should contain a const property with the name {key}fss (case incensitive)");
        //    }
        //}

        //protected Shader QueryStandardShader(VertexBuffer vertexBuffer)
        //{
        //    return QueryStandardShader(vertexBuffer.VertexFlags, vertexBuffer);
        //}

        //protected Shader QueryStandardShader(VertexFlag vertexFlag, VertexBuffer vertexBuffer)
        //{
        //    var key = vertexFlag.StandardShaderDictionaryKey();

        //    if (_standardShaderDictionary.ContainsKey(key.ToLowerInvariant()))
        //    {
        //        return _standardShaderDictionary[key];
        //    }

        //    throw new InvalidOperationException($"QueryShaderFor(VertexBuffer vertexBuffer) failed. Could not find a shader source for key: {key}.");
        //}

        protected Shader QueryStandardShader(VertexFlag vertexFlag)
        {
            var key = vertexFlag.StandardShaderDictionaryKey();

            if (_standardShaderDictionary.ContainsKey(key))
            {
                return _standardShaderDictionary[key];
            }

            throw new InvalidOperationException($"QueryShaderFor(VertexBuffer vertexBuffer) failed. Could not find a shader source for key: {key}.");
        }

        protected VertexBuffer QueryStandardVertexBuffer(VertexFlag vertexFlag)
        {
            // todo fix for multiple vertexbuffers
            return _standardVertexBufferArray[0];

            //foreach (var vertexBuffer in _standardVertexBufferArray)
            //{
            //    if(vertexBuffer.IsMatch(vertexFlag))
            //    {
            //        vertexFlag.RemoveFlag(vertexFlag);

            //        yield return vertexBuffer;
            //    }
            //}
        }

        #endregion Standard Shaders Functions

        public void Bind(BindFlag bindFlag = BindFlag.Default)
        {
            GL.BindFramebuffer(ToFramebufferTarget(bindFlag), Handle);

            GL.Viewport(0, 0, Width, Height);
        }

        private FramebufferTarget ToFramebufferTarget(BindFlag bindFlag)
        {
            if (bindFlag == BindFlag.Read)
            {
                return FramebufferTarget.ReadFramebuffer;
            }

            if (bindFlag == BindFlag.Write)
            {
                return FramebufferTarget.DrawFramebuffer;
            }

            return FramebufferTarget.Framebuffer;
        }

        public GraphicsState State { get; set; } = GraphicsState.DefaultState();

        public void ClearBuffers()
        {
            var clearFlag = ClearFlag.None;

            if (ColorBits != 0)
            {
                clearFlag = clearFlag.AddFlag(ClearFlag.ColorBuffer);
            }

            if (DepthBits != 0)
            {
                clearFlag = clearFlag.AddFlag(ClearFlag.DepthBuffer);
            }

            if (StencilBits != 0)
            {
                clearFlag = clearFlag.AddFlag(ClearFlag.StencilBuffer);
            }

            ClearBuffers(clearFlag);
        }

        public void ClearBuffers(ClearFlag clearFlag)
        {
            if(!State.IsEnabled(EnableFlag.ClearBuffers))
            {
                return;
            }

            GL.Clear((ClearBufferMask)clearFlag);
        }

        public void ClearColor(float grey, float a = 1f)
        {
            ClearColor(grey, grey, grey, a);
        }

        public void ClearColor(int grey, int a = 255)
        {
            ClearColor(grey, grey, grey, a);
        }
        public void ClearColor(int r, int g, int b, int a = 255)
        {
            ClearColor(r * Utility.ByteToUnitSingleFactor, g * Utility.ByteToUnitSingleFactor, b * Utility.ByteToUnitSingleFactor, a * Utility.ByteToUnitSingleFactor);
        }

        public void ClearColor(float r, float g, float b, float a = 1f)
        {
            _clearColor.R = r;
            _clearColor.G = g;
            _clearColor.B = b;
            _clearColor.A = a;

            GL.ClearColor(_clearColor);
        }

        public VertexBuffer StandardVertexBuffer()
        {
            return _standardVertexBufferArray[0];
        }

        public Shader StandardShader(VertexFlag vertexFlag)
        {
            return QueryStandardShader(vertexFlag);
        }

        /// <summary>
        /// Qeuries for a valid shader for this Geometry
        /// Looks first for the shader bound to this Geometry
        /// Then for the ActiveShader bound to this GraphicsContext
        /// Then queries the standard shaders
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns>null if no shader was found</returns>
        private VertexBuffer QueryVertexBuffer(AbstractGeometry geometry)
        {
            var vertexBuffer = AttachedVertexBuffer();

            if (vertexBuffer == null)
            {
                vertexBuffer = QueryStandardVertexBuffer(geometry.VertexFlags);
            }

            return vertexBuffer;
        }

        /// <summary>
        /// Qeuries for a valid shader for this Geometry
        /// Looks first for the shader bound to this Geometry
        /// Then for the ActiveShader bound to this GraphicsContext
        /// Then queries the standard shaders
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns>null if no shader was found</returns>
        private Shader QueryShader(AbstractGeometry geometry)
        {
            var shader = AttachedShader();

            if (shader == null)
            {
                shader = QueryStandardShader(geometry.VertexFlags);
            }

            return shader;
        }



        //private void TesselateGeometry()
        //{
        //    if (_geometryCount == 0)
        //    {
        //        return;
        //    }

        //    // todo
        //}

        //private void GeometryToRenderChunks()
        //{
        //    GeometryToRenderChunks(_geometry, _geometryCount);

        //    //GeometryToRenderChunks(_tesselatedGeometry, _tesselatedGeometryCount);
        //}

        //private void GeometryToRenderChunks(AbstractGeometry[] geometry, int count)
        //{
        //    for (var i = 0; i < count; i++)
        //    {
        //        InsertRenderBatchAndWriteToVertexBuffer(geometry[i]);
        //    }
        //}

        private void GeometryToRenderBatches()
        {
            for (var i = 0; i < _geometryCount; i++)
            {
                CreateRenderBatchesAndWriteToVertexBuffer(_geometry[i]);
            }
        }

        #region Geometry Functions

        public RectangleGeometry Rectangle()
        {
            var g = new RectangleGeometry(ModelMatrix());

            AddGeometry(g);

            return g;
        }

        public QuadGeometry Quad()
        {
            var g = new QuadGeometry(ModelMatrix());

            AddGeometry(g);

            return g;
        }

        public CircleGeometry Circle()
        {
            return Circle(CircleGeometry.DefaultRadius);
        }

        public LineGeometry Line()
        {
            var g = new LineGeometry(ModelMatrix());

            AddGeometry(g);

            return g;
        }

        public CircleGeometry Circle(float radius)
        {
            var g = new CircleGeometry(radius, ModelMatrix());

            AddGeometry(g);

            return g;
        }

        public EllipseGeometry Ellipse()
        {
            return Ellipse(EllipseGeometry.DefaultWidth, EllipseGeometry.DefaultHeight);
        }

        public EllipseGeometry Ellipse(float width, float height)
        {
            var g = new EllipseGeometry(width, height, ModelMatrix());

            AddGeometry(g);

            return g;
        }

        public CubeGeometry Cube()
        {
            return Cube(CubeGeometry.DefaultWidth, CubeGeometry.DefaultHeight, CubeGeometry.DefaultDepth);
        }

        public CubeGeometry Cube(float width, float height, float depth)
        {
            var g = new CubeGeometry(width, height, depth, ModelMatrix());

            AddGeometry(g);

            return g;
        }

        public SphereGeometry Sphere(SphereMode sphereMode = SphereMode.IcoSphere)
        {
            return Sphere(SphereGeometry.DefaultWidth, SphereGeometry.DefaultHeight, SphereGeometry.DefaultDepth, sphereMode);
        }

        public SphereGeometry Sphere(float width, float height, float depth, SphereMode sphereMode = SphereMode.IcoSphere)
        {
            var g = new SphereGeometry(width, height, depth, ModelMatrix(), sphereMode);

            AddGeometry(g);

            return g;
        }

        public Vector2 ModelToScreenSpace(Vector3 position, ref Matrix4 modelViewProjectionMatrix, int width, int height)
        {
            Vector4 pos = new Vector4(position, 1f) * modelViewProjectionMatrix;

            pos /= pos.W;
            pos.Y = -pos.Y;

            Vector2 screenSize = new Vector2(width, height);
            Vector2 screenCenter = screenSize / 2f;

            return screenCenter + pos.Xy * screenSize / 2f;
        }

        public Vector2 ModelToScreenSpace(Vector3 position, ref Matrix4 modelMatrix, ref Matrix4 viewMatrix, ref Matrix4 projectionMatrix, int width, int height)
        {
            var modelViewProjectionMatrix = Matrix4.Mult(modelMatrix, Matrix4.Mult(viewMatrix, projectionMatrix));
            
            return ModelToScreenSpace(position, ref modelViewProjectionMatrix, width, height);
        }

        public Vector2 ModelToScreenSpace(Vector3 position, Matrix4 modelMatrix, Matrix4 viewMatrix, Matrix4 projectionMatrix, int width, int height)
        {
            return ModelToScreenSpace(position, ref modelMatrix, ref viewMatrix, ref projectionMatrix, width, height);
        }

        //public Vector3 getRayFromScreenSpace(const vec2 & pos)
        //{
        //    mat4 invMat = inverse(m_glData.getPerspective() * m_glData.getView());
        //    vec4 near = vec4((pos.x - Constants::m_halfScreenWidth) / Constants::m_halfScreenWidth, -1 * (pos.y - Constants::m_halfScreenHeight) / Constants::m_halfScreenHeight, -1, 1.0);
        //    vec4 far = vec4((pos.x - Constants::m_halfScreenWidth) / Constants::m_halfScreenWidth, -1 * (pos.y - Constants::m_halfScreenHeight) / Constants::m_halfScreenHeight, 1, 1.0);
        //    vec4 nearResult = invMat * near;
        //    vec4 farResult = invMat * far;
        //    nearResult /= nearResult.w;
        //    farResult /= farResult.w;
        //    vec3 dir = vec3(farResult - nearResult);
        //    return normalize(dir);
        //}

        //// winX and winY will be the corners of your screen in pixels
        //// winZ is a number in [0,1] which will specify where between zNear and zFar
        //// 
        //int glhUnProjectf(float winx, float winy, float winz, float* modelview, float* projection, int* viewport, float* objectCoordinate)
        //    {
        //        // Transformation matrices
        //        float m[16], A[16];
        //        float in[4], out[4];
        //        // Calculation for inverting a matrix, compute projection x modelview
        //        // and store in A[16]
        //        MultiplyMatrices4by4OpenGL_FLOAT(A, projection, modelview);
        //        // Now compute the inverse of matrix A
        //        if (glhInvertMatrixf2(A, m) == 0)
        //            return 0;
        //  // Transformation of normalized coordinates between -1 and 1
        //  in[0]= (winx - (float)viewport[0]) / (float)viewport[2] * 2.0 - 1.0;
        //  in[1]= (winy - (float)viewport[1]) / (float)viewport[3] * 2.0 - 1.0;
        //  in[2]= 2.0 * winz - 1.0;
        //  in[3]= 1.0;
        //        // Objects coordinates
        //        MultiplyMatrixByVector4by4OpenGL_FLOAT(out, m, in);
        //        if (out[3]== 0.0)
        //     return 0;
        //  out[3]= 1.0 /out[3];
        //        objectCoordinate[0] =out[0]*out[3];
        //        objectCoordinate[1] =out[1]*out[3];
        //        objectCoordinate[2] =out[2]*out[3];
        //        return 1;
        //    }

        //public TGeometry CreateGeometry<TGeometry>(GeometryType geometryType) where TGeometry : AbstractGeometry, new()
        //{
        //    return CreateGeometry<TGeometry>(geometryType, ModelMatrix());
        //}

        //public TGeometry CreateGeometry<TGeometry>(GeometryType geometryType, Matrix4 modelMatrix) where TGeometry : AbstractGeometry, new()
        //{
        //    return CreateGeometry<TGeometry>(geometryType, Geometry.DefaultVertexCount, modelMatrix);
        //}

        //public TGeometry CreateGeometry<TGeometry>(GeometryType geometryType, int vertexCount, Matrix4 modelMatrix) where TGeometry : AbstractGeometry, new()
        //{
        //    var g = new TGeometry(geometryType, vertexCount, modelMatrix);

        //    AddGeometry(g);

        //    return g;
        //}

        public void AddGeometry(AbstractGeometry geometry)
        {
            if (_geometryCount + 1 >= _maxGeometryCount)
            {
                throw new InvalidOperationException($"AddGeometry(Geometry geometry) failed. _maxGeometryCount had been reached: {_geometryCount}");
            }

            _geometry[_geometryCount++] = geometry;
        }

        protected void ResetGeometry()
        {
            for (var i = 0; i < _geometryCount; i++)
            {
                _geometry[i].Dispose();
                _geometry[i] = null;
            }

            _geometryCount = 0;
        }

        //protected void ResetTesselatedGeometry()
        //{
        //    for (var i = 0; i < _tesselatedGeometryCount; i++)
        //    {
        //        _tesselatedGeometry[i].Dispose();
        //        _tesselatedGeometry[i] = null;
        //    }

        //    _tesselatedGeometryCount = 0;
        //}

        #endregion Geometry Functions

        #region Dispose

        public virtual void Dispose()
        {
            if(_frameBuffer != null)
            {
                _frameBuffer.Dispose();
                _frameBuffer = null;
            }

            for (var i = 0; i < _standardVertexBufferArrayCount; i++)
            {
                _standardVertexBufferArray[i].Dispose();
                _standardVertexBufferArray[i] = null;
            }

            GC.SuppressFinalize(this);
        }

        #endregion Dispose
    }
}
