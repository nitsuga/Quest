using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Quest.Common.Messages;
using Quest.Lib.Trace;
using Quest.Common.ServiceBus;

namespace Quest.Lib.ServiceBus
{
    public class MessageCache
    {
        public IServiceBusClient MsgSource;

        private const int Ttl = 20;

        private readonly AutoResetEvent _are = new AutoResetEvent(false);

        private readonly List<CacheEntry> _cache = new List<CacheEntry>();

        public MessageCache(IServiceBusClient msgSource)
        {
            MsgSource = msgSource;
        }

        public void Initialise(string queueName)
        {
            try
            {
                MsgSource.Initialise(queueName);
                MsgSource.NewMessage += msgSource_NewMessage;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to initialise", ex);
            }
        }

        /// <summary>
        ///     Catch incoming messages and store them.. web requests will pick out te objects they need
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void msgSource_NewMessage(object sender, NewMessageArgs e)
        {
            try
            {
                Logger.Write($"Got message {e.Payload.GetType()}","Web");
                lock (_cache)
                {
                    _cache.Add(new CacheEntry { Created = DateTime.UtcNow, Msg = e, CorrelationId = e.Metadata.CorrelationId });
                    _cache.RemoveAll(x => (DateTime.UtcNow - x.Created).TotalSeconds > Ttl);
                }
                // set flag to unblock any waiters..
                _are.Set();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Application", ex.ToString());
            }
        }

        /// <summary>
        ///     sends a request and waits for a response
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public T SendAndWait<T>(Request obj, TimeSpan timeout, string DestinationQueue= null) where T : class
        {
            var id = Guid.NewGuid().ToString();
            Task.Factory.StartNew(() => MsgSource.Broadcast(obj, new PublishMetaData() {CorrelationId = id, ReplyTo= MsgSource.QueueName, Destination = DestinationQueue }));
            return Waitfor<T>(id, timeout);
        }

        public void BroadcastMessage(MessageBase obj)
        {
            MsgSource.Broadcast(obj);
        }

        /// <summary>
        ///     wait for a message of type T with the given correlation id enter the cache.
        /// </summary>
        /// <param name="correlationId"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public T Waitfor<T>(string correlationId, TimeSpan timeout) where T : class
        {
            var sw = new Stopwatch();
            sw.Start();
            do
            {
                lock (_cache)
                {
                    var list = _cache.Where(x => x.CorrelationId == correlationId).ToList();
                    if (list.Any())
                    {
                        var first =list.Select(x => x.Msg.Payload).OfType<T>().FirstOrDefault();
                        if (first != null)
                        {
                            _cache.RemoveAll(x => x.CorrelationId == correlationId);
                            return first;
                        }
                    }
                }
                _are.WaitOne(200);
            } while (sw.Elapsed < timeout);
            return null;
        }

        private class CacheEntry
        {
            public string CorrelationId;
            public DateTime Created;
            public NewMessageArgs Msg;
        }
    }
}