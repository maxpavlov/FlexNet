using System;
using System.Activities;
using System.Configuration;
using SenseNet.ContentRepository.Storage;
using System.Diagnostics;
using SenseNet.ContentRepository;

namespace SenseNet.Workflow.Activities
{
    public class WaitForContentChanged : NativeActivity
    {

        public InArgument<string> ContentPath { get; set; }

        private Variable<int> notificationId = new Variable<int>("notificationId", 0);


        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.AddImplementationVariable(notificationId);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var bookMarkName = Guid.NewGuid().ToString();
            var content = new WfContent() { Path = ContentPath.Get(context) };

            notificationId.Set(context, context.GetExtension<ContentWorkflowExtension>().RegisterWait(content, bookMarkName));

            context.CreateBookmark(bookMarkName, new BookmarkCallback(Continue));
        }

        void Continue(NativeActivityContext context, Bookmark bookmark, object obj)
        {
            var eventArgs = obj as WorkflowNotificationEventArgs;
            if (eventArgs.NotificationType != WorkflowNotificationObserver.CONTENTCHANGEDNOTIFICATIONTYPE)
                return;

            InstanceManager.ReleaseWait(notificationId.Get(context));
        }
    }
}
