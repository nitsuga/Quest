using System;
using System.Collections.Generic;
using Quest.Lib.Utils;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Quest.Lib.ServiceBus.Messages;
using System.Data;
using System.Diagnostics;
using Quest.Lib.Processor;
using System.ComponentModel.Composition;
using System.Data.OracleClient;

namespace Quest.Lib.Trackers
{
    /// <summary>
    /// This module periodically queries CommandPoint to obtain additional information about events.
    /// The information is broadcast on RabbitMQ for resilience.
    /// The module is intended to be run as part of Quest.Processor.
    /// </summary>
    [Export("CommandPointTracker", typeof(ProcessorModule))]
    public class CommandPointTracker : ProcessorModule
    {
        #region privates

        /// <summary>
        /// master on/off switch
        /// </summary>
        private bool _enabled = true;

        /// <summary>
        /// link to rabbit message queue
        /// </summary>
        private MessageHelper _msgSource;

        /// <summary>
        /// name of the queue we read from 
        /// </summary>
        private string _QueueName;

        /// <summary>
        /// connectin to oracle to get CAD data
        /// </summary>
        private string _oracleConnection;

        /// <summary>
        /// ringback poll frequency in seconds
        /// </summary>
        private int _ringbacksFreq = 0;

        /// <summary>
        /// call disconnect  poll frequency in seconds
        /// </summary>
        private int _callDisconnectsFreq = 0;

        /// <summary>
        /// event status  poll frequency in seconds
        /// </summary>
        private int _eventStatusesFreq = 0;

        /// <summary>
        /// master loop counter for readers
        /// </summary>
        private int _loopcounter = 0;

        /// <summary>
        /// check state of whether we're enabled or disabled every 10 seconds
        /// </summary>
        private System.Timers.Timer _connecttimer = new System.Timers.Timer(10000);

        /// <summary>
        /// work timer fires every second
        /// </summary>
        private System.Timers.Timer _worktimer = new System.Timers.Timer(1000);

        /// <summary>
        /// time to go back on the oracle queries
        /// </summary>
        private string _ringbackSQL;
        private string _disconnectSQL;
        private string _eventdataSQL;

        #endregion

        public CommandPointTracker()
            : base("CommandPoint Tracker", Worker)
        {
        }

        /// <summary>
        /// This routing gets called when requested to start.
        /// Perform initialisation here 
        /// </summary>
        /// <param name="module"></param>
        static void Worker(ProcessorModule module)
        {
            CommandPointTracker me = module as CommandPointTracker;

            me.Initialise();

            for (; ; )
            {
                // StopRunning gets set when the system wants to shut us down
                if (module.StopRunning.WaitOne(1000))
                {
                    if (me._msgSource != null)
                        me._msgSource.Stop();
                    return;
                }
            }
        }

        public void Initialise()
        {
            _connecttimer.Elapsed += new System.Timers.ElapsedEventHandler(_connectiontimer_Elapsed);
            _connecttimer.Start();

            _worktimer.Elapsed += new System.Timers.ElapsedEventHandler(_worktimer_Elapsed);
            _worktimer.Start();
        }

        /// <summary>
        /// check health and enablement of system
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _connectiontimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            /// re-read variables in case something changed.
            _QueueName = SettingsHelper.GetVariable("CommandPointTracker.Queue", "Quest.CPTracker");
            _enabled = SettingsHelper.GetVariable("CommandPointTracker.Enabled", false);
            _oracleConnection = SettingsHelper.GetVariable("CommandPointTracker.OracleConnection", "");
            _ringbacksFreq = SettingsHelper.GetVariable("CommandPointTracker.RingbacksFreq", 5);
            _callDisconnectsFreq = SettingsHelper.GetVariable("CommandPointTracker.CallDisconnectsFreq", 5);
            _eventStatusesFreq = SettingsHelper.GetVariable("CommandPointTracker.EventStatusesFreq", 5);

            _ringbackSQL = SettingsHelper.GetVariable("CommandPointTracker.RingbackSQL", @"select 
                                   eventnumber
                                   ,creationdate as lastringback
                                   from
                                   CAD.CAD_ROW_DBEVH_SEG
                                   where segmentname='RBACK' and creationdate>sysdate-1/16");

            _disconnectSQL = SettingsHelper.GetVariable("CommandPointTracker.DisconnectSQL", @"select 
                    eventnumber
                    ,creationdate as disconnectime
                    from
                    CAD.CAD_ROW_DBEVH_SEG
                    where segmentname='CS' and creationdate>sysdate-1/16 and text like 'S/D:Disconnected%'"); ;

            _eventdataSQL = SettingsHelper.GetVariable("CommandPointTracker.EventdataSQL", @"select 
                        e.eventnumber
                        ,e.status
                        ,e.callertelephone
                        ,e.loccmt
                        ,d.problemdesc
                        ,d.patientage
                        ,d.patientsex
                        from
                        CAD.CAD_ROW_DBEVH e
                        join CAD.CAD_ROW_DBEVH_EMD d on e.eventnumber=d.eventnumber
                        where e.entrydate>sysdate-1/16");

        }

        #region Readers

        /// <summary>
        /// call each of the update functions according to their polll frequency, each offset by 1 second
        /// so they dont execute together.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _worktimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _loopcounter++;
            if (_enabled == true && _msgSource != null && _oracleConnection.Length > 0)
            {
                if (_ringbacksFreq > 0 && (_loopcounter % _ringbacksFreq) == 0)
                    GetRingbacks();

                if (_callDisconnectsFreq > 0 && ((_loopcounter + 1) % _callDisconnectsFreq) == 0)
                    GetCallDisconnects();

                if (_eventStatusesFreq > 0 && ((_loopcounter + 2) % _eventStatusesFreq) == 0)
                    GetEventStatuses();
            }
        }

        private void GetRingbacks()
        {
            try
            {
                using (var conn = new OracleConnection(_oracleConnection))
                {
                    RingbackStatusList msg = new RingbackStatusList();
                    msg.Items = new List<RingbackStatus>();
                    conn.Open();
                    // open up and get ringbacks for WAI calls
                    OracleCommand cmd = conn.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = _ringbackSQL;
                    OracleDataAdapter da = new OracleDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    da.Fill(ds);

                    foreach (DataRow a in ds.Tables[0].Rows)
                    {
                        if (a[0] != null)
                        {
                            RingbackStatus rb = new RingbackStatus();
                            rb.LastRingback = (DateTime)a["lastringback"];
                            rb.Serial = "L" + a["eventnumber"].ToString();
                            msg.Items.Add(rb);
                        }
                    }

                    // broadcast result to rabbit
                    if (msg.Items.Count > 0)
                        _msgSource.BroadcastMessage(msg);
                }
            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("GetRingbacks fault: {0}", ex.ToString()), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "CommandPointTracker");
            }
        }

        private void GetCallDisconnects()
        {
            try
            {
                using (var conn = new OracleConnection(_oracleConnection))
                {
                    CallDisconnectStatusList msg = new CallDisconnectStatusList();
                    msg.Items = new List<CallDisconnectStatus>();

                    conn.Open();

                    // open up and get event statuses
                    OracleCommand cmd = conn.CreateCommand();
                    cmd.CommandText = _disconnectSQL;

                    OracleDataAdapter da = new OracleDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    da.Fill(ds);

                    foreach (DataRow a in ds.Tables[0].Rows)
                    {
                        {
                            CallDisconnectStatus es = new CallDisconnectStatus();
                            es.Serial = "L" + a["eventnumber"].ToString();
                            es.DisconnectTime = (DateTime)a["disconnectime"];
                            msg.Items.Add(es);
                        }
                    }


                    // broadcast result to rabbit
                    if (msg.Items.Count > 0)
                        _msgSource.BroadcastMessage(msg);
                }

            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("GetCallDisconnects fault: {0}", ex.ToString()), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "CommandPointTracker");
            }
        }

        private void GetEventStatuses()
        {
            try
            {
                using (var conn = new OracleConnection(_oracleConnection))
                {

                    CPEventStatusList msg = new CPEventStatusList();
                    msg.Items = new List<CPEventStatus>();

                    conn.Open();

                    // open up and get event statuses
                    OracleCommand cmd = conn.CreateCommand();
                    cmd.CommandText = _eventdataSQL;

                    OracleDataAdapter da = new OracleDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    da.Fill(ds);

                    foreach (DataRow a in ds.Tables[0].Rows)
                    {
                        {
                            CPEventStatus es = new CPEventStatus();
                            es.Serial = "L" + a["eventnumber"].ToString();
                            es.Status = a["status"].ToString();
                            es.CallerTelephone = a["callertelephone"].ToString();
                            es.LocationComment = a["loccmt"].ToString();
                            es.ProblemDescription = a["problemdesc"].ToString();
                            es.Age = a["patientage"].ToString();
                            es.Sex = a["patientsex"].ToString();
                            msg.Items.Add(es);
                        }
                    }


                    // broadcast result to rabbit
                    if (msg.Items.Count > 0)
                        _msgSource.BroadcastMessage(msg);

                }

            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("GetEventStatuses fault: {0}", ex.ToString()), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "CommandPointTracker");
            }
        }

        #endregion

    }
}
