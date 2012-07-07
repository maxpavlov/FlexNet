using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.Portal.Virtualization;
using System.Threading;
using System.Web;

namespace SenseNet.Portal.UI.Bundling
{
    /// <summary>
    /// Contains information about bundling options.
    /// </summary>
    public class PortalBundleOptions
    {
        /// <summary>
        /// The list of CSS bundles that need to be added to the HTML header.
        /// </summary>
        public List<CssBundle> CssBundles { get; private set; }

        /// <summary>
        /// Gets whether CSS bunding is configured to be enabled or not.
        /// </summary>
        public bool AllowCssBundling
        {
            get
            {
                var settings = ConfigurationManager.GetSection(PortalContext.PortalSectionKey) as NameValueCollection;

                if (settings != null && settings.AllKeys.Contains("AllowCssBundling"))
                {
                    var val = settings["AllowCssBundling"] as string;

                    if (!string.IsNullOrEmpty(val))
                    {
                        return val.ToLower() == "true";
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Gets whether Javascript bundling is configured to be enabled or not.
        /// </summary>
        public bool AllowJsBundling
        {
            get
            {
                var settings = ConfigurationManager.GetSection(PortalContext.PortalSectionKey) as NameValueCollection;

                if (settings != null && settings.AllKeys.Contains("AllowJsBundling"))
                {
                    var val = settings["AllowJsBundling"] as string;

                    if (!string.IsNullOrEmpty(val))
                    {
                        return val.ToLower() == "true";
                    }
                }

                return true;
            }
        }

        private string[] _jsBlacklist;
        public IEnumerable<string> JsBlacklist
        {
            get { return _jsBlacklist ?? (_jsBlacklist = RepositoryConfiguration.GetStringArrayConfigValues(PortalContext.PortalSectionKey, "JsBundlingBlacklist")); }
        }

        private string[] _cssBlacklist;
        public IEnumerable<string> CssBlacklist
        {
            get { return _cssBlacklist ?? (_cssBlacklist = RepositoryConfiguration.GetStringArrayConfigValues(PortalContext.PortalSectionKey, "CssBundlingBlacklist")); }
        }

        /// <summary>
        /// Creates a new instance of PortalBundleOptions
        /// </summary>
        public PortalBundleOptions()
        {
            CssBundles = new List<CssBundle>();
        }

        /// <summary>
        /// Enables CSS bundling behaviour for the given HTML head control.
        /// </summary>
        /// <param name="header">The head control for which bundling needs to be enabled.</param>
        public void EnableCssBundling(Control header)
        {
            // Trick to ensure that this event handler is hooked up once and only once
            header.Page.PreRenderComplete -= OnPreRenderComplete;
            header.Page.PreRenderComplete += OnPreRenderComplete;
        }

        public static bool JsIsBlacklisted(string path)
        {
            return !string.IsNullOrEmpty(path) && PortalContext.Current.BundleOptions.JsBlacklist.Any(path.StartsWith);
        }

        public static bool CssIsBlacklisted(string path)
        {
            return !string.IsNullOrEmpty(path) && PortalContext.Current.BundleOptions.CssBlacklist.Any(path.StartsWith);
        }

        private static void OnPreRenderComplete(object sender, EventArgs args)
        {
            // Add a link tag for every CSS bundle in the current PortalContext

            var header = ((System.Web.UI.Page)sender).Header;

            foreach (var bundle in PortalContext.Current.BundleOptions.CssBundles)
            {
                // Also adding it to the bundle handler
                bundle.Close();
                BundleHandler.AddBundleIfNotThere(bundle);
                ThreadPool.QueueUserWorkItem(x => BundleHandler.AddBundleToCache(bundle));

                if (BundleHandler.IsBundleInCache(bundle))
                {
                    var cssLink = new HtmlLink();

                    cssLink.Href = "/sn-bundles/" + bundle.FakeFilename;
                    cssLink.Attributes["rel"] = "stylesheet";
                    cssLink.Attributes["type"] = bundle.MimeType;
                    cssLink.Attributes["media"] = bundle.Media;

                    header.Controls.Add(cssLink);
                }
                else
                {
                    // The bundle will be complete in a few seconds; disallow caching the page until then
                    header.Page.Response.Cache.SetCacheability(HttpCacheability.NoCache);

                    foreach (var path in bundle.Paths)
                        UITools.AddStyleSheetToHeader(header, path, 0, "stylesheet", bundle.MimeType, bundle.Media, string.Empty, false);
                }

                foreach (var postponedPath in bundle.PostponedPaths)
                {
                    UITools.AddStyleSheetToHeader(header, postponedPath, 0, "stylesheet", bundle.MimeType, bundle.Media, string.Empty, false);
                }
            }
        }
    }
}
