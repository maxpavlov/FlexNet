<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.Controls.RatingSearch" %>
<div id="sn-rating-search">
    <asp:TextBox ID="tbRatingSearchFrom" runat="server"></asp:TextBox>-tól &nbsp
    <asp:TextBox ID="tbRatingSearchTo" runat="server"></asp:TextBox>-ig
    <asp:Button ID="sn-rating-search-btn" runat="server" onclick="btnSearch_Click" Text="Search" />
</div>

<asp:ListView ID="RatingSearchListView" runat="server">
<LayoutTemplate>
    <div id="ratingSearchTable">
        <table style="border-collapse:collapse; border:1px solid #ccc;">
            <thead>
                <tr id="Tr3" runat="server" style="background:#eee">       
                    <th style="height: 25px; border:1px solid #ccc;">DisplayName</th>        
                    <th style="height: 25px; border:1px solid #ccc;">Description</th>
                    <th style="height: 25px; border:1px solid #ccc;">CreatedBy</th>        
                    <th style="height: 25px; border:1px solid #ccc;">Creation Date</th>
                    <th style="height: 25px; border:1px solid #ccc;">Modifycation Date</th>        
                </tr>
            </thead>
            <tbody>
                <tr runat="server" id="itemPlaceHolder" />
            </tbody>
        </table>
       </div>
      </LayoutTemplate>
      <ItemTemplate>
                <tr id="Tr5" runat="server">        
                  <td style="border:1px solid #ccc;"><a href='<%# Eval("Path") %>'><%# Eval("DisplayName")%></a></td>   
	              <td style="border:1px solid #ccc;"><%# Eval("Description ")%></td>    
	              <td style="border:1px solid #ccc;"><%# Eval("CreatedBy ")%></td>
	              <td style="border:1px solid #ccc;"><%# Eval("CreationDate")%></td>    
	              <td style="border:1px solid #ccc;"><%# Eval("ModificationDate ")%></td>   
                </tr>
      </ItemTemplate>
      <AlternatingItemTemplate>
                <tr id="Tr5" runat="server" style="background-color:#f0f0f0">          
                  <td style="border:1px solid #ccc;"><a href='<%# Eval("Path") %>'><%# Eval("DisplayName")%></a></td>      
	              <td style="border:1px solid #ccc;"><%# Eval("Description ")%></td>   
	              <td style="border:1px solid #ccc;"><%# Eval("CreatedBy ")%></td>
	              <td style="border:1px solid #ccc;"><%# Eval("CreationDate")%></td>    
	              <td style="border:1px solid #ccc;"><%# Eval("ModificationDate ")%></td>   
                </tr>
      </AlternatingItemTemplate>
</asp:ListView>
