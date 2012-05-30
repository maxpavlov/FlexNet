using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using System.DirectoryServices;

namespace SenseNet.DirectoryServices
{
    public sealed class SyncTree : IDisposable
    {
        /* =============================================================================== Properties */
        private SyncConfiguration _config;
        public SyncConfiguration Config
        {
            get { return _config; }
            set { _config = value; }
        }

        private string _ADPath;
        public string ADPath
        {
            get { return _ADPath; }
            set { _ADPath = value; }
        }

        private string _portalPath;
        public string PortalPath
        {
            get { return _portalPath; }
            set { _portalPath = value; }
        }

        private string _IPAddress;
        public string IPAddress
        {
            get { return _IPAddress; }
            set { _IPAddress = value; }
        }

        private string _deletedADObjectsPath;
        public string DeletedADObjectsPath
        {
            get { return _deletedADObjectsPath; }
            set { _deletedADObjectsPath = value; }
        }

        private List<string> _ADExceptions;
        public List<string> ADExceptions
        {
            get { return _ADExceptions; }
            set { _ADExceptions = value; }
        }

        private List<string> _portalExceptions;
        public List<string> PortalExceptions
        {
            get { return _portalExceptions; }
            set { _portalExceptions = value; }
        }

        private bool _syncGroups;
        public bool SyncGroups
        {
            get { return _syncGroups; }
            set { _syncGroups = value; }
        }

        //private bool _createdADUsersDisabled;
        //public bool CreatedADUsersDisabled
        //{
        //    get { return _createdADUsersDisabled; }
        //    set { _createdADUsersDisabled = value; }
        //}

        // ie: LDAP://192.168.0.75/
        public string ServerPath
        {
            get
            {
                return string.Format("LDAP://{0}/", this._IPAddress);
            }
        }

        private SearchResultCollection _allADUsers;
        public SearchResultCollection AllADUsers
        {
            get
            {
                if (_allADUsers == null)
                {
                    string sFilter = "(&(objectCategory=Person)(objectClass=user)(cn=*))";
                    
                    // NOVELL - no such property named "objectcategory"
                    if (_config.NovellSupport)
                        sFilter = "objectClass=Person";

                    // only enabled users:
                    //string sFilter = "(&(objectCategory=Person)(objectClass=user)(cn=*)(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectoryEntry root = ConnectToObject(this.ADPath))
                    {
                        _allADUsers = Common.Search(root, sFilter, _config.NovellSupport, _config.GuidProp);
                    }
                }
                return _allADUsers;
            }
        }

        private SearchResultCollection _allADContainers;
        public SearchResultCollection AllADContainers
        {
            get
            {
                if (_allADContainers == null)
                {
                    string sFilter = "(|(objectClass=Organization)(objectClass=organizationalUnit)(objectClass=domain)(objectClass=container))";
                    using (DirectoryEntry root = ConnectToObject(this.ADPath))
                    {
                        _allADContainers = Common.Search(root, sFilter, _config.NovellSupport, _config.GuidProp);
                    }
                }
                return _allADContainers;
            }
        }

        private SearchResultCollection _allADGroups;
        public SearchResultCollection AllADGroups
        {
            get
            {
                if (_allADGroups == null)
                {
                    string sFilter = "objectClass=group";
                    using (DirectoryEntry root = ConnectToObject(this.ADPath))
                    {
                        _allADGroups = Common.Search(root, sFilter, _config.NovellSupport, _config.GuidProp);
                    }
                }
                return _allADGroups;
            }
        }


        /* =============================================================================== AD Methods */
        public DirectoryEntry ConnectToObject(string objectPath)
        {
            string sLDAPPath = objectPath;

            if (!sLDAPPath.StartsWith("LDAP://"))
            {
                sLDAPPath = String.Concat("LDAP://", this.IPAddress, "/", sLDAPPath.Replace("/", "\\/"));
            }


            return Common.ConnectToAD(sLDAPPath, _config.CustomADAdminAccountName, _config.CustomADAdminAccountPwd, _config.NovellSupport, _config.GuidProp);
        }
        public SearchResultCollection GetUsersUnderADObject(DirectoryEntry searchRoot)
        {
            string sFilter = "(&(objectCategory=Person)(objectClass=user)(cn=*))";
            return Common.Search(searchRoot, sFilter, _config.NovellSupport, _config.GuidProp);
        }
        public DirectoryEntry GetADObjectByGuid(Guid guid)
        {
            var filter = string.Format("{0}={1}", _config.GuidProp, Common.Guid2OctetString(guid));
            using (DirectoryEntry root = ConnectToObject(this.ADPath))
            {
                return Common.SearchADObject(root, filter, _config.NovellSupport, _config.GuidProp);
            }
        }
        public SearchResultCollection SearchADObjectByGuid(Guid guid)
        {
            var filter = string.Format("{0}={1}", _config.GuidProp, Common.Guid2OctetString(guid));
            using (DirectoryEntry root = ConnectToObject(this.ADPath))
            {
                return Common.Search(root, filter, _config.NovellSupport, _config.GuidProp);
            }
        }
        public bool IsADPathExcluded(string objectADPath)
        {
            foreach (string exception in ADExceptions)
            {
                if (objectADPath.EndsWith(exception))
                    return true;
            }
            return false;
        }
        public bool IsPortalPathExcluded(string objectPortalPath)
        {
            foreach (string exception in PortalExceptions)
            {
                if (objectPortalPath.StartsWith(exception))
                    return true;
            }
            return false;
        }
        // gets if the synctree contains the given AD path
        public bool ContainsADPath(string objectADPath)
        {
            // objectADPath pl.: LDAP://192.168.0.75/OU=MyOrg,OU=OtherOrg,OU=ExampleOrg,DC=Nativ,DC=local

            //if (objectADPath.StartsWith(ServerPath) && objectADPath.EndsWith(ADPath))
            if (objectADPath.EndsWith(ADPath) && !IsADPathExcluded(objectADPath))
                return true;

            return false;
        }
        public bool ContainsPortalPath(string objectPortalPath)
        {
            // objectPortalPath pl.: /Root/IMS/NATIV/ExampleOrg
            if (objectPortalPath.StartsWith(PortalPath) && !IsPortalPathExcluded(objectPortalPath))
                return true;

            return false;
        }
        public string GetPortalPath(string objectADPath)
        {
            // objectADPath pl.: LDAP://192.168.0.75/OU=OtherOrg,OU=ExampleOrg,DC=Nativ,DC=local
            // ADPath pl.: "OU=ExampleOrg,DC=Nativ,DC=Local"
            // PortalPath pl.: "/Root/IMS/ExampleOrg"

            if (!this.ContainsADPath(objectADPath))
                return null;

            // trim serverpath from beginning
            string path = objectADPath;
            if (path.StartsWith(ServerPath))
                path = path.Substring(ServerPath.Length, path.Length - this.ServerPath.Length); // OU=OtherOrg,OU=ExampleOrg,DC=Nativ,DC=local

            // trim adpath from end
            path = path.Substring(0, path.Length - this.ADPath.Length); // OU=OtherOrg,

            string[] directories = path.Split(new string[] { "OU=", "CN=", "ou=", "cn=" }, StringSplitOptions.RemoveEmptyEntries);
            string objectPortalPath = string.Empty;

            foreach (string dir in directories)
            {
                objectPortalPath = RepositoryPath.Combine(Common.StripADName(dir), objectPortalPath);
            }

            // pl.: /Root/IMS/ExampleOrg/OtherOrg
            var objectPath = RepositoryPath.Combine(this.PortalPath, objectPortalPath).TrimEnd(new char[] { '/' });
            return objectPath;
        }
        // gets the portal parentpath for a given AD object path
        public string GetPortalParentPath(string objectADPath)
        {
            var objectPath = GetPortalPath(objectADPath);
            if (objectPath == null)
                return null;

            // pl.: /Root/IMS/ExampleOrg
            return RepositoryPath.GetParentPath(objectPath);
        }
        public string GetADPath(string objectPortalPath)
        {
            // objectPortalPath pl.: /Root/IMS/ExampleOrg/OtherOrg
            // ADPath pl.: "OU=ExampleOrg,DC=Nativ,DC=Local"
            // PortalPath pl.: "/Root/IMS/ExampleOrg"

            if (!this.ContainsPortalPath(objectPortalPath))
                return null;

            string path = objectPortalPath.Substring(PortalPath.Length).Trim(new char[] { '/' }); // /OtherOrg/MyOrg

            // go through path elements and add them one-by-one to the output path
            string actPortalPath = PortalPath;
            string actADPath = ADPath;
            foreach (string pathPart in path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
            {
                actPortalPath = RepositoryPath.Combine(actPortalPath, pathPart);
                var adObjName = Common.GetADObjectNameFromPath(actPortalPath);
                actADPath = Common.CombineADPath(actADPath, adObjName);
            }

            // pl.: OU=OtherOrg,OU=ExampleOrg,DC=Nativ,DC=Local
            return actADPath;
        }
        public string GetADParentObjectPath(string objectADPath)
        {
            // objectADPath pl.: LDAP://192.168.0.75/OU=OtherOrg,OU=ExampleOrg,DC=Nativ,DC=local

            string path = objectADPath;
            
            if (objectADPath.StartsWith(ServerPath))
                path = objectADPath.Substring(ServerPath.Length, objectADPath.Length - ServerPath.Length); //OU=OtherOrg,OU=ExampleOrg,DC=Nativ,DC=local

            string parentPath = path.Substring(path.IndexOf(",") + 1);

            // parent objektum path
            return parentPath;              //ExampleOrg,DC=Nativ,DC=local
        }


        /*================================================================================== Disposable */
        private bool disposed = false;
        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); 
        }

        #endregion
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (_allADContainers != null)
                        _allADContainers.Dispose();
                    if (_allADUsers != null)
                        _allADUsers.Dispose();
                    if (_allADGroups != null)
                        _allADGroups.Dispose();
                }
                // Release unmanaged resources. If disposing is false, 
                // only the following code is executed.
            }
            disposed = true;
        }
        // Use C# destructor syntax for finalization code.
        ~SyncTree()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }
    }
}
