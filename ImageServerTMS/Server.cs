using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manifold.ImageServer;

namespace ImageServerTMS
{
    public class ServerTMS:IServer 
    {
        int _ScaleHi;
        int _ScaleLo;
        
        //constructor
        public ServerTMS()
        {
            _ScaleHi = 19;
            _ScaleLo = 0;


        }


        #region IServer Members

        public string CoordinateSystem
        {
            get { throw new NotImplementedException(); }
        }

        public string DefaultImageType
        {
            get { throw new NotImplementedException(); }
        }

        public string DefaultURL
        {
            get { throw new NotImplementedException(); }
        }

        public bool DownloadTile(int _x, int _y, int _scale, string _filename)
        {
            throw new NotImplementedException();
        }

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public IRectangle GetRectPixels(int _scale, IRectangleD _rect)
        {
            throw new NotImplementedException();
        }

        public IRectangle GetRectTiles(int _scale, IRectangleD _rect)
        {
            throw new NotImplementedException();
        }

        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public string ProxyAddress
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string ProxyPassword
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string ProxyUserName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool ReverseY
        {
            get { throw new NotImplementedException(); }
        }

        public int ScaleHi
        {
            get { throw new NotImplementedException(); }
        }

        public int ScaleLo
        {
            get { throw new NotImplementedException(); }
        }

        public string ScaleNames
        {
            get { throw new NotImplementedException(); }
        }

        public int TileSizeX
        {
            get { throw new NotImplementedException(); }
        }

        public int TileSizeY
        {
            get { throw new NotImplementedException(); }
        }

        public string URL
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }


}
