using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.DirectoryServices
{
    public struct SyncInfo
    {
        public bool SyncTreeFound;
        public bool PortalNodeExists;
        public bool PortalParentExists;
        public string TargetPortalPath;
        public string SyncTreeADPath;
        public string SyncTreePortalPath;
        public string SyncTreeADIPAddress;
    }
}
