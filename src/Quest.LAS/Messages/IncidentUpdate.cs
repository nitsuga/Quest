using System;

namespace Quest.LAS.Messages
{
    public class IncidentUpdate
    {
        public Int32 IncidentNumber { get; set; }
        public DateTime IncidentDate { get; set; }
        public string Sector { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Caller { get; set; }
        public Int32 IncidentEasting { get; set; }
        public Int32 IncidentNorthing { get; set; }
        public string PatientType { get; set; }
        public bool IsInfectious { get; set; }
        public bool IsPsychiatric { get; set; }
        public string Location { get; set; }
        public string PatientName { get; set; }
        public string Age { get; set; }
        public string Sex { get; set; }
        public bool IsEscort { get; set; }
        public string InjuryDescription { get; set; }
        public string SpecialInstruction { get; set; }
        public string Destination { get; set; }
        public Int32 DestinationEasting { get; set; }
        public Int32 DestinationNorthing { get; set; }
        public DateTime OrconTime { get; set; }
        public String GridRefAccuracy { get; set; }
        public string IncidentCategory { get; set; }
        public string ChiefComplaint { get; set; }
        public string DeterminantDescription { get; set; }
        public string Conscious { get; set; }
        public string Breathing { get; set; }
        public Int32 NumPatients { get; set; }
        public string CritialKQS { get; set; }
        public Int32 CallIncomplete { get; set; }
        public Int32 JobType { get; set; }
        public DateTime PickupTime { get; set; }
        public DateTime AppointmentTime { get; set; }
        public bool IsResus { get; set; }
        public bool IsOxygen { get; set; }
        public string Mobility { get; set; }
    }

}
