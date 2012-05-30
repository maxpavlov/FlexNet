using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using SenseNet.ContentRepository.Storage;
using SenseNet.Diagnostics;
using SenseNet.Utilities;
using SenseNet.ContentRepository;
using SenseNet.Portal.Virtualization;
using SenseNet.ApplicationModel;

namespace SenseNet.Portal.Helpers
{
    public class Actions
    {
        public static string BrowseUrl(Content content)
        {
            return BrowseUrl(content, null as bool?);
        }

        public static string BrowseUrl(Content content, bool? includeBackUrl)
        {
            return ActionUrl(content, "Browse", includeBackUrl);
        }

        public static string BrowseUrl(Content c, string referenceFieldName)
        {
            return BrowseUrl(c, referenceFieldName, null);
        }

        public static string BrowseUrl(Content c, string referenceFieldName, bool? includeBackUrl)
        {
            if (c == null || string.IsNullOrEmpty(referenceFieldName) || !c.Fields.ContainsKey(referenceFieldName))
                return string.Empty;

            try
            {
                var refNodes = c[referenceFieldName] as IEnumerable<Node>;
                if (refNodes != null)
                {
                    var node = refNodes.FirstOrDefault();
                    if (node != null)
                        return BrowseUrl(Content.Create(node), includeBackUrl);
                }
            }
            catch (ApplicationException ex)
            {
                //property not found
                Logger.WriteException(ex);
            }
            catch (InvalidOperationException ex)
            {
                //property not found
                Logger.WriteException(ex);
            }
            catch(KeyNotFoundException ex)
            {
                //field not found
                Logger.WriteException(ex);
            }

            return string.Empty;
        }

        public static string BrowseAction(string contentPath)
        {
            return BrowseAction(contentPath, null);
        }

        public static string BrowseAction(string contentPath, bool? includeBackUrl)
        {
            if (string.IsNullOrEmpty(contentPath))
                return string.Empty;

            var c = Content.Load(contentPath);
            return c == null ? string.Empty : BrowseAction(c, includeBackUrl);
        }

        public static string BrowseAction(Content c)
        {
            return BrowseAction(c, null as bool?);
        }

        public static string BrowseAction(Content c, bool? includeBackUrl)
        {
            if (c == null)
                return string.Empty;

            var ba = c.Actions.FirstOrDefault(a => a.Name == "Browse");
            if (ba == null) return string.Empty;

            if (includeBackUrl.HasValue)
                ba.IncludeBackUrl = includeBackUrl.Value;

            return "<a href='" + ba.Uri + "'>" + c.DisplayName + "</a>";

        }

        public static string BrowseAction(Content c, string referenceFieldName)
        {
            return BrowseAction(c, referenceFieldName, null);
        }

        public static string BrowseAction(Content c, string referenceFieldName, bool? includeBackUrl)
        {
            if (c == null || string.IsNullOrEmpty(referenceFieldName) || !c.Fields.ContainsKey(referenceFieldName))
                return string.Empty;

            try
            {
                var refNodes = c[referenceFieldName] as IEnumerable<Node>;
                if (refNodes != null)
                {
                    var node = refNodes.FirstOrDefault();
                    if (node != null)
                        return BrowseAction(Content.Create(node), includeBackUrl);
                }
            }
            catch (ApplicationException ex)
            {
                //property not found
                Logger.WriteException(ex);
            }
            catch (InvalidOperationException ex)
            {
                //property not found
                Logger.WriteException(ex);
            }
            catch (KeyNotFoundException ex)
            {
                //field not found
                Logger.WriteException(ex);
            }

            return string.Empty;
        }

        public static string Action(Content content, string actionName)
        {
            return Action(content, actionName, null);
        }

        public static string Action(Content content, string actionName, bool? includeBackUrl)
        {
            if (content == null || string.IsNullOrEmpty(actionName))
                return string.Empty;

            var action = ActionFramework.GetAction(actionName, content, null);
            if (action == null)
                return string.Empty;

            if (includeBackUrl.HasValue)
                action.IncludeBackUrl = includeBackUrl.Value;

            return "<a href='" + action.Uri + "'>" + content.DisplayName + "</a>";

        }

        public static string ActionUrl(Content content, string actionName)
        {
            return ActionUrl(content, actionName, null);
        }

        public static string ActionUrl(Content content, string actionName, bool? includeBackUrl)
        {
            return ActionUrl(content, actionName, includeBackUrl, null);
        }

        public static string ActionUrl(Content content, string actionName, bool? includeBackUrl, object parameters)
        {
            if (content == null || string.IsNullOrEmpty(actionName))
                return string.Empty;

            var action = ActionFramework.GetAction(actionName, content, parameters);
            if (action == null)
                return string.Empty;

            if (includeBackUrl.HasValue)
                action.IncludeBackUrl = includeBackUrl.Value;

            return action.Uri;
        }

        public static string ActionMenu(string contentPath, string innerContent, string scenario)
        {
            if (string.IsNullOrEmpty(contentPath))
                return string.Empty;

            var id = Guid.NewGuid().ToString();
            var json = "\"ItemHoverCssClass\":null,\"Mode\":\"default\",\"ServiceUrl\":\"/SmartAppHelper.mvc/GetActions?path={0}&scenario={1}&back={2}&parameters=\",\"WrapperCssClass\":\"sn-actionmenu sn-action-default-mode\"".FillTemplate(HttpUtility.UrlEncode(contentPath), scenario,
                 HttpUtility.UrlEncode(PortalContext.Current.RequestedUri.ToString()));
            var script = "$create(SenseNet.Portal.UI.Controls.ActionMenu, { " + json + " }, null, null, $get(\"" + id + "\"));";
            ////
            //var res = string.Format("<span id='{0}' class='snActionMenuTag'>{1}</span><script>{2}</script>",id, innerContent, script);

            var element = @"<span id='{0}' 
                    class='sn-actionmenu sn-actionmenu-default-mode ui-widget'>
                    <span class='sn-actionmenu-inner ui-state-default ui-corner-all'>{1}</span></span>".FillTemplate(id, innerContent);

            return element + "<script>"+ script  + "</script>";
        }
        
    }
}
