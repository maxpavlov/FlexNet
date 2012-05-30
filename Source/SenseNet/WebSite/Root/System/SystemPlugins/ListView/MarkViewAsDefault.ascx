<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>
<%@ Import Namespace="SenseNet.ContentRepository" %>
<%@ Import Namespace="SenseNet.Portal" %>
<%@ Import Namespace="SenseNet.Portal.Virtualization" %>
<%@ Import Namespace="SenseNet.Portal.UI.ContentListViews.Handlers" %>
<%@ Import Namespace="SenseNet.Portal.UI.PortletFramework" %>

<script runat="server">

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        var ctxView = PortalContext.Current.ContextNode as ViewBase;
        if (ctxView != null)
        {
            var list = ContentList.GetContentListByParentWalk(ctxView);
            if (list != null)
            {
                list.DefaultView = ctxView.Name;
                list.Save(SavingMode.KeepVersion);
            }
        }
        
        var p = this.Page as PageBase;
        if (p != null)
            p.Done(false);
    }

</script>
