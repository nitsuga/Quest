using GeoAPI.Geometries;
using Quest.Common.Messages;
using Quest.Common.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quest.Lib.Simulation.Resources
{
    public class SimResourceManager
    {
        public int atDestinationRange { get; set; }
        public int GPSFrequencySecs { get; set; }
        public int IncidentResponseSpeed { get; set; }

        public List<SimResource> Resources;

        private IResourceStore _resourceStore;

        public SimResourceManager(IResourceStore resourceStore)
        {
            _resourceStore = resourceStore;
        }

        public SimResource FindResource(int resourceId)
        {
            return Resources.Where(x => x.ResourceId == resourceId).FirstOrDefault();
        }

        public SimResource FindResource(String callsign)
        {
            return Resources.Where(x => x.Callsign == callsign).FirstOrDefault();
        }

        public SimResource MakeResource(string callsign, Coordinate position, string vehicleType)
        {
            return new SimResource()
            {
                AcceptCancellation = true,
                Callsign = callsign,
                Enabled = true,
                OnDuty = false,
                LastTxtMessage = "",
                LastMdtMessage = "",
                LastSysMessage = "",
                Incident = null,
                Location = "",
                VehicleType = vehicleType,
                NonConveyCode = 0,
                PoweredOn = false,
                IncidentAccept = true,
                IncidentResponseSpeed = IncidentResponseSpeed, // respond within n seconds to new incidents.
                Position = position,
                AtDestinationRange = atDestinationRange,
                Status = ResourceStatus.Off,
                GPSFrequency = GPSFrequencySecs,
                LastChanged = DateTime.Now
            };
        }

        /// <summary>
        /// load initial set of resources from vehicle store
        /// </summary>
        public void LoadResources()
        {
            Resources.Clear();

            var vehicles = _resourceStore.GetVehicles();

            foreach (SimVehicle x in vehicles)
            {
                var resource = MakeResource(x.VehicleId.ToString(), x.Position, x.VehicleType);
                Resources.Add(resource);
            }
        }
    }
}

