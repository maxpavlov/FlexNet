using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using SenseNet.ContentRepository;

namespace SenseNet.Services.IdentityManagement
{
    [DataContract]
    public class Identity
    {
        [DataMember]
        public String Email { get; set; }
        [DataMember]
        public String FullName { get; set; }
        [DataMember]
        public String UserName { get; set; }

        public Identity()
        {
        }

        public Identity(User user)
        {
            this.Email = user.Email;
            this.FullName = user.FullName;
            this.UserName = user.Name;
        }
    }
}