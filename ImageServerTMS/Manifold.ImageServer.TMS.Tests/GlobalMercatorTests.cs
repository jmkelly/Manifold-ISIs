using System;
using NUnit.Framework;

namespace Manifold.ImageServer.TMS.Tests
{
    [TestFixture]
    class GlobalMercatorTests
    {
        GlobalMercator _globalMercator;
        PointD _latitudeLongitudePoint;
        PointD _mercatorPoint;

        [SetUp]
        public void Init()
        {
            _globalMercator = new GlobalMercator();
            _latitudeLongitudePoint = new PointD(143.068055770704, -38.4537515036619);
            _mercatorPoint = new PointD(15926263.117, -4643725.484);
           
        }

        [Test]
        public void TileSizeTest()
        {
            Assert.AreEqual(256, _globalMercator.TileSize);
        }

        [Test]
        public void InitialResolutionTest()
        {
            Assert.AreEqual((2 * Math.PI * 6378137) / _globalMercator.TileSize, _globalMercator.InitialResolution);
        }
        [Test]
        public void OriginShiftTest()
        {
            Assert.AreEqual((2 * Math.PI * 6378137) / 2.0, _globalMercator.OriginShift);
        }

        [Test]
        public void MercatorToPixelsX()
        {
            Point expected = new Point(459, 197);
            Point actual = _globalMercator.GetPixels(_mercatorPoint,1);
            Assert.AreEqual(expected.X,actual.X );
        }

        [Test]
        public void MercatorToPixelsY()
        {
            Point expected = new Point(459, 196);
            Point actual = _globalMercator.GetPixels(_mercatorPoint, 1);
            Assert.AreEqual(expected.Y, actual.Y);
        }

        public void ResolutionTest()
        {
            Double result = 2 * Math.PI * 6378137 / _globalMercator.TileSize  / Math.Pow(2, 0);
            Assert.AreEqual(result,_globalMercator.Resolution(0));
        }

        [Test]
        public void LatLonToMercatorX()
        {
            
            PointD expected = new PointD(15926263.117, -4643725.484);
            Assert.AreEqual(expected.X, Math.Round(_mercatorPoint.X,3));
           
        }
        [Test]
        public void LatLonToMercatorY()
        {
            PointD expected = new PointD(15926263.117, -4643725.484);
            Assert.AreEqual(expected.Y, Math.Round(_mercatorPoint.Y,3));
        }

        [Test]
        public void BitmapPointTestX()
        {
            Point p = new Point(459, 197);
            PointD pM = new PointD(15926263.117, -4643725.484);
            Assert.AreEqual(p.X, _globalMercator.GetBitmapCoord(pM, 1).X);
        }
        [Test]
        public void BitmapPointTestY()
        {
            Point p = new Point(459, 197);
            PointD pM = new PointD(15926263.117, -4643725.484);
            Assert.AreEqual(p.Y, _globalMercator.GetBitmapCoord(pM, 1).Y);
        }

       

        [Test]
        public void TileXFromPixelTest()
        {

            Point pixelCoordinate = new Point(459, 196);
            Point expected = new Point(1, 0);
            Point actual = _globalMercator.GetTile(pixelCoordinate);
            Assert.AreEqual(expected.X, actual.X);
        }
        [Test]
        public void TileYFromPixelTest()
        {

            Point pixelCoordinate = new Point(459, 196);
            Point expected = new Point(1, 0);
            Point actual = _globalMercator.GetTile(pixelCoordinate);
            Assert.AreEqual(expected.Y, actual.Y);
        }
    }
}
