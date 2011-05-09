using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manifold.ImageServer;
using Manifold.ImageServer.OSM;


namespace Manifold.ImageServer.Acetate
{
    public class ServerAcetateHillshade:ServerOSM
    {
         public ServerAcetateHillshade()
            : base()
        {
            //http://acetate.geoiq.com/tiles/hillshading/{Z}/{X}/{Y}.png
            String strDefaultUrl = "http://acetate.geoiq.com/tiles/hillshading/";
            BaseDefaultURL = strDefaultUrl;
            URL = strDefaultUrl;
            BaseName = "Acetate Hillshade";
            BaseDefaultImageType = ".png";


        }
    }
}
