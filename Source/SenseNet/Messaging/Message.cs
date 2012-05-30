using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;

namespace SenseNet.Messaging
{
    public partial class Message
    {
        internal MailMessage GenerateMailMessage()
        {
            var message = new MailMessage(Configuration.SenderAddress, Address, Subject, Body);
            
            message.IsBodyHtml = true;

            message.SubjectEncoding = Configuration.MessageEncoding;
            message.HeadersEncoding = Configuration.MessageEncoding;
            message.BodyEncoding = Configuration.MessageEncoding;
            
            return message;
        }

        internal static void DeleteAllMessages()
        {
            using (var context = new DataHandler())
            {
                context.ExecuteCommand("DELETE FROM [Messaging.Messages]");
            }
        }

        internal static IEnumerable<Message> GetAllMessages()
        {
            using (var context = new DataHandler())
            {
                return context.Messages.ToArray();
            }
        }

        private static readonly Message[] _emptyMessages = new Message[0];
        public static IEnumerable<Message> EmptyMessages { get { return _emptyMessages; } }
    }
}
