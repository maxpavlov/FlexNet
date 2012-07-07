<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" EnableViewState="true" %>
<%@ Import Namespace="SenseNet.ApplicationModel" %>

<asp:Panel ID="ActionListPanel" runat="server" CssClass="sn-actionlist">  
    
    <asp:ListView ID="ActionListView" runat="server" EnableViewState="true" OnSorting="ListView_Sorting" >
       <LayoutTemplate>
            <table class="sn-listgrid ui-widget-content">
                <thead>
                    <tr class="ui-widget-content">
                        <th class="sn-lg-col-1 ui-state-default" style="width:160px"><asp:LinkButton ID="HeaderLink1" runat="server" CommandName="Sort" CommandArgument="Action">Action</asp:LinkButton></th>
                        <th class="sn-lg-col-2 ui-state-default" style="width:350px"><asp:LinkButton ID="HeaderLink2" runat="server" CommandName="Sort" CommandArgument="Path">Application path</asp:LinkButton></th>
                        <th class="sn-lg-col-3 ui-state-default"><asp:LinkButton ID="HeaderLink3" runat="server" CommandName="Sort" CommandArgument="Scenario">Scenario(s)</asp:LinkButton></th>
                        <th class="sn-lg-col-4 ui-state-default" style="width:100px"></th>
                        <th class="sn-lg-col-5 ui-state-default" style="width:100px"></th>
                    </tr>
                </thead>
                <tbody>
                    <tr runat="server" id="itemPlaceHolder"></tr>
                </tbody>
            </table>               
       </LayoutTemplate>
       <ItemTemplate>
            <tr class="sn-lg-row0 ui-widget-content">
                <td><sn:ActionLinkButton runat="server" ID="ActionLink" /></td>
                <td><sn:ActionLinkButton runat="server" ID="AppBrowseAction" ActionName="Browse" IconVisible="false" 
                            NodePath='<%# ((ActionBase)Container.DataItem).GetApplication() == null ? string.Empty : ((ActionBase)Container.DataItem).GetApplication().Path %>' 
                            Text='<%# ((ActionBase)Container.DataItem).GetApplication() == null ? string.Empty : ((ActionBase)Container.DataItem).GetApplication().Path %>' /></td>
                <td><%# ((ActionBase)Container.DataItem).GetApplication() == null ? string.Empty : ((ActionBase)Container.DataItem).GetApplication().Scenario %></td>
                <td><sn:ActionLinkButton ID="ActionLinkButton1" runat="server" ActionName="CopyAppLocal" ParameterString="nodepath={CurrentContextPath}" Text="Copy local" 
                            NodePath='<%# ((ActionBase)Container.DataItem).GetApplication() == null ? string.Empty : ((ActionBase)Container.DataItem).GetApplication().Path %>' /></td>
                <td><sn:ActionLinkButton ID="ActionLinkButton2" runat="server" ActionName="DeleteLocal" Text="Delete local" 
                            NodePath='<%# ((ActionBase)Container.DataItem).GetApplication() == null ? string.Empty : ((ActionBase)Container.DataItem).GetApplication().Path %>' /></td>
            </tr>
       </ItemTemplate>
       <EmptyDataTemplate>
        No actions available
       </EmptyDataTemplate>
    </asp:ListView> 
    
</asp:Panel>

<script runat="server">
    
    protected void ListView_Sorting(object sender, ListViewSortEventArgs e)
    {
        var lv = sender as System.Web.UI.WebControls.ListView;
        if (lv == null)
            return;
        
        var actions = lv.DataSource as List<ActionBase>;
        if (actions == null)
            return;
        
        switch (e.SortExpression)
        {
            case "Action": actions.Sort((xa, ya) => xa.Text.CompareTo(ya.Text)); break;
            case "Path": actions.Sort(CompareActionsByAppPath); break;
            case "Scenario": actions.Sort(CompareActionsByAppScenario); break;
        }

        lv.DataBind();
    }
    
    private static int CompareActionsByAppPath(ActionBase x, ActionBase y)
    {
        int result = 0;
        if (FinishCompare(x, y, out result))
            return result;

        return x.GetApplication().Path.CompareTo(y.GetApplication().Path);
    }

    private static int CompareActionsByAppScenario(ActionBase x, ActionBase y)
    {
        int result = 0;
        if (FinishCompare(x, y, out result))
            return result;

        return (x.GetApplication().Scenario ?? string.Empty).CompareTo(y.GetApplication().Scenario ?? string.Empty);
    }
    
    private static bool FinishCompare(ActionBase x, ActionBase y, out int result)
    {
        if (x == null && y == null)
        {
            result = 0;
            return true;
        }
        if (x == null)
        {
            result = -1;
            return true;
        }
        if (y == null)
        {
            result = 1;
            return true;
        }

        var appx = x.GetApplication();
        var appy = y.GetApplication();

        if (appx == null && appy == null)
        {
            result = 0;
            return true;
        }
        if (appx == null)
        {
            result = -1;
            return true;
        }
        if (appy == null)
        {
            result = 1;
            return true;
        }

        result = 0;
        return false;
    }
    
</script>