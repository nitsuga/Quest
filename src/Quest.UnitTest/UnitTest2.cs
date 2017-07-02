using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Quest.UnitTest
{
    [TestClass]
    public class UnitTest1
    {

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            Common.ClassInit(context);
        }

    }
}
