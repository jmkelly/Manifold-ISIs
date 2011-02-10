// For conditions of distribution and use, see AssemblyInfo.cs

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Net.Cache;

namespace Manifold.ImageServer.Yahoo
{
	public abstract class ServerYahoo : IServer
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

        private String[] _urls = new String[3];
        protected String[] Urls { get { return _urls; } }

		// Constructor
		public ServerYahoo()
		{
            m_strName = "";
            m_strDefaultUrl = "";
			m_strUrl = m_strDefaultUrl;
			m_strImageType = ".png";
			m_strError = "";
			m_strProxyAddress = "";
			m_strProxyPassword = "";
			m_strProxyUserName = "";
			m_nScaleLo = 1;
			m_nScaleHi = 17;
			m_nTileSizeX = 256;
			m_nTileSizeY = 256;

			// compute coordinate system scale
            Int32 earthRadius = 6378137;
            Double imageSize = (1 << m_nScaleHi) * m_nTileSizeX;
            Double dScale = (2 * earthRadius * Math.PI) / imageSize;
            Double dOffset = earthRadius * Math.PI;

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
            writer.WriteElementString("localOffsetY", (-dOffset).ToString(CultureInfo.InvariantCulture));
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
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_url);
            RequestCachePolicy policy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
            request.CachePolicy = policy;
            //request.UserAgent = "Mozila/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; MyIE2;";

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

        // IServer members

		// Download tile data to file
		public Boolean DownloadTile(Int32 _x, Int32 _y, Int32 _scale, String _filename)
		{
            _scale = ScaleHi - _scale + 1;
            Int32 tiles = (Int32)Math.Pow(2, _scale); // currect scale tiles
            _scale += 1; // yahoo's scale
            _y -= tiles / 2; // move '0' to equator

            String url = DefaultURL.Equals(URL, StringComparison.InvariantCultureIgnoreCase)
                ? Urls[Math.Abs(_x + _y) % Urls.Length] : URL;

			String tileInfo = "";
            String strRequest = url;
			strRequest += "&x=" + _x.ToString(CultureInfo.InvariantCulture);
			strRequest += "&y=" + _y.ToString(CultureInfo.InvariantCulture);
			strRequest += "&z=" + _scale.ToString(CultureInfo.InvariantCulture);

			try
			{
				// initialize request
				HttpWebRequest request = CreateHttpRequest(strRequest, m_strProxyAddress, m_strProxyUserName, m_strProxyPassword);

				// obtain response
				HttpWebResponse response = (HttpWebResponse)request.GetResponse();

				// process response
				String strType = response.ContentType;
				Int32 nIndex = strType.IndexOf("image");
				if (nIndex > -1)
				{
					// try converting to an image
					Image image = Image.FromStream(response.GetResponseStream());
					// crop the image
                    //PixelFormat pixelFormat = image.PixelFormat;
                    //Bitmap bitmap = new Bitmap(image).Clone(new System.Drawing.Rectangle(1, 1, 256, 256), pixelFormat);

					if (_filename != null && _filename.Length != 0)
						image.Save(_filename, ImageFormat.Png);
				}
				else
				{
					// read all data
					StreamReader objReader = new StreamReader(response.GetResponseStream());
					String error = objReader.ReadToEnd();
					objReader.Close();

					if (error == null || error.Length == 0)
						error = "Can't obtain image from server";

					m_strError = tileInfo + error;

					response.Close();
					return false;
				}

				response.Close();
				return true;
			}
			catch (WebException we)
			{
				m_strError = tileInfo + we.Message;
				return false;
			}
			catch (Exception e)
			{
				m_strError = tileInfo + e.Message;
				throw;
			}
		}

		// Get the supplied area, in pixels
		public IRectangle GetRectPixels(Int32 _scale, IRectangleD _rect)
		{
            int d = (1 << (m_nScaleHi - _scale + 1)) * m_nTileSizeY;
            double c = Math.Pow(2.0, _scale - m_nScaleLo);
            Rectangle rect = new Rectangle((int)((_rect.XMin) / c), Math.Max(0, Math.Min(d - 1, (int)((_rect.YMin) / c))), (int)((_rect.XMax) / c), Math.Max(0, Math.Min(d - 1, (int)((_rect.YMax) / c))));
            return rect;
		}

		// Get the supplied area, in tiles
		public IRectangle GetRectTiles(Int32 _scale, IRectangleD _rect)
		{
			IRectangle rect = GetRectPixels(_scale, _rect);
			rect.XMin = (Int32)(rect.XMin / m_nTileSizeX);
			rect.YMin = (Int32)(rect.YMin / m_nTileSizeY);
			rect.XMax = (Int32)(rect.XMax / m_nTileSizeX);
			rect.YMax = (Int32)(rect.YMax / m_nTileSizeY);
			return rect;
		}

        // Get name
        public String Name
        {
            get { return m_strName; }
            protected set { m_strName = value; }
        }
        // Get default URL
        public String DefaultURL
        {
            get { return m_strDefaultUrl; }
            protected set { m_strDefaultUrl = value; m_strUrl = value; } 
        }
        // Get coordinate system in XML format
        public String CoordinateSystem { get { return m_strCoordinateSystemXml; } }
        // Get default image type (eg, ".png")
        public String DefaultImageType { get { return m_strImageType; } }
        // Obtain last error
        public String Error { get { return m_strError; } }
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
        public Boolean ReverseY { get { return false; } }
		// Get least detailed scale
        public Int32 ScaleHi { get { return m_nScaleHi; } }
		// Get most detailed scale
        public Int32 ScaleLo { get { return m_nScaleLo; } }
		// Get scale names separated by commas
		public String ScaleNames
		{
			get { return "1 m,2 m,5 m,10 m,20 m,40 m,80 m,160 m,320 m,640 m,1.3 km,2.5 km,5 km,10 km,20 km,40 km,80 km"; }
		}
		// Get tile size by X
        public Int32 TileSizeX { get { return m_nTileSizeX; } }
		// Get tile size by Y
        public Int32 TileSizeY { get { return m_nTileSizeY; } }
		// Get or set URL
		public String URL
		{
			get { return m_strUrl; }
			set { m_strUrl = value; }
		}
	}

    public class ServerYahooSatellite : ServerYahoo
    {
        public ServerYahooSatellite()
        {
            Name = "Yahoo! Maps Satellite Image";
            DefaultURL = "http://maps*.yimg.com/ae/ximg?v=1.9&t=a&s=256&.intl=en&r=1";
            Urls[0] = "http://maps1.yimg.com/ae/ximg?v=1.9&t=a&s=256&.intl=en&r=1";
            Urls[1] = "http://maps2.yimg.com/ae/ximg?v=1.9&t=a&s=256&.intl=en&r=1";
            Urls[2] = "http://maps3.yimg.com/ae/ximg?v=1.9&t=a&s=256&.intl=en&r=1";
        }
    }


    public class ServerYahooStreetMap : ServerYahoo
    {
        public ServerYahooStreetMap()
        {
            Name = "Yahoo! Maps Street Map Image";
            DefaultURL = "http://maps*.yimg.com/hx/tl?b=1&v=4.3&intl=en&r=1";
            Urls[0] = "http://maps1.yimg.com/hx/tl?b=1&v=4.3&intl=en&r=1";
            Urls[1] = "http://maps2.yimg.com/hx/tl?b=1&v=4.3&intl=en&r=1";
            Urls[2] = "http://maps3.yimg.com/hx/tl?b=1&v=4.3&intl=en&r=1";
        }
    }

    public class ServerYahooStreetMapTransparent : ServerYahoo
    {
        public ServerYahooStreetMapTransparent()
        {
            Name = "Yahoo! Maps Street Map Image (Transparent)";
            DefaultURL = "http://maps*.yimg.com/hx/tl?b=1&v=4.3&intl=en&r=1&t=h";
            Urls[0] = "http://maps1.yimg.com/hx/tl?b=1&v=4.3&intl=en&r=1&t=h";
            Urls[1] = "http://maps2.yimg.com/hx/tl?b=1&v=4.3&intl=en&r=1&t=h";
            Urls[2] = "http://maps3.yimg.com/hx/tl?b=1&v=4.3&intl=en&r=1&t=h";
        }
    }
}
