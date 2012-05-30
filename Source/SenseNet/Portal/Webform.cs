using SenseNet.ApplicationModel;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.Portal
{
    [ContentHandler]
    public class Webform : Application
    {
        public Webform(Node parent) : this(parent, null) { }
		public Webform(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected Webform(NodeToken nt) : base(nt) { }

        [RepositoryProperty("Binary", RepositoryDataType.Binary)]
        public virtual BinaryData Binary
        {
            get { return this.GetBinary("Binary"); }
            set { this.SetBinary("Binary", value); }
        }

        public override object GetProperty(string name)
        {
            switch (name)
            {
                case "Binary":
                    return this.Binary;
                default:
                    return base.GetProperty(name);
            }
        }

        public override void SetProperty(string name, object value)
        {
            switch (name)
            {
                case "Binary":
                    this.Binary = (BinaryData)value;
                    break;
                default:
                    base.SetProperty(name, value);
                    break;
            }
        }
    }
}
