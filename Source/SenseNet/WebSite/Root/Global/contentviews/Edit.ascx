<%@  Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>

<div class="sn-view-container">
	<div class="sn-view-top">
		<div class="sn-view-header">
			<div class="sn-icon-big snIconBigEdit_Content">
			</div>
			<div class="sn-view-header-text">
				<h2 class="sn-view-title"><% = ContentName %> (<% = ContentType.DisplayName%>)</h2>
				<% = ContentHandler.Path %>
				<br />
				<p>
				<%--<sn:ActionMenu ID="ContentEksnMenu" runat="server" Scenario="ListItem" Text="Actions" />--%>
				</p>
			</div>
		</div>
	</div>
	<div class="sn-view-main">
		<div class="sn-view-body">
			<sn:ErrorView ID="ErrorView1" runat="server" />
			<sn:GenericFieldControl ID="GenericField1" runat="server" />
		</div>
	</div>
	<div class="sn-view-bottom">
		<div class="sn-view-footer ui-helper-clearfix">
			<sn:DefaultButtons ButtonCssClass="sn-submit" id="DefaultButtons1" runat="server" />
		</div>
	</div>
</div>