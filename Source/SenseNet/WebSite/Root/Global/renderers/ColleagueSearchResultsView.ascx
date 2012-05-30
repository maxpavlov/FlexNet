<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.ContentCollectionView" %>
<%@ Import Namespace="SenseNet.Portal.UI" %>

<script runat="server" type="text/C#">
private string GetImageUrl(string path)
{
    var url = ContentTools.GetImageUrlFromImageField(path);
    if (!string.IsNullOrEmpty(url))
    {
        return url;
    }else
    {
        return "/Root/Global/images/orgc-missinguser.png"; 
    } 
}
</script>

<asp:ListView ID="ContentList" runat="server" EnableViewState="false">
    <LayoutTemplate>
        <div class="sn-pt-body">
            <asp:PlaceHolder ID="itemPlaceHolder" runat="server" />
        </div>
    </LayoutTemplate>
    <ItemTemplate>
        <div class="sn-user">           
            <sn:ActionLinkButton NodePath='<%#Eval("Path") %>' ActionName="Browse" IconVisible="false"
                runat="server" CssClass="sn-user-link">
                <asp:Image ID="Image1" CssClass="sn-pic sn-pic-left" runat="server" ImageUrl='<%# GetImageUrl(Eval("Path").ToString()) %>'
                Height="64" Width="64" AlternateText="Missing User Image" BorderColor="White" BorderStyle="Solid" BorderWidth="1px" />
                
                <div class="sn-user-properties">
                    <table>
                        <tr><td><%#Eval("FullName") %> (<%#Eval("Name") %>)</td></tr>
                        <tr><td>Department:<td></td> <%#Eval("Department") %></td></tr>
                        <tr><td>E-mail: <td></td><%#Eval("Email") %></td></tr>
                    </table>
                </div>             
            </sn:ActionLinkButton>
        </div>
    </ItemTemplate>
</asp:ListView>