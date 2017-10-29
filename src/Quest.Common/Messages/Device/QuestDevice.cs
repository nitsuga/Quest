using System;

namespace Quest.Common.Messages.Device
{
    public partial class QuestDevice
    {
        public int DeviceId { get; set; }
        public long? Revision { get; set; }
        public string OwnerId { get; set; }
        public string AuthToken { get; set; }
        public string DeviceIdentity { get; set; }
        public string NotificationTypeId { get; set; }
        public string NotificationId { get; set; }
        public float? HDoP { get; set; }
        public bool? IsEnabled { get; set; }
        public bool? SendNearby { get; set; }
        public float? NearbyDistance { get; set; }
        public DateTime? LoggedOnTime { get; set; }
        public DateTime? LoggedOffTime { get; set; }
        public int? DeviceRoleId { get; set; }
        public string Osversion { get; set; }
        public string DeviceMake { get; set; }
        public string DeviceModel { get; set; }
        public bool? UseExternalStatus { get; set; }
        public string Skill { get; set; }
        public float? Speed { get; set; }
        public float? Course { get; set; }
        public float? Latitude { get; set; }
        public float? Longitude { get; set; }
        public string FleetNo { get; set; }
        public bool? IsPrimary { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
