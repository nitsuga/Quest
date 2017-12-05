using Microsoft.EntityFrameworkCore;
using Quest.Common.Messages.Destination;
using Quest.Common.Messages.GIS;
using Quest.Common.Messages.Resource;
using Quest.Lib.Coords;
using Quest.Lib.Data;
using Quest.Lib.DataModel;
using Quest.Lib.Utils;
using System;
using System.Collections.Generic;
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

        /// <summary>
        /// get all current valid resource assignments
        /// </summary>
        /// <returns></returns>
        public List<ResourceAssignmentStatus> GetAssignmentStatus()
        {
            return _dbFactory.Execute<QuestContext, List<ResourceAssignmentStatus>>((db) =>
            {
                db.Database.ExecuteSqlCommand("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;");

                // find the most up-to-date resource record;
                var list = db.ResourceAssignment
                    .Include(x => x.Destination)
                    .Where(x => x.EndDate == null)
                    .ToList();

                return FromDatabase(list).ToList();
            });
        }

        /// <summary>
        /// update a resource assignment
        /// </summary>
        /// <param name="item">assignment details</param>
        /// <returns></returns>
        public ResourceAssignmentStatus UpdateResourceAssign(ResourceAssignmentUpdate item)
        {
            return _dbFactory.Execute<QuestContext, ResourceAssignmentStatus>((db) =>
            {
                // fleetno is the primary key
                if (item.Callsign == null)
                    return null;

                // find the most up-to-date resource record;
                var resource = db.Resource.AsNoTracking()
                    .Include(x => x.Callsign)
                    .Where(x => x.Callsign.Callsign1 == item.Callsign && x.EndDate == null)
                    .FirstOrDefault();

                if (resource == null)
                    return null;

                // find the most up-to-date resource assignment record;
                var oldres_list = db.ResourceAssignment
                    .Include(x => x.Destination)
                    .OrderBy(x => x.Destination.Shortcode)
                    .Where(x => x.Callsign == item.Callsign && x.EndDate == null)
                    .ToList();

                if (oldres_list != null && oldres_list.Count() > 0)
                {
                    // mark records as old
                    foreach (var v in oldres_list)
                        v.EndDate = DateTime.UtcNow;
                    db.SaveChanges();
                }

                var dest =  db.Destinations.FirstOrDefault(x => x.Destination == item.DestinationCode);
                var point = GeomUtils.GetPointFromWkt(dest.Wkt);
                var latlng = LatLongConverter.OSRefToWGS84(point.X, point.Y);

                //create a new record for this moving dimension
                // merge in previous values if null is supplied
                var resassign = new ResourceAssignment
                {
                    Callsign = resource.Callsign.Callsign1,
                    StartDate = DateTime.UtcNow,
                    EndDate = null,
                    ArrivedAt = item.ArrivedAt,
                    Assigned = item.Assigned,
                    CancelledAt = item.CancelledAt,
                    DestinationId = dest.DestinationId,
                    Eta = item.CurrentEta,
                    LeftAt = item.LeftAt,
                    Notes = item.Notes,
                    OriginalEta = item.OriginalEta,
                    StartLatitude = (float)item.StartPosition.Latitude,
                    StartLongitude= (float)item.StartPosition.Longitude,
                    Status = (int)item.Status
                };

                db.ResourceAssignment.Add(resassign);

                // save the old and new records
                db.SaveChanges();

                return FromDatabase(resassign);

            });
        }

        /// <summary>
        /// check if a vehicle exists
        /// </summary>
        /// <param name="fleetno"></param>
        /// <returns></returns>
        public bool FleetNoExists(string fleetno)
        {
            return _dbFactory.Execute<QuestContext, bool>((db) =>
            {
                return db.Resource.Any(x => x.FleetNo == fleetno && x.EndDate == null);
            });
        }

        /// <summary>
        /// get a resource by fleet number
        /// </summary>
        /// <param name="fleetno"></param>
        /// <returns></returns>
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

                return FromDatabase(dbinc);
            });
        }

        /// <summary>
        /// get a resource by callsign
        /// </summary>
        /// <param name="callsign"></param>
        /// <returns></returns>
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
                return FromDatabase(dbinc);
            });
        }

        /// <summary>
        /// get the status code for offroad
        /// </summary>
        /// <returns></returns>
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
        public ResourceUpdateResult Update(ResourceUpdateRequest update)
        {
            return _dbFactory.Execute<QuestContext, ResourceUpdateResult>((db) =>
            {
                // fleetno is the primary key
                if (update.Resource.FleetNo == null)
                    return null;

                // find the most up-to-date resource record;
                var oldres_list = db.Resource
                    .Include(x => x.Callsign)
                    .Include(x => x.ResourceStatus)
                    .Include(x => x.ResourceType)
                    .OrderByDescending(x => x.ResourceId)
                    .Where(x => x.FleetNo == update.Resource.FleetNo && x.EndDate == null)
                    .ToList();

                DataModel.Resource oldres = null;

                if (oldres_list == null || oldres_list.Count()==0)
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
                        Latitude = (float?)res?.Position?.Latitude,
                        Longitude = (float?)res?.Position?.Longitude,
                        EventId = res.EventId,
                        Speed = res.Speed,
                        Skill = res.Skill,
                        Sector = res.Skill,
                        Comment = res.Comment
                    };
                }
                else
                {
                    // mark records as old
                    foreach (var v in oldres_list)
                        v.EndDate = update.UpdateTime;
                    db.SaveChanges();

                    oldres = oldres_list.FirstOrDefault();
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
                    Latitude = update.Resource.Position.Latitude;
                    Longitude = update.Resource.Position.Longitude;
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
                    OldResource = FromDatabase(oldres),
                    NewResource = FromDatabase(newres)
                };
                
            });
        }

        /// <summary>
        /// get all resources 
        /// </summary>
        /// <param name="revision"></param>
        /// <param name="resourceGroups"></param>
        /// <param name="avail"></param>
        /// <param name="busy"></param>
        /// <returns></returns>
        public List<QuestResource> GetResources(long revision, string[] resourceGroups, bool avail, bool busy)
        {
            return _dbFactory.ExecuteNoTracking<QuestContext, List<QuestResource>>((db) =>
            {
                if (resourceGroups == null)
                    resourceGroups = new string[] { };

                db.Database.ExecuteSqlCommand("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;");

                // get all *current* resources
                var resources = db.Resource
                            .AsNoTracking()
                            .Include(x => x.Callsign)
                            .Include(x => x.ResourceStatus)
                            .Include(x => x.ResourceType)
                            .Where(x => (x.EndDate == null) && (x.Revision > revision))
                            .Where(x => resourceGroups==null || resourceGroups.Length == 0 || (resourceGroups.Length > 0 && resourceGroups.Contains(x.ResourceType.ResourceTypeGroup)))
                            .ToList();

                if (avail && busy)
                    return FromDatabase(resources).ToList();

                if (avail)
                    return FromDatabase(resources.Where(x => x.ResourceStatus.Available == true)).ToList();

                if (busy)
                    return FromDatabase(resources.Where(x => x.ResourceStatus.Busy == true)).ToList();

                return FromDatabase(resources.Where(x => x.ResourceStatus.Busy == false && x.ResourceStatus.Available == false)).ToList();
            });
        }

        public string GetStatusDescription(bool available, bool busy, bool enroute, bool rest)
        {
            if (available == true)
                return "Available";
            if (enroute == true)
                return "Enroute";
            if (busy == true)
                return "Busy";
            if (rest == true)
                return "Rest";
            return "Offroad";
        }

        private string GetStatusDescription(DataModel.ResourceStatus status)
        {
            if (status == null)
                return "???";
            return GetStatusDescription(status.Available ?? false, status.Busy ?? false, status.BusyEnroute ?? false, status.Rest ?? false);
        }

        private IEnumerable<ResourceAssignmentStatus> FromDatabase(IEnumerable<ResourceAssignment> items)
        {
            foreach (var r in items)
                yield return FromDatabase(r);
        }

        private ResourceAssignmentStatus FromDatabase(ResourceAssignment item)
        {
            return new ResourceAssignmentStatus {

                //TODO: 
                Percent="50%",
                Status = ResourceAssignmentStatus.StatusCode.InProgress,
                TTG = "7m",

                Resource = GetByCallsign(item.Callsign),
                ArrivedAt = item.ArrivedAt,
                Assigned =item.Assigned,
                CancelledAt = item.CancelledAt,
                DestinationCode = item.Destination.Shortcode,
                CurrentEta = item.Eta,
                LeftAt = item.LeftAt,
                Notes =item.Notes,
                OriginalEta =item.OriginalEta,
                StartPosition = new LatLng(item.StartLatitude, item.StartLongitude),
            };        
        }

        private IEnumerable<QuestResource> FromDatabase(IEnumerable<DataModel.Resource> resources)
        {
            foreach (var r in resources)
                yield return FromDatabase(r);
        }

        private QuestResource FromDatabase(DataModel.Resource newres)
        {
            return new QuestResource
            {
                Agency = newres.Agency,
                Callsign = newres.Callsign?.Callsign1,
                Destination = newres.Destination,
                Course = newres.Course,
                Eta = newres.Eta,
                EventType = newres.EventType,
                FleetNo = newres.FleetNo,
                Position = new LatLng(newres.Latitude ?? 0, newres.Longitude ?? 0),
                EventId = newres.EventId,
                Speed = newres.Speed,
                ResourceType = newres.ResourceType?.ResourceType1,
                ResourceTypeGroup = newres.ResourceType?.ResourceTypeGroup,
                Revision =newres.Revision??0,
                Skill = newres.Skill,
                Sector = newres.Sector,
                Status = newres.ResourceStatus?.Status,
                Comment = newres.Comment,
                StartDate = newres.StartDate,
                EndDate = newres.EndDate,
                StatusCategory = GetStatusDescription(newres.ResourceStatus)
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
