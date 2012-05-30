using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using SenseNet.ContentRepository.Storage;
using System.Net;
using SenseNet.Diagnostics;
using System.Threading;


namespace SenseNet.Portal.Virtualization
{
    public static class HttpHeaderTools
    {
        private delegate void PurgeDelegate(IEnumerable<string> urls);


        // ============================================================================================ Private methods
        private static bool IsClientCached(DateTime contentModified)
        {
            var modifiedSinceHeader = HttpContext.Current.Request.Headers["If-Modified-Since"];
            if (modifiedSinceHeader != null)
            {
                DateTime isModifiedSince;
                if (DateTime.TryParse(modifiedSinceHeader, out isModifiedSince))
                    return isModifiedSince - contentModified > TimeSpan.FromSeconds(-1);    // contentModified is more precise
            }
            return false;
        }
        private static string[] PurgeUrlFromProxy(string url, bool async)
        {
            // PURGE /contentem/maicontent.jpg HTTP/1.1
            // Host: myhost.hu

            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");

            if (PortalContext.ProxyIPs.Count == 0)
                return null;

            string contentPath;
            string host;

            var slashIndex = url.IndexOf("/");
            if (slashIndex >= 0)
            {
                contentPath = url.Substring(slashIndex);
                host = url.Substring(0, slashIndex);
            }
            else
            {
                contentPath = "/";
                host = url;
            }

            if (string.IsNullOrEmpty(host) && HttpContext.Current != null)
                host = HttpContext.Current.Request.Url.Host;

            string[] result = null;
            if (!async)
                result = new string[PortalContext.ProxyIPs.Count];

            var proxyIndex = 0;
            foreach (var proxyIP in PortalContext.ProxyIPs)
            {
                var proxyUrl = string.Concat("http://", proxyIP, contentPath);

                try
                {
                    var request = WebRequest.Create(proxyUrl) as HttpWebRequest;
                    if (request == null)
                        break;

                    request.Method = "PURGE";
                    request.Host = host;

                    if (!async)
                    {
                        using (request.GetResponse())
                        {
                            //we do not need to read the request here, just the status code
                            result[proxyIndex] = "OK";
                        }
                    }
                    else
                    {
                        request.BeginGetResponse(null, null);
                    }
                }
                catch (WebException wex)
                {
                    var wr = wex.Response as HttpWebResponse;
                    if (wr != null && !async)
                    {
                        switch (wr.StatusCode)
                        {
                            case HttpStatusCode.NotFound:
                                result[proxyIndex] = "MISS";
                                break;
                            case HttpStatusCode.OK:
                                result[proxyIndex] = "OK";
                                break;
                            default:
                                Logger.WriteException(wex);
                                result[proxyIndex] = wex.Message;
                                break;
                        }
                    }
                    else
                    {
                        Logger.WriteException(wex);
                        if (!async)
                            result[proxyIndex] = wex.Message;
                    }
                }

                proxyIndex++;
            }

            return result;
        }
        private static void PurgeUrlsFromProxyAsyncWithDelay(IEnumerable<string> urls) 
        {
            if (PortalContext.PurgeUrlDelayInMilliSeconds.HasValue)
            {
                Thread.Sleep(PortalContext.PurgeUrlDelayInMilliSeconds.Value);
            }
            var distinctUrls = urls.Distinct().Where(url => !string.IsNullOrEmpty(url));
            foreach (var url in distinctUrls)
            {
                PurgeUrlFromProxyAsync(url);
            }
        }


        // ============================================================================================ Public methods
        public static void SetCacheControlHeaders(int cacheForSeconds)
        {
            SetCacheControlHeaders(cacheForSeconds, HttpCacheability.Public);
        }
        public static void SetCacheControlHeaders(int cacheForSeconds, HttpCacheability httpCacheability)
        {
            HttpContext.Current.Response.Cache.SetCacheability(httpCacheability);
            HttpContext.Current.Response.Cache.SetMaxAge(new TimeSpan(0, 0, cacheForSeconds));
            HttpContext.Current.Response.Cache.SetSlidingExpiration(true);  // max-age does not appear in response header without this...
        }
        public static int? GetMaxAgeForExtension(string extension)
        {
            if (PortalContextModule.ClientCacheConfig != null && PortalContextModule.ClientCacheConfig.ContainsKey(extension))
                return PortalContextModule.ClientCacheConfig[extension];

            return null;
        }
        public static void EndResponseForClientCache(DateTime lastModificationDate)
        {
            //
            //  http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html
            //  14.25 If-Modified-Since
            //  14.29 Last-Modified
            //

            var context = HttpContext.Current;
            if (IsClientCached(lastModificationDate))
            {
                context.Response.StatusCode = 304;
                context.Response.SuppressContent = true;
                context.Response.Flush();
                context.Response.End();
                // thread exits here
            }
            else
                context.Response.Cache.SetLastModified(lastModificationDate);
        }

        /// <summary>
        /// Sends a PURGE request to all of the configured proxy servers for the given urls. Purge requests are synchronous.
        /// </summary>
        /// <param name="urls">Urls of the content that needs to be purged. The urls must start with the host name (e.g. www.example.com/mycontent/myimage.jpg).</param>
        /// <returns>A Dictionary with the given urls and the purge result Dictionaries.</returns>
        public static Dictionary<string, string[]> PurgeUrlsFromProxy(IEnumerable<string> urls)
        {
            return urls.Distinct().Where(url => !string.IsNullOrEmpty(url)).ToDictionary(url => url, PurgeUrlFromProxy);
        }

        /// <summary>
        /// Sends a PURGE request to all of the configured proxy servers for the given url. Purge request is synchronous and result is processed.
        /// </summary>
        /// <param name="url">Url of the content that needs to be purged. It must start with the host name (e.g. www.example.com/mycontent/myimage.jpg).</param>
        /// <returns>A Dictionary with the result of each proxy request. Possible values: OK, MISS, {error message}.</returns>
        public static string[] PurgeUrlFromProxy(string url)
        {
            return PurgeUrlFromProxy(url, false);
        }

        /// <summary>
        /// Sends a PURGE request to all of the configured proxy servers for the given url. Purge request is asynchronous and result is not processed.
        /// </summary>
        /// <param name="url">Url of the content that needs to be purged. It must start with the host name (e.g. www.example.com/mycontent/myimage.jpg).</param>
        public static void PurgeUrlFromProxyAsync(string url)
        {
            PurgeUrlFromProxy(url, true);
        }

        /// <summary>
        /// Starts an async thread that will start purging urls after a specified delay. Delay is configured with PurgeUrlDelayInSeconds key in web.config.
        /// </summary>
        /// <param name="urls"></param>
        public static void BeginPurgeUrlsFromProxyWithDelay(IEnumerable<string> urls)
        {
            var purgeDelegate = new PurgeDelegate(PurgeUrlsFromProxyAsyncWithDelay);
            purgeDelegate.BeginInvoke(urls, null, null);
        }
    }
}
