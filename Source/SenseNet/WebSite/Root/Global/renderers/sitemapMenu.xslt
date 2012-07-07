<?xml version="1.0" encoding="utf-8"?>
<xsl:transform version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:snc="sn://SenseNet.Portal.UI.ContentTools"
                exclude-result-prefixes="xsl">
  
  <xsl:output method="xml" indent="yes" omit-xml-declaration="yes" encoding="utf-8" />

  
  <xsl:template match="/">
    <div>
    <div class="column-title">
      <xsl:value-of select="Content/Fields/DisplayName"/>
    </div>
    <ul>
      <xsl:for-each select="Content/Children/Content">
        <xsl:sort select="Fields/Index" data-type="number"/>
        <xsl:choose>
          <xsl:when test="contains(Fields/Path,'features')">
            <xsl:choose>
              <xsl:when test="position() mod 2 = 0">
                <li class="right">
                  <a href="{Fields/Path}">
                    <xsl:value-of select="Fields/DisplayName"/>
                  </a>
                </li>
              </xsl:when>
              <xsl:otherwise>
                <li class="left">
                  <a href="{Fields/Path}">
                    <xsl:value-of select="Fields/DisplayName"/>
                  </a>
                </li>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:otherwise>
            <li>
              <a href="{Fields/Path}">
                <xsl:value-of select="Fields/DisplayName"/>
              </a>
            </li>
          </xsl:otherwise>
        </xsl:choose>
          
          
        
      </xsl:for-each>
    </ul>
    </div>
  </xsl:template>

  
</xsl:transform>