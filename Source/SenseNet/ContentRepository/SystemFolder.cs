using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.ContentRepository
{
    [ContentHandler]
    public class SystemFolder : Folder
    {
        public SystemFolder(Node parent) : this(parent, null) { }
		public SystemFolder(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected SystemFolder(NodeToken nt) : base(nt) { }

        public static GenericContent GetSystemContext(Node child)
        {
            SystemFolder ancestor = null;

            while ((child != null) && ((ancestor = child as SystemFolder) == null))
                child = child.Parent;

            return (ancestor != null) ? ancestor.Parent as GenericContent : null;
        }
    }
}
