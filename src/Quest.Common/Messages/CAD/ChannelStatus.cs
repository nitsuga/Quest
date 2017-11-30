namespace Quest.Common.Messages.CAD
{
    public enum ChannelStatus
    {
        /// <summary>
        /// channel status is unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// channel is disabled
        /// </summary>
        Disabled,

        /// <summary>
        /// channel is disconnected from XC
        /// </summary>
        Disconnected,

        /// <summary>
        /// channel is connected to xc, but might not be receiving data
        /// </summary>
        Connected,

        /// <summary>
        /// channel is connected and receiving data
        /// </summary>
        Active
    }

}