using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Hosting;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using System.IO;
using SenseNet.ContentRepository.Storage.Security;
using System.Web;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.Virtualization
{
    class RepositoryFile : VirtualFile
    {
        string _repositoryPath;
        Node _node;
   
        public RepositoryFile(string virtualPath, string repositoryPath)
            : base(virtualPath)
        {
            if (virtualPath.EndsWith(PortalContext.InRepositoryPageSuffix))
            {
                // It's an aspx page, a Page or a Site
                PortalContext currentPortalContext = PortalContext.Current;
                if (currentPortalContext == null)
                {
					throw new ApplicationException(string.Format("RepositoryFile cannot be instantiated. PortalContext.Current is null. virtualPath='{0}', repositoryPath='{1}'.", virtualPath, repositoryPath));
                }
                if (currentPortalContext.Page == null)
                {
					throw new ApplicationException(string.Format("RepositoryFile cannot be instantiated. PortalContext.Current is available, but the PortalContext.Current.Page is null. virtualPath='{0}', repositoryPath='{1}'.", virtualPath, repositoryPath));
                }
                _repositoryPath = currentPortalContext.Page.Path;
            }
            else
                _repositoryPath = virtualPath;
        }

        public override Stream Open()
        {
            if (_node == null)
            {
                try
                {
                    // http://localhost/TestDoc.docx?action=RestoreVersion&version=2.0A
                    // When there are 'action' and 'version' parameters in the requested URL the portal is trying to load the desired version of the node of the requested action. 
                    // This leads to an exception when the action doesn't have that version. 
                    // _repositoryPath will point to the node of action and ContextNode will be the document
                    // if paths are not equal then we will return the last version of the requested action
                    if (PortalContext.Current == null || string.IsNullOrEmpty(PortalContext.Current.VersionRequest) || _repositoryPath != PortalContext.Current.ContextNodePath)
                    {
                        _node = Node.LoadNode(_repositoryPath);
                    }
                    else
                    {
                        VersionNumber version;
                        if (VersionNumber.TryParse(PortalContext.Current.VersionRequest, out version))
                            _node = Node.LoadNode(_repositoryPath, version);
                    }
                }
                catch (SenseNetSecurityException ex) //logged
                {
                    Logger.WriteException(ex);

                    if (HttpContext.Current == null)
                        throw;
                    
                    AuthenticationHelper.DenyAccess(HttpContext.Current.ApplicationInstance);
                }
            }
            
            if (_node == null)
				throw new ApplicationException(string.Format("{0} not found. RepositoryFile cannot be served.", _repositoryPath));

            string propertyName = string.Empty;
            if (PortalContext.Current != null)
                propertyName = PortalContext.Current.QueryStringNodePropertyName;

            if (string.IsNullOrEmpty(propertyName))
                propertyName = PortalContext.DefaultNodePropertyName;

            var propType = _node.PropertyTypes[propertyName];
			if (propType == null)
			{
				throw new ApplicationException("Property not found: " + propertyName);
			}
			////<only_for_test>
			//if(propType == null)
			//{
			//    StringBuilder sb = new StringBuilder();
			//    sb.AppendFormat("Property {0} not found.", propertyName);
			//    sb.AppendLine();
			//    foreach (var pt in _node.PropertyTypes)
			//    {
			//        sb.AppendFormat("PropertyName='{0}' - DataType='{1}'", pt.Name, pt.DataType);
			//        sb.AppendLine();
			//    }
			//    return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(sb.ToString()));
			//}
			////</only_for_test>

            var propertyDataType = propType.DataType;
            Stream stream;

            switch (propertyDataType)
            {
                case DataType.Binary:
					var bd = _node.GetBinary(propertyName);
                    stream = bd.GetStream();
					if (stream == null)
						throw new ApplicationException(string.Format("BinaryProperty.Value.GetStream() returned null. RepositoryPath={0}, OriginalUri={1}, AppDomainFriendlyName={2} ", this._repositoryPath, ((PortalContext.Current != null) ? PortalContext.Current.OriginalUri.ToString() : "PortalContext.Current is null"), AppDomain.CurrentDomain.FriendlyName));

                    //let the client code log file downloads
                    var file = _node as ContentRepository.File;
                    if (file != null)
                        ContentRepository.File.Downloaded(file.Id);
					break;
                case DataType.String:
                case DataType.Text:
                case DataType.Int:
                case DataType.DateTime:
                    stream = new MemoryStream(Encoding.UTF8.GetBytes(_node[propertyName].ToString()));
                    break;
                default:
                    throw new NotSupportedException(string.Format("The {0} property cannot be served because that's datatype is {1}.", propertyName, propertyDataType));
            }

            return stream;
        }
    }
}