﻿using Quest.Common.Messages.CAD;
using Quest.Common.Messages.Incident;
using Quest.Common.ServiceBus;
using Quest.Lib.Notifier;

namespace Quest.Lib.Incident
{
    public class IncidentHandler
    {
        public void IncidentUpdate(IncidentUpdateRequest item, NotificationSettings settings, IServiceBusClient msgSource, IIncidentStore persist)
        {
            var inc = persist.Update(item);

            // updates go to assigned devices and to nearby ones of the right grade
            //TODO:
            //var devices = db.Devices.ToList().Where(x => IsNearbyDeviceOrAssigned(x, inc.Latitude, inc.Longitude, inc.Serial)).ToList();

            //SendEventNotification(devices, inc, settings, "Update");
            msgSource.Broadcast(new IncidentUpdate() { serial = inc.EventId, Item = inc });

        }

#if false
        /// <summary>
        ///     check if a device is the target (by callsign) or is nearby
        /// </summary>
        /// <param name="device"></param>
        /// <param name="target"></param>
        /// <param name="serial"></param>
        /// <returns></returns>
        private bool IsNearbyDeviceOrAssigned(DataModel.Devices device, float? Latitude, float? Longitude, string serial)
        {
            // enabled?
            if (device.IsEnabled == false)
                return false;

            // its linked to a resource - good, as it should always be anyway, and it has the right callsign?
            if (device.Resource?.Incident != null && string.Equals(device.Resource.Incident, serial, StringComparison.CurrentCultureIgnoreCase) && device.DeviceIdentity != null)
                return true;

            // check the nearby settings
            if (device.SendNearby == false)
                return false;

            if (device.NearbyDistance == 0)
                return false;

            // no position
            if (device.Latitude == null)
                return false;

            // no target
            if (Latitude == null)
                return false;

            // calculate the distance
            var distance = GeomUtils.Distance(device.Latitude ?? 0, device.Longitude ?? 0, Latitude ?? 0, Longitude ?? 0);

            if (distance <= device.NearbyDistance)
                return true;

            return false;
        }
#endif

        public void CloseIncident(CloseIncident item, NotificationSettings settings, IServiceBusClient msgSource, IIncidentStore persist)
        {
            persist.Close(item.Serial);
            msgSource.Broadcast(new IncidentUpdate() { serial = item.Serial });
        }

    //    public void CallDisconnectStatusListHandler(CallDisconnectStatusList item,
    //NotificationSettings settings)
    //    {
    //        _dbFactory.Execute<QuestContext>((db) =>
    //        {
    //            foreach (var i in item.Items)
    //            {
    //                var inc = db.Incident.FirstOrDefault(x => x.Serial == i.Serial);
    //                if (inc != null)
    //                {
    //                    inc.DisconnectTime = i.DisconnectTime;
    //                }
    //                db.SaveChanges();
    //            }
    //        });
    //    }


    //    public void CPEventStatusListHandler(CPEventStatusList item, NotificationSettings settings)
    //    {
    //        _dbFactory.Execute<QuestContext>((db) =>
    //        {
    //            foreach (var i in item.Items)
    //            {
    //                var inc = db.Incident.FirstOrDefault(x => x.Serial == i.Serial);
    //                if (inc != null)
    //                {
    //                    inc.PatientAge = i.Age;
    //                    inc.PatientSex = i.Sex;
    //                    inc.CallerTelephone = i.CallerTelephone;
    //                    inc.LocationComment = i.LocationComment;
    //                    inc.ProblemDescription = i.ProblemDescription;
    //                }
    //                db.SaveChanges();
    //            }
    //        });
    //    }
    }
}
