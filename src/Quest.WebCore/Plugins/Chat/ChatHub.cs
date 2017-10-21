using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quest.WebCore.Plugins.ChatPlugin
{
    public class ChatHub : Hub
    {
        public void Send(string name, string message)
        {
            if (!message.StartsWith("@"))
            {
                Clients.All.InvokeAsync(name, message);
                return;
            }

            // Strip the @username directive from the start of the message
            var ar = message.Split(new char[] { ' ' }, 2);

            // The target username should be in the form @xxx - remove the @
            var targetUser = ar[0].Replace("@", "");
            var targetMessage = ar[1];

            Clients.User(targetUser).addNewMessageToPage(name, targetMessage);
        }

        public override Task OnConnected()
        {
            UserHandler.ConnectedIds.Add(Context.ConnectionId);
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            UserHandler.ConnectedIds.Remove(Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }
    }


    public static class UserHandler
    {
        public static HashSet<string> ConnectedIds = new HashSet<string>();
    }
}
