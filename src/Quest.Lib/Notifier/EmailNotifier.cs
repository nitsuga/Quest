using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Mail;
using Quest.Lib.Trace;
using Quest.Common.Messages.Notification;

namespace Quest.Lib.Notifier
{
    public class EmailNotifier : INotifier
    {
        private string _host;
        private int _port;


        public NotificationResponse Send(Notification message)
        {
            Logger.Write($"Sending via {message.Method} to {message.Address} {message.Subject}", TraceEventType.Information, GetType().Name);

            var smtpmessage = new MailMessage(
                "",
                message.Address,
                message.Subject,
                message.Body.ToString()
                );

            smtpmessage.IsBodyHtml = true;

            //if (message.Attachments!=null && message.Attachments.Count()>0)
            //{           
            //    foreach( var file in message.Attachments)
            //    {
            //        // Create  the file attachment for this e-mail message.
            //        Attachment data = new Attachment(file, MediaTypeNames.Application.Octet);
            //        // Add time stamp information for the file.
            //        ContentDisposition disposition = data.ContentDisposition;
            //        disposition.CreationDate = System.IO.File.GetCreationTime(file);
            //        disposition.ModificationDate = System.IO.File.GetLastWriteTime(file);
            //        disposition.ReadDate = System.IO.File.GetLastAccessTime(file);
            //        // Add the file attachment to this e-mail message.
            //        smtpmessage.Attachments.Add(data);
            //    }
            //}

            //Send the message. Get settings from the app.Config/web.Config 

            //<!--  -->
            //<system.net>
            //  <mailSettings>
            //    <smtp deliveryMethod="network">
            //      <network
            //        host="localhost"
            //        port="25"
            //        defaultCredentials="true"
            //      />
            //    </smtp>
            //  </mailSettings>
            //</system.net>

            var client = new SmtpClient();

            try
            {
                client.Timeout = 60000;
                client.SendCompleted += client_SendCompleted;

                client.Send(smtpmessage);

                return new NotificationResponse { Message = $"Message sent", Success = true, RequestId = message.RequestId };
            }
            catch (Exception ex)
            {
                Logger.Write($"Exception caught in CreateMessageWithAttachment(): {ex}", GetType().Name);
                return new NotificationResponse { Message = $"Method {GetType().Name} failed: {ex.Message}", Success = false, RequestId = message.RequestId };
            }
        }

        public void Setup(string host, int port)
        {
            _host = host;
            _port = port;
        }

        private void client_SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Logger.Write("Email Sent....", TraceEventType.Information, "EmailNotifier");
        }
    }
}