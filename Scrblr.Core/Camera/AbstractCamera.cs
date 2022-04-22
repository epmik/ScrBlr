using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
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
        /// Must be specified in angles, default == 1f
        /// </summary>
        public float Near { get; set; } = 1f;

        /// <summary>
        /// Must be specified in angles, default == 1000f
        /// </summary>
        public float Far { get; set; } = 1000f;

        private float _left = -1f;

        /// <summary>
        /// Must be specified in angles, default == -1f
        /// </summary>
        public float Left
        {
            get
            {
                return _left;
            }
            set
            {
                _left = value;
                _aspectRatio = Width / value;
            }
        }

        private float _right = 1f;

        /// <summary>
        /// Must be specified in angles, default == 1f
        /// </summary>
        public float Right
        {
            get
            {
                return _right;
            }
            set
            {
                _right = value;
                _aspectRatio = Width / value;
            }
        }

        private float _top = -1;

        /// <summary>
        /// Must be specified in angles, default == 1f
        /// </summary>
        public float Top
        {
            get
            {
                return _top;
            }
            set
            {
                _top = value;
                _aspectRatio = Width / value;
            }
        }

        private float _bottom = -1f;

        /// <summary>
        /// Must be specified in angles, default == -1f
        /// </summary>
        public float Bottom 
        { 
            get 
            { 
                return _bottom; 
            }
            set {
                _bottom = value;
                _aspectRatio = Width / value;
            } 
        }

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
                _aspectRatio = value / Height;
            }
        }

        /// <summary>
        /// Changing Height also changes the Top and Bottom properties
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
                _aspectRatio = Width / value;
            }
        }

        private float _aspectRatio;

        /// <summary>
        /// The horizontal aspect ratio between Width and Height
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

        /// <summary>
        /// default Math.PI / 2 or 90 degrees
        /// </summary>
        private float _fov = (float)(Math.PI / 2);

        /// <summary>
        /// Field of view in degrees, default == 90
        /// <para>
        /// Changing the field of view also changes the Left/Right/Width and Top/Bottom/Height properties
        /// </para>
        /// </summary>
        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                _fov = MathHelper.DegreesToRadians(MathHelper.Clamp(value, 1f, 120f));

                Top = Near * MathF.Tan(0.5f * _fov);
                Bottom = -Top;
                Left = Bottom * AspectRatio;
                Right = Top * AspectRatio;

                // see https://stackoverflow.com/a/55009832/527843
                DepthRatio = (float)(Math.Atan(_fov / 2.0) * 2.0);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float DepthRatio { get; set; }

        public double ElapsedTime { get; set; }

        public KeyboardState KeyboardState { get; set; }

        public MouseState MouseState { get; set; }

        //public event Action LoadAction;
        //public event Action UnLoadAction;
        //public event Action<FrameEventArgs> UpdateAction;
        //public event Action<ResizeEventArgs> ResizeAction;
        //public event Action<MouseWheelEventArgs> MouseWheelAction;
        //public event Action<MouseButtonEventArgs> MouseUpAction;
        //public event Action<MouseButtonEventArgs> MouseDownAction;
        //public event Action<MouseMoveEventArgs> MouseMoveAction;
        //public event Action MouseLeaveAction;
        //public event Action MouseEnterAction;
        //public event Action<KeyboardKeyEventArgs> KeyDownAction;
        //public event Action<KeyboardKeyEventArgs> KeyUpAction;

        public AbstractCamera()
        {
        }

        public virtual void Resize(ResizeEventArgs a)
        {
            AspectRatio = (float)a.Size.X / (float)a.Size.Y;

            //ResizeAction?.Invoke(a);
        }

        public virtual void Update(FrameEventArgs a)
        {
            //UpdateAction?.Invoke(a);
        }

        public virtual void KeyDown(KeyboardKeyEventArgs a)
        {
            //KeyDownAction?.Invoke(a);
        }

        public virtual void KeyUp(KeyboardKeyEventArgs a)
        {
            //KeyUpAction?.Invoke(a);
        }

        public virtual void MouseEnter()
        {
            //MouseEnterAction?.Invoke();
        }

        public virtual void MouseLeave()
        {
            //MouseLeaveAction?.Invoke();
        }

        public virtual void MouseMove(MouseMoveEventArgs a)
        {
            //MouseMoveAction?.Invoke(a);
        }

        public virtual void MouseUp(MouseButtonEventArgs a)
        {
            //MouseUpAction?.Invoke(a);
        }

        public virtual void MouseDown(MouseButtonEventArgs a)
        {
            //MouseDownAction?.Invoke(a);
        }

        public virtual void MouseWheel(MouseWheelEventArgs a)
        {
            //MouseWheelAction?.Invoke(a);
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
        }
    }
}