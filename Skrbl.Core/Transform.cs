using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;

namespace Skrbl
{
    public class Transform
    {
        public Vector3 Position = Vector3.Zero;
        public Vector3 Rotation = Vector3.Zero;
        public Vector3 Scale = Vector3.Zero;

        public Matrix4 Matrix()
        {
            return Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(Quaternion()) * Matrix4.CreateTranslation(Position);
            //return Matrix4.CreateScale(_scale) * Matrix4.CreateRotationX(_rotation.X) * Matrix4.CreateRotationY(_rotation.Y) * Matrix4.CreateRotationZ(_rotation.Z) * Matrix4.CreateTranslation(_position);
        }

        public OpenTK.Mathematics.Quaternion Quaternion()
        {
            return OpenTK.Mathematics.Quaternion.FromEulerAngles(Rotation.X, Rotation.Y, Rotation.Z);
        }
    }
}
