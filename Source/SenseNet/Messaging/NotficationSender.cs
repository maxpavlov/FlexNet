using System;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Diagnostics;
using SenseNet.Diagnostics;
using System.Collections.Generic;
using SenseNet.ContentRepository.Storage.Data;
using System.Data;

namespace SenseNet.Messaging
{
    internal class NotificationSender
    {
        private static bool _mailSendingInProgress;
        private static SmtpClient _smtpClient;
        private static readonly bool _isSmtpConfigured;
        
        internal static bool TestMode { get; set; }

        static NotificationSender()
        {
            _smtpClient = new SmtpClient();
            _isSmtpConfigured = !String.IsNullOrEmpty(_smtpClient.Host);
            _mailSendingInProgress = false;
            TestMode = false;
        }

        public static void StartMessageProcessing()
        {
            if (!_isSmtpConfigured && !TestMode)
            {
                Debug.WriteLine("#Notification> Notification sender is unable to work. Reason: smtp server isn't configured.");
                Logger.WriteError("Notification sender is unable to work. Reason: smtp server isn't configured.");
                return;
            }
            
            if (_mailSendingInProgress)
                return;

            _mailSendingInProgress = true;
            Debug.WriteLine("#Notification> _mailSendingInProgress true ");
            
            var worker = new Thread(ProcessQueuedMessages);
            worker.Name = "NotificationSenderThread";
            worker.CurrentCulture = Thread.CurrentThread.CurrentCulture;
            worker.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
            worker.Start();
        }

        private static void ProcessQueuedMessages()
        {
                try
                {
                    while (true)
                    {
                        using (var context = new DataHandler())
                        {
                            try
                            {
                                var messages = GetMessagesToSend(context);
                                if (messages.Count() == 0)
                                    break;

                                foreach (var message in messages)
                                {
                                    ProcessMessage(message, context);
                                }
                            }
                            finally
                            {
                                context.SubmitChanges();
                            }
                        }
                        Debug.WriteLine("#Notification> End of iteration");
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine("#Notification> Rootlevel Exception:" + exception.Message);
                    Logger.WriteException(exception);
                }
                finally
                {
                    ProcessQueuedMessagesFinished();
                }
        }

        private static IEnumerable<Message> GetMessagesToSend(DataHandler context)
        {
            //var messages = context.Messages.Take(Configuration.TakeCount);
            //return messages;
            var lockId = Guid.NewGuid().ToString(); //TODO: #notif: LockId (machine+domain+thread?)
            if(SignMessagesToSend(lockId))
                return SelectSignedMessages(context, lockId);
            return Message.EmptyMessages;
        }
        internal static bool SignMessagesToSend(string lockId)
        {
            var until = DateTime.Now.AddMinutes(1);
            var sql = "UPDATE TOP (@Top) [Messaging.Messages] SET LockId = @LockId, LockedUntil = @LockedUntil WHERE LockId = @LockId OR LockId IS NULL OR LockedUntil < @Now";
            int rows = 0;
            using (var proc = SenseNet.ContentRepository.Storage.Data.DataProvider.CreateDataProcedure(sql))
            {
                proc.CommandType = System.Data.CommandType.Text;

                var topPrm = DataProvider.CreateParameter();
                topPrm.ParameterName = "@Top";
                topPrm.DbType = DbType.Int32;
                topPrm.Value = Configuration.TakeCount;
                proc.Parameters.Add(topPrm);

                var lockIdPrm = DataProvider.CreateParameter();
                lockIdPrm.ParameterName = "@LockId";
                lockIdPrm.DbType = DbType.String;
                lockIdPrm.Size = 500;
                lockIdPrm.Value = lockId;
                proc.Parameters.Add(lockIdPrm);

                var lockedUntilPrm = DataProvider.CreateParameter();
                lockedUntilPrm.ParameterName = "@LockedUntil";
                lockedUntilPrm.DbType = DbType.DateTime;
                lockedUntilPrm.Value = until;
                proc.Parameters.Add(lockedUntilPrm);

                var nowPrm = DataProvider.CreateParameter();
                nowPrm.ParameterName = "@Now";
                nowPrm.DbType = DbType.DateTime;
                nowPrm.Value = DateTime.Now;
                proc.Parameters.Add(nowPrm);

                rows = proc.ExecuteNonQuery();
            }
            return rows > 0;
        }
        internal static IEnumerable<Message> SelectSignedMessages(DataHandler context, string lockId)
        {
            return context.Messages.Where(m => m.LockId == lockId);
        }

        private static void ProcessQueuedMessagesFinished()
        {
            Debug.WriteLine("#Notification> _mailSendingInProgress false ");
            _mailSendingInProgress = false;
        }

        private static void ProcessMessage(Message message, DataHandler context)
        {
            if (String.IsNullOrEmpty(message.Address))
            {
                Debug.WriteLine("#Notification> Message is not sent and removed from the queue. Reason: Address is not provided. ");
                Logger.WriteWarning("Message is not sent. Reason: Address is not provided.");
                context.Messages.DeleteOnSubmit(message);
                return;
            }

            var retryCount = Configuration.RetryCount;
            while (true)
            {
                try
                {
                    SendMessage(message);
                    context.Messages.DeleteOnSubmit(message);
                    break;
                }
                catch (SmtpFailedRecipientException exception)
                {
                    SmtpStatusCode statusCode = exception.StatusCode;

                    if ((--retryCount != 0) &&
                        (statusCode == SmtpStatusCode.MailboxBusy || statusCode == SmtpStatusCode.MailboxUnavailable || statusCode == SmtpStatusCode.TransactionFailed))
                    {
                        Debug.WriteLine("#Notification SmtpFailedRecipientException RETRY> " + exception.Message + exception.StatusCode);
                        Logger.WriteException(exception);
                        Thread.Sleep(Configuration.RetryDelay);
                    }
                    else
                    {
                        context.Messages.DeleteOnSubmit(message);
                        Debug.WriteLine("#Notification SmtpFailedRecipientException> " + exception.Message + exception.StatusCode);
                        Debug.WriteLine("#Notification SmtpFailedRecipientException> Message is removed from the queue.");
                        Logger.WriteException(exception);
                        Logger.WriteWarning("Message is removed from queue.");
                        break;
                    }
                }
            }
        }

        private static void SendMessage(Message message)
        {
            if (TestMode)
                Debug.WriteLine("#Notification> Message sent, subject: " + message.Subject);
            else
            {
                _smtpClient.Send(message.GenerateMailMessage());

                Debug.WriteLine("#Notification> Message sent, subject: " + message.Subject);
            }

        }
    }
}
