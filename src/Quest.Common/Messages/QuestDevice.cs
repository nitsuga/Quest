using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Common.Messages
{
    public partial class QuestDevice
    {
        public int DeviceID { get; set; }
        public Nullable<long> Revision { get; set; }
        public string OwnerID { get; set; }
        public Nullable<int> ResourceID { get; set; }
        public string AuthToken { get; set; }
        public string DeviceIdentity { get; set; }
        public Nullable<int> NotificationTypeID { get; set; }
        public string NotificationID { get; set; }
        public Nullable<System.DateTime> LastUpdate { get; set; }
        public Nullable<System.DateTime> LastStatusUpdate { get; set; }
        public Nullable<float> PositionAccuracy { get; set; }
        public Nullable<bool> isEnabled { get; set; }
        public Nullable<bool> SendNearby { get; set; }
        public Nullable<float> NearbyDistance { get; set; }
        public Nullable<System.DateTime> LoggedOnTime { get; set; }
        public Nullable<System.DateTime> LoggedOffTime { get; set; }
        public Nullable<int> DeviceRoleID { get; set; }
        public string OSVersion { get; set; }
        public string DeviceMake { get; set; }
        public string DeviceModel { get; set; }
        public Nullable<bool> UseExternalStatus { get; set; }
        public Nullable<int> ResourceStatusId { get; set; }
        public string DeviceCallsign { get; set; }
        public string PrevStatus { get; set; }
        public string Destination { get; set; }
        public string Road { get; set; }
        public string Skill { get; set; }
        public Nullable<int> Speed { get; set; }
        public Nullable<int> Direction { get; set; }
        public string Event { get; set; }
        public Nullable<float> Latitude { get; set; }
        public Nullable<float> Longitude { get; set; }
        public string Token { get; set; }
    }
}
