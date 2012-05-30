namespace SenseNet.ApplicationModel
{
    public class ClientAction : PortalAction
    {
        public virtual string MethodName { get; set; }

        public virtual string ParameterList { get; set; }

        public override string Uri
        {
            get
            {
                return this.Forbidden ? string.Empty : Callback;
            }
        }

        private string _callback;
        public virtual string Callback
        {
            get
            {
                
                if (this.Forbidden)
                    return string.Empty;

                return _callback ?? string.Format("javascript:{0}({1});return false;", MethodName, ParameterList);
            }
            set
            {
                _callback = value;
            }
        }
    }
}
