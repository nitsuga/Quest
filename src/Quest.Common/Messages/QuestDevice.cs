using System;

namespace Quest.Common.Messages
{
    public partial class QuestDevice
    {
        public int DeviceID { get; set; }
        public Nullable<long> Revision { get; set; }
        public string OwnerId { get; set; }
        public string Callsign { get; set; }
        public string AuthToken { get; set; }
        public string DeviceIdentity { get; set; }
        public string NotificationTypeId { get; set; }
        public string NotificationId { get; set; }
        public Nullable<System.DateTime> LastUpdate { get; set; }
        public Nullable<float> PositionAccuracy { get; set; }
        public bool IsEnabled { get; set; }
        public bool SendNearby { get; set; }
        public float? NearbyDistance { get; set; }
        public Nullable<System.DateTime> LoggedOnTime { get; set; }
        public Nullable<System.DateTime> LoggedOffTime { get; set; }
        public Nullable<int> DeviceRoleId { get; set; }
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
        public string FleetNo { get; set; }
    }
}
