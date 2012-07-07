using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.PortletFramework;
using System.Web.UI.WebControls;

namespace SenseNet.Portal.UI.Controls
{
    public partial class PortletSynchronizer : UserControl
    {
        protected ListView ListView1;
        protected Button btnInstallPortlets;
        protected Button btnBack;
        protected Panel pnlSuccess;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            BindList();

            pnlSuccess.Visible = false;
        }

        private void BindList()
        {
            var uninstalled = this.GetUninstalledPortlets().
                Select(p => new { DisplayName = p.Portlet.Name, Description = p.Portlet.Description, Category = p.Portlet.Category.Title }).
                OrderBy(a => a.DisplayName). 
                ToList();

            if (uninstalled.Count == 0)
                btnInstallPortlets.Enabled = false;
            else
                btnInstallPortlets.Enabled = true;

            ListView1.DataSource = uninstalled;
            ListView1.DataBind();
        }
        private IEnumerable<PortletInventoryItem> GetUninstalledPortlets()
        {
            // get uninstalled portlets
            var allPortlets = PortletInventory.GetPortletsFromDll();
            var repoPortlets = PortletInventory.GetPortletsFromRepo();
            var uninstalled = allPortlets.
                Where(p => !repoPortlets.Any(r => r.GetProperty<string>("DisplayName") == p.Portlet.Name));
            return uninstalled;
        }

        protected void btnInstallPortlets_Click(object sender, EventArgs e)
        {
            var uninstalled = this.GetUninstalledPortlets().ToList();
            var categories = PortletInventory.GetCategories(uninstalled);
            var repoPortlets = PortletInventory.GetPortletsFromRepo();
            var repoCategories = PortletInventory.GetCategoriesFromRepo();
            foreach (var category in categories)
            {
                PortletInventory.ImportCategory(category, repoCategories);
            }
            foreach (var portlet in uninstalled)
            {
                try
                {
                    PortletInventory.ImportPortlet(portlet, repoPortlets);
                }
                catch (Exception ex)
                {
                    Logger.WriteException(ex);
                }
            }

            BindList();
            pnlSuccess.Visible = true;
        }
        protected void btnBack_Click(object sender, EventArgs e)
        {
            var page = this.Page as PageBase;
            if (page == null)
                return;

            page.Done();
        }
    }
}
