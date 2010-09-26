using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;


namespace Manifold.ImageServer.TMS
{
    public class TileImage
    {



        public TileImage(int _x, int _y, int _scale, string _filename)
        {
            X = _x;
            Y = _y;
            Scale  = _scale;
            FileName  = _filename;

            Int32 Width = 256;
            Int32 Height = 256;
            RectangleF RectF = new RectangleF(0, 0, Width, Height);
            Bitmap Pic = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(Pic);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            Color fontColor = Color.Black;
            Color rectColor = Color.White;
            SolidBrush fgBrush = new SolidBrush(fontColor);
            SolidBrush bgBrush = new SolidBrush(rectColor);
            g.FillRectangle(bgBrush, RectF);
            Pen pen = new Pen(Color.Blue);
            g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            string text = "xyz=" + _x.ToString() + "," + _y.ToString() + "," + _scale.ToString(); 
            FontFamily fontFamily = new FontFamily("Arial");
            Font font = new Font(fontFamily,18,FontStyle.Regular,GraphicsUnit.Pixel);
            g.DrawString(text, font, fgBrush, RectF);
            
            if (_filename != null && _filename.Length != 0)
                Pic.Save(_filename, ImageFormat.Png);



            
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int Scale { get; set; }
        public string FileName { get; set; }
         
    }
}
