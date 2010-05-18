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
        ServerTMS testServerTMS;
        public TMSTests()
        {
            testServerTMS = new ServerTMS();
        }
        
        [Fact]
        public void ScaleLoTest()
        {
            //var mock = new Mock<ServerTMS>();
            //mock.Setup(ServerTMS => ServerTMS.ScaleHi).Returns(19);
            //private ImageServerTMS.ServerTMS TestTMS = new ImageServerTMS.ServerTMS();
            int expected = 0;
            int actual = testServerTMS.ScaleLo;
            Assert.Equal(expected, actual);


 
        }
    }
}
