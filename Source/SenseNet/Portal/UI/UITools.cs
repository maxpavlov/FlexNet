using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.i18n;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Versioning;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.Bundling;
using SenseNet.Portal.UI.Controls;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.Virtualization;

namespace SenseNet.Portal.UI
{
    public static class UITools
    {
        /// <summary>
        /// Generates script block to run commands at client-side.
        /// </summary>
        /// <param name="scriptName">Script unique name in request.</param>
        /// <param name="script">Script will be run. For example: SN.PortalExplorer.addFileUploadCallback('Grid');</param>
        public static void RegisterStartupScript(string scriptName, string script, System.Web.UI.Page page)
        {
            var sb = new StringBuilder();
            string generatedScriptName = "msajax{0}";
            sb.Append("Sys.Application.add_load(");
            sb.Append(String.Format(generatedScriptName, scriptName));
            sb.Append("); ");
            sb.Append(Environment.NewLine);
            sb.Append("function ");
            sb.Append(String.Format(generatedScriptName, scriptName));
            sb.Append("() { ");
            sb.Append(Environment.NewLine);
            sb.Append(script);
            sb.Append(Environment.NewLine);
            sb.Append("Sys.Application.remove_load(");
            sb.Append(String.Format(generatedScriptName, scriptName));
            sb.Append(");");
            sb.Append(Environment.NewLine);
            sb.Append("};");

            if (page == null)
                return;

            ScriptManager currScriptManager = ScriptManager.GetCurrent(page);
            if (currScriptManager == null)
                return;

            ScriptManager.RegisterStartupScript(
                page,
                typeof (System.Web.UI.Page),
                String.Concat(String.Format(generatedScriptName, scriptName), "_callback"),
                sb.ToString(),
                true
                );
        }

        public static T FindFirstContainerOfType<T>(Control source) where T:Control
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var control = source as T;
            if (control != null)
                return control;

            return FindFirstContainerOfType<T>(source.Parent);
        }
        public static ContextInfo FindContextInfo(Control source, string controlId)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (string.IsNullOrEmpty(controlId))
                return null;

            Control nc = source;
            Control control = null;

            while (control == null && nc != null)
            {
                nc = nc.NamingContainer;

                if (nc != null)
                    control = nc.FindControl(controlId);
            }

            return control as ContextInfo;
        }

        [Obsolete("Use UITools.AddScript instead")]
        public static void AddScriptWithHttpContext(string scriptPath)
        {
            AddScript(scriptPath);
        }

        public static void AddScript(string scriptPath)
        {
            // use snscriptmanager's smartloader if present
            var smartLoader = SNScriptLoader.Current(GetPage());
            if (smartLoader != null)
                smartLoader.AddScript(scriptPath);
            else
            {
                // fallback to asp scriptmanager
                ScriptManager currScriptManager = GetScriptManager();
                if (currScriptManager == null) return;
                var scriptReference = new ScriptReference { Path = scriptPath, NotifyScriptLoaded = true };
                currScriptManager.Scripts.Add(scriptReference);
            }
        }

        /// <summary>
        /// Adds a CSS link to the given header
        /// </summary>
        /// <param name="header">Page header</param>
        /// <param name="cssPath">Path of CSS file</param>
        public static void AddStyleSheetToHeader(Control header, string cssPath)
        {
            AddStyleSheetToHeader(header, cssPath, 0);
        }

        /// <summary>
        /// Adds a CSS link to the given header using the given order. If a link with the given order already exists new link is added right after.
        /// </summary>
        /// <param name="header">Page header</param>
        /// <param name="cssPath">Path of CSS file</param>
        /// <param name="order">Desired order of CSS link</param>
        public static void AddStyleSheetToHeader(Control header, string cssPath, int order)
        {
            AddStyleSheetToHeader(header, cssPath, order, "stylesheet", "text/css", "all", string.Empty);
        }

        /// <summary>
        /// Adds a CSS link to the given header using the given order and parameters. If a link with the given order already exists new link is added right after.
        /// </summary>
        /// <param name="header">Page header</param>
        /// <param name="cssPath">Path of CSS file</param>
        /// <param name="order">Desired order of CSS link</param>
        public static void AddStyleSheetToHeader(Control header, string cssPath, int order, string rel, string type, string media, string title, bool allowBundlingIfEnabled = true)
        {
            if (header == null)
                return;

            if (string.IsNullOrEmpty(cssPath))
                return;

            var resolvedPath = SkinManager.Resolve(cssPath);

            if (allowBundlingIfEnabled && rel == "stylesheet" && type == "text/css" && PortalContext.Current.BundleOptions.AllowCssBundling)
            {
                if (!string.IsNullOrEmpty(title))
                    throw new Exception("The title attribute on link tags is not supported when CSS bundling is enabled.");

                PortalContext.Current.BundleOptions.EnableCssBundling(header);

                // If this is CSS stylesheet and bundling is enabled, add it to the bundle

                // Find the bundle object for the current media
                var bundle = PortalContext.Current.BundleOptions.CssBundles.SingleOrDefault(x => x.Media == media);

                if (bundle == null)
                {
                    bundle = new CssBundle()
                    {
                        Media = media,
                    };
                    PortalContext.Current.BundleOptions.CssBundles.Add(bundle);
                }

                // Add the current resolved path to the bundle
                if (PortalBundleOptions.CssIsBlacklisted(resolvedPath))
                    bundle.AddPostponedPath(resolvedPath);
                else
                    bundle.AddPath(resolvedPath, order);
            }
            else
            {
                // If bundling is disabled, fallback to the old behaviour

                var cssLink = new HtmlLink();
                cssLink.ID = "cssLink_" + resolvedPath.GetHashCode().ToString();

                // link already added to header
                if (header.FindControl(cssLink.ID) != null)
                    return;

                cssLink.Href = resolvedPath;
                cssLink.Attributes["rel"] = rel;
                cssLink.Attributes["type"] = type;
                cssLink.Attributes["media"] = media;
                cssLink.Attributes["title"] = title;
                cssLink.Attributes["cssorder"] = order.ToString();

                // find next control with higher order
                var index = -1;
                bool found = false;
                foreach (Control control in header.Controls)
                {
                    index++;

                    var link = control as HtmlLink;
                    if (link == null)
                        continue;

                    var orderStr = link.Attributes["cssorder"];
                    if (string.IsNullOrEmpty(orderStr))
                        continue;

                    int linkOrder = Int32.MinValue;
                    if (Int32.TryParse(orderStr, out linkOrder) && linkOrder > order)
                    {
                        found = true;
                        break;
                    }
                }
                if (found)
                {
                    // add link right before higher order link
                    header.Controls.AddAt(index, cssLink);
                }
                else
                {
                    // add link at end of header's controlcollection
                    header.Controls.Add(cssLink);
                }
            }
        }

        public static System.Web.UI.Page GetPage()
        {
            HttpContext currHttpCtx = HttpContext.Current;
            if (currHttpCtx == null) return null;
            IHttpHandler currentHandler = currHttpCtx.CurrentHandler;
            return currentHandler as System.Web.UI.Page;
        }

        public static string GetPageModeClass()
        {
            return GetPageModeClass(null);
        }

        public static string GetPageModeClass(string prefix)
        {
            var page = GetPage();
            if (page != null)
            {
                try
                {
                    var wpm = WebPartManager.GetCurrentWebPartManager(page);
                    if (wpm != null)
                        return (string.IsNullOrEmpty(prefix) ? "sn-viewmode-" : prefix) + wpm.DisplayMode.Name.ToLower();
                }
                catch (Exception ex)
                {
                    Logger.WriteException(ex);
                }
            }

            return string.Empty;
        }

        public static Control GetHeader()
        {
            System.Web.UI.Page currentPage = GetPage();
            return currentPage == null ? null : currentPage.Header;
        }

        public static ScriptManager GetScriptManager()
        {
            var currentPage = GetPage();
            return currentPage == null ? null : ScriptManager.GetCurrent(currentPage);
        }

        public static SNScriptManager GetSNScriptManager()
        {
            var currentPage = GetPage();
            return SNScriptManager.GetCurrent(currentPage) as SNScriptManager;
        }

        public static void AddPickerCss()
        {
            var header = GetHeader();
            AddStyleSheetToHeader(header, ClientScriptConfigurations.jQueryCustomUICssPath);
            AddStyleSheetToHeader(header, ClientScriptConfigurations.jQueryGridCSSPath);
            AddStyleSheetToHeader(header, ClientScriptConfigurations.IconsCssPath);
            AddStyleSheetToHeader(header, ClientScriptConfigurations.jQueryTreeThemePath);
            AddStyleSheetToHeader(header, ClientScriptConfigurations.jQueryUIWidgetCSSPath);
            AddStyleSheetToHeader(header, ClientScriptConfigurations.SNWidgetsCss, 100);
        }

        public static string GetAvatarUrl()
        {
            return GetAvatarUrl(User.Current as User);
        }

        public static string GetAvatarUrl(Node node)
        {
            var group = node as Group;
            var user = node as User;

            if (group != null)
            {
                return SkinManager.Resolve("$skin/images/default_groupavatar.png");
            }
            if (user != null)
            {
                var avatarUrl = user.AvatarUrl;
                return string.IsNullOrEmpty(avatarUrl) ? SkinManager.Resolve("$skin/images/default_avatar.png") : avatarUrl;
            }
            return string.Empty;
        }

        #region Nested type: ClientScriptConfigurations

        public static class ClientScriptConfigurations
        {
            public static string MSAjaxPath = GetScriptSetting("MSAjaxPath");
            public static string SNWebdavPath = GetScriptSetting("SNWebdavPath");
            public static string SNReferenceGridPath = GetScriptSetting("SNReferenceGridPath");
            public static string SNBinaryFieldControlPath = GetScriptSetting("SNBinaryFieldControlPath");
            public static string SNUtilsPath = GetScriptSetting("SNUtilsPath");
            public static string SNPickerPath = GetScriptSetting("SNPickerPath");
            public static string SNWallPath = "$skin/scripts/sn/SN.Wall.js";
            public static string SNPortalRemoteControlPath = GetScriptSetting("SNPortalRemoteControlPath");
            public static string SNListGridPath = GetScriptSetting("SNListGridPath");
            public static string TinyMCEPath = GetScriptSetting("TinyMCEPath");
            public static string jQueryPath = GetScriptSetting("jQueryPath");
            public static string JQueryUIPath = GetScriptSetting("jQueryUIPath");
            public static string JQueryUIFolderPath = RepositoryPath.GetParentPath(JQueryUIPath);
            public static string jQueryTreePath = GetScriptSetting("jQueryTreePath");
            public static string jQueryGridPath = GetScriptSetting("jQueryGridPath");
            public static string jQueryTreeCheckboxPluginPath = GetScriptSetting("jQueryTreeCheckboxPluginPath");
            public static string SwfUploadPath = GetScriptSetting("SwfUploadScriptPath");
            public static string SwfObjectPath = GetScriptSetting("SwfObjectPath");

            // themes
            public static string IconsCssPath = GetScriptSetting("IconsCssPath");
            public static string jQueryCustomUICssPath = GetScriptSetting("jQueryCustomUICssPath");
            public static string jQueryTreeThemePath = GetScriptSetting("jQueryTreeThemePath");
            public static string jQueryGridCSSPath = GetScriptSetting("jQueryGridCSSPath");
            public static string SNWidgetsCss = GetScriptSetting("SNWidgetsCss");
            public static string jQueryUIWidgetCSSPath = GetScriptSetting("jQueryUIWidgetCSSPath");
        }

        #endregion

        #region configuration settings

        public static string ScriptMode
        {
            get 
            {
                var configName = "ScriptMode";
                var result = GetScriptSetting(configName);
                if (result != "Debug" && result != "Release")
                    throw new ConfigurationErrorsException(
                        string.Format(
                            "The {1} property has been set to '{0}' in the appSettings section, which is invalid. The valid values are 'Release' and 'Debug'.",
                            result, configName));
                return result;
            }
        }

        private static string GetScriptSetting(string configName)
        {
            string result = ConfigurationManager.AppSettings[configName];
            if (result == null)
                throw new ConfigurationErrorsException(
                    string.Format(
                        "The {1} property is not given in the appSettings section.",
                        result, configName));
            return result;
        }

        #endregion
    
        public static class ControlChars
        {
            public const char Back = '\b';
            public const char Cr = '\r';
            public const string CrLf = "\r\n";
            public const char FormFeed = '\f';
            public const char Lf = '\n';
            public const string NewLine = "\r\n";
            public const char NullChar = '\0';
            public const char Quote = '"';
            public const char Tab = '\t';
            public const char VerticalTab = '\v'; 
        }

        public static string GetVersionText(GenericContent node)
        {
            if (node == null)
                return string.Empty;

            var result = string.Empty;

            switch (node.Version.Status)
            {
                case VersionStatus.Approved:
                    result = HttpContext.GetGlobalResourceObject("PortalRemoteControl", "Public") as string;
                    break;
                case VersionStatus.Draft:
                    result = HttpContext.GetGlobalResourceObject("PortalRemoteControl", "Draft") as string;
                    break;
                case VersionStatus.Locked:
                    // TODO: snippet comes from the old prc
                    result =
                        string.Format(
                            HttpContext.GetGlobalResourceObject("PortalRemoteControl", "CheckedOutBy") as string,
                            node.Lock.LockedBy.Name);
                    break;
                case VersionStatus.Pending:
                    result = HttpContext.GetGlobalResourceObject("PortalRemoteControl", "Approving") as string;
                    break;
                case VersionStatus.Rejected:
                    result = HttpContext.GetGlobalResourceObject("PortalRemoteControl", "Reject") as string;
                    break;
                default:
                    break;

            }
            return node.VersioningMode == VersioningType.None ? result : string.Concat(node.Version.VersionString, " ", result);
            
        }

        public static string GetVersioningModeText(GenericContent node)
        {
            if (node == null)
                return string.Empty;

            var modeString = HttpContext.GetGlobalResourceObject("Portal", node.VersioningMode.ToString()) as string;
            
            return string.IsNullOrEmpty(modeString) ? node.VersioningMode.ToString() : modeString;
        }

        /// <summary>
        /// Gets the user friendly string representation of a date relative to the current time
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string GetFriendlyDate(DateTime date)
        {
            //- 53 seconds ago
            //- 15 minutes ago
            //- 21 hours ago
            //- Yesterday at 3:43pm
            //- Sunday at 2:12pm
            //- May 25 at 1:23pm
            //- December 27, 2010 at 5:41pm

            var shortTime = date.ToShortTimeString();   // 5:41 PM

            string secondText = SenseNetResourceManager.Current.GetString("Portal", "SecondMessage");
            string minuteText = SenseNetResourceManager.Current.GetString("Portal", "MinutesMessage");
            string hoursext = SenseNetResourceManager.Current.GetString("Portal", "HoursMessage");
            string yesterdayAt = SenseNetResourceManager.Current.GetString("Portal", "YesterdayAt");
            string atString = SenseNetResourceManager.Current.GetString("Portal", "At");

            var ago = DateTime.Now - date;
            if (ago < new TimeSpan(0, 1, 0))
                return ago.Seconds == 1 ?
                    "1 " + secondText :
                    string.Format("{0} " + secondText, ago.Seconds);
            if (ago < new TimeSpan(1, 0, 0))
                return ago.Minutes == 1 ?
                    "1 " + minuteText :
                    string.Format("{0} " + minuteText, ago.Minutes);
            if (ago < new TimeSpan(1, 0, 0, 0))
                return ago.Hours == 1 ?
                    "1 " + hoursext :
                    string.Format("{0} " + hoursext, ago.Hours);
            if (ago < new TimeSpan(2, 0, 0, 0))
                return string.Format(yesterdayAt + " {0}", shortTime);
            if (ago < new TimeSpan(7, 0, 0, 0))
                return string.Format("{0}" + atString + " {1}", date.DayOfWeek.ToString(), shortTime);
            if (date.Year == DateTime.Now.Year)
                return string.Format("{0} {1}" + atString + " {2}", date.ToString("MMMM"), date.Day, shortTime);

            return string.Format("{0} {1}, {2}" + atString + " {3}", date.ToString("MMMM"), date.Day, date.Year, shortTime);
        }

        public static string GetFriendlyDate(Content content, string fieldName)
        {
            if (content == null || string.IsNullOrEmpty(fieldName))
                return string.Empty;

            var dt = string.Empty;

            try
            {
                return UITools.GetFriendlyDate(Convert.ToDateTime(content[fieldName]));
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
            }

            return dt;
        }
    }
}
