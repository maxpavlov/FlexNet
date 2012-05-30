using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Versioning;
using SenseNet.Diagnostics;
using SenseNet.ContentRepository.Schema;

namespace SenseNet.Workflow
{
    [ContentHandler]
    public class WorkflowDefinitionHandler : File
    {
        public WorkflowDefinitionHandler(Node parent) : this(parent, null) { }
        public WorkflowDefinitionHandler(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected WorkflowDefinitionHandler(NodeToken nt) : base(nt) { }

        public const string ABORTONRELATEDCONTENTCHANGE = "AbortOnRelatedContentChange";
        [RepositoryProperty(ABORTONRELATEDCONTENTCHANGE, RepositoryDataType.Int)]
        public bool AbortOnRelatedContentChange
        {
            get { return base.GetProperty<int>(ABORTONRELATEDCONTENTCHANGE) != 0; }
            set { base.SetProperty(ABORTONRELATEDCONTENTCHANGE, value ? 1 : 0); }
        }

        [RepositoryProperty(VERSIONINGMODE, RepositoryDataType.Int)]
        public override VersioningType VersioningMode
        {
            get
            {
                return VersioningType.MajorOnly;
            }
            set
            {
                Logger.WriteWarning("MajorOnly versioning is compulsory for WorkflowDefinition objects, ignoring change on node " + this.Path);
                base.VersioningMode = VersioningType.MajorOnly;
            }
        }

        public override object GetProperty(string name)
        {
            switch (name)
            {
                case VERSIONINGMODE:
                    return VersioningMode;
                case ABORTONRELATEDCONTENTCHANGE:
                    return AbortOnRelatedContentChange;
                default:
                    return base.GetProperty(name);
            }
        }

        public override void SetProperty(string name, object value)
        {
            switch (name)
            {
                case VERSIONINGMODE:
                    VersioningMode = (VersioningType)value;
                    break;
                case ABORTONRELATEDCONTENTCHANGE:
                    AbortOnRelatedContentChange = (bool)value;
                    break;
                default:
                    base.SetProperty(name, value);
                    break;
            }
        }
    }
}
