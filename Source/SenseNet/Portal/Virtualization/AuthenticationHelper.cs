using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SenseNet.Portal.Virtualization
{
    public static class AuthenticationHelper
    {
        public static void DenyAccess(HttpApplication application)
        {
            application.Context.Response.Clear();
            application.Context.Response.Status = "401 Unauthorized";
            application.Context.Response.StatusCode = 401;
            application.Context.Response.End();
        }

    }
}
