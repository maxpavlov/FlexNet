using System.Collections.Generic;
using System.Linq;
using System.Web;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Versioning;
using SenseNet.Search;

namespace SenseNet.ContentRepository
{
    [ContentHandler]
    public class ExpenseClaim : Folder
    {
        public ExpenseClaim(Node parent) : this(parent, null) { }
        public ExpenseClaim(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected ExpenseClaim(NodeToken nt) : base(nt) { }

        private Node GetAssociatedWorkflow()
        {
            if (HttpContext.Current != null && HttpContext.Current.Request.QueryString.AllKeys.Contains("ContentTypeName"))
            {
                var contentTypeName = HttpContext.Current.Request["ContentTypeName"];

                return string.IsNullOrEmpty(contentTypeName)
                           ? null
                           : Node.LoadNode(contentTypeName);
            }

            return null;
        }

        public User GetApprover()
        {
            var workflow = this.GetAssociatedWorkflow();
            
            return this.GetApprover(workflow.GetProperty<int>("BudgetLimit"), workflow.GetReference<User>("CEO"));
        }

        public User GetApprover(int budgetLimit, User CEO)
        {
            var manager = this.CreatedBy.GetReference<User>("Manager");

            if (this.Sum > budgetLimit || manager == null)
                return CEO;
            else
                return manager;
        }

        public int Sum
        {
            get
            {
                if (!StorageContext.Search.IsOuterEngineEnabled)
                    return 0;

                QueryResult cq = ContentQuery.Query(string.Format("+Type:expenseclaimitem +InTree:\"{0}\"", this.Path));
                return cq.Nodes.Sum(elem => elem.GetProperty<int>("Amount"));
            }
        }
        
        public override object GetProperty(string name)
        {
            switch (name)
            {
                //case "BudgetLimit":
                //    return this.BudgetLimit;
                case "Sum":
                    return this.Sum;
                //case "Approver":
                //    return this.Approver;
                default:
                    return base.GetProperty(name);
            }
        }

        public override void SetProperty(string name, object value)
        {
            switch (name)
            {
                case "BudgetLimit":
                    break;
                case "Sum":
                    break;
                case "Approver":
                    break;
                default:
                    base.SetProperty(name, value);
                    break;
            }
        }
    }
}