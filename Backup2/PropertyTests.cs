using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Manifold.ImageServer;
using Manifold.ImageServer.TMS;
using System.Xml;
using System.IO;

namespace Manifold.ImageServer.TMS.Tests
{   


    [TestFixture]
    public class PropertyTests
    {
        ServerTMS _Server;
        string _CoordinateSystem;


        [SetUp]
        public void init()
        {
            _Server = new ServerTMS();
            //TileSizeX = 256;
            //TileSizeY = 256;

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

            _CoordinateSystem = strWriter.ToString();
            strWriter.Close();
        }

        [Test]
        public void CoordinateSystemTest()
        {
            Assert.AreEqual(_CoordinateSystem, _Server.CoordinateSystem);
        }

        [Test]
        public void TileSizeXTest()
        {
            Assert.AreEqual(256, _Server.TileSizeX);
        }

        [Test]
        public void TileSizeYTest()
        {
            Assert.AreEqual(256, _Server.TileSizeY);
        }

        [Test]
        public void DefaultImageTypeTest()
        {
            Assert.AreEqual(".png", _Server.DefaultImageType); 
        }

        [Test]
        public void DefaultURLTest()
        {
            Assert.AreEqual("http://", _Server.DefaultURL);
        }
        [Test]
        public void ScaleHiTest()
        {
            Assert.AreEqual(21, _Server.ScaleHi);
        }

        [Test]
        public void ScaleLoTest()
        {
            Assert.AreEqual(0, _Server.ScaleLo);
        }

        [Test]
        public void ReverseYTest()
        {
            Assert.AreEqual(false, _Server.ReverseY);
        }

        [Test]
        public void CreateTile()
        {
            int x = 1;
            int y = 1;
            int z = 1;
            string fileName = "c:\\temp\\000.png";
            TileImage t = new TileImage(x, y, z, fileName);
            Assert.AreEqual(1, t.X);
            
        }

        [Test]
        public void StrRequestTest()
        {
            string expected = "http://localhost/j5412/21/0/0.png";

                _Server.URL = "http://localhost/j5412/";
               string actual = _Server.StringRequest(0,0,0);
               Assert.AreEqual(expected, actual);
                }

        [Test]
        public void HttpRequestTest()
        {
            

            _Server.URL = "http://localhost/j5412/";
            string request = _Server.StringRequest(0, 0, 0);
            bool actual = _Server.httpRequestSuccess(request);
            Assert.AreEqual(true, actual);
        }
        [Test]
        public void HttpResponseTest()
        {

            _Server.URL = "http://localhost/j5412/";
            bool actual = _Server.HttpResponseSuccess(459,196,12);
            Assert.AreEqual(true, actual);
        }

        [Test]
        public void DownloadTileTest()
        {
            _Server.URL = "http://localhost/j5412/";
            bool actual = _Server.DownloadTile(459, 196, 12, "c:\\temp\\test.png");
            bool expected = true;
            Assert.AreEqual(expected, actual);
        }
        [Test]
        public void TMSScaleTestMax()
        {
            _Server.ZoomLevelMaximum = 18;
            Int32 expected = 3;
            int actual = _Server.ScaleLo;
            Assert.AreEqual(expected,actual);
        }
        [Test]
        public void TMSScaleTestMin()
        {
            _Server.ZoomLevelMinimum = 3;
            Int32 expected = 18;
            int actual = _Server.ScaleHi;
            Assert.AreEqual(expected, actual);
        }

            
        

       //     Assert.AreEqual(459, p.X);
        //}
        //[Test]
        //public void GetBitmapCoordinateYTest()
        //{
        //    PointD pM = new PointD(15926263.117, -4643725.484);
        //    Point p = _Server.GetBitmapCoordinate(pM, 19);
        //    Assert.AreEqual(196, p.Y);
        //}  

        

            
    }
}
