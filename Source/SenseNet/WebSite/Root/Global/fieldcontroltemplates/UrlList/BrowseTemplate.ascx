<%@  Language="C#" %>
<asp:ListView ID="InnerListView" runat="server" EnableViewState="false" >
    <LayoutTemplate>
        <table class="sn-ctrl-urllist" >
          <tr style="background-color:#F5F5F5">  
              <th>Site name</th>    
              <th>Authentication type</th>	   
          </tr>
          <tr runat="server" id="itemPlaceHolder" />
        </table>
    </LayoutTemplate>
    <ItemTemplate>
        <tr>      		
          <td><asp:Label ID="LabelSiteName" runat="server" /></td>
          <td><asp:Label ID="LabelAuthType" runat="server" /></td> 	              	              
        </tr>
    </ItemTemplate>
    <EmptyDataTemplate>
    </EmptyDataTemplate>
</asp:ListView>   

