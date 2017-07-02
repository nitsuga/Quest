////////////////////////////////////////////////////////////////////////////////////////////////
//   
//   Copyright (C) 2014 Extent Ltd. Copying is only allowed with the express permission of Extent Ltd
//   
//   Use of this code is not permitted without a valid license from Extent Ltd
//   
////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Quest.Lib.Routing;
using Quest.Lib.ServiceBus.Messages;

namespace Quest.Lib
{
    public partial class ResourceView : ICloneable
    {
        public Routing.RoutingPoint RoutingPoint;
        public CoverageMap map;
        public IncidentView Incident;
        public int VehicleType = 1;

        public override string ToString()
        {
            return String.Format("{0}", Callsign);
        }

        public object Clone()
        {

            ResourceView res = new ResourceView()
            {
                //TODO: fill in right bits to clone
#if false
                isClone = true,
                AcceptCancellation = this.AcceptCancellation,
                AtDestinationRange = this.AtDestinationRange,
                Callsign = this.Callsign,
                DestHospital = this.DestHospital,
                Destination = this.Destination,
                DTG = this.DTG,
                Easting = this.Easting,
                FleetId = this.FleetId,
                GPSFrequency = this.GPSFrequency,
                Heading = this.Heading,
                Incident = this.Incident,
                LastChanged = this.LastChanged,
                LastMdtMessage = this.LastMdtMessage,
                LastMessagePriority = this.LastMessagePriority,
                LastSysMessage = this.LastSysMessage,
                LastTxtMessage = this.LastTxtMessage,
                Location = this.Location,
                map = this.map,
                MPCName = this.MPCName,
                NonConveyCode = this.NonConveyCode,
                Northing = this.Northing,
                OnDuty = this.OnDuty,
                PoweredOn = this.PoweredOn,
                ResourceId = this.ResourceId,
                Skill = this.Skill,
                Speed = this.Speed,
                StandbyPoint = this.StandbyPoint,
                Status = this.Status,
                SysMessage = this.SysMessage,
                TTG = this.TTG,
                TxtMessage = this.TxtMessage,
                Enabled = this.Enabled,
                IncidentResponseSpeed = this.IncidentResponseSpeed,
                VehicleType = this.VehicleType,
                MdtMessage = this.MdtMessage,
                IncidentAccept = this.IncidentAccept
#endif
            };

            if (this.RoutingPoint != null)
                res.RoutingPoint = new RoutingPoint() { X = this.RoutingPoint.X, Y = this.RoutingPoint.Y, Identifier = this.RoutingPoint.Identifier, Tag = this.RoutingPoint.Tag };

            return res;
        }
    }
}
