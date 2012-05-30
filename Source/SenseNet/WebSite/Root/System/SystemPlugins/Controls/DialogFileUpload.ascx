<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.Controls.DialogFileUpload" %>

<div style="width:310px;">
    <asp:PlaceHolder Visible="false" ID="ErrorPlaceHolder" runat="server">
        <asp:Label CssClass="sn-iu-error" ID="ErrorLabel" runat="server" />
    </asp:PlaceHolder>

    <asp:Repeater ID="UploadedFiles" runat="server">
        <ItemTemplate>
            <asp:Button ID="DeleteFile" runat="server" Text="X" CommandName=<%# DataBinder.Eval(Container.DataItem, "Path")%> ToolTip="Remove uploaded file" />
            <%# DataBinder.Eval(Container.DataItem, "FileName") %>
            <br />
        </ItemTemplate>
        <FooterTemplate>
            <hr />
        </FooterTemplate>
    </asp:Repeater>
    <asp:FileUpload ID="Upload" runat="server" />
    <asp:Button ID="UploadButton" runat="server" Text="Upload" CssClass="sn-submit" />
</div>
