using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

namespace Scrblr.Core
{
    public static class Camera
    {

        public static TCamera CreateFromWidthHeight<TCamera>(float width, float height, float near, float far) where TCamera : AbstractCamera, new()
        {
            var camera = new TCamera();

            return camera;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fov">field of view in degrees, between 1 and 120</param>
        /// <param name="near">distance to the near plane, must be a positive number</param>
        /// <param name="far">distance to the far plane, must be a positive number</param>
        public static TCamera CreateFromFieldOfView<TCamera>(float fov, float near, float far) where TCamera : AbstractCamera, new()
        {
            var camera = new TCamera();

            return camera;
        }
    }
}