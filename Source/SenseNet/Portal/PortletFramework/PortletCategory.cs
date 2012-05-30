using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.Portal.UI.PortletFramework
{
    public enum PortletCategoryType
    {
        None = 0,
        Navigation,
        Portal,
        Content,
        Collection,
        Search,
        Application,
        System,
        Custom,
        ContentOperation,
        KPI,
        Other,
        Workflow,
        Enterprise20
    }
    public class PortletCategory
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public PortletCategory(PortletCategoryType type)
        {
            switch (type)
            {
                case PortletCategoryType.Navigation:
                    this.Title = "Navigation";
                    this.Description = "Portlets used for navigating in portal";
                    break;
                case PortletCategoryType.Portal:
                    this.Title = "Portal";
                    this.Description = "Portal functions related portlets";
                    break;
                case PortletCategoryType.Content:
                    this.Title = "Content";
                    this.Description = "Content portlets";
                    break;
                case PortletCategoryType.ContentOperation:
                    this.Title = "Content operation";
                    this.Description = "Portlets for basic content operations";
                    break;
                case PortletCategoryType.Collection:
                    this.Title = "Content collection";
                    this.Description = "Content collection portlets";
                    break;
                case PortletCategoryType.Application:
                    this.Title = "Application";
                    this.Description = "Basic application portlets";
                    break;
                case PortletCategoryType.System:
                    this.Title = "System";
                    this.Description = "System related portlets";
                    break;
                case PortletCategoryType.Search:
                    this.Title = "Search";
                    this.Description = "Search portlets";
                    break;
                case PortletCategoryType.Custom:
                    this.Title = "Custom";
                    this.Description = "Custom portlets designed for current portal";
                    break;
                case PortletCategoryType.KPI:
                    this.Title = "KPI";
                    this.Description = "Workspace KPI portlets";
                    break;
                case PortletCategoryType.Other:
                    this.Title = "Other";
                    this.Description = "Other portlets";
                    break;
                case PortletCategoryType.Workflow:
                    this.Title = "Workflow";
                    this.Description = "Workflow related portlets";
                    break;
                case PortletCategoryType.Enterprise20:
                    this.Title = "Enterprise 2.0";
                    this.Description = "ECM 2.0 features";
                    break;
            }
        }
        public PortletCategory(string title, string description)
        {
            this.Title = title;
            this.Description = description;
        }
    }
}
