<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:snc="sn://SenseNet.Portal.UI.ContentTools"
                xmlns:snf="sn://SenseNet.Portal.UI.XmlFormatTools"
                xmlns:snp="sn://SenseNet.Portal.UI.PathTools"
                exclude-result-prefixes="msxsl">

  <xsl:output method="html" indent="yes"/>

  <xsl:template match="/Content">
    <ul class="sn-journallist">
      <xsl:apply-templates select="Children/Content" />
    </ul>
  </xsl:template>

  <xsl:template match="Children/Content">
    <li>
        <div class="sn-icon-small snIconSmall_Document"></div>
        <div class="sn-journal-info">
            <strong class="sn-journal-who">
                <xsl:value-of select="Fields/Who"/>
                <!--<xsl:value-of select="snc:GetContent(Fields/Who)/ContentName"/>-->
            </strong>
            <xsl:text> </xsl:text>
            <xsl:value-of select="Fields/What"/>
            <xsl:text>: </xsl:text>
            <span class="sn-journal-what">
              <a href="{Fields/Wherewith}">
                <xsl:value-of select="snp:GetFileName(Fields/Wherewith)"/>
              </a>
            </span>
            <xsl:text> </xsl:text>
            <span class="sn-journal-when">
                <xsl:value-of select="snf:FormatDate(Fields/When)"/>
            </span>
        </div>
    </li>
  </xsl:template>

</xsl:stylesheet>
