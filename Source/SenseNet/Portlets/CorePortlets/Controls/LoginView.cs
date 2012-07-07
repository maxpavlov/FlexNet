using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository;
using SenseNet.Portal.Virtualization;
using SenseNet.Portal.UI.Controls;
using SenseNet.Diagnostics;
using System.Collections.Generic;

namespace SenseNet.Portal.Portlets.Controls
{
    public partial class LoginView : UserControl
    {
        public event EventHandler OnUserLoggingIn;
        public event EventHandler OnUserLoggedIn;
        public event EventHandler OnUserLoggedOut;

        public string DefaultDomain { get; set; }

        internal bool _ssoEnabled;
        internal string  _ssoCookieName;
        private string _message;

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            var loginView = this.FindControl("LoginViewControl");

            if (loginView != null)
            {

                if (!User.Current.IsAuthenticated)
                    BindLoginEvents();

                if (User.Current.IsAuthenticated)
                    BindLoggedOut();

            }
        }
        private void BindLoginEvents()
        {
            var login = this.FindControl("LoginViewControl").FindControl("LoginControl") as Login;
            
            if (login != null)
            {
                login.LoggingIn += LoginControl_loggingIn;
                login.LoggedIn += LoginControll_LoggedIn;
                login.LoginError += new EventHandler(Login_LoginError);
                //login.Authenticate += new AuthenticateEventHandler(Login_Authenticate);
            }
        }
        private void BindLoggedOut()
        {
            var loginStatus = this.FindControl("LoginViewControl").FindControl("LoginStatusControl") as LoginStatus;
            if (loginStatus == null)
                return;

            if (PortalContext.Current.AuthenticationMode == "Windows")
            {
                loginStatus.Visible = false;
            }
            else
            {
                loginStatus.LoggingOut += LoginStatus_LoggingOut;
                loginStatus.LoggedOut += LoginStatus_LoggedOut;
            }
        }

        protected void LoginControl_loggingIn(object sender, LoginCancelEventArgs e)
        {
            var login = sender as Login;

            if (login != null && login.UserName.IndexOf("\\") == -1)
            {
                var domain = (String.IsNullOrEmpty(this.DefaultDomain) ?
                    System.Web.Configuration.WebConfigurationManager.AppSettings["DefaultDomain"] :
                    this.DefaultDomain) ??
                    string.Empty;

                login.UserName = string.Concat(domain, "\\", login.UserName);
            }

            if (OnUserLoggingIn != null)
                OnUserLoggingIn(sender, e);

            if (login != null)
            {
                var info = new CancellableLoginInfo { UserName = login.UserName };
                LoginExtender.OnLoggingIn(info);
                e.Cancel = info.Cancel;
                login.UserName = info.UserName;
                _message = info.Message;
            }
        }
        protected void LoginControll_LoggedIn(object sender, EventArgs e)
        {
            var targetUrl = GetPostLoginUrl();
            var userName = ((Login)sender).UserName;
            if (this._ssoEnabled)
            {
                this.GetCookie().Value = CryptoApi.Crypt(userName, "sensenet60beta1", "SenseNetContentRepository");
            }

            if (OnUserLoggedIn != null)
                OnUserLoggedIn(sender, e);

            Logger.WriteAudit(AuditEvent.LoginSuccessful, new Dictionary<string, object> { { "UserName", userName }, { "ClientAddress", Request.ServerVariables["REMOTE_ADDR"] } });

            LoginExtender.OnLoggedIn(new LoginInfo { UserName = userName });

            HttpContext.Current.Response.Redirect(targetUrl);
        }
        protected void Login_LoginError(object sender, EventArgs e)
        {
            var login = sender as Login;

            var userNameControl = this.FindControlRecursive("UserName");
            var userNameTextBox = userNameControl as TextBox;
            string userName = null;
            if (userNameTextBox != null)
            {
                userName = userNameTextBox.Text;
                if (!userName.Contains("\\"))
                {
                    //add default domain for logging reasons
                    var domain = (String.IsNullOrEmpty(this.DefaultDomain) ?
                        System.Web.Configuration.WebConfigurationManager.AppSettings["DefaultDomain"] :
                        this.DefaultDomain) ?? string.Empty;

                    userName = string.Concat(domain, "\\", userName);
                }

                Logger.WriteAudit(AuditEvent.LoginUnsuccessful, new Dictionary<string, object> { { "UserName", userName }, { "ClientAddress", Request.ServerVariables["REMOTE_ADDR"] } });
            }

            var info = new LoginInfo { UserName = userName, Message = login.FailureText };
            LoginExtender.OnLoginError(info);
            _message = info.Message;
        }
        //protected void Login_Authenticate(object sender, AuthenticateEventArgs e)
        //{
        //    e.Authenticated = true;
        //}

        protected void LoginStatus_LoggingOut(object sender, LoginCancelEventArgs e)
        {
            var info = new CancellableLoginInfo { UserName = User.Current.Username };
            LoginExtender.OnLoggingOut(info);
            e.Cancel = info.Cancel;
            _message = info.Message;
        }
        protected void LoginStatus_LoggedOut(object sender, EventArgs e)
        {
            Logger.WriteAudit(AuditEvent.Logout, new Dictionary<string, object> { { "UserName", User.Current.Username }, { "ClientAddress", Request.ServerVariables["REMOTE_ADDR"] } });
            if (OnUserLoggedOut != null)
                OnUserLoggedOut(sender, e);
            LoginExtender.OnLoggedOut(new LoginInfo { UserName = User.Current.Username });
        }

        //===============================================================================================

        protected override void OnPreRender(EventArgs e)
        {
            var login = this.FindControl("LoginViewControl").FindControl("LoginControl") as Login;
            if (login != null)
            {
                string defaultDomain = this.DefaultDomain;
                string userNameWithDomain = login.UserName;
                int removeStringCount = userNameWithDomain.IndexOf("\\");

                if (removeStringCount != -1)
                {
                    string domain = userNameWithDomain.Substring(0, userNameWithDomain.IndexOf("\\"));

                    if (domain.Equals(defaultDomain))
                        login.UserName = userNameWithDomain.Remove(0, removeStringCount + 1);
                }

                if (_message != null)
                {
                    var msgControl = login.FindControl("FailureText") as Label;
                    if (msgControl != null)
                        msgControl.Text = _message;
                }
            }

            base.OnPreRender(e);
        }

        private string GetPostLoginUrl()
        {
            var originalUrl = HttpUtility.UrlDecode(HttpContext.Current.Request.QueryString["OriginalUrl"]);

            if (string.IsNullOrEmpty(originalUrl))
            {
                var login = this.FindControl("LoginViewControl").FindControl("LoginControl") as Login;
                if (login != null) 
                    originalUrl = login.DestinationPageUrl;
            }

            if (string.IsNullOrEmpty(originalUrl))
                originalUrl = PortalContext.Current.RequestedUri.ToString();

            return originalUrl;
        }

        private string GetDomain()
        {
            string host = Context.Request.ServerVariables["HTTP_HOST"];
            string[] hostparts = host.Split('.');
            return (hostparts.Length > 2 ? string.Join(".", hostparts, hostparts.Length - 2, 2) : host);
        }

        private HttpCookie GetCookie()
        {
            var cookie = Context.Response.Cookies[_ssoCookieName];
            if (cookie == null)
            {
                
                cookie = new HttpCookie(_ssoCookieName);
                Context.Response.Cookies.Add(cookie);
            }
            cookie.Domain = GetDomain();
            return cookie;
        }

    }
}