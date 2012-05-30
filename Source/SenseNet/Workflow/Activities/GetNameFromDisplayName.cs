using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using SenseNet.ContentRepository;

namespace SenseNet.Workflow.Activities
{
    public class GetNameFromDisplayName : NativeActivity<string>
    {
        public InArgument<string> ContentDisplayName { get; set; }
        public InArgument<string> ContentOriginalName { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            var displayName = ContentDisplayName.Get(context);
            var originalName = ContentOriginalName.Get(context);

            var newName = ContentNamingHelper.GetNameFromDisplayName(originalName, displayName);

            Result.Set(context, newName);
        }
    }
}
