using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.IO;

namespace SenseNet.Benchmarking
{
    internal class Configuration
    {
        private static int? _threads;
        public static int Threads
        {
            get { return _threads.HasValue ? _threads.Value : (_threads = GetIntegerConfigValue("ThreadCount", 20)).Value; }
        }

        private static int? _maxFileCount;
        public static int MaxFileCount
        {
            get { return _maxFileCount.HasValue ? _maxFileCount.Value : (_maxFileCount = GetIntegerConfigValue("MaxFileCount", 10000000)).Value; }
        }

        private static string[] _fileProfile;
        public static string[] FileProfile
        {
            get
            {
                if (_fileProfile == null)
                {
                    var path = FileRootPath;
                    if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                        return new string[0];

                    _fileProfile = Directory.GetFiles(path).ToArray();
                }

                return _fileProfile;
            }
        }

        internal static string FileRootPath
        {
            get { return ConfigurationManager.AppSettings["FileRootPath"] ?? string.Empty; }
        }

        private static string[] _fileProfileNames;
        public static string[] FileProfileNames
        {
            get 
            {
                return _fileProfileNames ??
                       (_fileProfileNames = FileProfile.Select(Path.GetFileName).ToArray());
            }
        }

        private static long[] _fileProfileSizes;
        public static long[] FileProfileSizes
        {
            get
            {
                return _fileProfileSizes ??
                       (_fileProfileSizes = FileProfile.Select(x => new FileInfo(x).Length).ToArray());
            }
        }

        private static int[] _folderProfile;
        public static int[] FolderProfile
        {
            get
            {
                return _folderProfile ??
                    (_folderProfile = GetIntArrayConfigValue("FolderProfile"));
            }
        }

        public static string ContentRepositoryPath
        {
            get { return ConfigurationManager.AppSettings["ContentRepositoryPath"] ?? string.Empty; }
        }

        private static string[] _rootUrls;
        public static string[] Urls
        {
            get
            {
                return _rootUrls ??
                    (_rootUrls = GetStringArrayConfigValue("Urls"));
            }
        }

        private static int? _maxTaskQueueSize;
        public static int MaxTaskQueueSize
        {
            get { return _maxTaskQueueSize.HasValue ? _maxTaskQueueSize.Value : (_maxTaskQueueSize = GetIntegerConfigValue("MaxTaskQueueSize", 30)).Value; }
        }

        private static string _sessionName;
        public static string SessionName
        {
            get 
            {
                return _sessionName ??
                       (_sessionName = ConfigurationManager.AppSettings["SessionName"] ?? string.Empty);
            }
        }

        private static bool? _saveDetailedLog;
        public static bool SaveDetailedLog
        {
            get { return _saveDetailedLog.HasValue ? _saveDetailedLog.Value : (_saveDetailedLog = GetBooleanConfigValue("SaveDetailedLog", true)).Value; }
        }

        private static bool? _enableEqualLoad;
        public static bool EnableEqualLoad
        {
            get { return _enableEqualLoad.HasValue ? _enableEqualLoad.Value : (_enableEqualLoad = GetBooleanConfigValue("EnableEqualLoad", true)).Value; }
        }

        private static string[] _adminEmails;
        public static string[] AdminEmails
        {
            get 
            {
                return _adminEmails ??
                    (_adminEmails = GetStringArrayConfigValue("AdminEmails"));
            }
        }

        private static int? _emailFrequency;
        public static int EmailFrequency
        {
            get { return _emailFrequency.HasValue ? _emailFrequency.Value : (_emailFrequency = GetIntegerConfigValue("EmailFrequency", 30)).Value; }
        }

        private static string _elasticUserName;
        public static string ElasticEmailUserName
        {
            get
            {
                return _elasticUserName ??
                       (_elasticUserName = ConfigurationManager.AppSettings["ElasticEmailUserName"] ?? string.Empty);
            }
        }

        private static string _elasticApiKey;
        public static string ElasticEmailApiKey
        {
            get
            {
                return _elasticApiKey ??
                       (_elasticApiKey = ConfigurationManager.AppSettings["ElasticEmailApiKey"] ?? string.Empty);
            }
        }

        private static string _proxyAddress;
        public static string ProxyAddress
        {
            get
            {
                return _proxyAddress ??
                       (_proxyAddress = ConfigurationManager.AppSettings["ProxyAddress"] ?? string.Empty);
            }
        }

        private static string _proxyUserName;
        public static string ProxyUserName
        {
            get
            {
                return _proxyUserName ??
                       (_proxyUserName = ConfigurationManager.AppSettings["ProxyUserName"] ?? string.Empty);
            }
        }

        private static string _proxyPassword;
        public static string ProxyPassword
        {
            get
            {
                return _proxyPassword ??
                       (_proxyPassword = ConfigurationManager.AppSettings["ProxyPassword"] ?? string.Empty);
            }
        }

        private static int? _requestTimeout;
        public static int RequestTimeout
        {
            get { return _requestTimeout.HasValue ? _requestTimeout.Value : (_requestTimeout = GetIntegerConfigValue("RequestTimeout", 1000)).Value; }
        }

        //============================================================================================= Helper methods

        private static int GetIntegerConfigValue(string key, int defaultValue)
        {
            var result = defaultValue;

            var configString = ConfigurationManager.AppSettings[key];
            if (!string.IsNullOrEmpty(configString))
            {
                int configVal;
                if (int.TryParse(configString, out configVal))
                    result = configVal;
            }

            return result;
        }

        private static string[] GetStringArrayConfigValue(string key)
        {
            var configString = ConfigurationManager.AppSettings[key];
            return !string.IsNullOrEmpty(configString) 
                ? configString.Split(new[] {';', ','}, StringSplitOptions.RemoveEmptyEntries) 
                : new string[0];
        }

        private static int[] GetIntArrayConfigValue(string key)
        {
            var stringVals = GetStringArrayConfigValue(key);
            return stringVals.Select(sv => int.Parse(sv)).ToArray();
        }

        private static bool GetBooleanConfigValue(string key, bool defaultValue)
        {
            var result = defaultValue;

            var configString = ConfigurationManager.AppSettings[key];
            if (!string.IsNullOrEmpty(configString))
            {
                bool configVal;
                if (bool.TryParse(configString, out configVal))
                    result = configVal;
            }

            return result;
        }
    }
}
