<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<sn:ScriptRequest ID="request1" runat="server" Path="$skin/scripts/sn/SN.ContentEditable.js" />

<div class="sn-content-inlineview">
    <sn:ErrorView ID="ErrorView1" runat="server" />

    <div class="sn-article-content">
        <div id="InlineViewProperties">
            <sn:DisplayName ID="DisplayName" runat="server" FieldName="DisplayName" RenderMode="Edit" />            
        </div>

        <div class="sn-article-body sn-richtext">
            <sn:WikiEditor ID="ArticleRichText" ConfigPath="/Root/System/SystemPlugins/Controls/DemoRichTextConfig.config" runat="server" FieldName="WikiArticleText" Width="100%" ControlMode="Edit" FrameMode="NoFrame" />
        </div>    
    </div>    
</div>

<div class="sn-panel sn-buttons">
  <sn:CommandButtons ID="CommandButtons1" runat="server" HideButtons="Save CheckoutSave" />
</div>

<sn:InlineScript ID="InlineScript1" runat="server">
<script type="text/javascript">
    $(function() {
        // initialize the contenteditable fields with the class name of the controls
        SN.ContentEditable.setupContentEditableFields("sn-wysiwyg-ctrl");
    });
</script>
</sn:InlineScript>

