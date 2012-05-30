using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Exchange.WebServices.Data;

namespace SenseNet.Workflow.Activities
{
    public class ExchangeMarkAsRead : AsyncCodeActivity
    {
        public InArgument<EmailMessage> Message { get; set; }


        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            var message = Message.Get(context);

            var MarkAsReadDelegate = new Action<EmailMessage>(MarkAsRead);
            context.UserState = MarkAsReadDelegate;
            return MarkAsReadDelegate.BeginInvoke(message, callback, state);            
        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            var MarkAsReadDelegate = (Action<EmailMessage>)context.UserState;
            MarkAsReadDelegate.EndInvoke(result);            
        }

        private void MarkAsRead(EmailMessage message)
        {
            message.IsRead = true;
            message.Update(ConflictResolutionMode.AlwaysOverwrite);
        }
    }
}
