using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manifold.ImageServer;

namespace ImageServerTMS
{
    public class ServerTMS:IServer 
    {
        protected int _ScaleHi;
        protected int _ScaleLo;
        protected string _DefaultImageType;
        protected string _DefaultURL;
        protected String _ProxyAddress;
        protected String _ProxyPassword;
        protected String _ProxyUserName;
        
        //constructor
        public ServerTMS()
        {
            _ScaleHi = 19;
            _ScaleLo = 0;
            _DefaultImageType = ".png";
            _DefaultURL = "http://tileserver/";



        }


        #region IServer Members

        public string CoordinateSystem
        {
            get { throw new NotImplementedException(); }
        }

        public string DefaultImageType
        {
            get { return _DefaultImageType; }
        }

        public string DefaultURL
        {
            get { return _DefaultURL; }
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
                return _ProxyAddress ;
            }
            set
            {
                _ProxyAddress = value;
            }
        }

        public string ProxyPassword
        {
            get
            {
                return _ProxyPassword ;
            }
            set
            {
                _ProxyPassword = value;
            }
        }

        public string ProxyUserName
        {
            get
            {
                return _ProxyUserName;
            }
            set
            {
                _ProxyUserName = value;
            }
        }

        public bool ReverseY
        {
            get { throw new NotImplementedException(); }
        }

        public int ScaleHi
        {
            get { return _ScaleHi; }
        }

        public int ScaleLo
        {
            get { return _ScaleLo; }
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
