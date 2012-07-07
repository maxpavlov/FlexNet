using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ApplicationModel;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Workspaces;
using SenseNet.Diagnostics;
using System.Collections.Specialized;
using SenseNet.ContentRepository.Schema;
using SenseNet.Portal.Virtualization;
using SenseNet.Search;

namespace SenseNet.Portal.UI.Controls
{
    public class PermissionEditor : UserControl
    {
        //list of permissions that can be set even for the Everyone group - CURRENTLY NOT USED 
        protected static string[] EditableDenyPermissionList = new[] { "See", "Open", "Delete", "SeePermissions", "SetPermissions", "RunApplication" };

        protected ListView ListViewAcl;

        private ContextInfo _contextInfo;
        private ContextInfo ContextInfo
        {
            get { return _contextInfo ?? (_contextInfo = this.FindControlRecursive("PermissionContext") as ContextInfo); }
        }

        private Node _contextNode;
        private Node ContextNode
        {
            get { return _contextNode ?? (_contextNode = Node.LoadNode(ContextInfo.Path)); }
        }

        private List<int> _customEntryIds;
        protected List<int> CustomEntryIds
        {
            get { return _customEntryIds ?? (_customEntryIds = GetCustomEntryIds()); }
        }

        private SnAccessControlList _acl;
        private SnAccessControlList Acl
        {
            get { return _acl; }
            set
            {
                _acl = value;

                PermissionIds = null;
                EntryIds = null;
            }
        }

        private IdentitySearchInfo _isi;
        private IdentitySearchInfo Isi
        {
            get { return _isi ?? (_isi = new IdentitySearchInfo {Visible = false}); }
            set
            {
                _isi = value;
            }
        }

        private Dictionary<SnPermission, string> _permissionIds;
        private Dictionary<SnPermission, string> PermissionIds
        {
            get
            {
                if (_permissionIds == null && this.Acl != null)
                {
                    _permissionIds = new Dictionary<SnPermission, string>();

                    foreach (var acl in this.Acl.Entries)
                    {
                        foreach (var perm in acl.Permissions)
                        {
                            _permissionIds.Add(perm, Guid.NewGuid().ToString().Replace("-", ""));
                        }
                    }
                }

                return _permissionIds;
            }
            set 
            { 
                _permissionIds = value; 
            }
        }

        private Dictionary<SnAccessControlEntry, string> EntryIds { get; set; }

        private SnAccessControlEntry _currentAce;
        
        protected Panel BreakedPermission;
        protected Panel InheritedPermission;
        protected System.Web.UI.WebControls.HyperLink ParentLink;
        protected WebControl ButtonBreak;
        protected WebControl ButtonRemoveBreak;
        protected Panel PlcAddEntry;
        protected TextBox SearchText;
        protected RadioButtonList RbListIdentityType;
        protected ListBox ListEntries;
        protected Panel PanelError;

        protected override void OnInit(EventArgs e)
        {
            Page.RegisterRequiresControlState(this);

            base.OnInit(e);

            if (!this.Page.IsPostBack)
            {
                var context = Node.LoadNode(ContextInfo.Path);

                Acl = context.Security.GetAcl();
                this.Isi.RebuildAceVisiblityList(this.Acl);
            }

            RebuildEntryIdList();

            if (ListViewAcl != null)
                ListViewAcl.ItemDataBound += ListViewAcl_ItemDataBound;

            RefreshInheritanceControls();

            try
            {
                if (this.ContextNode.Id == RepositoryConfiguration.PortalRootId)
                {
                    if (BreakedPermission != null)
                        BreakedPermission.Visible = false;

                    if (ButtonBreak != null)
                        ButtonBreak.Visible = false;
                }
                else
                {
                    //Start the permission tree from this or parent node
                    var currentNode = this.ContextNode.Parent;
                    while (!HasCustomPermissions(currentNode) && currentNode.Id != RepositoryConfiguration.PortalRootId)
                    {
                        currentNode = currentNode.Parent;
                    }

                    ParentLink.Text = currentNode.DisplayName;
                    ParentLink.NavigateUrl = ActionFramework.GetActionUrl(currentNode.Path, "SetPermissions",
                                                                          PortalContext.Current.BackUrl);
                }
            }
            catch (Exception)
            {
                //there is a node in the tree where we can't see the permission settings
                if (InheritedPermission != null)
                    InheritedPermission.Visible = false;
            }

            if (PanelError != null)
            {
                PanelError.Visible = false;
                PanelError.Controls.Clear();
            }

            RefreshListView();

            RefreshAddEntryPanel();
        }

        protected override void LoadControlState(object savedState)
        {
            if (savedState != null)
            {
                var state = savedState as object[];
                if (state != null && state.Length == 3)
                {
                    base.LoadControlState(state[0]);

                    this.Acl = state[1] as SnAccessControlList;
                    this.Isi = state[2] as IdentitySearchInfo;
                }
            }
            else
                base.LoadControlState(savedState);
        }

        protected override object SaveControlState()
        {
            var state = new object[3];

            state[0] = base.SaveControlState();
            state[1] = this.Acl;
            state[2] = this.Isi;

            return state;
        }

        //======================================================================= Event handlers

        protected void ListViewAcl_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            var dataItem = e.Item as ListViewDataItem;
            if (dataItem == null)
                return;

            var ace = dataItem.DataItem as SnAccessControlEntry;
            if (ace == null)
                return;

            //Pin the current entry. It is used by the 
            //permission list databinding method.
            _currentAce = ace;

            var lblName = GetIdentityControl(dataItem);
            if (lblName != null)
            {
                var identity = Node.Load<GenericContent>(ace.Identity.Path);
                var name = identity is User ? ((User) identity).Username : identity.Name;
                if (!identity.Path.StartsWith(Repository.ImsFolderPath))
                    name = name + " " + HttpContext.GetGlobalResourceObject("Portal", "PermissionLocalGroup");
                else
                    name = identity.Path.Substring(Repository.ImsFolderPath.Length + 1);
                lblName.Text = string.Format("{0} ({1})", identity.DisplayName, name);
            }

            var lblIcon = GetIdentityIconControl(dataItem);
            if (lblIcon != null)
            {
                try
                {
                    lblIcon.CssClass += " snIconBig_" + ContentType.GetByName(Enum.GetName(typeof(SnIdentityKind), ace.Identity.Kind)).Icon;
                }
                catch (Exception ex)
                {
                    Logger.WriteException(ex);
                }
            }

            var lvAce = GetPermissionListViewControl(dataItem);
            if (lvAce != null)
            {
                lvAce.ItemDataBound += ListViewAce_ItemDataBound;
                lvAce.DataSource = ace.Permissions;
                lvAce.DataBind();
            }

            var lblHidden = GetHiddenAceLabel(dataItem);
            if (lblHidden != null)
                lblHidden.Text = this.EntryIds[ace];

            RefreshAcePanelVisibility(dataItem);
        }

        protected void ListViewAce_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            var dataItem = e.Item as ListViewDataItem;
            if (dataItem == null)
                return;

            var perm = dataItem.DataItem as SnPermission;
            if (perm == null)
                return;

            var lblHidden = GetHiddenPermissionLabel(dataItem);
            if (lblHidden != null)
                lblHidden.Text = PermissionIds[perm];

            var cbAllow = GetPermissionAllowCheckbox(dataItem);
            var cbDeny = GetPermissionDenyCheckbox(dataItem);
            var permName = perm.Name.ToLower();

            if (cbAllow != null)
            {
                cbAllow.InputAttributes.Add("class", String.Format("sn-allow-{0} sn-cb-{0}", permName));
                if (!perm.AllowEnabled)
                    cbAllow.InputAttributes.Add("data-inheritvalue", perm.Allow.ToString().ToLower());
            }

            if (cbDeny != null)
            {
                cbDeny.InputAttributes.Add("class", String.Format("sn-deny-{0} sn-cb-{0}", permName));
                if (!perm.DenyEnabled) 
                    cbDeny.InputAttributes.Add("data-inheritvalue", perm.Deny.ToString().ToLower());
            }

            if (_currentAce == null || _currentAce.Identity == null ||
                _currentAce.Identity.NodeId != RepositoryConfiguration.EveryoneGroupId)
                //|| !EditableDenyPermissionList.Contains(perm.Name)) 
                return;

            //disable deny checkboxes for the Everyone group
            if (cbDeny == null) 
                return;

            cbDeny.Checked = false;
            cbDeny.Enabled = false;
        }  

        protected void CbAllow_CheckedChanged(object sender, EventArgs e)
        {
            var cb = sender as CheckBox;
            var lblHidden = GetHiddenPermissionLabel(cb);

            if (lblHidden == null || cb == null)
                return;

            foreach (var perm in PermissionIds.Keys)
            {
                if (PermissionIds[perm].CompareTo(lblHidden.Text) != 0)
                    continue;

                perm.Allow = cb.Checked;
                break;
            }
        }

        protected void CbDeny_CheckedChanged(object sender, EventArgs e)
        {
            var cb = sender as CheckBox;
            var lblHidden = GetHiddenPermissionLabel(cb);

            if (lblHidden == null || cb == null)
                return;

            foreach (var perm in PermissionIds.Keys)
            {
                if (PermissionIds[perm].CompareTo(lblHidden.Text) != 0)
                    continue;

                perm.Deny = cb.Checked;
                break;
            }
        }

        protected void ButtonAddEntry_Click(object sender, EventArgs e)
        {
            this.Isi.Visible = true;

            RefreshAddEntryPanel();
        }

        protected void ButtonSearchId_Click(object sender, EventArgs e)
        {
            var idKind = (SnIdentityKind)Enum.Parse(typeof (SnIdentityKind), RbListIdentityType.SelectedValue, true);

            //store UI data to the control state variable
            this.Isi.SearchText = SearchText.Text;
            this.Isi.IdentityKind = idKind;

            RefreshIdentityResults();
        }

        protected void ButtonAddSelected_Click(object sender, EventArgs e)
        {
            if (ListEntries.SelectedIndex < 0)
                return;

            var acl = this.Acl.Entries.ToList();
            var aceList = new List<SnAccessControlEntry>();
            var index = 0;

            foreach (ListItem item in ListEntries.Items)
            {
                if (!item.Selected)
                    continue;
                
                var node = Node.LoadNode(item.Value);
                var ace = SnAccessControlEntry.CreateEmpty(node.Id, true);

                //add to new entries
                aceList.Add(ace);

                //insert into the top of old entries
                acl.Insert(index++, ace);
            }

            this.Acl.Entries = acl;

            //clear this to refresh
            PermissionIds = null;

            this.Isi.AddToAceVisiblityList(aceList);
            RebuildEntryIdList();

            //hide select panel
            this.Isi.Visible = false;
            RefreshAddEntryPanel();

            RefreshListView();
        }

        protected void ButtonCancelAddId_Click(object sender, EventArgs e)
        {
            this.Isi.Visible = false;

            RefreshAddEntryPanel();
        }

        protected void ButtonAcePanelVisible_Click(object sender, EventArgs e)
        {
            var button = sender as IButtonControl;
            var lblHidden = GetHiddenAceLabel(button as Control);

            if (lblHidden == null || button == null)
                return;

            var current = this.Isi.AceVisiblityList[lblHidden.Text];
            this.Isi.AceVisiblityList[lblHidden.Text] = current == "0" ? "1" : "0";

            RefreshAcePanelVisibility(GetListViewItemControl(button as Control));
        }

        protected void ButtonSave_Click(object sender, EventArgs e)
        {
            try
            {
                ClearEveryoneDeny();
                ValidateAcl();

                var context = Node.LoadNode(ContextInfo.Path);

                context.Security.SetAcl(this.Acl);

                var p = Page as PageBase;
                if (p != null)
                    p.Done(false);
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);

                //show error
                PanelError.Visible = true;
                PanelError.Controls.Add(new LiteralControl(ex.Message));
            }
        }

        protected void ButtonCancel_Click(object sender, EventArgs e)
        {
            var p = Page as PageBase;
            if (p != null)
                p.Done(false);
        }

        protected void ButtonBreak_Click(object sender, EventArgs e)
        {
            if (!this.Acl.Inherits) 
                return;

            var context = Node.LoadNode(ContextInfo.Path);
            if (context == null)
                return;

            context.Security.BreakInheritance();

            //refresh local data and gui
            this.Acl = context.Security.GetAcl();
            this.Isi.RebuildAceVisiblityList(this.Acl);

            _customEntryIds = null;

            RebuildEntryIdList();
            RefreshListView();
            RefreshInheritanceControls();
        }
        
        protected void ButtonRemoveBreak_Click(object sender, EventArgs e)
        {
            if (this.Acl.Inherits)
                return;

            var context = Node.LoadNode(ContextInfo.Path);
            if (context == null)
                return;

            context.Security.RemoveBreakInheritance();

            //refresh local data and gui
            this.Acl = context.Security.GetAcl();
            this.Isi.RebuildAceVisiblityList(this.Acl);

            _customEntryIds = null;

            RebuildEntryIdList();
            RefreshListView();
            RefreshInheritanceControls();
        }

        //======================================================================= Helper methods

        private void RefreshAddEntryPanel()
        {
            if (PlcAddEntry != null)
                PlcAddEntry.Visible = this.Isi.Visible;

            if (!this.Isi.Visible)
                return;

            SearchText.Text = this.Isi.SearchText;
            RbListIdentityType.SelectedIndex = (int) this.Isi.IdentityKind;

            RefreshIdentityResults();
        }

        private void RefreshIdentityResults()
        {
            ListEntries.Items.Clear();

            //do not allow empty search here
            if (SearchText.Text.Length == 0)
                return;

            var ws = Workspace.GetWorkspaceForNode(ContextNode);
            var permQuery = ContentQuery.CreateQuery(string.Format("InTree:\"{0}\"", Repository.ImsFolderPath));
            if (ws != null)
                permQuery.AddClause(string.Format("InTree:\"{0}/{1}\"", ws.Path, Repository.LocalGroupsFolderName), ChainOperator.Or);

            switch (this.Isi.IdentityKind)
            {
                case SnIdentityKind.User:
                    permQuery.AddClause("TypeIs:User");
                    break;
                case SnIdentityKind.Group:
                    permQuery.AddClause("TypeIs:Group");
                    break;
                case SnIdentityKind.OrganizationalUnit:
                    permQuery.AddClause("TypeIs:OrganizationalUnit");
                    break;
                default:
                    throw new InvalidOperationException("Unknown identity kind");
            }

            if (SearchText.Text.Length > 0)
            {
                var st = SearchText.Text;
                if (!st.StartsWith("*"))
                    st = "*" + st;
                if (!st.EndsWith("*"))
                    st = st + "*";

                permQuery.AddClause(string.Format("Name:{0}", st));
            }

            permQuery.Settings.EnableAutofilters = false;
            permQuery.Settings.Top = 500;
            permQuery.Settings.Sort = new List<SortInfo>() {new SortInfo {FieldName = "Name"}};
            
            ListEntries.Items.AddRange((from node in permQuery.Execute().Nodes
                                        select GetListItem(node)).ToArray());
        }

        private void RefreshListView()
        {
            ListViewAcl.DataSource = Acl.Entries;
            ListViewAcl.DataBind();
        }

        private void RefreshAcePanelVisibility(ListViewDataItem dataItem)
        {
            var lblHidden = GetHiddenAceLabel(dataItem);
            if (lblHidden == null) 
                return;

            var acePanel = GetAcePanel(dataItem);
            acePanel.Visible = this.Isi.AceVisiblityList[lblHidden.Text] == "1";

            var toggleButton = GetAceVisibilityButton(dataItem);

            if (toggleButton == null)
                return;

            toggleButton.ToolTip = acePanel.Visible ? "Hide" : "Show";
            toggleButton.CssClass = acePanel.Visible ? "sn-perm-toggle sn-perm-hide" : "sn-perm-toggle sn-perm-show";
        }

        private void RebuildEntryIdList()
        {
            //this list is needed to make a connection between 
            //Ace objects and hidden ids
            this.EntryIds = new Dictionary<SnAccessControlEntry, string>();
            var i = 0;

            foreach (var entry in this.Acl.Entries)
            {
                this.EntryIds.Add(entry, this.Isi.AceVisiblityList.AllKeys[i++]);
            }
        }

        private void ValidateAcl()
        {
            if (this.Acl == null || this.Acl.Entries.Count() == 0)
                throw new InvalidOperationException("Local Acl is empty.");

            if (this.Acl.Inherits)
                return;

            foreach (var entry in this.Acl.Entries)
            {
                var pSee = false;
                var pOpen = false;
                var pSeePermissions = false;
                var pSetPermissions = false;

                //Check all the permission entries. There must be at least one entry that
                //contains allow permissions for the following types: See, Open, SeePermissions, SetPermissions
                foreach (var permission in entry.Permissions)
                {
                    if (permission.Name == PermissionType.See.Name && permission.Allow && !permission.Deny)
                        pSee = true;

                    if (permission.Name == PermissionType.Open.Name && permission.Allow && !permission.Deny)
                        pOpen = true;

                    if (permission.Name == PermissionType.SeePermissions.Name && permission.Allow && !permission.Deny)
                        pSeePermissions = true;

                    if (permission.Name == PermissionType.SetPermissions.Name && permission.Allow && !permission.Deny)
                        pSetPermissions = true;
                }

                if (pSee && pOpen && pSeePermissions && pSetPermissions)
                    return;
            }

            throw new InvalidOperationException("Permission setting is invalid. Please allow See, Open, SeePermissions and SetPermissions rights to at least one user or group.");
        }

        private void ClearEveryoneDeny()
        {
            if (this.Acl == null || this.Acl.Entries.Count() == 0)
                throw new InvalidOperationException("Local Acl is empty.");

            //clear deny permissions from the Everyone group - even 
            //if the user hacked in the checkbox values
            foreach (var entry in this.Acl.Entries)
            {
                if (entry.Identity.NodeId != RepositoryConfiguration.EveryoneGroupId)
                    continue;

                foreach (var permission in entry.Permissions)
                {
                    permission.Deny = false;
                }
            }
        }

        private void RefreshInheritanceControls()
        {
            if (ButtonBreak != null)
                ButtonBreak.Visible = ButtonBreak.Enabled = this.Acl.Inherits;

            if (BreakedPermission != null)
                BreakedPermission.Visible = this.Acl.Inherits;

            if (ButtonRemoveBreak != null)
                ButtonRemoveBreak.Visible = ButtonRemoveBreak.Enabled = !this.Acl.Inherits;

            if (InheritedPermission != null)
                InheritedPermission.Visible = false;
        }

        public bool HasCustomPermissions()
        {
            return HasCustomPermissions(this.ContextNode);
        }

        private static bool HasCustomPermissions(Node node)
        {
            if (node == null)
                return false;

            if (node.Id == RepositoryConfiguration.PortalRootId)
                return true;

            var currentSec = node.Security;
            var expEntries = currentSec.GetExplicitEntries();
            if (expEntries.Length == 0)
                return false;

            if (!currentSec.GetAcl().Inherits)
                return true;

            //We need to do this manual check because after a break + unbreak
            //operation the explicit entries still exist on the content!
            using (new SystemAccount())
            {
                var parentSec = node.Parent.Security;
                var parentEntries = parentSec.GetEffectiveEntries();

                if (expEntries.Length != parentEntries.Length)
                    return true;

                foreach (var entry in expEntries)
                {
                    var parentEntry = parentEntries.FirstOrDefault(pe => pe.PrincipalId == entry.PrincipalId);
                    if (parentEntry == null || parentEntry.ValuesToString().CompareTo(entry.ValuesToString()) != 0)
                        return true;
                }
            }

            return false;
        }

        private List<int> GetCustomEntryIds()
        {
            var sec = this.ContextNode.Security;
            var expEntries = sec.GetExplicitEntries();

            //in case of broken inheritance every entry is custom for sure
            if (!this.Acl.Inherits)
                return expEntries.Select(e => e.PrincipalId).ToList();

            var idList = new List<int>();

            if (expEntries.Length > 0)
            {
                if (this.ContextNode.Id == RepositoryConfiguration.PortalRootId)
                {
                    idList.AddRange(expEntries.Select(ee => ee.PrincipalId));
                }
                else
                {
                    using (new SystemAccount())
                    {
                        var parentSec = this.ContextNode.Parent.Security;
                        var parentEntries = parentSec.GetEffectiveEntries();

                        foreach (var entry in expEntries)
                        {
                            var parentEntry = parentEntries.FirstOrDefault(pe => pe.PrincipalId == entry.PrincipalId);
                            if (parentEntry == null ||
                                parentEntry.ValuesToString().CompareTo(entry.ValuesToString()) != 0)
                                idList.Add(entry.PrincipalId);
                        }
                    }
                }
            }

            return idList;
        }

        //======================================================================= Helper methods

        private static ListItem GetListItem(Node node)
        {
            return node.Path.StartsWith(Repository.ImsFolderPath)
                       ? new ListItem(string.Format("{0} ({1})", node.Name, node.ParentPath.Substring(Repository.ImsFolderPath.Length + 1)), node.Path)
                       : new ListItem(string.Format("{0} ({1})", node.Name, HttpContext.GetGlobalResourceObject("Portal", "PermissionLocalGroup") as string), node.Path);
        }

        //======================================================================= Get controls

        protected static Label GetIdentityControl(ListViewDataItem dataItem)
        {
            return dataItem.FindControl("LabelIdentityName") as Label;
        }

        protected static Label GetIdentityIconControl(ListViewDataItem dataItem)
        {
            return dataItem.FindControl("LabelIcon") as Label;
        }

        protected static ListView GetPermissionListViewControl(ListViewDataItem dataItem)
        {
            return dataItem.FindControl("ListViewAce") as ListView;
        }

        protected static Label GetPermissionNameControl(ListViewDataItem dataItem)
        {
            return dataItem.FindControl("LabelPermissionName") as Label;
        }

        protected static CheckBox GetPermissionAllowCheckbox(ListViewDataItem dataItem)
        {
            return dataItem.FindControl("CbPermissionAllow") as CheckBox;
        }

        protected static Label GetAllowInheritsFromControl(ListViewDataItem dataItem)
        {
            return dataItem.FindControl("LabelAllowInheritsFrom") as Label;
        }

        protected static CheckBox GetPermissionDenyCheckbox(ListViewDataItem dataItem)
        {
            return dataItem.FindControl("CbPermissionDeny") as CheckBox;
        }

        protected static Label GetDenyInheritsFromControl(ListViewDataItem dataItem)
        {
            return dataItem.FindControl("LabelDenyInheritsFrom") as Label;
        }

        protected static Button GetAceVisibilityButton(ListViewDataItem dataItem)
        {
            return dataItem.FindControl("ButtonVisibleAcePanel") as Button;
        }

        protected static Label GetHiddenAceLabel(Control control)
        {
            return GetHiddenControl(control, "LabelHiddenAce");
        }

        protected static Label GetHiddenPermissionLabel(Control control)
        {
            return GetHiddenControl(control, "LabelHidden");
        }

        protected static Label GetHiddenControl(Control control, string name)
        {
            var lvdi = GetListViewItemControl(control);

            return lvdi != null ? lvdi.FindControlRecursive(name) as Label : null;
        }

        protected static PlaceHolder GetAcePanel(Control control)
        {
            var lvdi = GetListViewItemControl(control);

            return lvdi != null ? lvdi.FindControlRecursive("PanelAce") as PlaceHolder : null;
        }

        protected static ListViewDataItem GetListViewItemControl(Control control)
        {
            var lvdi = control;
            while (lvdi != null)
            {
                //find the first ListViewDataItem above the control
                if (lvdi is ListViewDataItem)
                    break;

                lvdi = lvdi.Parent;
            }

            return lvdi as ListViewDataItem;
        }
    }

    [Serializable]
    internal class IdentitySearchInfo
    {
        internal bool Visible { get; set; }
        internal string SearchText { get; set; }
        internal SnIdentityKind IdentityKind { get; set; }

        private NameValueCollection _aceVisibilityList;
        internal NameValueCollection AceVisiblityList
        {
            get { return _aceVisibilityList ?? (_aceVisibilityList = new NameValueCollection()); }
            set { _aceVisibilityList = value; }
        }

        internal void RebuildAceVisiblityList(SnAccessControlList acl)
        {
            this.AceVisiblityList.Clear();

            foreach (var entry in acl.Entries)
            {
                this.AceVisiblityList.Add(Guid.NewGuid().ToString().Replace("-", ""), "0");
            }
        }

        internal void AddToAceVisiblityList(List<SnAccessControlEntry> aceList)
        {
            var nvc = new NameValueCollection();

            //add new entries to the top of the list
            foreach (var ace in aceList)
            {
                nvc.Add(Guid.NewGuid().ToString().Replace("-", ""), "1");
            }

            //add old entries
            nvc.Add(this.AceVisiblityList);

            this.AceVisiblityList = nvc;
        }
    }
}
