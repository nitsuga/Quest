using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Quest.UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [ClassInitialize]
        public static void Device_Init(TestContext context)
        {
            Common.Init();
        }

        [TestMethod]
        public void Device_Test1()
        {
        }
    }
}
