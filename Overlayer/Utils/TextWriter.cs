using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;

namespace Overlayer.Utils
{
    public class TextWriter
    {
        private static readonly Font defFont = new Font(FontFamily.GenericSerif, 8);
        private Font font = defFont;

        /// <summary>
        /// If CustomBrush Is Not Null, This Will Be Ignored
        /// </summary>
        public Color Color = Color.White;
        public StringFormat Format = new StringFormat()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
        };
        /// <summary>
        /// If Null, Using Solid Brush With Color
        /// </summary>
        public Brush CustomBrush = null;

        public bool TrySetFont(string fileName, int emSize)
        {
            if (!File.Exists(fileName)) return false;
            using (var fonts = new PrivateFontCollection())
            {
                fonts.AddFontFile(fileName);
                font = new Font(fonts.Families[0], emSize);
            }
            return true;
        }
        public void Write(Image target, PointF pt, string text)
        {
            RectangleF rectf = new RectangleF(pt.X, pt.Y, target.Width - pt.X, target.Height - pt.Y);
            using (Graphics g = Graphics.FromImage(target))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.DrawString(text, font, CustomBrush ?? new SolidBrush(Color), rectf, Format);
                g.Flush();
            }
        }
        public void Write(Image target, float x, float y, string text) => Write(target, new PointF(x, y), text);
    }
}
