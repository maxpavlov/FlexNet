<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:snl="sn://SenseNet.Portal.UI.ContentListViews.ListHelper"
                exclude-result-prefixes="msxsl">

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
    
    <sn:ListGrid ID="ViewBody"
                  DataSourceID="ViewDatasource"
                  runat="server">
      <LayoutTemplate>
        <table class="sn-listgrid ]]><xsl:if test="$groupBy != ''"><xsl:text>sn-listgrid-grouped</xsl:text></xsl:if><![CDATA[ ui-widget-content">
          <thead>
            <asp:TableRow runat="server" class="ui-widget-content">
            ]]><xsl:apply-templates mode="Header" /><![CDATA[
            </asp:TableRow>
          </thead>
          <tbody>
            <asp:TableRow runat="server" id="itemPlaceHolder" />
          </tbody>
        </table>
      </LayoutTemplate>
      <ItemTemplate>
        <asp:TableRow runat="server" class="sn-lg-row0 ui-widget-content">]]><xsl:apply-templates mode="Item" /><![CDATA[</asp:TableRow>
      </ItemTemplate>
      <AlternatingItemTemplate>
        <asp:TableRow runat="server" class="sn-lg-row1 ui-widget-content">]]><xsl:apply-templates mode="Item" /><![CDATA[</asp:TableRow>
      </AlternatingItemTemplate>
      <EmptyDataTemplate>
        <table class="sn-listgrid ui-widget-content">
          <thead>
          <asp:TableRow runat="server">]]><xsl:apply-templates mode="Header" /><![CDATA[</asp:TableRow>
          </thead>
        </table>
        <div class="sn-warning-msg ui-widget-content ui-state-default">The list is empty&hellip;</div>
      </EmptyDataTemplate>
    </sn:ListGrid>
    <asp:Literal runat="server" id="ViewScript" />
    <sn:SenseNetDataSource ID="ViewDatasource" runat="server" />]]>
  </xsl:template>

  <xsl:template mode="Header" match="/Columns">
    <![CDATA[<sn:ListHeaderCell  runat="server" ID="checkboxHeader" class="sn-lg-cbcol ui-state-default"><input type='checkbox' /></sn:ListHeaderCell>]]>
    <xsl:for-each select="Column">
      <![CDATA[<sn:ListHeaderCell runat="server" class="sn-lg-col-]]><xsl:value-of select="position()"/><xsl:if test="$groupBy != '' and contains(@fullName,$groupBy)"><xsl:text> sn-lg-col-groupby</xsl:text></xsl:if><![CDATA[ sn-nowrap ui-state-default" FieldName="]]><xsl:value-of select="@fullName"/><![CDATA[" ]]><xsl:if test="@width > 0"><![CDATA[Width="]]><xsl:value-of select="@width"/><![CDATA["]]></xsl:if><![CDATA[>      
        <asp:LinkButton runat="server" CommandName="Sort" CommandArgument="]]><xsl:value-of select="@fullName"/><![CDATA[" >
          <span class="sn-sort">
            <span class="sn-sort-asc ui-icon ui-icon-carat-1-n"></span>
            <span class="sn-sort-desc ui-icon ui-icon-carat-1-s"></span>
          </span>
          <span>]]><xsl:value-of select="@title"/><![CDATA[</span>            
        </asp:LinkButton>
      </sn:ListHeaderCell>]]>
    </xsl:for-each>
  </xsl:template>

  <xsl:template mode="Item" match="/Columns">
    <![CDATA[<asp:TableCell class="sn-lg-cbcol" runat="server" Visible="<%# (this.ShowCheckboxes.HasValue && this.ShowCheckboxes.Value) ? true : false %>">
        <input type='checkbox' value='<%# Eval("Id") %>' />
      </asp:TableCell>]]>
    <xsl:for-each select="Column">
      <xsl:choose>
        <xsl:when test="contains(@modifiers,'main')">
          <![CDATA[<asp:TableCell runat="server" class="sn-lg-col-]]><xsl:value-of select="position()"/><xsl:choose><xsl:when test="@wrap='Wrap'"><xsl:text> sn-wrap</xsl:text></xsl:when><xsl:when test="@wrap='NoWrap'"><xsl:text> sn-nowrap</xsl:text></xsl:when></xsl:choose><![CDATA[" ]]><xsl:if test="string-length(@hAlign) > 0"><![CDATA[HorizontalAlign="]]><xsl:value-of select="@hAlign"/><![CDATA["]]></xsl:if><![CDATA[ >]]>
          <![CDATA[ <sn:ActionMenu NodePath='<%# Eval("Path") %>' runat="server" Scenario="ListItem" IconName="<%# ((SenseNet.ContentRepository.Content)Container.DataItem).Icon %>" >]]>
          <![CDATA[  <sn:ActionLinkButton runat='server' NodePath='<%# Eval("Path") %>' ActionName='<%# ((SenseNet.ContentRepository.Content)Container.DataItem).ContentHandler.NodeType.IsInstaceOfOrDerivedFrom("ViewBase") ? "Edit" : "Browse" %>' IconVisible='false' >]]>
          <xsl:call-template name="ColumnValue"/>
          <![CDATA[  </sn:ActionLinkButton>
              <asp:Placeholder runat="server" Visible="<%# !((SNCR.Content)Container.DataItem).Security.HasPermission(SNCR.Storage.Schema.PermissionType.Open) %>">
          ]]>
          <xsl:call-template name="ColumnValue"/>
          <![CDATA[
              </asp:Placeholder>
            </sn:ActionMenu>]]>
          <![CDATA[</asp:TableCell>]]>
        </xsl:when>
        <xsl:otherwise>
          <![CDATA[<asp:TableCell runat="server" class="sn-lg-col-]]><xsl:value-of select="position()"/><xsl:choose><xsl:when test="@wrap='Wrap'"><xsl:text> sn-wrap</xsl:text></xsl:when><xsl:when test="@wrap='NoWrap'"><xsl:text> sn-nowrap</xsl:text></xsl:when></xsl:choose><![CDATA[" ]]><xsl:if test="string-length(@hAlign) > 0"><![CDATA[HorizontalAlign="]]><xsl:value-of select="@hAlign"/><![CDATA["]]></xsl:if><![CDATA[ >]]>
          <xsl:call-template name="ColumnValue" />
          <![CDATA[</asp:TableCell>]]>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="ColumnValue">
    <!--<![CDATA[<%# Eval("]]><xsl:value-of select="@bindingName"/><![CDATA[") %>]]>-->
    <xsl:variable name="fName" select="@fullName" />
    <xsl:value-of select="snl:RenderCell($fName, $listPath)" disable-output-escaping="yes" />
  </xsl:template>

  <xsl:template mode="Columns" match="/Columns">
    <xsl:for-each select="Column">
      <xsl:value-of select="@bindingName"/>
      <xsl:text> </xsl:text>
    </xsl:for-each>
  </xsl:template>

</xsl:stylesheet>
