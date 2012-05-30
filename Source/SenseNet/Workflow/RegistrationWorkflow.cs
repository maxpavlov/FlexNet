using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.Search;
using SenseNet.ContentRepository.i18n;

namespace SenseNet.Workflow
{
    [ContentHandler]
    public class RegistrationWorkflow : WorkflowHandlerBase
    {
        public RegistrationWorkflow(Node parent) : this(parent, null) { }
		public RegistrationWorkflow(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected RegistrationWorkflow(NodeToken nt) : base(nt) { }

        public const string USERNAME = "UserName";
        [RepositoryProperty(USERNAME, RepositoryDataType.String)]
        public string UserName
        {
            get { return (string)base.GetProperty(USERNAME); }
            set { base.SetProperty(USERNAME, value); }
        }

        public override void Save()
        {
            AssertUserName();
            base.Save();
        }

        public override void Save(NodeSaveSettings settings)
        {
            AssertUserName();
            base.Save(settings);
        }

        //============================================================== Helper methods

        private void AssertUserName()
        {
            if (!StorageContext.Search.IsOuterEngineEnabled)
                return;

            if(this.WorkflowStarted)
                return;

            var uname = this.UserName;
            if (string.IsNullOrEmpty(uname))
                throw new InvalidOperationException(String.Format(SenseNetResourceManager.Current.GetString("RegistrationWorkflow", "CompulsoryField"), USERNAME));

            if (ContentQuery.Query(string.Format("+TypeIs:User +Name:{0} .COUNTONLY", uname)).Count > 0)
                throw new InvalidOperationException(String.Format(SenseNetResourceManager.Current.GetString("RegistrationWorkflow", "UserNameAlreadyExist"), uname));
        }
    }
}
