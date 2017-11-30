namespace Quest.Lib.Northgate
{

    /// </summary>
    public class XCConfig
    {
        /// <summary>
        /// channel name XC0 or LVM0
        /// </summary>
        public string Name { get; set; } = "XC0";
        public string Connection { get; set; } = "";

        /// <summary>
        /// enable connection to XC
        /// </summary>
        public bool ConnectionEnabled { get; set; } = true;
        
        /// <summary>
        /// 
        /// </summary>
        public bool EmitEnabled { get; set; } = true;

        public bool OutboundEnabled { get; set; } = true;
        public string Commands { get; set; } = "";
        public string Subscriptions { get; set; } = "";
        public string CliFormat { get; set; } = "";
        public string IncFormat { get; set; } = "";
        public string ResFormat { get; set; } = "";
        public string SpaFormat { get; set; } = "";
        public string SpbFormat { get; set; } = "";
        public string Lvm { get; set; } = "";
        public string DrFormat { get; set; } = "";
        public string RscFormat { get; set; } = "";
        public int HBTReceiveDelay { get; set; } = 60;
        public int HBTSendDelay { get; set; } = 30;
    }
}
