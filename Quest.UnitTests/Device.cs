using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quest.Common.Messages;
using Quest.Common.ServiceBus;
using Quest.Lib.Device;
using Quest.Lib.Resource;
using System;
using System.Text;

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
        public void Device_Test2()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var resHandler = Common.ApplicationContainer.Resolve<ResourceHandler>();
            var serviceBusClient = Common.ApplicationContainer.Resolve<IServiceBusClient>();

            serviceBusClient.Initialise("Test");

            ResourceUpdate newresource = new ResourceUpdate
            {
                Resource = new QuestResource
                {
                    Callsign = $"#0000",
                    FleetNo = $"DEV-0000",
                    Position = new GeoAPI.Geometries.Coordinate(0, 0),
                    ResourceType = "UNK",
                    Status = "OFF"
                },
                UpdateTime = DateTime.UtcNow
            };

            var updateResult = resHandler.ResourceUpdate(newresource, serviceBusClient, null);

        }
        [TestMethod]
        public void Device_Test1()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //var scope = Common.ApplicationContainer.BeginLifetimeScope();
            var deviceHandler = Common.ApplicationContainer.Resolve<DeviceHandler>();
            var serviceBusClient = Common.ApplicationContainer.Resolve<IServiceBusClient>();

            serviceBusClient.Initialise("Test");

            LoginRequest request = new LoginRequest()
            {  
                Username ="fred",
                DeviceIdentity = "unknown-000000000000000-a02f167ca32d28a9",
                DeviceMake= "samsung",
                DeviceModel = "GT-N8010",
                FleetNo="1000",
                Locale ="en-GB",
                NotificationId = "APA91bFaO1_1hIwgVo_R3qFD9QWmj6ZsTDUl0lzfAMnxK16XP0-Asdm7ELeuP2PvaD9ZDONKzrfXC9asOxDC8NQmH6DbNPpOHxYeXpSba6gDAI25TU6QrO75sZrUfzB_8aNtgzWsDand",
                NotificationTypeId ="GCM", 
                OSVersion="",
                QuestApi=1,
                RequestId="",
                SessionId ="",
                Timestamp =DateTime.Now.Ticks                   
             };

            var result = deviceHandler.Login(request, serviceBusClient);
        }
    }
}
