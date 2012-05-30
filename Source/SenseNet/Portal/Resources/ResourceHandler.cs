using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using SenseNet.ContentRepository;
using System.IO;
using SenseNet.ContentRepository.i18n;
using System.Globalization;

namespace SenseNet.Portal
{
    /// <summary>
    /// When /Resource.ashx?class=xy is requested, a javascript variable is defined
    /// </summary>
    public class ResourceHandler : IHttpHandler
    {
        //================================================================================= IHttpHandler
        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var className = context.Request["class"];

            if (string.IsNullOrEmpty(className))
            {
                DenyRequest(context);
                return;
            }

            context.Response.ContentType = "text/javascript";

            var cultureInfo = CultureInfo.CurrentUICulture.Parent;
            var classItems = SenseNetResourceManager.Current.GetClassItems(className, cultureInfo);
            if (classItems == null)
            {
                DenyRequest(context);
                return;
            }

            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                var sb = new StringBuilder();
                foreach (var key in classItems.Keys)
                {
                    var value = classItems[key].ToString();
                    
                    // end-of-line is not yet supported
                    value = value.Replace("\n", "").Replace("\r", "");

                    // quotation is not yet supported
                    value = value.Replace("\"", "");

                    sb.Append(key + ":\"" + value + "\",");
                }
                var keyvalues = sb.ToString();
                keyvalues = keyvalues.TrimEnd(',');

                writer.Write("var SN=SN || {}; SN.Resources=SN.Resources || {}; SN.Resources." + className + "={" + keyvalues + "};");
            }
            context.Response.OutputStream.Flush();
        }

        private void DenyRequest(HttpContext context)
        {
            context.Response.End();
        }
    }
}
