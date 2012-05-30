using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities.Hosting;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.Workflow
{
    public class ContentWorkflowExtension : IWorkflowInstanceExtension
    {
        //================================= IWorkflowInstanceExtension Members

        public WorkflowInstanceProxy _instance;

        public string WorkflowInstancePath { get; set; }
        public string ContentPath { get; set; }

        private string _uid;

        public string UID
        {
            get 
            {
                if (string.IsNullOrEmpty(_uid))
                    _uid = Guid.NewGuid().ToString();
                return _uid; 
            }
            set { _uid = value; }
        }
        
        public IEnumerable<object> GetAdditionalExtensions()
        {
            yield break;
        }

        public int RegisterWait(WfContent content, string bookMarkName)
        {
            var myGuid = _instance.Id;
            var wfNodePath = WorkflowInstancePath;
            return InstanceManager.RegisterWait(content.Id, myGuid, bookMarkName, wfNodePath);
        }

        public void ReleaseWait(int notificationId)
        {
            InstanceManager.ReleaseWait(notificationId);
        }

        public int[] RegisterWaitForMultipleContent(IEnumerable<WfContent> contents, string bookMarkName)
        {
            var myGuid = _instance.Id;
            var wfNodePath = WorkflowInstancePath;
            var idSet = new List<int>();
            foreach (var content in contents)
            {
                var id = InstanceManager.RegisterWait(content.Id, myGuid, bookMarkName, wfNodePath);
                idSet.Add(id);
            }
            return idSet.ToArray();
        }

        public void SetInstance(WorkflowInstanceProxy instance)
        {
            _instance = instance;
        }
    }
}
