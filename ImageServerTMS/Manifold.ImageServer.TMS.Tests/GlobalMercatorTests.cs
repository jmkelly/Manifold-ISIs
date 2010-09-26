using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Manifold.ImageServer;
using Manifold.ImageServer.TMS;

namespace Manifold.ImageServer.TMS.Tests
{
    [TestFixture]
    class GlobalMercatorTests
    {
        GlobalMercator globalMercator;
        PointD latitudeLongitudePoint;
        PointD mercatorPoint;

        [SetUp]
        public void init()
        {
            globalMercator = new GlobalMercator();
            latitudeLongitudePoint = new PointD(143.068055770704, -38.4537515036619);
            mercatorPoint = new PointD(15926263.117, -4643725.484);
           
        }

        [Test]
        public void TileSizeTest()
        {
            Assert.AreEqual(256, globalMercator.TileSize);
        }

        [Test]
        public void InitialResolutionTest()
        {
            Assert.AreEqual((2 * Math.PI * 6378137) / globalMercator.TileSize, globalMercator.InitialResolution);
        }
        [Test]
        public void OriginShiftTest()
        {
            Assert.AreEqual((2 * Math.PI * 6378137) / 2.0, globalMercator.OriginShift);
        }

        [Test]
        public void MercatorToPixelsX()
        {
            Point expected = new Point(459, 197);
            Point actual = globalMercator.GetPixels(mercatorPoint,1);
            Assert.AreEqual(expected.X,actual.X );
        }

        [Test]
        public void MercatorToPixelsY()
        {
            Point expected = new Point(459, 196);
            Point actual = globalMercator.GetPixels(mercatorPoint, 1);
            Assert.AreEqual(expected.Y, actual.Y);
        }

        public void ResolutionTest()
        {
            Double result = 2 * Math.PI * 6378137 / globalMercator.TileSize  / Math.Pow(2, 0);
            Assert.AreEqual(result,globalMercator.Resolution(0));
        }

        [Test]
        public void LatLonToMercatorX()
        {
            
            PointD expected = new PointD(15926263.117, -4643725.484);
            Assert.AreEqual(expected.X, Math.Round(mercatorPoint.X,3));
           
        }
        [Test]
        public void LatLonToMercatorY()
        {
            PointD expected = new PointD(15926263.117, -4643725.484);
            Assert.AreEqual(expected.Y, Math.Round(mercatorPoint.Y,3));
        }

        [Test]
        public void BitmapPointTestX()
        {
            Point p = new Point(459, 197);
            PointD pM = new PointD(15926263.117, -4643725.484);
            Assert.AreEqual(p.X, globalMercator.GetBitmapCoord(pM, 1).X);
        }
        [Test]
        public void BitmapPointTestY()
        {
            Point p = new Point(459, 197);
            PointD pM = new PointD(15926263.117, -4643725.484);
            Assert.AreEqual(p.Y, globalMercator.GetBitmapCoord(pM, 1).Y);
        }

       

        [Test]
        public void TileXFromPixelTest()
        {

            Point pixelCoordinate = new Point(459, 196);
            Point expected = new Point(1, 0);
            Point actual = globalMercator.GetTile(pixelCoordinate);
            Assert.AreEqual(expected.X, actual.X);
        }
        [Test]
        public void TileYFromPixelTest()
        {

            Point pixelCoordinate = new Point(459, 196);
            Point expected = new Point(1, 0);
            Point actual = globalMercator.GetTile(pixelCoordinate);
            Assert.AreEqual(expected.Y, actual.Y);
        }
    }
}
