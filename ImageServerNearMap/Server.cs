// Copyright (C) 2005-2006 Sergey Mitin
// For conditions of distribution and use, see copyright notice in AssemblyInfo.cs

using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Diagnostics;
using System.Collections.Generic;

namespace Manifold.ImageServer.NearMap
{
    public abstract class ServerNearMap : IServer
    {
        // Constructor
        public ServerNearMap()
        {
            
        }

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
       // CookieContainer cookies = null;

        #region Methods

        public static String GalileoExtract()
        {
            //required for firefox for caching issues.  Not needed for manifold
            Random rnd = new Random();
            int charNo = rnd.Next(1, 6);
            String str = "Galileo";
            String output = str.Substring(0, charNo);
            return output;
        }

        // Create HTTP request
        public HttpWebRequest CreateHttpRequest(String _url, String _strProxyAddress, String _strProxyUsername, String _strProxyPassword)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(_url));
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.2; WOW64; .NET CLR 2.0.50727; .NET CLR 1.1.4322; .NET CLR 3.0.04506.30; .NET CLR 3.0.04506.648; InfoPath.1; FDM; .NET CLR 3.5.21022; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729)";
            //request.UserAgent = "Windows-RSS-Platform/1.0 (MSIE 7.0; Windows NT 5.1)";
            request.Timeout = 15000;
            //request.CookieContainer = cookies;
            request.Method = "GET";
            request.Accept = "*/*";
            //request.Referer = "http://maps.google.com/maps?hl=en&tab=wl";

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

        // Download tile
        protected Boolean DownloadTile(String _strRequest, String _filename)
        {
            String tileInfo = "Url " + _strRequest + ". ";
            try
            {
                // initialize request
                HttpWebRequest request = CreateHttpRequest(_strRequest, m_strProxyAddress, m_strProxyUserName, m_strProxyPassword);

                // obtain response
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                // process response
                String strType = response.ContentType;
                try
                {
                    int nIndex = strType.IndexOf("image");
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

        #endregion

        #region IServer interface

        // Get coordinate system in XML format
        public String CoordinateSystem
        {
            get
            {
                return m_strCoordinateSystemXml;
            }
        }

        // Get default image type (eg, ".jpg")
        public String DefaultImageType
        {
            get
            {
                return m_strImageType;
            }
        }

        // Get default URL
        public String DefaultURL
        {
            get
            {
                return m_strDefaultUrl;
            }
        }

        // Download tile data to file
        abstract public Boolean DownloadTile(Int32 _x, Int32 _y, Int32 _scale, String _filename);

        // Obtain last error
        public String Error
        {
            get
            {
                return m_strError;
            }
        }

        // Get the supplied area, in pixels
        abstract public IRectangle GetRectPixels(Int32 _scale, IRectangleD _rect);

        // Get the supplied area, in tiles
        abstract public IRectangle GetRectTiles(Int32 _scale, IRectangleD _rect);

        // Get name
        public String Name
        {
            get
            {
                return m_strName;
            }
        }

        // Get or set proxy address
        public String ProxyAddress
        {
            get
            {
                return m_strProxyAddress;
            }
            set
            {
                m_strProxyAddress = value;
            }
        }

        // Get or set proxy password
        public String ProxyPassword
        {
            get
            {
                return m_strProxyPassword;
            }
            set
            {
                m_strProxyPassword = value;
            }
        }

        // Get or set proxy user name
        public String ProxyUserName
        {
            get
            {
                return m_strProxyUserName;
            }
            set
            {
                m_strProxyUserName = value;
            }
        }

        // Returns True if Y axis is reversed and False otherwise
        public Boolean ReverseY
        {
            get
            {
                return true;
            }
        }

        // Get least detailed scale
        public Int32 ScaleHi
        {
            get
            {
                return m_nScaleHi;
            }
        }

        // Get most detailed scale
        public Int32 ScaleLo
        {
            get
            {
                return m_nScaleLo;
            }
        }

        // Get scale names separated by commas
        abstract public String ScaleNames { get; }

        // Get tile size by X
        public Int32 TileSizeX
        {
            get
            {
                return m_nTileSizeX;
            }
        }

        // Get tile size by Y
        public Int32 TileSizeY
        {
            get
            {
                return m_nTileSizeY;
            }
        }

        // Get or set URL
        public String URL
        {
            get
            {
                return m_strUrl;
            }
            set
            {
                m_strUrl = value;
            }
        }

        #endregion

        public void writeToLog(string logMessage)
        {

            string sLog;
            string sEvent;
            string sSource;

            sSource = "NearMap Manifold Image Server";
            sLog = "Application";
            sEvent = logMessage;

            if (!EventLog.SourceExists(sSource))
                EventLog.CreateEventSource(sSource, sLog);

            //EventLog.WriteEntry(sSource, sEvent);
            EventLog.WriteEntry(sSource, sEvent,
                EventLogEntryType.Information, 234);

        }
    }


    public class ServerNeamMapAerial : ServerNearMap 
    {
        public ServerNeamMapAerial()
            : base()
        {
            m_strDefaultUrl = "http://www.nearmap.com/maps/hl=en";
            m_strImageType = ".jpg";
            m_strName = "NearMap Aerial Image";
            m_strUrl = m_strDefaultUrl;
            m_nScaleLo = 0;
            m_nScaleHi = 20;
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

        protected Point GetBitmapCoordinate(PointF ptMercator, int _scale)
        {
            Point ptBitmapCoord = new Point();

            double dZoomCoef = Math.Pow(2.0, m_nScaleHi - _scale);
            double dWidth = 2 * 20037508.342789244;
            double dHeight = dWidth;

            // left-top pixel is at (0, 0), reversed by Y
            ptBitmapCoord.X = (int)Math.Floor(m_nTileSizeX * dZoomCoef / 2.0 + ptMercator.X * m_nTileSizeX * dZoomCoef / dWidth);
            ptBitmapCoord.Y = (int)Math.Floor(m_nTileSizeY * dZoomCoef / 2.0 - ptMercator.Y * m_nTileSizeY * dZoomCoef / dHeight);
            ptBitmapCoord.Y = Math.Max(0, ptBitmapCoord.Y);
            ptBitmapCoord.Y = Math.Min((int)(m_nTileSizeY * dZoomCoef), ptBitmapCoord.Y);
            return ptBitmapCoord;
        }

        #region IServer interface


        public override Boolean DownloadTile(Int32 _x, Int32 _y, Int32 _scale, String _filename)
        {
            // remove whitespace characters from URI
            String strRequest = Regex.Replace(m_strUrl, "\\s", String.Empty);

            // add version
            //int nIndex = strRequest.IndexOf("?");
            //if (nIndex == -1)
            //	strRequest += "?";
            //else if (nIndex < strRequest.Length - 1 && strRequest[strRequest.Length - 1] != '&')
            //	strRequest += "&";
            Int32 TMSscale = m_nScaleHi - _scale;
            // add coordinates and zoom level
            strRequest += "&x=" + Convert.ToString(_x, CultureInfo.InvariantCulture);
            strRequest += "&y=" + Convert.ToString(_y, CultureInfo.InvariantCulture);
            strRequest += "&z=" + Convert.ToString(TMSscale, CultureInfo.InvariantCulture);
            strRequest += "&nml=Vert";
            

            return base.DownloadTile(strRequest, _filename);

        }
        

        // Get the supplied area, in pixels
        public override IRectangle GetRectPixels(Int32 _scale, IRectangleD _rect)
        {
            Point ptLB = GetBitmapCoordinate(new PointF((float)_rect.XMin, (float)_rect.YMin), _scale);
            Point ptRT = GetBitmapCoordinate(new PointF((float)_rect.XMax, (float)_rect.YMax), _scale);

            return new Rectangle(ptLB.X, ptRT.Y, ptRT.X, ptLB.Y);
        }

        // Get the supplied area, in tiles
        public override IRectangle GetRectTiles(Int32 _scale, IRectangleD _rect)
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

        // Get scale names separated by commas
        public override String ScaleNames
        {
            get
            {
                return "0.125m,0.25 m,0.5 m,1 m,2 m,5 m,10 m,20 m,40 m,80 m,160 m,320 m,640 m,1.3 km,2.5 km,5 km,10 km,20 km,40 km,80 km,160 km";
            }
        }

        #endregion
    }





}
