using Microsoft.EntityFrameworkCore;
using Quest.Common.Messages;
using Quest.Lib.Data;
using Quest.Lib.DataModel;
using Quest.Lib.Utils;
using System.Linq;

namespace Quest.Lib.Resource
{
    public class ResourceStoreMssql : IResourceStore
    {
        IDatabaseFactory _dbFactory;

        public ResourceStoreMssql(IDatabaseFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public bool FleetNoExists(string fleetno)
        {
            return _dbFactory.Execute<QuestContext, bool>((db) =>
            {
                return db.Resource.Any(x => x.FleetNo == fleetno && x.EndDate == null);
            });
        }

        public QuestResource GetByFleetNo(string fleetno)
        {
            return _dbFactory.Execute<QuestContext, QuestResource>((db) =>
            {
                var dbinc = db.Resource
                    .Include(x => x.Callsign)
                    .Include(x => x.ResourceStatus)
                    .Include(x => x.ResourceType)
                    .FirstOrDefault(x => x.FleetNo == fleetno && x.EndDate == null);

                if (dbinc == null)
                    return null;
                var res = Cloner.CloneJson<QuestResource>(dbinc);
                return res;
            });
        }

        public QuestResource GetByCallsign(string callsign)
        {
            return _dbFactory.Execute<QuestContext, QuestResource>((db) =>
            {
                var dbinc = db.Resource
                    .Include(x => x.Callsign)
                    .Include(x => x.ResourceStatus)
                    .Include(x => x.ResourceType).FirstOrDefault(x => x.Callsign.Callsign1 == callsign && x.EndDate == null);
                if (dbinc == null)
                    return null;
                var res = Cloner.CloneJson<QuestResource>(dbinc);
                return res;
            });
        }

        public int GetOffroadStatusId()
        {
            return _dbFactory.Execute<QuestContext, int>((db) =>
            {
                var status = db.ResourceStatus.FirstOrDefault(x => x.Offroad == true);
                return status.ResourceStatusId;
            });
        }

        /// <summary>
        /// merge in changes supplied by the resource update
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        public ResourceUpdateResult Update(ResourceUpdate update)
        {
            return _dbFactory.Execute<QuestContext, ResourceUpdateResult>((db) =>
            {
                // fleetno is the primary key
                if (update.Resource.FleetNo == null)
                    return null;

                // find the most up-to-date resource record;
                var oldres = db.Resource
                    .Include(x => x.Callsign)
                    .Include(x => x.ResourceStatus)
                    .Include(x => x.ResourceType)
                    .FirstOrDefault(x => x.FleetNo == update.Resource.FleetNo && x.EndDate == null);

                if (oldres == null)
                {
                    var res = update.Resource;
                    //create a new record for this moving dimension
                    oldres = new DataModel.Resource
                    {
                        // save the new resource record
                        Agency = res.Agency,
                        CallsignId = null,
                        Destination = res.Destination,
                        Course = res.Course,
                        Eta = res.Eta,
                        EventType = res.EventType,
                        FleetNo = res.FleetNo,
                        Latitude = (float?)res?.Position?.Y,
                        Longitude = (float?)res?.Position?.X,
                        EventId = res.EventId,
                        Speed = res.Speed,
                        Skill = res.Skill,
                        Sector = res.Skill,
                        Comment = res.Comment
                    };
                }
                else
                {
                    // mark record as old
                    oldres.EndDate = update.UpdateTime;
                    db.SaveChanges();
                }


                // look up callsign
                // make sure callsign exists
                Callsign callsign = null;
                if (!string.IsNullOrEmpty(update.Resource.Callsign))
                {
                    callsign = db.Callsign.FirstOrDefault(x => x.Callsign1 == update.Resource.Callsign);
                    if (callsign == null)
                    {
                        callsign = new Callsign { Callsign1 = update.Resource.Callsign };
                        db.Callsign.Add(callsign);
                        db.SaveChanges();
                    }
                }
                else
                {
                    // use the old record for the callsign
                    if (oldres.Callsign != null)
                        callsign = db.Callsign.FirstOrDefault(x => x.Callsign1 == oldres.Callsign.Callsign1);
                    else
                    {
                        // new record and old record are both null for the callsign
                    }
                }

                ////////////////////////////////////////////////////////////
                // use last resource type if update is null
                var restype = update.Resource.ResourceType??oldres.ResourceType?.ResourceType1;

                // look up resource type
                var resourceType = db.ResourceType.FirstOrDefault(x => x.ResourceType1 == restype);

                if (resourceType == null)
                {
                    resourceType = new ResourceType {  ResourceType1 = restype };
                    db.ResourceType.Add(resourceType);
                    db.SaveChanges();
                }

                ////////////////////////////////////////////////////////////
                // use last destination if update is null
                var destination = update.Resource.Destination ?? oldres.Destination;

                var dest = db.Destinations.FirstOrDefault(x => x.Destination == destination);

                var qdestination = new QuestDestination { Name = dest?.Destination };

                ////////////////////////////////////////////////////////////
                // find corresponding status;

                var status = update.Resource.Status ?? oldres.ResourceStatus?.Status;

                var statusrec = db.ResourceStatus.FirstOrDefault(x => x.Status == status);
                if (statusrec == null)
                {
                    statusrec = new DataModel.ResourceStatus
                    {
                        Status = update.Resource.Status??"???",
                        Rest = false,
                        Offroad = false,
                        NoSignal = false,
                        BusyEnroute = false,
                        Busy = false,
                        Available = false
                    };
                    db.ResourceStatus.Add(statusrec);
                    db.SaveChanges();
                }

                double? Latitude, Longitude;
                if (update.Resource.Position == null)
                {
                    Latitude = oldres.Latitude;
                    Longitude = oldres.Longitude;
                }
                else
                {
                    Latitude = update.Resource.Position.Y;
                    Longitude = update.Resource.Position.X;
                }

                //create a new record for this moving dimension
                // merge in previous values if null is supplied
                var newres = new DataModel.Resource
                {
                    // save the new resource record
                    Agency = update.Resource.Agency??oldres.Agency,
                    Callsign = callsign,
                    Destination = destination,
                    Course = update.Resource.Course ?? oldres.Course,
                    Eta = update.Resource.Eta??oldres.Eta,
                    EventType = update.Resource.EventType??oldres.EventType,
                    FleetNo = update.Resource.FleetNo??oldres.FleetNo,
                    Latitude = (float?)Latitude,
                    Longitude = (float?)Longitude,
                    EventId = update.Resource.EventId ?? oldres.EventId,
                    Speed = update.Resource.Speed??oldres.Speed,
                    ResourceType = resourceType,
                    Skill = update.Resource.Skill ?? oldres.Skill,
                    Sector = update.Resource.Sector ?? oldres.Sector,
                    EndDate = null,
                    StartDate = update.UpdateTime,
                    ResourceStatus = statusrec,
                    Comment = update.Resource.Comment ?? oldres.Comment,
                    HDoP = update.Resource.HDoP ?? oldres.HDoP,
                };

                db.Resource.Add(newres);

                // save the old and new records
                db.SaveChanges();

                return new ResourceUpdateResult
                {
                    NewResource = FromDatabase(newres),
                    OldResource = FromDatabase(oldres)
                };
                
            });
        }

        QuestResource FromDatabase(DataModel.Resource newres)
        {
            return new QuestResource
            {
                Agency = newres.Agency,
                Callsign = newres.Callsign.Callsign1,
                Destination = newres.Destination,
                Course = newres.Course,
                Eta = newres.Eta,
                EventType = newres.EventType,
                FleetNo = newres.FleetNo,
                Position = new GeoAPI.Geometries.Coordinate(newres.Longitude ?? 0, newres.Latitude ?? 0),
                EventId = newres.EventId,
                Speed = newres.Speed,
                ResourceType = newres.ResourceType.ResourceType1,
                Skill = newres.Skill,
                Sector = newres.Sector,
                Status = newres.ResourceStatus.Status,
                Comment = newres.Comment,
                LastUpdated = newres.StartDate,
                };
        }

        public void Clear()
        {
            _dbFactory.Execute<QuestContext>((db) =>
            {
                // remove all incidents
                db.Incident.RemoveRange(db.Incident);

                db.SaveChanges();

                // set all resource
                db.Resource.RemoveRange(db.Resource);

                db.SaveChanges();
            });
        }
    }
}
