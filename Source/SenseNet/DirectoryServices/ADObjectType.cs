using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.DirectoryServices
{
    public enum ADObjectType
    {
        None,
        OrgUnit,
        Group,
        User,
        Container,
        Domain,
        Organization,
        AllContainers   // hack: logic of getting the nodetype is different here
    }
}
