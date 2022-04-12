using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

namespace Scrblr.Core
{
    public class Rectangle
    {
        private Vector2 _position = new Vector2(0, 0);
        private Vector2 _size = new Vector2(1, 1);
        private Vector2 _translate = new Vector2(0, 0);
        private Color4 _fill = Color4.Black;

        public Rectangle Position(float x , float y)
        {
            _position.X = x;
            _position.Y = y;

            return this;
        }

        public Rectangle Size(float x, float y)
        {
            _size.X = x;
            _size.Y = y;

            return this;
        }

        public Rectangle Translate(float x, float y)
        {
            _translate.X = x;
            _translate.Y = y;

            return this;
        }

        public Rectangle Fill(int r, int g, int b, int a = 255)
        {
            return Fill(Utility.ToUnitSingle(r), Utility.ToUnitSingle(g), Utility.ToUnitSingle(b), Utility.ToUnitSingle(a));
        }

        public Rectangle Fill(float r, float g, float b, float a = 1.0f)
        {
            _fill.R = r;
            _fill.G = g;
            _fill.B = b;
            _fill.A = a;

            return this;
        }
    }
}