using System;
using System.Collections.Generic;
using System.Text;
using  SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Security.ADSync;

namespace SenseNet.ContentRepository
{
    [ContentHandler]
    public class Domain : Folder, IADSyncable
    {
        [Obsolete("Use typeof(Domain).Name instead.", true)]
        public static readonly string NodeTypeName = typeof(Domain).Name;
        public static readonly string BuiltInDomainName = "BuiltIn";

        public Domain(Node parent) : this(parent, null) { }
        public Domain(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected Domain(NodeToken token) : base(token) { }

        //////////////////////////////////////// Public Properties ////////////////////////////////////////

        public bool IsBuiltInDomain
        {
            get { return Name == BuiltInDomainName; }
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