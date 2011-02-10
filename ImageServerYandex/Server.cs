// For conditions of distribution and use, see AssemblyInfo.cs

using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Manifold.ImageServer.Yandex
{
	public abstract class ServerYandex : IServer
	{
		// Server data
        protected String m_strName;
        protected String m_strDefaultUrl;
        protected String m_strUrl;
		protected String m_strImageType;
		private String m_strCoordinateSystemXml;
		private String m_strError;
		private String m_strProxyAddress;
		private String m_strProxyPassword;
		private String m_strProxyUserName;
		private Int32 m_nScaleLo;
		private Int32 m_nScaleHi;
		private Int32 m_nTileSizeX;
		private Int32 m_nTileSizeY;

		// Constructor
        public ServerYandex()
		{
			m_strName = "";
            m_strDefaultUrl = "";
			m_strUrl = "";
			m_strImageType = "";
			m_strError = "";
			m_strProxyAddress = "";
			m_strProxyPassword = "";
			m_strProxyUserName = "";
			m_nScaleLo = 0;
			m_nScaleHi = 17;
			m_nTileSizeX = 256;
			m_nTileSizeY = 256;

            // save coordinate system in xml format
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Encoding = Encoding.UTF8;
            settings.Indent = true;

            StringWriter strWriter = new StringWriter();
            XmlWriter writer = XmlWriter.Create(strWriter, settings);
            writer.WriteStartDocument();
            writer.WriteStartElement("data");
            writer.WriteStartElement("coordinateSystem");
            writer.WriteElementString("name", "Mercator");
            writer.WriteElementString("datum", "World Geodetic 1984 (WGS84)");
            writer.WriteElementString("system", "Mercator");
            writer.WriteElementString("unit", "Meter");
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
//          request.UserAgent = "Mozila/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; MyIE2;";

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

        protected Point GetBitmapCoordinate(PointF ptMercator, int _scale)
        {
            Point ptBitmapCoord = new Point();

            long imageSize = (1 << m_nScaleHi) * m_nTileSizeX;
            double halfEq = 20037508.342789244; // pi*6378137
            double scale = imageSize / (2 * halfEq); // pixels per meter
            double zoom = 1 << _scale;

            double x = (halfEq + ptMercator.X) * scale;
            double y = (halfEq - ptMercator.Y) * scale;

            x = Math.Max(Math.Min(imageSize, x), 0);
            y = Math.Max(Math.Min(imageSize, y), 0);

            ptBitmapCoord.X = (int)Math.Round(x / zoom);
            ptBitmapCoord.Y = (int)Math.Round(y / zoom);

            return ptBitmapCoord;
        }

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
		}

		// Get default URL
		public String DefaultURL
		{
			get { return m_strDefaultUrl; }
		}

		// Download tile data to file
		public Boolean DownloadTile(Int32 _x, Int32 _y, Int32 _scale, String _filename)
		{
            String strRequest = Regex.Replace(m_strUrl, "\\s", String.Empty);

			int nIndex = strRequest.IndexOf("?");
            if (nIndex == -1)
                strRequest += "?";
            else if (nIndex < strRequest.Length - 1 && strRequest[strRequest.Length - 1] != '&')
                strRequest += "&";

            strRequest += "x=" + Convert.ToString(_x, CultureInfo.InvariantCulture);
            strRequest += "&y=" + Convert.ToString(_y, CultureInfo.InvariantCulture);
            strRequest += "&z=" + Convert.ToString(m_nScaleHi - _scale, CultureInfo.InvariantCulture);

            String tileInfo = "Url " + strRequest + ". ";
            try
			{
				// initialize request
				HttpWebRequest request = CreateHttpRequest(strRequest, m_strProxyAddress, m_strProxyUserName, m_strProxyPassword);

				// obtain response
				HttpWebResponse response = (HttpWebResponse)request.GetResponse();

				// process response
				String strType = response.ContentType;

                try
				{
				    nIndex = strType.IndexOf("image");
				    if (nIndex > -1)
				    {
					    // try converting to an image
					    Image image = Image.FromStream(response.GetResponseStream());
					    if (_filename != null && _filename.Length != 0)
                            image.Save(_filename);
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
                }
                catch
                {
                    m_strError = tileInfo + "Can't obtain image from server";
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
                return false;
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
            Point ptLB = GetBitmapCoordinate(new PointF((float)_rect.XMin, (float)_rect.YMin), _scale);
            Point ptRT = GetBitmapCoordinate(new PointF((float)_rect.XMax, (float)_rect.YMax), _scale);

            // left-top pixel is at (0, 0), reversed by Y
            return new Rectangle(ptLB.X, ptRT.Y, ptRT.X, ptLB.Y);
        }

        // Get the supplied area, in tiles
        public IRectangle GetRectTiles(Int32 _scale, IRectangleD _rect)
        {
            IRectangle rectFix = GetRectPixels(_scale, _rect);
            rectFix.XMax = rectFix.XMax - 1;
            rectFix.YMax = rectFix.YMax - 1;

            rectFix.XMin = rectFix.XMin / m_nTileSizeX;
            rectFix.YMin = rectFix.YMin / m_nTileSizeY;
            rectFix.XMax = rectFix.XMax / m_nTileSizeX;
            rectFix.YMax = rectFix.YMax / m_nTileSizeY;
            return rectFix;
        }

		// Get name
		public String Name
		{
			get { return m_strName; }
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
            get
            {
                return "1 m,2 m,5 m,10 m,20 m,40 m,80 m,160 m,320 m,640 m,1.3 km,2.5 km,5 km,10 km,20 km,40 km,80 km,160 km";
            }
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
	}

    public class ServerYandexStreetMap : ServerYandex
    {
        public ServerYandexStreetMap()
            :base()
        {
            m_strName = "Yandex Maps Street Map Image";
            m_strDefaultUrl = "http://vec.maps.yandex.net/tiles?l=map&v=2.16.0";
            m_strUrl = m_strDefaultUrl;
            m_strImageType = ".png";
        }
    }

    public class ServerYandexStreetMapTransparent : ServerYandex
    {
        public ServerYandexStreetMapTransparent()
            : base()
        {
            m_strName = "Yandex Maps Street Map Image (Transparent)";
            m_strDefaultUrl = "http://vec.maps.yandex.net/tiles?l=skl&v=2.16.0";
            m_strUrl = m_strDefaultUrl;
            m_strImageType = ".png";
        }
    }

    public class ServerYandexSatellite : ServerYandex
    {
        public ServerYandexSatellite()
            : base()
        {
            m_strName = "Yandex Maps Satellite Image";
            m_strDefaultUrl = "http://sat.maps.yandex.net/tiles?l=sat&v=1.19.0";
            m_strUrl = m_strDefaultUrl;
            m_strImageType = ".jpg";
        }
    }
}
