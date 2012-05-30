<%@ Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<%@ Import Namespace="System.Linq" %>
<%
    var extension = System.IO.Path.GetExtension(ContentHandler.Path).ToLower();
    if (SenseNet.ContentRepository.Repository.EditSourceExtensions.Contains(extension))
    {%>
<div class="sn-highlighteditor-container">
	<sn:Binary ID="Binary1" runat="server" FieldName="Binary" FullScreenText="true" FrameMode="NoFrame">
		<EditTemplate>
			 <asp:TextBox ID="BinaryTextBox" runat="server" TextMode="MultiLine" CssClass="sn-highlighteditor" Rows="40" Columns="100" />
		</EditTemplate>
	</sn:Binary>
</div>

<div class="sn-panel sn-buttons">
	<sn:CommandButtons ID="CommandButtons1" runat="server" />
</div>
<%} %>