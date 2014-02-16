namespace Manifold.ImageServer.OpenStreetMaps
{
    public class CycleMapsServer : Server
    {
        public CycleMapsServer()
            : base(18)
        {
            Name = "Open Cycle Maps";
            DefaultURL = "http://a.tile.opencyclemap.org/cycle";
            ScaleNames = "0.5 m,1 m,2 m,5 m,10 m,20 m,40 m,80 m,160 m,320 m,640 m,1.3 km,2.5 km,5 km,10 km,20 km,40 km,80 km";
            
        }
    }
}