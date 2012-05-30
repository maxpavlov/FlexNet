<%@  Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<%@ Import Namespace="SenseNet.ContentRepository" %>
<%@ Import Namespace="SenseNet.Portal.UI" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>

<sn:ErrorView ID="ErrorView1" runat="server" />

<% var eClaim = this.Content.ContentHandler.GetReference<ExpenseClaim>("ContentToApprove");
   var assignee = ((IEnumerable<SenseNet.ContentRepository.Storage.Node>)this.Content["AssignedTo"]).FirstOrDefault();
    if (eClaim != null) { %>

<div class="sn-inputunit ui-helper-clearfix" id="InputUnitPanel1">
    <div class="sn-iu-label">
        <asp:Label CssClass="sn-iu-title" ID="LabelForTitle" runat="server"><%=this.Content.Fields["ContentToApprove"].DisplayName %></asp:Label>
    </div>
    <div class="sn-iu-control">
        <%= IconHelper.RenderIconTag(eClaim.Icon)%>
        <a href='<%= Actions.BrowseUrl(SenseNet.ContentRepository.Content.Create(eClaim)) %>' title='<%=eClaim.DisplayName %>'><%=eClaim.DisplayName %></a> (in 
        <a href='<%= Actions.BrowseUrl(SenseNet.ContentRepository.Content.Create(eClaim.Parent)) %>' title='<%=eClaim.ParentPath %>' target="_blank"><%=eClaim.Parent.DisplayName%></a>)     
    </div>
</div>
   <% } %>

<div class="sn-inputunit ui-helper-clearfix">
	<div class=sn-iu-label>
		<span class=sn-iu-title>Expense claim items</span> <br/>
		<span class=sn-iu-desc></span> 
	</div>
	<div class=sn-iu-control>
        <table class="sn-listgrid ui-widget-content">
            <thead>
                <tr>  
                    <th class="sn-lg-col-1 ui-state-default">Display Name</th>
                    <th class="sn-lg-col-2 ui-state-default">Amount</th>
                    <th class="sn-lg-col-3 ui-state-default">Currency</th>
                </tr>
            </thead>
            <tbody>
        <% foreach (var ecItemNode in eClaim.Children) { %>            
            <tr class="sn-lg-row0 ui-widget-content">
                <td><%= ecItemNode.DisplayName %></td>
                <td><%= ecItemNode["Amount"] %></td>
                <td><%= ecItemNode["Currency"] %></td>
            </tr>
        <% } %>
            </tbody>
        </table>
        <br />
        <span>SUM: </span><strong><%= eClaim.Sum %></strong>
	</div>
</div>

 <sn:GenericFieldControl ID="GenericFields2" runat="server" FieldsOrder="DueDate Description" />

 <div class="sn-inputunit ui-helper-clearfix">
	<div class=sn-iu-label>
		<span class=sn-iu-title>Assigned to</span> <br/>
		<span class=sn-iu-desc></span> 
	</div>
	<div class=sn-iu-control>
        <span><%= assignee == null ? string.Empty : assignee.DisplayName %></span>
    </div>
</div>

<div class="sn-panel sn-buttons">
<% var status = this.Content.ContentHandler.GetProperty<string>("Result");
   if (status == null)
   { %>
    <asp:Button CssClass="sn-submit" Text="Approve" ID="Approve" runat="server" />
    <asp:Button CssClass="sn-submit" Text="Reject" ID="Reject" runat="server" />
    <sn:BackButton CssClass="sn-submit" Text="Cancel" ID="BackButton1" runat="server" />
    <% }
   else
   { %>
   Task already completed with answer:<strong> <%=GetValue("Result") %></strong>
   <sn:BackButton CssClass="sn-submit" Text="Done" ID="BackButton2" runat="server" />
    <% } %>
    
</div>