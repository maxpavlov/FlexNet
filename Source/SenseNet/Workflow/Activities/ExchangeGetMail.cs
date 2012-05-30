using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Diagnostics;
using SenseNet.Portal.Exchange;
using Microsoft.Exchange.WebServices.Data;

namespace SenseNet.Workflow.Activities
{
    public sealed class ExchangeGetMail : AsyncCodeActivity<EmailMessage>
    {
        public InArgument<string> ItemId { get; set; }
        public InArgument<string> MailboxEmail { get; set; }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            var GetMessageInfoDelegate = new Func<string, string, EmailMessage>(GetMessageInfo);
            context.UserState = GetMessageInfoDelegate;
            return GetMessageInfoDelegate.BeginInvoke(ItemId.Get(context), MailboxEmail.Get(context), callback, state);            
        }

        protected override EmailMessage EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            var GetMessageInfoDelegate = (Func<string, string, EmailMessage>)context.UserState;
            return (EmailMessage)GetMessageInfoDelegate.EndInvoke(result);            
        }

        private EmailMessage GetMessageInfo(string itemId, string email)
        {
            var service = ExchangeHelper.CreateConnection(email);
            if (service == null)
                return null;

            var item = ExchangeHelper.GetItem(service, itemId);
            var message = EmailMessage.Bind(service, item.Id, new PropertySet(BasePropertySet.FirstClassProperties, ItemSchema.Body, ItemSchema.Attachments, ItemSchema.MimeContent));
            return message;
        }
    }
}
