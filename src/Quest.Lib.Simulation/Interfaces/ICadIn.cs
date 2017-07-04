namespace Quest.Lib.Simulation
{
    public partial class Login : System.EventArgs { }
    public partial class Logout : System.EventArgs { }
    public partial class SATNAVLocation : System.EventArgs { }
    public partial class StatusChange : System.EventArgs { }
    public partial class AtDestination : System.EventArgs { }
    public partial class RejectCancelIncident : System.EventArgs { }
    public partial class SkillLevel : System.EventArgs { }

    /// <summary>
    /// Operations we can carry out on the CAD system
    /// </summary>
    [ServiceContract]
    public interface ICadIn
    {
        event System.EventHandler<Incident> NewIncidentEvent;
        event System.EventHandler<Incident> UpdateIncidentEvent;
        event System.EventHandler<Incident> CloseIncidentEvent;
        event System.EventHandler<Incident> UpdateAMPDSEvent;
        event System.EventHandler<StatusChange> StatusChangeEvent;
        event System.EventHandler<AtDestination> AtDestinationEvent;
        event System.EventHandler<Login> LoginEvent;
        event System.EventHandler<Logout> LogoutEvent;
        event System.EventHandler<RejectCancelIncident> RejectCancelIncidentEvent;
        event System.EventHandler<SkillLevel> SkillLevelEvent;
        event System.EventHandler<SATNAVLocation> SATNAVLocationEvent;
        
        bool EnableCoverage { get; set; }

        [OperationContract]
        void Newincident(Incident incident);

        [OperationContract]
        void UpdateLocation(Incident incident);

        [OperationContract]
        void UpdateAMPDS(Incident incident);

        [OperationContract]
        void CloseIncident(Incident incident);

        /// <summary>
        /// Assign a vehicle to the given incident
        /// </summary>
        /// <param name="IncidentKey"></param>
        /// <param name="Callsign"></param>
        [OperationContract]
        void AssignVehicle(long IncidentId, int ResourceId);

        /// <summary>
        /// Cancel a vehicle from its current incident
        /// </summary>
        /// <param name="Callsign"></param>
        [OperationContract]
        void CancelVehicle(long IncidentId, int ResourceId);

        [OperationContract]
        void StatusChange(StatusChange newStatus);

        [OperationContract]
        void AtDestination(AtDestination destinationDetails);
       
        [OperationContract]
        void Login(Login loginDetails);

        [OperationContract]
        void Logout(Logout logoutDetails);

        [OperationContract]
        void RejectCancelIncident(RejectCancelIncident rejectCancellation);

        [OperationContract]
        void SkillLevel(SkillLevel skillLevelDetails);

        [OperationContract]
        void SATNAVLocation(SATNAVLocation avlsDetails);

        [OperationContract]
        void NavigateTo(NavigateTo details);
    }
}
