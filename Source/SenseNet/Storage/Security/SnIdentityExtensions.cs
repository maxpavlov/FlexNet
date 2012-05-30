using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.ContentRepository.Storage.Security
{
    public partial class SnIdentity
    {
        internal static SnIdentity Create(int nodeId)
        {
            var node = Node.LoadNode(nodeId);
            if (node == null)
                throw new ApplicationException("Node not found. Id: " + nodeId);

            string name = node.Name;
            SnIdentityKind kind = SnIdentityKind.User;
            var nodeAsUser = node as IUser;
            if (nodeAsUser != null)
            {
                name = nodeAsUser.FullName;
                kind = SnIdentityKind.User;
            }
            else
            {
                var nodeAsGroup = node as IGroup;
                if (nodeAsGroup != null)
                {
                    kind = SnIdentityKind.Group;
                }
                else
                {
                    var nodeAsOrgUnit = node as IOrganizationalUnit;
                    if (nodeAsOrgUnit != null)
                    {
                        kind = SnIdentityKind.OrganizationalUnit;
                    }
                    else
                    {
                        throw new ApplicationException(String.Concat("Cannot create SnIdentity from NodeType ", node.NodeType.Name, ". Path: ", node.Path));
                    }
                }
            }

            return new SnIdentity
            {
                NodeId = nodeId,
                Path = node.Path,
                Name = name,
                Kind = kind
            };
        }

    }
}
