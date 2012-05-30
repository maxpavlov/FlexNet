<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<sn:ScriptRequest ID="request1" runat="server" Path="$skin/scripts/sn/SN.ContentEditable.js" />

<div class="sn-content-inlineview">
    <sn:ErrorView ID="ErrorView1" runat="server" />

    <div class="sn-article-content">        
        <sn:ShortText ID="DisplayName" runat="server" FieldName="DisplayName" FrameMode="NoFrame" ControlMode="Edit">
            <EditTemplate>
                <h1 class="sn-content-title sn-article-title sn-wysiwyg-text" contenteditable="true" title="DisplayName"><%= GetValue("DisplayName") %></h1>
                <asp:PlaceHolder ID="ErrorPlaceHolder" runat="server"></asp:PlaceHolder>
                <asp:TextBox CssClass="sn-wysiwyg-ctrl" ID="InnerShortText" runat="server"></asp:TextBox>
            </EditTemplate>            
        </sn:ShortText>

        <div class="sn-blogpost-edit-lead sn-richtext">
          <h3><%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("Portal", "SnBlog_NewEditBlogPost_LeadingText")%></h3>
            <sn:WikiEditor ID="LeadingTextRichEditor" ConfigPath="/Root/System/SystemPlugins/Controls/DemoRichTextConfig.config" runat="server" FieldName="LeadingText" Width="100%" ControlMode="Edit" FrameMode="NoFrame" />
        </div>    
        <div class="sn-blogpost-edit-body sn-richtext">
          <h3><%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("Portal", "SnBlog_NewEditBlogPost_BodyText")%></h3>
            <sn:WikiEditor ID="BodyTextRichEditor" ConfigPath="/Root/System/SystemPlugins/Controls/DemoRichTextConfig.config" runat="server" FieldName="BodyText" Width="100%" ControlMode="Edit" FrameMode="NoFrame" />
        </div>    
        <div class="sn-blogpost-edit-properties">
            <div class="sn-blogpost-edit-published">
                <sn:Boolean ID="PublishedBoolean" runat="server" FieldName="IsPublished" ControlMode="Edit" />
            </div>
            <div class="sn-blogpost-edit-publishedon">
                <sn:DatePicker ID="PublishedOnDate" runat="server" FieldName="PublishedOn" ControlMode="Edit" />
            </div>
            <div class="sn-blogpost-edit-tags">
                <sn:LongText ID="TagShortText" runat="server" FieldName="Tags" Width="100%" ControlMode="Edit" />
            </div>                   
        </div>
    </div>    
</div>

<div id="InlineViewProperties">    
    Modified: <sn:DatePicker ID="ValidFrom" runat="server" FieldName="ModificationDate" RenderMode="Browse" />, <%= GetValue("ModifiedBy") %>
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

