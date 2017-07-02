using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Mail;
using Quest.Lib.Trace;

namespace Quest.Lib.Notifier
{
    public class EmailNotifier : INotifier
    {
        private string _host;
        private int _port;


        public void Send(string address, string replyto, IMessage message)
        {
            Logger.Write($"Sending email to {address} {message.Subject}", 
                TraceEventType.Information, "EmailNotifier");

            var smtpmessage = new MailMessage(
                replyto,
                address,
                message.Subject,
                message.Body
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
            }
            catch (Exception ex)
            {
                Logger.Write($"Exception caught in CreateMessageWithAttachment(): {ex}", GetType().Name);
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