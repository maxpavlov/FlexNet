using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Web;
using System.Diagnostics;
using SenseNet.ContentRepository.Storage.Caching;
using SenseNet.Communication.Messaging;
using SenseNet.Diagnostics;

namespace SenseNet.ContentRepository
{
    public static class DistributedApplication
    {
        private static object _syncRoot = new object();
        private static ICache _cache;
        public static ICache Cache
        {
            get
            {
                if (_cache == null)
                {
                    lock (_syncRoot)
                    {
                        if (_cache == null)
                        {
                            _cache = new AspNetCache();
                        }
                    }
                }
                return _cache;
            }
        }

        private static IClusterChannel _currentChannel;

        public static IClusterChannel ClusterChannel
        {
            get
            {
                if (_currentChannel == null)
                {
                    lock (_syncRoot)
                    {
                        if (_currentChannel == null)
                        {
                            try
                            {
                                Type channelAdapterType = GetChannelProviderType();
                                //TODO - defer MessageFormatter type discovery and create
                                Logger.WriteVerbose("Cluster channel created.", Logger.GetDefaultProperties, channelAdapterType);
                                IClusterChannel c = (IClusterChannel)Activator.CreateInstance(channelAdapterType, new BinaryMessageFormatter(), ClusterMemberInfo.Current);
                                c.Start();
                                _currentChannel = c;
                            }
                            catch (Exception e) //logged
                            {
                                //if msmq is misconfigured or we can not access to the channel, we throw up the exception, because this is a vital configuration in our system.
                                //if msmq is configured, but the configuration is not correct, that makes SenseNet unusable 
                                Logger.WriteException(e);
                                throw;
                                //_currentChannel = GetFallbackChannel();
                            }
                        }
                    }
                }
                return _currentChannel;
            }
        }


        private static IClusterChannel GetFallbackChannel()
        {
            return new VoidChannel(new BinaryMessageFormatter(), ClusterMemberInfo.Current);
        }

        private static Type GetChannelProviderType()
        {
            string channelProviderTypeName = ConfigurationManager.AppSettings["ClusterChannelProvider"];
            //TODO localize error message
            if (string.IsNullOrEmpty(channelProviderTypeName))
                return typeof(VoidChannel);

            Type channelAdapterType = SenseNet.ContentRepository.Storage.TypeHandler.GetType(channelProviderTypeName);
            if (channelAdapterType == null)
                throw new ArgumentException("ClusterChannelProvider is not correctly configured.");
            return channelAdapterType;
        }

    }
}