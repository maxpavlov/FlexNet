using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SenseNet.Services.IdentityManagement
{
    [DataContract]
    public class LookupIdentityResult
    {
        [DataMember]
        public Identity Identity { get; set; }

        [DataMember]
        public LookupIdentityParameters RequestParameters { get; set; }

        [DataMember]
        public DateTime RequestTime { get; set; }

        [DataMember]
        public DateTime ResponseTime { get; set; }
    }
}