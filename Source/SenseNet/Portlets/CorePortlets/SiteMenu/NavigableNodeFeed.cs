using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Xml;
using System.Xml.Serialization;

namespace SenseNet.Portal.Portlets
{
    [XmlRoot("NodeFeed")]
    public class NavigableNodeFeed
    {
        [XmlArrayItem(ElementName="Node")]
        public NavigableTreeNode[] Nodes;

        public String PortletCssClass { get; set; }
        public void Render(HtmlTextWriter writer)
        {
            if (Nodes != null && Nodes.Length > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, PortletCssClass);
                writer.RenderBeginTag(HtmlTextWriterTag.Ul);
                foreach (NavigableTreeNode node in Nodes)
                {
                    node.Render(writer);
                }
                writer.RenderEndTag();
            }
        }
    }
}
