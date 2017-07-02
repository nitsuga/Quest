namespace Quest.Lib.Notifier
{
    public interface INotifier
    {
        void Send(string address, string replyto, IMessage message);
    }

    public interface IMessage
    {
        string Subject { get; set; }
        string Body { get; set; }
        //public String[] Attachments;
    }
}