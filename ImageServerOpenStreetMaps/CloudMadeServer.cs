using System;

namespace Manifold.ImageServer.OpenStreetMaps
{
    public class CloudMadeServer : Server
    {
        public CloudMadeServer()
            : base(18)
        {
            //http://developers.cloudmade.com/projects/web-maps-lite/examples
            Name = "CloudMade Maps";
            DefaultURL = "http://tile.cloudmade.com/BC9A493B41014CAABB98F0471D759707/1";
            ScaleNames = "0.5 m,1 m,2 m,5 m,10 m,20 m,40 m,80 m,160 m,320 m,640 m,1.3 km,2.5 km,5 km,10 km,20 km,40 km,80 km";
        }

        protected override string GetTileUrl(int x, int y, int scale)
        {
            // adds tile size
            return String.Format("/256/{0:d}/{1:d}/{2:d}.png", scale, x, y);
        }
    }
}