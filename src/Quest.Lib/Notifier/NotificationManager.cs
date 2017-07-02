using Autofac;
using Quest.Lib.Trace;
using System;
using System.Threading.Tasks;

namespace Quest.Lib.Notifier
{
    public class NotificationManager
    {
        /// <summary>
        ///     Send a message to one or more targets
        /// </summary>
        /// <param name="scope">Composition container</param>
        /// <param name="addresses">target addresses in the form address[,address...] where address is like mailto:someone@home.com</param>
        /// <param name="replyto">a return address if the target wishes to respond</param>
        /// <param name="message"></param>
        public void Send(ILifetimeScope scope, string[] addresses, string replyto, IMessage message)
        {
            if (addresses == null)
                return;

            if (addresses.Length == 0)
                return;

            if (scope == null)
                return;

            if (message == null)
                return;

            foreach (var address in addresses)
            {
                try
                {
                    Task.Run(() => SendAsync(scope, address, replyto, message));
                }
                catch (Exception ex)
                {
                      Logger.Write("Error sending message: " + ex.Message, GetType().Name);
                }
            }
        }

        /// <summary>
        ///     Send a message to a target
        /// </summary>
        /// <param name="container">MEF Composition container</param>
        /// <param name="address">target address in the form mailto:someone@home.com</param>
        /// <param name="replyto">a return address if the target wishes to respond</param>
        /// <param name="message"></param>
        public async Task SendAsync(ILifetimeScope scope, string address, string replyto, IMessage message)
        {
            if (address == null)
                return;

            if (scope == null)
                return;

            if (message == null)
                return;

            var parts = address.Split(':');
            if (parts.Length != 2)
                throw new ApplicationException($"Incorrect address format: {address}");
            var processor = scope.ResolveNamed<INotifier>(parts[0]);
            if (processor != null)
            {
                await Task.Run(() => processor.Send(parts[1], replyto, message));
            }
        }
    }
}