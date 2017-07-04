////////////////////////////////////////////////////////////////////////////////////////////////
//   
//   Copyright (C) 2014 Extent Ltd. Copying is only allowed with the express permission of Extent Ltd
//   
//   Use of this code is not permitted without a valid license from Extent Ltd
//   
////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Quest.Lib.DataModel;
using Quest.Lib.Job;
using Quest.Lib.ServiceBus.Messages;
using Quest.Lib.Trace;
using Quest.Lib.Utils;

namespace Quest.Lib.Simulation
{
    /// <summary>
    ///     This module reads resource and incident data from
    /// </summary>
    [Export("Player", typeof(JobProcessor))]
    public class Player : JobProcessor
    {
        private const string QueueName = "Quest.Player";

        [Import] private TimedEventQueue _eventQueue;

        private MessageHelper _msgSource;

        // Offset to add to event records to bring them back to simulator time
        private TimeSpan offsetTime;
        private Dictionary<string, SimResource> resources = new Dictionary<string, SimResource>();

        public Player()
            : base("Player", Worker)
        {
        }

        /// <summary>
        ///     This routing gets called when requested to start.
        ///     Perform initialisation here
        /// </summary>
        /// <param name="module"></param>
        private static void Worker(JobProcessor module)
        {
            Logger.Write("Initialising", LoggingPolicy.Category.Trace, TraceEventType.Information, "XReplayPlayer");

            var me = module as Player;

            me.Initialise();

            me.Wait();
        }

        private void Initialise()
        {
            if (_eventQueue != null)
            {
                _eventQueue = new TimedEventQueue();
                _eventQueue.Now = DateTime.Now;
                _eventQueue.Speed = 1;
                _eventQueue.Start();
            }

            Logger.Write("Initialisation complete", LoggingPolicy.Category.Trace, TraceEventType.Information, "XReplayPlayer");

            // get the start time of the records and calculate the offset to bring database records to sim time.
            if (_eventQueue != null) offsetTime = _eventQueue.Now - GetBaseTime();

            LowWaterIncidents(null);
            LowWaterResource(null);
        }


        private DateTime GetBaseTime()
        {
            using (var db = new QuestEntities())
            {
                return db.FinalRawSpeedDatas.Min(x => x.TimeStamp);
            }
        }

        //MsgId   |Workstation| |   |Serial       |Determinant|Status|Easting         |Northing            | |Location          |                   | |Priority|Sector|IncidentType|Complaint|Description||||||
        //19631507|cad1       | |PII|L210814000008|METPOL     |DSP   |51.4965045849101|-0.06055089708179556| |34 LOCKWOOD SQUARE|2014-08-21 00:02:18|E|R2      |N1DG  |R2          |METPOL   |E||
        // 0        1          2  3      4          6             7    8                      9            10         11                 12         13     14    15        16          17     18   
        private void PushIncident(TaskEntry te)
        {
            var msg = te.DataTag.ToString();
            var parts = msg.Split('|');

            switch (parts[3])
            {
                case "PII":
                    double lat, lon;
                    double.TryParse(parts[7], out lat); // this is incorrect field in XC
                    double.TryParse(parts[8], out lon); // this is incorrect field in XC

                    var result = LatLongConverter.WGS84ToOSRef(lat, lon);
                    var geom = GeomUtils.ConvertToGeometryString(result);

                    DateTime update;
                    DateTime.TryParse(parts[11], out update);

                    var item = new IncidentUpdate
                    {
                        Geometry = geom,
                        Sector = parts[14],
                        Status = parts[6],
                        Complaint = parts[16],
                        Description = parts[17],
                        Determinant = parts[5],
                        IncidentType = parts[15],
                        Location = parts[10],
                        Priority = parts[13],
                        Serial = parts[4]
                    };

                    _msgSource.BroadcastMessage(item);

                    break;

                case "DI":
                    //19637953|cad1||DI|L200814004279|
                    // 000      1   2 3   4
                    var ci = new CloseIncident
                    {
                        Serial = parts[4]
                    };

                    _msgSource.BroadcastMessage(ci);
                    break;
            }
        }

        private void LowWaterIncidents(TaskEntry te)
        {
            var quantity = 500;
            var lastID = 0;
            var fireTime = DateTime.MinValue;

            if (te != null && te.DataTag != null)
                lastID = (int) te.DataTag;

            using (var db = new QuestEntities())
            {
                var id = lastID;
                var incs = db.Incidents.Where(x => x.IncidentID > id).OrderBy(x => x.IncidentID).Take(quantity);

                foreach (var inc in incs)
                {
                    lastID = inc.IncidentID;
                    var message = inc.IncidentID.ToString();
                    if (inc.Created != null)
                    {
                        var time = inc.Created.Value;

                        fireTime = time + offsetTime;

                        // push events onto the queue
                        var taskEntry = new TaskEntry(_eventQueue, new TaskKey("Incident " + lastID, "MARK"), PushIncident, message,
                            fireTime);
                    }
                }
                new TaskEntry(_eventQueue, new TaskKey("LowIncWatermark", "MARK"), LowWaterIncidents, lastID, fireTime);
            }
        }

        private void PushResource(TaskEntry te)
        {
            var id = (int) te.DataTag;

            using (var db = new QuestEntities())
            {
                var res = db.FinalRawSpeedDatas.FirstOrDefault(x => x.RawSpeedDataID == id);

                Incident inc = null;
                if (res.IncidentId != null)
                {
                    inc = db.Incidents.FirstOrDefault(x => x.IncidentID == res.IncidentId);
                }

                var item = new ResourceUpdate
                {
                    Agency = "",
                    Callsign = res.Callsign,
                    Class = "",
                    Destination = "",
                    Direction = res.Direction,
                    Incident = inc == null ? "" : inc.Serial,
                    Emergency = "",
                    EventType = inc == null ? "" : inc.Complaint,
                    FleetNo = res.VehicleId,
                    Geometry = res.latlon.ToString(),
                    LastUpdate = DateTime.Now,
                    ResourceType = res.VehicleId.ToString(),
                    Sector = "",
                    Skill = "",
                    Speed = res.Speed,
                    Status = res.Status
                };

                _msgSource.BroadcastMessage(item);
            }
        }

        private void LowWaterResource(TaskEntry te)
        {
            var quantity = 500;
            var lastID = 0;
            var fireTime = DateTime.MinValue;

            if (te != null && te.DataTag != null)
                lastID = (int) te.DataTag;

            using (var db = new QuestEntities())
            {
                var res =
                    db.FinalRawSpeedDatas.Where(x => x.RawSpeedDataID > lastID)
                        .OrderBy(x => x.RawSpeedDataID)
                        .Take(quantity);

                foreach (var r in res)
                {
                    lastID = r.RawSpeedDataID;
                    var time = r.TimeStamp;
                    fireTime = time + offsetTime;

                    // push events onto the queue
                    new TaskEntry(_eventQueue, new TaskKey("Resource " + lastID, "MARK"), PushResource, lastID, fireTime);
                }

                new TaskEntry(_eventQueue, new TaskKey("LowResWatermark", "MARK"), LowWaterResource, lastID, fireTime);
            }
        }
    } // End of Class
} //End of Namespace