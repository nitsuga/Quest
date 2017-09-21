namespace Quest.Lib.Notifier
{
    public interface INotifier
    {
        void Send(string address, string replyto, INotificationMessage message);
    }

    public interface INotificationMessage
    {
        string Subject { get; set; }
        string Body { get; set; }
        //public String[] Attachments;
    }
}