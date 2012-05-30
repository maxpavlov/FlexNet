using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI;
using SenseNet.Portal.UI.PortletFramework;
using sn = SenseNet.ContentRepository;
using SenseNet.Portal.PortletFramework;
using SenseNet.Portal.Virtualization;

namespace SenseNet.Portal.UI.Controls
{
  public class SmartMenu : Panel
  {
    #region properties

    // Get the NodePath attribute from the viewstate (it's the WebControl class
    // that puts it there, from the declarative markup attributes)
      public string NodePath
      {
          get
          {
              string s = (string)this.ViewState["NodePath"];
              if (s == null) return string.Empty;
              return s;
          }
          set
          {
              this.ViewState["NodePath"] = value;
          }
      }

      public string ContextInfoID
      {
          get
          {
              string s = (string)this.ViewState["ContextInfoID"];
              if (s == null) return string.Empty;
              return s;
          }
          set
          {
              this.ViewState["ContextInfoID"] = value;
          }
      }

      public string Css { get; set; }
      public string Scenario { get; set; }

      public string Text { get; set; }

      private ContextInfo FindContextInfo(string controlID)
      {
          if (string.IsNullOrEmpty(controlID))
              return null;

          var nc = this as Control;
          Control control = null;

          while (control == null && nc != null)
          {
              nc = nc.NamingContainer;
              control = nc.FindControl(controlID);
          }

          return control as ContextInfo;
      }

    #endregion

    #region snake's poison tooth is removed

    //protected override void OnLoad(EventArgs e)
    //{
    //    UITools.AddExtCssFiles();
    //    UITools.AddSnUxScripts();
    //    UITools.RegisterStartupScript(this.ClientID + "_script", String.Format(@"sn.ux.smartMenu.Init($('#{0}'));", this.ClientID), Page);
    //    base.OnLoad(e);
    //}

      //public override void RenderBeginTag(HtmlTextWriter writer)
      //{
      //    this.CssClass = String.Format("sn-ux-smartmenu {0}", this.Css);
      //    var context = FindContextInfo(this.ContextInfoID);
      //    var portlet = ContextBoundPortlet.GetContainingContextBoundPortlet(this);

      //    string path = !String.IsNullOrEmpty(ContextInfoID) ? context.Path : NodePath;

      //    //hack hack hack
      //    string encodedReturnUrl = Uri.EscapeDataString(PortalContext.Current.RequestedUri.PathAndQuery.ToString());
      //    string callbackUrl;
      //    if (Scenario == "Views")
      //        callbackUrl = String.Format("/ContentListViewHelper.mvc/GetAvailableViews?path={0}&uiContextId={1}&back={2}",
      //            path, portlet.ID, encodedReturnUrl);
      //    else
      //        callbackUrl = String.Format("/SmartAppHelper.mvc/GetActions?path={0}&scenario={1}&back={2}",
      //            path, Scenario, encodedReturnUrl);

      //    /*      var callbackUrl = !String.IsNullOrEmpty(ContextInfoID) && portlet != null ?
      //            PortalActionLinkResolver.FormatActionLink(context.Path, portlet.ID, this.Scenario) :
      //            PortalActionLinkResolver.Instance.ResolveRelative(NodePath, "ListActions"); */

      //    this.Attributes.Add("CallbackUrl", callbackUrl);

      //    base.RenderBeginTag(writer);
      //}

      //public override void RenderEndTag(HtmlTextWriter writer)
      //{
      //    if (!String.IsNullOrEmpty(Text))
      //        writer.Write(Text);
      //    base.RenderEndTag(writer);
      //}

    #endregion
  }
}
