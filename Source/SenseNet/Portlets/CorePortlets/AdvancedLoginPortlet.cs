using System;
using System.ComponentModel;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Services.Instrumentation;
using PortletControls = SenseNet.Portal.Portlets.Controls;
using SenseNet.Diagnostics;
using SenseNet.ContentRepository.Storage;
using System.Collections.Generic;
using System.Diagnostics;

namespace SenseNet.Portal.Portlets
{
    public class AdvancedLoginPortlet : PortletBase
    {
        private string _defaultDomainPrefix;


        private string _loginViewPath = "/Root/System/SystemPlugins/Portlets/AdvancedLogin/LoginView.ascx";

        /// <summary>
        /// Gets or sets the user interface path of the LoginView state of the portlet.
        /// </summary>
        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Login view path")]
        [WebDescription("Path of the .ascx user control which provides the UI elements of the portlet")]
        [WebCategory(EditorCategory.Login, EditorCategory.Login_Order)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        public string LoginViewPath
        {
            get { return _loginViewPath; }
            set { _loginViewPath = value; }
        }

        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(false), Personalizable(true)]
        public override string Renderer { get; set; }


        /// <summary>
        /// Gets or sets the default domain prefix of the user. If it is empty, the default domain will be used which is set in the web.config.
        /// </summary>
        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Default domain prefix")]
        [WebDescription("You can override default domain prefix settings defined in the web.config")]
        [WebCategory(EditorCategory.Login, EditorCategory.Login_Order)]
        public string DefaultDomainPrefix
        {
            get { return _defaultDomainPrefix; }
            set { _defaultDomainPrefix = value; }
        }

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Enable Single Sign-On (SSO)")]
        [WebDescription("Set this to true to enable SSO behavior")]
        [WebCategory(EditorCategory.Login, EditorCategory.Login_Order)]
        public bool SSOEnabled { get; set; }

        [WebBrowsable(true), Personalizable(true), DefaultValue("sso-token")]
        [WebDisplayName("SSO-token")]
        [WebDescription("Cookie name used for SSO")]
        [WebCategory(EditorCategory.Login, EditorCategory.Login_Order)]
        public string SSOCookieName { get; set; }


        public AdvancedLoginPortlet()
        {
            this.Name = "Login";
            this.Description = "Users can log in and out using this portlet";
            this.Category = new PortletCategory(PortletCategoryType.Portal);
        }


        private static string SignOffCommand = "SignOff";

        protected override void CreateChildControls()
        {
            Controls.Clear();
            bool redirecting = HandleSSOSignOffCommand();
            if (!redirecting)
            {
                CreateLoginView();
            }
            ChildControlsCreated = true;
        }

        #region SSOSignOff

        private bool HandleSSOSignOffCommand()
        {
            bool redirecting = false;
            string originalUrl = HttpContext.Current.Request.QueryString["OriginalUrl"];
            bool isSignOff = HttpContext.Current.Request.QueryString["Command"] == AdvancedLoginPortlet.SignOffCommand;
            if (isSignOff)
            {
                //string OriginalUrl = 
                FormsAuthentication.SignOut();
                GetCookie().Value = string.Empty;
                redirecting = !string.IsNullOrEmpty(originalUrl);
                if (redirecting)
                    HttpContext.Current.Response.Redirect(originalUrl);
            }
            return redirecting;
        }


        private string GetDomain()
        {
            string host = HttpContext.Current.Request.ServerVariables["HTTP_HOST"];
            string[] hostparts = host.Split('.');
            return (hostparts.Length > 2 ? string.Join(".", hostparts, hostparts.Length - 2, 2) : host);
        }

        private HttpCookie GetCookie()
        {
            HttpCookie cookie = HttpContext.Current.Response.Cookies[SSOCookieName];
            if (cookie == null)
            {
                cookie = new HttpCookie(SSOCookieName);
                Context.Response.Cookies.Add(cookie);
            }
            cookie.Domain = GetDomain();
            return cookie;
        }

        #endregion

        protected virtual void CreateLoginView()
        {
            var loginViewPath = this.LoginViewPath;

            PortletControls.LoginView lw = null;

            try
            {
                lw = this.Page.LoadControl(loginViewPath) as PortletControls.LoginView;
                lw._ssoEnabled = this.SSOEnabled;
                lw._ssoCookieName = this.SSOCookieName;

                if (!String.IsNullOrEmpty(this.DefaultDomainPrefix))
                    lw.DefaultDomain = this.DefaultDomainPrefix;
            }
            catch (Exception exc) //logged
            {
                WriteErrorMessageOnly(String.Format("Couldn't load {0}", loginViewPath));
                //WriteErrorMessageOnly(String.Format("{0} {1} {2}",exc.Message, exc.Source, exc.StackTrace));
                Logger.WriteException(exc);
                return;
            }

            this.Controls.Add(lw);
        }


        /// <summary>
        /// This method only displays the given message. Every constructed control will be removed from the ctlcoll.
        /// </summary>
        /// <param name="message">The message will be displayed to the end user.</param>
        protected virtual void WriteErrorMessageOnly(string message)
        {
            this.Controls.Clear();
            this.Controls.Add(new LiteralControl(message));
        }
    }

    public class LoginManager
    {
        private static object _sync;
        private static LoginManager __instance;
        public static LoginManager Instance
        {
            get
            {
                if (__instance == null)
                    lock (_sync)
                        if (__instance == null)
                            __instance = CreateInstance();
                return __instance;
            }
        }
        private LoginManager() { }
        private static LoginManager CreateInstance()
        {
            var x = TypeHandler.GetTypesByBaseType(typeof(LoginExtender));
            throw new NotImplementedException();
        }

    }

    public class LoginInfo
    {
        public string UserName { get; set; }
        public string Message { get; set; }
    }
    public class CancellableLoginInfo : LoginInfo
    {
        private bool _cancel;
        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel |= value; }
        }
    }
    public abstract class LoginExtender
    {
        public virtual void LoggingIn(CancellableLoginInfo info)
        {
            // do nothing
        }
        public virtual void LoggedIn(LoginInfo info)
        {
            // do nothing
        }
        public virtual void LoggingOut(CancellableLoginInfo info)
        {
            // do nothing
        }
        public virtual void LoggedOut(LoginInfo info)
        {
            // do nothing
        }
        public virtual void LoginError(LoginInfo info)
        {
            // do nothing
        }

        //==========================================================

        private static object _sync = new object();
        private static Type[] __extenderTypes;
        private static Type[] ExtenderTypes
        {
            get
            {
                if (__extenderTypes == null)
                    lock (_sync)
                        if (__extenderTypes == null)
                            __extenderTypes = TypeHandler.GetTypesByBaseType(typeof(LoginExtender));
                return __extenderTypes;
            }
        }

        internal static void OnLoggingIn(CancellableLoginInfo info)
        {
            foreach (var extender in GetLoginExtenders())
                extender.LoggingIn(info);
        }
        internal static void OnLoggedIn(LoginInfo info)
        {
            foreach (var extender in GetLoginExtenders())
                extender.LoggedIn(info);
        }
        internal static void OnLoggingOut(CancellableLoginInfo info)
        {
            foreach (var extender in GetLoginExtenders())
                extender.LoggingOut(info);
        }
        internal static void OnLoggedOut(LoginInfo info)
        {
            foreach (var extender in GetLoginExtenders())
                extender.LoggedOut(info);
        }
        internal static void OnLoginError(LoginInfo info)
        {
            foreach (var extender in GetLoginExtenders())
                extender.LoginError(info);
        }
        private static IEnumerable<LoginExtender> GetLoginExtenders()
        {
            foreach (var extenderType in ExtenderTypes)
            {
                var extender = (LoginExtender)Activator.CreateInstance(extenderType);
                yield return extender;
            }
        }
    }
}