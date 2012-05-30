<%@  Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<div class="sn-view-container">
	<div class="sn-view-top">
        <div class="sn-view-header">
            <div class="sn-icon-big snIconBigNew_Content"></div>
            <div class="sn-view-header-text">
                <h2 class="sn-view-title">Create a new <% = ContentTypeName %> to:</h2>
                <% = ParentPath %>
            </div>
        </div>
	</div>
	<div class="sn-view-main">
        <div class="sn-view-body">
			<sn:ErrorView ID="ErrorView1" runat="server" />
			<sn:GenericFieldControl ID="GenericFieldControl1" runat="server" />
		</div>
	</div>
	<div class="sn-view-bottom">
        <div class="sn-view-footer ui-helper-clearfix">
            <sn:DefaultButtons ButtonCssClass="sn-submit" ID="DefaultButtons1" runat="server" />
        </div>
	</div>
</div>