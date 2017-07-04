using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using QuestXC.Properties;
using Quest.Lib.Utils;
using Quest.Lib.Net;
using System.Diagnostics;


namespace Quest.XC
{
    public class ChannelController
    {
        private XCReflector _reflector = new XCReflector();
        private MessageProcessor _messageProcessor = new MessageProcessor();
        private MessageHelper msgSource;

        public event System.EventHandler<DataEventArgs> IncomingData;

        /// <summary>
        /// holds all the channels we connect to.
        /// </summary>
        public Dictionary<String, XCConnector> Channels;

        /// <summary>
        /// create all channels
        /// </summary>
        /// <param name="connectionString"></param>
        public void Initialise(String ReaderQueueName, String WriterQueueName)
        {
            try
            {
                Logger.Write(string.Format("Initialising"), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "ChannelController");

                msgSource = new MessageHelper();
                msgSource.Initialise(ReaderQueueName);

                if (Settings.Default.EnableReader)
                    Channels = CreateChannels();

                if (Settings.Default.EnableReflector)
                    _reflector.Initialise(this, "Reflector");

                if (Settings.Default.EnableWriter)
                    _messageProcessor.Initialise(WriterQueueName);

                Logger.Write(string.Format("Initialised"), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "ChannelController");

            }
            catch (Exception ex)
            {
                if (ExceptionPolicy.HandleException(ex, LoggingPolicy.Policy.TracePolicy.ToString()))
                    throw;
            }
        }

        public void Start()
        {
            try
            {
                Logger.Write(string.Format("Starting"), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "ChannelController");

                if (Settings.Default.EnableReader)
                    foreach (XCConnector c in Channels.Values)
                        c.Start();

                Logger.Write(string.Format("Started"), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "ChannelController");
            }
            catch (Exception ex)
            {
                if (ExceptionPolicy.HandleException(ex, LoggingPolicy.Policy.TracePolicy.ToString()))
                    throw;
            }
        }

        public void Stop()
        {
            Logger.Write(string.Format("Stopping"), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "ChannelController");
            if (Settings.Default.EnableReader)
                foreach (XCConnector c in Channels.Values)
                    c.Stop();

            if (Settings.Default.EnableReflector)
                _reflector.Stop();

            if (Settings.Default.EnableWriter)
                _messageProcessor.Stop();

            Logger.Write(string.Format("Stopped"), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "ChannelController");

        }

        /// <summary>
        /// create all the channels
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        private Dictionary<String, XCConnector> CreateChannels()
        {
            Logger.Write(string.Format("Creating channels"), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "ChannelController");
            Dictionary<String, XCConnector> channels = new Dictionary<String, XCConnector>();
            String channelString = SettingsHelper.GetVariable("XC.Channels", "");
            String[] parts = channelString.Split(',');
            foreach (String baseName in parts)
            {
                try
                {
                    // create a single channel and add it into our list
                    XCConnector conn = CreateChannel(baseName);
                    channels.Add(baseName, conn );
                }
                catch (Exception ex)
                {
                    if (ExceptionPolicy.HandleException(ex, LoggingPolicy.Policy.TracePolicy.ToString()))
                        throw;
                }
                
            }
            Logger.Write(string.Format("Created channels"), LoggingPolicy.Category.Trace.ToString(), 0, 0, TraceEventType.Information, "ChannelController");
            return channels;
        }

        /// <summary>
        /// create a single XC channel
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="baseName"></param>
        /// <returns></returns>
        private XCConnector CreateChannel(String baseName)
        {
            XCConnector connector = new XCConnector();
            connector.Initialise(this, baseName, msgSource);
            connector.IncomingData += new EventHandler<DataEventArgs>(connector_IncomingData);
            return connector;
        }

        /// <summary>
        /// data recieved by primary channels
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void connector_IncomingData(object sender, DataEventArgs e)
        {
            if (IncomingData != null)
                IncomingData(this, e);

        }
    }
}
