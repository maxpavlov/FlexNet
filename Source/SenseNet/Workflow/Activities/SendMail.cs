using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Net.Mail;
using SenseNet.Diagnostics;
using System.ComponentModel;

namespace SenseNet.Workflow.Activities
{
    //[Designer(typeof(SenseNet.Workflow.Activities.SendMailDesigner))]
    public sealed class SendMail : CodeActivity
    {
        public InArgument<string> Address { get; set; }
        public InArgument<string> Subject { get; set; }
        public InArgument<string> Body { get; set; }
        public InArgument<bool> IsBodyHtml { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                using (var client = new SmtpClient())
                {
                    var msg = new MailMessage(@"noreply@" + client.Host, Address.Get(context), Subject.Get(context), Body.Get(context));
                    msg.IsBodyHtml = IsBodyHtml.Get(context);
                    client.Send(msg);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
            }
        }
    }
}
