<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html"/>

  <xsl:key name="list" match="Content/Children/Content" use="substring(Fields/PublishDate,1,10)"/>
  <xsl:template match="/">
    <ul>
      <xsl:for-each select="Content/Children/Content[generate-id(.)=generate-id(key('list',substring(Fields/PublishDate,1,10)))]/Fields/PublishDate">
        <xsl:sort case-order="upper-first" select="."/>
        <li>
          <span>
            Released:<br/>
            <xsl:value-of select="substring(.,1,10)"/>
          </span>
          <ul>

            <xsl:for-each select="key('list', substring(.,1,10))">
              <xsl:sort/>
              <li>
                <xsl:apply-templates select="."/>
              </li>
            </xsl:for-each>

          </ul>
        </li>
      </xsl:for-each>
    </ul>
  </xsl:template>

  <xsl:template match="Content">
    <div>
      <div>
        <a href="{Actions/Details}">
          <img src="alma.jpg" class="almaImg"/>
        </a>
      </div>
      <div>
        <span>
          <xsl:value-of select="Fields/DisplayName"/>
        </span>

        <xsl:if test="Fields/IsRateable='True'">
          <span>//TODO: RATING!!!</span>
        </xsl:if>

        <span>
          by: <xsl:value-of select="Fields/Author"/>
        </span>
        <span>
          <a class="sn-actionlinkbutton icon" href="{Actions/Delete}" style="background-image:url=(/Root/Global/images/icons/16/delete.png);">
            Delete
          </a>
          <a class="sn-actionlinkbutton icon" href="{Actions/Edit}" style="background-image:url=(/Root/Global/images/icons/16/edit.png);">
            Edit
          </a>
        </span>
      </div>
    </div>
  </xsl:template>
</xsl:stylesheet>