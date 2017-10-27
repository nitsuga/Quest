using System;
using System.Collections.Generic;

namespace Quest.Common.Messages.Device
{
    /// <summary>
    ///     In response to a patient search, this class contains details about a specific patient that
    ///     matches the search criteria.
    /// </summary>
    [Serializable]
    
    public class PatientDetailsResponse : Response
    {
        /// <summary>
        ///     The NHS number of the patient
        /// </summary>
        
        public string NHSNumber { get; set; }

        /// <summary>
        ///     The date of birth of the patient
        /// </summary>
        
        public string DoB { get; set; }

        /// <summary>
        ///     the first name of the patient
        /// </summary>
        
        public string FirstName { get; set; }

        /// <summary>
        ///     The last name of the patient
        /// </summary>
        
        public string LastName { get; set; }

        /// <summary>
        ///     Information relating to this patient
        /// </summary>
        
        public List<string> Notes { get; set; }
    }

    
}