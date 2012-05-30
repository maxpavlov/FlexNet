using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.ContentRepository.Storage.Security
{
    [Serializable]
    public partial class SnIdentity
    {
        public int NodeId { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public SnIdentityKind Kind { get; set; }

    }
}
