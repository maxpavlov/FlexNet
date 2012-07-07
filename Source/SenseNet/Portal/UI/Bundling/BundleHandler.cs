using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Web;

namespace SenseNet.Portal.UI.Bundling
{
    public class BundleHandler : IHttpHandler
    {
        private static MemoryCache _cache = new MemoryCache("SenseNetBundleCache");

        private static List<Bundle> _bundles = new List<Bundle>();
        private static ReaderWriterLockSlim _bundlesLock = new ReaderWriterLockSlim();

        bool IHttpHandler.IsReusable
        {
            get { return true; }
        }

        public static void AddBundleIfNotThere(Bundle b)
        {
            if (!b.IsClosed)
                throw new Exception("You can only add closed bundles to the BundleHandler, sorry.");

            Bundle alreadyThere;

            _bundlesLock.EnterReadLock();
            
            try
            {
                alreadyThere = _bundles.SingleOrDefault(x => x.Hash == b.Hash);
            }
            finally 
            {
                _bundlesLock.ExitReadLock();
            }

            if (alreadyThere == null)
            {
                _bundlesLock.EnterWriteLock();

                try
                {
                    alreadyThere = _bundles.SingleOrDefault(x => x.Hash == b.Hash);

                    if (alreadyThere == null)
                    {
                        b.LastCacheInvalidationDate = DateTime.Now;
                        _bundles.Add(b);
                    }
                }
                finally 
                {
                    _bundlesLock.ExitWriteLock();
                }
            }
            else
            {
                b.LastCacheInvalidationDate = alreadyThere.LastCacheInvalidationDate;
            }
        }

        public static bool IsBundleInCache(Bundle b)
        {
            return (_cache[b.Hash] as string) != null;
        }

        public static void AddBundleToCache(Bundle bundle)
        {
            Bundle alreadyThere;

            _bundlesLock.EnterReadLock();

            try
            {
                alreadyThere = _bundles.SingleOrDefault(x => x.Hash == bundle.Hash);
            }
            finally 
            {
                _bundlesLock.ExitReadLock();
            }

            if (alreadyThere == null)
            {
                AddBundleIfNotThere(bundle);
            }
            else
            {
                bundle = alreadyThere;
            }

            lock (bundle)
            {
                if (!IsBundleInCache(bundle))
                {
                    var result = bundle.Combine();
                    _cache.Set(bundle.Hash, result, new CacheItemPolicy() { AbsoluteExpiration = bundle.LastCacheInvalidationDate.AddDays(7) });
                }
            }
        }

        public static void InvalidateCacheForPath(string path)
        {
            Bundle[] matchingBundles = null;

            _bundlesLock.EnterReadLock();

            try
            {
                matchingBundles = _bundles.Where(x => x.Paths.Select(z => z.ToLower()).Contains(path.ToLower())).ToArray();
            }
            finally 
            {
                _bundlesLock.ExitReadLock();
            }

            foreach (var bundle in matchingBundles)
            {
                var hash = bundle.Hash;
                bundle.LastCacheInvalidationDate = DateTime.Now;

                if (_cache[hash] as string != null)
                {
                    _cache.Remove(hash);
                }
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Headers["If-Modified-Since"] != null)
            {
                // Since the URLs contains the date of creation, we can be sure that the contents of a URL will never, ever change
                context.Response.StatusCode = 304;
                return;
            }

            var bundleUrl = Path.GetFileName(context.Request.RawUrl);
            Bundle bundle;

            _bundlesLock.EnterReadLock();

            try
            {
                bundle = _bundles.SingleOrDefault(x => x.FakeFilename == bundleUrl);
            }
            finally 
            {
                _bundlesLock.ExitReadLock();
            }

            if (bundle == null)
            {
                // If the bundle is not found, just return 404
                context.Response.StatusCode = 404;
                return;
            }

            var bundleHash = bundle.Hash;
            var result = _cache[bundleHash] as string;

            if (result == null)
            {
                lock (bundle)
                {
                    if (!IsBundleInCache(bundle))
                    {
                        result = bundle.Combine();
                        _cache.Set(bundleHash, result, new CacheItemPolicy() { AbsoluteExpiration = bundle.LastCacheInvalidationDate.AddDays(7) });
                    }
                }

                result = _cache[bundleHash] as string;
            }

            // Check what kind of encodings the client supports
            string acceptEncoding = context.Request.Headers["Accept-Encoding"];

            // Compress the response, if it's supported
            if (!string.IsNullOrEmpty(acceptEncoding))
            {
                if (acceptEncoding.Contains("deflate"))
                {
                    context.Response.Filter = new DeflateStream(context.Response.Filter, CompressionMode.Compress);
                    context.Response.AppendHeader("Content-Encoding", "deflate");
                }
                else if (acceptEncoding.Contains("gzip"))
                {
                    context.Response.Filter = new GZipStream(context.Response.Filter, CompressionMode.Compress);
                    context.Response.AppendHeader("Content-Encoding", "gzip");
                }
            }

            // Allow proxy servers to cache encoded and unencoded versions separately
            context.Response.AppendHeader("Vary", "Content-Encoding");

            // Set headers
            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.Cache.SetExpires(bundle.LastCacheInvalidationDate.AddDays(7));
            context.Response.Cache.SetLastModified(bundle.LastCacheInvalidationDate);
            context.Response.ContentType = bundle.MimeType;

            // Send the actual response
            context.Response.Write(result);
        }
    }
}
