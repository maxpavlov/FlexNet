<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>
<%@ Register Src="~/Root/System/SystemPlugins/Controls/AdvancedPanelButton.ascx" TagName="AdvancedFieldsButtonControl" TagPrefix="sn" %>

<sn:ScriptRequest ID="request1" runat="server" Path="$skin/scripts/sn/SN.ColumnSelector.js" />

        <asp:ListView ID="InnerListView" runat="server" EnableViewState="false" >
            <LayoutTemplate>      
                <table style="width:100%" >
                  <tr style="background-color:#F5F5F5">  
                      <th>Display</th>	                
                      <th style="width:205px">Field name</th>	   
                      <th>Type</th>    
                      <th>Width</th>    
                      <th>Align</th>    
                      <th>Wrap</th> 
                      <th>Position</th>           
                  </tr>
                  <tr runat="server" id="itemPlaceHolder" />
                </table>
            </LayoutTemplate>
            <ItemTemplate>
                <tr>      		
                  <td style="text-align:center"><asp:CheckBox id="cbField" runat="server" /></td>    	 
                  <td><%# Eval("Title")%>  <asp:Label ID="lblColumnFullName" runat="server" Visible="false" Text='<%# Eval("FullName")%>' /></td>
                  <td><asp:Label ID="lblColumnType" runat="server" /> </td>
                  <td><asp:TextBox id="tbWidth" runat="server" Width="30" /></td>    	 
                  <td><asp:DropDownList ID="ddHAlign" runat="server" Width="60px"></asp:DropDownList></td> 
                  <td><asp:DropDownList ID="ddWrap" runat="server" Width="65px"></asp:DropDownList></td> 
	              <td><asp:DropDownList ID="ddIndex" runat="server" Width="50px" onchange="SN.ColumnSelector.rearrangeSelects(this);"></asp:DropDownList></td> 
                </tr>
            </ItemTemplate>
            <EmptyDataTemplate>
            No fields found
            </EmptyDataTemplate>
        </asp:ListView>   
        
        <sn:AdvancedFieldsButtonControl runat="server" ID="AdvancedFieldsButton"/> <br/>
        
        <asp:Panel runat="server" ID="AdvancedPanel" style="display:none" >        
            <asp:ListView ID="InnerListViewAdvanced" runat="server" EnableViewState="false"  >
                <LayoutTemplate>      
                    <table style="width:100%" >
                      <tr style="background-color:#F5F5F5">  
                          <th>Display</th>	                
                          <th style="width:205px">Field name</th>	   
                          <th>Type</th>    
                          <th>Width</th>  
                          <th>Align</th>   
                          <th>Wrap</th>  
                          <th>Position</th>           
                      </tr>
                      <tr runat="server" id="itemPlaceHolder" />
                    </table>
                </LayoutTemplate>
                <ItemTemplate>
                    <tr>      		
                      <td style="text-align:center"><asp:CheckBox id="cbField" runat="server" /></td>    	 
                      <td><%# Eval("Title")%>  <asp:Label ID="lblColumnFullName" runat="server" Visible="false" Text='<%# Eval("FullName")%>' /></td>
                      <td><asp:Label ID="lblColumnType" runat="server" /> </td>
                      <td><asp:TextBox id="tbWidth" runat="server" Width="30" /></td>    	 
                      <td><asp:DropDownList ID="ddHAlign" runat="server" Width="60px"></asp:DropDownList></td> 
                      <td><asp:DropDownList ID="ddWrap" runat="server" Width="65px"></asp:DropDownList></td> 
	                  <td><asp:DropDownList ID="ddIndex" runat="server" Width="50px" onchange="SN.ColumnSelector.rearrangeSelects(this);"></asp:DropDownList></td> 
                    </tr>
                </ItemTemplate>
                <EmptyDataTemplate>
                No fields found
                </EmptyDataTemplate>
            </asp:ListView>   
        </asp:Panel>