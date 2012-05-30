<?xml version="1.0" encoding="utf-8"?>
<xsl:transform version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" exclude-result-prefixes="xsl">
  <xsl:output method="html" indent="yes" omit-xml-declaration="yes" />
  
	  <xsl:template match="/">
		<div>
            <table>
               <tr>
                  <td width="150px"><b>Content</b></td>
                  <td><b>Path</b></td>
               </tr>
               <xsl:for-each select="/Content/Children/Content">
                  <tr>
                     <td width="150px"><xsl:value-of select="Fields/DisplayName" /></td>
                     <td><xsl:value-of select="Fields/Path" /></td>
                  </tr>
               </xsl:for-each>
            </table>
		</div>
	  </xsl:template>
  
</xsl:transform> 
