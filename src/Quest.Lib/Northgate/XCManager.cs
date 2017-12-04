using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Collections.Specialized;
using Quest.Lib.Utils;
using Quest.Lib.Net;
using Quest.Lib.Trace;
using Quest.Lib.Processor;
using Quest.Lib.Notifier;
using Quest.Common.ServiceBus;
using Quest.Lib.ServiceBus;
using Quest.Common.Messages;
using Quest.Lib.Coords;
using Quest.Lib.Device;
using Quest.Common.Messages.CAD;
using Quest.Common.Messages.Incident;
using Quest.Common.Messages.Resource;
using Quest.Common.Messages.System;
using Autofac;
using Quest.Common.Utils;

namespace Quest.Lib.Northgate
{

    /// <summary>
    /// Connects to XCConnect and broadcasts inbound XC traffic onto the ESB
    /// Also listens for XC Outbound ESB messages and sends on to CAD
    /// </summary>
    public class XCManager : ServiceBusProcessor
    {
        internal class XCCluster
        {
            public string PrimaryChannel;
            public string BackupChannel;
            public bool Enabled=true;
            public int SwitchOverDelay = 5;
        }

        internal class STMItem
        {
            internal ChannelStatus Primary;
            internal ChannelStatus Backup;
            internal ChannelAction Action;
        }

        public string Channels { get; set; }
        public string AssignSbpChannels { get; set; }
        public string AssignCallsignChannels { get; set; }
        

        public int WatchdogPeriod { get; set; }
        public int SwitchOverDelay { get; set; }
        
        private StatusCollection _systemStatus;
        private List<XCCluster> _clusters;
        private ILifetimeScope _scope;
        private List<STMItem> _stateTransitionMatrix;
        private Timer _watchdog;
        

        internal delegate void ChannelAction(XCCluster cluster);

        public XCManager(
            ILifetimeScope scope,
            IServiceBusClient serviceBusClient,
            MessageHandler msgHandler,
            TimedEventQueue eventQueue) : base(eventQueue, serviceBusClient, msgHandler)
        {
            _systemStatus = new StatusCollection();

            _clusters = new List<XCCluster>();

            _scope = scope;
            
            _stateTransitionMatrix = new List<STMItem>()
            {
                //
                new STMItem { Primary = ChannelStatus.Active,       Backup = ChannelStatus.Active, Action = StopBackup },
                new STMItem { Primary = ChannelStatus.Connected,    Backup = ChannelStatus.Active, Action = StartBackupIfPrimaryDelayed },
                new STMItem { Primary = ChannelStatus.Disabled,     Backup = ChannelStatus.Active, Action = StartPrimary },
                new STMItem { Primary = ChannelStatus.Disconnected, Backup = ChannelStatus.Active, Action = StartPrimary },
                new STMItem { Primary = ChannelStatus.Unknown,      Backup = ChannelStatus.Active, Action = StartPrimary },

                new STMItem { Primary = ChannelStatus.Active,       Backup = ChannelStatus.Connected, Action = StopBackup },
                new STMItem { Primary = ChannelStatus.Connected,    Backup = ChannelStatus.Connected, Action = StartBackupIfPrimaryDelayed },
                new STMItem { Primary = ChannelStatus.Disabled,     Backup = ChannelStatus.Connected, Action = StartPrimaryIfBackupDelayed },
                new STMItem { Primary = ChannelStatus.Disconnected, Backup = ChannelStatus.Connected, Action = StartPrimary },
                new STMItem { Primary = ChannelStatus.Unknown,      Backup = ChannelStatus.Connected, Action = StartPrimary },

                new STMItem { Primary = ChannelStatus.Active,       Backup = ChannelStatus.Disabled, Action = StartBackup },
                new STMItem { Primary = ChannelStatus.Connected,    Backup = ChannelStatus.Disabled, Action = StartBackupIfPrimaryDelayed },
                new STMItem { Primary = ChannelStatus.Disabled,     Backup = ChannelStatus.Disabled, Action = StartBackup },
                new STMItem { Primary = ChannelStatus.Disconnected, Backup = ChannelStatus.Disabled, Action = StartBackup },
                new STMItem { Primary = ChannelStatus.Unknown,      Backup = ChannelStatus.Disabled, Action = StartPrimary },

                new STMItem { Primary = ChannelStatus.Active,       Backup = ChannelStatus.Disconnected, Action = StartBackup },
                new STMItem { Primary = ChannelStatus.Connected,    Backup = ChannelStatus.Disconnected, Action = StartBackup },
                new STMItem { Primary = ChannelStatus.Disabled,     Backup = ChannelStatus.Disconnected, Action = StartBackup },
                new STMItem { Primary = ChannelStatus.Disconnected, Backup = ChannelStatus.Disconnected, Action = StartPrimary },
                new STMItem { Primary = ChannelStatus.Unknown,      Backup = ChannelStatus.Disconnected, Action = StartPrimary },

                new STMItem { Primary = ChannelStatus.Active,       Backup = ChannelStatus.Unknown, Action = StartBackup },
                new STMItem { Primary = ChannelStatus.Connected,    Backup = ChannelStatus.Unknown, Action = StartBackup },
                new STMItem { Primary = ChannelStatus.Disabled,     Backup = ChannelStatus.Unknown, Action = StartBackup },
                new STMItem { Primary = ChannelStatus.Disconnected, Backup = ChannelStatus.Unknown, Action = StartBackup },
                new STMItem { Primary = ChannelStatus.Unknown,      Backup = ChannelStatus.Unknown, Action = StartPrimary },

            };

        }

        private void CreateClusters()
        {
            _clusters = new List<XCCluster>();
            var channels = Channels.Split("|");
            foreach( var ch in channels)
            {
                var parts = ch.Split(",");
                if (parts.Length==2)
                {
                    _clusters.Add(new XCCluster { PrimaryChannel = parts[0], BackupChannel = parts[1], Enabled = true, SwitchOverDelay = SwitchOverDelay });
                }
            }
        }

        protected override void OnPrepare()
        {
            CreateClusters();

            // listen for channel status reports
            MsgHandler.AddHandler<XCChannelStatus>(XCChannelStatusHandler);
            MsgHandler.AddHandler<AssignToDestinationRequest>(AssignToDestinationRequestHandler);
            MsgHandler.AddHandler<AssignCallsign>(AssignCallsignHandler);

            try
            {
                _watchdog = new Timer((z) =>
                {
                    CheckClusters();
                },null, WatchdogPeriod, WatchdogPeriod);

            }
            catch (Exception ex)
            {

            }
        }


        private Response AssignCallsignHandler(NewMessageArgs arg)
        {
            var request = arg.Payload as AssignCallsign;
            if (request == null)
                return null;

            if (String.IsNullOrEmpty(AssignCallsignChannels))
                return null;

            base.ServiceBusClient.Broadcast(new XCOutbound
            {
                Channel = AssignCallsignChannels,
                Command = $"0|Quest||PRI|{request.Callsign}|||||||||||{request.FleetNo}||||||||||||"
            });

            return null;
        }


        private Response AssignToDestinationRequestHandler(NewMessageArgs arg)
        {
            var request = arg.Payload as AssignToDestinationRequest;
            if (request == null)
                return null;

            if (String.IsNullOrEmpty(AssignSbpChannels))
                return null;

            base.ServiceBusClient.Broadcast(new XCOutbound
            {
                Channel = AssignSbpChannels,
                Command =$"0|Quest||RRA||{request.DestinationCode}-{request.DestinationCode}-DESCRIPTION|{request.Callsign}"
            });

            return null;
        }

        private Response XCChannelStatusHandler(NewMessageArgs arg)
        {
            XCChannelStatus status = arg.Payload as XCChannelStatus;
            if (status != null)
            {
                Logger.Write($"Got status: Starting {status.Channel} => {status.Status}", TraceEventType.Information);
                // update internal cache of statuses
                _systemStatus[status.Channel] = status;

                CheckClusters();
            }
            return null;
        }

        private void CheckClusters()
        {
            foreach (var cluster in _clusters)
                CheckCluster(cluster);
        }
        
        private void CheckCluster(XCCluster cluster)
        {
            // get status of primary and backup
            var primaryStatus = _systemStatus[cluster.PrimaryChannel];
            var backupStatus = _systemStatus[cluster.BackupChannel];

            // get action associated with this state
            var state = _stateTransitionMatrix.Where(x => x.Primary == primaryStatus?.Status && x.Backup == backupStatus?.Status).FirstOrDefault();

            string ps = "";
            string bs = "";

            if (primaryStatus == null)
                ps = $"Primary **Unknown**";
            else
                ps = $"Primary {primaryStatus?.Status} @ {Time.UnixTime(primaryStatus.Timestamp)}";

            if (backupStatus == null)
                bs = $"Backup **Unknown**";
            else
                bs = $"Backup {backupStatus.Status} @ {Time.UnixTime(backupStatus.Timestamp)}";

            Logger.Write($"Status: {ps} {bs}", TraceEventType.Information);

            // execute the action if one is defined
            if (state != null)
                state.Action(cluster);
        }
        
        internal void StartBackup(XCCluster cluster)
        {
            Logger.Write($"StartBackup: {cluster.BackupChannel} => EnableAsBackup", TraceEventType.Information);
            ServiceBusClient.Broadcast(new XCChannelControl { Action = XCChannelControl.Command.EnableAsBackup, Channel = cluster.BackupChannel });
        }

        internal void StartBackupAsActive(XCCluster cluster)
        {
            Logger.Write($"StartBackupAsActive: {cluster.BackupChannel} => EnableAsPrimary", TraceEventType.Information);
            ServiceBusClient.Broadcast(new XCChannelControl { Action = XCChannelControl.Command.EnableAsPrimary, Channel = cluster.BackupChannel });
        }

        /// <summary>
        /// Switch to backup if the last status report was olde than switchOverDelay seconds
        /// </summary>
        /// <param name="cluster"></param>
        internal void StartBackupIfPrimaryDelayed(XCCluster cluster)
        {
            var primaryStatus = _systemStatus[cluster.PrimaryChannel];
            var backupStatus = _systemStatus[cluster.BackupChannel];
            if (primaryStatus != null)
            {
                var lastReport = Time.UnixTime(primaryStatus.Timestamp);
                if (DateTime.Now.Subtract(lastReport).TotalMilliseconds < cluster.SwitchOverDelay)
                {
                    Logger.Write($"StartBackupIfPrimaryDelayed: {cluster.BackupChannel} => EnableAsPrimary", TraceEventType.Information);
                    ServiceBusClient.Broadcast(new XCChannelControl { Action = XCChannelControl.Command.EnableAsPrimary, Channel = cluster.BackupChannel });
                }
                else
                {
                    Logger.Write($"StartBackupIfPrimaryDelayed: {cluster.BackupChannel} => EnableAsPrimary", TraceEventType.Information);
                    ServiceBusClient.Broadcast(new XCChannelControl { Action = XCChannelControl.Command.EnableAsBackup, Channel = cluster.BackupChannel });

                }
            }
        }

        /// <summary>
        /// Switch to primary if the last status report from the backup was older than switchOverDelay seconds
        /// </summary>
        /// <param name="cluster"></param>
        internal void StartPrimaryIfBackupDelayed(XCCluster cluster)
        {
            var primaryStatus = _systemStatus[cluster.PrimaryChannel];
            var backupStatus = _systemStatus[cluster.BackupChannel];
            if (primaryStatus != null)
            {
                var lastReport = Time.UnixTime(primaryStatus.Timestamp);
                if (DateTime.Now.Subtract(lastReport).TotalSeconds < cluster.SwitchOverDelay)
                {
                    Logger.Write($"StartPrimaryIfBackupDelayed: {cluster.PrimaryChannel} => EnableAsPrimary", TraceEventType.Information);
                    ServiceBusClient.Broadcast(new XCChannelControl { Action = XCChannelControl.Command.EnableAsPrimary, Channel = cluster.PrimaryChannel });
                }
            }
        }

        internal void StartPrimary(XCCluster cluster)
        {
            Logger.Write($"StartPrimary: {cluster.PrimaryChannel} => EnableAsPrimary", TraceEventType.Information);
            ServiceBusClient.Broadcast(new XCChannelControl { Action = XCChannelControl.Command.EnableAsPrimary, Channel = cluster.PrimaryChannel });
        }

        internal void StopPrimary(XCCluster cluster)
        {
            Logger.Write($"StopPrimary: {cluster.PrimaryChannel} => Disable", TraceEventType.Information);
            ServiceBusClient.Broadcast(new XCChannelControl { Action = XCChannelControl.Command.Disable, Channel = cluster.PrimaryChannel });
        }

        internal void StopBackup(XCCluster cluster)
        {
            Logger.Write($"StopBackup: {cluster.BackupChannel} => Disable", TraceEventType.Information);
            ServiceBusClient.Broadcast(new XCChannelControl { Action = XCChannelControl.Command.Disable, Channel = cluster.BackupChannel });
        }

    }
}
