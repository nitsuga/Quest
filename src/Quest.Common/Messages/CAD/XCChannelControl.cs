namespace Quest.Common.Messages.CAD
{
    public class XCChannelControl : MessageBase
    {
        public string Channel;

        public Command Action;

        public enum Command
        {
            Disable,
            EnableAsPrimary,
            EnableAsBackup,
        }

    }

}