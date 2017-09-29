using Quest.Common.Messages;

namespace Quest.Lib.Notifier
{
    public interface INotifier
    {
        NotificationResponse Send(Notification message);
    }
}