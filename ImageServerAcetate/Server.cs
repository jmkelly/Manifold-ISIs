using Manifold.ImageServer.OpenStreetMaps;

namespace Manifold.ImageServer.Acetate
{
   
    
        public class AcetateHillshade : ServerOpenStreetMaps 
        {
            public AcetateHillshade()
                : base(18)
            {
                Name = "GeoIQ Hillshade Acetate Images";
                DefaultURL = "http://acetate.geoiq.com/tiles/hillshading";
                ScaleNames = "0.5 m,1 m,2 m,5 m,10 m,20 m,40 m,80 m,160 m,320 m,640 m,1.3 km,2.5 km,5 km,10 km,20 km,40 km,80 km";
            }
        }
    
}
