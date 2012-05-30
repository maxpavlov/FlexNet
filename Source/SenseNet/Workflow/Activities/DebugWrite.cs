using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Diagnostics;

namespace SenseNet.Workflow.Activities
{
    public class DebugWrite : NativeActivity
    {
        public InArgument<string> Message { get; set; }
        protected override void Execute(NativeActivityContext context)
        {
            var msg = Message.Get<string>(context);
            if (!String.IsNullOrEmpty(msg))
                Trace.WriteLine(msg);
        }
    }
}
