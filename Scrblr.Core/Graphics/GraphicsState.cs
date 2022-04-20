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
    public enum EnableFlag
    {
        DepthTest = EnableCap.DepthTest,
        StencilTest = EnableCap.StencilTest,
        Texture2d = EnableCap.Texture2D,
        Blending = EnableCap.Blend,
        MultiSampling = EnableCap.Multisample,

        Rendering = 1,
        ClearBuffer,
        FrontFaceCulling,
        BackFaceCulling,
        ClockWiseFace,
        CounterClockWiseFace,
        Lighting,
    }

    public class GraphicsState
    {
        #region Fields and Properties

        public static readonly GraphicsState Default = new GraphicsState();

        private bool _enableRendering = true;
        private bool _enableClearBuffer = true;
        private bool _enableBackFaceCulling = true;
        private bool _enableFrontFaceCulling = false;
        private bool _enableBlending = true;
        private FrontFaceDirection _frontFaceDirection = FrontFaceDirection.Ccw;

        #endregion Fields and Properties

        #region Constructors

        public GraphicsState()
        {

        }

        public GraphicsState(GraphicsState state)
        {

        }

        #endregion Constructors

        public void Enable(EnableFlag enableFlag)
        {
            switch (enableFlag)
            {
                case EnableFlag.Rendering:
                    _enableRendering = true;
                    break;
                case EnableFlag.ClearBuffer:
                    _enableClearBuffer = true;
                    break;
                case EnableFlag.Blending:
                    _enableBlending = true;
                    GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                    GL.Enable((EnableCap)enableFlag);
                    break;
                case EnableFlag.BackFaceCulling:
                    _enableBackFaceCulling = true;
                    if (_enableFrontFaceCulling)
                    {
                        GL.CullFace(CullFaceMode.FrontAndBack);
                    }
                    else
                    {
                        GL.CullFace(CullFaceMode.Back);
                    }
                    GL.Enable(EnableCap.CullFace);
                    break;
                case EnableFlag.FrontFaceCulling:
                    _enableFrontFaceCulling = true;
                    if (_enableBackFaceCulling)
                    {
                        GL.CullFace(CullFaceMode.FrontAndBack);
                    }
                    else
                    {
                        GL.CullFace(CullFaceMode.Front);
                    }
                    GL.Enable(EnableCap.CullFace);
                    break;
                case EnableFlag.ClockWiseFace:
                    _frontFaceDirection = FrontFaceDirection.Cw;
                    GL.FrontFace(FrontFaceDirection.Cw);
                    break;
                case EnableFlag.CounterClockWiseFace:
                    _frontFaceDirection = FrontFaceDirection.Ccw;
                    GL.FrontFace(FrontFaceDirection.Ccw);
                    break;
                case EnableFlag.Lighting:
                    _enableLighting = true;
                    break;
                case EnableFlag.DepthTest:
                case EnableFlag.StencilTest:
                case EnableFlag.Texture2d:
                case EnableFlag.MultiSampling:
                    GL.Enable((EnableCap)enableFlag);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public bool IsEnabled(EnableFlag enableFlag)
        {
            switch (enableFlag)
            {
                case EnableFlag.Rendering:
                    return _enableRendering;
                case EnableFlag.ClearBuffer:
                    return _enableClearBuffer;
                case EnableFlag.BackFaceCulling:
                    return _enableBackFaceCulling;
                case EnableFlag.FrontFaceCulling:
                    return _enableFrontFaceCulling;
                default:
                    return GL.IsEnabled((EnableCap)enableFlag);
            }
        }

        public void Disable(EnableFlag enableFlag)
        {
            switch (enableFlag)
            {
                case EnableFlag.Rendering:
                    _enableRendering = false;
                    break;
                case EnableFlag.ClearBuffer:
                    _enableClearBuffer = false;
                    break;
                case EnableFlag.Blending:
                    _enableBlending = false;
                    GL.Disable((EnableCap)enableFlag);
                    break;
                case EnableFlag.BackFaceCulling:
                    _enableBackFaceCulling = false;
                    if (_enableFrontFaceCulling)
                    {
                        GL.CullFace(CullFaceMode.Front);
                    }
                    else
                    {
                        GL.Disable(EnableCap.CullFace);
                    }
                    break;
                case EnableFlag.FrontFaceCulling:
                    _enableFrontFaceCulling = false;
                    if (_enableBackFaceCulling)
                    {
                        GL.CullFace(CullFaceMode.Back);
                    }
                    else
                    {
                        GL.Disable(EnableCap.CullFace);
                    }
                    break;
                case EnableFlag.ClockWiseFace:
                    GL.FrontFace(FrontFaceDirection.Ccw);
                    break;
                case EnableFlag.CounterClockWiseFace:
                    GL.FrontFace(FrontFaceDirection.Cw);
                    break;
                case EnableFlag.Lighting:
                    _enableLighting = false;
                    break;
                default:
                    GL.Disable((EnableCap)enableFlag);
                    break;
            }
        }
    }
}
