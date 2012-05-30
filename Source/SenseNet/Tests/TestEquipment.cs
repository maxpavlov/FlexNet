using System;
using System.Collections.Generic;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Schema;

namespace SenseNet.ContentRepository.Tests
{
    internal static class TestEquipment
    {
        internal class PermissionDescriptor
        {
            public string AffectedPath { get; set; }
            public IUser AffectedUser { get; set; }
            public PermissionType PType { get; set; }
            public PermissionValue NewValue { get; set; }
            public PermissionValue OldValue { get; set; }

            public PermissionDescriptor()
            {
                
            }
            
            public PermissionDescriptor(string contentPath, IUser user, PermissionType permissionType, PermissionValue permissionValue)
            {
                AffectedPath = contentPath;
                AffectedUser = user;
                PType = permissionType;
                NewValue = permissionValue;
            }
        }

        internal class ContextSimulator : IDisposable
        {
            private readonly IEnumerable<PermissionDescriptor> _descriptors;

            public ContextSimulator(IUser actualUser)
            {
                AccessProvider.Current.SetCurrentUser(actualUser);
            }
            
            public ContextSimulator(IEnumerable<PermissionDescriptor> descriptors, IUser actualUser)
            {
                    foreach (var descriptor in descriptors)
                    {
                        //load effected node
                        var node = Node.LoadNode(descriptor.AffectedPath);

                        //save the original value, before the modification
                        descriptor.OldValue = node.Security.GetPermission(descriptor.AffectedUser, descriptor.PType);

                        //set the new value
                        node.Security.SetPermission(descriptor.AffectedUser, true, descriptor.PType, descriptor.NewValue);

                        //save changes
                        node.Save();
                    }

                    _descriptors = descriptors;
                
                AccessProvider.Current.SetCurrentUser(actualUser);
            }

            #region IDisposable Members

            public void Dispose()
            {
                AccessProvider.Current.SetCurrentUser(User.Administrator);

                if (_descriptors == null) return;
                foreach (var descriptor in _descriptors)
                {
                    //load effected node
                    var node = Node.LoadNode(descriptor.AffectedPath);

                    //set the original value
                    node.Security.SetPermission(descriptor.AffectedUser, true, descriptor.PType, descriptor.OldValue);

                    node.Save();
                }
            }

            #endregion
        }

        // --- static methods
        // ------------------
        internal static void SetPermissions(IEnumerable<PermissionDescriptor> descriptors)
        {
            foreach (var descriptor in descriptors)
            {
                //load effected node
                var node = Node.LoadNode(descriptor.AffectedPath);

                //set the new value
                node.Security.SetPermission(descriptor.AffectedUser, true, descriptor.PType, descriptor.NewValue);

                //save changes
                node.Save();
            }
        }

        internal static int EnsureNode(string path)
        {
            if (Node.Exists(path))
                return -1;

            var name = RepositoryPath.GetFileName(path);
            var parentPath = RepositoryPath.GetParentPath(path);

            EnsureNode(parentPath);

            var type = GetLeadingChars(name);

            if (ContentType.GetByName(type) == null)
                throw new InvalidOperationException(String.Concat(type + " type doesn't exist!"));

            switch (type)
            {
                default:
                    CreateNode(parentPath, name, type);
                    break;
            }

            var node = Node.LoadNode(path);
            return node.Id;
        }

        private static void CreateNode(string parentPath, string name, string typeName)
        {
            var parent = Content.Load(parentPath);
            var content = Content.CreateNew(typeName, parent.ContentHandler, name);

            content.Save();
        }
        
        private static string GetLeadingChars(string input)
        {
            var chars = input.ToCharArray();

            var lastValid = -1;

            for (int i = 0; i < chars.Length; i++)
            {
                if (Char.IsLetter(chars[i]))
                    lastValid = i;
                else
                    break;
            }

            return new string(chars, 0, lastValid + 1);
        }
    }
}
