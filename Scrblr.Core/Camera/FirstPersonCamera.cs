using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.IO;

namespace Scrblr.Core
{
    public class FirstPersonCamera : AbstractCamera
    {
        // x-axis rotation
        private float _pitch;

        // y-axis rotation
        private float _yaw = -MathHelper.PiOver2;
        
        private Vector2? _lastMousePosition = null;

        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(_pitch);
            set
            {
                var angle = MathHelper.Clamp(value, -89f, 89f);

                _pitch = MathHelper.DegreesToRadians(angle);

                UpdateVectors();
            }
        }

        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(_yaw);
            set
            {
                _yaw = MathHelper.DegreesToRadians(value);

                UpdateVectors();
            }
        }

        public float MouseMoveSensitivity = 0.2f;
        public float MoveSpeed = 2.5f;
        public float ScrollSpeed = 12f;

        public override void Update(FrameEventArgs a)
        {
            var ElapsedTime = a.Time;

            var input = KeyboardState;


            if (input.IsKeyDown(Keys.W))
            {
                Position += LookVector * MoveSpeed * (float)ElapsedTime; // Forward
            }

            if (input.IsKeyDown(Keys.S))
            {
                Position -= LookVector * MoveSpeed * (float)ElapsedTime; // Backwards
            }
            if (input.IsKeyDown(Keys.A))
            {
                Position -= RightVector * MoveSpeed * (float)ElapsedTime; // Left
            }
            if (input.IsKeyDown(Keys.D))
            {
                Position += RightVector * MoveSpeed * (float)ElapsedTime; // Right
            }
            if (input.IsKeyDown(Keys.Space))
            {
                Position += UpVector * MoveSpeed * (float)ElapsedTime; // Up
            }
            if (input.IsKeyDown(Keys.LeftShift))
            {
                Position -= UpVector * MoveSpeed * (float)ElapsedTime; // Down
            }

            // Get the mouse state
            var mouse = MouseState;

            if (_lastMousePosition == null) // This bool variable is initially set to true.
            {
                _lastMousePosition = new Vector2(mouse.X, mouse.Y);
            }
            else
            {
                // Calculate the offset of the mouse position
                var deltaX = mouse.X - _lastMousePosition.Value.X;
                var deltaY = mouse.Y - _lastMousePosition.Value.Y;

                _lastMousePosition = new Vector2(mouse.X, mouse.Y);

                // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                Yaw += deltaX * MouseMoveSensitivity;
                Pitch -= deltaY * MouseMoveSensitivity; // Reversed since y-coordinates range from bottom to top
            }
        }

        public override void MouseWheel(MouseWheelEventArgs a)
        {
            Position += a.Offset.Y * ScrollSpeed * LookVector * MoveSpeed * (float)ElapsedTime; // forward/backwards
        }

        private void UpdateVectors()
        {
            LookVector.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
            LookVector.Y = MathF.Sin(_pitch);
            LookVector.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);

            LookVector = Vector3.Normalize(LookVector);

            RightVector = Vector3.Normalize(Vector3.Cross(LookVector, Vector3.UnitY));
            UpVector = Vector3.Normalize(Vector3.Cross(RightVector, LookVector));
        }
    }
}