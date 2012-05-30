using System;
using System.Collections.Generic;
using System.Text;
using SenseNet.Diagnostics;

namespace SenseNet.Communication.Messaging
{
    [Serializable]
    public class ClusterMemberInfo
    {
        public string ClusterID;
        public string ClusterMemberID;
        public string InstanceID;


        private static ClusterMemberInfo _current;
        private static object _syncRoot = new object();
        public static ClusterMemberInfo Current
        {
            get
            {
                if (_current == null)
                {
                    lock (_syncRoot)
                    {
                        if (_current == null)
                        {
                            var current = new ClusterMemberInfo();
                            current.InstanceID = Guid.NewGuid().ToString();
                            _current = current;
                            Logger.WriteInformation("ClusterMemberInfo created.",
                                new Dictionary<string, object> { { "InstanceID", _current.InstanceID } });
                        }
                    }
                }
                return _current;
            }
        }

        public bool IsMe
        {
            get
            {
                return this.InstanceID == ClusterMemberInfo.Current.InstanceID;
            }
        }
    }
}