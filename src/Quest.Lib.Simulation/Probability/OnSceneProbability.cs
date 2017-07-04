using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Quest.Lib.Utils;

namespace Quest.Lib.Simulation.Probability
{

    [Serializable]
    public class OnSceneProbability : ProbabilityEngine, IDisposable 
    {
        private SqlConnection _conn=null;

        public OnSceneProbability()
        {
        }

        public int GetAtSceneTime(string ampds, DateTime simTime, string vehicle)
        {
            ProbabilityEngine pe = GetProbability(0, ampds, simTime.Hour, vehicle);
            if (pe == null)
                return 0;
            return (int)pe.GetNextRand();
        }

        public int GetHospitalTime(string ampds, DateTime simTime, string vehicle)
        {
            ProbabilityEngine pe = GetProbability(1, ampds, simTime.Hour, vehicle);
            if (pe == null)
                return 0;
            return (int)pe.GetNextRand();
        }

        public void Dispose()
        {
            if (_conn != null)
            {
                _conn.Close();
                _conn.Dispose();
                _conn = null;
            }
        }


        /// <summary>
        /// Initialise a probablity engine from a specific database instance
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="ampds"></param>
        /// <param name="hour"></param>
        /// <param name="vehicle"></param>
        public ProbabilityEngine GetProbability(int cdfType, string ampds, int hour, string vehicle)
        {
            string query = string.Format("SELECT cdf from OnsceneStats where cdfType={0} and ampds='{1}' and hour={2} and vehicleTypeId={3}", cdfType, ampds, hour, vehicle);

            if (_conn.State != ConnectionState.Open)
                _conn.Open();

            using (SqlCommand cmd = new SqlCommand(query, _conn))
            {
                cmd.CommandTimeout = 0;
                String cdf = (String)cmd.ExecuteScalar();
                
                if (cdf == null)
                    return null;

                return (ProbabilityEngine)Serialiser.Deserialize(cdf, typeof(ProbabilityEngine));
            }
        }

        public static void CalculateOnsceneTimes(string inConnectionString, string outConnectionString)
        {
            ProbabilityEngine onscene = new ProbabilityEngine();
            ProbabilityEngine hospitalturnaround = new ProbabilityEngine();

            string query = "SELECT ampdscode,vehicletype ,datepart( hour, [callstart]) + (24 * datepart( weekday, [callstart])) as hourofday, [onscene], [hospitalturnaround] " +
                            "from activation, ampdscodes, callsign where activation.[ampdsKey] = ampdscodes.[ampdsKey] " +
                            "and activation.[callsignkey] = callsign.[callsignkey] " +
                            "order by vehicletype, hourofday, ampdscode ";

            using (SqlConnection conn = new SqlConnection(inConnectionString))
            {
                conn.Open();
                using (SqlConnection conn1 = new SqlConnection(outConnectionString))
                {
                    conn1.Open();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.CommandTimeout = 0;

                        SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                        string lastampds = "";
                        string lastvehicle = "";
                        long lasthour = 0;

                        foreach (IDataRecord current in from row in dr.Cast<IDataRecord>() select row)
                        {
                            string ampds;
                            string vehicle;
                            int hour;
                            long onscenetime;
                            long athospitaltime;

                            ampds = current.GetString(0);
                            vehicle = current.GetString(1);
                            hour = current.GetInt32(2);

                            if (current.IsDBNull(3))
                                onscenetime = 0;
                            else
                                onscenetime = current.GetInt64(3);

                            if (current.IsDBNull(4))
                                athospitaltime = 0;
                            else
                                athospitaltime = current.GetInt64(4);

                            if (ampds != lastampds || vehicle != lastvehicle || hour != lasthour)
                            {
                                // write out the last sample if core details have changed
                                if (lastampds.Length > 0)
                                {
                                    string insertsql;

                                    onscene.EndSampling();
                                    onscene.samples.Clear();

                                    // make insert command
                                    insertsql = string.Format("insert into OnsceneStats values ( {0},{1},'{2}','{3}','{4}',{5},{6},{7})", 0, hour, vehicle, Serialiser.Serialize(onscene), ampds, onscene.mean, onscene.stddev, onscene.count);
                                    using (SqlCommand insertcmd = new SqlCommand(insertsql, conn1))
                                    {
                                        insertcmd.ExecuteNonQuery();
                                    }

                                    hospitalturnaround.EndSampling();
                                    hospitalturnaround.samples.Clear();
                                    //System.Diagnostics.Debug.Print("{0},{1},{2},\"hospitalturnaround\", {3}, {4}\n", ampds, vehicle, hour, hospitalturnaround.min, hospitalturnaround.max);

                                    insertsql = string.Format("insert into OnsceneStats values ( {0},{1},'{2}','{3}','{4}',{5},{6},{7})", 1, hour, vehicle, Serialiser.Serialize(hospitalturnaround), ampds, hospitalturnaround.mean, hospitalturnaround.stddev, hospitalturnaround.count);
                                    using (SqlCommand insertcmd = new SqlCommand(insertsql, conn1))
                                    {
                                        insertcmd.ExecuteNonQuery();
                                    }
                                }
                                onscene.BeginSampling();
                                hospitalturnaround.BeginSampling();

                                lastampds = ampds;
                                lastvehicle = vehicle;
                                lasthour = hour;
                            }

                            if (onscenetime != 0) onscene.AddSample(onscenetime);
                            if (athospitaltime != 0) hospitalturnaround.AddSample(athospitaltime);

                        }
                    }
                }
            }
        }
    }
}
