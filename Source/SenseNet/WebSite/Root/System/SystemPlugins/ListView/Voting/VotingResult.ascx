<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.VotingResultContentView" %>
<%@ Register Assembly="SenseNet.Portal" Namespace="SenseNet.Portal.UI.Controls" TagPrefix="sn" %>
<%@ Import Namespace="SenseNet.ContentRepository.Storage" %>
<%@ Import Namespace="SenseNet.Portal.Virtualization" %>

<div class="sn-content sn-voting-result">
    <h2 class="sn-content-title"><%=GetGlobalResourceObject("Voting", "Result")%></h2>
    <asp:ListView ID="ResultList" runat="server">
        <LayoutTemplate>
            <div class="sn-inputunit ui-helper-clearfix">
                <asp:PlaceHolder runat="server" id="itemPlaceHolder" />
            </div>
        </LayoutTemplate>
        <ItemTemplate>
                <div class="sn-iu-label">
                    <span class="sn-iu-title"><%# Eval("Question") %> (<%# Eval("Count") %>%)</span>
                </div>
                <div class="sn-iu-control">
                    <span class="sn-voting-result-graph ui-progressbar ui-widget ui-widget-content ui-corner-all" style="display:inline-block; width:100%">
                        <span class="sn-voting-result-bar ui-progressbar-value ui-widget-header ui-corner-left" style="width: <%# Eval("Count") %>%; display:inline-block;"></span>
                    </span>
                </div>
        </ItemTemplate>
    </asp:ListView>
</div>
<div class="sn-panel sn-buttons">
    <asp:LinkButton CssClass="sn-button sn-submit" ID="VotingAndResult" runat="server" Text="Link" Visible="false"/>
</div>
