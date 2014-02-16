using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Manifold.ImageServer.TMS
{
    public class GlobalMercator
    {
        Int32 _tileSize;
        double _initialResolution;
        double _originShift;
        public GlobalMercator()
        {
            _tileSize = 256;
            _initialResolution = (2.0 * Math.PI * 6378137.0) / _tileSize;
            _originShift = 2 * Math.PI * 6378137 / 2.0;
        }

        public double InitialResolution { get { return _initialResolution; } }
        public double OriginShift { get { return _originShift; }  }
        public Int32 TileSize { get {return _tileSize;} }

        public Point GetBitmapCoord(PointD mercatorPoint, Int32 zoom)
        {
            Double res = Resolution(zoom);
            Int32 x = Convert.ToInt32((mercatorPoint.X + OriginShift) / res);
            Int32 y = Convert.ToInt32((mercatorPoint.Y + OriginShift) / res);
            Point p = new Point(x, y);
            return p;

        }

        public double Resolution(Int32 zoom)
        {
            return InitialResolution / (Math.Pow(2.0,zoom));
        }

        public PointD GetMercatorCoordinate(PointD LatitudeLongitudePoint)
        {
            double mx = LatitudeLongitudePoint.X * OriginShift / 180;
            double my = Math.Log(Math.Tan((90 + LatitudeLongitudePoint.Y) * Math.PI / 360)) / (Math.PI / 180);
            my = my * OriginShift / 180;
            PointD mercatorPoint = new PointD(mx, my);
            return mercatorPoint;
        }

        public Point GetPixels(PointD MercatorPoint, Int32 Zoom)
        {
            double res = Resolution(Zoom);
            Int32 px = (int)((MercatorPoint.X + OriginShift) / res);
            Int32 py = (int)((MercatorPoint.Y + OriginShift) / res);
            Point p = new Point(px, py);
            return p;
        }

        public Point GetTile(Point pixelPoint)
        {
            Int32 tx = (int)( Math.Ceiling( (double)pixelPoint.X  / (double)(TileSize) ) - 1 );
            Int32 ty = (int)( Math.Ceiling( (double)pixelPoint.Y  / (double)(TileSize) ) - 1 );
            Point tilePoint = new Point(tx,ty);
            return tilePoint;
        }

        public Point GetTile(PointD mercatorPoint, Int32 zoom)
        {
            Point pix = GetPixels(mercatorPoint, zoom);
            return GetTile(pix);
        }


    }
}
