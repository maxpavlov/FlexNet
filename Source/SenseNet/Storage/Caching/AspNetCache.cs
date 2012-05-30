using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Collections;
using System.Configuration;
using System.Globalization;
using SenseNet.Diagnostics;

namespace SenseNet.ContentRepository.Storage.Caching
{
    /// <summary>
    /// Wrapper class around the good old ASP.NET cache 
    /// main features: populator (create/load/whatever the cached item)
    /// Distributed environment wireup
    /// </summary>
    /// 
    public class AspNetCache : CacheBase
    {
        private static object _lockObject = new object();

        public enum TraceVerbosity { Silent, Basic, Verbose };

        //public TraceVerbosity Verbosity { get; set; }

        private System.Web.Caching.Cache _cache;

        public AspNetCache()// : this(TraceVerbosity.Silent)
        {
            _cache = HttpRuntime.Cache;
            if (ConfigurationManager.AppSettings["DisableCache"] == "true")
                Logger.WriteInformation("Cache Disabled.");
        }

        //public AspNetCache(TraceVerbosity verbosity)
        //{
        //    _cache = HttpRuntime.Cache;
        //    Verbosity = verbosity;
        //}


        public override object Get(string key)
        {
            if (ConfigurationManager.AppSettings["DisableCache"] == "true")
                return null;
            return _cache.Get(key);
        }

        public override void Insert(string key, object value)
        {
            //Logger.WriteVerbose("Cache Insert", new Dictionary<string, object>() { { "Key", key }, { "Value", value } });
            _cache.Insert(key, value);
        }

        public override void Remove(string key)
        {
            //Logger.WriteVerbose("Cache Remove", new Dictionary<string, object>() { { "Key", key } });
            _cache.Remove(key);
        }


        public override void Insert(string key, object value, CacheDependency dependencies,
            DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority,
            CacheItemRemovedCallback onRemoveCallback)
        {
            //Logger.WriteVerbose("Cache Insert", CollectInsertedEntryProperties, new object[] { key, absoluteExpiration, slidingExpiration, priority, value });
            _cache.Insert(key, value, dependencies, absoluteExpiration, slidingExpiration, priority, onRemoveCallback);
        }
        //private static IDictionary<string, object> CollectInsertedEntryProperties(object[] values)
        //{
        //    return new Dictionary<string, object>() { 
        //        { "Key", values[0] }, 
        //        { "AbsoluteExpiration", values[1] },
        //        { "SlidingExpiration", values[2] },
        //        { "Priority", values[3] },
        //        { "Value", values[4] }
        //    };
        //}

        public override void Reset()
        {
            Logger.WriteInformation("Cache Reset.");

            List<string> keys = new List<string>();
            lock (_lockObject)
            {
                foreach (DictionaryEntry entry in _cache)
                    keys.Add(entry.ToString());
            }

            foreach (var key in keys)
            {
                _cache.Remove(key);
            }
        }

        public override int Count
        {
            get { return _cache.Count; }
        }

        public override long EffectivePercentagePhysicalMemoryLimit
        {
            get { return _cache.EffectivePercentagePhysicalMemoryLimit; }
        }

        public override long EffectivePrivateBytesLimit
        {
            get { return _cache.EffectivePrivateBytesLimit; }
        }

        public override IEnumerator GetEnumerator()
        {
            return _cache.GetEnumerator();
        }
        public override object this[string key]
        {
            get
            {
                Logger.WriteVerbose("Cache indexed Get.", new Dictionary<string, object> { { "Key", key } });
                return _cache[key];
            }
            set
            {
                Logger.WriteVerbose("Cache indexed Set.", new Dictionary<string, object> { { "Key", key } });
                _cache[key] = value;
            }
        }
    }

}
