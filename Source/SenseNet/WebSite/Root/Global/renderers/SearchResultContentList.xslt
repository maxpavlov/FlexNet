<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" 
    xmlns:snc="sn://SenseNet.Portal.UI.ContentTools"
    xmlns:snf="sn://SenseNet.Portal.UI.XmlFormatTools"
    xmlns:sni="sn://SenseNet.Portal.UI.IconHelper"
    exclude-result-prefixes="msxsl" >

  <xsl:template match="/">
    <div class="sn-search-results">
        <xsl:for-each select="/Content/Children/Content">
          <div class="sn-search-result ui-helper-clearfix">
            <div style="float:left; padding:3px 10px 3px 10px;">
              <xsl:variable name="iconName" select="Icon" />
              <xsl:value-of select="sni:RenderIconTag($iconName, '', 32)" disable-output-escaping="yes" />
            </div>
            <div style="padding:3px 0 5px 0;">
              <a href="{Actions/Browse}">
                <xsl:value-of select="Fields/DisplayName" />
              </a>
              <br/>
              <xsl:value-of select="Fields/Path" />
            </div>
          </div>
        </xsl:for-each>
    </div>
  </xsl:template>
  
</xsl:stylesheet>
