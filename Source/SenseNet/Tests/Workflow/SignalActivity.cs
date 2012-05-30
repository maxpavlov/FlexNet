using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;

namespace SenseNet.ContentRepository.Tests.Workflow
{
    public class SignalActivity : CodeActivity
    {
        public InArgument<string> Message { get; set; }
        
        protected override void Execute(CodeActivityContext context)
        {
            WfWatcher.SendMessage(Message.Get(context));
        }
    }
}
