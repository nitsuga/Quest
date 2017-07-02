////////////////////////////////////////////////////////////////////////////////////////////////
//   
//   Copyright (C) 2014 Extent Ltd. Copying is only allowed with the express permission of Extent Ltd
//   
//   Use of this code is not permitted without a valid license from Extent Ltd
//   
////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Quest.Lib.Routing;
using System.Diagnostics;

namespace Quest.Lib.AutoDispatch
{
    public class CoverageCalculator
    {
        static public void BuildCoverageMap(List<RoutingPoint> destinations,
                                            Dictionary<int, DestinationCoverage> destinationMap,
                                            IRouteEngine router,
                                            DestinationCoverage target,
                                            int vehicleTypeId,
                                            double maxDistance = double.MaxValue,
                                            double maxDuration = double.MaxValue,
                                            int tileSize = int.MaxValue
            )
        {
            using (QuestEntities context = new QuestEntities())
            {
                // build list of destinations for later use
                foreach (DestinationView d in context.DestinationViews)
                {
                    if (d.IsStandby == true)
                    {
                        RoutingPoint rp = new RoutingPoint() { X = (int)d.e, Y = (int)d.n, Tag = d };
                        destinations.Add(rp);

                        // calculate the coverage.. now returns map of minimum travel time in minutes / cell
                        var result = router.CalculateCoverage(new RouteRequestCoverage()
                                                                    {
                                                                        Name = d.Destination,
                                                                        DistanceMax = maxDistance,
                                                                        DurationMax = maxDuration,
                                                                        Hour = DateTime.Now.Hour,
                                                                        SearchType = SearchType.Quickest,
                                                                        StartPoints = new RoutingPoint[] { rp },
                                                                        TileSize = tileSize,
                                                                        VehicleType = vehicleTypeId
                                                                    }
                                                                    );

                        Logger.Write(string.Format("....coverage {0} ... {1}", d.Destination, result.Value.Coverage()), "Trace", 0, 0, TraceEventType.Information, "ARD");

                        target.Add(d.DestinationId, result.Value);
                    }
                }
            }
        }
    }
}
