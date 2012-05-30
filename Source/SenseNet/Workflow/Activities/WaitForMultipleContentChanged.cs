using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Diagnostics;
using SenseNet.ContentRepository;

namespace SenseNet.Workflow.Activities
{
    public class WaitForMultipleContentChanged : NativeActivity
    {
        public InArgument<string> ContentPath { get; set; }
        public InArgument<IEnumerable<string>> ContentPaths { get; set; }
        public InArgument<bool> WaitForAll { get; set; }

        // Cannot be int[]: 
        // System.Activities.InvalidWorkflowException: The following errors were encountered while processing the workflow tree:
        // 'DynamicActivity': The private implementation of activity '1: DynamicActivity' has the following validation error:
        // Literal only supports value types and the immutable type System.String.  The type System.Int32[] cannot be used as a literal.
        private Variable<string> notificationIds = new Variable<string>("notificationIds", String.Empty);
        private Variable<string> nodeIds = new Variable<string>("nodeIds", String.Empty);
        private Variable<int> counter = new Variable<int>("counter", 0);

        protected override bool CanInduceIdle
        {
            get { return true; }
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.AddImplementationVariable(notificationIds);
            metadata.AddImplementationVariable(nodeIds);
            metadata.AddImplementationVariable(counter);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var waitForAll = WaitForAll.Get(context);
            var paths = ContentPaths.Get(context);
            var contents = paths.Select(p => new WfContent { Path = p }).ToArray();
            var ext = context.GetExtension<ContentWorkflowExtension>();

            var bookMarkName = Guid.NewGuid().ToString();
            var waitIds = ext.RegisterWaitForMultipleContent(contents, bookMarkName);
            var waitIdString = String.Join(",", waitIds.Select(w => w.ToString()).ToArray());
            var nodeIdString = String.Join(",", contents.Select(w => w.Id.ToString()).ToArray());

            notificationIds.Set(context, waitIdString);
            counter.Set(context, waitForAll ? contents.Length : 1);

            context.CreateBookmark(bookMarkName, new BookmarkCallback(Continue));
        }

        private void Continue(NativeActivityContext context, Bookmark bookmark, object obj)
        {
            var eventArgs = obj as WorkflowNotificationEventArgs;
            if (eventArgs.NotificationType != WorkflowNotificationObserver.CONTENTCHANGEDNOTIFICATIONTYPE)
                return;

            ReleaseWait(context, eventArgs.NodeId);

            var count = counter.Get(context) - 1;
            counter.Set(context, count);
            if (count > 0)
                context.CreateBookmark(bookmark.Name, new BookmarkCallback(Continue));
        }
        private void ReleaseWait(NativeActivityContext context, int nodeId)
        {
            var notifIdList = notificationIds.Get(context).Split(',');
            var nodeIdList = nodeIds.Get(context).Split(',');
            var nodeIdString = nodeId.ToString();
            for (int i = 0; i < nodeIdList.Length; i++)
            {
                if (nodeIdList[i] == nodeIdString)
                {
                    InstanceManager.ReleaseWait(Convert.ToInt32(notifIdList[i]));
                    break;
                }
            }
        }
        private void ReleaseWaits(NativeActivityContext context)
        {
            var idSet = notificationIds.Get(context).Split(',').Select(i => int.Parse(i));
            foreach (var id in idSet)
                InstanceManager.ReleaseWait(id);
        }

    }
}
