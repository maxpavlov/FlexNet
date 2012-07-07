using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.Portal.UI.PortletFramework;
using System.Web.UI.WebControls;
using SenseNet.DirectoryServices;
using SenseNet.Diagnostics;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using System.Security;
using System.Security.Principal;
using System.Web.UI.WebControls.WebParts;

namespace SenseNet.Portal.Portlets
{
    public class SyncObjectFromADPortlet : PortletBase
    {
        /* ==================================================================================== Members */
        private TextBox _tbLdapPath;
        private Button _btnSyncObject;
        private Button _btnCheck;


        /* ==================================================================================== Properties */
        private bool _useImpersonate = true;
        [WebDisplayName("Use Windows auth impersonation")]
        [WebDescription("When checked the currently loggod on Windows user account will be used to connect to Active Directory, otherwise the app pool user account. <div class='ui-helper-clearfix sn-dialog-editportlet-warning'><img class='sn-icon sn-icon16 sn-floatleft' src='/Root/Global/images/icons/16/warning.png' /><i>Using the app pool user for sync exposes the AD to security threats!</i></div>")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.ADSync, EditorCategory.ADSync_Order)]
        [WebOrder(100)]
        public bool UseImpersonate
        {
            get { return _useImpersonate; }
            set { _useImpersonate = value; }
        }


        /* ==================================================================================== Constructor */
        public SyncObjectFromADPortlet()
        {
            this.Name = "Sync Object from AD";
            this.Description = "You can synchronize Users, Groups, OrganizationalUnits from Active Directory providing an LDAP path with this portlet.";
            this.Category = new PortletCategory(PortletCategoryType.Portal);

            this.HiddenProperties.Add("Renderer");
        }


        /* ==================================================================================== Methods */
        protected override void CreateChildControls()
        {
            _tbLdapPath = new TextBox { Columns = 110 };
            _btnCheck = new Button { Text = "Check", CssClass="sn-submit" };
            _btnCheck.Click += new EventHandler(_btnCheck_Click);
            _btnSyncObject = new Button { Text = "Sync Object", CssClass = "sn-submit" };
            _btnSyncObject.Click += new EventHandler(_btnSyncObject_Click);

            this.Controls.Add(new Literal { Text = "Enter LDAP path of object to be synced to portal <i>(e.g.: CN=MyGroup,OU=MyOrg,DC=Nativ,DC=local)</i>:" });
            this.Controls.Add(new Literal { Text = "<br/>" });
            this.Controls.Add(_tbLdapPath);
            this.Controls.Add(new Literal { Text = "&nbsp;" });
            this.Controls.Add(_btnCheck);
            this.Controls.Add(new Literal { Text = "&nbsp;" });
            this.Controls.Add(_btnSyncObject);

            this.ChildControlsCreated = true;
            base.CreateChildControls();
        }


        /* ==================================================================================== Event handlers */
        protected void _btnCheck_Click(object sender, EventArgs e)
        {
            var syncAD2Portal = new SyncAD2Portal();
            var syncInfo = syncAD2Portal.GetSyncInfo(_tbLdapPath.Text);
            
            string syncInfoStr;
            if (!syncInfo.SyncTreeFound) {
                syncInfoStr = "Configured SyncTree could not be found for this path. Check the <a href='/Explore.html#/Root/System/SystemPlugins/Tools/DirectoryServices/AD2PortalConfig.xml' target='_blank'>configuration</a>!";
            }
            else 
            {
                syncInfoStr = string.Format("Configured synctree: ({0}, {1}) -> ({2}) (<a href='/Explore.html#/Root/System/SystemPlugins/Tools/DirectoryServices/AD2PortalConfig.xml' target='_blank'>configuration</a>)<br/>Target portal path: <a href='/Explore.html#{3}' target='_blank'>{3}</a><br/>{4}<br/>{5}",
                    syncInfo.SyncTreeADIPAddress,
                    syncInfo.SyncTreeADPath,
                    syncInfo.SyncTreePortalPath,
                    syncInfo.TargetPortalPath,
                    syncInfo.PortalNodeExists ? "Target path exists" : "Target path does not exist",
                    syncInfo.PortalParentExists ? "Target parent path exists" : "Target parent path does not exist"
                    );
            }
            this.Controls.Add(new Literal { Text = "<hr/><strong>Results:</strong><br/>" });
            this.Controls.Add(new Literal { Text = syncInfoStr });
        }
        protected void _btnSyncObject_Click(object sender, EventArgs e)
        {
            this.Controls.Add(new Literal { Text = "<hr/><strong>Results:</strong><br/>" });
            
            try
            {
                var syncAD2Portal = new SyncAD2Portal();

                // impersonate to currently logged on windows user, to use its credentials to connect to AD
                WindowsImpersonationContext impersonationContext = null;
                if (this.UseImpersonate)
                {
                    var windowsIdentity = ((User)User.Current).WindowsIdentity;
                    if (windowsIdentity == null)
                    {
                        this.Controls.Add(new Literal { Text = "Windows identity impersonation failed for current user." });
                        return;
                    }
                    impersonationContext = windowsIdentity.Impersonate();
                }

                int? logid = null;
                bool noerrors = false;
                try
                {
                    logid = AdLog.SubscribeToLog();
                    syncAD2Portal.SyncObjectFromAD(_tbLdapPath.Text);
                    noerrors = true;
                }
                catch (Exception ex)
                {
                    Logger.WriteException(ex);
                    this.Controls.Add(new Literal { Text = string.Format("An error occurred during execution ({0}).<br/>See adsync log below and eventlog for details.<br/><br/>", ex.Message) });
                }
                finally
                {
                    if (impersonationContext != null)
                        impersonationContext.Undo();

                    if (logid.HasValue)
                    {
                        var logStr = AdLog.GetLogAndRemoveSubscription(logid.Value);
                        this.Controls.Add(new Literal { Text = logStr.Replace(Environment.NewLine, "<br/>") });
                    }

                    // add link to object to bottom
                    if (noerrors)
                    {
                        var syncInfo = syncAD2Portal.GetSyncInfo(_tbLdapPath.Text);
                        if (syncInfo.PortalNodeExists)
                            this.Controls.Add(new Literal { Text = string.Format("<hr/>Check the results: <a href='/Explore.html#{0}' target='_blank'>{0}</a>", syncInfo.TargetPortalPath) });
                    }
                }
            }
            catch (SecurityException ex)
            {
                Logger.WriteException(ex);
                this.Controls.Add(new Literal { Text = string.Format("A security exception occurred ({0}).<br/>See eventlog for details.", ex.Message) });
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
                this.Controls.Add(new Literal { Text = string.Format("An exception occurred ({0}).<br/>See eventlog for details.", ex.Message) });
            }
        }
    }
}
