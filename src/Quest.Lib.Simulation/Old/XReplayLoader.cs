////////////////////////////////////////////////////////////////////////////////////////////////
//   
//   Copyright (C) 2014 Extent Ltd. Copying is only allowed with the express permission of Extent Ltd
//   
//   Use of this code is not permitted without a valid license from Extent Ltd
//   
////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Quest.Lib.ServiceBus.Messages;
using Quest.Lib.Utils;

namespace Quest.Lib.Simulation
{
    /// <summary>
    ///     This module reads resource and incident data from
    /// </summary>
    public class XReplayLoader
    {
        // string conn_string = "Data Source=86.29.75.151;Initial Catalog=xreplay3;user=questuser;password=quest";
        private readonly string conn_string = "Data Source=.;Initial Catalog=xreplay3;user=sa;password=M3Gurdy*";

        public void Import()
        {
            Task[] alltasks = {new Task(PushIncidents), new Task(PushResources)};
            alltasks.ToList().ForEach(x => x.Start());
            Task.WaitAll(alltasks);
        }

        private void PushIncidents()
        {
            using (var connection = new SqlConnection(conn_string))
            {
                connection.Open();

                // get a block of incidents
                var queryString = "select * from INCIDENT_LOG";

                using (var command = new SqlCommand(queryString, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var message = reader.GetString(1);
                            PushIncident(message);
                        }
                    }
                }
            }
        }

        private void PushResources()
        {
            using (var connection = new SqlConnection(conn_string))
            {
                connection.Open();

                // get a block of incidents
                var queryString = "select * from RESOURCE_LOG";

                using (var command = new SqlCommand(queryString, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var message = reader.GetString(1);

                            PushResource(message);
                        }
                    }
                }
            }
        }


        //MsgId   |Workstation| |   |Serial       |Determinant|Status|Easting         |Northing            | |Location          |                   | |Priority|Sector|IncidentType|Complaint|Description||||||
        //19631507|cad1       | |PII|L210814000008|METPOL     |DSP   |51.4965045849101|-0.06055089708179556| |34 LOCKWOOD SQUARE|2014-08-21 00:02:18|E|R2      |N1DG  |R2          |METPOL   |E||
        // 0        1          2  3      4          6             7    8                      9            10         11                 12         13     14    15        16          17     18   
        private void PushIncident(string msg)
        {
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


                    break;

                case "DI":
                    //19637953|cad1||DI|L200814004279|
                    // 000      1   2 3   4
                    var ci = new CloseIncident
                    {
                        Serial = parts[4]
                    };

                    break;
            }
        }

        //MsgId|Workstation|||Callsign|ResourceType|Status|Easting|Northing||Incident||Speed|Direction|LastUpdate|FleetNo|Skill||Sector|Emergency|Destination|Agency|Class|EventType
        // 19630482|cad1||PRI|S185|AEU|TRN|51.458719179383436|-0.19411379421266495||L200814004232||25|90|2014-08-233 00:00:27|7098|L|AEU|S1DG|N|STGEO:ST GEORGES HOSPITAL, BLACKSHAW ROAD, SW17|L|E|17D3|N/A||
        //  0        1   2 3   4    5    6   7                         8           9  10         1112 13   14                  15  16 17  18  19   20                                           2122 23   24
        private void PushResource(string msg)
        {
            var parts = msg.Split('|');

            switch (parts[3])
            {
                case "PRI":
                    double lat, lon;
                    double.TryParse(parts[7], out lat); // this is incorrect field in XC
                    double.TryParse(parts[8], out lon); // this is incorrect field in XC

                    var result = LatLongConverter.WGS84ToOSRef(lat, lon);
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
                        Status = parts[6]
                    };

                    break;

                case "DR":
                    // MsgId|Workstation|||Callsign
                    var dr = new DeleteResource
                    {
                        Callsign = parts[4]
                    };

                    var item2 = new ResourceUpdate
                    {
                        Callsign = parts[4]
                    };

                    break;
            }
        }
    } // End of Class
} //End of Namespace