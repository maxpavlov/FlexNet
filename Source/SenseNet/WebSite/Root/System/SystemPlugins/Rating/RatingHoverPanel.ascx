<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.Controls.RatingHoverPanel" %>
<div class="rating-popup">
    <div class="rating-inside">
        <table cellspacing="0" cellpadding="0">
            <tr>
                <td>
                    <img src="/Root/Skins/book/images/rating-bf.png" />
                </td>
                <td class="rating-tooltip-bg">
                </td>
                <td>
                    <img src="/Root/Skins/book/images/rating-jf.png" />
                </td>
            </tr>
            <tr>
                <td class="rating-tooltip-bg">
                </td>
                <td align="center" class="rating-tooltip-bg">
                    <div>
                        <strong>Average: (<span id="rating-avg">avg</span></strong>)</div>
                    <div>
                        <table cellspacing="5" cellpadding="2">
                            <asp:Repeater ID="r1" runat="server">
                                <ItemTemplate>
                                    <tr>
                                        <td>
                                            <strong><%# DataBinder.Eval(Container.DataItem, "Index")%>:</strong>
                                        </td>
                                        <td class="rating-scale">
                                            <div class="rating-scale-fill" id="rating-scale-<%# DataBinder.Eval(Container.DataItem, "Index")%>">
                                                *</div>
                                        </td>
                                        <td>
                                            <span id="rating-value-<%# DataBinder.Eval(Container.DataItem, "Index")%>">(<%# DataBinder.Eval(Container.DataItem, "Value")%>)</span>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </table>
                    </div>
                </td>
                <td class="rating-tooltip-bg">
                </td>
            </tr>
            <tr>
                <td>
                    <img src="/Root/Skins/book/images/rating-ba.png" />
                </td>
                <td class="rating-tooltip-bg">
                </td>
                <td>
                    <img src="/Root/Skins/book/images/rating-ja.png" />
                </td>
            </tr>
        </table>
    </div>
</div>
