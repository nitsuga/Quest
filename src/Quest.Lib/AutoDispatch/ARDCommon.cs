////////////////////////////////////////////////////////////////////////////////////////////////
//   
//   Copyright (C) 2014 Extent Ltd. Copying is only allowed with the express permission of Extent Ltd
//   
//   Use of this code is not permitted without a valid license from Extent Ltd
//   
////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using Quest.Lib.Routing;
using Quest.Common.Messages;

namespace Quest.Lib.AutoDispatch
{
    /// <summary>
    /// This class provides common routines used by ARD modules.
    /// </summary>
    public class ARDCommon
    {


#if false
        /// <summary>
        /// try and fulfil this incidents resource requirements - only allocates one vehicle at a time
        /// </summary>
        /// <param name="inc"></param>
        public static IMove FREDA_IncidentAssignment(IRouteEngine router, IncidentView inc, SimEngine simEngine, List<ResourceView> Resources, int hour, double maxDistance, double maxDuration, int instanceMax, double enrouteFactor, double waitingFactor)
        {
            try
            {
                int frucount = 0;
                int ambcount = 1;

                ARDCommon.CalculateResourceRequired(inc, simEngine);

                // check to see if amb count has been acheived
                ambcount = (from a in Resources where a.Incident != null && a.Incident.IncidentId == inc.IncidentId && a.VehicleType == 1 select a).Count();

                // check to see if fru count has been acheived
                frucount = (from a in Resources where a.Incident != null && a.Incident.IncidentId == inc.IncidentId && a.VehicleType == 2 select a).Count();

                int totalcount = ambcount + frucount;

                if (totalcount < inc.TotalRequired)
                {
                    if (frucount < inc.FRURequired)
                    {
                        CandidateResource cr = ARDCommon.GetBestAppropriateResource(router, hour, (int)inc.Easting, (int)inc.Northing, 2, maxDistance, maxDuration, inc.Category, Resources, instanceMax, enrouteFactor, waitingFactor);

                        if (cr!=null)
                            return new AssignMove() { Incident = inc, Resource = cr.resource, route = cr.route };
                    }

                    if (ambcount < inc.AMBRequired)
                    {
                        CandidateResource cr = ARDCommon.GetBestAppropriateResource(router, hour, (int)inc.Easting, (int)inc.Northing, 1, maxDistance, maxDuration, inc.Category, Resources, instanceMax, enrouteFactor, waitingFactor);
                        if (cr!=null)
                            return new AssignMove() { Incident = inc, Resource = cr.resource, route = cr.route };
                    }
                }                
            }
            catch (Exception ex)
            {
                simEngine.OnError(ex);
            }
            finally
            {
            }
            return null;
        }


        /// <summary>
        /// FREDA - called every 30 seconds to determine if new assignments can be made to incidents
        /// it checks waitin and enroute incidents to make sure they have the correct number and type of
        /// vehicles assigned.
        /// </summary>
        public static List<IMove> FREDA_Assignments(IRouteEngine router, SimEngine simEngine, List<ResourceView> resources, int hour, double maxDistance, double maxDuration, int instanceMax, double enrouteFactor, double waitingFactor)
        {
            List<IMove> moves = new List<IMove>();
            try
            {
                // build map of outstanding incidents and try and assign them
                // its sorted by priority and then age
                var waiting_incs = from i in simEngine.LiveIncidents
                                   where i.Status == ResourceStatus.Waiting || i.Status == ResourceStatus.Enroute
                                   orderby i.Status, i.Category, i.CallStart
                                   select i;

                // for each incident find the fastest resource
                if (waiting_incs.Count() > 0)
                    foreach (Incident inc in waiting_incs)
                    {
                        IMove m = FREDA_IncidentAssignment(router, inc, simEngine, resources, hour, maxDistance, maxDuration, instanceMax, enrouteFactor, waitingFactor);
                        if (m!=null)
                            moves.Add(m);
                    }
            }
            catch (Exception ex)
            {
                simEngine.OnError(ex);
            }
            finally
            {
            }
            return moves;
        }

#endif

        public static CandidateResource GetBestAppropriateResource(IRouteEngine router, int hour, int easting, int northing, String vehicleType, double distanceMax, double durationMax, int category, List<ResourceView> Resources, int instanceMax, double enrouteFactor, double waitingFactor)
        {
            IOrderedEnumerable<CandidateResource> result = GetBestAppropriateResources(router, hour, easting, northing, vehicleType, distanceMax, durationMax, category, Resources, instanceMax, enrouteFactor, waitingFactor);

            return result.FirstOrDefault();
        }


        /// <summary>
        /// get the best waiting resource to this e/n location
        /// </summary>
        /// <param name="router"></param>
        /// <param name="hour"></param>
        /// <param name="easting"></param>
        /// <param name="northing"></param>
        /// <param name="resourceType"></param>
        /// <param name="distanceMax"></param>
        /// <param name="durationMax"></param>
        /// <param name="category"></param>
        /// <param name="Resources"></param>
        /// <param name="instanceMax"></param>
        /// <param name="enrouteFactor"></param>
        /// <param name="waitingFactor"></param>
        /// <returns></returns>
        public static IOrderedEnumerable<CandidateResource> GetBestAppropriateResources(IRouteEngine router, int hour, int easting, int northing, String resourceType, double distanceMax, double durationMax, int category, List<QuestResource> Resources, int instanceMax, double enrouteFactor, double waitingFactor)
        {
            List<RoutingPoint> lr = new List<RoutingPoint>();
            List<CandidateResource> candidates = new List<CandidateResource>();
            RoutingResult results;

            // get location of the incident
            RoutingPoint inclocation = new RoutingPoint() { X = easting, Y = northing };

            //-------------------------------------------------------------------------------
            // Get list of waiting resources and add them to the list of candidates... 
            // a weight to applied to each resource of WaitingFactor * Distance_to_Incident
            // 
            var waitingResources = from v in Resources
//                                   where (v.Status == ResourceStatus.Waiting || v.Status == ResourceStatus.Enroute)
                                   where (v.Available == true )
                                   && v.ResourceType == resourceType
                                   select v;

            // get the fastest WAITING vehicle.. 
            if (waitingResources.Count() == 0)
                return null;

            int routingType = 0;
            // build up an array of waitingResources and enrouteResources and merge into a single list of RoutingLocations
            lr.Clear();
            waitingResources.ToList().ForEach(x => 
            {
                RoutingPoint rp = new RoutingPoint(x.Easting, x.Northing);
                rp.Tag = x; lr.Add(rp);
                routingType = x.RoutingTypeId??0;
            }
            );

            if (lr.Count() == 0)
                return null;

            RouteRequestMultiple request = new Routing.RouteRequestMultiple()
            {
                StartLocation = inclocation,
                EndLocations = lr,
                DistanceMax = distanceMax,
                DurationMax = durationMax,
                InstanceMax = instanceMax,
                MakeRoute = false,
                VehicleType = routingType,
                SearchType = SearchType.Quickest,
                Hour = hour,
                Map = null
            };

            // calculate distance of each resource
            results = router.CalculateRouteMultiple(request);

            // this bit that deals with enroute vehicles is redundant as we only deal with waiting resources
            if (results != null)
            {
                // calculate a weight for each
                foreach (var v in results.items)
                {
                    ResourceView res = (ResourceView)v.Tag;
                    if (res.BusyEnroute==true)
                    {
                        // add enroute resource if its quicker than where its going
                        //if (res.ETA < DateTime.Now()+v.Duration)
                        //    candidates.Add(new CandidateResource() { resource = res, weight = enrouteFactor * v.Distance, route=v });
                    }
                    else
                        candidates.Add(new CandidateResource() { resource = res, weight = waitingFactor * v.Distance, route=v });
                }
            }


            // find the minimum weight and return it
            var sorted = from c in candidates orderby c.weight select c;

            return sorted;
        }


    }

    public class CandidateResource
    {
        public double weight;
        public ResourceView resource;
        public RoutingResult route;
    }

    public interface IMove
    {
       
    }

    public class AssignMove : IMove
    {
        public ResourceView Resource;
        public IncidentView Incident;
        public RoutingResult route;

        public override string ToString()
        {
            if (Resource.Callsign != null && Incident != null)
            {
                // calculate distance.
                int distance = Resource.RoutingPoint.CompareTo(new RoutingPoint(Incident.Easting, Incident.Northing));

                return String.Format("Assign {0}->{1} meters={2,5}", Resource.Callsign, Incident, distance);
            }
            return base.ToString();
        }
    }

    public class RelocateMove : IMove
    {
        public ResourceView Resource;
        public DestinationView dest;
        public RoutingResult route;

        public override string ToString()
        {
            if (Resource != null && dest != null)
            {
                RoutingPoint res = new RoutingPoint(Resource.Easting, Resource.Northing);
                int distance = res.CompareTo(new RoutingPoint(dest.e, dest.n));
                return String.Format("Move {0}->{1} meters={2}", Resource.Callsign, dest.Destination , distance);
            }

            return base.ToString();
        }
    }

}
