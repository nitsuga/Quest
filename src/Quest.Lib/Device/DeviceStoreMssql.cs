using Quest.Common.Messages;
using Quest.Lib.DataModel;
using Quest.Lib.Utils;
using System.Linq;

namespace Quest.Lib.Device
{
    public class DeviceStoreMssql : IDeviceStore
    {
        public QuestDevice Get(string deviceIdentity)
        {
            using (var db = new QuestEntities())
            {
                // locate record and create or update
                var rec = db.Devices.FirstOrDefault(x => x.DeviceIdentity == deviceIdentity);
                var dev = Cloner.CloneJson<QuestDevice>(rec);
                return dev;
            }
        }


        public QuestDevice GetByToken(string token)
        {
            using (var db = new QuestEntities())
            {
                // locate record and create or update
                var rec = db.Devices.FirstOrDefault(x => x.AuthToken == token);
                var dev = Cloner.CloneJson<QuestDevice>(rec);
                return dev;
            }
        }

        public void Update(QuestDevice device)
        {
            using (var db = new QuestEntities())
            {
                // locate record and create or update
                var resrecord = db.Devices.FirstOrDefault(x => x.DeviceIdentity == device.DeviceIdentity);
                if (resrecord == null)
                {
                    resrecord = new DataModel.Device();
                    db.Devices.Add(resrecord);

                    // use the tiestamp of the message for the creation time
                    //resrecord.Created = new DateTime((item.Timestamp + 62135596800) * 10000000);
                }
                
                var status = db.ResourceStatus.FirstOrDefault(x => x.Offroad == true);

                resrecord.OwnerID = device.OwnerID;
                resrecord.DeviceIdentity = device.DeviceIdentity;
                resrecord.LoggedOnTime = device.LoggedOnTime;
                resrecord.LastUpdate = device.LastUpdate;
                resrecord.DeviceRoleID = device.DeviceRoleID;
                resrecord.NotificationTypeID = device.NotificationTypeID;
                resrecord.NotificationID = device.NotificationID;
                resrecord.AuthToken = device.Token;
                resrecord.isEnabled = device.isEnabled;
                resrecord.LastStatusUpdate = device.LastStatusUpdate;
                resrecord.LoggedOffTime = device.LoggedOffTime;
                resrecord.OSVersion = device.OSVersion;
                resrecord.DeviceMake = device.DeviceMake;
                resrecord.DeviceModel = device.DeviceModel;
                resrecord.ResourceID = device.ResourceID;
                resrecord.PositionAccuracy = device.PositionAccuracy;
                resrecord.NearbyDistance = device.NearbyDistance;

                db.SaveChanges();

            } // using
        }


        
    }
}