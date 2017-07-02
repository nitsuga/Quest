using Quest.Common.Messages;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Web.Mvc;


namespace Quest.Mobile.Models
{
    [DataContract]
    [Serializable]
    public class PositionUpdateRequestWrapper
    {
        public PositionUpdateRequestWrapper()
        {
            Request = new PositionUpdateRequest();
        }

        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public PositionUpdateRequest Request { get; set; }
    }

    [Serializable]
    public class TestMessage
    {
        public List<SelectListItem> StatusGroupCodes { get; set; }

        public string AuthToken { get; set; }
        public LoginRequest LoginRequest { get; set; }
        public LogoutRequest LogoutRequest { get; set; }
        public CallsignChangeRequest CallsignChangeRequest { get; set; }
        public RefreshStateRequest GetAssignedEventRequest { get; set; }
        public PositionUpdateRequestWrapper PositionUpdateRequest { get; set; }
        public MakePatientObservationRequest MakePatientObservationRequest { get; set; }
        public PatientDetailsRequest PatientDetailsRequest { get; set; }
        public AckAssignedEventRequest AckAssignedEventRequest { get; set; }
        public SetStatusRequest SetStatusRequest { get; set; }
        public GetStatusCodesRequest GetStatusCodesRequest { get; set; }
        public GetEntityTypesRequest GetEntityTypesRequest { get; set; }
        public GetHistoryRequest GetHistoryRequest { get; set; }
        public CallEvent CallEvent { get; set; }
        public CallLookupRequest CallDetails { get; set; }
        public CallEnd CallEnd { get; set; }

        public string Request { get; set; }
        public string Result { get; set; }

        public TestMessage()
        {
            LoginRequest = new LoginRequest();
            LogoutRequest = new LogoutRequest();
            CallsignChangeRequest = new CallsignChangeRequest();
            GetAssignedEventRequest = new RefreshStateRequest();
            PositionUpdateRequest = new PositionUpdateRequestWrapper();
            MakePatientObservationRequest = new MakePatientObservationRequest();
            PatientDetailsRequest = new PatientDetailsRequest();
            AckAssignedEventRequest = new AckAssignedEventRequest();
            SetStatusRequest = new SetStatusRequest();
            GetStatusCodesRequest = new GetStatusCodesRequest();
            GetEntityTypesRequest = new GetEntityTypesRequest();
            GetHistoryRequest = new GetHistoryRequest();
        }

        public void MakeTestMessage()
        {
            LoginRequest = new LoginRequest() { QuestApi = "1.0.0", Locale = "en-GB", Username = "marcus", Password = "secret", DeviceIdentity = "SOMEIDENTITY", NotificationTypeId = 2, NotificationId = "GCM TOKEN" };
            CallsignChangeRequest = new CallsignChangeRequest() { Callsign = "G460" };
        }
    }
}