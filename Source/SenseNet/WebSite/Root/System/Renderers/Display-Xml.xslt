<?xml version="1.0" encoding="utf-8"?>
<xsl:transform version="1.0"
xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
exclude-result-prefixes="xsl">
  <xsl:output method="xml" indent="yes" omit-xml-declaration="yes" />
  <xsl:template match="/">
    <textarea>
      <xsl:copy-of select="/"/>
    </textarea>
  </xsl:template>
</xsl:transform>