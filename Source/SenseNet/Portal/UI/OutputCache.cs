using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.Portal.Virtualization;
using System.Web;
using System.Security.Cryptography;
using SenseNet.ContentRepository;
using System.Web.Caching;

namespace SenseNet.Portal.UI
{
    /// <summary>
    /// This class encloses functionality related to output caching (cacheableportlet, xsltapplication, ...)
    /// </summary>
    public class OutputCache
    {
        // ===================================================================================================== Consts
        private const string CacheKeyPrefix = "OutputCache_";
        private static readonly string DisableCacheParam = "DisableCache";


        // ===================================================================================================== Public static methods
        public static string GetCacheKey(string customCacheKey, string appNodePath, string portletClientId, bool cacheByPath, bool cacheByParams)
        {
            string key = string.Empty;
            if (!string.IsNullOrEmpty(customCacheKey))
            {
                // if custom cache key is explicitely given, it overrides any other logic, and cache key is explicitely set
                key = string.Concat(CacheKeyPrefix, customCacheKey);
            }
            else
            {
                // by default cache key consists of current application page path and portlet clientid
                key = String.Concat(CacheKeyPrefix, appNodePath, portletClientId);

                if (cacheByPath)
                {
                    // if cache by path is true, absoluteuri is also added to cache key
                    // added means: same content is requested, but presented with different application page the output will be cached independently
                    var absoluteUri = PortalContext.Current.RequestedUri.AbsolutePath;
                    key = String.Concat(key, absoluteUri);
                }

                if (cacheByParams)
                {
                    // if cachebyparams is true, url query params are also added to cache key
                    // added means: same parameters used, but different application page is requested the output will be cached independently
                    var queryPart = HttpContext.Current.Request.Url.GetComponents(UriComponents.Query, UriFormat.Unescaped);
                    key = String.Concat(key, queryPart);
                }
            }

            var sha = new SHA1CryptoServiceProvider();
            var encoding = new UnicodeEncoding();
            return Convert.ToBase64String(sha.ComputeHash(encoding.GetBytes(key)));
        }
        public static bool DisableCache()
        {
            var request = HttpContext.Current.Request;
            if (request != null && request.Params[DisableCacheParam] != null)
            {
                var enableCache = request.Params[DisableCacheParam] as string;
                var enableCacheValue = false;
                return bool.TryParse(enableCache, out enableCacheValue) && enableCacheValue;
            }
            return false;
        }
        public static bool CanCache(bool cacheableForLoggedInUser)
        {
            //return User.Current.Id == User.Visitor.Id && !OutputCache.DisableCache();
            if (OutputCache.DisableCache())
                return false;

            if (User.Current.Id == User.Visitor.Id)
                return true;

            if(!cacheableForLoggedInUser)
                return false;

            return PortalContext.Current.LoggedInUserCacheEnabled;
        }
        public static string GetCachedOutput(string cacheKey)
        {
            return DistributedApplication.Cache.Get(cacheKey) as string;
        }
        public static void InsertOutputIntoCache(double absoluteExpiration, double slidingExpiration, string cacheKey, string output, CacheDependency cacheDependency, CacheItemPriority priority)
        {
            // -1 means it comes from web config
            var absBase = absoluteExpiration == -1 ? OutputCache.GetAbsoluteExpirationSeconds() : absoluteExpiration;
            var slidingBase = slidingExpiration == -1 ? OutputCache.GetSlidingExpirationSeconds() : slidingExpiration;

            // 0 means no caching
            var abs = absBase == 0 ? Cache.NoAbsoluteExpiration : DateTime.Now.AddSeconds((double)absBase);
            var sliding = slidingBase == 0 ? Cache.NoSlidingExpiration : TimeSpan.FromSeconds((double)slidingBase);

            if (abs != Cache.NoAbsoluteExpiration && sliding != Cache.NoSlidingExpiration)
                sliding = Cache.NoSlidingExpiration;

            DistributedApplication.Cache.Insert(cacheKey, output, cacheDependency, abs, sliding, priority, null);
        }
        internal static double GetAbsoluteExpirationSeconds()
        {
            var configValue = System.Configuration.ConfigurationManager.AppSettings["AbsoluteExpirationSeconds"];
            if (String.IsNullOrEmpty(configValue))
                return 0;
            double result = 0;
            return double.TryParse(configValue, out result) ? result : 0;
        }
        internal static double GetSlidingExpirationSeconds()
        {
            var configValue = System.Configuration.ConfigurationManager.AppSettings["SlidingExpirationSeconds"];
            if (String.IsNullOrEmpty(configValue))
                return 0;
            double result = 0;
            return double.TryParse(configValue, out result) ? result : 0;
        }
    }
}
