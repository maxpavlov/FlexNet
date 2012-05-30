////////////////////////////////////////////////////////////////////////////////////////
//
// !!!
//
// BEFORE executing the program, copy JobHttpHandlerProxy.ashx to the web folder
//
// !!!
//
////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SenseNet.Utilities.ExecutionTesting;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Security;

namespace ConcurrencyTester
{
    /// <summary>
    /// Serves as a proxy class between SN6 and the OperationTesting framework
    /// </summary>
    public class JobHttpHandlerProxy : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            var jobHttpHandler = new JobHttpHandler
                                     {
                                         AuthenticationAction = (domain, username) =>
                                                                    {
                                                                        var u = User.Load(domain, username);
                                                                        HttpContext.Current.User =
                                                                            new PortalPrincipal(u ?? User.Visitor);
                                                                    }
                                     };

            jobHttpHandler.ProcessRequest(context);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
