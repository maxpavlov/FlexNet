using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository;
using System.ComponentModel;
using SenseNet.Workflow.Activities.Design;

namespace SenseNet.Workflow.Activities
{
    [Designer(typeof(ApproveContentDesigner))]
    public sealed class ApproveContent : CodeActivity
    {
        // Define an activity input argument of type string
        public InArgument<string> ContentPath { get; set; }

        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override void Execute(CodeActivityContext context)
        {
            // Obtain the runtime value of the Text input argument
            var contentItem = Node.Load<GenericContent>(ContentPath.Get(context));
            using (InstanceManager.CreateRelatedContentProtector(contentItem, context))
            {
                contentItem.Approve();
            }
        }
    }
}
