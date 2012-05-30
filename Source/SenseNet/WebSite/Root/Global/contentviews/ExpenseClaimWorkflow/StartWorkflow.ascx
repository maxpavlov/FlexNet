<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<%@ Register Assembly="SenseNet.Portal" Namespace="SenseNet.Portal.UI.Controls" TagPrefix="sn" %>
<%@ Import Namespace="SenseNet.ContentRepository" %>
<%@ Import Namespace="SenseNet.ContentRepository.Storage" %>
<%@ Import Namespace="SenseNet.Portal.UI" %>

<script runat="server">
    ExpenseClaim relatedContent;
    
    protected override void OnInit(EventArgs e)
    {
        relatedContent = (ExpenseClaim)this.Content["RelatedContent"];
        DataSourceExpenseClaimItems.ContentPath = relatedContent.Path;

        base.OnInit(e);
    }
</script>

<sn:SenseNetDataSource ID="DataSourceExpenseClaimItems" runat="server" />

<div id="InlineViewContent" runat="server" class="sn-content sn-content-inlineview">

<asp:ListView ID="ListViewExpenseClaimItems" runat="server" DataSourceID="DataSourceExpenseClaimItems" >
    <LayoutTemplate>
        <table class="sn-listgrid ui-widget-content">
            <thead>
                <tr>  
                    <th class="sn-lg-col-1 ui-state-default">DisplayName</th>
                    <th class="sn-lg-col-2 ui-state-default">Amount</th>
                    <th class="sn-lg-col-3 ui-state-default">Currency</th>
                    <th class="sn-lg-col-4 ui-state-default">Date</th>
                    <th class="sn-lg-col-5 ui-state-default">Description</th>
                </tr>
            </thead>
            <tbody>
                <tr runat="server" id="itemPlaceHolder" />
            </tbody>
        </table>
    </LayoutTemplate>
    
    <ItemTemplate>
        <tr class="sn-lg-row0 ui-widget-content">
            <td><%# Eval("DisplayName")%></td>
            <td><%# Eval("Amount") %></td>
            <td><%# Eval("Currency")%></td>
            <td><%# Eval("Date")%></td>
            <td><%# Eval("Description")%></td>
        </tr>
    </ItemTemplate>
    
    <EmptyDataTemplate>
        no items
    </EmptyDataTemplate>
</asp:ListView>

<br/>
<div class="sn-inputunit ui-helper-clearfix">
	<div class=sn-iu-label>
		<span class=sn-iu-title>Approver</span> <br/>
		<span class=sn-iu-desc>The user who will be responsible for approving the expense claim</span> 
	</div>
	<div class=sn-iu-control>
        <span><%= relatedContent.GetApprover() %></span>
	</div>
</div>

</div>
<div class="sn-panel sn-buttons">
      <% 
        var doc = this.Content["RelatedContent"] as Node;
        if (doc != null && doc.Version.Status == VersionStatus.Pending ) { %>
        <asp:Button class="sn-submit" ID="StartWorkflow" runat="server" Text="START" />
    <% } else { %>
        <%= IconHelper.RenderIconTag("warning", null, 32) %>
        <span>The content must be in <strong>Pending</strong> state to start the approval process</span>
    <% } %>
    <sn:BackButton CssClass="sn-submit" Text="Cancel" ID="BackButton1" runat="server" />
</div>
