using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;

namespace Skrbl
{
    public class Camera
    {
        public Vector3 Position = Vector3.Zero;
        public Vector3 Rotation = Vector3.Zero;

        public ProjectionType ProjectionType = ProjectionType.Perspective;

        public float AspectRatio;
        public float FovInRadians = MathHelper.DegreesToRadians(66.0f);
        public float Near = 0.01f;
        public float Far = 10000f;
        public float FrustumWidth;
        public float FrustumHeight;

        private Vector3? _target = Vector3.Zero;
        private Vector3? _direction = null;
        private Vector3 _up = Vector3.UnitY;

        public void Target(Vector3 target)
        {
            _target = target.Normalized();
            _direction = null;
            //CameraViewMatrix = Matrix4.LookAt(
            //    _cameraPosition,   // Camera is here
            //    _cameraPosition + direction,         // and looks here : at the same position, plus "direction"
            //    up         // Head is up (set to 0,-1,0 to look upside-down)
            //);
        }

        public void Direction(Vector3 direction)
        {
            _direction = direction.Normalized();
            _target = null;
            //CameraViewMatrix = Matrix4.LookAt(
            //    _cameraPosition,   // Camera is here
            //    _cameraPosition + direction,         // and looks here : at the same position, plus "direction"
            //    up         // Head is up (set to 0,-1,0 to look upside-down)
            //);
        }

        public Matrix4 ViewMatrix()
        {
            return _direction != null
                ? Matrix4.LookAt(
                    Position,
                    Position + _direction.Value,
                    _up)
                : Matrix4.LookAt(
                    Position,
                    _target.Value,
                    _up);
        }

        public Matrix4 ProjectionMatrix()
        {
            return ProjectionType == ProjectionType.Perspective
                ? Matrix4.CreatePerspectiveFieldOfView(FovInRadians, AspectRatio, Near, Far)
                : Matrix4.CreateOrthographic(FrustumWidth, FrustumHeight, Near, Far);
        }
    }
}
