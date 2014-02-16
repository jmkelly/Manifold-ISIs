using System.Text;
using NUnit.Framework;
using System.Xml;
using System.IO;

namespace Manifold.ImageServer.TMS.Tests
{   
    [TestFixture]
    public class PropertyTests
    {
        TmsServer _server;
        string _coordinateSystem;


        [SetUp]
        public void Init()
        {
            _server = new TmsServer();
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
        }

        [Test]
        public void CoordinateSystemTest()
        {
            Assert.AreEqual(_coordinateSystem, _server.CoordinateSystem);
        }

        [Test]
        public void TileSizeXTest()
        {
            Assert.AreEqual(256, _server.TileSizeX);
        }

        [Test]
        public void TileSizeYTest()
        {
            Assert.AreEqual(256, _server.TileSizeY);
        }

        [Test]
        public void DefaultImageTypeTest()
        {
            Assert.AreEqual(".png", _server.DefaultImageType); 
        }

        [Test]
        public void DefaultUrlTest()
        {
            Assert.AreEqual("http://", _server.DefaultURL);
        }
        [Test]
        public void ScaleHiTest()
        {
            Assert.AreEqual(21, _server.ScaleHi);
        }

        [Test]
        public void ScaleLoTest()
        {
            Assert.AreEqual(0, _server.ScaleLo);
        }

        [Test]
        public void ReverseYTest()
        {
            Assert.AreEqual(false, _server.ReverseY);
        }

        [Test]
        public void CreateTile()
        {
            const int x = 1;
            const int y = 1;
            const int z = 1;
            const string fileName = "c:\\temp\\000.png";
            TileImage t = new TileImage(x, y, z, fileName);
            Assert.AreEqual(1, t.X);
        }

        [Test]
        public void StrRequestTest()
        {
            const string expected = "http://localhost/j5412/21/0/0.png";
                _server.URL = "http://localhost/j5412/";
               string actual = _server.StringRequest(0,0,0);
               Assert.AreEqual(expected, actual);
                }

        [Test]
        public void HttpRequestTest()
        {
            _server.URL = "http://localhost/j5412/";
            string request = _server.StringRequest(0, 0, 0);
            bool actual = _server.httpRequestSuccess(request);
            Assert.AreEqual(true, actual);
        }
        [Test]
        public void HttpResponseTest()
        {

            _server.URL = "http://localhost/j5412/";
            bool actual = _server.HttpResponseSuccess(459,196,12);
            Assert.AreEqual(true, actual);
        }

        [Test]
        public void DownloadTileTest()
        {
            _server.URL = "http://localhost/j5412/";
            bool actual = _server.DownloadTile(459, 196, 12, "c:\\temp\\test.png");
            Assert.AreEqual(true, actual);
        }
        [Test]
        public void TmsScaleTestMax()
        {
            _server.ZoomLevelMaximum = 18;
            const int expected = 3;
            int actual = _server.ScaleLo;
            Assert.AreEqual(expected,actual);
        }
        [Test]
        public void TmsScaleTestMin()
        {
            _server.ZoomLevelMinimum = 3;
            const int expected = 18;
            int actual = _server.ScaleHi;
            Assert.AreEqual(expected, actual);
        }
    }
}
