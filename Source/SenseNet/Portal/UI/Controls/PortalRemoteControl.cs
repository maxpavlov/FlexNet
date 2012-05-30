using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Portal.Virtualization;
using SenseNet.ContentRepository;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.ApplicationModel;
using SenseNet.ContentRepository.Schema;
using System.Drawing;
using SenseNet.Search;

namespace SenseNet.Portal.UI.Controls
{
    public sealed class PortalRemoteControl : Control
    {
        private Control prcTemplateControl;

        // members //////////////////////////////////////////////////////////////////
        private readonly string prcTemplatePath = string.Concat(PortalContext.WebRootFolderPath, "/prc.ascx");


        // properties ///////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets a value indicating whether this instance is in async postback.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is in async postback; otherwise, <c>false</c>.
        /// </value>
        public bool IsInAsyncPostback
        {
            get
            {
                var scm = ScriptManager.GetCurrent(this.Page);
                return scm != null && scm.IsInAsyncPostBack;
            }
        }

        private bool? _isVisible;

        /// <summary>
        /// Gets a value indicating whether this instance is visible.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsVisible
        {
            get
            {
                if (_isVisible.HasValue)
                    return _isVisible.Value;

                var user = User.Current as Node;

                _isVisible = user != null && this.AdminGroupNodes.Any(ag => user.Security.IsInGroup(ag.Id));

                return _isVisible.Value;
            }
        }

        private string _adminGroups;
        public string Groups
        {
            get { return _adminGroups ?? "Administrators"; }
            set { _adminGroups = value; }
        }

        private List<ISecurityContainer> _adminGroupNodes;
        private IEnumerable<ISecurityContainer> AdminGroupNodes
        {
            get { return _adminGroupNodes ?? (_adminGroupNodes = GetAdminGroups().ToList()); }
        }

        /// <summary>
        /// Gets or sets the tag container.
        /// </summary>
        /// <value>The tag container.</value>
        public string TagContainer { get; set; }

        /// <summary>
        /// Gets or sets the PRC template which holds the declared server controls and html markup.
        /// </summary>
        /// <value>The PRC template.</value>
        [TemplateContainer(typeof(PortalRemoteControl))]
        public ITemplate PrcTemplate { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is WCMS mode.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is WCMS mode; otherwise, <c>false</c>.
        /// </value>
        [Obsolete("This property will be removed in the upcoming releases. It's recommended not to use it anymore.")]
        public bool IsWcmsMode
        {
            get
            {
                var pagePath = PortalContext.Current.Page.Path;
                var contextPath = PortalContext.Current.ContextNodePath;
                return pagePath.Equals(contextPath) || pagePath.ToLower().Contains("(apps)/this");
            }
        }

        public bool IsApplicationMode
        {
            get
            {
                return PortalContext.Current.GetApplicationContext() != null;
            }
        }

        [Obsolete("This property will be removed in the upcoming releases. It's recommended not to use it anymore.")]
        public bool IsPage
        {
            get
            {
                //PortalContext.Current.ContextNode.NodeType.
                //bool isPage = ContentType.GetByName("Page").IsDescendantOf(PortalContext.Current.ContextNode);
                //bool isPage = PortalContext.Current.NodeType.IsInstaceOfOrDerivedFrom("Page");
                //return isPage || IsWcmsMode;
                //return (!IsApplicationMode && IsWcmsMode) || (IsApplicationMode && IsWcmsMode);
                return IsWcmsMode;
            }
        }

        public WebPartManager WPManager
        {
            get
            {
                return WebPartManager.GetCurrentWebPartManager(this.Page);
            }
        }


        // events ///////////////////////////////////////////////////////////////////

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            if (!IsVisible)
            {
                base.OnInit(e);
                return;
            }

            var wpm = WebPartManager.GetCurrentWebPartManager(Page);
            if (wpm != null)
            {
                wpm.SelectedWebPartChanged += WpmSelectedWebPartChanged;
                
            }
                

            prcTemplateControl = new Control();
            PrcTemplate = Page.LoadTemplate(prcTemplatePath);
            PrcTemplate.InstantiateIn(prcTemplateControl);
            Controls.Add(prcTemplateControl);

            base.OnInit(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"/> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            if (!IsVisible)
            {
                base.OnLoad(e);
                return;
            }

            UITools.AddStyleSheetToHeader(Page.Header, UITools.ClientScriptConfigurations.IconsCssPath);
            UITools.AddStyleSheetToHeader(Page.Header, UITools.ClientScriptConfigurations.jQueryCustomUICssPath);
            UITools.AddStyleSheetToHeader(Page.Header, UITools.ClientScriptConfigurations.SNWidgetsCss, 100);
            UITools.AddPickerCss();

            UITools.AddScript(UITools.ClientScriptConfigurations.SNPortalRemoteControlPath);

            base.OnLoad(e);

            InitializeControl();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            if (!IsVisible)
            {
                base.OnPreRender(e);
                return;
            }

            DisplayInformation();

            if (!String.IsNullOrEmpty(TagContainer))
            {
                Control c;
                c = prcTemplateControl.FindControlRecursive("PRCIcon");
                if (c != null) c.Visible = false;
                c = prcTemplateControl.FindControlRecursive("prctoolbarmenu");
                if (c != null) c.Visible = true;
                var containerControl = Page.FindControlRecursive(TagContainer) as PlaceHolder;
                if (containerControl == null)
                    return;
                containerControl.Controls.Clear();
                containerControl.Controls.Add(c);
                UITools.RegisterStartupScript(
                        "PortalRemoteControlIds",
                        String.Format(@"SN.PortalRemoteControl.PRCToolbarId = '{0}';", containerControl.FindControlRecursive("prctoolbarmenu").ClientID, prcTemplateControl.FindControlRecursive("PRCIcon").ClientID),
                        this.Page);

            }
            UITools.RegisterStartupScript(
                    "PortalRemoteControlIds",
                    String.Format(@"SN.PortalRemoteControl.PRCIconId = '{0}'; ", prcTemplateControl.FindControlRecursive("PRCIcon").ClientID),
                    this.Page);

            var scm = ScriptManager.GetCurrent(Page);
            if (scm == null)
                return;
            UITools.RegisterStartupScript(
                    "PortalRemoteControlInit",
                    String.Format(@"SN.PortalRemoteControl.PRCInitialize('{0}'); ", "PortalRemoteControl"),
                    this.Page);

            base.OnPreRender(e);
        }

        // internals ////////////////////////////////////////////////////////////////

        /// <summary>
        /// Binds the Click event to the specified control. If the control does not exist, nothing happens.
        /// </summary>
        /// <param name="controlId">The control id.</param>
        /// <param name="visible">if set to <c>true</c> the control is visible on the page under PortalRemoteControl.</param>
        private void BindEvent(string controlId, bool visible)
        {
            LinkButton linkButton = null;
            linkButton = prcTemplateControl.FindControlRecursive(controlId) as LinkButton;
            if (linkButton == null)
                return;
            linkButton.Click -= LinkButtonClick;
            linkButton.Click += LinkButtonClick;
            linkButton.Visible = visible;
        }

        /// <summary>
        /// Displays the information at the top of the Portal Remote Control header.
        /// </summary>
        private void DisplayInformation()
        {
            var application = PortalContext.Current.GetApplicationContext();
            var node = application ?? PortalContext.Current.ContextNode;

            if (node == null)
                return;

            //Content name
            var text = HttpContext.GetGlobalResourceObject("PortalRemoteControl", "ContentName") as string;
            var infoLabel = prcTemplateControl.FindControlRecursive("ContentNameLabel") as Label;
            if (infoLabel != null)
            {
                infoLabel.Text = string.Format(text ?? "{0}", node.Name);
                infoLabel.ToolTip = node.Path;
            }

            //Content type
            text = HttpContext.GetGlobalResourceObject("PortalRemoteControl", "ContentType") as string;
            infoLabel = prcTemplateControl.FindControlRecursive("ContentTypeLabel") as Label;
            if (infoLabel != null)
                infoLabel.Text = string.Format(text ?? "{0}", node.NodeType.Name);

            //Content type icon
            string iconPath;
            if (node is GenericContent) iconPath = IconHelper.ResolveIconPath((node as GenericContent).Icon, 16);
            else if (node is ContentType) iconPath = IconHelper.ResolveIconPath((node as ContentType).Icon, 16);
            else iconPath = "";
            var ctImage = prcTemplateControl.FindControlRecursive("ContentTypeImage") as System.Web.UI.WebControls.Image;
            if (!string.IsNullOrEmpty(iconPath) && ctImage != null)
                ctImage.ImageUrl = iconPath;

            //Version
            text = HttpContext.GetGlobalResourceObject("PortalRemoteControl", "Version") as string;
            infoLabel = prcTemplateControl.FindControlRecursive("VersionLabel") as Label;
            if (infoLabel != null && !string.IsNullOrEmpty(text))
                infoLabel.Text = string.Format(text, node.Version.VersionString);

            //Page mode
            infoLabel = prcTemplateControl.FindControlRecursive("ModeLabel") as Label;
            if (infoLabel != null && !string.IsNullOrEmpty(text))
            {
                if (IsApplicationMode)
                {
                    infoLabel.Visible = true;
                    text = HttpContext.GetGlobalResourceObject("PortalRemoteControl", "PageMode") as string;
                    var wpm = WebPartManager.GetCurrentWebPartManager(this.Page);
                    var modeName = wpm.DisplayMode == wpm.DisplayModes["Browse"]
                                       ? HttpContext.GetGlobalResourceObject("PortalRemoteControl", "Preview") as string
                                       : HttpContext.GetGlobalResourceObject("PortalRemoteControl", "Edit") as string;

                    if (!string.IsNullOrEmpty(text)) 
                        infoLabel.Text = string.Format(text, modeName);
                }
                else
                {
                    infoLabel.Visible = false;
                }
            }

            //Checked out
            infoLabel = prcTemplateControl.FindControlRecursive("CheckedOutByLabel") as Label;
            if (infoLabel != null)
            {
                if (node.Locked)
                {
                    infoLabel.Text = (HttpContext.GetGlobalResourceObject("PortalRemoteControl", "Checked-out") as string);

                    var checkedOutLink = prcTemplateControl.FindControlRecursive("CheckedOutLink") as System.Web.UI.WebControls.HyperLink;
                    if (checkedOutLink != null)
                    {
                        checkedOutLink.Visible = true;
                        checkedOutLink.NavigateUrl = node.LockedBy.Path;
                        checkedOutLink.Text = node.LockedBy.Username;
                    }

                    if (User.Current.Id != node.LockedById && !string.IsNullOrEmpty(node.LockedBy.Email))
                    {
                        var sendMailLink = prcTemplateControl.FindControlRecursive("SendMessageLink") as System.Web.UI.WebControls.HyperLink;
                        if (sendMailLink != null)
                        {
                            sendMailLink.Visible = true;
                            sendMailLink.NavigateUrl = string.Format("mailto:{0}?subject={1}&body={2}{3}",
                                node.LockedBy.Email,
                                (HttpContext.GetGlobalResourceObject("PortalRemoteControl", "MailSubject") as string),
                                (HttpContext.GetGlobalResourceObject("PortalRemoteControl", "MailBody") as string),
                                PortalContext.Current.UrlWithoutBackUrl);
                        }
                    }
                }
                else
                {
                    text = HttpContext.GetGlobalResourceObject("PortalRemoteControl", "CheckedIn") as string;
                    infoLabel.Text = text;
                }
            }

            //Last modified
            text = HttpContext.GetGlobalResourceObject("PortalRemoteControl", "LastModified") as string;
            infoLabel = prcTemplateControl.FindControlRecursive("LastModifiedLabel") as Label;
            if (infoLabel != null && !string.IsNullOrEmpty(text))
            {
                infoLabel.Text = string.Format(text, node.ModificationDate);
            }

            //Modified by
            var modLink = prcTemplateControl.FindControlRecursive("LastModifiedLink") as System.Web.UI.WebControls.HyperLink;
            if (modLink != null)
            {
                modLink.NavigateUrl = node.ModifiedBy.Path;
                modLink.Text = node.ModifiedBy.Name;
            }

            //Page template
            text = HttpContext.GetGlobalResourceObject("PortalRemoteControl", "PageTemplate") as string;
            infoLabel = prcTemplateControl.FindControlRecursive("PageTemplateLabel") as Label;
            if (infoLabel != null && !string.IsNullOrEmpty(text))
                infoLabel.ToolTip = infoLabel.Text = string.Format(text, Portal.Page.Current.PageTemplateNode.Name);

            //Skin
            text = HttpContext.GetGlobalResourceObject("PortalRemoteControl", "Skin") as string;
            infoLabel = prcTemplateControl.FindControlRecursive("SkinLabel") as Label;
            if (infoLabel != null && !string.IsNullOrEmpty(text))
                infoLabel.Text = string.Format(text, PortalContext.Current.CurrentSkin);
            
        }
        
        private void InitializeControl()
        {
            Control c;

            var browseLink = prcTemplateControl.FindControlRecursive("BrowseLink") as System.Web.UI.WebControls.HyperLink;
            if (browseLink != null)
            {
                if (PortalContext.Current.ActionName == null || PortalContext.Current.ActionName.ToLower() == "browse")
                    browseLink.Visible = false;
            }

            var browseAppLink = prcTemplateControl.FindControlRecursive("BrowseApp") as ActionLinkButton;
            if (browseAppLink != null)
            {
                var app = PortalContext.Current.GetApplicationContext();
                if (app != null)
                {
                    browseAppLink.Visible = false;
                }
                else if (PortalContext.Current.ActionName != null && PortalContext.Current.ActionName.ToLower() == "explore")
                {
                    browseAppLink.ParameterString = browseAppLink.ParameterString.Replace("context={CurrentContextPath}", string.Empty);
                }
            }

            var wbpm = WebPartManager.GetCurrentWebPartManager(this.Page);
            if (wbpm != null)
            {
                if (wbpm.DisplayMode.Name == "Edit")
                {
                    c = prcTemplateControl.FindControlRecursive("Rename");
                    c.Visible = true;
                    c = prcTemplateControl.FindControlRecursive("CopyTo");
                    c.Visible = true;
                    c = prcTemplateControl.FindControlRecursive("MoveTo");
                    c.Visible = true;
                    c = prcTemplateControl.FindControlRecursive("DeletePage");
                    c.Visible = true;

                    var hyperLink =
                        prcTemplateControl.FindControlRecursive("Browse") as System.Web.UI.WebControls.HyperLink;
                    if (hyperLink != null)
                        hyperLink.Visible = false;

                    c = prcTemplateControl.FindControlRecursive("Versions");
                    c.Visible = false;
                    c = prcTemplateControl.FindControlRecursive("EditPage");
                    c.Visible = false;
                    c = prcTemplateControl.FindControlRecursive("SetPermissions");
                    c.Visible = false;
                } else if (wbpm.DisplayMode.Name == "Browse")
                {
                    c = prcTemplateControl.FindControlRecursive("Versions");
                    c.Visible = true;
                    c = prcTemplateControl.FindControlRecursive("EditPage");
                    c.Visible = true;
                    c = prcTemplateControl.FindControlRecursive("SetPermissions");
                    c.Visible = true;
                }

                BindEvent("AddPortletButton", true);
                if (wbpm.DisplayMode == WebPartManager.BrowseDisplayMode)
                {
                    BindEvent("BrowseModeButton", false);
                    BindEvent("EditModeButton", IsApplicationMode && 
                        (SavingAction.HasCheckOut(PortalContext.Current.Page) ||
                        SavingAction.HasCheckIn(PortalContext.Current.Page) ||
                        SavingAction.HasForceUndoCheckOutRight(PortalContext.Current.Page)));
                }

                if (wbpm.DisplayMode == WebPartManager.EditDisplayMode)
                {
                    BindEvent("BrowseModeButton", true);
                    BindEvent("EditModeButton", false);
                }
            }
        }

        /// <summary>
        /// Main entry event for all buttons under the Portal Remote Control. 
        /// </summary>
        /// <param name="sender">The sender. IButtonControl must be implemented by this control.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void LinkButtonClick(object sender, EventArgs e)
        {
            var b = sender as IButtonControl;
            if (b == null)
                return;
            var wpm = WebPartManager.GetCurrentWebPartManager(this.Page);

            var command = b.CommandName.ToLower();
            switch (command)
            {
                case "entereditmode":
                    if (wpm == null)
                        return;
                    var mode = wpm.SupportedDisplayModes["Edit"];
                    if (mode == null)
                        break;

                    wpm.DisplayMode = mode;
                    var eButton = prcTemplateControl.FindControlRecursive("EditModeButton") as LinkButton;
                    if (eButton != null)
                        eButton.Visible = false;

                    if (SavingAction.HasCheckOut(PortalContext.Current.Page))
                        PortalContext.Current.Page.CheckOut();

                    BindEvent("BrowseModeButton", true);
                    ResetVersioningButtons();

                    break;

                case "enterbrowsemode":
                    if (wpm == null)
                        return;
                    var mode2 = wpm.SupportedDisplayModes["Browse"];
                    if (mode2 == null)
                        break;
                    wpm.DisplayMode = mode2;
                    var bmButton = prcTemplateControl.FindControlRecursive("BrowseModeButton") as LinkButton;
                    if (bmButton!= null)
                        bmButton.Visible = false;

                    BindEvent("EditModeButton", true);
                    ResetVersioningButtons();

                    break;

                case "addportlet":

                    var addPortletTextBox = prcTemplateControl.FindControlRecursive("AddPortletButtonTextBox") as TextBox;
                    if (addPortletTextBox == null)
                        break;

                    var addPortletInfo = addPortletTextBox.Text;
                    // FIX: Bug 5539
                    if (String.IsNullOrEmpty(addPortletInfo))
                    {
                        var eventArgs = HttpContext.Current.Request.Form["__EVENTARGUMENT"];
                        addPortletInfo = eventArgs;
                    }
                    var portletParams = addPortletInfo.Split(';');
                    System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(addPortletInfo), "I've must been losing my mind: AddPortletButtonTextBox has no valid value. (missing portlet info) ");
                    this.AddPortlet(Int32.Parse(portletParams[0]), portletParams[1]);
                    break;

                default:
                    break;

            }
            InitializeControl();
        }

        /// <summary>
        /// Refreshes versioning related ActionLinkButtons after changing the versioning state of the content.
        /// </summary>
        private void ResetVersioningButtons()
        {
            var c = prcTemplateControl.FindControlRecursive("CheckoutButton") as ActionLinkButton;
            if (c != null) c.Reset();
            c = prcTemplateControl.FindControlRecursive("CheckinButton") as ActionLinkButton;
            if (c != null) c.Reset();
            c = prcTemplateControl.FindControlRecursive("PublishButton") as ActionLinkButton;
            if (c != null) c.Reset();
            c = prcTemplateControl.FindControlRecursive("Approve") as ActionLinkButton;
            if (c != null) c.Reset();
            c = prcTemplateControl.FindControlRecursive("UndoCheckoutButton") as ActionLinkButton;
            if (c != null) c.Reset();
            c = prcTemplateControl.FindControlRecursive("ForceUndoCheckOut") as ActionLinkButton;
            if (c != null) c.Reset();
        }

        private void AddPortlet(int portletId, string zoneId)
        {
            var portlet = Node.LoadNode(portletId);
            var typeName = portlet["TypeName"].ToString();
            var wpz = WPManager.Zones[zoneId];
            if (wpz == null) 
                return;

            var privateType = Type.GetType(typeName);
            object instance = null;
            try
            {
                instance = Activator.CreateInstance(privateType);
            }
            catch (Exception e)
            {
                Logger.WriteException(e);
            }
            var wp = (WebPart)instance;
            if (wp != null)
                WPManager.AddWebPart(wp, wpz, 0);

            var mode = WPManager.SupportedDisplayModes["Edit"];
            if (mode != null)
            {
                WPManager.DisplayMode = mode;
                InitializeControl();
            }

            var snwpm = this.WPManager as SNWebPartManager;
            if (snwpm != null)
                snwpm.SetDirty();
        }

        private IEnumerable<ISecurityContainer> GetAdminGroups()
        {
            var ag = new List<ISecurityContainer>();
            if (string.IsNullOrEmpty(this.Groups))
                return ag;

            var ags = this.Groups.Split(new []{',', ';', ' '}, StringSplitOptions.RemoveEmptyEntries);

            using (new SystemAccount())
            {
                foreach (var agName in ags)
                {
                    try
                    {
                        if (agName.StartsWith("/Root/"))
                        {
                            var group = Node.LoadNode(agName) as ISecurityContainer;
                            if (group != null)
                                ag.Add(group);
                        }
                        else
                        {
                            ContentQuery cq = null;
                            const string queryText = "+TypeIs:(Group OrganizationalUnit) +Name:{0}";
                            var nameParts = agName.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

                            switch (nameParts.Length)
                            {
                                case 0:
                                    break;
                                case 1:
                                    //load group or OU only by name
                                    cq = ContentQuery.CreateQuery(string.Format(queryText, nameParts[0]));
                                    break;
                                default:
                                    //load group or OU by domain and name
                                    var domain = ContentQuery.Query("+TypeIs:Domain +Name:" + nameParts[0]).Nodes.FirstOrDefault();
                                    if (domain != null)
                                    {
                                        cq = ContentQuery.CreateQuery(string.Format(queryText, nameParts[1]));
                                        cq.AddClause("InTree:\"" + domain.Path + "\"");
                                    }
                                    break;
                            }

                            if (cq != null)
                                ag.AddRange(cq.Execute().Nodes.Cast<ISecurityContainer>());
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteException(ex);
                    }
                }
            }

            return ag;
        }

        /// <summary>
        /// Fires when the selected webpart has changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.WebParts.WebPartEventArgs"/> instance containing the event data.</param>
        protected void WpmSelectedWebPartChanged(object sender, WebPartEventArgs e)
        {
            ToggleEditorZone(e.WebPart, this.Page);
        }

        /// <summary>
        /// Toggles the editor zone.
        /// </summary>
        /// <param name="webPart">The selected portlet.</param>
        /// <param name="page">Running Page instance.</param>
        internal static void ToggleEditorZone(IWebPart webPart, System.Web.UI.Page page)
        {
            var wpm = WebPartManager.GetCurrentWebPartManager(page);
            if (wpm == null)
                return;

            var mode = wpm.DisplayMode;
            var selectedWebPart = wpm.SelectedWebPart;
            var displaySidePanel = ((mode == WebPartManager.EditDisplayMode && selectedWebPart != null) || (mode == WebPartManager.CatalogDisplayMode) || (mode == WebPartManager.ConnectDisplayMode && selectedWebPart != null));

            if (!displaySidePanel)
                return;
            if (webPart == null)
                return;

            //var toolPanel = page.Master.FindControl("sndlgToolPanel") as HtmlGenericControl;
            var toolPanel = page.Master.FindControl("snToolPanel") as HtmlGenericControl;
            if (toolPanel == null)
                throw new ApplicationException("sndlgToolPanel element does not exist in the MasterPage.");

            string webPartName = string.Empty;
            var portletBase = webPart as PortletBase;
            if (portletBase != null)
                webPartName = portletBase.Name;
            var webPartTypeName = webPart.GetType().Name;
            var title = String.Format("{0} portlet properties ({1})", webPartName, webPartTypeName);
            //var callback = String.Format(@"SN.PortalRemoteControl.showDialog('{0}', {{ autoOpen: true, width: 550, height:600, minWidth: 500, minHeight: 550, resize: SN.PortalRemoteControl.ResizePortletEditorAccordion, title:'{1}' }});", toolPanel.ClientID, title);
            var callback = String.Format(@"SN.PortalRemoteControl.showDialog('{0}', {{ autoOpen: true, width: 550, height:600, minWidth: 500, minHeight: 550, resize: SN.PortalRemoteControl.ResizePortletEditorAccordion }});", toolPanel.ClientID);
            var editorZone = page.Master.FindControlRecursive("EditorZone_Editor") as CollapsibleEditorZone;
            if (editorZone != null)
            {
                var p = page.ClientScript.GetPostBackEventReference(editorZone, "cancel");
                callback = String.Format(
                    @"SN.PortalRemoteControl.showDialog('{0}', {{ autoOpen: true, width: 550, height:600, minWidth: 500, minHeight: 550, resize: SN.PortalRemoteControl.ResizePortletEditorAccordion, title:'{1}', close: function(event,ui) {{ {2}; }} }} );",
                    toolPanel.ClientID, title, p);
            }

            UITools.RegisterStartupScript("PropertyGridEditorShow", callback , page);
        }

    }
}
