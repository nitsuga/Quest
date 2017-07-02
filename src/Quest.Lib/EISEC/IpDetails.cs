using System;

namespace Quest.Lib.EISEC
{
    [Serializable]
    public struct IpDetails
    {
        public string Addr;

        public int Port;

        public User User;

        /// <summary>
        ///     how long to wait for a response before disconnecting MUST BE MORE THAN SendPollSeconds
        /// </summary>
        public int LocalPollTimeoutSeconds;

        /// <summary>
        ///     How long EISEC waits for a heartbeat before disconnecting MUST BE MORE THAN SendPollSeconds
        /// </summary>
        public int RemotePollTimeoutSeconds;

        /// <summary>
        ///     how often to send a poll - set to 0 to disable
        /// </summary>
        public int SendPollSeconds;
    }
}