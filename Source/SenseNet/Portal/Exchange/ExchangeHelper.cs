using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using Microsoft.Exchange.WebServices.Data;
using System.IO;
using SenseNet.ContentRepository;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Search;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.Exchange
{
    public class ExchangeHelper
    {
        public static string PUSHNOTIFICATIONMAILCONTAINER = "IncomingEmails";
        public static readonly string[] ExchangeLogCategory = new[] { "Exchange" };

        public static ExchangeService CreateConnection(string emailAddress)
        {
            // Hook up the cert callback to prevent error if Microsoft.NET doesn't trust the server
            ServicePointManager.ServerCertificateValidationCallback =
                delegate(Object obj, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
                {
                    return true;
                };

            ExchangeService service = null;
            if (!string.IsNullOrEmpty(Configuration.ExchangeAddress))
            {
                service = new ExchangeService();
                service.Url = new Uri(Configuration.ExchangeAddress);
            }
            else
            {
                if (!string.IsNullOrEmpty(emailAddress))
                {
                    service = new ExchangeService();
                    service.AutodiscoverUrl(emailAddress);
                }
            }

            return service;
        }

        public static string GetWaterMark(Node doclibrary)
        {
            var incomingEmailsContainer = Node.LoadNode(RepositoryPath.Combine(doclibrary.Path, ExchangeHelper.PUSHNOTIFICATIONMAILCONTAINER));
            if (incomingEmailsContainer == null)
                return null;

            return incomingEmailsContainer["Description"] as string;
        }

        public static void Subscribe(Node doclibrary)
        {
            var service = ExchangeHelper.CreateConnection(doclibrary["ListEmail"] as string);
            ExchangeHelper.Subscribe(doclibrary, service);
        }
        public static void Subscribe(Node doclibrary, ExchangeService service)
        {
            if (service == null)
                return;

            var address = doclibrary["ListEmail"] as string;
            if (string.IsNullOrEmpty(address))
                return;

            var mailbox = new Mailbox(address);
            var folderId = new FolderId(WellKnownFolderName.Inbox, mailbox);
            var servicePath = string.Format(Configuration.PushNotificationServicePath, doclibrary.Path);

            var watermark = ExchangeHelper.GetWaterMark(doclibrary);

            var ps = service.SubscribeToPushNotifications(new List<FolderId> { folderId }, new Uri(servicePath), Configuration.StatusPollingIntervalInMinutes, watermark, EventType.NewMail);

            var loginfo = string.Concat(" - Path:",doclibrary.Path, ", Email:", address, ", Watermark:", watermark, ", SubscriptionId:", ps.Id);
            Logger.WriteInformation("Exchange subscription" + loginfo, ExchangeHelper.ExchangeLogCategory);

            // persist subscription id to doclib, so that multiple subscriptions are handled correctly
            var user = User.Current;
            try
            {
                AccessProvider.Current.SetCurrentUser(User.Administrator);

                var retryCount = 3;
                while (retryCount > 0)
                {
                    try
                    {
                        doclibrary["ExchangeSubscriptionId"] = ps.Id;
                        doclibrary.Save();
                        break;
                    }
                    catch (NodeIsOutOfDateException)
                    {
                        retryCount--;
                        doclibrary = Node.LoadNode(doclibrary.Id);
                    }
                }
            }
            finally
            {
                AccessProvider.Current.SetCurrentUser(user);
            }
        }

        public static FindItemsResults<Item> GetItems(ExchangeService service, string address)
        {
            var searchFilter = new SearchFilter.SearchFilterCollection(LogicalOperator.And, new SearchFilter.IsEqualTo(EmailMessageSchema.IsRead, false));
            //var searchFilter = new SearchFilter.SearchFilterCollection(LogicalOperator.And, new SearchFilter.IsEqualTo(EmailMessageSchema.Subject, "MyExchangeTest"));

            var mailbox = new Mailbox(address);
            var folderId = new FolderId(WellKnownFolderName.Inbox, mailbox);

            var items = service.FindItems(folderId, searchFilter, new ItemView(5));
            return items;
        }

        public static FindItemsResults<Item> GetItems(ExchangeService service, Node doclibrary)
        {
            var address = doclibrary["ListEmail"] as string;
            if (string.IsNullOrEmpty(address))
                return null;

            var items = ExchangeHelper.GetItems(service, address);
            return items;
        }

        public static Item GetItem(ExchangeService service, string id)
        {
            var item = Item.Bind(service, new ItemId(id));
            return item;
        }

        public static void SetAttachment(SenseNet.ContentRepository.File file, FileAttachment fileAttachment)
        {
            using (var stream = new MemoryStream())
            {
                fileAttachment.Load(stream);
                stream.Seek(0, SeekOrigin.Begin);

                var binaryData = new BinaryData();
                binaryData.SetStream(stream);
                file.Binary = binaryData;
                file.Save();
            }
        }
    }
}
