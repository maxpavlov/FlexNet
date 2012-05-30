<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet
    version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    xmlns:snc="sn://SenseNet.Portal.UI.ContentTools"
    xmlns:snf="sn://SenseNet.Portal.UI.XmlFormatTools"
    xmlns:acf="sn://SenseNet.ApplicationModel.ActionFramework"
    exclude-result-prefixes="xsl msxsl snc snf">
  <xsl:output method="html" indent="yes"/>

  <xsl:template match="/">

    <xsl:variable name="uploadLink" select="acf:GetActionUrl(Content/SelfLink, 'Upload', Content/SelfLink)" />
    <xsl:if test="$uploadLink!=''">
      <a href="{Content/SelfLink}?action=upload&amp;back={Content/SelfLink}" style="background-image:url(/Root/Global/images/icons/16/newimage.png)" class="sn-actionlinkbutton icon"> New Image</a>
    </xsl:if>

    <xsl:if test="Content/Children/Content[ContentType='Image']">
      <div class="book-gallery">
        <xsl:for-each select="Content/Children/Content[ContentType='Image']">
          <xsl:sort data-type="number" order="ascending" select="Fields/Index" />
          <!--
          <img src="{Actions/Browse}" title="{Fields/DisplayName}" style="height:150px; margin: 5px;" />
          -->
          <img src="/binaryhandler.ashx?nodeid={Fields/Id}&amp;propertyname=Binary&amp;width=75&amp;height=100" title="{Fields/DisplayName}"/>
        </xsl:for-each>
      </div>
    </xsl:if>

  </xsl:template>

</xsl:stylesheet>
