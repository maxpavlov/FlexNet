using System;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;

namespace SenseNet.Services.WebDav
{

    public class Head : IHttpMethod
    {
        private WebDavHandler _handler;
        public Head(WebDavHandler handler)
        {
            _handler = handler;
        }

        #region IHttpMethod Members

        public void HandleMethod()
        {
            var node = Node.LoadNode(_handler.GlobalPath);
            var f = node as File;

            if (f == null)
            {
                _handler.Context.Response.StatusCode = 404;
                _handler.Context.Response.Flush();
            }
            else if (!f.Security.HasPermission(PermissionType.Open))
            {
                _handler.Context.Response.StatusCode = 403;
                _handler.Context.Response.Flush();
            }
            else
            {
                _handler.Context.Response.ContentType = "application/octet-stream";

                var eTag = Guid.NewGuid();
                _handler.Context.Response.Cache.SetETag(eTag.ToString());
                _handler.Context.Response.Flush();                
            }

        }

        #endregion
    }
}
