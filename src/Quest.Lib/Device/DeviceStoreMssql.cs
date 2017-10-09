using Quest.Common.Messages;
using Quest.Lib.Data;
using Quest.Lib.DataModel;
using Quest.Lib.Utils;
using System;
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

        public QuestDevice Update(QuestDevice device, DateTime timestamp)
        {
            return _dbFactory.Execute<QuestContext, QuestDevice>((db) =>
            {
                if (device.DeviceIdentity == null)
                    return null;

                // locate record and create or update
                var oldrec = db.Devices.FirstOrDefault(x => x.DeviceIdentity == device.DeviceIdentity);

                if (oldrec == null)
                {
                    oldrec = new DataModel.Devices();
                    db.Devices.Add(oldrec);
                }
                else
                {
                    oldrec.EndDate = timestamp;
                    db.SaveChanges();
                }

                var status = db.ResourceStatus.FirstOrDefault(x => x.Offroad == true);

                var newres = new DataModel.Devices
                {
                    DeviceIdentity = device.DeviceIdentity,
                    OwnerId = device.OwnerId??oldrec.OwnerId,
                    LoggedOnTime = device.LoggedOnTime??oldrec.LoggedOnTime,
                    DeviceRoleId = device.DeviceRoleId??oldrec.DeviceRoleId,
                    NotificationTypeId = device.NotificationTypeId??oldrec.NotificationTypeId,
                    NotificationId = device.NotificationId??oldrec.NotificationId,
                    AuthToken = device.Token,
                    IsEnabled = device.IsEnabled,
                    LoggedOffTime = device.LoggedOffTime,
                    Osversion = device.OSVersion??oldrec.Osversion,
                    DeviceMake = device.DeviceMake??oldrec.Osversion,
                    DeviceModel = device.DeviceModel??oldrec.DeviceModel,
                    PositionAccuracy = device.PositionAccuracy??oldrec.PositionAccuracy,
                    NearbyDistance = device.NearbyDistance??oldrec.NearbyDistance,
                    EndDate =null,
                    StartDate =timestamp
                };

                db.SaveChanges();

                return  Cloner.CloneJson<QuestDevice>(newres);

            });
        }
    }
}