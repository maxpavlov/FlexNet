<?xml version="1.0" encoding="iso-8859-1"?>

<xsl:stylesheet
    version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    xmlns:snc="sn://SenseNet.Portal.UI.ContentTools"
    xmlns:snf="sn://SenseNet.Portal.UI.XmlFormatTools"
    exclude-result-prefixes="xsl msxsl snc snf">

  <xsl:output method="html" indent="yes"/>

  <xsl:template match="/">
    <div class="snTagList">
      <ul>

        <xsl:for-each select="snc:SplitTags(/Content/Fields/Tags)">
          <li>
            <a>
              <xsl:attribute name="href">
                ?action=SearchTag&amp;filter=<xsl:value-of select="./@Title"/>
              </xsl:attribute>
              <xsl:value-of select="./@Title"/>
            </a>
          </li>
        </xsl:for-each>

      </ul>
    </div>
  </xsl:template>

</xsl:stylesheet>
