using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Web.Mvc;

namespace SenseNet.Services.SupportTypes
{
    public class JsonFilter : ActionFilterAttribute
    {
        public string Param { get; set; }
        public Type DataType { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Request.ContentType.Contains("application/json"))
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(DataType);
                var data = ser.ReadObject(filterContext.HttpContext.Request.InputStream);
                filterContext.ActionParameters[Param] = data;
            }
        }
    }
}
