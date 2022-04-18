using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace Scrblr.Core
{
    public abstract class AbstractCamera : ICamera
    {
        public ProjectionMode ProjectionMode { get; set; } = ProjectionMode.Perspective;

        /// <summary>
        /// default == Vector3(0, 0, 0)
        /// </summary>
        public Vector3 Position = new Vector3(0, 0, 0);

        /// <summary>
        /// default look direction is along the negative z-axis Vector3(0, 0, -1)
        /// </summary>
        public Vector3 LookVector = new Vector3(0, 0, -1);

        /// <summary>
        /// default forward direction is along the negative z-axis Vector3(0, 0, -1)
        /// </summary>
        public Vector3 ForwardVector = new Vector3(0, 0, -1);

        /// <summary>
        /// default up direction is along the positive y-axis Vector3(0, 1, 0)
        /// </summary>
        public Vector3 UpVector = new Vector3(0, 1, 0);

        /// <summary>
        /// default right direction is along the positive x-axis Vector3(1, 0, 0)
        /// </summary>
        public Vector3 RightVector = new Vector3(1, 0, 0);

        /// <summary>
        /// default Math.PI / 2 or 90 degrees
        /// </summary>
        private float _fov = (float)(Math.PI / 2);

        /// <summary>
        /// Field of view in degrees, default == 90
        /// </summary>
        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                _fov = MathHelper.DegreesToRadians(MathHelper.Clamp(value, 1f, 120f));

                // see https://stackoverflow.com/a/55009832/527843
                DepthRatio = (float)(Math.Atan(_fov / 2.0) * 2.0);
            }
        }

        /// <summary>
        /// Must be specified in degrees, default == 45f
        /// </summary>
        public float DepthRatio { get; set; }

        /// <summary>
        /// Must be specified in angles, default == 1f
        /// </summary>
        public float Near { get; set; } = 1f;

        /// <summary>
        /// Must be specified in angles, default == 1000f
        /// </summary>
        public float Far { get; set; } = 1000f;

        /// <summary>
        /// Must be specified in angles, default == -400f
        /// </summary>
        public float Left { get; set; } = -400f;

        /// <summary>
        /// Must be specified in angles, default == 400f
        /// </summary>
        public float Right { get; set; } = 400f;

        /// <summary>
        /// Must be specified in angles, default == 400f
        /// </summary>
        public float Top { get; set; } = 400f;

        /// <summary>
        /// Must be specified in angles, default == -400f
        /// </summary>
        public float Bottom { get; set; } = -400f;

        /// <summary>
        /// changing Width also changes the Left and Right properties
        /// default == 800
        /// </summary>
        public float Width
        {
            get
            {
                return Right - Left;
            }
            set
            {
                var h = value / 2;
                Left = -h;
                Right = h;
                //AspectRatio = Width / Height;
            }
        }

        /// <summary>
        /// changing Height also changes the Top and Bottom properties
        /// default == 800
        /// </summary>
        public float Height
        {
            get
            {
                return Top - Bottom;
            }
            set
            {
                var h = value / 2;
                Bottom = -h;
                Top = h;
            }
        }

        private float _aspectRatio;

        /// <summary>
        /// changing Height also changes the Top and Bottom properties
        /// default == 800
        /// </summary>
        public float AspectRatio
        {
            get
            {
                return _aspectRatio;
            }
            set
            {
                _aspectRatio = value;
                Width = Height * _aspectRatio;
            }
        }

        public AbstractCamera()
        {
        }

        public void Resize(ResizeEventArgs a)
        {
            AspectRatio = (float)a.Size.X / (float)a.Size.Y;
        }

        public virtual Matrix4 ProjectionMatrix()
        {
            switch(ProjectionMode)
            {
                case ProjectionMode.Perspective:
                    return Matrix4.CreatePerspectiveOffCenter(Left, Right, Bottom, Top, Near, Far);
                default:
                    return Matrix4.CreateOrthographicOffCenter(Left, Right, Bottom, Top, Near, Far);
            }
        }

        public virtual Matrix4 ViewMatrix()
        {
            return Matrix4.LookAt(
                Position, 
                Position + LookVector, 
                UpVector);

            //return Matrix4.LookAt(
            //    Position,
            //    Position + ForwardVector,
            //    UpVector);
        }
    }
}