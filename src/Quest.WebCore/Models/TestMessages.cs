﻿using Microsoft.AspNetCore.Mvc.Rendering;
using Quest.Common.Messages.Device;
using Quest.Common.Messages.Telephony;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Quest.WebCore.Services
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
            GetHistoryRequest = new GetHistoryRequest();
        }

        public void MakeTestMessage()
        {
            LoginRequest = new LoginRequest() { QuestApi = 1, Locale = "en-GB", Username = "marcus", DeviceIdentity = "SOMEIDENTITY", NotificationTypeId = "GCM", NotificationId = "GCM TOKEN" };
            CallsignChangeRequest = new CallsignChangeRequest() { Callsign = "G460" };
        }
    }
}