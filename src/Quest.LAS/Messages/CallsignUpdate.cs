namespace Quest.LAS.Messages
{
    public class CallsignUpdate: ICadMessage
    {
        public string Callsign { get; set; }
        public int FleetNo { get; set; }
        public bool DsoFlag { get; set; }
        public string SkillLevel { get; set; }
        public bool OnShift { get; set; }
        public SetStatus StatusUpdate { get; set; }
        public IncidentUpdate IncidentUpdate { get; set; }
    }

}
