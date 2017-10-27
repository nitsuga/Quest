using System;

namespace Quest.Common.Messages.Device
{
    /// <summary>
    ///     the device can submit a patient observation to be associated with this
    ///     event. The details may be passed on to the receiving hospital.
    /// </summary>
    [Serializable]
    
    public class MakePatientObservationRequest : Request
    {
        public MakePatientObservationRequest()
        {
            Observation = new PatientObservation();
        }

        /// <summary>
        ///     a class containing the patient observation details
        /// </summary>
        public PatientObservation Observation { get; set; }

        public override string ToString()
        {
            return ""; // String.Format("EventUpdate EventId={0} Updated={1}", EventId, Updated);
        }
    }

    
}