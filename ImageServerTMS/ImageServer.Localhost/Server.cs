using System;
using Manifold.ImageServer.TMS;
using System.IO;
using System.Net;
using System.Globalization;
using System.Drawing;
using System.Drawing.Imaging;

namespace Manifold.ImageServer.Localhost
{
    public class ServerLocalhost :TmsServer  
    {
        public ServerLocalhost()
        {
            DefaultURL  = "http://localhost/j5412/";
            Name = "Localhost TMS";
   
        }

        public override bool DownloadTile(int x, int y, int scale, string filename)
        {
            Int32 z = ConvertZoomToManifoldScale(scale);
            if (x == 0 && y == 0)
            {
                var image = new TileImage(x, y, z, filename);
                image.Save();

                return true;
            }

            //custom request
            string strRequest = m_strUrl + z.ToString(CultureInfo.InvariantCulture);
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
                    Bitmap bitmap = new Bitmap(image);

                    if (!string.IsNullOrEmpty(filename))
                        bitmap.Save(filename, ImageFormat.Png);

                }
                else
                {
                    // read all data
                    StreamReader objReader = new StreamReader(response.GetResponseStream());
                    String error = objReader.ReadToEnd();
                    objReader.Close();

                    if (error.Length == 0)
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
