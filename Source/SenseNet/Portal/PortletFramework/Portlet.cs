//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Web.UI.WebControls.WebParts;
//using System.Diagnostics;
//using System.Web;
//using System.Security.Cryptography;
//using System.Web.Caching;

//using SenseNet.ContentRepository.Storage.Caching.Dependency;
//using SenseNet.ContentRepository;
//using SenseNet.Diagnostics;

//namespace SenseNet.Portal.UI.PortletFramework
//{
//    /// <summary>
//    /// Abstract portlet base class which provides basic functionalities, properties to use your custom portlet in a simplified way.
//    /// Portlet is inherited from WebPart, so it forwards features of the WebPart to your portlets.
//    /// </summary>
//    [Obsolete("Portlet class is obsolote. Use PortletBase or CacheablePortlet instad.", true)]
//    public abstract class Portlet : WebPart
//    {
//        #region cache 

//        private bool fromCache = false;

//        private static readonly string CacheKeyPrefix = "Portlet_";

//        private List<System.Web.Caching.CacheDependency> _dependencies;
//        protected virtual List<System.Web.Caching.CacheDependency> Dependencies
//        {
//            get 
//            {
//                if (_dependencies == null)
//                    _dependencies = new List<System.Web.Caching.CacheDependency>();
//                return _dependencies; 
//            }
//        }

//        private string _cacheKey;
//        /// <summary>
//        /// Gets or sets the cachekey that holds the key, which is used by caching mechanizm to store values in distributed cache.
//        /// </summary>
//        public virtual string CacheKey
//        {
//            get { return _cacheKey; }
//            set { _cacheKey = value; }
//        }

//        /// <summary>
//        /// Gets the value whether the user has been disabled the cache or not.
//        /// Default value is false: cache is in use.
//        /// </summary>
//        private bool DisableCache
//        {
//            get
//            {
//                HttpRequest request = HttpContext.Current.Request;
//                if (request != null && request.Params["DisableCache"] != null)
//                {
//                    string enableCache = request.Params["DisableCache"] as string;
//                    bool enableCacheValue = false;
//                    if (bool.TryParse(enableCache, out enableCacheValue))
//                        return enableCacheValue;
//                }
//                return false;
//            }
//        }

//        private bool IsCacheRead { get; set; }
//        private string CachedOutput { get; set; }
        
//        /// <summary>
//        /// Gets the value which indicates that portlet is in cache.
//        /// Returns true, if portlet contents is in the cache.
//        /// </summary>
//        private bool IsInCache
//        {
//            get
//            {
//                if (IsCacheRead)
//                    return String.IsNullOrEmpty(CachedOutput) && Cacheable;

//                var cacheKey = this.GetCacheKey();
//                CachedOutput = HttpContext.Current.Cache.Get(cacheKey) as string;
//                IsCacheRead = true;
//                return (String.IsNullOrEmpty(CachedOutput)) && Cacheable;
//            }
//        }

//        /// <summary>
//        /// Gets the value, if portlet is allowed to put contents into cache.
//        /// Only returns true, when the page is browsedisplay mode or the page is requested by the visitor user (anonymous user).
//        /// </summary>
//        protected bool CanCache
//        {
//            get
//            {
//                WebPartManager wpm = WebPartManager.GetCurrentWebPartManager(this.Page);
//                if (wpm.DisplayMode == WebPartManager.BrowseDisplayMode && User.Current.Id == User.Visitor.Id && !DisableCache)
//                    return true;
//                return false;
//            }
//        }

//        private bool _cacheByParams;
//        /// <summary>
//        /// Gets or sets whether the portlet cachekey generation can use querystring.
//        /// </summary>
//        [WebBrowsable(true),Personalizable(true)]
//        [WebDisplayName("Cache by params")]
//        [WebDescription("Cache by params")]
//        public bool CacheByParams
//        {
//            get { return _cacheByParams; }
//            set { _cacheByParams = value; }
//        }

//        /// <summary>
//        /// Returns a unique cachekey value.
//        /// This method is virtual, so developers can override and implement their custom cachekey logic.
//        /// </summary>
//        /// <returns>String key.</returns>
//        protected virtual string GetCacheKey()
//        {
//            string key = String.Concat(CacheKeyPrefix, this.UniqueID);
//            if (this.CacheByParams)
//                key = String.Concat(key, HttpContext.Current.Request.Url.ToString());

//            SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider();
//            UnicodeEncoding encoding = new UnicodeEncoding();
//            string hash = Convert.ToBase64String(sha.ComputeHash(encoding.GetBytes(key)));

//            return hash;
//        }

//        #endregion

//        #region properties

//        private Stopwatch _timer;
//        private string _skinPreFix;
//        private string _name;
//        private bool _cacheable;
        


//        [WebBrowsable(true),Personalizable(true)]
//        [WebDisplayName("Skin prefix")]
//        [WebDescription("Skin prefix")]
//        public string SkinPreFix
//        {
//            get { return _skinPreFix; }
//            set { _skinPreFix = value; }
//        }
        
//        /// <summary>
//        /// Gets or sets the Name of the portlet. This only makes the portlets migration of the previous Sense/Net Content Repository easier.
//        /// </summary>
//        [WebBrowsable(false),Personalizable(true)]
//        public string Name
//        {
//            get { return _name; }
//            set { _name = value; }
//        }
//        /// <summary>
//        /// Gets or sets the value which indicates that portlet can use the internal cache functionality.
//        /// </summary>
//        [WebBrowsable(true), Personalizable(true),WebDisplayName("Portlet is cacheable"),WebDescription("If set the output of the portlet will be cached")]
//        public bool Cacheable
//        {
//            get { return _cacheable; }
//            set { _cacheable = value; }
//        }

//        /// <summary>
//        /// Indicates that the portlet can render information about its exection time at the end of html fragment of the portlet.
//        /// </summary>
//        private bool ShowExecutionTime
//        {
//            get
//            {
//                HttpRequest request = HttpContext.Current.Request;
//                if (request != null && request.Params["ShowExecutionTime"] != null)
//                {
//                    string showExecutionTime = request.Params["ShowExecutionTime"] as string;
//                    bool showExecutionTimeValue = false;
//                    if (bool.TryParse(showExecutionTime, out showExecutionTimeValue))
//                        return showExecutionTimeValue;
//                }
//                return false;
//            }
//        }

//        private double _slidingExpirationSeconds;
//        [WebBrowsable(true), Personalizable(true),WebDisplayName("Sliding expiration (minutes)"),WebDescription("If the portlet is not accessed for the given minutes it will be refreshed") ]
//        public double SlidingExpirationMinutes
//        {
//            get { return _slidingExpirationSeconds; }
//            set { _slidingExpirationSeconds = value; }
//        }

//        private double _absoluteExpirationSeconds;
//        [WebBrowsable(true), Personalizable(true), WebDisplayName("Absolute expiration (seconds)"), WebDescription("The portlet will be refreshed with the given time period (in seconds)")]
//        public double AbsoluteExpiration
//        {
//            get { return _absoluteExpirationSeconds; }
//            set { _absoluteExpirationSeconds = value; }
//        }
//        #endregion

//        /// <summary>
//        /// This is a retired method. Don't use this anymore, anywhere, please.
//        /// </summary>
//        [Obsolete("This is a retired method. Don't use this anymore, anywhere, please.",false)]
//        protected virtual void Initialize() { }
		
//        public virtual void PageSave() { }

//        public Portlet()
//        {
//            // start timer if the user has been requested it.
//            if (this.ShowExecutionTime)
//                _timer = new Stopwatch();
//        }

//        protected override void OnInit(EventArgs e)
//        {
//            // start timer if the user has been requested it.
//            if (this.ShowExecutionTime) _timer.Start();
//            base.OnInit(e);
//            if (this.ShowExecutionTime) _timer.Stop();
//        }

//        protected override void OnLoad(EventArgs e)
//        {
//            if (this.ShowExecutionTime) _timer.Start();
//            base.OnLoad(e);
//            if (this.ShowExecutionTime) _timer.Stop();
//        }

//        public virtual void AddPortletDependency()
//        {
//            PortletDependency portletDep = new PortletDependency(this.ID);
//            this.Dependencies.Add(portletDep);
//        }

//        protected override void TrackViewState()
//        {
//            base.TrackViewState();

//            if (!DisableCache)
//                _cacheKey = GetCacheKey();
            
//            // force a CreateChildControls with Initialize: 
//            // practically -> forces main method of all portlets right after viewstate and personalization data has been processed.
//            EnsureChildControls();  
//        }

//        protected override void CreateChildControls()
//        {
//            if (this.ShowExecutionTime) _timer.Start();
//            //if (!IsInCache || !CanCache)
//                base.CreateChildControls();
          
//            if (!IsInCache || !CanCache)
//                Initialize();
//            if (this.ShowExecutionTime) _timer.Stop();
//        }

//        public virtual void NotifyCheckin()
//        {
//            PortletDependency.NotifyChange(this.ID);
//        }


//        protected override void OnPreRender(EventArgs e)
//        {
//            base.OnPreRender(e);

//            // re-initialize (recreate) childcontrols.
//            if (!CanCache)
//            {
//                WebPartManager wpm = WebPartManager.GetCurrentWebPartManager(this.Page);
//                if (wpm.DisplayMode == WebPartManager.EditDisplayMode ||
//                    wpm.DisplayMode == WebPartManager.CatalogDisplayMode)
//                    Initialize();
//            }
//        }

//        protected override void Render(System.Web.UI.HtmlTextWriter writer)
//        {
//            if (this.ShowExecutionTime) _timer.Start();
//            string output = string.Empty;

//            if (this.Cacheable && this.CanCache)
//            {
//                if (!this.IsInCache)
//                {
//                    // converts htmltextwriter contents into string
//                    using (System.IO.StringWriter sw = new System.IO.StringWriter())
//                    {
//                        using (System.Web.UI.HtmlTextWriter htmlWriter = new System.Web.UI.HtmlTextWriter(sw))
//                        {
//                            base.Render(htmlWriter);
//                            output = sw.ToString();
//                        }
//                    }

//                    AddPortletDependency();

//                    CacheDependency cacheDependency = (this.Dependencies.Count > 1 ? new AggregateCacheDependency() : this.Dependencies[0]);

//                    if (this.Dependencies.Count > 1)
//                        foreach (CacheDependency dep in this.Dependencies)
//                            ((AggregateCacheDependency)cacheDependency).Add(dep);

//                    // checks portlet values
//                    TimeSpan sliding = (_slidingExpirationSeconds == 0 ? System.Web.Caching.Cache.NoSlidingExpiration :
//                        TimeSpan.FromSeconds((double)_slidingExpirationSeconds));
//                    DateTime abs = (_absoluteExpirationSeconds == 0 ? System.Web.Caching.Cache.NoAbsoluteExpiration :
//                        DateTime.Now.AddSeconds((double)_absoluteExpirationSeconds));
                    
//                    // gets from web.config
//                    if (_slidingExpirationSeconds == -1)
//                    {
//                        sliding = this.GetSlidingExpirationSeconds() == 0 ? System.Web.Caching.Cache.NoSlidingExpiration :
//                            TimeSpan.FromSeconds(this.GetSlidingExpirationSeconds());
//                        abs = System.Web.Caching.Cache.NoAbsoluteExpiration;
//                    }
//                    else if (_absoluteExpirationSeconds == -1)
//                    {
//                        abs = this.GetAbsoluteExpirationSeconds() == 0 ? System.Web.Caching.Cache.NoAbsoluteExpiration :
//                            DateTime.Now.AddSeconds(this.GetAbsoluteExpirationSeconds());
//                        sliding = System.Web.Caching.Cache.NoSlidingExpiration;
//                    }

//                    DistributedApplication.Cache.Insert(this.GetCacheKey(), output, cacheDependency, abs, sliding, CacheItemPriority.Normal, null);
//                    IsCacheRead = true;
//                    CachedOutput = output;
//                }
//                else
//                {
//                    //output = DistributedApplication.Cache.Get(this.GetCacheKey()) as string;
//                    output = CachedOutput;
//                    fromCache = true;
//                }
//                //
//                // when output is dirty, let's rebuild the controls and call the common rendering method.
//                // no need to put this into cache again because IsInCache returns false in the next cycle, and the output of the portlet will be put into cache again.
//                if (String.IsNullOrEmpty(output))
//                {
//                    try
//                    {
//                        Initialize();
//                        base.Render(writer);                                                
//                    }
//                    catch(Exception exc) //logged
//                    {
//                        Logger.WriteException(exc);
//                        var sr = String.Concat("<!-- ", exc.Message," -->");
//                        writer.Write(sr);
//                    }
//                } else
//                    writer.Write(output);       // sends to user

//            } else 
//                base.Render(writer);        // default rendering
            
//            // stop timer
//            if (this.ShowExecutionTime)
//            {
//                _timer.Stop();
//                RenderTimerValue(writer);
//            } 
//        }
        
//        /// <summary>
//        /// Renders a(n) html fragment which contains the text is displayed in the browser of the end user, if user requests it.
//        /// </summary>
//        /// <param name="writer">HtmlTextWriter stores the content.</param>
//        private void RenderTimerValue(System.Web.UI.HtmlTextWriter writer)
//        {
//            StringBuilder sb = new StringBuilder();
//            if (fromCache)
//                sb.Append(@"<div style=""color:#fff;background:#060;font-weight:bold,padding:2px"">");
//            else
//                sb.Append(@"<div style=""color:#fff;background:#c00;font-weight:bold,padding:2px"">");
//            //this.Cacheable && this.CanCache
//            sb.Append(String.Format("(from cache:{5}) Execution time of the {1} portlet was <b>{0:F10}</b> seconds. Cacheable:{2}, CanCache:{3}, IsInCache:{4}", _timer.Elapsed.TotalSeconds, this.ID, this.Cacheable.ToString(), this.CanCache.ToString(), this.IsInCache.ToString(), this.fromCache.ToString()));
//            sb.Append(@"</div>");
//            writer.Write(sb.ToString());
//        }

//        private double GetAbsoluteExpirationSeconds()
//        {
//            string configValue = System.Configuration.ConfigurationManager.AppSettings["AbsoluteExpirationSeconds"];
//            if (String.IsNullOrEmpty(configValue))
//                return 0;
//            double result = 0;
//            if (double.TryParse(configValue, out result))
//                return result;
//            return result;
//        }

//        private double GetSlidingExpirationSeconds()
//        {
//            string configValue = System.Configuration.ConfigurationManager.AppSettings["SlidingExpirationSeconds"];
//            if (String.IsNullOrEmpty(configValue))
//                return 0;
//            double result = 0;
//            if (double.TryParse(configValue, out result))
//                return result;
//            return result;
//        }

//    }
//}