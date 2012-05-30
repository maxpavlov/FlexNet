<%@ Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<%@ Register Src="~/Root/System/SystemPlugins/Controls/ContentTypeInstallerControl.ascx" TagName="ContentTypeInstallerControl" TagPrefix="sn" %>

THIS TEMPLATE CONTAINS THE MOST COMMON FIELD TYPES AND OPTIONS.<br />
CUSTOMIZE IT AND REMOVE UNNECESSARY FIELDS AND OPTIONS BEFORE INSTALLING IT. <br/>
For more information go to <a href="http://wiki.sensenet.com/index.php?title=Content_Type_Definition">Sense/Net wiki CTD</a> or <a href="http://wiki.sensenet.com/index.php?title=Table_of_Contents#Fields">Sense/Net wiki list of Fields</a>
<br/><br/>

<div class="sn-highlighteditor-container">
	<sn:Binary ID="Binary1" runat="server" FieldName="Binary" FullScreenText="true" FrameMode="NoFrame">
		<EditTemplate>
			 <asp:TextBox ID="BinaryTextBox" runat="server" TextMode="MultiLine" CssClass="sn-highlighteditor" Rows="40" Columns="100" />
		</EditTemplate>
	</sn:Binary>
</div>

<div class="sn-panel sn-buttons">
	<sn:ContentTypeInstallerControl ID="ContentTypeInstaller1" runat="server" />
</div>