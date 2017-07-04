using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Quest.Lib.Simulation
{
    /// <summary>
    /// Operations we can carry out on the Vehicle system
    /// </summary>
    [ServiceContract]
    public interface ICadOut
    {
        [OperationContract]
        void Incident(MDTIncident incidentDetails);

        [OperationContract]
        void CancelIncident(CancelIncident CancelIncidentdetails);

        [OperationContract]
        void SendMessage(Message Messagedetails);

        [OperationContract]
        void CallsignUpdate(CallsignUpdate callsignDetails);

        [OperationContract]
        void SetStatus(SetStatus setStatusdetails);

        [OperationContract]
        void NavigateTo(NavigateTo details);

    }

}
