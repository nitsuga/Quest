using Quest.Common.Messages.Notification;

namespace Quest.Lib.Notifier
{
    public interface INotifier
    {
        NotificationResponse Send(Notification message);
    }
}