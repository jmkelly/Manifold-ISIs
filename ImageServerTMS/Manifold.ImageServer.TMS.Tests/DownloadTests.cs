using System.Text;
using NUnit.Framework;
using System.Xml;
using System.IO;

namespace Manifold.ImageServer.TMS.Tests
{
    class DownloadTests
    {
        TmsServer _server;
        string _coordinateSystem;

        [SetUp]
        public void Init()
        {
            _server = new TmsServer();
            //TileSizeX = 256;
            //TileSizeY = 256;

            // save coordinate system in xml format
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
        public void DownloadTileTest()
        {
            _server.URL = "http://localhost/j5412/";
            Assert.AreEqual(true, _server.DownloadTile(459, 196, 12, "c:\\temp\\temp.png"));
        }
    }

}
