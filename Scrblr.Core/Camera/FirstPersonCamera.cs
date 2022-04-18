using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

namespace Scrblr.Core
{
    public class FirstPersonCamera : AbstractCamera
    {
        // Rotation around the X axis (radians)
        private float _pitch;

        // Rotation around the Y axis (radians)
        private float _yaw = -MathHelper.PiOver2;

        public FirstPersonCamera(Vector3 position, float width, float height)
        {
            Position = position;
            AspectRatio = width / height;
        }

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

        public override Matrix4 ProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Fov), AspectRatio, 0.01f, 100f);
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