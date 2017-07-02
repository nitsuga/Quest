using System;

namespace Quest.Mobile.Controllers
{
    [Serializable]
    public class StateFlags
    {
        public bool Avail { get; set; }

        public bool Busy { get; set; }
        /// <summary>
        /// display urgent events
        /// </summary>
        public bool CatA { get; set; }

        /// <summary>
        /// display non-urgent events
        /// </summary>
        public bool CatC { get; set; }

        /// <summary>
        /// recieve notifications for this telephony extension
        /// </summary>
        public int? Extension { get; set; }
    }
}