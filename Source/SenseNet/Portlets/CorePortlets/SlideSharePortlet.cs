using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Xml;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository.i18n;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Diagnostics;
using System.Web.UI.HtmlControls;
using System.Text.RegularExpressions;

namespace SenseNet.Portal.Portlets
{
    public class SlideSharePortlet : CacheablePortlet, IWebEditable 
    {
        [WebCategory("SlideSharePortlet", "CategoryTitle", 5), WebOrder(10)]
        [WebBrowsable(true), Personalizable(true)]
        [LocalizedWebDisplayName("SlideSharePortlet", "LayoutStyleTitle"), LocalizedWebDescription("SlideSharePortlet", "LayoutStyleDescription")]
        [TextEditorPartOptions(TextEditorCommonType.MultiLine)]
        public string Html
        {
            get;set;
        }

        public SlideSharePortlet()
        {
            this.Name = SenseNetResourceManager.Current.GetString("SlideSharePortlet", "PortletTitle");
            this.Description = SenseNetResourceManager.Current.GetString("SlideSharePortlet", "PortletDescription");
            this.Category = new PortletCategory(PortletCategoryType.Application);

            this.HiddenProperties.Add("Renderer");
        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            if (String.IsNullOrEmpty(Html))
            {
                writer.Write(SenseNetResourceManager.Current.GetString("SlideSharePortlet", "CodeForHere"));
            }
            else
            {
                Html = Html.Replace("<embed", "<param name=\"wmode\" value=\"transparent\" ></param><embed wmode=\"transparent\"");
                string HtmlRegex = Html;
                try
                {
                    const string pattern = @"\""[\w\d\\?:;_,.\-+%\/=&@]*\""";
                    HtmlRegex=Regex.Replace(HtmlRegex, pattern, "\"\"");
                   
                    var xml = new XmlDocument();
                    xml.LoadXml(HtmlRegex);
                }
                catch (Exception ex)
                {
                    writer.Write(SenseNetResourceManager.Current.GetString("SlideSharePortlet", "WrongCode"));
                    return;
                }
            }
            writer.Write(Html);
        }
    }
}
