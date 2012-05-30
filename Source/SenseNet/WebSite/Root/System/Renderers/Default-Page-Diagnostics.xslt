<?xml version="1.0" encoding="utf-8"?>
<xsl:transform version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" exclude-result-prefixes="xsl">
  <xsl:output method="html" indent="yes" omit-xml-declaration="yes" />

  <xsl:template match="/">
    <div style="background-color:#FFFFFF; color:#000000;">
      <b>Page execution diagnostics</b>
      <br/>
      Full page duration: <xsl:value-of select="sum(//OpNode/@duration)" />
      <xsl:call-template name="Toolbar" />
      <ul style="list-style-type:decimal;">
        <xsl:apply-templates select="OpNode" />
      </ul>
      <script>$('div[name="x"]').toggle(); $('span[name="x"]').toggle();</script>
      <xsl:call-template name="Toolbar" />
    </div>
  </xsl:template>

  <xsl:template name="Toolbar">
    <br/>
    <a>
      <xsl:attribute name="onclick">$('div[name="x"]').toggle(); $('span[name="x"]').toggle();</xsl:attribute>
      toggle
    </a>
  </xsl:template>

  <xsl:template match="OpNode">
    <li>
      <div style="padding-top: 5px;">
        <span style="font-weight:bold; color:blue;">
          <xsl:if test="./@successful='false'">
            <xsl:attribute name="style">
              font-weight:bold; color: red;
            </xsl:attribute>
          </xsl:if>
          <xsl:value-of select="./@name" />;
        </span>
        <span style="color:green;">
          <xsl:value-of select="./@duration" />;
        </span>
        <span name="x" style="color:black;">
          <xsl:value-of select="./@title" />
        </span>
        <div style="color:black;">
          <div style="background-color:darkgreen; display:inline-block; height:8px;">
            <xsl:attribute name="title">
              <xsl:value-of select="./@duration" />
            </xsl:attribute>
            <span>
              <xsl:attribute name="style">
                margin-left:<xsl:value-of select="round((./@finishTicks - ./@startTicks) div 1000)" />px;
              </xsl:attribute>
            </span>
          </div>
          <div name="x">
            <b>method name: </b>
            <xsl:value-of select="./@methodName" />
          </div>
          <div name="x">
            <b>message: </b>
            <xsl:value-of select="./@message" />
          </div>
        </div>
      </div>
      <xsl:apply-templates select="Children" />
    </li>
  </xsl:template>

  <xsl:template match="Children">
    <xsl:if test="count(OpNode) > 0">
      <!--<a>
        <xsl:attribute name="onclick">$(this).next().toggle();</xsl:attribute>
        toggle children
      </a>-->
      <ul style="padding-left:20px; list-style-type:decimal;">
        <xsl:apply-templates select="OpNode" />
      </ul>
    </xsl:if>
  </xsl:template>

</xsl:transform>