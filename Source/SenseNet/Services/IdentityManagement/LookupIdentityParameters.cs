using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SenseNet.Services.IdentityManagement
{
    [DataContract]
    public class LookupIdentityParameters
    {
        [DataMember]
        public string IdentityToken { get; set; }

        [DataMember]
        public DateTime RequestTime { get; set; }
    }
}