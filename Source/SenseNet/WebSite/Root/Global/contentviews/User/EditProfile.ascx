<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>

<div class="sn-content sn-content-inlineview">
    
    <sn:ErrorView ID="ErrorView1" runat="server" />
    <h1><%= GetValue("FullName") %></h1>
    <sn:ShortText ID="ShortText0" runat="server" FieldName="Name" ControlMode="Edit">
      <EditTemplate>
        <asp:TextBox ID="InnerShortText" Class="sn-ctrl sn-ctrl-text sn-ctrl-username" runat="server"></asp:TextBox>
      </EditTemplate>
    </sn:ShortText>
    <sn:ShortText ID="ShortText1" runat="server" FieldName="FullName" ControlMode="Edit" />
    <sn:Password ID="Password1" runat="server" FieldName="Password" RenderMode="Edit" />
    <sn:Image ID="Image1" CssClass="sn-avatar" runat="server" FieldName="Avatar" ControlMode="Edit">
        <browsetemplate>
            <asp:Image CssClass="sn-avatar" ImageUrl="/Root/Global/images/default-avatar.png" ID="ImageControl" runat="server" alt="Missing User Image" title="" />
        </browsetemplate>
    </sn:Image>
    <h2>Contacts</h2>
    <sn:ShortText ID="ShortText2" runat="server" FieldName="Email" ControlMode="Edit" />
    <sn:ShortText ID="ShortText5" runat="server" FieldName="Phone" ControlMode="Edit" />
    <h2>Work</h2>
    <sn:ReferenceGrid ID="ReferenceGrid1" runat="server" FieldName="Manager" ControlMode="Edit"/>
    <sn:ShortText ID="ShortText3" runat="server" FieldName="Department" ControlMode="Edit" />
    <h2>Personal</h2>    
    <sn:DatePicker runat="server" FieldName="BirthDate" ScriptDisabled="false" ServerDateFormat="yyyy.MM.dd" ControlMode="Edit"/>
    <sn:DropDown ID="DropDown1" runat="server" FieldName="Gender" ControlMode="Edit"/>
    <sn:DropDown ID="DropDown2" runat="server" FieldName="MaritalStatus" ControlMode="Edit"/>
    <h2>Skills</h2>
    <sn:EducationEditor ID="LongText1" runat="server" FieldName="Education" ControlMode="Edit"/>
    <sn:ShortText ID="ShortText4" runat="server" FieldName="Languages" ControlMode="Edit" />
    <h2>Community</h2>
    <sn:ShortText ID="ShortText6" runat="server" FieldName="TwitterAccount" ControlMode="Edit" />
    <sn:ShortText ID="ShortText7" runat="server" FieldName="FacebookURL" ControlMode="Edit" />
    <sn:ShortText ID="ShortText8" runat="server" FieldName="LinkedInURL" ControlMode="Edit" />
</div>

<div class="sn-panel sn-buttons">
  <sn:CommandButtons ID="CommandButtons1" runat="server"/>
</div>

