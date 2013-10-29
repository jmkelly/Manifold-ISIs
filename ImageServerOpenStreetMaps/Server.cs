// For conditions of distribution and use, see AssemblyInfo.cs

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Net.Cache;

namespace Manifold.ImageServer.OpenStreetMaps
{
    public abstract class ServerOpenStreetMaps : IServer
    {
        private String _defaultUrl; 
        private String _url;
        private readonly String _imageType;
        private readonly String _coordinateSystemXml;
        private String _proxyAddress;
        private String _proxyPassword;
        private String _proxyUserName;
        private Int32 _scaleLo;
        private Int32 _scaleHi;
        private readonly Int32 _tileSizeX;
        private readonly Int32 _tileSizeY;

        private const Int32 EarthRadius = 6378137;

        protected ServerOpenStreetMaps(Int32 scaleHi)
        {
            Name = "";
            _defaultUrl = "";
            _url = _defaultUrl;
            ScaleNames = "";
            _imageType = ".png";
            _scaleLo = 1;
            _scaleHi = scaleHi;
            _tileSizeX = 256;
            _tileSizeY = 256;
            Error = "";

            // coordinate system scale and offset
            {
                Double imageSize = (1 << _scaleHi) * _tileSizeX;
                Double scale = (2 * EarthRadius * Math.PI) / imageSize;
                const double offset = EarthRadius * Math.PI;

                // save coordinate system in xml format
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;
                settings.Encoding = Encoding.UTF8;
                settings.Indent = true;

                StringWriter strWriter = new StringWriter(CultureInfo.InvariantCulture);
                XmlWriter writer = XmlWriter.Create(strWriter, settings);
                writer.WriteStartDocument();
                writer.WriteStartElement("data");
                writer.WriteStartElement("coordinateSystem");
                writer.WriteElementString("name", "Mercator");
                writer.WriteElementString("datum", "World Geodetic 1984 (WGS84) Auto");
                writer.WriteElementString("system", "Mercator");
                writer.WriteElementString("unit", "Meter");
                writer.WriteElementString("localOffsetX", (-offset).ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("localOffsetY", (offset).ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("localScaleX", scale.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("localScaleY", scale.ToString(CultureInfo.InvariantCulture));
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
                writer.Close();

                _coordinateSystemXml = strWriter.ToString();
                strWriter.Close();
            }
        }

        protected virtual String GetTileUrl(Int32 x, Int32 y, Int32 scale)
        {
            return String.Format("{0:s}/{1:d}/{2:d}/{3:d}.png", _url, scale, x, y);
        }

        // IServer implementation
        // Download tile data to file
        public bool DownloadTile(int x, int y, int scale, string filename)
        {
            scale = _scaleHi - scale + 1;
            String url = GetTileUrl(x, y, scale);
            const string tileInfo = "";
            try
            {
                // initialize request
                HttpWebRequest request = CreateHttpRequest(url, _proxyAddress, _proxyUserName, _proxyPassword);

                // obtain response
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                // process response
                String strType = response.ContentType;
                Int32 nIndex = strType.IndexOf("image", System.StringComparison.Ordinal);
                if (nIndex > -1)
                {
                    Image image = Image.FromStream(response.GetResponseStream());
                    if (!string.IsNullOrEmpty(filename))
                        image.Save(filename, ImageFormat.Png);
                }
                else
                {
                    // read all data
                    StreamReader objReader = new StreamReader(response.GetResponseStream());
                    String error = objReader.ReadToEnd();
                    objReader.Close();

                    if (error.Length == 0)
                        error = "Can't obtain image from server";

                    Error = tileInfo + error;

                    response.Close();
                    return false;
                }

                response.Close();
                return true;
            }
            catch (WebException we)
            {
                Error = tileInfo + we.Message;
                return false;
            }
            catch (Exception e)
            {
                Error = tileInfo + e.Message;
                throw;
            }
        }

        // Get the supplied area, in pixels
        public IRectangle GetRectPixels(Int32 scale, IRectangleD rect)
        {
            int d = (1 << (_scaleHi - scale + 1)) * _tileSizeY;
            double c = Math.Pow(2.0, scale - _scaleLo);
            //reverse y
            Rectangle rectPixels = new Rectangle((int)((rect.XMin) / c), Math.Max(0, Math.Min(d - 1, (int)((-rect.YMax) / c))), (int)((rect.XMax) / c), Math.Max(0, Math.Min(d - 1, (int)((-rect.YMin) / c))));
            return rectPixels;
        }

        // Get the supplied area, in tiles
        public IRectangle GetRectTiles(Int32 scale, IRectangleD rect)
        {
            IRectangle rectTiles = GetRectPixels(scale, rect);
            rectTiles.XMin = rectTiles.XMin / _tileSizeX;
            rectTiles.YMin = rectTiles.YMin / _tileSizeY;
            rectTiles.XMax = rectTiles.XMax / _tileSizeX;
            rectTiles.YMax = rectTiles.YMax / _tileSizeY;
            return rectTiles;
        }

        // Get coordinate system in XML format
        public string CoordinateSystem { get { return _coordinateSystemXml; } }
        // Get default image type (eg, ".png")
        public String DefaultImageType { get { return _imageType; } }
        // Get default URL
        public String DefaultURL 
        {
            get { return _defaultUrl; }
            protected set { _defaultUrl = value; _url = value; }
        }
        // Obtain last error
        public string Error { get; private set; }
        // Get name
        public string Name { get; protected set; }

        // Get or set proxy address
        public String ProxyAddress
        {
            get { return _proxyAddress; }
            set { _proxyAddress = value; }
        }
        // Get or set proxy password
        public String ProxyPassword
        {
            get { return _proxyPassword; }
            set { _proxyPassword = value; }
        }
        // Get or set proxy user name
        public String ProxyUserName
        {
            get { return _proxyUserName; }
            set { _proxyUserName = value; }
        }
        // Returns True if Y axis is reversed and False otherwise
        public Boolean ReverseY { get { return true; } }
        // Get least detailed scale
        public Int32 ScaleHi 
        { 
            get { return _scaleHi; }
            protected set { _scaleHi = value; }
        }
        // Get most detailed scale
        public Int32 ScaleLo 
        { 
            get { return _scaleLo; }
            protected set { _scaleLo = value; } 
        }
        // Get scale names separated by commas
        public string ScaleNames { get; protected set; }

        // Get tile size by X
        public Int32 TileSizeX { get { return _tileSizeX; } }
        // Get tile size by Y
        public Int32 TileSizeY { get { return _tileSizeY; } }
        // Get or set URL
        public String URL
        {
            get { return _url; }
            set { _url = value; }
        }

        // Create HTTP request
        private static HttpWebRequest CreateHttpRequest(String _url, String _strProxyAddress, String _strProxyUsername, String _strProxyPassword)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_url);
            RequestCachePolicy policy = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable);
            request.CachePolicy = policy;

            // set user name and password
            String strUserInfo = request.RequestUri.UserInfo;
            if (strUserInfo.Length != 0)
            {
                // send credentials with the request
                request.PreAuthenticate = true;

                String strUsername = "";
                String strPassword = "";

                // try to find password token
                int index = strUserInfo.IndexOf(":", System.StringComparison.Ordinal);
                if (index > -1)
                {
                    strUsername = strUserInfo.Substring(0, index);
                    strPassword = strUserInfo.Substring(index + 1);
                }
                else
                    strUsername = strUserInfo;

                // set credentials
                NetworkCredential networkCredential = new NetworkCredential(strUsername, strPassword);

                request.Credentials = networkCredential;
            }

            try
            {
                if (_strProxyAddress == null) _strProxyAddress = "";
                if (_strProxyUsername == null) _strProxyUsername = "";
                if (_strProxyPassword == null) _strProxyPassword = "";

                // use explicit or default proxy
                if (_strProxyAddress.Length != 0)
                {
                    WebProxy proxy = new WebProxy(_strProxyAddress);

                    // use explicit or default credentials
                    if (_strProxyUsername.Length == 0)
                        proxy.UseDefaultCredentials = true;
                    else
                        proxy.Credentials = new NetworkCredential(_strProxyUsername, _strProxyPassword);

                    request.Proxy = proxy;
                }
                else
                {
                    IWebProxy proxySystem = WebRequest.GetSystemWebProxy();

                    // supply proxy if URL is not bypassed
                    if (!proxySystem.IsBypassed(new Uri(_url)))
                    {
                        WebProxy proxy = new WebProxy(proxySystem.GetProxy(new Uri(_url)))
                        {
                            UseDefaultCredentials = true
                        };

                        // use default credentials

                        request.Proxy = proxy;
                    }
                }
            }
            catch (WebException we)
            {
                throw new Exception("Error proxy: " + we.Message);
            }
            catch (Exception e)
            {
                throw new Exception("Error proxy: " + e.Message);
            }

            return request;
        }
    }

    public class ServerOpenStreetMapsMapnik : ServerOpenStreetMaps
    {
        public ServerOpenStreetMapsMapnik() : base(18)
        {
            // http://www.openstreetmap.org/
            // http://wiki.openstreetmap.org/wiki/Slippy_map_tilenames
            // http://wiki.openstreetmap.org/wiki/Mapnik

            Name = "OpenStreet Maps Street Map Image / Mapnik";
            DefaultURL = "http://tile.openstreetmap.org";
            ScaleNames = "0.5 m,1 m,2 m,5 m,10 m,20 m,40 m,80 m,160 m,320 m,640 m,1.3 km,2.5 km,5 km,10 km,20 km,40 km,80 km";
        }
    }

    public class ServerOpenStreetMapsOsmarender : ServerOpenStreetMaps
    {
        public ServerOpenStreetMapsOsmarender() : base(17)
        {
            Name = "OpenStreet Maps Street Map Image / Osmarender";
            DefaultURL = "http://tah.openstreetmap.org/Tiles/tile";
            ScaleNames = "1 m,2 m,5 m,10 m,20 m,40 m,80 m,160 m,320 m,640 m,1.3 km,2.5 km,5 km,10 km,20 km,40 km,80 km";
        }
    }

    public class ServerCloudMadeMaps : ServerOpenStreetMaps
    {
        public ServerCloudMadeMaps()
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
            return String.Format("{0:s}/256/{1:d}/{2:d}/{3:d}.png", URL, scale, x, y);
        }
    }

    public class ServerCycleMaps : ServerOpenStreetMaps
    {
        public ServerCycleMaps()
            : base(18)
        {
            Name = "Open Cycle Maps";
            DefaultURL = "http://a.tile.opencyclemap.org/cycle";
            ScaleNames = "0.5 m,1 m,2 m,5 m,10 m,20 m,40 m,80 m,160 m,320 m,640 m,1.3 km,2.5 km,5 km,10 km,20 km,40 km,80 km";
            
        }
    }

    public class ServerMapQuestOsmMaps : ServerOpenStreetMaps
    {
        public ServerMapQuestOsmMaps()
            : base(18)
        {
            Name = "MapQuest-OSM street maps";
            DefaultURL = "http://otile1.mqcdn.com/tiles/1.0.0/osm";
            ScaleNames = "0.5 m,1 m,2 m,5 m,10 m,20 m,40 m,80 m,160 m,320 m,640 m,1.3 km,2.5 km,5 km,10 km,20 km,40 km,80 km";

        }
    }

    public class ServerMapQuestOpenAerial : ServerOpenStreetMaps
    {
        public ServerMapQuestOpenAerial()
            : base(18)
        {
            Name = "MapQuest Open Aerial Images";
            DefaultURL = "http://oatile1.mqcdn.com/naip";
            ScaleNames = "0.5 m,1 m,2 m,5 m,10 m,20 m,40 m,80 m,160 m,320 m,640 m,1.3 km,2.5 km,5 km,10 km,20 km,40 km,80 km";

        }
    }
}
