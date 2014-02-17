using System;
using Manifold.ImageServer.OpenStreetMaps;
using NUnit.Framework;
using Shouldly;

namespace OpenStreetMapsTests
{
    [TestFixture]
    public class OpenStreetMapTests
    {

        [Test]
        public void FirstTest()
        {
            true.ShouldBe(true);
        }

        [Test]
        public void CanCreateNewServer()
        {
            Should.NotThrow(() => new MapnikServer());
        }

        [Test]
        public void CanFindLog()
        {
            var server = new MapnikServer();
            server.Log.FileName.ShouldNotBe(string.Empty);
            Console.WriteLine(server.Log.FileName);
        }

    }
}
