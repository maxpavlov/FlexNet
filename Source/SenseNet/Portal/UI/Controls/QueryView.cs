using System;
using System.Collections.Generic;
using System.Web.UI;
using SenseNet.ContentRepository.Storage;
using SN = SenseNet.ContentRepository;

namespace SenseNet.Portal.UI.Controls
{
    [ParseChildren(true)]
    [ToolboxData("<{0}:QueryView runat=server />")]
    public class QueryView : UserControl
    {
        private List<Node> _nodeItemList;
        private List<PagerControl> _pagerControls;

        public List<Node> NodeItemList
        {
            get { return _nodeItemList; }
            set { _nodeItemList = value; }
        }

        public List<PagerControl> PagerControls
        {
            get
            {
                if (_pagerControls == null)
                {
                    _pagerControls = new List<PagerControl>();

                    foreach (Control control in this.Controls)
                    {
                        var pc = control as PagerControl;

                        if (pc != null)
                            _pagerControls.Add(pc);
                        else
                            _pagerControls.AddRange(GetPagerControls(control));
                    }
                }

                return _pagerControls;
            }
        }

        private static List<PagerControl> GetPagerControls(Control control)
        {
            var pagers = new List<PagerControl>();

            if (control != null)
            {
                foreach (Control ccc in control.Controls)
                {
                    var pc = ccc as PagerControl;

                    if (pc != null)
                    {
                        pagers.Add(pc);
                        continue;
                    }

                    pagers.AddRange(GetPagerControls(ccc));
                }
            }

            return pagers;
        }

        [Obsolete("This method is obsolete. Please use  the GetValue method instead.", false)]
        public string GetProperty(object node, string propertyName)
        {
            return (GetValue(node, propertyName));
        }

        protected string GetValue(object node, string propertyName)
        {
            if (node is Node)
            {
                var content = SN.Content.Load(((Node)node).Id);
                return (ContentView.GetValue(propertyName, content));
            }
            return string.Concat(node, " is not type of Node");
        }

        public IEnumerable<Node> GetReference(object node, string propertyName)
        {
            var n = node as Node;
            if (n == null)
                return null;
            if (!n.HasProperty(propertyName))
                return null;
            return n[propertyName] as IEnumerable<Node>;
        }

        public object GetObject(object node, string propertyName)
        {
            var n = node as Node;
            if (n == null)
                return String.Concat(node, " is not type of Node");
            if (!n.HasProperty(propertyName))
                return String.Concat(propertyName, " not found in ", n.Name);
            return n[propertyName];
        }

        public int GetIndex(object node)
        {
            var n = node as Node;
            if (n == null)
                return -1;
            return _nodeItemList.IndexOf(n);
        }

        public string PropertySet(object node, string propertyName)
        {
            var set = !string.IsNullOrEmpty(GetValue(node, propertyName));
            return set.ToString();
        }

    }
}