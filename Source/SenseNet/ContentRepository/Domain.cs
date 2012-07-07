using System;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Security.ADSync;
using SenseNet.ContentRepository.Storage.Data;

namespace SenseNet.ContentRepository
{
    [ContentHandler]
    public class Domain : Folder, IADSyncable
    {
        [Obsolete("Use typeof(Domain).Name instead.", true)]
        public static readonly string NodeTypeName = typeof(Domain).Name;
        [Obsolete("Use RepositoryConfiguration.BuiltInDomainName instead.", true)]
        public static readonly string BuiltInDomainName = RepositoryConfiguration.BuiltInDomainName;

        public Domain(Node parent) : this(parent, null) { }
        public Domain(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected Domain(NodeToken token) : base(token) { }

        //////////////////////////////////////// Public Properties ////////////////////////////////////////

        public bool IsBuiltInDomain
        {
            get { return Name == RepositoryConfiguration.BuiltInDomainName; }
        }

        //=================================================================================== IADSyncable Members
        public void UpdateLastSync(System.Guid? guid)
        {
            if (guid.HasValue)
                this["SyncGuid"] = ((System.Guid)guid).ToString();
            this["LastSync"] = System.DateTime.Now;

            this.Save();
        }
    }
}