namespace Quest.Lib.Notifier
{
    public interface INotifier
    {
        void Send(INotificationMessage message);
    }

    public interface INotificationMessage
    {
        string Method { get; set; }
        string Address { get; set; }
        string Subject { get; set; }
        string Body { get; set; }
    }
}