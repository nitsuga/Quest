using System;

namespace Quest.Common.Messages.Device
{
    /// <summary>
    ///     The device can ask for a database search for this patient.
    /// </summary>
    [Serializable]
    
    public class PatientDetailsRequest : Request
    {
        /// <summary>
        ///     The NHS number of the patient if known
        /// </summary>
        
        public string NHSNumber { get; set; }

        /// <summary>
        ///     The date of birth of the patient if known or approximate age
        /// </summary>
        
        public string DoB { get; set; }

        /// <summary>
        ///     The first name of the patient if known
        /// </summary>
        
        public string FirstName { get; set; }

        /// <summary>
        ///     The lastname/surname of the patient if known
        /// </summary>
        
        public string LastName { get; set; }

        public override string ToString()
        {
            return ""; // String.Format("EventUpdate EventId={0} Updated={1}", EventId, Updated);
        }
    }

    
}