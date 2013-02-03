// For conditions of distribution and use, see AssemblyInfo.cs

using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Manifold.ImageServer.VirtualEarth.Properties;
using System.Net.Cache;

namespace Manifold.ImageServer.VirtualEarth
{
	public abstract class ServerVirtualEarth : IServer
	{
		// Server data
		private String m_strName;
		private String m_strDefaultUrl;
		private String m_strUrl;
		private String m_strImageType;
		private String m_strCoordinateSystemXml;
		private String m_strError;
		private String m_strProxyAddress;
		private String m_strProxyPassword;
		private String m_strProxyUserName;
		private Int32 m_nScaleLo;
		private Int32 m_nScaleHi;
		private Int32 m_nTileSizeX;
		private Int32 m_nTileSizeY;
        private String[] m_strUrlsArray = new String[4];
        private String m_strMapType;
        private String m_strUrlOpts;
		// Bing
		private Int32 m_nEarthRadius = 6378137;

        //private WebBrowser _browser;

		// Constructor
		protected ServerVirtualEarth()
		{
            //_browser = new WebBrowser();

            m_strDefaultUrl = "";
            m_strUrl = "";
			m_strError = "";
			m_strProxyAddress = "";
			m_strProxyPassword = "";
			m_strProxyUserName = "";
			m_nScaleLo = 1;
			m_nScaleHi = 20;
			m_nTileSizeX = 256;
			m_nTileSizeY = 256;
            m_strUrlOpts = "";
            m_strImageType = ".jpg";

			// coordinate system scale and offset
			Double dWholeImageWidth = (1 << m_nScaleHi) * m_nTileSizeX;
			Double dScale = (2 * m_nEarthRadius * Math.PI) / dWholeImageWidth;
			Double dOffset = m_nEarthRadius * Math.PI;

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
			writer.WriteElementString("localOffsetX", (-dOffset).ToString(CultureInfo.InvariantCulture));
			writer.WriteElementString("localOffsetY", "0");
			writer.WriteElementString("localScaleX", dScale.ToString(CultureInfo.InvariantCulture));
			writer.WriteElementString("localScaleY", dScale.ToString(CultureInfo.InvariantCulture));
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteEndDocument();
			writer.Flush();
			writer.Close();

			m_strCoordinateSystemXml = strWriter.ToString();
			strWriter.Close();
		}

		// Create HTTP request
		private HttpWebRequest CreateHttpRequest(String _url, String _strProxyAddress, String _strProxyUsername, String _strProxyPassword)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(_url));
            request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);

			// set user name and password
			String strUserInfo = request.RequestUri.UserInfo;
			if (strUserInfo != null && strUserInfo.Length != 0)
			{
				// send credentials with the request
				request.PreAuthenticate = true;

				String strUsername = "";
				String strPassword = "";

				// try to find password token
				int index = strUserInfo.IndexOf(":");
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
				if (_strProxyAddress == null)
					_strProxyAddress = "";
				if (_strProxyUsername == null)
					_strProxyUsername = "";
				if (_strProxyPassword == null)
					_strProxyPassword = "";

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
					IWebProxy proxySystem = HttpWebRequest.GetSystemWebProxy();

					// supply proxy if URL is not bypassed
					if (!proxySystem.IsBypassed(new Uri(_url)))
					{
						WebProxy proxy = new WebProxy(proxySystem.GetProxy(new Uri(_url)));

						// use default credentials
						proxy.UseDefaultCredentials = true;

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

        /*
        protected String GetTileUrl(Int32 x, Int32 y, Int32 scale, String style)
        {
            int zoom = m_nScaleHi - scale + 1;

            // convert (x, y) to (lon, lat)
            Double px = (x) * 256;
            Double py = (y == 0 ? 0.5 : y) * 256;
            for (Int32 z = zoom; z < 19; z += 1)
            { py *= 2; px *= 2; }

            Double imageSize = (1 << m_nScaleHi) * m_nTileSizeX;
            Double imageScale = (2 * m_nEarthRadius * Math.PI) / imageSize;
            py = (imageSize - py - imageSize / 2) * imageScale;
            px = -(imageSize - px - imageSize / 2) * imageScale;

            Double lat = 180 / Math.PI * (Math.PI / 2.0 - 2.0 * Math.Atan(Math.Exp(-py / m_nEarthRadius)));
            Double lon = 180 / Math.PI * (px / m_nEarthRadius);

            InitializeBrowser(lat, lon, zoom, style);
            WaitForBrowserToComplete();

            String url = "";
            foreach (HtmlElement img in _browser.Document.Images)
            {
                if (img.OffsetRectangle.Left >= 0 && img.OffsetRectangle.Top >= 0)
                { url = img.GetAttribute("src"); break; }
            }

            return url;
        }

        private void InitializeBrowser(Double lat, Double lon, Int32 zoom, String style)
        {
            String document = Resources.document;
            document = document.Replace("%lat%", lat.ToString());
            document = document.Replace("%lon%", lon.ToString());
            document = document.Replace("%zoom%", zoom.ToString());
            document = document.Replace("%style%", style);

            _browser.DocumentText = document;
        }

        private void WaitForBrowserToComplete()
        { 
            // todo:
            while (_browser.IsBusy || _browser.ReadyState != WebBrowserReadyState.Complete
                || _browser.Document.Images.Count < 5)
                Application.DoEvents();
        }
        */ 

		#region IServer Members

		// Get coordinate system in XML format
		public String CoordinateSystem
		{
			get { return m_strCoordinateSystemXml; }
		}

		// Get default image type (eg, ".png")
		public String DefaultImageType
		{
			get { return m_strImageType; }
            protected set { m_strImageType = value; } 
		}

		// Get default URL
		public String DefaultURL
		{
			get { return m_strDefaultUrl; }
            protected set { m_strDefaultUrl = value; }
		}

		// Download tile data to file
		public Boolean DownloadTile(Int32 _x, Int32 _y, Int32 _scale, String _filename)
		{
            // uncomment this to use browser
            //String strRequest = GetTileUrl(_x, _y, _scale, BaseMapStyle);

            // old-style behavior
			String strRequest = "";
			String strKey = "";

			// obtain proper tile base URL
			Int32 nServer = ((_x & 1) + ((_y & 1) << 1)) % m_strUrlsArray.Length;

			Int32 i = m_nScaleHi - _scale + 1;
			while (i > 0)
			{
				Int32 mask = 1 << (i - 1);
				Int32 cell = 0;
				if ((_x & mask) != 0) cell++;
				if ((_y & mask) != 0) cell += 2;
				strKey += cell.ToString(CultureInfo.InvariantCulture);
				i--;
			}

			if (m_strDefaultUrl == m_strUrl)
                strRequest = m_strUrlsArray[nServer] + m_strMapType + strKey + "?g=516&mkt=en-us&n=z" + m_strUrlOpts;
			else // custom url
                strRequest = m_strUrl + m_strMapType + strKey + "?g=516&mkt=en-us&n=z" + m_strUrlOpts;

            String tileInfo = "Url " + strRequest + ". ";

			try
			{
				// initialize request
				HttpWebRequest request = CreateHttpRequest(strRequest, m_strProxyAddress, m_strProxyUserName, m_strProxyPassword);
				// obtain response
				HttpWebResponse response = (HttpWebResponse)request.GetResponse();
				// process response
				String strType = response.ContentType;

				int nIndex = strType.IndexOf("image");
				if (nIndex > -1)
				{
					// try converting to an image
					Image image = Image.FromStream(response.GetResponseStream());
                    if (_filename != null && _filename.Length != 0)
                    {
                        if (m_strImageType.Equals(".jpg", StringComparison.InvariantCultureIgnoreCase))
                            image.Save(_filename, System.Drawing.Imaging.ImageFormat.Jpeg);
                        else
                            image.Save(_filename, System.Drawing.Imaging.ImageFormat.Png);
                    }
				}
				else
				{
					// read all data
					StreamReader objReader = new StreamReader(response.GetResponseStream());
					String error = objReader.ReadToEnd();
					objReader.Close();

					if (error == null || error.Length == 0)
						error = "Can't obtain image from server. ";

					m_strError = tileInfo + error;

					response.Close();
					return false;
				}

				response.Close();
				return true;
			}
			catch (WebException we)
			{
				m_strError = tileInfo + "Can't obtain image from server. " + we.Message;
				return false;
			}
			catch (ArgumentException ae)
			{
				m_strError = tileInfo + "Can't obtain image from server. " + ae.Message;
				return false;
			}
			catch (Exception e)
			{
				m_strError = tileInfo + "Can't obtain image from server. " + e.Message;
				throw;
			}
		}

		// Obtain last error
		public String Error
		{
			get { return m_strError; }
		}

		// Get the supplied area, in pixels
		public IRectangle GetRectPixels(Int32 _scale, IRectangleD _rect)
		{
			int d = (1 << (m_nScaleHi - _scale + 1)) * m_nTileSizeY;
			double c = Math.Pow(2.0, _scale - m_nScaleLo);

			// y should be reversed
			Rectangle rect = new Rectangle((int)((_rect.XMin) / c), Math.Max(0, Math.Min(d - 1, (int)(d / 2 - (_rect.YMax) / c))), (int)((_rect.XMax) / c), Math.Max(0, Math.Min(d - 1, (int)(d / 2 - (_rect.YMin) / c))));
            return rect;
		}

		// Get the supplied area, in tiles
		public IRectangle GetRectTiles(Int32 _scale, IRectangleD _rect)
		{
			IRectangle rectTiles = GetRectPixels(_scale, _rect);
			rectTiles.XMin = rectTiles.XMin / m_nTileSizeX;
			rectTiles.YMin = rectTiles.YMin / m_nTileSizeY;
			rectTiles.XMax = rectTiles.XMax / m_nTileSizeX;
			rectTiles.YMax = rectTiles.YMax / m_nTileSizeY;
			return rectTiles;
		}

		// Get name
		public String Name
		{
			get { return m_strName; }
            protected set { m_strName = value; }
		}

		// Get or set proxy address
		public String ProxyAddress
		{
			get { return m_strProxyAddress; }
			set { m_strProxyAddress = value; }
		}

		// Get or set proxy password
		public String ProxyPassword
		{
			get { return m_strProxyPassword; }
			set { m_strProxyPassword = value; }
		}

		// Get or set proxy user name
		public String ProxyUserName
		{
			get { return m_strProxyUserName; }
			set { m_strProxyUserName = value; }
		}

		// Returns True if Y axis is reversed and False otherwise
		public Boolean ReverseY
		{
			get { return true; }
		}

		// Get least detailed scale
		public Int32 ScaleHi
		{
			get { return m_nScaleHi; }
		}

		// Get most detailed scale
		public Int32 ScaleLo
		{
			get { return m_nScaleLo; }
		}

		// Get scale names separated by commas
		public String ScaleNames
		{
			get { return "0.125 m, 0.25 m,0.5 m,1 m,2 m,5 m,10 m,20 m,40 m,80 m,160 m,320 m,640 m,1.3 km,2.5 km,5 km,10 km,20 km,40 km,80 km"; }
		}

		// Get tile size by X
		public Int32 TileSizeX
		{
			get { return m_nTileSizeX; }
		}

		// Get tile size by Y
		public Int32 TileSizeY
		{
			get { return m_nTileSizeY; }
		}

		// Get or set URL
		public String URL
		{
			get { return m_strUrl; }
			set { m_strUrl = value; }
		}

		#endregion

        // Get or set Bing map type
        protected String BaseMapType
        {
            get { return m_strMapType; }
            set { m_strMapType = value; }
        }

        protected String BaseUrlOpts
        {
            get { return m_strUrlOpts; }
            set { m_strUrlOpts = value; }
        }

        // Get or set default URLs array member
        protected String BaseURL0
        {
            get { return m_strUrlsArray[0]; }
            set { m_strUrlsArray[0] = value; }
        }

        // Get or set default URLs array member
        protected String BaseURL1
        {
            get { return m_strUrlsArray[1]; }
            set { m_strUrlsArray[1] = value; }
        }

        // Get or set default URLs array member
        protected String BaseURL2
        {
            get { return m_strUrlsArray[2]; }
            set { m_strUrlsArray[2] = value; }
        }

        // Get or set default URLs array member
        protected String BaseURL3
        {
            get { return m_strUrlsArray[3]; }
            set { m_strUrlsArray[3] = value; }
        }
	}

	public class ServerVirtualEarthSatellite : ServerVirtualEarth
	{
		// Constructor
		public ServerVirtualEarthSatellite()
			: base()
		{
			Name = "Bing Satellite Image";
            String strDefaultUrl = "http://ecn.t*.tiles.virtualearth.net/tiles/";
            BaseMapType = "a";
            BaseURL0 = "http://ecn.t0.tiles.virtualearth.net/tiles/";
            BaseURL1 = "http://ecn.t1.tiles.virtualearth.net/tiles/";
            BaseURL2 = "http://ecn.t2.tiles.virtualearth.net/tiles/";
            BaseURL3 = "http://ecn.t3.tiles.virtualearth.net/tiles/";
            DefaultURL = strDefaultUrl;
            URL = strDefaultUrl;
		}
	}


    public class ServerVirtualEarthStreetMap : ServerVirtualEarth
    {
        // Constructor
        public ServerVirtualEarthStreetMap()
            : base()
        {
            Name = "Bing Street Map Image";
            DefaultImageType = ".png";
            String strDefaultUrl = "http://ecn.t*.tiles.virtualearth.net/tiles/";
            BaseMapType = "r";
            BaseURL0 = "http://ecn.t0.tiles.virtualearth.net/tiles/";
            BaseURL1 = "http://ecn.t1.tiles.virtualearth.net/tiles/";
            BaseURL2 = "http://ecn.t2.tiles.virtualearth.net/tiles/";
            BaseURL3 = "http://ecn.t3.tiles.virtualearth.net/tiles/";
            DefaultURL = strDefaultUrl;
            BaseUrlOpts = "&lbl=l1&stl=h&shading=hill";
            URL = strDefaultUrl;
        }
    }

    public class ServerVirtualEarthHybrid : ServerVirtualEarth
    {
        // Constructor
        public ServerVirtualEarthHybrid()
            : base()
        {
            Name = "Bing Hybrid Image";
            String strDefaultUrl = "http://ecn.t*.tiles.virtualearth.net/tiles/";
            BaseMapType = "h";
            BaseURL0 = "http://ecn.t0.tiles.virtualearth.net/tiles/";
            BaseURL1 = "http://ecn.t1.tiles.virtualearth.net/tiles/";
            BaseURL2 = "http://ecn.t2.tiles.virtualearth.net/tiles/";
            BaseURL3 = "http://ecn.t3.tiles.virtualearth.net/tiles/";
            DefaultURL = strDefaultUrl;
            URL = strDefaultUrl;
        }
    }
}
