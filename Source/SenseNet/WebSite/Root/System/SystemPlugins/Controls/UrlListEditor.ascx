<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>

<asp:UpdatePanel id="updGridEditor" UpdateMode="Conditional" runat="server">
   <ContentTemplate>
        
        <asp:Panel ID="pnlConfigInfo" runat="server" Visible="false">
            * Please note that the url settings of the sites are currently configured in the web configuration file. In order that the following settings take effect remove the urlList settings from the web configuration or contact the system operator!
            <br />        
        </asp:Panel>
        
        <asp:LinkButton ID="ButtonAddRow" runat="server"  >
            <img src="/Root/Global/images/icons/16/add.png" alt='[add]' class="sn-icon sn-icon16"/>Add new site URL
        </asp:LinkButton>
        
        <br/><br/>

        <asp:ListView ID="InnerListView" runat="server" EnableViewState="false"  >
            <LayoutTemplate>      
                <table>
                  <tr style="background-color:#F5F5F5">  
                      <th>Site name</th>
                      <th>Authentication type</th>
                      <th> </th>
                  </tr>
                  <tr runat="server" id="itemPlaceHolder" />
                </table>
            </LayoutTemplate>
            <ItemTemplate>
                <tr>      		
                  <td><asp:TextBox ID="TextBoxSiteName" runat="server" /></td>
	              <td><asp:DropDownList ID="ListAuthenticationType" runat="server" Width="120px" >
	                    <asp:ListItem Value="Windows" Text="Windows" />
	                    <asp:ListItem Value="Forms" Text="Forms" />
	                    <asp:ListItem Value="None" Text="None" />
	                  </asp:DropDownList></td>
                  <td><asp:Button ID="ButtonRemoveRow" runat="server" CommandName="Remove" BorderStyle="None"
                                style="background-image: url('/Root/Global/images/icons/16/delete.png'); background-color:Transparent; width:17px; height:17px"/>
                                </td>
                </tr>
            </ItemTemplate>
            <EmptyDataTemplate>
            </EmptyDataTemplate>
        </asp:ListView>       
                
    </ContentTemplate>
</asp:UpdatePanel>