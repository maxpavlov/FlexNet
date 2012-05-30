using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Xml;
//using Microsoft.Web.Administration;
using System.Web.UI.WebControls;

namespace SenseNet.Portal.Setup.IISConfig
{
    public partial class Config : System.Web.UI.Page
    {
        #region Impersonation

        public const int LOGON32_LOGON_INTERACTIVE = 2;
        public const int LOGON32_PROVIDER_DEFAULT = 0;

        WindowsImpersonationContext impersonationContext;

        [DllImport("advapi32.dll")]
        public static extern int LogonUserA(String lpszUserName,
            String lpszDomain,
            String lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            ref IntPtr phToken);
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int DuplicateToken(IntPtr hToken,
            int impersonationLevel,
            ref IntPtr hNewToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool RevertToSelf();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr handle);

        private bool ImpersonateValidUser(string userName, string domain, string password)
        {
            WindowsIdentity tempWindowsIdentity;
            var token = IntPtr.Zero;
            var tokenDuplicate = IntPtr.Zero;

            if (RevertToSelf())
            {
                if (LogonUserA(userName, domain, password, LOGON32_LOGON_INTERACTIVE,
                    LOGON32_PROVIDER_DEFAULT, ref token) != 0)
                {
                    if (DuplicateToken(token, 2, ref tokenDuplicate) != 0)
                    {
                        tempWindowsIdentity = new WindowsIdentity(tokenDuplicate);
                        impersonationContext = tempWindowsIdentity.Impersonate();
                        if (impersonationContext != null)
                        {
                            CloseHandle(token);
                            CloseHandle(tokenDuplicate);

                            //WindowsIdentity.GetCurrent()
                            Thread.CurrentPrincipal = new WindowsPrincipal(tempWindowsIdentity);
                            HttpContext.Current.User = Thread.CurrentPrincipal;

                            return true;
                        }
                    }
                }
            }

            if (token != IntPtr.Zero)
                CloseHandle(token);
            if (tokenDuplicate != IntPtr.Zero)
                CloseHandle(tokenDuplicate);

            return false;
        }

        #endregion

        private static readonly object Locker = new object();
        private const string FolderName = "/IISConfig/";

        //================================================================= Properties

        private string RunOnceMarkerPath
        {
            get { return this.MapPath(FolderName + RunOnce.RunOnceGuid); }
        }

        private string DefaultSiteName
        {
            get { return System.Web.Configuration.WebConfigurationManager.AppSettings["DefaultSiteName"] ?? "Default_Site"; }
        }

        //================================================================= Main methods

        protected void Page_Load(object sender, EventArgs e)
        {
            lock (Locker)
            {
                if (!System.IO.File.Exists(RunOnceMarkerPath)) // || !WindowsAuthenticationMode())
                {
                    RedirectToDefault();
                } 
            }

            //Login1.Authenticate += Login1_Authenticate;
            buttonForms.Click += ButtonForms_Click;

            //temp: we don't allow win auth install
            SetSiteAuthenticationMode("Forms");

            RemoveMarker();
            RedirectToDefault();
        }

        //================================================================= Event handlers

        protected void ButtonForms_Click(object sender, EventArgs e)
        {
            SetSiteAuthenticationMode("Forms");
            //SetRequestFiltering();

            RemoveMarker();
            RedirectToDefault();
        }

        //protected void Login1_Authenticate(object sender, AuthenticateEventArgs e)
        //{
        //    try
        //    {
        //        lock (Locker)
        //        {
        //            Login1.FailureText = "Your login attempt was not successful. Please try again.";

        //            var fullUsername = Login1.UserName;
        //            var slashIndex = fullUsername.IndexOf('\\');
        //            if (slashIndex < 0)
        //            {
        //                //empty domain name? no way.
        //                e.Authenticated = false;
        //                Login1.FailureText = "Please give your credentials in a domain\\username form";
        //                ShowLoginPanel();

        //                return;
        //            }
                    
        //            var domain = fullUsername.Substring(0, slashIndex);
        //            var username = fullUsername.Substring(slashIndex + 1);

        //            if (!ImpersonateValidUser(username, domain, Login1.Password))
        //            {
        //                //without authentication? no way.
        //                e.Authenticated = false;
        //                ShowLoginPanel();

        //                return;
        //            }

        //            var siteName = System.Web.Hosting.HostingEnvironment.ApplicationHost.GetSiteName();

        //            using (var sm = new ServerManager())
        //            {
        //                var app = sm.GetApplicationHostConfiguration();
        //                var locSecAno = app.GetSection("system.webServer/security/authentication/anonymousAuthentication", siteName);
        //                var locSecWin = app.GetSection("system.webServer/security/authentication/windowsAuthentication", siteName);
        //                //var secAscx = app.GetSection("system.webServer/security/requestFiltering/fileExtensions/add[@fileExtension='.ascx']", siteName);
        //                //secAscx.SetAttributeValue("allowed", false);
        //                locSecAno.SetAttributeValue("enabled", false);
        //                locSecWin.SetAttributeValue("enabled", true);

        //                sm.CommitChanges();

        //                sm.Sites[siteName].Stop();
        //                sm.Sites[siteName].Start();
        //            }

        //            SetSiteAuthenticationMode("Windows");

        //            RemoveMarker();
        //            RedirectToDefault();
        //        }    
        //    }
        //    catch (Exception ex)
        //    {
        //        e.Authenticated = false;
        //    }

        //    ShowLoginPanel();
        //}

        //================================================================= Helper methods

        private void SetSiteAuthenticationMode(string authMode)
        {
            try
            {
                var rootPath = this.MapPath("/Root/Sites");
                var domain = HttpContext.Current.Request.Url.Authority.ToLower();
                var defaultSiteName = DefaultSiteName;
                var foundDefaultSite = false;

                foreach (var file in System.IO.Directory.GetFiles(rootPath))
                {
                    if (!file.ToLower().EndsWith(".content"))
                        continue;

                    var xDoc = new XmlDocument();
                    xDoc.Load(file);

                    var ct = xDoc.SelectSingleNode("ContentMetaData/ContentType");
                    if (ct == null || ct.InnerText != "Site")
                    {
                        continue;
                    }

                    var contentNameNode = xDoc.SelectSingleNode("ContentMetaData/ContentName");
                    if (contentNameNode != null)
                    {
                        foundDefaultSite = foundDefaultSite || (contentNameNode.InnerText.CompareTo(defaultSiteName) == 0);
                    }

                    var urlList = xDoc.SelectNodes("ContentMetaData/Fields/UrlList/Url");
                    if (urlList == null || urlList.Count == 0)
                    {
                        continue;
                    }

                    foreach (XmlNode node in urlList)
                    {
                        if (node.InnerText.ToLower().CompareTo(domain) != 0)
                            continue;

                        var authAttr = node.Attributes["authType"];
                        if (authAttr == null)
                            continue;

                        authAttr.Value = authMode;
                        xDoc.Save(file);

                        return;
                    }
                }

                //we didn't found the current url in any of the sites, so register it
                RegisterHost(authMode, foundDefaultSite);
            }
            catch(Exception ex)
            {
                //maybe a MapPath or a permission problem
            }

            return;
        }

        private void RegisterHost(string authMode, bool foundDefaultSite)
        {
            try
            {
                var rootPath = this.MapPath("/Root/Sites");
                var domain = HttpContext.Current.Request.Url.Authority.ToLower();
                var defaultSiteName = DefaultSiteName;

                foreach (var file in System.IO.Directory.GetFiles(rootPath))
                {
                    if (!file.ToLower().EndsWith(".content"))
                        continue;

                    var xDoc = new XmlDocument();
                    xDoc.Load(file);

                    var ct = xDoc.SelectSingleNode("ContentMetaData/ContentType");
                    if (ct == null || ct.InnerText != "Site")
                    {
                        continue;
                    }

                    var siteName = string.Empty;
                    var contentNameNode = xDoc.SelectSingleNode("ContentMetaData/ContentName");
                    if (contentNameNode != null)
                    {
                        siteName = contentNameNode.InnerText;
                    }

                    //we have a default site but this is not that site
                    if (foundDefaultSite && siteName.CompareTo(defaultSiteName) != 0)
                        continue;

                    var fieldsNode = xDoc.SelectSingleNode("ContentMetaData/Fields");
                    var urlList = xDoc.SelectSingleNode("ContentMetaData/Fields/UrlList");
                    if (urlList == null)
                    {
                        //this site doesn't have fields...
                        if (fieldsNode == null)
                            continue;

                        //create url list node
                        urlList = xDoc.CreateNode(XmlNodeType.Element, "UrlList", string.Empty);
                        fieldsNode.AppendChild(urlList);
                    }

                    var currentHost = xDoc.CreateNode(XmlNodeType.Element, "Url", string.Empty);
                    var authAttrib = xDoc.CreateAttribute("authType");
                    authAttrib.Value = authMode;

                    currentHost.Attributes.Append(authAttrib);
                    currentHost.InnerText = domain;

                    urlList.AppendChild(currentHost);

                    xDoc.Save(file);

                    return;
                }
            }
            catch (Exception ex)
            {
                //maybe a MapPath or a permission problem
            }

            return;
        }

        private void RemoveMarker()
        {
            System.IO.File.Delete(RunOnceMarkerPath);
        }

        private void ShowLoginPanel()
        {
            panelFormsId.Attributes.CssStyle["display"] = "none";
            panelWindowsId.Attributes.CssStyle["display"] = "block";
        }

        //private void SetRequestFiltering()
        //{
        //    try
        //    {
        //        var siteName = System.Web.Hosting.HostingEnvironment.ApplicationHost.GetSiteName();

        //        using (var sm = new ServerManager())
        //        {
        //            var app = sm.GetApplicationHostConfiguration();
        //            var secAscx = app.GetSection("system.webServer/security/requestFiltering/fileExtensions/add[@fileExtension='.ascx']", siteName);

        //            secAscx.SetAttributeValue("allowed", false);

        //            sm.CommitChanges();

        //            sm.Sites[siteName].Stop();
        //            sm.Sites[siteName].Start();
        //        }

        //    }
        //    catch (Exception ex)
        //    {
 
        //    }
        //}

        private static void RedirectToDefault()
        {
            var url = HttpContext.Current.Request.Url.AbsoluteUri.ToLower();
            url = url.Remove(url.IndexOf("/iisconfig/")) + "/Default.aspx";

            HttpContext.Current.Response.Redirect(url, true);
        }
    }
}
