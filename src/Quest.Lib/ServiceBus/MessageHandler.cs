using System;
using System.Collections.Generic;
using System.Diagnostics;
using Quest.Lib.Trace;
using Quest.Common.Messages;
using Quest.Common.ServiceBus;

namespace Quest.Lib.ServiceBus
{
    /// <summary>
    /// Provides a mechansim for dispatch messages based on the object type.
    /// The caller can provide a list of message types and a handler.
    /// The senders' correlation ids are preserved
    /// </summary>
    public class MessageHandler
    {
        public delegate void MsgHandler(MessageBase msg);

        /// <summary>
        ///     list of messages and handlers for those messages
        /// </summary>
        private readonly Dictionary<string, Func<NewMessageArgs, Response>> _msgHandlers = new Dictionary<string, Func<NewMessageArgs, Response>>();

        private readonly Dictionary<string, Action<MessageBase>> _msg2Handlers = new Dictionary<string, Action<MessageBase>>();
        private string _defaultReply;

        public void Clear()
        {
            _msgHandlers.Clear();
        }

        public void AddHandler(string message, Func<NewMessageArgs, Response> handler)
        {
            lock (_msgHandlers)
            {
                if (!_msgHandlers.ContainsKey(message))
                    _msgHandlers.Add(message, handler);
            }
        }

        public void AddHandler<T>(Func<NewMessageArgs, Response> handler) where T : class
        {
            lock (_msgHandlers)
            {
                if (!_msgHandlers.ContainsKey(typeof(T).Name))
                    _msgHandlers.Add(typeof(T).Name, handler);
            }
        }

        public void AddActionHandler<T>(Action<MessageBase> handler) where T : MessageBase
        {
            lock (_msgHandlers)
            {
                if (!_msg2Handlers.ContainsKey(typeof(T).Name))
                    _msg2Handlers.Add(typeof(T).Name, handler);
            }
        }

        /// <summary>
        /// process a message from a service bus and send response back through the supplied service bus
        /// </summary>
        /// <param name="serviceBusClient">the service bus client to use to send a response</param>
        /// <param name="e"></param>
        public void ProcessMessage(IServiceBusClient serviceBusClient, NewMessageArgs e )
        {
            try
            {
                var response = DispatchMessage(e);

                if (response == null)
                {
                    //Logger.Write($"{_defaultReply} does not process {e.Payload.GetType()}", TraceEventType.Information, GetType().Name);
                    return;
                }

                PublishMetaData metadata = new PublishMetaData()
                {
                    CorrelationId = e.Metadata.CorrelationId,
                    ReplyTo = _defaultReply,
                    Source = _defaultReply,
                    Destination = e.Metadata.ReplyTo
                };
                serviceBusClient.Broadcast(response, metadata);
            }
            catch (Exception ex)
            {
                Logger.Write($"Unhandled error processing message: {_defaultReply} {e.Payload} ", TraceEventType.Information, GetType().Name);
                Logger.Write($"Error is : {ex} ", TraceEventType.Information, GetType().Name);
            }
        }

        /// <summary>
        /// standard service bus message handling. look up a handler in the list of registered handlers
        /// and obtain a response. this method does not interact with the service bus
        /// </summary>
        /// <param name="e"></param>
        /// <returns>true if the message was handled</returns>
        private Response DispatchMessage(NewMessageArgs e)
        {
            try
            {
                Func<NewMessageArgs, Response> handler = null;
                Response response = null;

                // look up a suitable handler for this messge based on its type
                _msgHandlers.TryGetValue(e.Payload.GetType().Name, out handler);

                if (handler != null)
                {
                    Logger.Write($"Dispatch message {e.Payload.GetType().Name}", TraceEventType.Information, GetType().Name);
                    response = handler(e);
                }

                Action<MessageBase> handler2;
                // look up a suitable handler for this messge based on its type
                _msg2Handlers.TryGetValue(e.Payload.GetType().Name, out handler2);

                if (handler2 != null)
                {
                    //Logger.Write($"Processing message: {e.Payload.GetType().Name}", TraceEventType.Information, "DeviceTracker");

                    handler2((MessageBase)e.Payload);
                }
                return response;
            }
            catch (Exception ex)
            {
                Logger.Write($"Unhandled error processing message: {ex} ",
                    TraceEventType.Information, GetType().Name);
            }

            return null;
        }

        internal void SetReply(string reply)
        {
            _defaultReply = reply;
        }
    }
}