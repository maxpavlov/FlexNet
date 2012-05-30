using System;
using System.Globalization;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI;
using System.Web;
using SenseNet.ContentRepository;
using SenseNet.Portal.PortletFramework;
using SenseNet.Portal.Virtualization;
using SenseNet.Diagnostics;

[assembly: TagPrefix("SenseNet.Portal.UI.PortletFramework", "sn")]
namespace SenseNet.Portal.UI.PortletFramework
{
    /// <summary>
    /// Custom WebPartChrome implementation without table tags. 
    /// This chrome supports 2 default action verbs: Delete and Edit. Of course, you can use your custom action verbs, in your webpart.
    /// </summary>   
    public class SNWebPartChrome : WebPartChrome
    {
        public SNWebPartChrome(WebPartZoneBase zone, WebPartManager manager)
            : base(zone, manager) {}

        public override void PerformPreRender()
        {
            // We override the PerformPreRender method to avoid the original implementation (table styles in html header).
            // In SNWebPartChrome the different chrometypes (border only, title and border, etc.) are generated with css classes.
        }

        public override void RenderWebPart(HtmlTextWriter writer, WebPart webPart)
        {
            var chromeType = this.Zone.GetEffectiveChromeType(webPart);
            AddPortletSkinCss(writer, webPart, chromeType.ToString().ToLower());
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.GetWebPartChromeClientID(webPart));
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            var currentDisplayMode = this.WebPartManager.DisplayMode;

            if (currentDisplayMode == WebPartManager.EditDisplayMode || currentDisplayMode == WebPartManager.DesignDisplayMode)
            {
                RenderVerbs(writer, webPart);
            }

            if (chromeType == PartChromeType.TitleAndBorder || chromeType == PartChromeType.BorderOnly)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-pt-border ui-widget-content ui-corner-all");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
            }

            if ((chromeType == PartChromeType.TitleOnly) || (chromeType == PartChromeType.TitleAndBorder))
                RenderTitleBar(writer, webPart);

            if (chromeType == PartChromeType.TitleAndBorder || chromeType == PartChromeType.BorderOnly)
            {
                RenderPortletBodyBeginTag(writer);
            }
            else
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-pt-body-borderless");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
            }

            RenderPartContents(writer, webPart);

            if (chromeType == PartChromeType.TitleAndBorder || chromeType == PartChromeType.BorderOnly)
            {
                RenderPortletBodyEndTag(writer);
            }
            else
            {
                writer.RenderEndTag(); // sn-pt-body-borderless
            }

            if (chromeType == PartChromeType.TitleAndBorder)
                RenderChromeFooter(writer);

            if (chromeType == PartChromeType.TitleAndBorder || chromeType == PartChromeType.BorderOnly)
                writer.RenderEndTag(); // sn-pt-border

            if (this.WebPartManager.DisplayMode == WebPartManager.DesignDisplayMode)
                RenderMaskTag(writer);

            writer.RenderEndTag();
        }
        protected virtual void RenderChromeFooter(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-pt-footer");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            //"<div class="sn-pt-footer-bl"></div>
            //"<div class="sn-pt-footer-br"></div>
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-pt-footer-bl");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.RenderEndTag();
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-pt-footer-br");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.RenderEndTag();
            writer.RenderEndTag(); //sn-pt-footer
        }
        protected virtual void RenderTitleBar(HtmlTextWriter writer, WebPart webPart)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-pt-header ui-widget-header ui-corner-all ui-helper-clearfix");
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.GetWebPartTitleClientID(webPart));
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            //<div class="sn-pt-header-tl"></div>
            RenderHeaderTagTL(writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-pt-header-center");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            //<div class="sn-pt-icon"></div>
            RenderIconTag(writer);
            RenderTitle(writer, webPart);
            writer.RenderEndTag();//sn-pt-header-center
            RenderHeaderTagTR(writer);
            writer.RenderEndTag();//sn-pt-header
        }

        protected virtual void RenderVerb(HtmlTextWriter writer, WebPart webPart, WebPartVerb verb1)
        {
            if (IsRestricted(verb1))
                return;

            var verbTypeName = verb1.GetType().Name;
            var isEditVerb = verbTypeName.Equals("WebPartEditVerb");
            var isDeleteVerb = verbTypeName.Equals("WebPartDeleteVerb");
            var currentDisplayMode = this.WebPartManager.DisplayMode;

            if (currentDisplayMode != WebPartManager.EditDisplayMode &&
                isEditVerb)
                return;

            if (currentDisplayMode != WebPartManager.EditDisplayMode &&
                currentDisplayMode == WebPartManager.BrowseDisplayMode &&
                isDeleteVerb)
                return;

            var linkClass = "sn-verb ui-corner-all";

            if (isEditVerb)
                linkClass = String.Concat(linkClass, " ", "sn-verb-editportlet");

            if (isDeleteVerb)
                linkClass = String.Concat(linkClass, " ", "sn-verb-delete");
            else
                linkClass = String.Concat(linkClass, " sn-verb-", verb1.ID.ToLower());

            if (isDeleteVerb && webPart.IsStatic)
                return;

            //a href="#" class="sn-verb sn-verb-editportlet">Edit portlet</a>
            //<a href="#" class="sn-verb sn-verb-delete sn-verb-disabled">Delete</a>

            RenderVerbTag(writer, webPart, verb1, linkClass);


        }
        protected virtual void RenderVerbTag(HtmlTextWriter writer, WebPart webPart, WebPartVerb verb1, string linkClass)
        {
            var onclickscript = Zone.Page.ClientScript.GetPostBackEventReference(Zone, string.Format("{0}:{1}", GetVerbId(verb1), webPart.ID));
            //var onclickscript = String.Format("__doPostBack('{0}','{2}:{1}');", Zone.UniqueID.Replace("_", "$"), webPart.ID, GetVerbId(verb1));
            writer.AddAttribute(HtmlTextWriterAttribute.Href, "javascript:;");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, linkClass);
            writer.AddAttribute(HtmlTextWriterAttribute.Title, GetLocalizedVerbText(verb1.Text));

            writer.AddAttribute(HtmlTextWriterAttribute.Onclick, onclickscript);
            writer.RenderBeginTag(HtmlTextWriterTag.A);

            if (String.IsNullOrEmpty(verb1.ImageUrl))
            {
                //writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-hide");
                //writer.RenderBeginTag(HtmlTextWriterTag.Span);
                writer.Write(verb1.Text);
                //writer.RenderEndTag();
            }
            else
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Src, verb1.ImageUrl);
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
            }

            writer.RenderEndTag();
        }

        private string GetLocalizedVerbText(string verbText)
        {
            switch (verbText)
            {
                case "Delete":
                    return HttpContext.GetGlobalResourceObject("Portal", "ChromeDelete") as string;
                case "Edit":
                    return HttpContext.GetGlobalResourceObject("Portal", "ChromeEdit") as string;
                default:
                    return verbText;
            }

        }

        //========================================================================================== Helper methods

        private void RenderMaskTag(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-pt-mask");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.RenderEndTag();
        }
        private void RenderPortletBodyBeginTag(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-pt-body-border ui-widget-content ui-corner-all");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-pt-body ui-corner-all");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
        }
        private void RenderPortletBodyEndTag(HtmlTextWriter writer)
        {
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        private void RenderVerbs(HtmlTextWriter writer, WebPart webPart)
        {
            var verbs = this.GetWebPartVerbs(webPart);

            //writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-verbs");

            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-admin sn-verbs");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-verbs-openbtn ui-corner-all ui-icon ui-icon-triangle-1-s");
            writer.AddAttribute(HtmlTextWriterAttribute.Title, "Open portlet actions");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.Write("Open portlet actions");
            writer.RenderEndTag();
            writer.RenderEndTag();

            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-admin sn-verbs-panel");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "ui-widget ui-widget-content sn-admin-content ui-corner-all ui-helper-clearfix");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            writer.AddAttribute(HtmlTextWriterAttribute.Title, "Close");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-verbs-closebtn ui-icon ui-icon-triangle-1-n");
            //writer.AddAttribute(HtmlTextWriterAttribute.Onclick, "SN.PortalRemoteControl.ClosePortletMenu(event, this);");
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            writer.Write("Close");
            writer.RenderEndTag();

            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-verbs-content");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            foreach (WebPartVerb verb1 in verbs)
                RenderVerb(writer, webPart, verb1);

            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();

        }
        private void AddPortletSkinCss(HtmlTextWriter writer, WebPart webPart, String chromeType)
        {
            var portletSkinName = "sn-portlet sn-{1} {0} sn-chrome-{2} ui-widget";
            var portletBase = webPart as PortletBase;

            if (portletBase != null)
            {
                var skinPrefix = String.Empty;
                var portletTypeName = String.Empty;

                skinPrefix = portletBase.SkinPreFix;
                portletTypeName = portletBase.GetType().Name;

                if (String.IsNullOrEmpty(skinPrefix))
                {
                    portletSkinName = "sn-portlet sn-{0} sn-chrome-{1} ui-widget";
                    portletSkinName = String.Format(portletSkinName, portletTypeName.ToLower(), chromeType);
                }
                else
                    portletSkinName = String.Format(portletSkinName, skinPrefix, portletTypeName.ToLower(), chromeType);
            }

            writer.AddAttribute(HtmlTextWriterAttribute.Class, portletSkinName);
        }

        /// <summary>
        /// Return true, if the verb is not allowed. It's an inner logic, Later, we can change this.
        /// Actually, we don't support the following webpart verbs: minimize, close, help, restore, connect, export
        /// </summary>
        /// <param name="verb1">Verb instance</param>
        /// <returns>true if verb is permited, otherwise false</returns>
        private bool IsRestricted(WebPartVerb verb1)
        {
            var typeName = verb1.GetType().Name;
            return (typeName == "WebPartMinimizeVerb") ||
                   (typeName == "WebPartCloseVerb") ||
                   (typeName == "WebPartHelpVerb") ||
                   (typeName == "WebPartRestoreVerb") ||
                   (typeName == "WebPartConnectVerb") ||
                   (typeName == "WebPartExportVerb");
        }
        /// <summary>
        /// Handles default and custom verbs ids. If a custom verb is used, fw renders its id with 'partVerb:' prefix.
        /// </summary>
        /// <param name="verb">Verb object</param>
        /// <returns>the verb id: part of the clientscript</returns>
        private string GetVerbId(WebPartVerb verb)
        {
            var verbTypeName = verb.GetType().Name;
            switch (verbTypeName)
            {
                case "WebPartEditVerb":
                    return "Edit";
                case "WebPartDeleteVerb":
                    return "Delete";
                default:
                    return String.Concat("partVerb:", verb.ID);
            }

        }

        private void RenderHeaderTagTR(HtmlTextWriter writer)
        {
            //<div class="sn-pt-header-tr"></div>
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-pt-header-tr");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.RenderEndTag();
        }

        protected virtual void RenderTitle(HtmlTextWriter writer, WebPart webPart)
        {
            //<div class="sn-pt-title">{Title}</div>
            string title;
            if (string.IsNullOrEmpty(webPart.TitleUrl))
            {
                title = webPart.Title;
                title = String.IsNullOrEmpty(title) ? "Untitled" : GetTitleByFormat(title.Trim(), webPart);
            }
            else
            {
                title = String.Format("<a title=\"{0}\" href=\"{1}\">{0}</a>", webPart.Title, webPart.TitleUrl);
            }
            RenderTitleTag(writer, title);
        }
        private static string GetTitleByFormat(string title, WebPart wp)
        {
            #region //orig
            //if (string.IsNullOrEmpty(title))
            //    return title;
            //if (PortalContext.Current.ContextNode == null)
            //    return title;

            //var currentContentName = GetCurrentContentOrTypeName(wp);
            //string formattedTitle;
            //try
            //{
            //    var isExpression = title.Contains(SenseNet.ContentRepository.i18n.SenseNetResourceManager.ResourceStartKey);
            //    if (isExpression)
            //    {
            //        var resourceTitle = ContentRepository.i18n.SenseNetResourceManager.Current.GetStringByExpression(title);
            //        formattedTitle = String.Format(CultureInfo.CurrentUICulture, resourceTitle, currentContentName);
            //    }
            //    else 
            //        formattedTitle = String.Format(CultureInfo.CurrentUICulture, title, currentContentName);
            //}
            //catch(Exception e) //logged
            //{
            //    Logger.WriteException(e);
            //    formattedTitle = title;
            //}
            //return formattedTitle;
            #endregion

            if (PortalContext.Current.ContextNode == null)
                return title;

            string formattedTitle;

            try
            {
                var currentContentName = GetCurrentContentOrTypeName(wp);
                var portlet = wp as PortletBase;

                if (title.Contains(SenseNet.ContentRepository.i18n.SenseNetResourceManager.ResourceStartKey))
                    title = ContentRepository.i18n.SenseNetResourceManager.Current.GetStringByExpression(title);

                if (portlet != null)
                    formattedTitle = portlet.FormatTitle(CultureInfo.CurrentUICulture, title, currentContentName);
                else
                    formattedTitle = String.Format(CultureInfo.CurrentUICulture, title, currentContentName);
            }
            catch (Exception e) //logged
            {
                Logger.WriteException(e);
                formattedTitle = title;
            }
            return formattedTitle;

        }
        private static string GetCurrentContentOrTypeName(WebPart wp)
        {
            //var request = HttpContext.Current.Request;
            //if (request != null && request.Params["ContentTypeName"] != null)
            //    return request.Params["ContentTypeName"];

            //var ctProvider = wp as IContentProvider;
            //if (ctProvider == null)
            //    return null;

            //return ctProvider.ContentName ?? ctProvider.ContentTypeName;

            var request = HttpContext.Current.Request;
            if (request != null && request.Params["ContentTypeName"] != null)
            {
                return System.IO.Path.GetFileName(request.Params["ContentTypeName"]);
            }

            var ctProvider = wp as IContentProvider;
            if (ctProvider != null)
            {
                var name = ctProvider.ContentName ?? ctProvider.ContentTypeName;
                if (!String.IsNullOrEmpty(name))
                    return System.IO.Path.GetFileName(name);
            }

            var contextNode = PortalContext.Current.ContextNode;
            var genericContent = contextNode as GenericContent;
            if (genericContent != null)
                return genericContent.DisplayName;

            return contextNode.Name;
        }

        private void RenderTitleTag(HtmlTextWriter writer, string title)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-pt-title");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.Write(title);
            writer.RenderEndTag();
        }
        private void RenderIconTag(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-pt-icon");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.RenderEndTag();
        }
        private void RenderHeaderTagTL(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-pt-header-tl");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.RenderEndTag();
        }

    }
}