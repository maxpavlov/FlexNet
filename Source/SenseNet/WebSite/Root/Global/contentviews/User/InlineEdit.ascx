<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<%@ Register Assembly="SenseNet.Portal" Namespace="SenseNet.Portal.UI.Controls" TagPrefix="sn" %>

<div class="sn-user">
    <sn:Image ID="Image" runat="server" FieldName="Avatar" FrameMode="NoFrame">
        <EditTemplate>
            <div><asp:Image CssClass="sn-pic sn-pic-left" ImageUrl="/Root/Global/images/orgc-missinguser.png" Width="128" Height="128" ID="ImageControl" runat="server" alt="Missing User Image" title="" /></div>
            <div class="sn-user-upload">
                <asp:FileUpload ID="FileUploadControl" runat="server" /><br />
                <asp:Label ID="Label1" AssociatedControlID="ImageIsReferenceControl" runat="server">Image is persisted to repository:</asp:Label> 
                <asp:CheckBox ID="ImageIsReferenceControl" runat="server" />
            </div>
        </EditTemplate>
    </sn:Image>
    
    <div class="sn-user-actions">
        <sn:ActionLinkButton ID="BtnEdit" CssClass="sn-user-btnedit" runat="server" ActionName="Browse" />
        <sn:ActionLinkButton ID="BtnDelete" CssClass="sn-user-btndelete" runat="server" ActionName="Delete" />
        <sn:ActionLinkButton ID="BtnExplore" CssClass="sn-user-btnexplore" runat="server" ActionName="Explore" />
        <sn:ActionLinkButton ID="BtnVersion" CssClass="sn-user-btnversion" runat="server" ActionName="Versions" />
    </div>
</div>​
    
<sn:ShortText runat="server" ID="ShortTextFullName" FieldName="FullName" />
<sn:ShortText runat="server" ID="ShortTextName" FieldName="Name">
  <EditTemplate>
    <asp:TextBox ID="InnerShortText" Class="sn-ctrl sn-ctrl-text sn-ctrl-username" runat="server"></asp:TextBox>
  </EditTemplate>
</sn:ShortText>
<sn:Boolean runat="server" ID="Boolean1" FieldName="Enabled" />
<sn:Password runat="server" ID="Password1" FieldName="Password" />
<sn:ShortText runat="server" ID="ShortTextEmail" FieldName="Email" />
<sn:ShortText runat="server" ID="ShortTextDomain" FieldName="Domain" />
<sn:GenericFieldControl runat="server" ID="GenericFieldControl1" 
    ExcludedFields="Avatar Name FullName Enabled Password Email Domain Version Index" />

<div class="sn-panel sn-buttons">
    <sn:CommandButtons ID="CommandButtons1" runat="server" />
</div>
