using System;

namespace Manifold.ImageServer.TMS
{
    public class PointD
    {
        public PointD(Double x , Double y){
            X = x;
             Y = y;
        }
        public PointD()
        {
        }


        public Double X { get; set; }
        public Double Y { get; set; }

    }

    public class Point
    {
        public Point(Int32 x, Int32 y)
        {
            X = x;
            Y = y;
        }

        public Point() { }

        public Int32 X { get; set; }
        public Int32 Y { get; set; }
    }

}
