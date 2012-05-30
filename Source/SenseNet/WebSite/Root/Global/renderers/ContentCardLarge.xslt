<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                              xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
                              xmlns:snf="sn://SenseNet.Portal.UI.XmlFormatTools" >
  
  <xsl:output method="html" indent="yes"/>

  <xsl:template match="/">
    <xsl:apply-templates select="Content" mode="ContentCardLarge" />
  </xsl:template>
  
  <xsl:template match="Content" mode="ContentCardLarge">
    <div class="sn-contentcard ui-helper-clearfix">
      <!--<div class="sn-cc-image">
        <img src="/Root/Global/images/icons/32/form.png" alt="" title="Content Card" />
      </div>-->
      <div class="sn-cc-content">
        <h4 class="sn-cc-head">
          <img src="/Root/Global/images/icons/16/form.png" alt="" class="sn-cc-typeico" />
          <xsl:value-of select="Fields/DisplayName"/>
          <xsl:text xml:space="preserve"> (</xsl:text>
          <a target="_blank">
            <xsl:attribute name="href">
              <xsl:text xml:space="preserve">/Explore.html#</xsl:text>
              <xsl:value-of select="ContentTypePath" />
            </xsl:attribute>
            <xsl:attribute name="title">
              <xsl:text xml:space="preserve">Explore </xsl:text>
              <xsl:value-of select="ContentType" />
              <xsl:text xml:space="preserve"> content type</xsl:text>
            </xsl:attribute>
            <xsl:value-of select="ContentType"/>
          </a>       
          <xsl:text xml:space="preserve">)</xsl:text>
          <xsl:text xml:space="preserve"> </xsl:text>
          <input type="text" value="{SelfLink}" class="sn-cc-selflink" />
          <div ID="PRCEcms"></div>
        </h4>        
        <!--<div class="sn-cc-info">
          <span>Creation date:</span><b><xsl:value-of select="snf:FormatDate(Fields/CreationDate)" /></b><span class="sn-cc-info-separator">|</span><span>Last modification:</span><b><xsl:value-of select="snf:FormatDate(Fields/ModificationDate)"/></b>
        </div><br />-->        
      </div>
    </div>
  </xsl:template>

  <xsl:template match="*" />
  
</xsl:stylesheet>
