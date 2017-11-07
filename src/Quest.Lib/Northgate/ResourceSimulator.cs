
using System;
using System.Linq;
using Quest.Lib.DataModel;
using Quest.Common.Messages;
using Quest.Lib.Processor;
using Quest.Lib.ServiceBus;
using Quest.Common.ServiceBus;
using Quest.Lib.Utils;
using Quest.Lib.Data;
using Quest.Common.Messages.Telephony;
using System.Threading;
using Quest.Common.Messages.Resource;
using Quest.Common.Messages.GIS;
using System.Collections.Generic;

namespace Quest.Lib.Trackers
{
    public class ResourceSimulator : ServiceBusProcessor
    {
        private IDatabaseFactory _dbFactory;
        private Timer t;

        string[] status = new string[] { "Available", "Busy", "Enroute", "Rest", "Offroad" };
        string[] statuscode = new string[] { "AOR", "TRN", "ENR", "RNA", "OOS" };


        public ResourceSimulator(
            IDatabaseFactory dbFactory,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _dbFactory = dbFactory;

        }

        protected override void OnPrepare()
        {
        }

        protected override void OnStart()
        {
            var counter = 0;
            var lat = 51.5;
            var lon = -0.2;
            var num = 10;

            List<QuestResource> vehicles = new List<QuestResource>();
            Random r = new Random();

            for (int i = 0; i < num; i++)
            {

                var x = r.NextDouble() - 0.5;
                var y = r.NextDouble() - 0.5;

                var veh = new QuestResource
                {
                    Callsign = $"A{i}",
                    FleetNo = $"VEH:A{i}",
                    Status = "AOR",
                    ResourceType = "AEU",
                    StatusCategory ="Available",
                    Position = new LatLongCoord { Longitude = lon + x, Latitude = lat + y }
                };

                vehicles.Add(veh);
            }

            var period =  (15.0 / (double)num) * 1000.0; // 15 secs per vehicle 

            t = new Timer((z) => {
                
                counter++;

                var i = (int)(r.NextDouble() * (double)num);

                var x = (r.NextDouble() - 0.5) * 0.002;
                var y = (r.NextDouble() - 0.5) * 0.002;

                var v = vehicles[i];
                v.StatusCategory = status[counter % 5]; 
                v.Status = statuscode[counter % 5];
                v.Position = new LatLongCoord { Longitude = v.Position.Longitude + x, Latitude = v.Position.Latitude + x };

                ServiceBusClient.Broadcast(new ResourceUpdateRequest()
                {
                    Resource = v,
                    UpdateTime = DateTime.Now
                });

            }, null, (int)period, (int)period);
        }

        protected override void OnStop()
        {
        }
    }
}
