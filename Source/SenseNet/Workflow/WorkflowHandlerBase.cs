using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using System.Activities;
using SenseNet.ContentRepository.Storage;
using System.Activities.Hosting;
using SenseNet.ContentRepository.Schema;
using System.Activities.XamlIntegration;
using System.Web.Configuration;
using System.Reflection;
using System.Configuration;
using System.Diagnostics;
using SenseNet.ContentRepository.Versioning;
using SenseNet.Diagnostics;

namespace SenseNet.Workflow
{
    public enum WorkflowStatusEnum { Created = 0, Running = 1, Aborted = 2, Completed = 3 }
    
    [ContentHandler]
    public class WorkflowHandlerBase : GenericContent
    {
        public WorkflowHandlerBase(Node parent) : this(parent, null) { }
        public WorkflowHandlerBase(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected WorkflowHandlerBase(NodeToken nt) : base(nt) { }
        
        public const string WORKFLOWSTATUS = "WorkflowStatus";
        [RepositoryProperty(WORKFLOWSTATUS, RepositoryDataType.String)]
        public WorkflowStatusEnum WorkflowStatus
        {
            get
            {
                var result = WorkflowStatusEnum.Created;
                var enumVal = base.GetProperty<string>(WORKFLOWSTATUS);
                if (string.IsNullOrEmpty(enumVal))
                    return result;

                Enum.TryParse(enumVal, false, out result);

                return result;
            }
            set
            {
                this[WORKFLOWSTATUS] = Enum.GetName(typeof(WorkflowStatusEnum), value); 
            }
        }

        public const string WORKFLOWDEFINITIONVERSION = "WorkflowDefinitionVersion";
        [RepositoryProperty(WORKFLOWDEFINITIONVERSION, RepositoryDataType.String)]
        public string WorkflowDefinitionVersion
        {
            get { return (string)base.GetProperty(WORKFLOWDEFINITIONVERSION); }
            internal set { base.SetProperty(WORKFLOWDEFINITIONVERSION, value); }
        }

        public const string WORKFLOWINSTANCEGUID = "WorkflowInstanceGuid";
        [RepositoryProperty(WORKFLOWINSTANCEGUID, RepositoryDataType.String)]
        public string WorkflowInstanceGuid
        {
            get { return (string)base.GetProperty(WORKFLOWINSTANCEGUID); }
            internal set { base.SetProperty(WORKFLOWINSTANCEGUID, value); }
        }

        public const string RELATEDCONTENT = "RelatedContent";
        [RepositoryProperty(RELATEDCONTENT, RepositoryDataType.Reference)]
        public Node RelatedContent
        {
            get { return base.GetReference<Node>(RELATEDCONTENT); }
            set
            {
                if (value != null)
                    this.RelatedContentTimestamp = value.NodeTimestamp;
                base.SetReference(RELATEDCONTENT, value);
            }
        }

        public const string RELATEDCONTENTTIMESTAMP = "RelatedContentTimestamp";
        [RepositoryProperty(RELATEDCONTENTTIMESTAMP, RepositoryDataType.Currency)]
        public long RelatedContentTimestamp
        {
            get
            {
                var value = base.GetProperty(RELATEDCONTENTTIMESTAMP);
                if (value == null)
                    return 0;
                return Convert.ToInt64(value);
            }
            internal set { base.SetProperty(RELATEDCONTENTTIMESTAMP, Convert.ToDecimal(value)); }
        }

        public const string SYSTEMMESSAGES = "SystemMessages";
        [RepositoryProperty(SYSTEMMESSAGES, RepositoryDataType.Text)]
        public string SystemMessages
        {
            get { return base.GetProperty<string>(SYSTEMMESSAGES); }
            internal set { base.SetProperty(SYSTEMMESSAGES, value); }
        }

        public const string CONTENTWORKFLOW = "ContentWorkflow";
        [RepositoryProperty(CONTENTWORKFLOW, RepositoryDataType.Int)]
        public bool ContentWorkflow
        {
            get { return base.GetProperty<int>(CONTENTWORKFLOW) != 0; }
            set { base.SetProperty(CONTENTWORKFLOW, value ? 1 : 0); }
        }

        public const string ABORTONRELATEDCONTENTCHANGE = "AbortOnRelatedContentChange";
        [RepositoryProperty(ABORTONRELATEDCONTENTCHANGE, RepositoryDataType.Int)]
        public bool AbortOnRelatedContentChange
        {
            get { return base.GetProperty<int>(ABORTONRELATEDCONTENTCHANGE) != 0; }
            set { base.SetProperty(ABORTONRELATEDCONTENTCHANGE, value ? 1 : 0); }
        }

        public string WorkflowTypeName
        {
            get { return NodeType.Name; }
        }

        public string WorkflowHostType
        {
            get
            {
                if (string.IsNullOrEmpty(WorkflowDefinitionVersion))
                    return null;

                return WorkflowTypeName + "-" + WorkflowDefinitionVersion;
            }
        }

        public bool WorkflowStarted
        {
            get
            {
                return !(string.IsNullOrEmpty(WorkflowInstanceGuid));
            }
        }

        public bool WorkflowRunnable
        {
            get
            {
                return !WorkflowStarted && Node.Exists(GetWorkflowDefinitionPath());
            }
        }
        
        public override bool IsTrashable
        {
            get
            {
                return false;
            }
        }

        public override VersioningType VersioningMode
        {
            get
            {
                return VersioningType.None;
            }
            set { }
        }

        public override ApprovingType ApprovingMode
        {
            get
            {
                return ApprovingType.False;
            }
            set { }
        }

        public override object GetProperty(string name)
        {
            switch (name)
            {
                case "VersioningMode":
                    return this.VersioningMode;
                case "ApprovingMode":
                    return this.ApprovingMode;
                case WORKFLOWSTATUS:
                    return this.WorkflowStatus;
                case WORKFLOWDEFINITIONVERSION:
                    return this.WorkflowDefinitionVersion;
                case WORKFLOWINSTANCEGUID:
                    return this.WorkflowInstanceGuid;
                case RELATEDCONTENT:
                    return RelatedContent;
                case RELATEDCONTENTTIMESTAMP:
                    return RelatedContentTimestamp;
                case SYSTEMMESSAGES:
                    return SystemMessages;
                case CONTENTWORKFLOW:
                    return ContentWorkflow;
                case ABORTONRELATEDCONTENTCHANGE:
                    return AbortOnRelatedContentChange;
                case "WorkflowTypeName":
                    return this.WorkflowTypeName;
                case "WorkflowHostType":
                    return this.WorkflowHostType;
                case "WorkflowStarted":
                    return this.WorkflowStarted;
                case "WorkflowRunnable":
                    return this.WorkflowRunnable;
                default:
                    return base.GetProperty(name);
            }
        }

        public override void SetProperty(string name, object value)
        {
            switch (name)
            {
                case "VersioningMode":
                    this.VersioningMode = (VersioningType)value;
                    break;
                case "ApprovingMode":
                    this.ApprovingMode = (ApprovingType) value;
                    break;
                case WORKFLOWSTATUS:
                    this.WorkflowStatus = (WorkflowStatusEnum) value;
                    break;
                case WORKFLOWDEFINITIONVERSION:
                    this.WorkflowDefinitionVersion = (string)value;
                    break;
                case WORKFLOWINSTANCEGUID:
                    this.WorkflowInstanceGuid = (string)value;
                    break;
                case RELATEDCONTENT:
                    this.RelatedContent = (Node)value;
                    break;
                case RELATEDCONTENTTIMESTAMP:
                    this.RelatedContentTimestamp = (long)value;
                    break;
                case SYSTEMMESSAGES:
                    this.SystemMessages = (string)value;
                    break;
                case CONTENTWORKFLOW:
                    this.ContentWorkflow = (bool)value;
                    break;
                case ABORTONRELATEDCONTENTCHANGE:
                    this.AbortOnRelatedContentChange = (bool)value;
                    break;
                default:
                    base.SetProperty(name, value);
                    break;
            }
        }

        public string GetWorkflowDefinitionPath()
        {
            var wfDefinitionName = String.Concat(this.NodeType.Name,".xaml");
            return System.IO.Path.Combine(Repository.WorkflowDefinitionPath, wfDefinitionName);
        }

        private WorkflowDefinitionHandler LoadWorkflowDefinition(out string version)
        {
            if (string.IsNullOrEmpty(WorkflowDefinitionVersion))
            {
                var def = Node.Load<WorkflowDefinitionHandler>(GetWorkflowDefinitionPath());
                version = def.Version.VersionString;
                return def;
            }

            version = WorkflowDefinitionVersion;
            return Node.Load<WorkflowDefinitionHandler>(GetWorkflowDefinitionPath(), VersionNumber.Parse(WorkflowDefinitionVersion));
        }


        internal Activity CreateWorkflowInstance()
        {
            string version;
            return CreateWorkflowInstance(out version);
        }

        internal Activity CreateWorkflowInstance(out string version)
        {
            string ns = WebConfigurationManager.AppSettings["NativeWorkflowNamespace"] as string;
            if (!string.IsNullOrEmpty(ns))
            {
                var asm = Assembly.LoadWithPartialName(ns);
                var cn = ns + "." + WorkflowTypeName;
                object act = asm.CreateInstance(cn);
                version = asm.GetName().Version.ToString();
                return act as Activity;
            }

            var workflowDefinition = LoadWorkflowDefinition(out version);
            var activity = ActivityXamlServices.Load(workflowDefinition.Binary.GetStream());
            return activity;
        }

        public virtual Dictionary<string, object> CreateParameters()
        {
            return new Dictionary<string, object>();
        }

        public override void Delete()
        {
            if(WorkflowStatus == WorkflowStatusEnum.Running)
                InstanceManager.Abort(this, WorkflowApplicationAbortReason.StateContentDeleted);
            base.Delete();
        }
        public override void ForceDelete()
        {
            if (WorkflowStatus == WorkflowStatusEnum.Running)
                InstanceManager.Abort(this, WorkflowApplicationAbortReason.StateContentDeleted);
            base.ForceDelete();
        }

        public override void Save(NodeSaveSettings settings)
        {
            // copy abortonrelatedcontentchange info from definition to instance
            var workflowDefinition = Node.Load<WorkflowDefinitionHandler>(GetWorkflowDefinitionPath());
            if (workflowDefinition != null)
                this.AbortOnRelatedContentChange = workflowDefinition.AbortOnRelatedContentChange;

            base.Save(settings);
        }
    }
}
