using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.UI;
using SenseNet.Portal.UI.Bundling;
using SenseNet.Portal.Virtualization;
using System.Web;

namespace SenseNet.Portal.UI.PortletFramework
{
    public class SNScriptManager : System.Web.UI.AjaxScriptManager
    {
        private SNScriptLoader _smartLoader;
        public SNScriptLoader SmartLoader
        {
            get { return _smartLoader; }
        }

        public SNScriptManager()
        {
            _smartLoader = new SNScriptLoader();
        }

        protected override void OnInit(EventArgs e)
        {
            this.Page.PreRenderComplete += new EventHandler(Page_PreRenderComplete);
            base.OnInit(e);
        }

        private void RenderScriptReferences()
        {
            var smartList = SmartLoader.GetScriptsToLoad();
            var allowJsBundling = PortalContext.Current.BundleOptions.AllowJsBundling;
            JsBundle bundle = null;
            List<string> postponedList = null;
            if (allowJsBundling)
            {
                bundle = new JsBundle();
                postponedList = new List<string>();
            }

            foreach (var spath in smartList)
            {
                var lower = spath.ToLower();

                if (lower.EndsWith(".css"))
                {
                    UITools.AddStyleSheetToHeader(UITools.GetHeader(), spath);
                }
                else
                {
                    if (allowJsBundling)
                    {
                        // If bundling is allowed, add the path to the bundle - if it is not blacklisted
                        if (PortalBundleOptions.JsIsBlacklisted(spath))
                            postponedList.Add(spath);
                        else
                            bundle.AddPath(spath);
                    }
                    else
                    {
                        // If bundling is disabled, fall back to the old behaviour
                        var sref = new ScriptReference(spath);
                        Scripts.Add(sref);
                    }
                }
            }

            if (allowJsBundling)
            {
                // If bundling is allowed, closing the bundle and adding it as a single script reference
                bundle.Close();
                BundleHandler.AddBundleIfNotThere(bundle);
                ThreadPool.QueueUserWorkItem(x => BundleHandler.AddBundleToCache(bundle));

                if (BundleHandler.IsBundleInCache(bundle))
                {
                    // If the bundle is complete, add it as a single script reference
                    var sref = new ScriptReference("/sn-bundles/" + bundle.FakeFilename);
                    Scripts.Add(sref);
                }
                else
                {
                    // The bundle will be complete in a few seconds; disallow caching the page until then
                    this.Page.Response.Cache.SetCacheability(HttpCacheability.NoCache);

                    // Fallback to adding every script again as separate script references
                    foreach (var path in bundle.Paths)
                    {
                        var sref = new ScriptReference(path);
                        Scripts.Add(sref);
                    }
                }

                //add blacklisted js path's to the script collection individually
                foreach (var path in postponedList)
                {
                    Scripts.Add(new ScriptReference(path));
                }
            }
        }

        protected void Page_PreRenderComplete(object sender, EventArgs e)
        {
            this.RenderScriptReferences();
        }
    }
}