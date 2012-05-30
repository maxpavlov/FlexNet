<%@ Control Language="C#" AutoEventWireup="true" Inherits="System.Web.UI.UserControl" %>

<sn:SenseNetDataSource ContentPath="/Root/Sites/Default_Site/features/Books" runat="server" ID="DSBooks" FieldNames="DisplayName Author ISBN Genre Description Path" />

<asp:ListView ID="LVBooks" DataSourceID="DSBooks" runat="server" >
      <LayoutTemplate>
        <table style="width: 100%; border-collapse:collapse; border:1px solid #ccc;">
            <thead>
                <tr id="Tr3" runat="server" style="background:#eee">
                    <th style="padding:2px;height: 25px; vertical-align:middle; text-align:left; border:1px solid #ccc;">&#160;</th>        
                    <th style="padding:2px;height: 25px; vertical-align:middle; text-align:left; border:1px solid #ccc;">DisplayName</th>        
                    <th style="padding:2px;height: 25px; vertical-align:middle; text-align:left; border:1px solid #ccc;">Author</th>
                    <th style="padding:2px;height: 25px; vertical-align:middle; text-align:left; border:1px solid #ccc;">ISBN</th>
                    <th style="padding:2px;height: 25px; vertical-align:middle; text-align:left; border:1px solid #ccc;">Genre</th>
                </tr>
            </thead>
            <tbody>
                <tr runat="server" id="itemPlaceHolder" />
            </tbody>
        </table>
      </LayoutTemplate>
      <ItemTemplate>
                <tr id="Tr5" runat="server">      
	              <td style="padding:2px;vertical-align:middle; border:1px solid #ccc; text-align:center"><img src='/binaryhandler.ashx?nodepath=<%# Eval("Path") %>&amp;propertyname=Image&amp;width=40&amp;height=60' alt="" /></td>    
                  <td style="padding:2px;vertical-align:middle; border:1px solid #ccc;"><a href='<%# Eval("Path") %>'><%# Eval("DisplayName") %></a></td>    
                  <td style="padding:2px;vertical-align:middle; border:1px solid #ccc;"><%# Eval("Author") %></td>    
	              <td style="padding:2px;vertical-align:middle; border:1px solid #ccc;"><%# Eval("ISBN") %></td>     
	              <td style="padding:2px;vertical-align:middle; border:1px solid #ccc;"><%# Eval("Genre") %></td>     
                </tr>
      </ItemTemplate>
      <AlternatingItemTemplate>
                <tr id="Tr5" runat="server" style="background-color:#f0f0f0">      
	              <td style="padding:2px;vertical-align:middle; border:1px solid #ccc; text-align:center"><img src='/binaryhandler.ashx?nodepath=<%# Eval("Path") %>&amp;propertyname=Image&amp;width=40&amp;height=60' alt="" /></td>    
                  <td style="padding:2px;vertical-align:middle; border:1px solid #ccc;"><a href='<%# Eval("Path") %>'><%# Eval("DisplayName") %></a></td>    
                  <td style="padding:2px;vertical-align:middle; border:1px solid #ccc;"><%# Eval("Author") %></td>    
	              <td style="padding:2px;vertical-align:middle; border:1px solid #ccc;"><%# Eval("ISBN") %></td>     
	              <td style="padding:2px;vertical-align:middle; border:1px solid #ccc;"><%# Eval("Genre") %></td>     
                </tr>
      </AlternatingItemTemplate>
  </asp:ListView>
