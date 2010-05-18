using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Moq;
using ImageServerTMS;


namespace Manifold.ImageServer
{
    public class TMSTests

    {
        ServerTMS TestServerTMS;
        public TMSTests()
        {
            TestServerTMS = new ServerTMS();
            TestServerTMS.ProxyAddress = "http://testproxy/";
            TestServerTMS.ProxyUserName = "james";
            TestServerTMS.ProxyPassword = "abc973";
        }
        
        [Fact]
        public void ScaleLoTest()
        {
            //var mock = new Mock<ServerTMS>();
            //mock.Setup(ServerTMS => ServerTMS.ScaleHi).Returns(19);
            //private ImageServerTMS.ServerTMS TestTMS = new ImageServerTMS.ServerTMS();
            int expected = 0;
            int actual = TestServerTMS.ScaleLo;
            Assert.Equal(expected, actual);

        }
        [Fact]
        public void ScaleHiTest()
        {
            int expected = 19;
            int actual = TestServerTMS.ScaleHi;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DefaultImageTypeTest()
        {
            string expected = ".png";
            string actual = TestServerTMS.DefaultImageType;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DefaultURLTest(){
        string expected = "http://tileserver/";
        string actual = TestServerTMS.DefaultURL;
        Assert.Equal(expected, actual);
        }

        [Fact]
        public void ProxyAddressTest() {
            string expected = "http://testproxy/";
            string actual = TestServerTMS.ProxyAddress;
            Assert.Equal(expected, actual);

        }

        [Fact]
        public void ProxyUsernameTest() {
            string expected = "james";
            string actual = TestServerTMS.ProxyUserName ;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ProxyPasswordTest()
        {
            string expected = "abc973";
            string actual = TestServerTMS.ProxyPassword ;
            Assert.Equal(expected, actual); 
        }

    }
}
