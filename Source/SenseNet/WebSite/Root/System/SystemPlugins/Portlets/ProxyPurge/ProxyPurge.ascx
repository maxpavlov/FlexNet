<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>
    
    <style type="text/css">
        .sn-purge-ok { color:Green; font-weight:bold; }
        .sn-purge-miss { color:Orange; font-weight:bold; }
        .sn-purge-error { color:Red; font-weight:bold; }
    </style>

    <asp:ListView ID="UrlListView" runat="server" EnableViewState="false" >
       <LayoutTemplate>
            <table class="sn-listgrid ui-widget-content">
                <thead>
                    <tr class="ui-widget-content">
                        <th class="sn-lg-col-1 ui-state-default" style="width:350px">URL</th>
                        <th class="sn-lg-col-2 ui-state-default" style="width:350px">Result</th>
                    </tr>
                </thead>
                <tbody>
                    <tr runat="server" id="itemPlaceHolder"></tr>
                </tbody>
            </table>               
       </LayoutTemplate>
       <ItemTemplate>
            <tr class="sn-lg-row0 ui-widget-content">
                <td><%# Eval("Url") %></td>
                <td>
                    <asp:ListView ID="ProxyResultList" runat="server" EnableViewState="false">
                        <LayoutTemplate>
                            <table>
                                <tr>
                                    <td runat="server" id="itemPlaceHolder"></td>
                                </tr>
                            </table>
                        </LayoutTemplate>
                        <ItemTemplate>
                            <td style="margin:3 20 3 20; border:0; width:40px"><span class='<%# "sn-purge-" + Eval("Message").ToString().ToLower() %>' title='<%# Eval("ProxyIP")%>'><%# Eval("Message")%></span></td>
                        </ItemTemplate>
                    </asp:ListView>
                </td>
            </tr>
       </ItemTemplate>
       <AlternatingItemTemplate>
            <tr class="sn-lg-row1 ui-widget-content">
                <td><%# Eval("Url") %></td>
                <td>
                    <asp:ListView ID="ProxyResultList" runat="server" EnableViewState="false">
                        <LayoutTemplate>
                            <table>
                                <tr>
                                    <td runat="server" id="itemPlaceHolder"></td>
                                </tr>
                            </table>
                        </LayoutTemplate>
                        <ItemTemplate>
                            <td style="margin:3 20 3 20; border:0; width:40px"><span class='<%# "sn-purge-" + Eval("Message").ToString().ToLower() %>' title='<%# Eval("ProxyIP")%>'><%# Eval("Message")%></span></td>
                        </ItemTemplate>
                    </asp:ListView>
                </td>
            </tr>
       </AlternatingItemTemplate>
       <EmptyDataTemplate>
            <span class='sn-purge-error'><%= HttpContext.GetGlobalResourceObject("Portal","NoUrlsToPurge") %></span>
       </EmptyDataTemplate>
    </asp:ListView>   
   <asp:PlaceHolder ID="ErrorPlaceholder" runat="server" Visible="false"></asp:PlaceHolder>
   <br/>

<div class="sn-pt-body-border ui-widget-content sn-dialog-buttons">
    <div class="sn-pt-body">
        <sn:BackButton Text="Done" ID="DoneButton" runat="server" CssClass="sn-submit" />
    </div>
</div>