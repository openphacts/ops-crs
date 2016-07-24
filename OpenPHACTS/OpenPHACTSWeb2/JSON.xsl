<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet
  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns="http://www.w3.org/1999/xhtml">
 
  <xsl:output method="html" indent="yes" encoding="UTF-8"/>

  <!--<xsl:variable name="base" select="//resources/@base" />-->

  <xsl:variable name="base">
    <xsl:choose>
      <xsl:when test="substring(//resources/@base, string-length(//resources/@base)) = '/'"><xsl:value-of select="//resources/@base"/></xsl:when>
      <xsl:otherwise><xsl:value-of select="//resources/@base"/>/</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  
  <xsl:template match="/application">
    <html>
      <head>
        <title>JSON handler help</title>

        <link rel="stylesheet" type="text/css" href="common.css" />
        <link rel="stylesheet" type="text/css" href="chemspider.css" />
        <link rel="stylesheet" type="text/css" href="help.css" />
      </head>
      <body>
        <div class="wrapper0" style="z-index:99">
          <div id="header" style="margin-bottom:2em;">
            <div class="logo-main" id="logo-chemspider">
              <a href="/">
                <img alt="ChemSpider logo" src="images/website-logo.png" />
              </a>
            </div>

            <div id="logo-rsc">
              <a target="_blank" href="http://www.rsc.org">
                <img alt="RSC logo" src="images/rsc-logo.png" />
              </a>
            </div>

          </div>

          <xsl:apply-templates />

          <xsl:call-template name="enumes" />
        </div>
      </body>
    </html>
  </xsl:template>
 
  <xsl:template match="resources">
    <div class="round-border">
      <xsl:value-of select="description"/>
    </div>

    <div style="clear:both;">
      <div id="methodsList" style="width:300px; float:left;">
        <h1>Methods</h1>
        <ul>
          <xsl:for-each select="service">
            <li>
              <a href="#{@opParamValue}">
                <xsl:value-of select="@opParamValue"/>
              </a>
            </li>
          </xsl:for-each>
        </ul>
      </div>

      <div id="typesList" style="width:300px; float:left;">
        <h1>Types</h1>
        <ul>
          <xsl:for-each select="//type">
            <li>
              <a href="#{@name}">
                <xsl:value-of select="@name"/>
              </a>
            </li>
          </xsl:for-each>
        </ul>
      </div>

      <div id="enumesList" style="width:300px; float:left;">
        <h1>Enumes</h1>
        <ul>
          <xsl:for-each select="//enum">
            <li>
              <a href="#{@name}">
                <xsl:value-of select="@name"/>
              </a>
            </li>
          </xsl:for-each>
        </ul>
      </div>
    </div>

    <br style="clear:both;" />

    <h1>Methods</h1>
    <xsl:apply-templates select="service" />
  </xsl:template>

  <xsl:template match="service">
    <div class="help-service round-border">
      <h1 id="{@opParamValue}">
        <xsl:value-of select="@opParamValue"/>
      </h1>
      
      <xsl:value-of select="description"/>

      <div>
        <h4 class="subHeading">Parameters</h4>

        <table>
          <tr>
            <th style="width:20%">Name</th>
            <th style="width:20%">Type</th>
            <th style="width:60%">Description</th>
          </tr>
          <xsl:for-each select="parameters/parameter">
            <xsl:variable name="type" select="@type" />
            <tr>
              <td class="parameter">
                <xsl:value-of select="@bodyParam"/><xsl:value-of select="@urlParam"/>
              </td>
              <td>
                <xsl:choose>
                  <xsl:when test="//type[@name=$type] or //enum[@name=$type]">
                    <a href="#{$type}">
                      <xsl:value-of select="$type"/>
                    </a>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="$type"/>
                  </xsl:otherwise>
                </xsl:choose>
              </td>
              <td>
                <xsl:value-of select="description"/>
                <xsl:if test="obsolete">
                  <span class="obsolete">
                    Obsolete and will be removed soon. <xsl:value-of select="obsolete"/>
                  </span>
                </xsl:if>
              </td>
            </tr>
          </xsl:for-each>
        </table>

        <h4 class="subHeading">Return Value</h4>
        <dl>
          <dd>
            Type:
            <xsl:variable name="type" select="return/@type" />
            <xsl:choose>
              <xsl:when test="//type[@name=$type]">
                <a href="#{$type}">
                  <xsl:value-of select="$type"/>
                </a>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$type"/>
              </xsl:otherwise>
            </xsl:choose>
            <xsl:if test="description">
              <br />
              <p>
                <xsl:value-of select="return/description"/>
              </p>
            </xsl:if>
          </dd>
        </dl>

        <xsl:if test="usage">
          <h4 class="subHeading">Example</h4>

          <xsl:for-each select="usage">
            <p>
              <xsl:value-of select="description"/>
            </p>

            <xsl:variable name="example_url">
              <xsl:value-of select="$base"/>
              <xsl:value-of select="example"/>
            </xsl:variable>

            <a href="{$example_url}" target="_JSONExample">
              <xsl:value-of select="$example_url"/>
            </a>
          </xsl:for-each>
          
        </xsl:if>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="types">
    <h1>Types</h1>

    <xsl:apply-templates select="//type" />
  </xsl:template>

  <xsl:template match="type">
    <div class="help-service round-border">
      <h1 id="{@name}">
        <xsl:value-of select="@name"/>
      </h1>

      <xsl:if test="description">
         <xsl:value-of select="description"/>
      </xsl:if>

      <div>
        <h4 class="subHeading">Properties</h4>

        <table>
          <tr>
            <th style="width:20%">Name</th>
            <th style="width:20%">Type</th>
            <th style="width:60%">Description</th>
          </tr>
          <xsl:for-each select="property">
            <xsl:variable name="type" select="@type" />
            <tr>
              <td class="parameter">
                <xsl:value-of select="@name"/>
              </td>
              <td>
                <xsl:choose>
                  <xsl:when test="//type[@name=$type] or //enum[@name=$type]">
                    <a href="#{$type}">
                      <xsl:value-of select="$type"/>
                    </a>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="$type"/>
                  </xsl:otherwise>
                </xsl:choose>
              </td>
              <td>
                <xsl:copy-of select="description/node()" />
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </div>

    </div>
  </xsl:template>

  <xsl:template name="enumes">
    <h1>Enumes</h1>

    <xsl:apply-templates select="//enum" />
  </xsl:template>

  <xsl:template match="enum">
    <div class="help-service round-border">
      <A NAME="{@name}" id="{@name}" />
      <xsl:value-of select="@name"/>

      <div>
        <h4 class="subHeading">Possible values</h4>

        <ul>
          <xsl:for-each select="element">
            <li>
              <xsl:value-of select="@value"/>: <xsl:value-of select="@name"/>
            </li>
          </xsl:for-each>
        </ul>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="error">
    <!--<xsl:value-of select="."/>-->
  </xsl:template>

</xsl:stylesheet>