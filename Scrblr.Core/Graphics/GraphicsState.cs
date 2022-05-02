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

        public void SetState()
        {
            SetState(this);
        }

        public static void SetState(GraphicsState state)
        {
            if (state.IsEnabled(EnableFlag.DepthTest))
            {
                GL.Enable(EnableCap.DepthTest);
            }
            else
            {
                GL.Disable(EnableCap.DepthTest);
            }

            if (state.IsEnabled(EnableFlag.StencilTest))
            {
                GL.Enable(EnableCap.StencilTest);
            }
            else
            {
                GL.Disable(EnableCap.StencilTest);
            }

            if (state.IsEnabled(EnableFlag.Texturing))
            {
                GL.Enable(EnableCap.Texture2D);
            }
            else
            {
                GL.Disable(EnableCap.Texture2D);
            }

            if (state.IsEnabled(EnableFlag.Blending))
            {
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                GL.Enable(EnableCap.Blend);
            }
            else
            {
                GL.Disable(EnableCap.Blend);
            }

            if (state.IsEnabled(EnableFlag.MultiSampling))
            {
                GL.Enable(EnableCap.Multisample);
            }
            else
            {
                GL.Disable(EnableCap.Multisample);
            }

            if (state.IsEnabled(EnableFlag.FrontFaceCulling) && state.IsEnabled(EnableFlag.BackFaceCulling))
            {
                GL.CullFace(CullFaceMode.FrontAndBack);
                GL.Enable(EnableCap.CullFace);
            }
            else if (state.IsEnabled(EnableFlag.FrontFaceCulling))
            {
                GL.CullFace(CullFaceMode.Front);
                GL.Enable(EnableCap.CullFace);
            }
            else if (state.IsEnabled(EnableFlag.BackFaceCulling))
            {
                GL.CullFace(CullFaceMode.Back);
                GL.Enable(EnableCap.CullFace);
            }
            else
            {
                GL.Disable(EnableCap.CullFace);
            }

            if (state.IsEnabled(EnableFlag.ClockWiseFace))
            {
                GL.FrontFace(FrontFaceDirection.Cw);
            }
            else
            {
                GL.FrontFace(FrontFaceDirection.Ccw);
            }
        }

        public void Enable(EnableFlag enableFlag)
        {
            _enableFlagBitArray[(int)enableFlag] = true;

            SetState(this);
        }

        public bool IsEnabled(EnableFlag enableFlag)
        {
            return _enableFlagBitArray[(int)enableFlag];
        }

        public void Disable(EnableFlag enableFlag)
        {
            _enableFlagBitArray[(int)enableFlag] = false;

            SetState(this);
        }

        public void Toggle(EnableFlag enableFlag)
        {
            _enableFlagBitArray[(int)enableFlag] = !_enableFlagBitArray[(int)enableFlag];

            SetState(this);
        }
    }
}
