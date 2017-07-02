namespace Quest.Lib.Notifier
{
    public class NotificationSettings
    {
        public string adminSound { get; set; }
        public bool AppleIsProduction { get; set; }
        public string AppleP12Certificate { get; set; }
        public string AppleP12Password { get; set; }
        public string GCMKey { get; set; }
        public string targetSound { get; set; }
    }
}