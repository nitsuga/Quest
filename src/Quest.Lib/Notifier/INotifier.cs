using Quest.Common.Messages;

namespace Quest.Lib.Notifier
{
    public interface INotifier
    {
        void Send(Notification message);
    }
}