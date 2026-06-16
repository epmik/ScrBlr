using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using static OpenGlTutorialOrg.Tutorial14WindowExtented06;

namespace Skrbl
{
    internal static class Gdi32
    {
        // Source - https://stackoverflow.com/a/10057267
        // Posted by Abel, modified by community. See post 'Timeline' for change history
        // Retrieved 2026-01-29, License - CC BY-SA 3.0

        // the declarations
        public struct FIXED
        {
            public short fract;
            public short value;
        }

        public struct MAT2
        {
            [MarshalAs(UnmanagedType.Struct)] public FIXED eM11;
            [MarshalAs(UnmanagedType.Struct)] public FIXED eM12;
            [MarshalAs(UnmanagedType.Struct)] public FIXED eM21;
            [MarshalAs(UnmanagedType.Struct)] public FIXED eM22;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINTFX
        {
            [MarshalAs(UnmanagedType.Struct)] public FIXED x;
            [MarshalAs(UnmanagedType.Struct)] public FIXED y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GLYPHMETRICS
        {

            public int gmBlackBoxX;
            public int gmBlackBoxY;
            [MarshalAs(UnmanagedType.Struct)] public POINT gmptGlyphOrigin;
            [MarshalAs(UnmanagedType.Struct)] public POINTFX gmptfxGlyphOrigin;
            public short gmCellIncX;
            public short gmCellIncY;

        }

        public const int GGO_METRICS = 0;
        public const uint GDI_ERROR = 0xFFFFFFFF;

        [DllImport("gdi32.dll")]
        public static extern uint GetGlyphOutline(IntPtr hdc, uint uChar, uint uFormat,
           out GLYPHMETRICS lpgm, uint cbBuffer, IntPtr lpvBuffer, ref MAT2 lpmat2);

        [DllImport("gdi32.dll", ExactSpelling = true, PreserveSig = true, SetLastError = true)]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        // Source - https://stackoverflow.com/a/10057267
        // Posted by Abel, modified by community. See post 'Timeline' for change history
        // Retrieved 2026-01-29, License - CC BY-SA 3.0

        // if you want exact metrics, use a high font size and divide the result
        // otherwise, the resulting rectangle is rounded to nearest int
        [SupportedOSPlatform("windows")]
        public static int GetGlyphHeight(char letter, System.Drawing.Font font)
        {
            GLYPHMETRICS metrics;

            // identity matrix, required
            MAT2 matrix = new MAT2
            {
                eM11 = { value = 1 },
                eM12 = { value = 0 },
                eM21 = { value = 0 },
                eM22 = { value = 1 }
            };

            // HDC needed, we use a bitmap
            using (Bitmap b = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(b))
                {
                    IntPtr hdc = g.GetHdc();
                    IntPtr prev = Gdi32.SelectObject(hdc, font.ToHfont());
                    uint retVal = Gdi32.GetGlyphOutline(
                         /* handle to DC   */ hdc,
                         /* the char/glyph */ letter,
                         /* format param   */ Gdi32.GGO_METRICS,
                         /* glyph-metrics  */ out metrics,
                         /* buffer, ignore */ 0,
                         /* buffer, ignore */ IntPtr.Zero,
                         /* trans-matrix   */ ref matrix);

                    if (retVal == GDI_ERROR)
                    {
                        // something went wrong. Raise your own error here, 
                        // or just silently ignore
                        return 0;
                    }

                    // return the height of the smallest rectangle containing the glyph
                    return metrics.gmBlackBoxY;
                }
            }
        }

        // Source - https://stackoverflow.com/a/10057267
        // Posted by Abel, modified by community. See post 'Timeline' for change history
        // Retrieved 2026-01-29, License - CC BY-SA 3.0

        // if you want exact metrics, use a high font size and divide the result
        // otherwise, the resulting rectangle is rounded to nearest int
        [SupportedOSPlatform("windows")]
        public static int GetGlyphWidth(char letter, System.Drawing.Font font)
        {
            GLYPHMETRICS metrics;

            // identity matrix, required
            MAT2 matrix = new MAT2
            {
                eM11 = { value = 1 },
                eM12 = { value = 0 },
                eM21 = { value = 0 },
                eM22 = { value = 1 }
            };

            // HDC needed, we use a bitmap
            using (Bitmap b = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(b))
                {
                    IntPtr hdc = g.GetHdc();
                    IntPtr prev = Gdi32.SelectObject(hdc, font.ToHfont());
                    uint retVal = Gdi32.GetGlyphOutline(
                         /* handle to DC   */ hdc,
                         /* the char/glyph */ letter,
                         /* format param   */ Gdi32.GGO_METRICS,
                         /* glyph-metrics  */ out metrics,
                         /* buffer, ignore */ 0,
                         /* buffer, ignore */ IntPtr.Zero,
                         /* trans-matrix   */ ref matrix);

                    if (retVal == GDI_ERROR)
                    {
                        // something went wrong. Raise your own error here, 
                        // or just silently ignore
                        return 0;
                    }

                    // return the width of the smallest rectangle containing the glyph
                    return metrics.gmBlackBoxX;
                }
            }
        }

    }
}
