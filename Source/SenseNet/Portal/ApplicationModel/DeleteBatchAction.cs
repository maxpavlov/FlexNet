using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Schema;
using SenseNet.Search;

namespace SenseNet.ApplicationModel
{
    public class DeleteBatchAction : ClientAction
    {
        protected virtual string TargetActionName
        {
            get { return "DeleteBatchTarget"; }
        }

        protected virtual string TargetParameterName
        {
            get { return "sourceids"; }
        }

        public override string Callback
        {
            get
            {
                return this.Forbidden ? string.Empty : string.Format("{0};", GetCallBackScript());
            }
            set
            {
                base.Callback = value;
            }
        }

        private string _portletClientId;
        public string PortletClientId
        {
            get 
            { 
                return _portletClientId ?? (_portletClientId = GetPortletClientId());
            }
        }

        protected virtual string GetCallBackScript()
        {
            return string.Format(@"javascript:SN.ListGrid.redirectWithIds('{0}', '{1}', '{2}');",
                PortletClientId, TargetActionName, TargetParameterName);
        }

        protected string GetPortletClientId()
        {
            var parameters = GetParameteres();
            return parameters.ContainsKey("PortletClientID") ? parameters["PortletClientID"].ToString() : string.Empty;
        }
    }
}
