using System;

namespace Quest.Lib.Simulation
{
    // User interface to the MDT
    [ServiceContract]
    public interface IUserOut
    {
        [OperationContract]
        String Version();

        /// <summary>
        /// Ask for the current simulation configuration - causes IUserOut::Configuration to be invoked
        /// </summary>
        [OperationContract]
        Resource[] GetVehicles(DateTime since);

        /// <summary>
        /// randomise the positions of all the ambulances
        /// </summary>
        [OperationContract]
        void RandomisePositions();

        /// <summary>
        /// (re)boot an MDT
        /// </summary>
        /// <param name="ResourceId"></param>
        [OperationContract]
        void StartMDT(int ResourceId);

        /// <summary>
        /// turn off an MDT
        /// </summary>
        /// <param name="ResourceId"></param>
        [OperationContract]
        void ShutdownMDT(int ResourceId, string reason);

        /// <summary>
        /// report an Vehicle is at scene
        /// </summary>
        /// <param name="ResourceId"></param>
        [OperationContract]
        void AtDestination(int ResourceId, DestType destType);

        /// <summary>
        /// user is changing the status on the MDT
        /// </summary>
        /// <param name="ResourceId"></param>
        /// <param name="status"></param>
        [OperationContract]
        void UserStatusChange(int ResourceId, ResourceStatus status, int nonConveyCode, string destHospital);

        /// <summary>
        /// user has asked the Vehicle to navigate to a specific named destination
        /// </summary>
        /// <param name="ResourceId"></param>
        /// <param name="location"></param>
        [OperationContract]
        void NavigateTo(NavigateTo info);

        [OperationContract]
        void SetAcceptCancellation(int ResourceId, bool value);

        [OperationContract]
        void SetStandbyPoint(int ResourceId, string name);
    }

    public enum DestType : int
    {
        Incident=1,
        Hospital=5,
        Other=6,
    }

}
