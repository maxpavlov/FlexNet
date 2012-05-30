using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace SenseNet.Portal.Exchange
{
    public class Configuration
    {
        private static readonly string EXCHANGESERVICESECTIONKEY = "sensenet/exchangeService";


        private static readonly string SUBSCRIPTIONSERVICEENABLEDKEY = "SubscriptionServiceEnabled";
        private static object __subscriptionServiceEnabledSync = new object();
        private static bool? _subscriptionServiceEnabled = null;
        public static bool SubscriptionServiceEnabled
        {
            get
            {
                if (!_subscriptionServiceEnabled.HasValue)
                {
                    lock (__subscriptionServiceEnabledSync)
                    {
                        if (!_subscriptionServiceEnabled.HasValue)
                        {
                            var section = ConfigurationManager.GetSection(Configuration.EXCHANGESERVICESECTIONKEY) as System.Collections.Specialized.NameValueCollection;
                            if (section != null)
                            {
                                var valStr = section[SUBSCRIPTIONSERVICEENABLEDKEY];
                                if (!string.IsNullOrEmpty(valStr))
                                {
                                    bool val;
                                    if (bool.TryParse(valStr, out val))
                                        _subscriptionServiceEnabled = val;
                                }
                            }
                            if (!_subscriptionServiceEnabled.HasValue)
                                _subscriptionServiceEnabled = false;
                        }
                    }
                }
                return _subscriptionServiceEnabled.Value;
            }
        }


        private static readonly string SUBSCRIBETOPUSHNOTIFICATIONSKEY = "SubscribeToPushNotifications";
        private static object __subscribeToPushNotifications = new object();
        private static bool? _subscribeToPushNotifications = null;
        public static bool SubscribeToPushNotifications
        {
            get
            {
                if (!_subscribeToPushNotifications.HasValue)
                {
                    lock (__subscribeToPushNotifications)
                    {
                        if (!_subscribeToPushNotifications.HasValue)
                        {
                            var section = ConfigurationManager.GetSection(Configuration.EXCHANGESERVICESECTIONKEY) as System.Collections.Specialized.NameValueCollection;
                            if (section != null)
                            {
                                var valStr = section[SUBSCRIBETOPUSHNOTIFICATIONSKEY];
                                if (!string.IsNullOrEmpty(valStr))
                                {
                                    bool val;
                                    if (bool.TryParse(valStr, out val))
                                        _subscribeToPushNotifications = val;
                                }
                            }
                            if (!_subscribeToPushNotifications.HasValue)
                                _subscribeToPushNotifications = false;
                        }
                    }
                }
                return _subscribeToPushNotifications.Value;
            }
        }


        private static readonly string STATUSPOLLINGINTERVALINMINUTESKEY = "StatusPollingIntervalInMinutes";
        private static object __statusPollingIntervalInMinutesSync = new object();
        private static int? _statusPollingIntervalInMinutes = null;
        public static int StatusPollingIntervalInMinutes
        {
            get
            {
                if (!_statusPollingIntervalInMinutes.HasValue)
                {
                    lock (__statusPollingIntervalInMinutesSync)
                    {
                        if (!_statusPollingIntervalInMinutes.HasValue)
                        {
                            var section = ConfigurationManager.GetSection(Configuration.EXCHANGESERVICESECTIONKEY) as System.Collections.Specialized.NameValueCollection;
                            if (section != null)
                            {
                                var valStr = section[STATUSPOLLINGINTERVALINMINUTESKEY];
                                if (!string.IsNullOrEmpty(valStr))
                                {
                                    int val;
                                    if (int.TryParse(valStr, out val))
                                        _statusPollingIntervalInMinutes = val;
                                }
                            }
                            if (!_statusPollingIntervalInMinutes.HasValue)
                                _statusPollingIntervalInMinutes = 60;
                        }
                    }
                }
                return _statusPollingIntervalInMinutes.Value;
            }
        }


        private static readonly string PUSHNOTIFICATIONSERVICEPATHKEY = "PushNotificationServicePath";
        private static object __pushNotificationServicePath = new object();
        private static string _pushNotificationServicePath = null;
        public static string PushNotificationServicePath
        {
            get
            {
                if (_pushNotificationServicePath == null)
                {
                    lock (__pushNotificationServicePath)
                    {
                        if (_pushNotificationServicePath == null)
                        {
                            var section = ConfigurationManager.GetSection(Configuration.EXCHANGESERVICESECTIONKEY) as System.Collections.Specialized.NameValueCollection;
                            if (section != null)
                            {
                                var valStr = section[PUSHNOTIFICATIONSERVICEPATHKEY];
                                if (!string.IsNullOrEmpty(valStr))
                                {
                                    _pushNotificationServicePath = valStr;
                                }
                            }
                            if (_pushNotificationServicePath == null)
                                _pushNotificationServicePath = string.Empty;
                        }
                    }
                }
                return _pushNotificationServicePath;
            }
        }


        private static readonly string EXCHANGEADDRESSKEY = "ExchangeAddress";
        private static object __exchangeAddress = new object();
        private static string _exchangeAddress = null;
        public static string ExchangeAddress
        {
            get
            {
                if (_exchangeAddress == null)
                {
                    lock (__exchangeAddress)
                    {
                        if (_exchangeAddress == null)
                        {
                            var section = ConfigurationManager.GetSection(Configuration.EXCHANGESERVICESECTIONKEY) as System.Collections.Specialized.NameValueCollection;
                            if (section != null)
                            {
                                var valStr = section[EXCHANGEADDRESSKEY];
                                if (!string.IsNullOrEmpty(valStr))
                                {
                                    _exchangeAddress = valStr;
                                }
                            }
                            if (_exchangeAddress == null)
                                _exchangeAddress = string.Empty;
                        }
                    }
                }
                return _exchangeAddress;
            }
        }

    }
}
