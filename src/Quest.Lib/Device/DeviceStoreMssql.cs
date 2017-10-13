using Quest.Common.Messages;
using Quest.Lib.Data;
using Quest.Lib.DataModel;
using Quest.Lib.Utils;
using System;
using System.Collections.Generic;
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

        /// <summary>
        /// get device details by device indentity
        /// Note this is not secure.
        /// </summary>
        /// <param name="deviceIdentity"></param>
        /// <returns></returns>
        public QuestDevice Get(string deviceIdentity)
        {
            return _dbFactory.Execute<QuestContext, QuestDevice>((db) =>
            {
                // locate record and create or update
                var rec = db.Devices.FirstOrDefault(x => x.DeviceIdentity == deviceIdentity && x.EndDate == null);
                var dev = Cloner.CloneJson<QuestDevice>(rec);
                return dev;
            });
        }

        /// <summary>
        /// get a list of devices associated with a fleet number.
        /// </summary>
        /// <param name="fleetNo"></param>
        /// <returns></returns>
        public List<QuestDevice> GetByFleet(string fleetNo)
        {
            return _dbFactory.Execute<QuestContext,List<QuestDevice>>((db) =>
            {
                // locate record and create or update
                var recs = db.Devices.Where(x => x.FleetNo == fleetNo && x.EndDate == null ).ToArray();
                List<QuestDevice> result = new List<QuestDevice>();
                foreach ( var dev in recs)
                    result.Add(Cloner.CloneJson<QuestDevice>(dev));
                return result;
            });
        }

        /// <summary>
        /// get device details by access token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public QuestDevice GetByToken(string token)
        {
            return _dbFactory.Execute<QuestContext, QuestDevice>((db) =>
            {
                // locate record and create or update
                var rec = db.Devices.FirstOrDefault(x => x.AuthToken == token && x.EndDate == null  );
                var dev = Cloner.CloneJson<QuestDevice>(rec);
                return dev;
            });
        }

        /// <summary>
        /// update a device record
        /// </summary>
        /// <param name="device"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public QuestDevice Update(QuestDevice device, DateTime timestamp)
        {
            return _dbFactory.Execute<QuestContext, QuestDevice>((db) =>
            {
                if (device.DeviceIdentity == null)
                    return null;

                // locate record and create or update
                var oldrec = db.Devices.FirstOrDefault(x => x.DeviceIdentity == device.DeviceIdentity && x.EndDate == null );

                if (oldrec == null)
                {
                    //oldrec = new DataModel.Devices();
                    //db.Devices.Add(oldrec);
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
                    AuthToken = device.AuthToken,
                    IsEnabled = device.IsEnabled,
                    LoggedOffTime = device.LoggedOffTime,
                    Osversion = device.Osversion ?? oldrec.Osversion,
                    DeviceMake = device.DeviceMake??oldrec.Osversion,
                    DeviceModel = device.DeviceModel??oldrec.DeviceModel,
                    PositionAccuracy = device.PositionAccuracy??oldrec.PositionAccuracy,
                    NearbyDistance = device.NearbyDistance??oldrec.NearbyDistance,
                    EndDate =null,
                    StartDate =timestamp,
                    Direction = device.Direction ?? oldrec.Direction,
                    FleetNo = device.FleetNo ?? oldrec.FleetNo,
                    IsPrimary = device.IsPrimary ?? oldrec.IsPrimary,
                    Latitude = device.Latitude ?? oldrec.Latitude,
                    Longitude = device.Longitude ?? oldrec.Longitude,
                    SendNearby = device.SendNearby ?? oldrec.SendNearby,
                    Skill = device.Skill ?? oldrec.Skill,
                    Speed = device.Speed ?? oldrec.Speed,
                    UseExternalStatus = device.UseExternalStatus ?? oldrec.UseExternalStatus 
                };

                db.Devices.Add(newres);

                db.SaveChanges();

                return  Cloner.CloneJson<QuestDevice>(newres);

            });
        }
    }
}