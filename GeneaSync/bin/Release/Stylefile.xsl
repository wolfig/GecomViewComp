<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:template match="/">
    <html>
      <body>
        <h2>GEDCOM View and Compare XML Data </h2>
        <p>
          Number Of Records: <xsl:value-of select="count(CATALOG/INDI_L0)"/>
        </p>
        <table border="1">
          <tr>
            <th>Complete Name</th>
            <th>First Name</th>
            <th>Last Name</th>
            <th>Date of Birth</th>
            <th>Place of Birth</th>
            <th>Date of Death</th>
            <th>Place of Death</th>
            <th>Source File</th>
          </tr>
          <xsl:for-each select="CATALOG/INDI_L0">
            <xsl:sort select="NAME_L1/@Value" />

            <xsl:variable name="theColor">
              <xsl:choose>
                <xsl:when test="@Checked='Missing'">
                  <xsl:text>lightcoral</xsl:text>
                </xsl:when>
                <xsl:when test="@Checked='Name-Match'">
                  <xsl:text>cadetblue</xsl:text>
                </xsl:when>
                <xsl:when test="@Checked='Ambigious'">
                  <xsl:text>gold</xsl:text>
                </xsl:when>
                <xsl:when test="@Checked='Match'">
                  <xsl:text>white</xsl:text>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:text>red</xsl:text>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            
            <tr bgcolor="{$theColor}">
              <td>
                 <xsl:value-of select="NAME_L1/@Value" />
              </td>
              <td>
                 <xsl:value-of select="NAME_L1/SURN_L2/@Value" />
              </td>
              <td>
                 <xsl:value-of select="NAME_L1/GIVN_L2/@Value" />
              </td>
              <td>
                 <xsl:value-of select="BIRT_L1/DATE_L2/@Value" />
              </td>
              <td>
                 <xsl:value-of select="BIRT_L1/PLAC_L2/@Value" />
              </td>
              <td>
                 <xsl:value-of select="DEAT_L1/DATE_L2/@Value" />
              </td>
              <td>
                 <xsl:value-of select="BIRT_L1/PLAC_L2/@Value" />
              </td>
              <td>
                 <xsl:value-of select="@File" />
              </td>
              <td>
                 <xsl:value-of select="@Checked" />
              </td>
           </tr>
          </xsl:for-each>
        </table>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>