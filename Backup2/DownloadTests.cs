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
    class DownloadTests
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
        public void DownloadTileTest()
        {
            _Server.URL = "http://localhost/j5412/";
            Assert.AreEqual(true, _Server.DownloadTile(459, 196, 12, "c:\\temp\\temp.png"));
        }
    }

}
