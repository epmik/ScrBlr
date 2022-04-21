using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections;
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
        Rendering = 0,
        DepthTest,
        StencilTest,
        Texturing,
        Blending,
        MultiSampling,
        ClearBuffers,
        FrontFaceCulling,
        BackFaceCulling,
        ClockWiseFace,
        Lighting,
    }

    public class GraphicsState
    {
        #region Fields and Properties

        private static readonly GraphicsState _default;
        private static readonly EnableFlag[] _enableFlagArray;
        private static readonly int _bitArraySize = 0;

        static GraphicsState()
        {
            _enableFlagArray = (EnableFlag[])Enum.GetValues(typeof(EnableFlag));
            _bitArraySize = _enableFlagArray.Length;

            _default = new GraphicsState();

            _default.Enable(EnableFlag.Rendering);
            _default.Enable(EnableFlag.DepthTest);
            _default.Enable(EnableFlag.Texturing);
            _default.Enable(EnableFlag.Blending);
            _default.Enable(EnableFlag.MultiSampling);
            _default.Enable(EnableFlag.ClearBuffers);
            _default.Enable(EnableFlag.BackFaceCulling);
            _default.Disable(EnableFlag.ClockWiseFace);
        }

        private BitArray _enableFlagBitArray;

        #endregion Fields and Properties

        #region Constructors

        private GraphicsState()
        {
            _enableFlagBitArray = new BitArray(_bitArraySize);
        }

        public GraphicsState(GraphicsState state)
        {
            _enableFlagBitArray = new BitArray(state._enableFlagBitArray);
        }

        #endregion Constructors

        public static GraphicsState DefaultState()
        {
            return new GraphicsState(_default);
        }

        public static GraphicsState EmptyState()
        {
            return new GraphicsState();
        }

        public void SetState()
        {
            foreach(var enableFlag in _enableFlagArray)
            {
                SetState(enableFlag);
            }
        }

        private void SetState(EnableFlag enableFlag)
        {
            switch (enableFlag)
            {
                case EnableFlag.Rendering:
                    break;
                case EnableFlag.DepthTest:
                    if (_enableFlagBitArray[(int)EnableFlag.DepthTest])
                    {
                        GL.Enable(EnableCap.DepthTest);
                    }
                    else
                    {
                        GL.Disable(EnableCap.DepthTest);
                    }
                    break;
                case EnableFlag.StencilTest:
                    if (_enableFlagBitArray[(int)EnableFlag.StencilTest])
                    {
                        GL.Enable(EnableCap.StencilTest);
                    }
                    else
                    {
                        GL.Disable(EnableCap.StencilTest);
                    }
                    break;
                case EnableFlag.Texturing:
                    if (_enableFlagBitArray[(int)EnableFlag.Texturing])
                    {
                        GL.Enable(EnableCap.Texture2D);
                    }
                    else
                    {
                        GL.Disable(EnableCap.Texture2D);
                    }
                    break;
                case EnableFlag.Blending:
                    if (_enableFlagBitArray[(int)EnableFlag.Blending])
                    {
                        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                        GL.Enable(EnableCap.Blend);
                    }
                    else
                    {
                        GL.Disable(EnableCap.Blend);
                    }
                    break;
                case EnableFlag.MultiSampling:
                    if (_enableFlagBitArray[(int)EnableFlag.MultiSampling])
                    {
                        GL.Enable(EnableCap.Multisample);
                    }
                    else
                    {
                        GL.Disable(EnableCap.Multisample);
                    }
                    break;
                case EnableFlag.ClearBuffers:
                    break;
                case EnableFlag.FrontFaceCulling:
                case EnableFlag.BackFaceCulling:
                    if (_enableFlagBitArray[(int)EnableFlag.FrontFaceCulling] && _enableFlagBitArray[(int)EnableFlag.BackFaceCulling])
                    {
                        GL.CullFace(CullFaceMode.FrontAndBack);
                        GL.Enable(EnableCap.CullFace);
                    }
                    else if (_enableFlagBitArray[(int)EnableFlag.FrontFaceCulling])
                    {
                        GL.CullFace(CullFaceMode.Front);
                        GL.Enable(EnableCap.CullFace);
                    }
                    else if (_enableFlagBitArray[(int)EnableFlag.BackFaceCulling])
                    {
                        GL.CullFace(CullFaceMode.Back);
                        GL.Enable(EnableCap.CullFace);
                    }
                    else
                    {
                        GL.Disable(EnableCap.CullFace);
                    }
                    break;
                case EnableFlag.ClockWiseFace:
                    if (_enableFlagBitArray[(int)EnableFlag.ClockWiseFace])
                    {
                        GL.FrontFace(FrontFaceDirection.Cw);
                    }
                    else
                    {
                        GL.FrontFace(FrontFaceDirection.Ccw);
                    }
                    break;
                case EnableFlag.Lighting:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public void Enable(EnableFlag enableFlag)
        {
            _enableFlagBitArray[(int)enableFlag] = true;

            SetState(enableFlag);
        }

        public bool IsEnabled(EnableFlag enableFlag)
        {
            return _enableFlagBitArray[(int)enableFlag];
        }

        public void Disable(EnableFlag enableFlag)
        {
            _enableFlagBitArray[(int)enableFlag] = false;

            SetState(enableFlag);
        }
    }
}
