using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Scrblr.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Scrblr.Leaning
{
    [Sketch(Name = "Learn024-Text-Font-Creation-Rendering")]
    public class Learn024 : AbstractSketch2d20220413
    {
        public Learn024()
            : base(600, 600)
        {
            LoadAction += Load;
            UnLoadAction += UnLoad;
            RenderAction += Render;
            UpdateAction += Update;

            Samples = 8;
        }

        const string vertexShaderSource = @"
#version 330 core

in vec3 iPosition;  
in vec4 iColor;

out vec4 ioColor;

uniform mat4 uModelMatrix;
uniform mat4 uViewMatrix;
uniform mat4 uProjectionMatrix;

void main(void)
{
    gl_Position = vec4(iPosition, 1.0) * uModelMatrix * uViewMatrix * uProjectionMatrix;

	ioColor = iColor;
}";

        const string fragmentShaderSource = @"
#version 330 core

in vec4 ioColor;

out vec4 oColor;

void main()
{
    oColor = ioColor;
}";

        private Texture20220413 _fontTexture;

        public void Load()
        {
            Graphics.ClearColor(255, 255, 255);

            Graphics.CurrentShader = new Shader20220413(vertexShaderSource, fragmentShaderSource);

            QueryGraphicsCardCapabilities();

            //GenerateFontImage("Arial");
            //GenerateFontImage("Arial Black");
            //GenerateFontImage("Verdana");
            //GenerateFontImage("Courier New");
            //GenerateFontImage("MS Sans Serif");
            //GenerateFontImage("Times New Roman");
            //GenerateFontImage("Tahoma");
            //GenerateFontImage("Lucida Sans Unicode");
            //GenerateFontImage("Lucida Console");
            //GenerateFontImage("Impact");
            //GenerateFontImage("Georgia");
            //GenerateFontImage("Comic Sans MS");
            GenerateFontImage("Consolas");
            //GenerateFontImage("MS Reference Sans Serif");

            _fontTexture = new Texture20220413("resources/fonts/Consolas.png");

        }

        public static class Settings
        {
            //public static string FontBitmapFilename = "test.png";
            public static int GlyphsPerLine = 16;
            public static int GlyphLineCount = 6;
            public static int GlyphWidth = 11;
            //public static int CharXSpacing = 11;

            public static string Text = "GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);";

            // Used to offset rendering glyphs to bitmap
            //public static int AtlasOffsetX = -3, AtlassOffsetY = 0;
            public static int FontSize = 14;
            //public static bool BitmapFont = false;
            //public static string FromFile; //= "joystix monospace.ttf";
        }

        void GenerateFontImage(string fontName)
        {
            System.Drawing.Font font;
            //if (!String.IsNullOrWhiteSpace(Settings.FromFile))
            //{
            //    var collection = new PrivateFontCollection();
            //    collection.AddFontFile(Settings.FromFile);
            //    var fontFamily = new FontFamily(Path.GetFileNameWithoutExtension(Settings.FromFile), collection);
            //    font = new Font(fontFamily, Settings.FontSize);
            //}
            //else
            //{
            font = new System.Drawing.Font(new System.Drawing.FontFamily(fontName), Settings.FontSize);
            //}

            var glyphHeight = font.Height;

            int bitmapWidth = Settings.GlyphsPerLine * Settings.GlyphWidth;
            int bitmapHeight = Settings.GlyphLineCount * glyphHeight;

            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(bitmapWidth, bitmapHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {


                using (var g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    g.PageUnit = System.Drawing.GraphicsUnit.Pixel;

                    //if (Settings.BitmapFont)
                    //{
                    //    g.SmoothingMode = SmoothingMode.None;
                    //    g.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
                    //}
                    //else
                    //{
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    //g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    //g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                    //}

                    var c = 32;

                    for (int p = 0; p < Settings.GlyphLineCount; p++)
                    {
                        for (int n = 0; n < Settings.GlyphsPerLine; n++)
                        {
                            if(c > 127)
                            {
                                break;
                            }

                            g.DrawString(
                                ((char)c).ToString(), 
                                font,
                                System.Drawing.Brushes.Black, 
                                n * Settings.GlyphWidth, 
                                p * glyphHeight);

                            c++;
                        }
                    }
                }

                bitmap.Save($"resources/fonts/{fontName}.png");
            }
        }

        public void UnLoad()
        {

        }

        public void Render()
        {
            Graphics.ClearBuffers();

            //Graphics.Disable(EnableFlag.DepthTest);

            Graphics.PushMatrix();
            {
                Rotate(12);

                //Translate(0, 0);

                //Graphics.Fill(Color4.IndianRed);

                //Graphics.Rectangle(0, 0, 100, 100);

                {
                    Graphics.PushMatrix();

                    Translate(100, 0, -2);

                    Graphics.Fill(30);

                    Graphics.Quad(
                        50, 50,
                        50, -50,
                        -50, 50,
                        -50, -50
                        );

                    Graphics.PopMatrix();
                }

                {
                    Graphics.PushMatrix();

                    Translate(50, 0, -4);

                    Graphics.Fill(60);

                    Graphics.Quad(
                        -50, -50,
                        50, -50,
                        -50, 50,
                        50, 50);

                    Graphics.PopMatrix();
                }

                {
                    Graphics.PushMatrix();

                    Translate(0, 0, -6);

                    Graphics.Fill(90);

                    Graphics.Quad(-50, 50, 50, 50, -50, -50, 50, -50);

                    Graphics.PopMatrix();
                }

                {
                    Graphics.PushMatrix();

                    Translate(0, -50, -8);

                    Graphics.Fill(120);

                    Graphics.Quad(
                        50, -50,
                        -50, -50,
                        50, 50,
                        -50, 50);

                    Graphics.PopMatrix();
                }
            }
            Graphics.PopMatrix();
        }

        public void Update()
        {
        }
    }
}
