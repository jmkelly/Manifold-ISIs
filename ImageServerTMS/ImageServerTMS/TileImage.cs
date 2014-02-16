using System.Drawing;
using System.Drawing.Imaging;


namespace Manifold.ImageServer.TMS
{
    public class TileImage
    {
        public readonly Bitmap Pic;
        public TileImage(int x, int y, int scale, string filename)
        {
            X = x;
            Y = y;
            Scale  = scale;
            FileName  = filename;

            const int width = 256;
            const int height = 256;
            RectangleF rectF = new RectangleF(0, 0, width, height);
            Pic = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(Pic);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            Color fontColor = Color.Black;
            Color rectColor = Color.White;
            SolidBrush fgBrush = new SolidBrush(fontColor);
            SolidBrush bgBrush = new SolidBrush(rectColor);
            g.FillRectangle(bgBrush, rectF);
            Pen pen = new Pen(Color.Blue);
            g.DrawRectangle(pen, 0, 0, width - 1, height - 1);
            string text = string.Format("xyz={0},{1},{2}",  x, y, scale); 
            FontFamily fontFamily = new FontFamily("Arial");
            Font font = new Font(fontFamily,18,FontStyle.Regular,GraphicsUnit.Pixel);
            g.DrawString(text, font, fgBrush, rectF);
        }

        public void Save()
        {
            if (!string.IsNullOrEmpty(FileName))
                Pic.Save(FileName, ImageFormat.Png);
            
        }

        public int X { get; private set; }
        public int Y { get; private set; }
        public int Scale { get; private set; }
        public string FileName { get; private set; }
         
    }
}
