<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
  <xsl:output method="html" indent="yes"/>
  <xsl:template match="/">
    <xsl:for-each select="/Content/Children/Content">
        <h4><xsl:value-of select="Fields/DisplayName" /></h4>
        <xsl:value-of select="Fields/Lead" disable-output-escaping="yes"/>
    </xsl:for-each>
  </xsl:template>
</xsl:stylesheet>
