<?xml version="1.0" ?>
<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:wix="http://schemas.microsoft.com/wix/2006/wi">

  <!-- Copy all attributes and elements to the output. -->
  <xsl:template match="@*|*">
    <xsl:copy>
      <xsl:apply-templates select="@*" />
      <xsl:apply-templates select="*" />
    </xsl:copy>
  </xsl:template>

  <!-- ### Adding the Win64-attribute to all Components -->
  <xsl:template match="wix:Component">
    <xsl:copy>
      <xsl:apply-templates select="@*" />
      <!-- Adding the Win64-attribute if we have a x64 application -->
      <xsl:attribute name="Win64">$(var.Win64)</xsl:attribute>

      <!-- Now take the rest of the inner tag -->
      <xsl:apply-templates select="node()" />
    </xsl:copy>
  </xsl:template>

  <xsl:template match="wix:Wix">
    <xsl:copy>
      <xsl:processing-instruction name="include">..\..\Bitness.wxi</xsl:processing-instruction>
      <xsl:apply-templates select="@*" />
      <xsl:apply-templates select="*" />
    </xsl:copy>
  </xsl:template>

  <xsl:output method="xml" indent="yes" />

  <!-- Remove .exe files, do them manually -->
  <xsl:key name="exe-search" match="wix:Component['.exe' = substring(wix:File/@Source, string-length(wix:File/@Source) - string-length('.exe') +1)]" use="@Id" />
  <xsl:template match="wix:Component[key('exe-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('exe-search', @Id)]" />

  <!-- Remove .pdb files -->
  <xsl:key name="pdb-search" match="wix:Component['.pdb' = substring(wix:File/@Source, string-length(wix:File/@Source) - string-length('.pdb') +1)]" use="@Id" />
  <xsl:template match="wix:Component[key('pdb-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('pdb-search', @Id)]" />
</xsl:stylesheet>