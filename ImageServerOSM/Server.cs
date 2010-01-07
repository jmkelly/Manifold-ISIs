// For conditions of distribution and use, see copyright notice in AssemblyInfo.cs
// Code largely copied from the Manifold Google Image Server driver, (c)
// Sergey Mitin 2005 - 2006

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Collections.Generic;


namespace Manifold.ImageServer.OSM
{


    public class ServerOSMmapNik : ServerOSM
    {
        // Constructor
        public ServerOSMmapNik()
            : base()
        {
            //Mapnik rendered tiles
            //OSM usage guidelines specify not more than 2 connections to the tile server for MapNik.
            String strDefaultUrl = "http://*.tile.openstreetmap.org/";
            BaseURL0 = "http://a.tile.openstreetmap.org/";
            BaseURL1 = "http://b.tile.openstreetmap.org/";
            BaseDefaultURL = strDefaultUrl;
            URL = strDefaultUrl;
            BaseName = "OSM MapNik Steet Maps";
            BaseDefaultImageType = ".png";


        }
    }



    public class ServerOSMOsmarendered : ServerOSM
    {
        // Constructor
        public ServerOSMOsmarendered()
            : base()
        {
            //Osmarender rendered tiles
            String strDefaultUrl = "http://*.tah.openstreetmap.org/Tiles/tile/";
            BaseURL0 = "http://a.tah.openstreetmap.org/Tiles/tile/";
            BaseURL1 = "http://b.tah.openstreetmap.org/Tiles/tile/";
            BaseURL2 = "http://c.tah.openstreetmap.org/Tiles/tile/";
            BaseURL3 = "http://d.tah.openstreetmap.org/Tiles/tile/";
            BaseURL4 = "http://e.tah.openstreetmap.org/Tiles/tile/";
            BaseURL5 = "http://f.tah.openstreetmap.org/Tiles/tile/";
            BaseDefaultURL = strDefaultUrl;
            URL = strDefaultUrl;
            BaseName = "OSM Osmarenderer Street Maps";
            BaseDefaultImageType = ".png";



        }
    }

    public class ServerOSMcycleMap : ServerOSM
    {
        // Constructor
        public ServerOSMcycleMap()
            : base()
        {
            //Cloudemade cycle maps rendered tiles
            String strDefaultUrl = "http://*.andy.sandbox.cloudmade.com/tiles/cycle/";
            BaseURL0 = "http://a.andy.sandbox.cloudmade.com/tiles/cycle/";
            BaseURL1 = "http://b.andy.sandbox.cloudmade.com/tiles/cycle/";
            BaseURL2 = "http://c.andy.sandbox.cloudmade.com/tiles/cycle/";            
            BaseName = "OSM Cloudmade Cycle Maps";
            BaseDefaultURL = strDefaultUrl;
            URL = strDefaultUrl;
            BaseDefaultImageType = ".png";




        }
    }
    


    public abstract class ServerOSM : IServer
    {
        // Server data
        protected Int32 m_nScaleHi = 0;
        protected Int32 m_nScaleLo = 0;
        protected Int32 m_nTileSizeX = 0;
        protected Int32 m_nTileSizeY = 0;
        protected String m_strCoordinateSystemXml = "";
        protected String m_strDefaultUrl = "";
        protected String m_strError = "";
        protected String m_strImageType = "";
        protected String m_strName = "";
        protected String m_strProxyAddress = "";
        protected String m_strProxyPassword = "";
        protected String m_strProxyUserName = "";
        protected String m_strUrl = "";
        //private String[] m_strUrlsArray = new String[6];
        private List<string> m_strUrlsArray = new List<string>();

        // Constructor
        public ServerOSM()
        {
            m_strError = "";
            m_strProxyAddress = "";
            m_strProxyPassword = "";
            m_strProxyUserName = "";
            m_nScaleLo = 0;
            m_nScaleHi = 18;
            m_nTileSizeX = 256;
            m_nTileSizeY = 256;



            // compute coordinate system scale
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
            writer.WriteElementString("datum", "World Geodetic 1984 (WGS84) Auto");
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

        public void writeToLog(string logMessage)
        {

            string sLog;
            string sEvent;
            string sSource;

            sSource = "OSM mapnik Manifold Image Server";
            sLog = "Application";
            sEvent = logMessage;

            if (!EventLog.SourceExists(sSource))
                EventLog.CreateEventSource(sSource, sLog);

            //EventLog.WriteEntry(sSource, sEvent);
            EventLog.WriteEntry(sSource, sEvent,
                EventLogEntryType.Information, 234);

        }


        protected Point GetBitmapCoordinate(PointF ptMercator, int _scale)
        {
            Point ptBitmapCoord = new Point();

            double dZoomCoef = Math.Pow(2.0, m_nScaleHi - _scale);
            double dWidth = 2 * 20037508.342789244; // sphere: 20015077.371242613
            double dHeight = dWidth;

            // left-top pixel is at (0, 0), reversed by Y
            ptBitmapCoord.X = (int)Math.Floor(m_nTileSizeX * dZoomCoef / 2.0 + ptMercator.X * m_nTileSizeX * dZoomCoef / dWidth);
            ptBitmapCoord.Y = (int)Math.Floor(m_nTileSizeY * dZoomCoef / 2.0 - ptMercator.Y * m_nTileSizeY * dZoomCoef / dHeight);
            ptBitmapCoord.Y = Math.Max(0, ptBitmapCoord.Y);
            ptBitmapCoord.Y = Math.Min((int)(m_nTileSizeY * dZoomCoef), ptBitmapCoord.Y);

            return ptBitmapCoord;

        }


        #region IServer Members

        public string CoordinateSystem
        {
            get { return m_strCoordinateSystemXml; }
        }

        public string DefaultImageType
        {
            get { return m_strImageType; }
        }

        public string DefaultURL
        {
            get { return m_strDefaultUrl; }
        }

        public bool DownloadTile(int _x, int _y, int _scale, string _filename)
        {
            //reverse the scaling values
            Int32 TMSscale = m_nScaleHi - _scale;

            String tileInfo = "";
            String strRequest = "";

            Int32 nServer = ((_x & 1) + ((_y & 1) << 1)) % m_strUrlsArray.Count;
            
            if (m_strDefaultUrl == m_strUrl)
            {
                //request image from standard request
                strRequest = m_strUrlsArray[nServer] + TMSscale.ToString(CultureInfo.InvariantCulture);
                strRequest += "/" + _x.ToString(CultureInfo.InvariantCulture);
                strRequest += "/" + _y.ToString(CultureInfo.InvariantCulture);
                strRequest += m_strImageType;
                
            }
            else
            {
                //custom request
                strRequest += m_strUrl + TMSscale.ToString(CultureInfo.InvariantCulture);
                strRequest += "/" + _x.ToString(CultureInfo.InvariantCulture);
                strRequest += "/" + _y.ToString(CultureInfo.InvariantCulture);
                strRequest += m_strImageType;
            }





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

                    Image image = Image.FromStream(response.GetResponseStream());
                    PixelFormat pixelFormat = image.PixelFormat;
                    Bitmap bitmap = new Bitmap(image);
                   
                    if (_filename != null && _filename.Length != 0)
                        bitmap.Save(_filename, ImageFormat.Png);

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

        public string Error
        {
            get
            {
                return m_strError;
            }
        }

        public IRectangle GetRectPixels(int _scale, IRectangleD _rect)
        {

            Point ptLB = GetBitmapCoordinate(new PointF((float)_rect.XMin, (float)_rect.YMin), _scale);
            Point ptRT = GetBitmapCoordinate(new PointF((float)_rect.XMax, (float)_rect.YMax), _scale);

            // left-top pixel is at (0, 0), reversed by Y
            return new Rectangle(ptLB.X, ptRT.Y, ptRT.X, ptLB.Y);

        }

        public IRectangle GetRectTiles(int _scale, IRectangleD _rect)
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

        public string Name
        {
            get { return m_strName; }
        }

        public string ProxyAddress
        {
            get { return m_strProxyAddress; }
            set { m_strProxyAddress = value; }
        }

        public string ProxyPassword
        {
            get { return m_strProxyPassword; }
            set { m_strProxyPassword = value; }
        }

        public string ProxyUserName
        {
            get { return m_strProxyUserName; }
            set { m_strProxyUserName = value; }
        }

        public bool ReverseY
        {
            get
            {
                return true;
            }
        }

        public int ScaleHi
        {
            get { return m_nScaleHi; }
        }

        public int ScaleLo
        {
            get { return m_nScaleLo; }
        }

        public string ScaleNames
        {
            get { return "1 m,2 m,5 m,10 m,20 m,40 m,80 m,160 m,320 m,640 m,1.3 km,2.5 km,5 km,10 km,20 km,40 km,80 km"; }
        }

        public int TileSizeX
        {
            get { return m_nTileSizeX; }
        }

        public int TileSizeY
        {
            get { return m_nTileSizeY; }
        }

        public string URL
        {
            get { return m_strUrl; }
            set { m_strUrl = value; }
        }

        #endregion

        // Get or set default URLs array member
        protected String BaseURL0
        {
            get { return m_strUrlsArray[0]; }
            set { m_strUrlsArray.Add(value); }
        }

        // Get or set default URLs array member
        protected String BaseURL1
        {
            get { return m_strUrlsArray[1]; }
            set { m_strUrlsArray.Add(value); }

        }
        protected String BaseURL2
        {
            get { return m_strUrlsArray[2]; }
            set { m_strUrlsArray.Add(value); }

        }

        protected String BaseURL3
        {
            get { return m_strUrlsArray[3]; }
            set { m_strUrlsArray.Add(value); }

        }

        protected String BaseURL4
        {
            get { return m_strUrlsArray[4]; }
            set { m_strUrlsArray.Add(value); }

        }

        protected String BaseURL5
        {
            get { return m_strUrlsArray[5]; }
            set { m_strUrlsArray.Add(value); }

        }

        protected String BaseName
        {
            get { return m_strName; }
            set { m_strName = value; }
        }

        // Get or set default image type
        protected String BaseDefaultImageType
        {
            get { return m_strImageType; }
            set { m_strImageType = value; }
        }


        // Get or set default URL
        protected String BaseDefaultURL
        {
            get { return m_strDefaultUrl; }
            set { m_strDefaultUrl = value; }
        }

    }
}
