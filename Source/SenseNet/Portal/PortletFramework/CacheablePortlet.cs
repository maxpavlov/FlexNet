using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository.Storage.Caching.Dependency;
using SenseNet.ContentRepository;
using SenseNet.Portal.Virtualization;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.UI.PortletFramework
{
    public abstract class CacheablePortlet : PortletBase
    {
        public enum CacheableForOption 
        {
            VisitorsOnly = 0,
            Everyone
        }

        private static readonly string CacheTimerStringFormat =
            "Execution time of the {1} portlet was <b>{0:F10}</b> seconds. Cacheable:{2}, CanCache:{3}, IsInCache:{4}";

        // Properties /////////////////////////////////////////////////////////////
        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Portlet is cached")]
        [WebDescription("If set the output of the portlet will be cached. <div class='ui-helper-clearfix sn-dialog-editportlet-warning'><img class='sn-icon sn-icon16 sn-floatleft' src='/Root/Global/images/icons/16/warning.png' /><i>Switching off portlet cache may cause performance issues!</i></div>")]
        [WebCategory(EditorCategory.Cache, EditorCategory.Cache_Order)]
        [WebOrder(10)]
        public bool Cacheable { get; set; }

        [WebBrowsable(true), Personalizable(false)]
        [WebDisplayName("Portlet is cached for")]
        [WebDescription("In case of 'Everyone' the output of the portlet will be cached for logged in users as well. <div class='ui-helper-clearfix sn-dialog-editportlet-warning'><img class='sn-icon sn-icon16 sn-floatleft' src='/Root/Global/images/icons/16/warning.png' /><i>Choose 'Everyone' only if you do not need personalized content!</i></div>")]
        [WebCategory(EditorCategory.Cache, EditorCategory.Cache_Order)]
        [WebOrder(11)]
        public CacheableForOption CacheableFor { get { return _cacheableForLoggedInUser ? CacheableForOption.Everyone : CacheableForOption.VisitorsOnly; } set { _cacheableForLoggedInUser = value == CacheableForOption.Everyone ? true : false; } }

        private bool _cacheableForLoggedInUser;
        [WebBrowsable(false), Personalizable(true)]
        public bool CacheableForLoggedInUser { get { return _cacheableForLoggedInUser; } set { _cacheableForLoggedInUser = value; } }

        private bool _cacheByPath = true;
        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Request path influences caching")]
        [WebDescription("Defines whether the requested content path is included in the cache key. When unchecked portlet output is preserved regardless of the page's current context content or request path. Check it if you want to cache portlet output depending on the requested context content.")]
        [WebCategory(EditorCategory.Cache, EditorCategory.Cache_Order)]
        [WebOrder(15)]
        public bool CacheByPath 
        {
            get { return _cacheByPath; }
            set { _cacheByPath = value; }
        }

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Url query params influence caching")]
        [WebDescription("Defines whether the url query params are also included in the cache key. When unchecked portlet output is preserved regardless of changing url params.")]
        [WebCategory(EditorCategory.Cache, EditorCategory.Cache_Order)]
        [WebOrder(20)]
        public bool CacheByParams { get; set; }

        private double _absoluteExpiration = -1;
        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Absolute expiration")]
        [WebDescription("Given in seconds. The portlet will be refreshed periodically with the given time period. -1 means that the value is defined by 'AbsoluteExpirationSeconds' setting in the web.config.")]
        [WebCategory(EditorCategory.Cache, EditorCategory.Cache_Order)]
        [WebOrder(30)]
        [Editor(typeof(TextEditorPartField), typeof(IEditorPartField))]
        [TextEditorPartOptions(TextEditorCommonType.Small)]
        public double AbsoluteExpiration
        {
            get { return _absoluteExpiration; }
            set { _absoluteExpiration = value; }
        }

        private double _slidingExpirationMinutes = -1;
        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Sliding expiration")]
        [WebDescription("Given in seconds. The portlet is refreshed when it has not been accessed for the given seconds. -1 means that the value is defined by 'SlidingExpirationSeconds' setting in the web.config.")]
        [WebCategory(EditorCategory.Cache, EditorCategory.Cache_Order)]
        [WebOrder(40)]
        [Editor(typeof(TextEditorPartField), typeof(IEditorPartField))]
        [TextEditorPartOptions(TextEditorCommonType.Small)]
        public double SlidingExpirationMinutes
        {
            get { return _slidingExpirationMinutes; }
            set { _slidingExpirationMinutes = value; }
        }

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Custom cache key")]
        [WebDescription("Defines a custom cache key independent of requested path and query params. Useful when the same static output is rendered at various pages. <div class='ui-helper-clearfix sn-dialog-editportlet-warning'><img class='sn-icon sn-icon16 sn-floatleft' src='/Root/Global/images/icons/16/warning.png' /><i>For experts only! Leave empty if unsure.</i></div>")]
        [WebCategory(EditorCategory.Cache, EditorCategory.Cache_Order)]
        [WebOrder(50)]
        public string CustomCacheKey { get; set; }

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Debug")]
        [WebDescription("Check this property to show debug info about portlet cache state at the bottom of the portlet's layout.")]
        [WebCategory(EditorCategory.Cache, EditorCategory.Cache_Order)]
        [WebOrder(100)]
        public bool Debug { get; set; }

        protected string CachedOutput { get; set; }
        protected bool IsCacheRead { get; set; }
        protected bool IsInCache
        {
            get
            {
                return CheckCachedOutput();
            }
        }
        protected bool CanCache
        {
            get
            {
                if (Page == null)
                    return false;

                var wpm = WebPartManager.GetCurrentWebPartManager(Page);
                if (wpm == null)
                    return false;

                // portlets are not cached when page is being edited with webpart editor
                if (wpm.DisplayMode != WebPartManager.BrowseDisplayMode)
                    return false;

                return OutputCache.CanCache(this.CacheableForLoggedInUser);
            }
        }

        // Virtuals ///////////////////////////////////////////////////////////////
        private List<CacheDependency> _dependencies;
        protected virtual List<CacheDependency> Dependencies
        {
            get
            {
                if (_dependencies == null)
                    _dependencies = new List<CacheDependency>();
                return _dependencies;
            }
        }
        protected virtual string GetCacheKey()
        {
            var page = PortalContext.Current.Page;
            var pagePath = page == null ? string.Empty : page.Path;
            return OutputCache.GetCacheKey(this.CustomCacheKey, pagePath, this.ClientID, this.CacheByPath, this.CacheByParams);
        }
        /// <summary>
        /// Add dependencies of the portlet to dependencies list.
        /// </summary>
        public virtual void AddPortletDependency()
        {
            var portletDep = new PortletDependency(ID);
            Dependencies.Add(portletDep);
        }
        public virtual void NotifyCheckin()
        {
            PortletDependency.NotifyChange(ID);
        }

        // Events /////////////////////////////////////////////////////////////////
        protected override void Render(HtmlTextWriter writer)
        {
            using (var traceOperation = Logger.TraceOperation("CacheablePortlet.Render: ", this.Name))
            {
                if (!CanCache || !Cacheable)
                {
                    RenderTimer = false;
                    base.Render(writer);

                    if (ShowExecutionTime)
                        RenderTimerValue(writer, "CacheablePortlet-normal workflow");

                    if (Debug)
                        writer.Write(string.Concat("Portlet info: normal workflow.", "<br />Cache key: ", GetCacheKey()));
                }
                else if (IsInCache) // IsInCache calls the GetCachedOutput
                {
                    //CachedOutput = GetCachedOutput();
                    writer.Write(CachedOutput);
                    if (ShowExecutionTime)
                        RenderTimerValue(writer, "CacheablePortlet-retrieved from cache");
                    if (Debug)
                        writer.Write(string.Concat("Portlet info: fragment has been retrieved from Cache.", "<br />Cache key: ", GetCacheKey()));
                }
                else
                {
                    using (var sw = new StringWriter())
                    {
                        using (var hw = new HtmlTextWriter(sw))
                        {
                            RenderTimer = false;
                            base.Render(hw);
                            CachedOutput = sw.ToString();
                            if (!HasError)
                                InsertOutputIntoCache(CachedOutput);

                            writer.Write(CachedOutput);

                            if (ShowExecutionTime)
                                RenderTimerValue(writer, "CacheablePortlet-output was placed in the cache");
                            if (Debug)
                                writer.Write(string.Concat("Portlet info: fragment has been put into Cache.", "<br />Cache key: ", GetCacheKey()));
                        }
                    }
                }
                traceOperation.IsSuccessful = true;
            }
        }
        protected override void OnInit(EventArgs e)
        {
            using (var traceOperation = Logger.TraceOperation("CacheablePortlet.OnInit", this.Name))
            {
                base.OnInit(e);

                var wpm = WebPartManager.GetCurrentWebPartManager(Page);
                if (wpm != null)
                    wpm.SelectedWebPartChanged += new WebPartEventHandler(wpm_SelectedWebPartChanged);
                traceOperation.IsSuccessful = true;
            }
        }
        protected override void OnPreRender(EventArgs e)
        {
            var wpm = WebPartManager.GetCurrentWebPartManager(Page);
            if (wpm != null && wpm.DisplayMode != WebPartManager.BrowseDisplayMode)
            {
                // hide advanced cache parameters if portlet cache is switched off
                UITools.RegisterStartupScript("hidecacheparameters", "function showcacheparams(show) {$('.sn-editorpart-Cacheable').siblings().css('color',show?'':'#AAA');var cs=$('input,select', $('.sn-editorpart-Cacheable').siblings());show ? cs.removeAttr('disabled') : cs.attr('disabled','disabled');};showcacheparams($('.sn-editorpart-Cacheable input').attr('checked'));$('.sn-editorpart-Cacheable input').live('click', function() { showcacheparams($(this).attr('checked')); });", this.Page);
            }

            base.OnPreRender(e);
        }
        protected void wpm_SelectedWebPartChanged(object sender, WebPartEventArgs e)
        {
            //
            // SelectedWebPartChanged event is called at the end of changing setting of the selected webpart.  
            // Fact: If a page is in EditDisplayMode, SelectedWebPartChanged will be called twice.
            // Interesting: in the second call the WebPartEventArgs.WebPart property is null, in the first call, the property is hold a reference
            // for the selected portlet.
            //
            if (e.WebPart == null)
                NotifyCheckin();
        }

        // Internals //////////////////////////////////////////////////////////////
        private void InsertOutputIntoCache(string output)
        {
            AddPortletDependency();

            var cacheDependency = 
                (this.Dependencies.Count > 1) ? 
                            new AggregateCacheDependency() :
                            ((this.Dependencies.Count > 0) ? this.Dependencies[0] : null);

            if (this.Dependencies.Count > 1)
                foreach (var dep in this.Dependencies)
                    ((AggregateCacheDependency)cacheDependency).Add(dep);

            this._dependencies = null;

            OutputCache.InsertOutputIntoCache(AbsoluteExpiration, SlidingExpirationMinutes, this.GetCacheKey(), output, cacheDependency, CacheItemPriority.Normal);
        }
        private bool CheckCachedOutput()
        {
            // if the output was once read from the cache and contains anything the function returns with true
            // if it is empty or null it returns with false
            if (IsCacheRead)
                return !String.IsNullOrEmpty(CachedOutput);
            // if the output was not read from the cache, we load the cached value
            // if the cached value is empty or null, we return false, otherwise true
            CachedOutput = OutputCache.GetCachedOutput(this.GetCacheKey());
            IsCacheRead = true;
            return !String.IsNullOrEmpty(CachedOutput);
        }

        protected override void RenderTimerValue(HtmlTextWriter writer, string message)
        {
            var sb = new StringBuilder();
            if (IsInCache)
                sb.Append(@"<div style=""color:#fff;background:#060;font-weight:bold,padding:2px"">");
            else
                sb.Append(@"<div style=""color:#fff;background:#c00;font-weight:bold,padding:2px"">");
            var msg = String.Format(CacheTimerStringFormat, Timer.Elapsed.TotalSeconds, ID, Cacheable, CanCache,
                                    IsInCache);
            if (!string.IsNullOrEmpty(message))
                msg = String.Concat(msg, "-", message);
            sb.Append(msg);
            sb.Append(@"</div>");
            writer.Write(sb.ToString());
        }
    }
}
