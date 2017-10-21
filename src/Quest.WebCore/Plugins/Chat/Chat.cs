using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;

namespace Quest.WebCore.Plugins.ChatPlugin
{
    public interface IChat
    {
    }
    public interface IChatProxy: IClientProxy
    {
        void addNewMessageToPage(string name, string message);
    }

}
