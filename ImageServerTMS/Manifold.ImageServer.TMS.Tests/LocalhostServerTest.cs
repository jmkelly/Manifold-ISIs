using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Manifold.ImageServer;
using System.Xml;
using System.IO;
using Manifold.ImageServer.Localhost;


namespace Manifold.ImageServer.TMS.Tests
{
    class LocalhostServerTest
    {
        

        ServerLocalhost server;

        [SetUp]
        public void init()
        {
            server = new ServerLocalhost();
            //TileSizeX = 256;
            //TileSizeY = 256;

            // save coordinate system in xml format
           
        }

        [Test]
        public void scaleLoTest()
        {
            int expected = 0;
            int actual = server.ScaleLo;
            Assert.AreEqual(expected, actual);
        }
        [Test]
        public void scaleHiTest()
        {
            int expected = 21;
            int actual = server.ScaleHi;
            Assert.AreEqual(expected, actual);
        }

    }
}
