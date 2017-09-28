using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Nest;
using Newtonsoft.Json.Linq;
using Quest.Common.Messages;
using Quest.Lib.DataModel;
using Quest.Lib.Search.Elastic;
using Quest.Lib.Utils;
using Quest.Lib.Trace;
using Quest.Common.ServiceBus;
using Quest.Lib.Notifier;
using Quest.Lib.Incident;
using Quest.Lib.Resource;
using PushSharp.Google;
using Quest.Lib.Data;
using PushSharp.Apple;

namespace Quest.Lib.Device
{

    public class CADDeviceRelay
    {
        public System.EventHandler<IncidentUpdate> IncidentUpdate;
        //public System.EventHandler<StatusUpdate> StatusUpdate;

        public CADDeviceRelay()
        {
        }

        /// <summary>
        /// Create a link to target system
        /// </summary>
        public void Prepare()
        {
        }

        public void Login(LoginRequest request, IResourceStore resStore, IDeviceStore devStore)
        {
        }

        public void Logout(LogoutRequest request, IDeviceStore devStore)
        {
        }
    
        public void CallsignChange(CallsignChangeRequest request)
        {
        }

        public void AckAssignedEvent(AckAssignedEventRequest request)
        {
        }

        public void PositionUpdate(PositionUpdateRequest request)
        {
        }

        public void MakePatientObservation(MakePatientObservationRequest request)
        {
        }

        /// <summary>
        ///     Status request by device
        /// </summary>
        /// <param name="request"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public void SetStatusRequest(SetStatusRequest request, NotificationSettings settings)
        {
        }
    }
}