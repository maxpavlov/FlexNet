<%@ Control Language="C#" Inherits="SenseNet.Portal.Portlets.Controls.AuditLogControl" AutoEventWireup="true" %>
<div>
  <h1>
    Audit napló böngészése</h1>
  <asp:Table ID="filterTable" runat="server">
    <asp:TableRow>
      <asp:TableCell CssClass="sn-iu-label sn-iu-title">
        <asp:Label ID="Label1" runat="server" Text="<%$ Resources:AuditLogPortlet, StartDateLabel %>" />
      </asp:TableCell>
      <asp:TableCell CssClass="sn-iu-control">
        <asp:TextBox ID="startDate" runat="server" CssClass=" sn-ctrl sn-ctrl-text"></asp:TextBox>
      </asp:TableCell>
    </asp:TableRow>
    <asp:TableRow>
      <asp:TableCell CssClass="sn-iu-label sn-iu-title">
        <asp:Label ID="Label2" runat="server" Text="<%$ Resources:AuditLogPortlet, EndDateLabel %>" />
      </asp:TableCell>
      <asp:TableCell CssClass="sn-iu-control">
        <asp:TextBox ID="endDate" runat="server" CssClass=" sn-ctrl sn-ctrl-text"></asp:TextBox>
      </asp:TableCell>
    </asp:TableRow>
    <asp:TableRow>
      <asp:TableCell CssClass="sn-iu-label sn-iu-title">
        <asp:Label ID="Label3" runat="server" Text="<%$ Resources:AuditLogPortlet, UserLabel %>" />
      </asp:TableCell>
      <asp:TableCell CssClass="sn-iu-control">
        <asp:TextBox ID="userName" runat="server" CssClass=" sn-ctrl sn-ctrl-text"></asp:TextBox>
      </asp:TableCell>
    </asp:TableRow>
    <asp:TableRow>
      <asp:TableCell CssClass="sn-iu-label sn-iu-title">
        <asp:Label ID="Label4" runat="server" Text="<%$ Resources:AuditLogPortlet, EventLabel %>" />
      </asp:TableCell>
      <asp:TableCell CssClass="sn-iu-control">
        <asp:ListBox ID="eventList" runat="server" CssClass=" sn-ctrl sn-ctrl-text" DataSourceID="eventTypeDataSource" DataTextField="Title" DataValueField="Title"></asp:ListBox>
        <asp:SqlDataSource ID="eventTypeDataSource" runat="server" ProviderName="System.Data.SqlClient" SelectCommand="SELECT DISTINCT [Title] FROM [LogEntries] ORDER BY [Title]"></asp:SqlDataSource>
      </asp:TableCell>
    </asp:TableRow>
    <asp:TableRow>
      <asp:TableCell CssClass="sn-iu-label sn-iu-title">
        <asp:Label ID="Label5" runat="server" Text="<%$ Resources:AuditLogPortlet, ParamLabel %>" />
      </asp:TableCell>
      <asp:TableCell CssClass="sn-iu-control">
        <asp:TextBox ID="parameter" runat="server" CssClass=" sn-ctrl sn-ctrl-text"></asp:TextBox>
      </asp:TableCell>
    </asp:TableRow>
  </asp:Table>
  <div style="text-align: right;">
    <asp:Button ID="filterButton" runat="server" Text="<%$ Resources:AuditLogPortlet, ListButton %>" OnClick="FilterBtnOnClick" />
  </div>
</div>
<div style="overflow-x: scroll;">
  <asp:GridView ID="auditLogGridView" runat="server" DataSourceID="auditlogDataSource" AllowPaging="True" AutoGenerateColumns="False" DataKeyNames="LogId" 
    PageSize="5">
    <Columns>
      <asp:BoundField DataField="LogId" HeaderText="<%$ Resources:AuditLogPortlet, IdTitle %>" InsertVisible="False" ReadOnly="True" SortExpression="LogId" />
      <asp:BoundField DataField="Timestamp" HeaderText="<%$ Resources:AuditLogPortlet, DateTitle %>" SortExpression="Timestamp" />
      <asp:BoundField DataField="UserName" HeaderText="<%$ Resources:AuditLogPortlet, UserTitle %>" SortExpression="UserName" />
      <asp:BoundField DataField="ContentPath" HeaderText="<%$ Resources:AuditLogPortlet, ContentPathTitle %>" SortExpression="ContentPath" />
      <asp:BoundField DataField="Message" HeaderText="<%$ Resources:AuditLogPortlet, EventTitle %>" SortExpression="Message" />
      <%--<asp:BoundField DataField="Title" HeaderText="Title" SortExpression="Title" />
      
      <asp:BoundField DataField="EventId" HeaderText="EventId" SortExpression="EventId" />
      <asp:BoundField DataField="Priority" HeaderText="Priority" SortExpression="Priority" />
      <asp:BoundField DataField="Severity" HeaderText="Severity" SortExpression="Severity" />
      <asp:BoundField DataField="MachineName" HeaderText="MachineName" SortExpression="MachineName" />
      <asp:BoundField DataField="AppDomainName" HeaderText="AppDomainName" SortExpression="AppDomainName" />
      <asp:BoundField DataField="ProcessID" HeaderText="ProcessID" SortExpression="ProcessID" />
      <asp:BoundField DataField="ProcessName" HeaderText="ProcessName" SortExpression="ProcessName" />
      <asp:BoundField DataField="ThreadName" HeaderText="ThreadName" SortExpression="ThreadName" />
      <asp:BoundField DataField="Win32ThreadId" HeaderText="Win32ThreadId" SortExpression="Win32ThreadId" />--%>
      <asp:TemplateField HeaderText="<%$ Resources:AuditLogPortlet, ParamsTitle %>" SortExpression="FormattedMessage">
        <ItemTemplate>
          <div style="overflow: scroll; width: 300px; height: 240px;">
            <asp:Literal ID="literal" runat="server" Text='<%# Bind("FormattedMessage") %>' Mode="Encode" />
          </div>
        </ItemTemplate>
      </asp:TemplateField>
    </Columns>
  </asp:GridView>
  <asp:SqlDataSource ID="auditlogDataSource" runat="server" ProviderName="System.Data.SqlClient" SelectCommand="proc_LogSelect" CancelSelectOnNullParameter="False" SelectCommandType="StoredProcedure">
    <SelectParameters>
      <asp:Parameter DefaultValue="" Name="startDate" Type="DateTime" />
      <asp:Parameter DefaultValue="" Name="endDate" Type="DateTime" />
      <asp:Parameter DefaultValue="" Name="usrName" Type="String" />
      <asp:Parameter DefaultValue="" Name="title" Type="String" />
      <asp:Parameter DefaultValue="" Name="params" Type="String" />
    </SelectParameters>
  </asp:SqlDataSource>
</div>
