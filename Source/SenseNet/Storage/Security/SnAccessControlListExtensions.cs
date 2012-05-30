using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.ContentRepository.Storage.Security
{
    public partial class SnAccessControlList
    {
        public IEnumerable<SnAccessControlEntry> Entries { get; set; }
    }
}
