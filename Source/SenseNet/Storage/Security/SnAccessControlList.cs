using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SenseNet.ContentRepository.Storage.Security
{
    [Serializable]
    public partial class SnAccessControlList
    {
        public int NodeId { get; set; }
        public string Path { get; set; }
        public bool Inherits { get; set; }
        public SnIdentity Creator { get; set; }
        public SnIdentity LastModifier { get; set; }
    }
}
