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
        public void Send(ILifetimeScope scope, INotificationMessage message)
        {
            if (scope == null)
                return;

            if (message == null)
                return;

            Task.Run(() => SendAsync(scope, message));
        }

        /// <summary>
        ///     Send a message to a target
        /// </summary>
        /// <param name="container">MEF Composition container</param>
        /// <param name="address">target address in the form mailto:someone@home.com</param>
        /// <param name="replyto">a return address if the target wishes to respond</param>
        /// <param name="message"></param>
        public async Task SendAsync(ILifetimeScope scope, INotificationMessage message)
        {
            if (scope == null)
                return;

            if (message == null)
                return;

            var processor = scope.ResolveNamed<INotifier>(message.Method);
            if (processor != null)
            {
                await Task.Run(() => processor.Send(message));
            }
        }
    }
}