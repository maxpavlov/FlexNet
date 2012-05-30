using System;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using System.Security;
using SenseNet.Diagnostics;

namespace SenseNet.Services.WebDav
{
    public class Move : IHttpMethod
    {
        private WebDavHandler _handler;
        public Move(WebDavHandler handler)
        {
            _handler = handler;
        }

        #region IHttpMethod Members

        public void HandleMethod()
        {
            bool overwrite = false;
            string origPath = _handler.GlobalPath;
            string destPath = _handler.Context.Server.UrlDecode(_handler.Context.Request.Headers["Destination"]);
            string urlRoot = _handler.Protocol + _handler.Host;
            destPath = _handler.GetGlobalPath(destPath.Substring(urlRoot.Length));
            
            if (_handler.Context.Request.Headers["Overwrite"] != null && _handler.Context.Request.Headers["Overwrite"] == "T")
                overwrite = true;

            try
            {
                var destNode = Node.LoadNode(destPath);
                if (overwrite || destNode == null)
                {
                    var origName = RepositoryPath.GetFileName(origPath);
                    var destName = RepositoryPath.GetFileName(destPath);
                    var origNode = Node.LoadNode(_handler.GlobalPath);

                    // check if moving
                    if (RepositoryPath.GetParentPath(destPath) != RepositoryPath.GetParentPath(origPath))
                    {
                        // move node to destination directory
                        string parentPath = RepositoryPath.GetParentPath(destPath);
                        origNode.MoveTo(Node.LoadNode(parentPath));
                    }
                    // renaming
                    if (origName != destName)
                    {
                        origNode.Name = RepositoryPath.GetFileName(destPath);
                        origNode.Save();
                    }

                    _handler.Context.Response.StatusCode = 201;
                    _handler.Context.Response.Flush();
                }
                else
                {
                    _handler.Context.Response.StatusCode = 409;
                }
            }
            catch (SecurityException e) //logged
            {
                Logger.WriteException(e);
                _handler.Context.Response.StatusCode = 403;
            }
            catch (SenseNetSecurityException ee) //logged
            {
                Logger.WriteException(ee);
                _handler.Context.Response.StatusCode = 403;
            }
            catch (Exception eee) //logged
            {
                Logger.WriteException(eee);
                _handler.Context.Response.StatusCode = 409;
            }
        }
        
        #endregion
    }
}
