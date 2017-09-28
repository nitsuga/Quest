using System.Collections.Generic;

namespace Quest.Lib.DataModel
{
    public partial class DeviceRole
    {
        public DeviceRole()
        {
            Devices = new HashSet<Devices>();
        }

        public int DeviceRoleId { get; set; }
        public string DeviceRoleName { get; set; }

        public ICollection<Devices> Devices { get; set; }
    }
}
