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
    /// This class holds a snapshot of resourcees and incidents at any one time. It can then calculate which moves are possible,
    /// and then weight them by pretending to make the move.
    /// </summary>
    public class ARD2Board
    {
        private int _generation;
        private int _generationLimit;
        private string _name;
        private double _waitingFactor;
        private int _maxDistance;
        private int _maxDuration;
        private double _enrouteFactor = 1.1;
        private int _instanceMax = 20;
        private readonly List<ARD2Board> _children = new List<ARD2Board>();
        private DateTime _currentTime;
        private double[] weights;

        enum ResourceStatusCode
        {
            Waiting,
            Enroute,
        }

        public IResourceCalculator Calculator;

        public IRouteEngine Router;

        public ARD2Board(double[] weights)
        {
            this.weights = weights;
        }

        // max travel time to nearby standby point for a resource
        private int _maxStandbyRelocateTime = 60 * 8;

        // maximum number of standby points to look for for each resource when looking for nearby standby points
        private int _maxStandbys = 3;
        private CoverageMap _currentCoverage;
        private CoverageMap _incidentDensity;
        private double _value = double.MaxValue;
        private double _delta = double.MinValue;
        private double _incsum = 0;

        public CoverageMap CurrentCoverage
        {
            get { return _currentCoverage; }
            set { _currentCoverage = value; }
        }

        public CoverageMap IncidentDensity
        {
            get { return _incidentDensity; }
            set { 
                _incidentDensity = value;
                if (_incidentDensity!=null)
                _incsum = CoverageMapUtil.Sum(_incidentDensity);
            }
        }
        
        public ARD2Board Parent = null;

        public IMove AppliedMove;

        /// <summary>
        /// a list of live incidents 
        /// </summary>
        public List<QuestIncident> Incidents;

        /// <summary>
        /// a list of resources
        /// </summary>
        public List<QuestResource> Resources;

        /// <summary>
        /// the current time of the board
        /// </summary>
        public DateTime CurrentTime
        {
            get { return _currentTime; }
            set { _currentTime = value; }
        }

        /// <summary>
        /// The number of resources to consider for standby point reassignment
        /// </summary>
        public int WaitingResourceThreshold { get; set; }

        /// <summary>
        ///  factor to be applied to resources being considered for enroute assignment as opposed to 
        ///  waiting assignment.
        /// </summary>
        public double WaitingFactor
        {
            get { return _waitingFactor; }
            set { _waitingFactor = value; }
        }

        /// <summary>
        ///  a list of destination standby points/hospitals etc.
        /// </summary>
        public List<RoutingPoint> Destinations;

        /// <summary>
        /// a static map of coverages if a vehicle is sent to a standby point.
        /// </summary>
        public Dictionary<int, DestinationCoverage> DestinationMap;

        public int InstanceMax
        {
            get { return _instanceMax; }
            set { _instanceMax = value; }
        }


        /// <summary>
        /// maximum distance for a resource to travel when being asssigned to an incident
        /// </summary>
        public int MaxDistance
        {
            get { return _maxDistance; }
            set { _maxDistance = value; }
        }

        /// <summary>
        /// maximum time lag for a resource to be assigned to an incident
        /// </summary>
        public int MaxDuration
        {
            get { return _maxDuration; }
            set { _maxDuration = value; }
        }

        /// <summary>
        /// a list of child boards. one child board gets built for each considered move 
        /// </summary>
        public IEnumerable<ARD2Board> Children
        {
            get
            {
                AddChildBoards();
                return _children;
            }
        }

        /// <summary>
        ///  carry out assignment but this is as if we make the move now, and not what
        ///  the result of the move would be in the future
        /// </summary>
        /// <param name="m"></param>
        private void ApplyMove(IMove m)
        {
            AssignMove am = m as AssignMove;
            RelocateMove rm = m as RelocateMove;

            AppliedMove = m;

            // carry out assignment move
            if (am != null)
            {
                ApplyIncidentAllocationMove(am);
            }


            if (rm != null)
            {
                ApplyStandbyPointMove(rm);
            }
        }

        /// <summary>
        /// Apply the move to the board.. this *does not* apply the move in reality!
        /// </summary>
        /// <param name="am"></param>
        void ApplyIncidentAllocationMove(AssignMove am)
        {
            // get resource
            var res = (from r in Resources where r.Callsign == am.Resource.Callsign select r).FirstOrDefault();
            var inc = (from i in Incidents where i.Incidentid == am.Incident.Incidentid select i).FirstOrDefault();

            bool isReassignment = res.BusyEnroute == true;

            // reassigned from another job?
            if (isReassignment)
            {
                // cancel off original job first
                RemoveResource(res.Incident, res);
            }

            // update the resource statue and update the coverage
            res.Status = ResourceStatusCode.Enroute.ToString();
            inc.Status = ResourceStatusCode.Enroute.ToString();
            res.Serial = inc.Serial;
            res.ETA = DateTime.Now + new TimeSpan(0,0, (int)am.route.Duration);
            //res.DTG = (int)am.route.Distance;

            /// add the resource to the incident
            AssignedResource resRecord = new AssignedResource() { Resource = res , Dispatched = null, Onscene = null, Convey = null, Enroute = null, Hospital = null, Released = null };
            if (inc.AssignedResources == null)
                inc.AssignedResources = new AssignedResource[] { resRecord };
            else
            {
                List<AssignedResource> list = inc.AssignedResources.ToList();
                list.Add(resRecord);
                inc.AssignedResources = list.ToArray();
            }

            // remove old coverage from global if the resource was original available.
            if (!isReassignment)
                if (res.map != null)
                    _currentCoverage.Subtract(res.map);
        }


        /// <summary>
        /// remove an assigned resource from an incident
        /// </summary>
        /// <param name="incident"></param>
        /// <param name="resource"></param>
        void RemoveResource(IncidentView incident, ResourceView resource)
        {
            // remove the assigned resource from the list
            AssignedResource ar = (from a in incident.AssignedResources where a.Resource.Callsign == resource.Callsign select a).FirstOrDefault();
            List<AssignedResource> list = new List<AssignedResource>(incident.AssignedResources);
            list.Remove(ar);
            incident.AssignedResources = list.ToArray();
            resource.ETA = null;
            resource.Status = ResourceStatusCode.Waiting.ToString();
        }


        void ApplyStandbyPointMove(RelocateMove rm)
        {
            ResourceView res = (from r in Resources where r.Callsign == rm.Resource.Callsign select r).FirstOrDefault();

            // move the resource and update the coverage
            res.Easting = rm.dest.e;
            res.Northing = rm.dest.n;
            res.RoutingPoint = new RoutingPoint() { X = (int)res.Easting, Y = (int)res.Northing };

            // add new coverage assuming the vehicle is at the standby point. The coverage for 
            // the standby point is built at startup and cached.
            CoverageMap dc = DestinationMap[res.VehicleType][rm.dest.DestinationId];

            if (res.map != null && dc != null)
            {
                CoverageMapUtil.Move(res.map, dc, _currentCoverage);
                res.map = CoverageMapUtil.Clone(dc);
            }
            else
            {
                // remove old coverage from global
                if (res.map != null)
                    _currentCoverage.Subtract(res.map);

                if (dc != null)
                {
                    _currentCoverage.Add(dc);
                    res.map = dc.Clone();
                }
            }
        }

        // the coverage is calculated as sum( incidentdensity * coverage )
        private double CalcCoverage()
        {
            if (Resources.Count == 0 || Resources[0].map == null || _currentCoverage==null)
                return 0;

            // count locations with 0 as these are uncovered
            double sum = CoverageMapUtil.Multiply(_currentCoverage, _incidentDensity);

            //Debug.Print("Coverage : {0} {1} {2}", sum, _incsum, sum / _incsum);
            double coverage = sum / _incsum;

            return coverage;
        }

        /// <summary>
        /// The value of this board calculated using the objective function
        /// </summary>
        public double Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        /// <summary>
        /// The difference between this boards objective function before and after the applied move was made.
        /// </summary>
        public double Delta
        {
            get
            {
                return _delta;
            }
            set
            {
                _delta = value;
            }
        }

        /// <summary>
        /// This is the objective function. Calculate the value of this board using the weights provided.
        /// The parameters are:
        /// 1. The number of Cat A calling waiting
        /// 2. The number of Cat B calling waiting
        /// 3. Coverage
        /// 4. average drive time to Cat A
        /// 5. average drive time to Cat B
        /// </summary>
        /// <returns></returns>
        public double CalcValue(String description)
        {
            double[] basis = new double[5];

            // count the number of P1
            basis[0] = (from i in Incidents where i.Category == "A" && i.IsWaiting == true select i).Count();

            // count P2
            basis[1] = (from i in Incidents where i.Category != "A" && i.IsWaiting == true select i).Count();

            if (weights.Length > 2)
            {
                // calculate total coverage
                basis[2] = CalcCoverage();
            }

            if (weights.Length > 3)
            {
                // calc average travel time
                var tt1c = from r in Resources where r.BusyEnroute == true && r.IncidentPriorityOrdinal != 1 && r.ETA != null select (DateTime.Now - (DateTime)r.ETA).TotalMinutes;
                if (tt1c.Count() != 0)
                    basis[3] = tt1c.Sum();
            }

            if (weights.Length > 4)
            {
                // calc average travel time
                var tt2c = from r in Resources where r.BusyEnroute == true && r.IncidentPriorityOrdinal != 1 && r.ETA != null select (DateTime.Now - (DateTime)r.ETA).TotalMinutes;
                if (tt2c.Count() != 0)
                    basis[4] = tt2c.Sum();
            }

            // calc final weight
            double finalweight = 0.0;
            for (int i = 0; i < weights.Length; i++)
            {
                finalweight += (basis[i] * weights[i]);
            }
            return finalweight;
        }

        /// <summary>
        /// clone this board including all its incidents and resources
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new ARD2Board(weights)
            {
                GenerationLimit = _generationLimit,
                CurrentCoverage = CoverageMapUtil.Clone(this.CurrentCoverage),
                IncidentDensity = this.IncidentDensity,
                DestinationMap = this.DestinationMap,
                Destinations = this.Destinations,
                Incidents = ( from i in this.Incidents select (IncidentView)i.Clone()).ToList(),
                Resources = ( from i in this.Resources select (ResourceView)i.Clone()).ToList(),
                MaxDistance = this.MaxDistance,
                MaxDuration = this.MaxDuration,
                InstanceMax = this.InstanceMax,
                CurrentTime = this.CurrentTime,
                WaitingFactor = this.WaitingFactor
            };
        }

        public String Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public int Generation
        {
            get
            {
                return _generation;
            }
            set
            {
                _generation = value;
            }
        }

        public int GenerationLimit
        {
            get
            {
                return _generationLimit;
            }
            set
            {
                _generationLimit = value;
            }
        }

        /// <summary>
        /// general strategy is to take a snapshot of the board and then work out which moves are possible.
        /// for each move, build another board, move the peice and then predict what will happen in n minutes.
        /// the process is then repeated
        /// </summary>
        private void AddChildBoards()
        {
            if (Generation < GenerationLimit)
            {
                // haven't reached the right depth.. find best child
                // calculate a list of posible moves
                List<IMove> moves1 = new List<IMove>();
                List<IMove> moves2 = new List<IMove>();

                moves1 = CalculateIncidentAssignmentMoves();

                //if (moves1.Count==0)
                    moves2 = CalculateStandbyAssignmentMoves();

                int movenum = 0;
                // apply each move in turn and figure out the best one
                foreach (IMove m in moves1.Union(moves2))
                {
                    movenum++;
                    ARD2Board childBoard = (ARD2Board)Clone();
                    childBoard.Parent = this;
                    childBoard.Generation = Generation + 1;
                    childBoard.Name = Name + "." + movenum.ToString();
                    childBoard.AppliedMove = m;
                    _children.Add(childBoard);
                }

                
                // apply each move in turn and figure out the best one                
                foreach (ARD2Board childBoard in _children.AsParallel())
                {
                    // apply the move

                    // update the value of this board
                    double before = childBoard.CalcValue(childBoard.AppliedMove + " (before)");

                    childBoard.ApplyMove(childBoard.AppliedMove);

                    // update the value of this board
                    childBoard.Value = childBoard.CalcValue(childBoard.AppliedMove + "  (after)");
                    childBoard.Delta = childBoard.Value - before;

                }
            }
        }

        List<IMove> CalculateStandbyAssignmentMoves()
        {
            List<IMove> moves = new List<IMove>();
            var waitingResources = Resources.Where(x => x.Status == ResourceStatusCode.Waiting.ToString() && x.Speed == 0 );
            if (waitingResources.Count() < WaitingResourceThreshold)
                foreach (ResourceView r in waitingResources.Take(WaitingResourceThreshold))
                    moves.AddRange(CalculateStandbyAssignmentMoves(r));

            return moves;

        }

        /// <summary>
        /// for each available resource build a list of standby points > certain range, say 1km (no point moving otherwise)
        /// find the nearest destination in the list to this Vehicle 
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="destinations"></param>
        /// <returns></returns>
        List<IMove> CalculateStandbyAssignmentMoves(ResourceView r)
        {
            List<IMove> moves = new List<IMove>();
            const int MINDISTANCE = 500;

            RouteRequestMultiple request = new Routing.RouteRequestMultiple()
                {
                    StartLocation = new RoutingPoint() {  X = (int)r.Easting, Y = (int)r.Northing },
                    EndLocations = Destinations,
                    DistanceMax = double.MaxValue,
                    DurationMax = _maxStandbyRelocateTime,
                    InstanceMax = _maxStandbys,
                    MakeRoute = false,
                    VehicleType = r.VehicleType,
                    SearchType = SearchType.Quickest,
                    Hour = DateTime.Now.Hour,
                    Map = null
                };

                RoutingResults result = Router.CalculateRouteMultiple(request);

                if (result == null || result.items.Count() == 0)
                    return moves;

                // add the nearest destinations to the list
                foreach (RoutingResult rr in result.items.Where(x => x.Distance > MINDISTANCE && (x.Tag as DestinationView).Destination != r.NearestStandby))
                    moves.Add(new RelocateMove() { dest = (DestinationView)rr.Tag, Resource = r, route = rr });



            return moves;

        }

        /// <summary>
        /// generate a list of moves to acheive resourcing 
        /// </summary>
        /// <returns></returns>
        List<IMove> CalculateIncidentAssignmentMoves()
        {
            List<IMove> moves = new List<IMove>();
            try
            {
            
                // build map of outstanding incidents and try and assign them
                // its sorted by priority and then age
                var waiting_incs = from i in Incidents
                                   where (i.ExcludeFromHeldCalls ==false ) && i.Category != "X"
                                   orderby i.Status, i.Category, i.Created
                                   select i;

                // for each incident find a combination of resources to check
                if (waiting_incs.Count() > 0)
                    foreach (QuestIncident inc in waiting_incs)
                    {
                        int frucount = 0;
                        int ambcount = 0;

                        Calculator.CalculateResourceRequired(inc);

                        ambcount = (int)inc.AMBAssigned;
                        frucount = (int)inc.FRUAssigned;

                        int totalcount = ambcount + frucount;

                        if (totalcount < inc.TotalRequired)
                        {
                            if (frucount < inc.FRURequired)
                                AddMoves(moves, inc, "FRU");

                            if (ambcount < inc.AMBRequired)
                                AddMoves(moves, inc, "AMB");
                        }
                    }
            }
            catch (Exception ex)
            {                
            }
            finally
            {
            }
            return moves;
        }

        /// <summary>
        /// add potential moves for resource type for a given incident
        /// </summary>
        /// <param name="moves"></param>
        /// <param name="inc"></param>
        /// <param name="vehicleType"></param>
        void AddMoves(List<IMove> moves, IncidentView inc, String vehicleType)
        {
            IOrderedEnumerable<CandidateResource> list = ARDCommon.GetBestAppropriateResources(Router, CurrentTime.Hour, (int)inc.Easting, (int)inc.Northing, vehicleType, _maxDistance, _maxDuration, (int)inc.Ordinal, Resources, _instanceMax, _enrouteFactor, _waitingFactor);
            if (list != null)
                foreach (CandidateResource r in list)
                    moves.Add(new AssignMove() { Incident = inc, Resource = r.resource, route = r.route });
        }


    }
}

