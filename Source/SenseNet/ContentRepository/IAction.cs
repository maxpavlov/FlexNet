using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using SenseNet.ContentRepository.Storage.Schema;

namespace SenseNet.ContentRepository
{
    public interface IAction
    {
        string Name { get; }
        bool Enabled { get; }
        bool Visible { get; }
        IEnumerable<PermissionType> RequiredPermissions { get; }

        IAction Clone();
    }
}
