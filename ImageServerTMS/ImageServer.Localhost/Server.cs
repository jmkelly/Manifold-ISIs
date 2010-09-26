using System;
using System.Collections.Generic;
using System.Text;
using Manifold.ImageServer.TMS;
using System.Xml;
using System.IO;
using System.Net;
using System.Globalization;
using System.Drawing;
using System.Drawing.Imaging;

namespace Manifold.ImageServer.Localhost
{
    public class ServerLocalhost :ServerTMS  
    {
        public ServerLocalhost()
            : base()
        {
            DefaultURL  = "http://localhost/j5412/";
            Name = "Localhost TMS";
   
        }

        public override bool DownloadTile(int _x, int _y, int _scale, string _filename)
        {
            Int32 z = base.ConvertZoomToManifoldScale(_scale);
            if (_x == 0 && _y == 0)
            {
                TileImage img = new TileImage(_x, _y, z, _filename);
                return true;
            }
                        
            String strRequest;

            //custom request
            strRequest = m_strUrl + z.ToString(CultureInfo.InvariantCulture);
            strRequest += "/" + _x.ToString(CultureInfo.InvariantCulture);
            strRequest += "/" + _y.ToString(CultureInfo.InvariantCulture);
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

                    Error = error + strRequest ;

                    response.Close();
                    return false;
                }

                response.Close();

                return true;
            }
            catch (WebException we)
            {
                Error = we.Message + strRequest;
                return false;
            }
            catch (Exception e)
            {
                Error = e.Message + strRequest;
                throw;
            }

        }
    }

}
