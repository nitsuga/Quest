////////////////////////////////////////////////////////////////////////////////////////////////
//   
//   Copyright (C) 2016 Extent Ltd. Copying is only allowed with the express permission of Extent Ltd
//   
//   Use of this code is not permitted without a valid license from Extent Ltd
//   
////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
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
    [Export("XReplayPlayer", typeof(JobProcessor))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class XReplayPlayer : JobProcessor
    {
        [Import]
        private TimedEventQueue _eventQueue;

        // Offset to add to event records to bring them back to EISECSimulator time
        DateTime incOffsetTime = DateTime.MinValue;
        DateTime resOffsetTime = DateTime.MinValue;

        private DateTime _startTime;

        private readonly int _records = 1000;

        private readonly int _secondshead = 15;

        private JobInstance _data;

        protected override void Start(JobInstance data)
        {
            _data = data;

            var t = Task.Factory.StartNew(() =>
            {
                try
                {
                    Initialise();

                    SetMessage(data, "XReplayPlayer initialised", GetType().Name);

                    Wait();
                }
                catch (Exception ex)
                {
                    SetMessage(data, $"Failed: {ex.Message}", GetType().Name, TraceEventType.Error);
                }
            });

            // when complete - update the observable on the UI thread
            t.ContinueWith(x =>
            {
                Completed(data);
            });
        }

        private void Initialise()
        {
            _startTime = _eventQueue.Now;


            Logger.Write("Initialisation complete", LoggingPolicy.Category.Trace , TraceEventType.Information, "XReplayPlayer");

            // get the start time of the records and calculate the offset to bring database records to sim time.
            GetBaseTime();

            //Thread.Sleep(5000);

            LowWaterIncidents(null);
            LowWaterResource(null);
        }


        private void GetBaseTime()
        {
            using (QuestEntities db = new QuestEntities())
            {
                using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                {
                    connection.Open();
                    var queryString = "select Top 1 DATE_AND_TIME from SimIncident";
                    using (var command = new SqlCommand(queryString, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var time = reader.GetDateTime(0);
                                incOffsetTime = time;
                            }
                        }
                    }

                    var queryString2 = "select Top 1 DATE_AND_TIME from SimResource";
                    using (var command = new SqlCommand(queryString2, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var time = reader.GetDateTime(0);
                                resOffsetTime = time;
                            }
                        }
                    }
                }
            }
        }

        //MsgId   |Workstation| |   |Serial       |Determinant|Status|Easting         |Northing            | |Location          |                   | |Priority|Sector|IncidentType|Complaint|Description||||||
        //19631507|cad1       | |PII|L210814000008|METPOL     |DSP   |51.4965045849101|-0.06055089708179556| |34 LOCKWOOD SQUARE|2014-08-21 00:02:18|E|R2      |N1DG  |R2          |METPOL   |E||
        // 0        1          2  3      4          6             7    8                      9            10         11                 12         13     14    15        16          17     18   
        private void PushIncident(TaskEntry te)
        {
            try
            {

                var msg = te.DataTag.ToString();
                var parts = msg.Split('|');

                switch (parts[3])
                {
                    case "PII":
                        double lat, lon;
                        double.TryParse(parts[7], out lat); // this is incorrect field in XC
                        double.TryParse(parts[8], out lon); // this is incorrect field in XC

                        // sometimes we get invalid location
                        if (lon < -7 || lat < 49)
                            break;

                        var result = LatLongConverter.WGS84ToOSRef(lat, lon);
                        if (result != null) //just skipt errorneous cooridinates
                        {
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
                                Serial = parts[4],
                            };

                            ServiceBusClient.Broadcast(item);
                        }
                        break;

                    case "DI":
                        //19637953|cad1||DI|L200814004279|
                        // 000      1   2 3   4
                        var ci = new CloseIncident
                        {
                            Serial = parts[4]
                        };

                        ServiceBusClient.Broadcast(ci);
                        break;
                }
            }
            catch (Exception ex)
            {
                SetMessage(_data, $"XReplayPlayer error:  {ex}", GetType().Name);
            }

        }

        private void LowWaterIncidents(TaskEntry te)
        {
            try
            {
                var count = 0;
                var lastId = 0;
                var fireTime = DateTime.MinValue;

                // get _secondshead seconds of data

                DateTime targetTime = _eventQueue.Now.AddSeconds(_secondshead);

                if (te != null && te.DataTag != null)
                    lastId = (int)te.DataTag;

                using (QuestEntities db = new QuestEntities())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        // get a block of incidents
                        var queryString = $"select top {_records} C_ID, MESSAGE,DATE_AND_TIME from SimIncident where C_ID>={lastId} order by C_ID"  ;

                        using (var command = new SqlCommand(queryString, connection))
                        {
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    lastId = reader.GetInt32(0);
                                    var message = reader.GetString(1);
                                    var time = reader.GetDateTime(2);

                                    if (time < incOffsetTime)
                                        continue;

                                    fireTime = _startTime.Add(time - incOffsetTime);

                                    // push events onto the queue
                                    var taskEntry = new TaskEntry(_eventQueue, new TaskKey("Incident " + lastId, "MARK"),
                                        PushIncident, message,
                                        fireTime);
                                    count++;
                                    if (time >= targetTime)
                                        break;

                                }
                            }
                        }
                        var entry = new TaskEntry(_eventQueue, new TaskKey("LowIncWatermark", "MARK"), LowWaterIncidents,
                            lastId,
                            fireTime);
                    }
                }
                SetMessage(_data, $"XReplayPlayer pumped {count} incidents", GetType().Name);

            }
            catch (Exception ex)
            {
                SetMessage(_data, $"XReplayPlayer error:  {ex}", GetType().Name);
            }

        }

        //MsgId|Workstation|||Callsign|ResourceType|Status|Easting|Northing||Incident||Speed|Direction|LastUpdate|FleetNo|Skill||Sector|Emergency|Destination|Agency|Class|EventType
        // 19630482|cad1||PRI|S185|AEU|TRN|51.458719179383436|-0.19411379421266495||L200814004232||25|90|2014-08-233 00:00:27|7098|L|AEU|S1DG|N|STGEO:ST GEORGES HOSPITAL, BLACKSHAW ROAD, SW17|L|E|17D3|N/A||
        //  0        1   2 3   4    5    6   7                         8           9  10         1112 13   14                  15  16 17  18  19   20                                           2122 23   24
        private void PushResource(TaskEntry te)
        {
            try
            {


                var msg = te.DataTag.ToString();
                var parts = msg.Split('|');

                switch (parts[3])
                {
                    case "PRI":
                        double lat, lon;
                        double.TryParse(parts[7], out lat); // this is incorrect field in XC
                        double.TryParse(parts[8], out lon); // this is incorrect field in XC

                        // sometimes we get invalid location
                        if (lon < -7 || lat < 49)
                            break;

                        if (lat != 0 || lon != 0)
                        {
                            var result = LatLongConverter.WGS84ToOSRef(lat, lon);
                            if (result != null)
                            {
                                var geom = GeomUtils.ConvertToGeometryString(result);

                                int speed, fleetno, direction;
                                DateTime update;

                                int.TryParse(parts[15], out fleetno);
                                int.TryParse(parts[12], out speed);
                                int.TryParse(parts[13], out direction);
                                DateTime.TryParse(parts[14], out update);

                                var item = new ResourceUpdate
                                {
                                    Agency = parts[21],
                                    Callsign = parts[4],
                                    Class = parts[22],
                                    Destination = parts[20],
                                    Direction = direction,
                                    Incident = parts[10],
                                    Emergency = parts[19],
                                    EventType = parts[25],
                                    FleetNo = fleetno,
                                    Geometry = geom,
                                    LastUpdate = DateTime.Now,
                                    ResourceType = parts[5],
                                    Sector = parts[18],
                                    Skill = parts[16],
                                    Speed = speed,
                                    Status = parts[6],
                                };

                                ServiceBusClient.Broadcast(item);
                            }
                        }
                        break;

                    case "DR":
                        // MsgId|Workstation|||Callsign
                        var dr = new DeleteResource
                        {
                            Callsign = parts[4]
                        };

                        ServiceBusClient.Broadcast(dr);
                        break;
                }
            }
            catch (Exception ex)
            {
                SetMessage(_data, $"XReplayPlayer error:  {ex}", GetType().Name);
            }

        }

        private void LowWaterResource(TaskEntry te)
        {
            try
            {

                var lastId = 0;
                var fireTime = DateTime.MinValue;

                if (te != null && te.DataTag != null)
                    lastId = (int)te.DataTag;

                DateTime targetTime = _eventQueue.Now.AddSeconds(_secondshead);

                int count=0;

                using (QuestEntities db = new QuestEntities())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();


                        // get a block of incidents
                        var queryString = $"select top {_records} C_ID, MESSAGE,DATE_AND_TIME from SimResource where C_ID>={lastId}  order by C_ID";

                        using (var command = new SqlCommand(queryString, connection))
                        {
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    lastId = reader.GetInt32(0);
                                    var message = reader.GetString(1);
                                    var time = reader.GetDateTime(2);

                                    if (time < resOffsetTime)
                                        continue;

                                    fireTime = _startTime.Add(time - resOffsetTime);
                                    //if (message.Contains("G122"))
                                    // push events onto the queue
                                    new TaskEntry(_eventQueue, new TaskKey("Resource " + lastId, "MARK"), PushResource,
                                        message,
                                        fireTime);
                                    count++;

                                    if (time >= targetTime)
                                        break;

                                }
                            }
                        }

                        new TaskEntry(_eventQueue, new TaskKey("LowResWatermark", "MARK"), LowWaterResource, lastId,
                            fireTime);
                    }
                }

                SetMessage(_data, $"XReplayPlayer pumped {count} resources", GetType().Name);
            }
            catch (Exception ex)
            {
                SetMessage(_data, $"XReplayPlayer error:  {ex}", GetType().Name);
            }

        }
    } // End of Class
} //End of Namespace