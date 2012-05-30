using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage.Events;
using System.Configuration;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository;

namespace SenseNet.Workflow
{
    public class WorkflowNotificationObserver : NodeObserver
    {
        public static string CONTENTCHANGEDNOTIFICATIONTYPE = "ContentChanged";

        protected override void OnNodeModified(object sender, NodeEventArgs e)
        {
            InstanceManager.NotifyContentChanged(new WorkflowNotificationEventArgs(e.SourceNode.Id, CONTENTCHANGEDNOTIFICATIONTYPE, null));
            
            AbortRelatedWorkflows(e.SourceNode, WorkflowApplicationAbortReason.RelatedContentChanged);
        }

        private void AbortRelatedWorkflows(Node currentNode, WorkflowApplicationAbortReason reason)
        {
            //TODO: WF: Testing StorageContext.Search.IsOuterEngineEnabled flag hack
            if (!StorageContext.Search.IsOuterEngineEnabled)
                return;

            var query = String.Format("+TypeIs:Workflow +RelatedContent:{0} .AUTOFILTERS:OFF", currentNode.Id);
            var result = SenseNet.Search.ContentQuery.Query(query);

            foreach (WorkflowHandlerBase workflow in result.Nodes)
                if (workflow.WorkflowStatus == WorkflowStatusEnum.Running && workflow.AbortOnRelatedContentChange)
                    InstanceManager.Abort(workflow, reason);
        }
    }
}
