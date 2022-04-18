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
    public class GraphicsState
    {
        #region Fields and Properties

        public static readonly GraphicsState Default = new GraphicsState();

        private bool _enableRendering = true;
        private bool _enableClearBuffer = true;
        private bool _enableBackFaceCulling = true;
        private bool _enableFrontFaceCulling = false;

        #endregion Fields and Properties

        #region Constructors

        public GraphicsState()
        {

        }

        #endregion Constructors

    }
}
