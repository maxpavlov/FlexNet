<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
                xmlns="http://schemas.sensenet.com/SenseNet/ContentRepository/SearchExpression"
                xmlns:ed="http://schemas.sensenet.com/SenseNet/ContentRepository/QueryEditor"
                xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <xsl:output method="xml" indent="yes" />

  <xsl:template match="/">
    <SearchExpression>
      <And>
        <xsl:apply-templates />
      </And>
    </SearchExpression>
  </xsl:template>

  <xsl:template match="ed:ExpressionItem[(@xsi:type = 'Group') and not(@logicalOperator)]">
    <xsl:apply-templates />
  </xsl:template>
  <xsl:template match="ed:ExpressionItem[(@xsi:type = 'Group') and (@logicalOperator = 'None')]">
    <xsl:apply-templates />
  </xsl:template>
  <xsl:template match="ed:ExpressionItem[(@xsi:type = 'Group') and (@logicalOperator != 'None')]">
    <xsl:element name="{@logicalOperator}">
      <xsl:apply-templates />
    </xsl:element>
  </xsl:template>

  <xsl:template match="ed:ExpressionItem[(@xsi:type = 'Predicate') and not(@logicalOperator)]">
    <xsl:call-template name="RenderPredicate" />
  </xsl:template>
  <xsl:template match="ed:ExpressionItem[(@xsi:type = 'Predicate') and (@logicalOperator = 'None')]">
    <xsl:call-template name="RenderPredicate" />
  </xsl:template>
  <xsl:template match="ed:ExpressionItem[(@xsi:type = 'Predicate') and (@logicalOperator != 'None')]">
    <xsl:element name="{@logicalOperator}">
      <xsl:call-template name="RenderPredicate" />
    </xsl:element>
  </xsl:template>
  <xsl:template name="RenderPredicate">
    <xsl:choose>
      <!-- String -->
      <xsl:when test="@type = 'StringExpression'">
        <xsl:element name="String">
          <xsl:call-template name="RenderInnerPredicate" />
        </xsl:element>
      </xsl:when>
      <!-- Int -->
      <xsl:when test="@type = 'IntExpression'">
        <xsl:element name="Int">
          <xsl:call-template name="RenderInnerPredicate" />
        </xsl:element>
      </xsl:when>
      <!-- DateTime -->
      <xsl:when test="@type = 'DateTimeExpression'">
        <xsl:element name="DateTime">
          <xsl:call-template name="RenderInnerPredicate" />
        </xsl:element>
      </xsl:when>
      <!-- Currency -->
      <xsl:when test="@type = 'CurrencyExpression'">
        <xsl:element name="Currency">
          <xsl:call-template name="RenderInnerPredicate" />
        </xsl:element>
      </xsl:when>
      <!-- Type -->
      <xsl:when test="@type = 'TypeExpression'">
        <xsl:element name="Type">
          <xsl:attribute name="nodeType">
            <xsl:value-of select="ed:LeftOperand"/>
          </xsl:attribute>
          <xsl:if test="@operator = 'Equal' or @operator = 'Equals'">
            <xsl:attribute name="exactMatch">yes</xsl:attribute>
          </xsl:if>
          <xsl:apply-templates select="ed:RightOperand" />
        </xsl:element>
      </xsl:when>
      <!-- Reference -->
      <xsl:when test="@type = 'ReferenceExpression'">
        <xsl:element name="Reference">
          <xsl:attribute name="property">
            <xsl:value-of select="ed:LeftOperand"/>
          </xsl:attribute>
          <xsl:choose>
            <xsl:when test="@operator = 'Exists'">
              <xsl:attribute name="existenceOnly">true</xsl:attribute>
            </xsl:when>
            <xsl:otherwise>
              <xsl:attribute name="referencedNodeId">
                <xsl:value-of select="ed:RightOperand"/>
              </xsl:attribute>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:element>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  <!---->
  <xsl:template name="RenderInnerPredicate">
    <xsl:attribute name="property">
      <xsl:value-of select="ed:LeftOperand"/>
    </xsl:attribute>
    <xsl:attribute name="op">
      <xsl:value-of select="@operator"/>
    </xsl:attribute>
    <xsl:apply-templates select="ed:RightOperand" />
  </xsl:template>

  <!-- Copy RightOperand into the new namespace -->
  <xsl:template match="ed:RightOperand">
    <xsl:apply-templates mode="copy" />
  </xsl:template>
  <xsl:template match="*" mode="copy">
    <xsl:element name="{local-name()}">
      <xsl:for-each select="@* | node()">
        <xsl:apply-templates select="." mode="copy"/>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>
  <xsl:template match="@*" mode="copy">
    <xsl:attribute name="{local-name()}">
      <xsl:value-of select="."/>
    </xsl:attribute>
  </xsl:template>

</xsl:stylesheet>
