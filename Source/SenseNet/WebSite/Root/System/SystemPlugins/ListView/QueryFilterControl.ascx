<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>

<asp:UpdatePanel id="updAdvancedSearch" UpdateMode="Conditional" runat="server">
   <ContentTemplate>
        
        <div>
            <input type="button" id="btnLucene" value="Text mode" onclick="$('.sn-filter-lucpanel').show();$('.sn-filter-nodequerypanel').hide();return false;" class="sn-submit" /> 
            <input type="button" id="btnNodeQ" value="Builder mode" onclick="$('.sn-filter-lucpanel').hide();$('.sn-filter-nodequerypanel').show();return false;" class="sn-submit" />
        </div>
        <br />
        <div id="panelLucId" style="display:block;" runat="server" class="sn-filter-lucpanel">
            <asp:TextBox ID="tbLucene" runat="server" TextMode="MultiLine" Rows="5" Width="519px"></asp:TextBox>
        </div>
        <div id="panelNQId" style="display:none;" runat="server" class="sn-filter-nodequerypanel">
<asp:LinkButton ID="ButtonAddRule" runat="server" Text = "+ Add rule" ></asp:LinkButton>
<br />

        <asp:ListView ID="InnerListView" runat="server" EnableViewState="false" >
            <LayoutTemplate>      
                <table style="width:100%" >
                  <tr style="background-color:#F5F5F5">  
                      <th> </th>	                
                      <th>Field</th>	   
                      <th>Operator</th>    
                      <th>Expression</th>           
                      <th> </th>
                  </tr>
                  <tr runat="server" id="itemPlaceHolder" />
                </table>
            </LayoutTemplate>
            <ItemTemplate>
                <tr>      		
                  <td><asp:Button ID="btnRemove" runat="server" CommandName="Remove" Text="X" /></td>    	                   
	              <td><asp:DropDownList ID="ddField" runat="server" Width="150px"></asp:DropDownList></td> 
	              <td><asp:DropDownList ID="ddRelOp" runat="server" Width="140px">
	                    <asp:ListItem Text="Equal" Value="0" />
	                    <asp:ListItem Text="Not equal" Value="1" />
	                    <asp:ListItem Text="Less than" Value="2" />
	                    <asp:ListItem Text="Greater than" Value="3" />
	                    <asp:ListItem Text="Less than or equal" Value="4" />
	                    <asp:ListItem Text="Greater than or equal" Value="5" />
	                    <asp:ListItem Text="Starts with" Value="6" />
	                    <asp:ListItem Text="Ends with" Value="7" />
	                    <asp:ListItem Text="Contains" Value="8" />
	                  </asp:DropDownList></td> 
	              <td><asp:TextBox ID="tbExp" runat="server" /></td>
	              <td><asp:DropDownList ID="ddChainOp" runat="server">
	                    <asp:ListItem Text="And" Value="0" />
	                    <asp:ListItem Text="Or" Value="1" />
	                  </asp:DropDownList></td> 
                </tr>
            </ItemTemplate>
            <EmptyDataTemplate>
            </EmptyDataTemplate>
        </asp:ListView>   
        </div>
    </ContentTemplate>
</asp:UpdatePanel>