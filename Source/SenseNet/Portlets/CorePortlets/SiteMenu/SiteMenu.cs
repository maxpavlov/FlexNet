
using SenseNet.Portal.UI.PortletFramework;
namespace SenseNet.Portal.Portlets
{
    public class SiteMenu : SiteMenuBase
    {
        public SiteMenu()
        {
            this.Name = "Site menu";
            this.Description = "This portlet displays a menu that users can traverse to get to different pages in your site (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.Navigation);
        }
    }
}
