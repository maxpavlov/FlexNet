<?xml version="1.0" encoding="utf-8"?>
<xsl:transform version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" exclude-result-prefixes="xsl">
<xsl:output method="xml" indent="yes" omit-xml-declaration="yes" encoding="utf-8" />

  <xsl:template match="/">
    <xsl:apply-templates select="NodeFeed" />
  </xsl:template>

  <xsl:template match="NodeFeed">
    <ul class="custommenu1">
      <xsl:apply-templates select="Nodes/Node">
        <xsl:sort data-type="text" order="ascending" select="Name"/>
      </xsl:apply-templates>
    </ul>
  </xsl:template>

  <xsl:template match="Nodes">
    <xsl:if test="Node">
      <ul>
        <xsl:apply-templates select="Node">
          <xsl:sort data-type="text" order="ascending" select="Name"/>
        </xsl:apply-templates>
      </ul>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Node">
    <li>
      <a href="{Url}"><xsl:value-of select="Name" /></a>
      <xsl:apply-templates select="Nodes">
        <xsl:sort data-type="text" order="ascending" select="Name"/>
      </xsl:apply-templates>
    </li>
  </xsl:template>

</xsl:transform>