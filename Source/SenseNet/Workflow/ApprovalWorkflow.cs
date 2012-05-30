using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository;

namespace SenseNet.Workflow
{
    [ContentHandler]
    class ApprovalWorkflow : WorkflowHandlerBase
    {
        public ApprovalWorkflow(Node parent) : this(parent, null) { }
		public ApprovalWorkflow(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected ApprovalWorkflow(NodeToken nt) : base(nt) { }

        public const string FIRSTLEVELTIMEFRAME = "FirstLevelTimeFrame";
        [RepositoryProperty(FIRSTLEVELTIMEFRAME, RepositoryDataType.String)]
        public string FirstLevelTimeFrame
        {
            get { return (string)base.GetProperty(FIRSTLEVELTIMEFRAME); }
            set { base.SetProperty(FIRSTLEVELTIMEFRAME, value); }
        }

        public const string SECONDLEVELTIMEFRAME = "SecondLevelTimeFrame";
        [RepositoryProperty(SECONDLEVELTIMEFRAME, RepositoryDataType.String)]
        public string SecondLevelTimeFrame
        {
            get { return (string)base.GetProperty(SECONDLEVELTIMEFRAME); }
            set { base.SetProperty(SECONDLEVELTIMEFRAME, value); }
        }

        public override void Save()
        {
            AssertTimeSpan();
            base.Save();
        }

        public override void Save(NodeSaveSettings settings)
        {
            AssertTimeSpan();
            base.Save(settings);
        }

        public override void Save(SavingMode mode)
        {
            AssertTimeSpan();
            base.Save(mode);
        }
        
        public override object GetProperty(string name)
        {
            switch (name)
            {
                case FIRSTLEVELTIMEFRAME:
                    return this.FirstLevelTimeFrame;
                case SECONDLEVELTIMEFRAME:
                    return this.SecondLevelTimeFrame;
                default:
                    return base.GetProperty(name);
            }
        }

        public override void SetProperty(string name, object value)
        {
            switch (name)
            {
                case FIRSTLEVELTIMEFRAME:
                    this.FirstLevelTimeFrame = value as string;
                    break;
                case SECONDLEVELTIMEFRAME:
                    this.SecondLevelTimeFrame = value as string;
                    break;
                default:
                    base.SetProperty(name, value);
                    break;
            }
        }

        //============================================================== Helper methods

        private void AssertTimeSpan()
        {
            var ts1 = this.FirstLevelTimeFrame ?? string.Empty;
            var ts2 = this.SecondLevelTimeFrame ?? string.Empty;
            TimeSpan tsVal;

            if (!TimeSpan.TryParse(ts1, out tsVal))
                throw new InvalidOperationException("Invalid value: " + FIRSTLEVELTIMEFRAME);

            if (!TimeSpan.TryParse(ts2, out tsVal))
                throw new InvalidOperationException("Invalid value: " + SECONDLEVELTIMEFRAME);
        }
    }
}
