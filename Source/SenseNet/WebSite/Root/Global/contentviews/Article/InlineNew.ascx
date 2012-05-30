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

        <sn:ShortText ID="SubTitle" runat="server" FieldName="Subtitle" FrameMode="NoFrame" ControlMode="Edit">
            <EditTemplate>
                <h3 class="sn-content-subtitle sn-article-subtitle sn-wysiwyg-text" contenteditable="true" title="Subtitle"><%= GetValue("Subtitle") %></h3>
                <asp:PlaceHolder ID="ErrorPlaceHolder" runat="server"></asp:PlaceHolder>
                <asp:TextBox CssClass="sn-wysiwyg-ctrl" ID="InnerShortText" runat="server"></asp:TextBox>
            </EditTemplate>            
        </sn:ShortText>

        <div class="sn-article-info">
            <span>Author: <strong><%=GetValue("Author") %></strong></span>
            <span class="sn-article-info-separator">|</span>
            <span>Published: <strong><%=GetValue("CreationDate") %></strong></span>
        </div>
 
        <sn:Image ID="Image1" runat="server" FieldName="Image" FrameMode="NoFrame" ControlMode="Edit" Width="510" Height="290">
            <EditTemplate>
                <div style="margin-bottom:1em; text-align:center;">
                    <div class="sn-article-img">
                        <asp:Image ImageUrl="/Root/Global/images/missingphoto.png" ID="ImageControl" runat="server" alt="" />
                    </div>
                    <asp:FileUpload ID="FileUploadControl" runat="server" class="snFileInput" /> 
                    <asp:Label ID="Label2" AssociatedControlID="ImageIsReferenceControl" runat="server"></asp:Label> 
                    <asp:CheckBox ID="ImageIsReferenceControl" runat="server" style="display: none;"/>
                </div>
            </EditTemplate>
        </sn:Image>

        <div class="sn-article-lead sn-richtext" style="margin-bottom:1em">
            <sn:RichText ID="Lead" ConfigPath="/Root/System/SystemPlugins/Controls/DemoRichTextConfig.config" runat="server" FieldName="Lead" Width="100%" ControlMode="Edit" FrameMode="NoFrame" />
        </div>

        <div class="sn-article-body sn-richtext">
            <sn:RichText ID="Body" ConfigPath="/Root/System/SystemPlugins/Controls/DemoRichTextConfig.config" runat="server" FieldName="Body" Width="100%" ControlMode="Edit" FrameMode="NoFrame" />
        </div>
    
    </div>
    
</div>

<div id="InlineViewProperties" class="sn-content-meta">
    <sn:Boolean ID="IsPinned" runat="server" FieldName="Pinned" RenderMode="Edit" />
    <sn:ShortText ID="Author" runat="server" FieldName="Author" RenderMode="Edit" />
    <sn:LongText ID="Keywords" runat="server" FieldName="Keywords" RenderMode="Edit" />
    <sn:ShortText ID="Name" runat="server" FieldName="Name" RenderMode="Edit" />
    <sn:Boolean ID="Hidden" runat="server" FieldName="Hidden" RenderMode="Edit" />
    <sn:Boolean ID="EnableLifespan" runat="server" FieldName="EnableLifespan" RenderMode="Edit" />
    <sn:DatePicker ID="ValidFrom" runat="server" FieldName="ValidFrom" RenderMode="Edit" />
    <sn:DatePicker ID="ValidTill" runat="server" FieldName="ValidTill" RenderMode="Edit" />
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

