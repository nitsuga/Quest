using System.Linq;
using System.Diagnostics;
using Quest.Lib.ServiceBus;
using Quest.Lib.Processor;
using Quest.Lib.Utils;
using Quest.Common.ServiceBus;
using Quest.Common.Simulation;

namespace Quest.Lib.Simulation.Incidents
{
    /// <summary>
    /// Used for pumping fake incidents
    /// </summary>
    public class IncSimulator : ServiceBusProcessor
    {
        private long _lastIncidentId = 0;

        public int quantity { get; set; } = 0;
        public int lowWatermark { get; set; } = 0;

        SimIncidentManager _incidentManager;
        private SimContext _context;

        public IncSimulator(
            SimContext context,
            SimIncidentManager incidentManager, 
            IServiceBusClient serviceBusClient, 
            MessageHandler msgHandler,
            TimedEventQueue eventQueue
            ) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _context = context;
            _incidentManager = incidentManager;
        }

        protected override void OnPrepare()
        {
            // create a list of actions associated with each object type arriving from the queue
            _lastIncidentId = -1;

            LogMessage($"Parameters Quantity={quantity} LowWatermark:{lowWatermark} StartTime={_context.StartDate} EndTime={_context.EndDate}", TraceEventType.Warning);
        }

        protected override void OnStart()
        {
            LowWaterIncidents();
        }

        protected override void OnStop()
        {
        }

        private void LowWaterIncidents()
        {
            LogMessage($"Low watermark, loading {quantity} incidents", TraceEventType.Warning);

            // get 'quantity' incidents from a specific incident number
            var incs = _incidentManager.GetIncidents(_lastIncidentId, quantity, _context.StartDate, _context.EndDate);

            int count = incs.Count();
            foreach (var i in incs)
            {
                count--;

                // create a template update
                SimIncidentUpdate inc = new SimIncidentUpdate()
                {
                    IncidentId = i.IncidentId,
                    CallStart = i.CallStart,
                    AMPDSTime = i.Ampdstime,
                    Easting = i.Easting,
                    Northing = i.Northing,
                    AMPDSCode = i.Ampdscode,
                    Category = i.Category,
                    WasConveyed = i.WasConveyed,
                    WasDispatched = i.WasDispatched,
                    OutsideLAS = i.OutsideLas,
                    UpdateTime = i.CallStart,
                    UpdateType = SimIncidentUpdate.UpdateTypes.CallStart
                };

                SetTimedMessage($"INCNEW-{i.IncidentId}", i.CallStart, inc);

                if (i.Ampdstime != null)
                {
                    inc.UpdateTime = i.Ampdstime;
                    inc.UpdateType = SimIncidentUpdate.UpdateTypes.AMPDS;
                    SetTimedMessage($"INCAMPDS-{i.IncidentId}", inc.UpdateTime, inc);
                }

                _lastIncidentId = i.IncidentId;

                // add a trigger for when the number of incs goes below a certain amount
                if (count == lowWatermark)
                    SetTimedEvent($"INCLOWWATER", inc.UpdateTime, () => LowWaterIncidents());
            }
            LogMessage($"Low watermark, pumped {count} incidents", TraceEventType.Information);
        }

    } // End of Class

} //End of Namespace
