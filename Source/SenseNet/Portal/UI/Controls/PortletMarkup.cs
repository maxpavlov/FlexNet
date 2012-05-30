using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.ContentRepository.Storage;
using System.Web.UI;

namespace SenseNet.Portal.UI.Controls
{
    public class PortletMarkup
    {
        /* ===================================================================== Constants */
        private static readonly string MarkupStartFullTag = "<table sncontentintext";
        private static readonly string MarkupStartTag = "<table";
        private static readonly string MarkupEndTag = "</table>";


        /* ===================================================================== Properties */
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public string InnerText { get; set; }
        public string CustomRootPath { get; set; }
        public string Renderer { get; set; }


        /* ===================================================================== Public methods */
        public void AddToControls(ControlCollection controls)
        {
            var portletStart = this.InnerText.IndexOf("<td>") + 4;
            var beginTable = this.InnerText.Substring(0, portletStart);
            var portletEnd = this.InnerText.LastIndexOf("</td>");
            var endTable = this.InnerText.Substring(portletEnd);

            var portlet = TypeHandler.CreateInstance("SenseNet.Portal.Portlets.ContentCollectionPortlet") as ContextBoundPortlet;
            portlet.ID = Guid.NewGuid().ToString();
            portlet.CustomRootPath = this.CustomRootPath;
            portlet.Renderer = this.Renderer;
            portlet.RenderingMode = SenseNet.Portal.UI.PortletFramework.RenderMode.Xslt;
            portlet.BindTarget = BindTarget.CustomRoot;

            controls.Add(new LiteralControl { Text = beginTable });
            controls.Add(portlet);
            controls.Add(new LiteralControl { Text = endTable });
        }
        public static PortletMarkup GetFirstMarkup(string text)
        {
            var snPortletMarkup = new PortletMarkup();

            try
            {
                var startIndex = text.IndexOf(MarkupStartFullTag);
                if (startIndex == -1)
                    return null;

                // find closing 'table' tag, but the enclosed fraction should be well-formed with respect to the 'table' tags
                // (should contain equal number of opening and closing 'table' tags)
                snPortletMarkup.StartIndex = startIndex;
                var endIndex = text.IndexOf(MarkupEndTag, startIndex);
                var innterText = text.Substring(startIndex, endIndex - startIndex + MarkupEndTag.Length);
                while (!IsValidMarkupSelection(innterText))
                {
                    endIndex = text.IndexOf(MarkupEndTag, endIndex + 1);
                    innterText = text.Substring(startIndex, endIndex - startIndex + MarkupEndTag.Length);
                }

                // found valid innertext
                snPortletMarkup.EndIndex = endIndex + MarkupEndTag.Length;
                snPortletMarkup.InnerText = innterText;

                // extract portlet information (resides in the section enclosed by the first 2 quatation marks)
                var sections = innterText.Split(new char[] { '"' });
                var portletInfo = sections[1];

                var infos = portletInfo.Split(new char[] { ';' });
                snPortletMarkup.CustomRootPath = infos[0];
                snPortletMarkup.Renderer = infos[1];
                return snPortletMarkup;
            }
            catch
            {
                // could not extract portlet info from text fragment
                return null;
            }
        }


        /* ===================================================================== Helper methods */
        private static bool IsValidMarkupSelection(string text)
        {
            // get the number of opening and closing 'table' tags : they should be equal in number in a well-formed textfraction
            var noOfStartTags = text.Split(new string[] { MarkupStartTag }, StringSplitOptions.None).Length - 1;
            var noOfEndTags = text.Split(new string[] { MarkupEndTag }, StringSplitOptions.None).Length - 1;
            return noOfStartTags == noOfEndTags;
        }
    }
}
