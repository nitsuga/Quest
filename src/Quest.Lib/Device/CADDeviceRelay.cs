using Quest.Common.Messages;
using Quest.Common.Messages.Device;
using Quest.Common.Messages.Incident;
using Quest.Lib.Notifier;
using Quest.Lib.Resource;

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