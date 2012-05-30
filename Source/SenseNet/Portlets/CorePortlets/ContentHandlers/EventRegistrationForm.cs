using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.Portal.Portlets.ContentHandlers
{
    [ContentHandler]
    public class EventRegistrationForm : Form
    {
        //================================================================================= Constructors

        public EventRegistrationForm(Node parent) : this(parent, null) { }
        public EventRegistrationForm(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected EventRegistrationForm(NodeToken nt) : base(nt) { }

        public override object GetProperty(string name)
        {
            switch (name)
            {
                case "Event":
                    return this.Event;
                default:
                    return base.GetProperty(name);
            }
        }


        [RepositoryProperty("Event", RepositoryDataType.Reference)]
        public Node Event
        {
            get { /*return this.GetProperty<Node>("Event");*/ return this.GetReference<Node>("Event"); }
            set { /*this["Event"] = value;*/ this.SetReference("Event", value); }
        }

        public override void SetProperty(string name, object value)
        {
            switch (name)
            {
                case "Event":
                    Event = (Node)value;
                    break;
                default:
                    base.SetProperty(name, value);
                    break;
            }
        }

    }
}
