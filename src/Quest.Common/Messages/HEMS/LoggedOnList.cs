using System;
using System.Collections.Generic;
using System.Linq;

namespace Quest.Common.Messages.HEMS
{

    [Serializable]
    public class LoggedOnList : MessageBase
    {
        public List<LogonRecord> Users;

        public override string ToString()
        {

            if (Users != null)
            {
                var all = Users.Select(x => x.Callsign).ToArray();

                return String.Format("Logged On List Count={0} Callsigns={1}", Users.Count(), string.Join(",", all));
            }
            else
            {
                return String.Format("Logged On List Count=NULL ");
            }
        }
    }
}
