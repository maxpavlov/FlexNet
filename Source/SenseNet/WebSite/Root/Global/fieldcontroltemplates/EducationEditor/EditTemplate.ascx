<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>

<asp:UpdatePanel id="updGridEditor" UpdateMode="Conditional" runat="server">
   <ContentTemplate>
        
        <asp:LinkButton ID="ButtonAddRow" runat="server">
            <img src="/Root/Global/images/icons/16/add.png" alt='[add]' class="sn-icon sn-icon16"/>Add school
        </asp:LinkButton>
        
        <br/><br/>

        <asp:ListView ID="InnerListView" runat="server" EnableViewState="false"  >
            <LayoutTemplate>      
                <table >
                  <tr runat="server" id="itemPlaceHolder" />
                </table>
            </LayoutTemplate>
            <ItemTemplate>
                <tr>      		
                  <td><asp:TextBox ID="tbSchoolName" runat="server" Width="250px" /></td>
                  <td><asp:Button ID="ButtonRemoveRow" runat="server" CommandName="Remove" BorderStyle="None" 
                        style="background-image: url('/Root/Global/images/icons/16/delete.png'); background-color:Transparent; width:17px; height:17px;"/></td>
                </tr>
            </ItemTemplate>
            <EmptyDataTemplate>
            </EmptyDataTemplate>
        </asp:ListView>       
                
    </ContentTemplate>
</asp:UpdatePanel>