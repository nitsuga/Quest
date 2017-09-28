using Autofac;
using Quest.Common.Messages;
using Quest.Lib.Data;
using Quest.Lib.DataModel;
using Quest.Lib.Utils;
using System.Linq;

namespace Quest.Lib.Device
{
    public class DeviceStoreMssql : IDeviceStore
    {
        IDatabaseFactory _dbFactory;

        public DeviceStoreMssql(IDatabaseFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public QuestDevice Get(string deviceIdentity)
        {
            return _dbFactory.Execute<QuestContext, QuestDevice>((db) =>
            {
                // locate record and create or update
                var rec = db.Devices.FirstOrDefault(x => x.DeviceIdentity == deviceIdentity);
                var dev = Cloner.CloneJson<QuestDevice>(rec);
                return dev;
            });
        }


        public QuestDevice GetByToken(string token)
        {
            return _dbFactory.Execute<QuestContext, QuestDevice>((db) =>
            {
                // locate record and create or update
                var rec = db.Devices.FirstOrDefault(x => x.AuthToken == token);
                var dev = Cloner.CloneJson<QuestDevice>(rec);
                return dev;
            });
        }

        public void Update(QuestDevice device)
        {
            _dbFactory.Execute<QuestContext>((db) =>
            {
                // locate record and create or update
                var resrecord = db.Devices.FirstOrDefault(x => x.DeviceIdentity == device.DeviceIdentity);
                if (resrecord == null)
                {
                    resrecord = new DataModel.Devices();
                    db.Devices.Add(resrecord);

                    // use the tiestamp of the message for the creation time
                    //resrecord.Created = new DateTime((item.Timestamp + 62135596800) * 10000000);
                }

                var status = db.ResourceStatus.FirstOrDefault(x => x.Offroad == true);

                resrecord.OwnerId = device.OwnerId;
                resrecord.DeviceIdentity = device.DeviceIdentity;
                resrecord.LoggedOnTime = device.LoggedOnTime;
                resrecord.LastUpdate = device.LastUpdate;
                resrecord.DeviceRoleId = device.DeviceRoleId;
                resrecord.NotificationTypeId = device.NotificationTypeId;
                resrecord.NotificationId = device.NotificationId;
                resrecord.AuthToken = device.Token;
                resrecord.IsEnabled = device.IsEnabled;
                resrecord.LastStatusUpdate = device.LastStatusUpdate;
                resrecord.LoggedOffTime = device.LoggedOffTime;
                resrecord.Osversion = device.OSVersion;
                resrecord.DeviceMake = device.DeviceMake;
                resrecord.DeviceModel = device.DeviceModel;
                resrecord.ResourceId = device.ResourceId;
                resrecord.PositionAccuracy = device.PositionAccuracy;
                resrecord.NearbyDistance = device.NearbyDistance;

                db.SaveChanges();

            });
        }


        
    }
}