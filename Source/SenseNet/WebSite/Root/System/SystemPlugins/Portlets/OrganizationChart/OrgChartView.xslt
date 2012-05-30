<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="xml" indent="yes"/>


  <xsl:template match="/">
    <link type="text/css" rel="stylesheet" href="/Root/Global/styles/sn-org-chart.css" />
    <!--<textarea>
      <xsl:copy-of select="/"/>
    </textarea>-->

    <div class="sn-orgc">
      <xsl:apply-templates select="Content">
      </xsl:apply-templates>
    </div>
  </xsl:template>

  <xsl:template match="Content">

    <table cellspacing="0" cellpadding="0" border="0" style="width: 100%; border-collapse: collapse;">
      <tr>
        <td colspan="{count(Employees/Content)}" align="center">
          <div class="sn-orgc-card">
            <a class="sn-orgc-userlink" href="{Actions/Browse}">
              <div class="sn-pic-left">
                <xsl:if test="Fields/Avatar[@imageMode = 'BinaryData']">
                  <img src="{Fields/Avatar}" width="64" height="64" alt="User Image" />
                </xsl:if>
                <xsl:if test="Fields/Avatar[@imageMode = 'None' or @imageMode = 'Reference']">
                  <img src="/Root/Global/images/orgc-missinguser.png" width="64" height="64" alt="Missing User Image" />
                </xsl:if>
              </div>
              <div class="sn-orgc-name sn-content">
                <h1 class="sn-content-title">
                  <xsl:value-of select="Fields/FullName"/>
                </h1>
                <h2 class="sn-content-subtitle">
                  <xsl:value-of select="Fields/Domain"/>\<xsl:value-of select="ContentName"/>
                </h2>
              </div>
              <div class="sn-orgc-position">
                <span>
                  [Position in the company]
                </span>
              </div>
            </a>
          </div>
        </td>
      </tr>

      <xsl:if test="Employees/Content">
        <tr>
          <td colspan="{count(Employees/Content)}" align="center" style="margin: 0; padding: 0; height: 30px;">
            <span class="sn-orgc-border-vertical" style="height: 30px; display: block; width: 1px; margin: 0; padding: 0;"></span>
          </td>
        </tr>

        <xsl:if test="count(Employees/Content)>1">
          <tr>
            <xsl:variable name="maxChildren" select="count(Employees/Content)"/>
            <xsl:for-each select="Employees/Content">
              <xsl:if test="position()=1">
                <td align="right" style="display: block; margin: 0; padding: 0; height: 0;">
                  <span class="sn-orgc-border-horizontal"  style="height: 0; display: block; width: 50%; margin: 0; padding: 0;"></span>
                </td>
              </xsl:if>
              <xsl:if test="position()>1 and position()&lt;$maxChildren">
                <td style="height: 0; margin: 0; padding: 0; height: 0;">
                  <span class="sn-orgc-border-horizontal" style="height: 0; display: block; width: 100%; margin: 0; padding: 0;"></span>
                </td>
              </xsl:if>
              <xsl:if test="position()=$maxChildren and position()>1">
                <td align="left" style="height: 0; margin: 0; padding: 0; height: 0;">
                  <span class="sn-orgc-border-horizontal"  style="height: 0; display: block; width: 50%; margin: 0; padding: 0;"></span>
                </td>
              </xsl:if>
            </xsl:for-each>
          </tr>

          <tr>
            <xsl:for-each select="Employees/Content">
              <td align="center" style="height: 30px; margin: 0; padding: 0;">
                <span class="sn-orgc-border-vertical" style="height: 30px; display: block; width: 1px; margin: 0; padding: 0;"></span>
              </td>
            </xsl:for-each>
          </tr>
        </xsl:if>
        <tr>
          <xsl:for-each select="Employees/Content">
            <td valign="top">
              <xsl:apply-templates select="."></xsl:apply-templates>
            </td>
          </xsl:for-each>
        </tr>
      </xsl:if>
    </table>

  </xsl:template>

</xsl:stylesheet>
