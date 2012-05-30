using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SenseNet.Services.IdentityManagement
{
    [DataContract]
    public class GetServiceUrlsResult
    {
        [DataMember]
        public String SignOnUrl { get; set; }
        [DataMember]
        public String SignOffUrl { get; set; }
        [DataMember]
        public String RegistrationUrl { get; set; }
        [DataMember]
        public String LookupIdentityUrl { get; set; }
    }

}