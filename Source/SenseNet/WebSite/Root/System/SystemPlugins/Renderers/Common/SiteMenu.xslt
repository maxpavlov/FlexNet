<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:snc="sn://SenseNet.Portal.UI.ContentTools"
                xmlns:snf="sn://SenseNet.Portal.UI.XmlFormatTools"
                xmlns:sni="sn://SenseNet.Portal.UI.IconHelper"
                xmlns:snp="sn://SenseNet.Portal.Virtualization.PortalContext"
                exclude-result-prefixes="msxsl">

	<xsl:output method="html" indent="yes" omit-xml-declaration="yes" />

	<xsl:variable name="Remote_Context">yes</xsl:variable>
  <xsl:template match="/">
		<ul class="sn-menu">
			<xsl:for-each select="/Content/Children/Content">
        <xsl:sort data-type="number" order="ascending" select="Fields/Index"/>
        <xsl:if test="Fields/Hidden != 'True'">
          <li class="sn-menu-{position()}">
            <a href="{snp:GetSiteRelativePath(SelfLink)}">
              <!-- 
              <xsl:variable name="iconName" select="Icon" />
              <xsl:value-of select="sni:RenderIconTag($iconName)" disable-output-escaping="yes" />
              -->
              <xsl:value-of select="Fields/DisplayName"/>
            </a>
          </li>
        </xsl:if>
			</xsl:for-each>
		</ul>
	</xsl:template>

</xsl:stylesheet>