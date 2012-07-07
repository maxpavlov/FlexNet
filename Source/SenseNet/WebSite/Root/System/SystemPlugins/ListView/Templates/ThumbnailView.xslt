<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:snl="sn://SenseNet.Portal.UI.ContentListViews.ListHelper"
                exclude-result-prefixes="msxsl"
                xmlns:snf="sn://SenseNet.Portal.UI.XmlFormatTools">

  <xsl:param name="listPath"/>
  <xsl:param name="groupBy"/>
  <xsl:output method="text" />

  <!--<xsl:variable name="eval_open"><![CDATA[<%# Eval("]]></xsl:variable>
  <xsl:variable name="eval_close"><![CDATA[") %>]]></xsl:variable>
  <xsl:variable name="eval_line" select="concat($eval_open, @bindingName, $eval_close)" />
  <xsl:value-of select="$eval_line" disable-output-escaping="yes"/>-->

  
  <xsl:template match="/">
    <![CDATA[<%@ Control Language="C#" AutoEventWireup="false" Inherits="SenseNet.Portal.UI.ContentListViews.ListView" %>
    <%@ Import Namespace="SNCR=SenseNet.ContentRepository" %>
    <%@ Import Namespace="SenseNet.Portal.UI.ContentListViews" %>
    <%@ Import Namespace="System.Linq" %>
    <%@ Import Namespace="SCR=SenseNet.ContentRepository.Fields" %>
    
    <sn:CssRequest ID="gallerycss" runat="server" CSSPath="$skin/styles/sn-gallery.css" />
    <sn:CssRequest ID="gallerycss2" runat="server" CSSPath="/Root/Global/styles/prettyPhoto.css" />
    <sn:ScriptRequest runat="server" Path="$skin/scripts/jquery/plugins/jquery.prettyPhoto.js" />
    
    <script>
      $(function () {
      
        $(".galleryContainer tr").each(function() {
        var list = $(this);
        var size = 5;
        var current_size = 0;
        list.children().each(function() {
        console.log(current_size + ": " + $(this).text());
          if (++current_size > size) {
            var new_list = $("<tr></tr>").insertAfter(list);
            list = new_list;
            current_size = 1;
          }
          list.append(this);
        });
      });
        });
        $(document).ready(function() {
        $(".sn-gallery-table a[rel^='prettyPhoto']").prettyPhoto({
            theme: 'facebook',
            deeplinking: false,
            overlay_gallery: false
        });
    });
    </script>
    
    <div class="galleryContainer">
    <sn:ListGrid ID="ViewBody"
                  DataSourceID="ViewDatasource"
                  runat="server">
      <LayoutTemplate>
        <table class="sn-datagrid sn-gallery-table">
          <tbody>
          <tr>
            <asp:TableCell runat="server" id="itemPlaceHolder" />
            
            </tr>
          </tbody>
        </table>
      </LayoutTemplate>
      <ItemTemplate>
        <asp:TableCell runat="server" class="sn-gallery-cell">]]><xsl:apply-templates mode="Item" /><![CDATA[</asp:TableCell>
      </ItemTemplate>
      <EmptyDataTemplate>
        <table class="sn-listgrid ui-widget-content">
          <thead>
          <asp:TableRow runat="server">]]><xsl:apply-templates mode="Header" /><![CDATA[</asp:TableRow>
          </thead>
        </table>
        <div class="sn-warning-msg ui-widget-content ui-state-default">The list is empty&hellip;</div>
      </EmptyDataTemplate>
    </sn:ListGrid>
    </div>
    <asp:Literal runat="server" id="ViewScript" />
    <sn:SenseNetDataSource ID="ViewDatasource" runat="server" />]]>
  </xsl:template>


  <xsl:template mode="Item" match="/Columns">
    <xsl:for-each select="Column">

      <xsl:choose>
        <xsl:when test="contains(@fullName,GenericContent.Binary)">
          <xsl:call-template name="ColumnValue" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:choose>
            <xsl:when test="contains(@modifiers,'main')">
              <xsl:call-template name="ColumnValue"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="ColumnValue" />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="ColumnValue">
    <xsl:choose>
      <xsl:when test="@fullName = 'File.Binary'">
        <div class="sn-image">
          <![CDATA[<a href="<%# (((SNCR.Content)Container.DataItem).ContentHandler.NodeType.IsInstaceOfOrDerivedFrom("Folder"))? Eval("Path") : Eval("Path") %>" rel="<%# (((SNCR.Content)Container.DataItem).ContentHandler.NodeType.IsInstaceOfOrDerivedFrom("Folder"))? " " : "prettyphoto[pp_gal]" %>" class="<%# (((SNCR.Content)Container.DataItem).ContentHandler.NodeType.IsInstaceOfOrDerivedFrom("Folder"))? "sn-folder-img" : "sn-gallery-img" %>" title="<%# (((SNCR.Content)Container.DataItem).ContentHandler.NodeType.IsInstaceOfOrDerivedFrom("Folder"))? Eval("DisplayName") : Eval("DisplayName") %>"><img src='<%# (((SNCR.Content)Container.DataItem).ContentHandler.NodeType.IsInstaceOfOrDerivedFrom("Folder"))? "/Root/Global/images/icons/folder-icon-125.jpg" : Eval("Path") + "?action=Thumbnail" %>' title="<%# (((SNCR.Content)Container.DataItem).ContentHandler.NodeType.IsInstaceOfOrDerivedFrom("Folder"))? Eval("DisplayName") : Eval("DisplayName") %>" /></a>]]>
        </div>
      </xsl:when>
      <xsl:when test="@fullName = 'GenericContent.DisplayName'">
        
          <![CDATA[<div class="sn-title"><%# Eval("DisplayName") %></div>]]>
        
      </xsl:when>
      <xsl:when test="@fullName = 'GenericContent.ModifiedBy'">

        <![CDATA[<div class="sn-modifiedby"><span>Uploaded by:</span><sn:ActionLinkButton ID="ModifiedBy" runat='server' NodePath='<%# ((SNCR.Content)Container.DataItem).ContentHandler.ModifiedBy.Path%>' ActionName='Profile' IconVisible="false"
    Text='<%# ((SNCR.User)((SNCR.Content)Container.DataItem).ContentHandler.ModifiedBy).FullName %>'
ToolTip='<%# ((SNCR.User)((SNCR.Content)Container.DataItem).ContentHandler.ModifiedBy).Domain + "/" + ((SNCR.Content)Container.DataItem).ContentHandler.ModifiedBy.Name %>'  />​</div>]]>

      </xsl:when>
      <xsl:otherwise>
        <!--<![CDATA[<%# Eval("]]><xsl:value-of select="@bindingName"/><![CDATA[") %>]]>-->
          <xsl:variable name="fName" select="@fullName" />
          <xsl:value-of select="snl:RenderCell($fName, $listPath)" disable-output-escaping="yes" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template mode="Columns" match="/Columns">
    
    <xsl:for-each select="Column">
      <xsl:value-of select="@bindingName"/>
      <xsl:text> </xsl:text>
    </xsl:for-each>
  </xsl:template>

</xsl:stylesheet>