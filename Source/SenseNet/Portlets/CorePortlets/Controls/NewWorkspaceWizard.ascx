<%@ Language="C#" AutoEventWireup="true" %>

<asp:Label ID="ErrorMessage" runat="server" ForeColor="Red"></asp:Label>
<br />
<!-- be brave, you can remove the newnamelabel control :) -->
<asp:Label ID="NewNameLabel" runat="server" AssociatedControlID="NewNameText" Text="Workpace name:"></asp:Label>
<asp:TextBox ID="NewNameText" runat="server" Text="WorkspaceName"></asp:TextBox>
<asp:RequiredFieldValidator ID="NewNameReqValidator" ControlToValidate="NewNameText" Display="Dynamic" runat="server"></asp:RequiredFieldValidator>
<br />
<asp:DropDownList ID="WorkspaceList" runat="server"></asp:DropDownList>
<asp:Button ID="CreateWorkspaceButton" runat="server" Text="Create workspace..." />

