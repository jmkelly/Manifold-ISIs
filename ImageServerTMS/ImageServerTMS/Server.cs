using System;
using System.Text;
using System.Xml;
using System.IO;
using System.Net;
using System.Globalization;
using System.Drawing;
using System.Drawing.Imaging;

namespace Manifold.ImageServer.TMS
{
    public class TmsServer:IServer
    {
        private string _coordinateSystem;
        string m_DefaultImageType;
        string m_DefaultURL;
        string m_Error;
        string m_Name;
        Int32 m_ScaleHi;
        Int32 m_ScaleLo;
        Int32 m_TileSizeX;
        Int32 m_TileSizeY;
        Boolean m_ReverseY;
        protected String m_strProxyAddress;
        protected String m_strProxyPassword;
        protected String m_strProxyUserName;
        protected String m_strUrl;

        public TmsServer()
        {
            XmlWriterSettings settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Encoding = Encoding.UTF8,
                Indent = true
            };

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

            _coordinateSystem = strWriter.ToString();
            strWriter.Close();
            m_DefaultImageType = ".png";
            m_ReverseY = false;
            m_ScaleHi = 21;
            m_ScaleLo = 0;
            m_TileSizeX = 256;
            m_TileSizeY = 256;
            m_Name = "TMS Tile Coordinates";
            m_DefaultURL = "http://";
            ScaleNames = "0.675m,1.25m,2.5m,5m,10m,20 m,40 m,80 m,160 m,320 m,640 m,1.2km,2.5km,5km,10km,20km,40km,80km,160km,320km,640km,1280km";
        }

        #region IServer Members

        public string CoordinateSystem
        {
            get { 
                return _coordinateSystem; 
            }
        }

        public string DefaultImageType
        {
            get { return m_DefaultImageType; }
        }

        public string DefaultURL
        {
            get { return m_DefaultURL; }
            set { m_DefaultURL = value; }
        }

        public string StringRequest(int x, int y, int scale)
        {
            int z = ScaleHi - scale;
            return URL + z.ToString() + "/" + x.ToString() +"/" + y.ToString() + DefaultImageType;
        }

        public bool httpRequestSuccess(String strRequest)
        {
            try
            {
                HttpWebRequest request = CreateHttpRequest(strRequest, m_strProxyAddress, m_strProxyUserName, m_strProxyPassword);
               
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool HttpResponseSuccess(int x, int y, int scale)
        {
                bool success = false;
                string strRequest = StringRequest(x, y, scale);
                HttpWebRequest request = CreateHttpRequest(strRequest, m_strProxyAddress, m_strProxyUserName, m_strProxyPassword);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                success = true;
                return success;
            }


        protected int ConvertManifoldScaleToZoom(int manifoldScale){
            return  Math.Abs(manifoldScale - 21);
        }

        protected int ConvertZoomToManifoldScale(int zoom)
        {
            return Math.Abs(zoom - 21);
        }

        public int ZoomLevelMaximum { 
            get
            {
                return ConvertManifoldScaleToZoom(m_ScaleLo);
            }
             
            set{
                m_ScaleLo = ConvertZoomToManifoldScale(value);
            } 
        }

        public int ZoomLevelMinimum
        {
            get
            {
                return ConvertManifoldScaleToZoom(m_ScaleHi);
            }
            set
            {
                m_ScaleHi = ConvertZoomToManifoldScale(value);
            }
        }

       


        public virtual bool DownloadTile(int x, int y, int scale, string filename)
        {
            Int32 z = ConvertZoomToManifoldScale(scale);
          
            String strRequest;
            if (m_DefaultURL == m_strUrl)
            {
                //request image from standard request
                TileImage img = new TileImage(x, y, z, filename);
                img.Save();
                return true;

            }
            else
            {
                //custom request
                strRequest = m_strUrl + z.ToString(CultureInfo.InvariantCulture);
                strRequest += "/" + x.ToString(CultureInfo.InvariantCulture);
                strRequest += "/" + y.ToString(CultureInfo.InvariantCulture);
                strRequest += DefaultImageType;
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

                        if (filename != null && filename.Length != 0)
                            bitmap.Save(filename, ImageFormat.Png);

                    }
                    else
                    {
                        // read all data
                        StreamReader objReader = new StreamReader(response.GetResponseStream());
                        String error = objReader.ReadToEnd();
                        objReader.Close();

                        if (error == null || error.Length == 0)
                            error = "Can't obtain image from server";

                        m_Error =  error + " tile:" + x.ToString() + "," + y.ToString() + "," + z.ToString();

                        response.Close();
                        return false;
                    }

                    response.Close();

                    return true;
                }
                catch (WebException we)
                {
                    m_Error = we.Message + " tile:" + x.ToString() + "," + y.ToString() + "," + z.ToString();
                    return false;
                }
                catch (Exception e)
                {
                    m_Error = e.Message + "tile:" + x.ToString() + "," + y.ToString() + "," + z.ToString();
                    throw;
                }
           
            }
           
              
            
       
        }

        public string Error
        {
            get { return m_Error; }
            set { m_Error = value; }
        }
      
        public IRectangle GetRectPixels(int _scale, IRectangleD _rect)
        {
            //_rect is in the coordinate system of the image, in this case mercator spherical projection
            GlobalMercator p = new GlobalMercator();
            //reverse the scale values (Manifold goes the opposite way to TMS / Google / Microsoft)
            Int32 zoom = ScaleHi - _scale;
            Point ptLB = p.GetPixels(new PointD(_rect.XMin, _rect.YMin), zoom);
            Point ptRT = p.GetPixels(new PointD(_rect.XMax, _rect.YMax), zoom);

            // bottom-left pixel is at (0, 0)           
            return new Rectangle(Convert.ToInt32(ptLB.X), Convert.ToInt32(ptLB.Y), Convert.ToInt32(ptRT.X), Convert.ToInt32(ptRT.Y));
        }

        public IRectangle GetRectTiles(int _scale, IRectangleD _rect)
        {
            IRectangle rectFix = GetRectPixels(_scale, _rect);
            rectFix.XMax = rectFix.XMax - 1;
            rectFix.YMax = rectFix.YMax - 1;

            rectFix.XMin = rectFix.XMin / m_TileSizeX;
            rectFix.YMin = rectFix.YMin / m_TileSizeY;
            rectFix.XMax = rectFix.XMax / m_TileSizeX;
            rectFix.YMax = rectFix.YMax / m_TileSizeY;
            return rectFix;
        }

        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
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
            get { return m_ReverseY; }
        }

        public int ScaleHi
        {
            get { return m_ScaleHi; }
        }

        public int ScaleLo
        {
            get { return m_ScaleLo; }
        }

        public string ScaleNames { get; set; }

        public int TileSizeX
        {
            get { return m_TileSizeX; }
        }

        public int TileSizeY
        {
            get { return m_TileSizeY; }
        }

        public string URL
        {
            get { return m_strUrl; }
            set { m_strUrl = value; }
        }

        protected HttpWebRequest CreateHttpRequest(String _url, String _strProxyAddress, String _strProxyUsername, String _strProxyPassword)
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

        #endregion
    }
}
