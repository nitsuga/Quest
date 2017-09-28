﻿using Quest.Common.Messages;
using Quest.Common.ServiceBus;
using Quest.Lib.Notifier;

namespace Quest.Lib.Incident
{
    public class IncidentHandler
    {
        public void IncidentUpdate(IncidentUpdate item, NotificationSettings settings, IServiceBusClient msgSource, IIncidentStore persist)
        {
            var inc = persist.Update(item);

            var incsFeature = new EventMapItem
            {
                ID = inc.IncidentID.ToString(),
                revision = inc.Revision ?? 0,
                X = inc.Longitude ?? 0,
                Y = inc.Latitude ?? 0,
                EventId = inc.Serial,
                Notes = inc.Determinant,
                Priority = inc.Priority,
                Status = inc.Status,
                Created = inc.Created?.ToString("hh:MM") ?? "?",
                LastUpdated = inc.LastUpdated,
                //AssignedResources = inc.AssignedResources,
                AZ = inc.AZ,
                Determinant = inc.Determinant,
                DeterminantDescription = inc.DeterminantDescription,
                Location = inc.Location,
                LocationComment = inc.LocationComment,
                PatientAge = inc.PatientAge,
                PatientSex = inc.PatientSex,
                ProblemDescription = inc.ProblemDescription
            };

            // updates go to assigned devices and to nearby ones of the right grade
            //TODO:
            //var devices = db.Devices.ToList().Where(x => IsNearbyDeviceOrAssigned(x, inc.Latitude, inc.Longitude, inc.Serial)).ToList();

            //SendEventNotification(devices, inc, settings, "Update");
            msgSource.Broadcast(new IncidentDatabaseUpdate() { serial = inc.Serial, Item = incsFeature });

        }

        public void CloseIncident(CloseIncident item, NotificationSettings settings, IServiceBusClient msgSource, IIncidentStore persist)
        {
            persist.Close(item.Serial);
            msgSource.Broadcast(new IncidentDatabaseUpdate() { serial = item.Serial });
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
