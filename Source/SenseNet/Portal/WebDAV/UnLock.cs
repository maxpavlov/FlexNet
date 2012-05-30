using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Storage;
using SenseNet.Diagnostics;
using SenseNet.ContentRepository;

namespace SenseNet.Services.WebDav
{
    public class UnLock : IHttpMethod
    {
        private WebDavHandler _handler;

        public UnLock(WebDavHandler handler)
        {
            _handler = handler;
        }

        public void HandleMethod()
        {
            _handler.Context.Response.StatusCode = 204;
            _handler.Context.Response.Flush();
        }
    }
}
