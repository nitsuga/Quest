using System;

namespace Quest.Common.Messages
{
    /// <summary>
    ///     a class containing the patient observation details
    /// </summary>
    [Serializable]    
    public class PatientObservation
    {        
        public int GCS { get; set; }
        public int Diastolic { get; set; }
        public int Systolic { get; set; }
        public int SATS { get; set; }
        public override string ToString()
        {
            return ""; // String.Format("EventUpdate EventId={0} Updated={1}", EventId, Updated);
        }
    }

    
}