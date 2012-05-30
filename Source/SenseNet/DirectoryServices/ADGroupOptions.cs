using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.DirectoryServices
{
    public enum ADGroupOptions
    {
        GlobalSecurityGroup = -2147483646,
        LocalSecurityGroup = -2147483644,
        BuiltInGroup = -2147483643,
        UniversalSecurityGroup = -2147483640,
        GlobalDistributionGroup = 2,
        LocalDistributionGroup = 4,
        UniversalDistributionGroup = 8
    }
}
