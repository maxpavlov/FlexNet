using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Exchange.WebServices.Data;
using SenseNet.ContentRepository.Storage;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Diagnostics;
using SenseNet.Portal.Exchange;
using SenseNet.Diagnostics;

namespace SenseNet.Workflow.Activities
{

    public sealed class ExchangePoller : AsyncCodeActivity<EmailMessage[]>
    {
        public InArgument<bool> PushNotification { get; set; }
        public InArgument<string> ContentListPath { get; set; }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            var pushNotification = PushNotification.Get(context);
            var contentListPath = ContentListPath.Get(context);

            if (string.IsNullOrEmpty(contentListPath))
            {
                Logger.WriteError("ExchangePoller activity: ContentList path is empty.", ExchangeHelper.ExchangeLogCategory);
                return null;
            }

            var GetMessageInfosDelegate = new Func<bool, string, EmailMessage[]>(GetMessageInfos);
            context.UserState = GetMessageInfosDelegate;
            return GetMessageInfosDelegate.BeginInvoke(pushNotification, contentListPath, callback, state);            
        }

        protected override EmailMessage[] EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            var GetMessageInfosDelegate = (Func<bool, string, EmailMessage[]>)context.UserState;
            return (EmailMessage[])GetMessageInfosDelegate.EndInvoke(result);            
        }

        private EmailMessage[] GetMessageInfos(bool pushNotification, string contentListPath)
        {
            var contentList = Node.LoadNode(contentListPath);
            var mailboxEmail = contentList["ListEmail"] as string;

            if (!pushNotification)
            {
                var service = ExchangeHelper.CreateConnection(mailboxEmail);
                if (service == null)
                    return new EmailMessage[0];

                var items = ExchangeHelper.GetItems(service, mailboxEmail);
                var infos = items.Select(item => EmailMessage.Bind(service, item.Id, new PropertySet(BasePropertySet.FirstClassProperties, ItemSchema.Body, ItemSchema.Attachments, ItemSchema.MimeContent)));

                return infos.ToArray();
            }
            else
            {
                var emailsPath = RepositoryPath.Combine(contentListPath, ExchangeHelper.PUSHNOTIFICATIONMAILCONTAINER);
                var incomingEmails = SenseNet.Search.ContentQuery.Query("+InFolder:\""+emailsPath+"\"", new Search.QuerySettings { EnableAutofilters = false });
                if (incomingEmails.Count == 0)
                    return new EmailMessage[0];

                var service = ExchangeHelper.CreateConnection(mailboxEmail);
                if (service == null)
                    return new EmailMessage[0];

                var msgs = new List<EmailMessage>();
                foreach (var emailnode in incomingEmails.Nodes)
                {
                    var ids = emailnode["Description"] as string;
                    if (string.IsNullOrEmpty(ids))
                        continue;

                    var idList = ids.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var id in idList)
                    {
                        var msg = EmailMessage.Bind(service, id, new PropertySet(BasePropertySet.FirstClassProperties, ItemSchema.Body, ItemSchema.Attachments, ItemSchema.MimeContent));
                        msgs.Add(msg);
                    }

                    // delete email node 
                    emailnode.ForceDelete();
                }
                return msgs.ToArray();
            }
        }
    }
}
