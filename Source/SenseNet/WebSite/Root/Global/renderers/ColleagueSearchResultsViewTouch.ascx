<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.ContentCollectionView" %>
<%@ Import Namespace="SenseNet.Portal.UI" %>

<script runat="server" type="text/C#">
    private string GetImageUrl(string path)
    {
        var url = ContentTools.GetImageUrlFromImageField(path);
        if (!string.IsNullOrEmpty(url))
        {
            return url;
        }
        else
        {
            return "/Root/Global/images/orgc-missinguser.png";
        }
    }
</script>

<asp:ListView ID="ContentList" runat="server" EnableViewState="false">
    <LayoutTemplate>
        <div class="sn-tch-searchres">
            <asp:PlaceHolder ID="itemPlaceHolder" runat="server" />
        </div>
    </LayoutTemplate>
    <ItemTemplate>
        <div id="sn-tch-user" class="sn-tch-user">
            <sn:ActionLinkButton ID="ActionLinkButton1" NodePath='<%#Eval("Path") %>' ActionName="Browse"
                IconVisible="false" runat="server" CssClass="sn-tch-userlink" OnMouseOver="Changeclass();">
                <div class="sn-pic sn-pic-left">
                    <asp:Image ID="Image1" runat="server" ImageUrl='<%# GetImageUrl(Eval("Path").ToString()) %>'
                Height="128" Width="128" AlternateText="Missing User Image" BorderColor="White" BorderStyle="Solid" BorderWidth="1px" />
                </div>
                            
                <div class="sn-user-properties">
                    <div class="sn-tch-prop"><h1><%#Eval("FullName") %> (<%#Eval("Name") %>)</h1></div>
                    <div class="sn-tch-prop"><h2>Department: <%#Eval("Department") %></h2></div>
                    <div class="sn-tch-prop"><h2>E-mail: <%# Eval("Email") %></h2></div>
                </div>           
            </sn:ActionLinkButton>
        </div>
    </ItemTemplate>
</asp:ListView>
