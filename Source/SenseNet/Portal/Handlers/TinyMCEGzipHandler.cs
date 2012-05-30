/*
 * $Id: tiny_mce_gzip.aspx 316 2007-10-25 14:50:55Z spocke $
 *
 * @author Moxiecode
 * @copyright Copyright © 2006, Moxiecode Systems AB, All rights reserved.
 *
 * This file compresses the TinyMCE JavaScript using GZip and
 * enables the browser to do two requests instead of one for each .js file.
 *
 * It's a good idea to use the diskcache option since it reduces the servers workload.
 * 
 * Modified by: Attila Szabo
 * Remark: The original code is customized to fit in the portal layer of the Sense/Net 6.0.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using ICSharpCode.SharpZipLib.GZip;
using System.Text.RegularExpressions;
using System.IO;
using System.Security.Cryptography;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.UI;
using SN = SenseNet.ContentRepository;
using SenseNet.ContentRepository;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.Handlers
{
    public class TinyMCEGzipHandler : IHttpHandler
    {
        private const string CacheFolderPath = "/Root/Global/scripts";

        private string _cacheKey;
        private string _cacheFile;
        private string _content;
        private string _encodingText;
        private string _suffix;

        private string[] _plugins;
        private string[] _languages;
        private string[] _themes;
        private string[] _custom;    // custom scripts
        private bool _diskCache;
        private bool _supportsGzip;
        private bool _isJS;
        private bool _compress;
        private bool _core;
        private int _expiresOffset;

        private IFolder _cacheFolder;
        private IFolder CacheFolder
        {
            get
            {
                if (this._cacheFolder == null)
                   this._cacheFolder = Node.LoadNode(CacheFolderPath) as IFolder;
               return this._cacheFolder;
            }
        }

        private Encoding GetEncoding
        {
            get { return Encoding.GetEncoding("windows-1252"); }
        }
        private string FullCacheFilePath
        {
            get { return RepositoryPath.Combine(CacheFolderPath, this._cacheFile); }
        }
        

        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            ProcessParams(context);
            _custom = new string[] { };

            SetHeaders(context);
            SetCacheValues(context);

            // Is called directly then auto init with default settings
            if (!_isJS)
            {
                WriteTinyMCESource(context);
                return;
            }

            // Setup cache info
            if (_diskCache)
                SetCacheInfo();

            SetEncoding(context);

            // Use cached file disk cache
            bool cacheFileExists = Node.Exists(this.FullCacheFilePath);
            if (_diskCache && _supportsGzip && cacheFileExists)
            {
                LoadCacheFile(context);
                return;
            }

            ProcessJavaScriptSources();

            // Generate GZIP'd content
            if (_supportsGzip)
            {
                if (_compress)
                    context.Response.AppendHeader("Content-Encoding", _encodingText);

                BinaryData cacheBinary = new BinaryData();

                if (_diskCache && _cacheKey != "")
                {
                    //-- disk caching

                    if (this.CacheFolder == null)
                        throw new ApplicationException(String.Concat(CacheFolderPath, " folder could not be loaded."));
                    
                    // Gzip compress
                    CreateOrModifyCacheFile(cacheBinary, this._compress);                    

                    // Write to stream
                    WriteDatas(context, cacheBinary);
                }
                else
                    WriteWithoutCache(context);
            }
            else
                context.Response.Write(_content);
        }
        #endregion

        private void WriteWithoutCache(HttpContext context)
        {
            GZipOutputStream gzipStream = new GZipOutputStream(context.Response.OutputStream);
            byte[] buff = GetEncoding.GetBytes(_content.ToCharArray());
            gzipStream.Write(buff, 0, buff.Length);
            gzipStream.Flush();
            gzipStream.Close();
        }

        private static void WriteDatas(HttpContext context, BinaryData cacheBinary)
        {
            if (cacheBinary.GetStream() != null)
            {
                MemoryStream result = new MemoryStream();
                result = (MemoryStream)cacheBinary.GetStream();
                context.Response.BinaryWrite(result.ToArray());
            }
        }

        private void CreateOrModifyCacheFile(BinaryData cacheBinary, bool compress)
        {
            SN.File f = null;
            MemoryStream cacheStream = new MemoryStream();

            if (compress)
            {
                GZipOutputStream gzipStream = new GZipOutputStream(cacheStream);
                byte[] buff = Encoding.ASCII.GetBytes(this._content.ToCharArray());
                gzipStream.Write(buff, 0, buff.Length);
                gzipStream.Flush();
                gzipStream.Close();

                // set compressed binary
                byte[] compressedData = cacheStream.ToArray();
                cacheBinary.SetStream(new MemoryStream(compressedData));
            } else
                cacheBinary.SetStream(Tools.GetStreamFromString(_content));

            // gets cache file or creates a new one, the new stream will be saved in both cases
            if (!Node.Exists(FullCacheFilePath))
            {
				f = SN.File.CreateByBinary(this.CacheFolder, cacheBinary);
                f.Name = _cacheFile;
            }
            else
            {
				f = Node.Load<SN.File>(this.FullCacheFilePath);
                f.Binary = cacheBinary;
            }
            f.Save();

        }
        private void ProcessJavaScriptSources()
        {
            int i;
            int x;
            // Add core
            if (_core)
            {
                _content += GetFileContents("tiny_mce" + _suffix + ".js");

                // Patch loading functions
                _content += "tinyMCE_GZ.start();";
            }

            // Add core languages
            for (x = 0; x < _languages.Length; x++)
                _content += GetFileContents("langs/" + _languages[x] + ".js");

            // Add themes
            for (i = 0; i < _themes.Length; i++)
            {
                _content += GetFileContents("themes/" + _themes[i] + "/editor_template" + _suffix + ".js");

                for (x = 0; x < _languages.Length; x++)
                    _content += GetFileContents("themes/" + _themes[i] + "/langs/" + _languages[x] + ".js");
            }

            // Add plugins
            for (i = 0; i < _plugins.Length; i++)
            {
                _content += GetFileContents("plugins/" + _plugins[i] + "/editor_plugin" + _suffix + ".js");

                for (x = 0; x < _languages.Length; x++)
                    _content += GetFileContents("plugins/" + _plugins[i] + "/langs/" + _languages[x] + ".js");
            }

            // Add custom files
            for (i = 0; i < _custom.Length; i++)
                _content += GetFileContents(_custom[i]);

            // Restore loading functions
            if (_core)
                _content += "tinyMCE_GZ.end();";
        }

        private void LoadCacheFile(HttpContext context)
        {
			var f = Node.Load<SN.File>(this.FullCacheFilePath);
            if (f == null) 
                return;
            
            context.Response.AppendHeader("Content-Encoding", _encodingText);
            var dataStream = f.Binary.GetStream();
            var contentBytes = new byte[dataStream.Length];
            dataStream.Position = 0;
            dataStream.Read(contentBytes, 0, (int)dataStream.Length);
            context.Response.BinaryWrite(contentBytes);
        }

        private void SetEncoding(HttpContext context)
        {
            // Check if it supports gzip
            _encodingText = Regex.Replace("" + context.Request.Headers["Accept-Encoding"], @"\s+", "").ToLower();
            _supportsGzip = _encodingText.IndexOf("gzip") != -1 || context.Request.Headers["---------------"] != null;
            _encodingText = _encodingText.IndexOf("x-gzip") != -1 ? "x-gzip" : "gzip";
        }

        private void SetCacheInfo()
        {
            int i;
            _cacheKey = GetParam("plugins", "") + GetParam("languages", "") + GetParam("themes", "");

            for (i = 0; i < _custom.Length; i++)
                _cacheKey += _custom[i];

            _cacheKey = MD5(_cacheKey);

            if (_compress)
                _cacheFile = "tiny_mce_" + _cacheKey + ".gz";
            else
                _cacheFile = "tiny_mce_" + _cacheKey + ".js";
        }

        private static void WriteTinyMCESource(HttpContext context)
        {
			var f = Node.Load<SN.File>(UITools.ClientScriptConfigurations.TinyMCEPath);
            if (f != null && f.Binary.GetStream() != null)
            {
                string writeData = Tools.GetStreamString(f.Binary.GetStream());
                context.Response.Write(writeData);
                context.Response.Write("tinyMCE_GZ.init({});");
            }
        }

        private void ProcessParams(HttpContext context)
        {
            _plugins = GetParam("plugins", "").Split(',');
            _languages = GetParam("languages", "").Split(',');
            _themes = GetParam("themes", "").Split(',');
            _diskCache = GetParam("diskcache", "") == "true";
            _isJS = GetParam("js", "") == "true";
            _compress = GetParam("compress", "true") == "true";
            _core = GetParam("core", "true") == "true";
            _suffix = GetParam("suffix", "") == "_src" ? "_src" : "";
            _expiresOffset = 10; // Cache for 10 days in browser cache
        }

        private void SetCacheValues(HttpContext context)
        {
            context.Response.Cache.SetExpires(DateTime.Now.AddDays(_expiresOffset));
            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.Cache.SetValidUntilExpires(false);

            // Vary by all parameters and some headers
            context.Response.Cache.VaryByHeaders["Accept-Encoding"] = true;
            context.Response.Cache.VaryByParams["theme"] = true;
            context.Response.Cache.VaryByParams["language"] = true;
            context.Response.Cache.VaryByParams["plugins"] = true;
            context.Response.Cache.VaryByParams["lang"] = true;
            context.Response.Cache.VaryByParams["index"] = true;
        }

        private static void SetHeaders(HttpContext context)
        {
            context.Response.ContentType = "text/javascript";
            context.Response.Charset = "UTF-8";
            context.Response.Buffer = false;
        }

        public string GetParam(string name, string def)
        {
            string value = HttpContext.Current.Request.QueryString[name] != null ? "" + HttpContext.Current.Request.QueryString[name] : def;

            return Regex.Replace(value, @"[^0-9a-zA-Z\\-_,]+", "");
        }
        public string GetFileContents(string path)
        {
            try
            {
                string content;

                string tinyRootPath = UITools.ClientScriptConfigurations.TinyMCEPath;
                tinyRootPath = tinyRootPath.Substring(0, tinyRootPath.LastIndexOf("/") + 1);
                string filePath = VirtualPathUtility.Combine(tinyRootPath, path);

				var f = Node.Load<SN.File>(filePath);
                if (f != null && f.Binary.GetStream() != null)
                {
                    Stream fileStream = null;
                    fileStream = f.Binary.GetStream();
                    content = Tools.GetStreamString(fileStream);
                }
                else
                    return string.Empty;
                return content;
            }
            catch (Exception ex) //logged
            {
                Logger.WriteException(ex);
            }

            return "";
        }
        public string MD5(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(Encoding.ASCII.GetBytes(str));
            str = BitConverter.ToString(result);

            return str.Replace("-", "");
        }
    }
}