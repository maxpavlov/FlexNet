using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;

namespace SenseNet.Workflow.Activities
{
    public class CreateApprovalTask : NativeActivity
    {
        public InArgument<string> TaskType { get; set; }
        public InArgument<string> TasksLocationPath { get; set; }
        public InArgument<string> AssignToPath { get; set; }


        protected override void Execute(NativeActivityContext context)
        {
            
        }
    }
}
