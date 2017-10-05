﻿using System;
using System.Linq;
using System.Diagnostics;
using Quest.Common.Messages;
using Quest.Lib.ServiceBus;
using Quest.Lib.Processor;
using Quest.Lib.Utils;
using Quest.Common.ServiceBus;
using Quest.Lib.Simulation.DataModelSim;
using Quest.Lib.Data;

namespace Quest.Lib.Simulation.Resources
{
    /// <summary>
    /// Pumps historic resources into the system.
    /// </summary>
    public class ResSimulator : ServiceBusProcessor
    {
        private DateTime _lastResourceId;

        public int quantity { get; set; } = 0;
        public int lowWatermark { get; set; } = 0;
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }

        private IResourceStore _resourceStore;
        private IDatabaseFactory _dbFactory;


        public ResSimulator(
            IResourceStore resourceStore, 
            IServiceBusClient serviceBusClient,
            IDatabaseFactory dbFactory,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue,serviceBusClient, msgHandler)
        {
            _dbFactory = dbFactory;
            _resourceStore = resourceStore;
        }
    
        protected override void OnPrepare()
        {
            _lastResourceId = DateTime.MinValue;

            LogMessage($"Parameters Quantity={quantity} LowWatermark:{lowWatermark} StartTime={startTime} EndTime={endTime}", TraceEventType.Warning);
        }

        protected override void OnStart()
        {
            LowWaterResources();
        }

        protected override void OnStop()
        {
        }

        private void LowWaterResources()
        {
            LogMessage($"Low watermark, loading {quantity} Resources", TraceEventType.Warning);

            _dbFactory.Execute<QuestSimContext>((db) =>
            {
                int quantity = this.quantity;

                // get 'quantity' Resources from a specific Resource number
                var data = _resourceStore.GetHistoricResources(_lastResourceId, quantity, startTime, endTime);

                int count = data.Count();
                foreach (var i in data)
                {
                    count--;

                    ResourceUpdate msg = new ResourceUpdate()
                    {
                        Callsign = i.Callsign,
                        ResourceType = i.VehicleTypeId == 1 ? "AEU" : "FRU",
                        Status = i.Status,
                        Latitude = 0,
                        Longitude = 0,
                        Speed = i.Speed ?? 0,
                        Direction = i.Direction ?? 0,
                        Skill = "",
                        UpdateTime = i.AvlsDateTime ?? DateTime.MinValue,
                        FleetNo = i.FleetNumber.ToString(),
                        Incident = i.IncidentId.ToString(),
                        Emergency = false,
                        Destination = "",
                        Agency = "",
                        Class = "",
                        EventType = ""
                    };

                    if (i.X != 0)
                    {
                        var c = LatLongConverter.OSRefToWGS84(i.X ?? 0, i.Y ?? 0);
                        msg.Latitude = c.Latitude;
                        msg.Longitude = c.Longitude;
                    }

                    SetTimedMessage($"RESNEW-{msg.Callsign}", msg.UpdateTime, msg);

                    _lastResourceId = msg.UpdateTime;

                    // add a trigger for when the number of incs goes below a certain amount
                    if (count == lowWatermark)
                        SetTimedEvent($"RESLOWWATER-{msg.UpdateTime}", msg.UpdateTime, () => LowWaterResources());
                }
                LogMessage($"Low watermark, pumped {count} Resources", TraceEventType.Information);
            });
        }


    } // End of Class

} //End of Namespace
