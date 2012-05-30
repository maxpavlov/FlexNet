using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.ContentRepository;
using System.Security.Cryptography;
using SenseNet.Portal.PortletFramework;
using asp = System.Web.UI.WebControls;
using SenseNet.ContentRepository.Schema;
using System.Web;
using SenseNet.Portal.Virtualization;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.UI.ContentListViews
{
    public class ViewFrame : System.Web.UI.UserControl
    {
        #region properties

        #region context

        private ContextBoundPortlet _ownerPortlet;
        public ContextBoundPortlet OwnerPortlet
        {
            get
            {
                if (_ownerPortlet == null)
                    _ownerPortlet = ContextBoundPortlet.GetContainingContextBoundPortlet(this);

                return _ownerPortlet;
            }
        }

        private Node _contextNode;
        public Node ContextNode
        {
            get
            {
                if (_contextNode == null)
                    _contextNode = OwnerPortlet.ContextNode;

                return _contextNode;
            }
        }

        private ContentList _contextList;
        public ContentList ContextList
        {
            get
            {
                try
                {
                    if (_contextList == null)
                        _contextList = ContentList.GetContentListForNode(ContextNode);
                }
                catch { }

                return _contextList;
            }
        }

        public Node MostRelevantContext
        {
            get { return ContextList ?? ContextNode; }
        }

        #endregion

        #region view_selection

        private List<LinkButton> viewButtons;

        private string _customHashCode;
        protected virtual string CustomHashCode
        {
            get
            {
                if (string.IsNullOrEmpty(_customHashCode))
                {
                  _customHashCode = ViewFrame.GetHashCode(MostRelevantContext.Path, OwnerPortlet.ID);
                }
                return _customHashCode;
            }
        }

        public string SelectedViewName
        {
            get { return GetSelectedView(CustomHashCode); }
            set { SetView(CustomHashCode, value); }
        }

        private string _defaultViewName;
        public string DefaultViewName
        {
            get
            {
                if (string.IsNullOrEmpty(_defaultViewName))
                {
                    if (ContextList != null)
                        _defaultViewName = ContextList.DefaultView;
                }
                return _defaultViewName;
            }
            set { _defaultViewName = value; }
        }

        public string LoadedViewName
        {
            get
            {
                string loaded = SelectedViewName;
                if (string.IsNullOrEmpty(loaded))
                    loaded = DefaultViewName;
                return loaded;
            }
        }

        #endregion

        #region child_controls

        private Panel _listViewPanel;
        protected Panel ListViewPanel
        {
            get
            {
                if (_listViewPanel == null)
                    _listViewPanel = this.FindControl("ListViewPanel") as Panel;
                return _listViewPanel;
            }
        }

        private asp.ListView _viewSelector;
        protected asp.ListView ViewSelector
        {
            get
            {
                if (_viewSelector == null)
                    _viewSelector = this.FindControl("ViewSelector") as asp.ListView;
                return _viewSelector;
            }
        }

        private asp.ListView _newMenu;
        protected asp.ListView NewMenu
        {
            get
            {
                if (_newMenu == null)
                    _newMenu = this.FindControl("NewMenu") as asp.ListView;
                return _newMenu;
            }
        }

        #endregion

        #region aspnet_members

        protected override void OnLoad(EventArgs e)
        {
            // hack
            //UITools.AddSnUxScripts();
            base.OnLoad(e);
        }

        protected override void CreateChildControls()
        {
            // Load the selected view
            //SelectedViewName = LoadedViewName;
            LoadSelectedView(LoadedViewName);
            //UpdateViewButtons(LoadedViewName);

            this.ChildControlsCreated = true;
        }

        void viewSelector_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                LinkButton link = null;
                var query = from Control c in e.Item.Controls
                           where c is LinkButton
                           select c;
                if (query.Count() > 0)
                    link = query.First() as LinkButton;

                if (link != null)
                    viewButtons.Add(link);
            }
        }

        void viewSelector_ItemCommand(object sender, ListViewCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "ChangeView":
                    ChangeListView(e.CommandArgument as string);
                    break;
                default:
                    break;
            }
        }

        #endregion

        #endregion

        public ViewFrame() : base()
        {
            viewButtons = new List<LinkButton>();
        }

        private void ChangeListView(string viewPath)
        {
            if (!string.IsNullOrEmpty(viewPath))
            {
                ListViewPanel.Controls.Clear();
                SelectedViewName = viewPath;
                LoadSelectedView(viewPath);
                //UpdateViewButtons(viewPath);
            }
        }

        //private void UpdateViewButtons(string viewPath)
        //{
        //    foreach (LinkButton b in viewButtons)
        //    {
        //        if (b.CommandArgument == viewPath)
        //            b.CssClass = "sn-ux-selected";
        //        else
        //            b.CssClass = string.Empty;
        //    }
        //}

        private void LoadSelectedView(string name)
        {
            //if (string.IsNullOrEmpty(name))
            //    return;

            try
            {
                var respath = ViewManager.GetViewPath(MostRelevantContext, name);
                Control view = Page.LoadControl(respath);
                view.ID = "ListViewInternal";

                ListViewPanel.Controls.Add(view);
            }
            catch (Exception e)
            {
                Logger.WriteError(e);

                //give a hint to the portal builder about what went wrong
                this.Controls.Add(new LiteralControl(e.Message));
            }
        }

        // Static members

        public static ViewFrame GetContainingViewFrame(Control child)
        {
            ViewFrame ancestor = null;

            while ((child != null) && ((ancestor = child as ViewFrame) == null))
            {
                child = child.Parent;
            }

            return ancestor;
        }

        // hack (possibly)
        // A thin facade for inline code. May need to be reworked.
        public static string ResolveAction(string nodepath, string actionname)
        {
            //hack
            return PortalActionLinkResolver.Instance.ResolveRelative(nodepath, actionname) + "&back=" + Uri.EscapeDataString(PortalContext.Current.RequestedUri.PathAndQuery.ToString());
        }

        public static string GetHashCode(string nodePath, string uiContextID)
        {
            var key = String.Concat(uiContextID, nodePath);
            var sha = new SHA1CryptoServiceProvider();
            var encoding = new UnicodeEncoding();
            return Convert.ToBase64String(sha.ComputeHash(encoding.GetBytes(key)));
        }

        public static string GetSelectedView(string hash)
        {
            return HttpContext.Current.Session[hash] as string;
        }
        public static void SetView(string hash, string viewPath)
        {
            HttpContext.Current.Session[hash] = viewPath;
        }
    }
}
