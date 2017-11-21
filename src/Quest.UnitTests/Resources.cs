using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quest.Common.Messages.Resource;
using Quest.Common.ServiceBus;
using Quest.Lib.Resource;
using System;
using System.Text;

namespace Quest.UnitTests
{
    [TestClass]
    public class ResourceUnitTest
    {
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            Common.Init();
        }

        [TestMethod]
        public void Resource_01_ResourceUpdate()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var resHandler = Common.ApplicationContainer.Resolve<ResourceHandler>();
            var serviceBusClient = Common.ApplicationContainer.Resolve<IServiceBusClient>();

            serviceBusClient.Initialise("Test");

            ResourceUpdateRequest newresource = new ResourceUpdateRequest
            {
                Resource = new QuestResource
                {
                    Callsign = $"C1000",
                    FleetNo = $"1000",
                    Position = new  Quest.Common.Messages.GIS.LatLongCoord(0, 0),
                    ResourceType = "UNK",
                    Status = "OFF"
                },
                UpdateTime = DateTime.UtcNow
            };

            var result = resHandler.ResourceUpdate(newresource, serviceBusClient, null);

            Assert.IsNotNull(result);
        }

    }
}
