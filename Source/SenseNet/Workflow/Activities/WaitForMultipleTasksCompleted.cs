using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository;

namespace SenseNet.Workflow.Activities
{
    public class WaitForMultipleTasksCompleted : NativeActivity<bool>
    {
        public InArgument<IEnumerable<string>> ContentPaths { get; set; }
        public InArgument<bool> WaitForAllTrue { get; set; }

        // Cannot be int[]: 
        // System.Activities.InvalidWorkflowException: The following errors were encountered while processing the workflow tree:
        // 'DynamicActivity': The private implementation of activity '1: DynamicActivity' has the following validation error:
        // Literal only supports value types and the immutable type System.String.  The type System.Int32[] cannot be used as a literal.
        private Variable<string> notificationIds = new Variable<string>("notificationIds", String.Empty);

        protected override bool CanInduceIdle
        {
            get { return true; }
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.AddImplementationVariable(notificationIds);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var waitForAllTrue = WaitForAllTrue.Get(context);
            var paths = ContentPaths.Get(context);
            var contents = paths.Select(p => new WfContent { Path = p }).ToArray();
            var ext = context.GetExtension<ContentWorkflowExtension>();

            var bookMarkName = Guid.NewGuid().ToString();

            var waitIds = ext.RegisterWaitForMultipleContent(contents, bookMarkName);
            var waitIdString = String.Join(",", waitIds.Select(w => w.ToString()).ToArray());
            notificationIds.Set(context, waitIdString);

            context.CreateBookmark(bookMarkName, new BookmarkCallback(Continue));
        }

        private void Continue(NativeActivityContext context, Bookmark bookmark, object obj)
        {
            var eventArgs = obj as WorkflowNotificationEventArgs;
            if (eventArgs.NotificationType != WorkflowNotificationObserver.CONTENTCHANGEDNOTIFICATIONTYPE)
                return;

            //null, "yes", "no", "tentative"
            var results = ContentPaths.Get(context).Select(p => (string)((new WfContent(p))["Result"]));

            var waitForAllTrue = WaitForAllTrue.Get(context);
            var countOfFalse = 0;
            var countOfNull = 0;
            var countOfTrue = 0;
            foreach (var item in results)
            {
                switch (item)
                {
                    case null: countOfNull++; break;
                    case "": countOfNull++; break;
                    case "no": countOfFalse++; break;
                    case "yes": countOfTrue++; break;
                    default: break;
                }
            }

            bool result;
            bool finish;
            if (countOfFalse > 0)
            {
                result = false;
                finish = true;
            }
            else
            {
                if (waitForAllTrue)
                    finish = countOfFalse == 0 && countOfNull == 0;
                else
                    finish = countOfTrue > 0;
                result = finish;
            }

            this.Result.Set(context, result);
            if (finish)
                ReleaseWaits(context);
            else
                context.CreateBookmark(bookmark.Name, new BookmarkCallback(Continue));
        }
        private void ReleaseWaits(NativeActivityContext context)
        {
            var idSet = notificationIds.Get(context).Split(',').Select(i => int.Parse(i));
            foreach (var id in idSet)
                InstanceManager.ReleaseWait(id);
        }

    }
}
