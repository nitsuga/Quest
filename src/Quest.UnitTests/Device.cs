using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quest.Common.Messages;
using Quest.Common.Messages.Device;
using Quest.Common.Messages.Entities;
using Quest.Common.Messages.GIS;
using Quest.Common.ServiceBus;
using Quest.Lib.Device;
using Quest.Lib.Resource;
using System;
using System.Text;

namespace Quest.UnitTests
{
    [TestClass]
    public class DeviceUnitTest
    {
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            Common.Init();
        }

        [TestMethod]
        public void Device_01_Logon()
        {
            var result = Login();
            Assert.IsNotNull(result);
        }

        string Login()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //var scope = Common.ApplicationContainer.BeginLifetimeScope();
            var deviceHandler = Common.ApplicationContainer.Resolve<DeviceHandler>();
            var serviceBusClient = Common.ApplicationContainer.Resolve<IServiceBusClient>();

            serviceBusClient.Initialise("Test");

            //LoginRequest request = new LoginRequest()
            //{
            //    Username = "fred",
            //    DeviceIdentity = "unknown-000000000000000-a02f167ca32d28a9",
            //    DeviceMake = "samsung",
            //    DeviceModel = "GT-N8010",
            //    FleetNo = "1000",
            //    Locale = "en-GB",
            //    NotificationId = "APA91bFaO1_1hIwgVo_R3qFD9QWmj6ZsTDUl0lzfAMnxK16XP0-Asdm7ELeuP2PvaD9ZDONKzrfXC9asOxDC8NQmH6DbNPpOHxYeXpSba6gDAI25TU6QrO75sZrUfzB_8aNtgzWsDand",
            //    NotificationTypeId = "GCM",
            //    OSVersion = "",
            //    QuestApi = 1,
            //    RequestId = "",
            //    SessionId = "",
            //};

            LoginRequest request = new LoginRequest()
            {
                Username = null,
                DeviceIdentity = "031603e207913602",
                DeviceMake = "Unknown",
                DeviceModel = "SM-G920F",
                FleetNo = null,
                Locale = "en-GB",
                NotificationId = "dtBtKZd1q3M:APA91bGZkpF_8iZH5iCQ3pWUGqfdMZkJaZkolJaDZ6CVM1XFk1wd190EyZ65ufvA0ZCaqMOxZc28F9WoGwCrOZw6Ty2f0Rad5EY5fSWOn9gWW57jJFtoiKgC0BMi0-GZNWZRDcHSuNhF",
                NotificationTypeId = "2",
                OSVersion = "7.0",
                QuestApi = 1,
                RequestId = "",
                SessionId = "",
            };

            var result = deviceHandler.Login(request, serviceBusClient);
            Assert.IsTrue(result.Success);
            return result.SessionId;
        }


        [TestMethod]
        public void Device_02_RefreshStateRequest()
        {
            var sessionid = Login();

            Assert.IsNotNull(sessionid);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //var scope = Common.ApplicationContainer.BeginLifetimeScope();
            var deviceHandler = Common.ApplicationContainer.Resolve<DeviceHandler>();
            var serviceBusClient = Common.ApplicationContainer.Resolve<IServiceBusClient>();

            serviceBusClient.Initialise("Test");

            RefreshStateRequest request = new RefreshStateRequest()
            {
                RequestId = "",
                SessionId = sessionid,
            };

            var result = deviceHandler.RefreshState(request, serviceBusClient);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void Device_03_Logout()
        {
            var sessionid = Login();

            Assert.IsNotNull(sessionid);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //var scope = Common.ApplicationContainer.BeginLifetimeScope();
            var deviceHandler = Common.ApplicationContainer.Resolve<DeviceHandler>();
            var serviceBusClient = Common.ApplicationContainer.Resolve<IServiceBusClient>();

            serviceBusClient.Initialise("Test");

            LogoutRequest request = new LogoutRequest()
            { 
                RequestId = "",
                SessionId = sessionid,
            };

            var result = deviceHandler.Logout(request);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void Device_04_AckAssignedEvent_EmptyEvent()
        {
            var sessionid = Login();

            Assert.IsNotNull(sessionid);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //var scope = Common.ApplicationContainer.BeginLifetimeScope();
            var deviceHandler = Common.ApplicationContainer.Resolve<DeviceHandler>();
            var serviceBusClient = Common.ApplicationContainer.Resolve<IServiceBusClient>();

            serviceBusClient.Initialise("Test");

            AckAssignedEventRequest request = new AckAssignedEventRequest()
            {
                RequestId = "",
                SessionId = sessionid,
            };

            var result = deviceHandler.AckAssignedEvent(request);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success, result.Message);
        }

        [TestMethod]
        public void Device_04_AckAssignedEvent_BadEvent()
        {
            var sessionid = Login();

            Assert.IsNotNull(sessionid);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //var scope = Common.ApplicationContainer.BeginLifetimeScope();
            var deviceHandler = Common.ApplicationContainer.Resolve<DeviceHandler>();
            var serviceBusClient = Common.ApplicationContainer.Resolve<IServiceBusClient>();

            serviceBusClient.Initialise("Test");

            AckAssignedEventRequest request = new AckAssignedEventRequest()
            {
                Accept=true,
                RequestId = "",
                SessionId = sessionid,
                EventId="some garbage"
            };

            var result = deviceHandler.AckAssignedEvent(request);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success, result.Message);
        }


        [TestMethod]        
        public void Device_06_GetHistoryRequest()
        {
            var sessionid = Login();

            Assert.IsNotNull(sessionid);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //var scope = Common.ApplicationContainer.BeginLifetimeScope();
            var deviceHandler = Common.ApplicationContainer.Resolve<DeviceHandler>();
            var serviceBusClient = Common.ApplicationContainer.Resolve<IServiceBusClient>();

            serviceBusClient.Initialise("Test");

            GetHistoryRequest request = new GetHistoryRequest()
            {
                RequestId = "",
                SessionId = sessionid,
            };

            var result = deviceHandler.GetHistory(request);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
        }



        [TestMethod]
        public void Device_08_MakePatientObservationRequest()
        {
            var sessionid = Login();

            Assert.IsNotNull(sessionid);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //var scope = Common.ApplicationContainer.BeginLifetimeScope();
            var deviceHandler = Common.ApplicationContainer.Resolve<DeviceHandler>();
            var serviceBusClient = Common.ApplicationContainer.Resolve<IServiceBusClient>();

            serviceBusClient.Initialise("Test");

            MakePatientObservationRequest request = new MakePatientObservationRequest()
            {
                RequestId = "",
                SessionId = sessionid,
            };

            var result = deviceHandler.MakePatientObservation(request);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void Device_10_PatientDetailsRequest_NotImplemented()
        {
            var sessionid = Login();

            Assert.IsNotNull(sessionid);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //var scope = Common.ApplicationContainer.BeginLifetimeScope();
            var deviceHandler = Common.ApplicationContainer.Resolve<DeviceHandler>();
            var serviceBusClient = Common.ApplicationContainer.Resolve<IServiceBusClient>();

            serviceBusClient.Initialise("Test");

            PatientDetailsRequest request = new PatientDetailsRequest()
            {
                RequestId = "",
                SessionId = sessionid,
            };

            var result = deviceHandler.PatientDetails(request);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void Device_11_PositionUpdateRequest()
        {
            var sessionid = Login();

            Assert.IsNotNull(sessionid);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //var scope = Common.ApplicationContainer.BeginLifetimeScope();
            var deviceHandler = Common.ApplicationContainer.Resolve<DeviceHandler>();
            var serviceBusClient = Common.ApplicationContainer.Resolve<IServiceBusClient>();

            serviceBusClient.Initialise("Test");

            PositionUpdateRequest request = new PositionUpdateRequest()
            {
                RequestId = "",
                SessionId = sessionid, Vector = new LocationVector
                {
                     Altitude=200,
                     Course=45,
                     CaptureMethod="GPS",
                     HDoP=20, Coord = new LatLongCoord
                     {
                         Latitude = 51.15254,
                         Longitude = -0.187382
                     },
                     Speed=6.76,
                     VDoP=22
                }
            };

            var result = deviceHandler.PositionUpdate(request, serviceBusClient);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void Device_12_SetStatusRequest()
        {
            var sessionid = Login();

            Assert.IsNotNull(sessionid);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //var scope = Common.ApplicationContainer.BeginLifetimeScope();
            var deviceHandler = Common.ApplicationContainer.Resolve<DeviceHandler>();
            var serviceBusClient = Common.ApplicationContainer.Resolve<IServiceBusClient>();

            serviceBusClient.Initialise("Test");

            SetStatusRequest request = new SetStatusRequest()
            {
                RequestId = "",
                SessionId = sessionid,
            };

            var result = deviceHandler.SetStatusRequest(request, serviceBusClient);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
        }



    }
}
