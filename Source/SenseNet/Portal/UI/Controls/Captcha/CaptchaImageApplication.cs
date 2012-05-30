using System;
using System.Web;
using System.Drawing;
using SenseNet.ApplicationModel;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.Portal.UI.Controls.Captcha
{
    [ContentHandler]
    public class CaptchaImageApplication : Application, IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public CaptchaImageApplication(Node parent) : this(parent, "CaptchaImageApplication") { }
        public CaptchaImageApplication(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected CaptchaImageApplication(NodeToken nt) : base(nt) { }


        void System.Web.IHttpHandler.ProcessRequest(System.Web.HttpContext context)
        {
            HttpApplication app = context.ApplicationInstance;
            //-- get the unique GUID of the captcha; this must be passed in via the querystring
            string guid = app.Request.QueryString["guid"];
            CaptchaImage ci = null;
            if (guid != "")
            {
                if (string.IsNullOrEmpty(app.Request.QueryString["s"]))
                {
                    ci = (CaptchaImage)HttpRuntime.Cache.Get(guid);
                }
                else
                {
                    ci = (CaptchaImage)HttpContext.Current.Session[guid];
                }
            }
            if (ci == null)
            {
                app.Response.StatusCode = 404;
                context.ApplicationInstance.CompleteRequest();
                return;
            }
            //-- write the image to the HTTP output stream as an array of bytes
            using (Bitmap b = ci.RenderImage())
            {
                b.Save(app.Context.Response.OutputStream, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            app.Response.ContentType = "image/jpeg";
            app.Response.StatusCode = 200;
            context.ApplicationInstance.CompleteRequest();
        }
        bool System.Web.IHttpHandler.IsReusable {
            get {
                return true;
            }
        }
    }
}