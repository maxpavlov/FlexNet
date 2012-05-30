<%@ Language="C#" AutoEventWireup="true" %>

<asp:Label ID="ErrorMessage" runat="server" ForeColor="Red"></asp:Label>

<asp:DropDownList ID="ContentTypeList" runat="server"></asp:DropDownList>
<asp:Button ID="SelectContentTypeButton" runat="server" Text="Select" />

<asp:PlaceHolder ID="ContentViewPlaceHolder" runat="server"></asp:PlaceHolder>