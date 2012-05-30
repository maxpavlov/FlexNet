using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.Portal.Virtualization;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage;
using System.Web.UI.HtmlControls;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.Setup
{
    public static class Extensions
    {
        public static void RedirectTo(this Page page)
        {
            AccessProvider.ChangeToSystemAccount();

            var sitepath = PortalContext.Current.Site.Path;
            var pagepath = PortalContext.Current.Site.StartPage.Path;
            pagepath = pagepath.Substring(sitepath.Length);

            AccessProvider.RestoreOriginalUser();

            HttpContext.Current.Response.Redirect(pagepath);
        }
    }

    public partial class Default : System.Web.UI.Page
    {
        private int _runOnceState;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!PortalContext.IsWebSiteRoot)
            {
                //installing Sense/Net to a virtual folder is 
                //not allowed currently. If the software is not 
                //in the root of a website than present an info page.

                if (InstallPanel != null) 
                    InstallPanel.Visible = false;
                if (AppNameErrorPanel != null)
                    AppNameErrorPanel.Visible = true;

                return;
            }

            if (!this.IsPostBack)
            {
                _runOnceState = RunOnce.Run(this);

                if (_runOnceState == 0)
                {
                    var ctx = PortalContext.Current;
                    if (PortalContext.Current.Site != null)
                    {
                        Response.Redirect("/", true);
                    }
                    else
                    {
                        RegisterHost();
                        Response.Redirect(HttpContext.Current.Request.Url.ToString());
                    }
                }
            }
        }

        private int GetMaxPictures()
        {
            var d = this.FindControl("MaxPictures") as HtmlInputHidden;
            return (d != null) ? Convert.ToInt32(d.Value) : 2;
        }

        protected void Timer_OnTick(object sender, EventArgs e)
        {
            _runOnceState = RunOnce.Run(this);

            if (_runOnceState != 0)
            {
                string imgUrl = imgMain.ImageUrl;
                int currentIndex = -1;
                int maxPictures = GetMaxPictures();

                string currentChar = imgUrl.Substring(imgUrl.Length - 5, 1);
                if (int.TryParse(currentChar, out currentIndex))
                    currentIndex++;

                if (currentIndex < 0 || currentIndex > maxPictures)
                    currentIndex = 0;

                imgMain.ImageUrl = string.Format("Install/pic{0}.png", currentIndex);

                if (string.IsNullOrEmpty(RunOnce.ImportError) && RunOnce.TotalCount > 0)
                {
                    //max value is 99% not 100%, because we need to run index 
                    //population after importing all the contents...
                    var percentFull = Math.Min(Math.Max((RunOnce.ImportedCount*1.0/RunOnce.TotalCount)*100, 1.15), 99);
                    var perCentRound = Math.Min(Math.Max(Convert.ToInt32(Math.Round(percentFull)), 1), 99);

                    labelProgressPercent.Text = percentFull.ToString("0.00") + "%";
                    panelBar.Width = new Unit(string.Format("{0}%", perCentRound));
                }
                else
                {
                    ErrorPanel.Visible = true;
                    plcProgressBar.Visible = false;

                    labelError.Text = RunOnce.ImportError ?? "unknown error";
                    timerMain.Enabled = false;
                }

                updSensenetFooter.Update();
            }
            else
            {
                plcProgressBar.Visible = false;

                imgMain.ImageUrl = "Install/picLast.png";
                imgMessage.ImageUrl = "Install/installer-portlet-caption-text-2.png";
                finish.Visible = true;
                timerMain.Enabled = false;

                // wait 1 sec and then redirect to cleanup.aspx -> this way 'Congratulations' picture is shown before being deleted
                var redirectScript = "setTimeout('window.location = \"/Cleanup.aspx\";', 1000)";
                ScriptManager.RegisterStartupScript(this, typeof(Page), "RedirectToCleanup", redirectScript, true);

                updSensenetFooter.Update();

                //HACK: Reset the pinned object: User.Visitor
                SenseNet.ContentRepository.DistributedApplication.Cache.Reset();
                SenseNet.ContentRepository.User.Reset();

            }
        }

        private static string RegisterHost()
        {
            string resultMessage;

            try
            {
                AccessProvider.ChangeToSystemAccount();
                var query = new NodeQuery();
                ContentRepository.Storage.Schema.NodeType nt = ActiveSchema.NodeTypes["Site"];
                query.Add(new TypeExpression(nt, false));
                var result = query.Execute();
                AccessProvider.RestoreOriginalUser();

                if (result.Count > 0)
                {
                    var defSiteName = System.Web.Configuration.WebConfigurationManager.AppSettings["DefaultSiteName"] ?? "Default Site";
                    var authoryity = HttpContext.Current.Request.Url.Authority;

                    var site = (from n in result.Nodes
                                where n.Name == defSiteName
                                select n).FirstOrDefault() as Site;

                    if (site == null)
                        return string.Format("No site exists in the repository with the name {0}", defSiteName);
                    

                    if (!site.UrlList.ContainsKey(authoryity))
                    {
                        // default authentication mode
                        var authMode = System.Web.Configuration.WebConfigurationManager.AppSettings["DefaultAuthenticationMode"];
                        var newList = new Dictionary<string, string>(site.UrlList)
                                          {
                                              { authoryity, authMode }
                                          };

                        site.UrlList = newList;
                        AccessProvider.Current.SetCurrentUser(ContentRepository.User.Administrator);
                        site.Save();
                        resultMessage = string.Format(
                            "The host '{0}' has been succesfully assigned to the site '{1}'.", authoryity, site.Path);
                    }
                    else
                    {
                        resultMessage = string.Format("The host '{0}' is already assigned to the site '{1}'.", authoryity, site.Path);
                    }
                }
                else
                {
                    resultMessage = string.Format("You cannot use RegisterHost if you don't have a Site in your ContentRepositoy.");
                }
            }
            catch (Exception ex) //logged
            {
                Logger.WriteException(ex);
                resultMessage = ex.Message;
            }

            return resultMessage;
        }
    }
}
