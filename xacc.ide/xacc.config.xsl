<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
xmlns:xacc="xacc:config">
<xsl:output method="html" />
<!-- this is really silly, but i cant figure out how to handle xmlns's here -->
<xsl:template match="*">

<html>
<head><title>Configuration summary</title></head>
<body>
<h3>
Configuration summary
</h3>

<xsl:for-each select="xacc:action">
  

<table width="100%" style="border-style: 1px" cellspacing="0" cellpadding="2" >
<tr><td colspan="3" style="background-image: none; color: white;  background-color: black"><font size="4" ><strong>Action: <xsl:value-of select="@name"/></strong></font></td></tr>
<tr bgcolor="lightblue"><td colspan="3"><font size="2"><em><xsl:value-of select="xacc:description"/></em></font></td></tr>
<tr bgcolor="beige"><td width="250px"><font size="2"><strong>ID</strong></font></td><td colspan="2"><font size="2"><xsl:value-of select="@id"/></font> </td></tr>
<tr bgcolor="beige"><td><font size="2"><strong>Program</strong></font></td><td colspan="2"><font size="2"><xsl:value-of select="@program"/></font></td></tr>
<tr bgcolor="beige"><td><font size="2"><strong>Default arguments</strong></font></td><td colspan="2"><font size="2"><xsl:value-of select="@defaultargs"/></font></td></tr>
<tr bgcolor="beige"><td><font size="2"><strong>Accepts multiple input</strong></font></td><td colspan="2"><font size="2"><xsl:value-of select="@multipleinput"/></font></td></tr>
<tr bgcolor="beige"><td><font size="2"><strong>Input extensions</strong></font></td><td colspan="2"><font size="2"><xsl:value-of select="@inputext"/></font></td></tr>
<tr bgcolor="beige"><td><font size="2"><strong>Output extensions</strong></font></td><td colspan="2"><font size="2"><xsl:value-of select="@outputext"/></font></td></tr>
<tr bgcolor="beige"><td><font size="2"><strong>Image</strong></font></td><td colspan="2"><font size="2"><xsl:value-of select="@image"/></font></td></tr>


<xsl:for-each select="xacc:envvars">
	<tr>
	<td colspan="3" style="background-image: none; color: white;  background-color: gray">
	<font size="4" ><strong>Enviromental Variables</strong></font>
	</td>
	</tr>
	<xsl:for-each select="xacc:envvar">
	<tr bgcolor="beige"><td><font size="2"><strong><xsl:value-of select="@name"/></strong></font></td><td colspan="2"><font size="2"><xsl:value-of select="."/></font></td></tr>
	</xsl:for-each>
</xsl:for-each>

<xsl:for-each select="xacc:options">


<xsl:for-each select="xacc:category">
	
	<tr>
	<td colspan="3" style="background-image: none; color: white;  background-color: gray">
	<font size="4" ><strong><xsl:value-of select="@name"/></strong></font>
	</td>
	</tr>
	<tr bgcolor="lightblue">
	<td colspan="3">
	<font size="2"><em><xsl:value-of select="@description"/></em></font>
	</td>
	</tr>
	
	<xsl:for-each select="*">
	<tr bgcolor="beige">
	<td width="250px">
	<font size="2"><strong><xsl:value-of select="@name"/></strong></font>
	</td>
	<td width="120px">
	<font size="2"><em>
	<xsl:if test="@argtype != ''">
	<xsl:value-of select="@argtype"/>
	</xsl:if>
	<xsl:if test="@argtype = false()">
	string
	</xsl:if>
	</em></font>
	</td>
	<td>
	<font size="2"><xsl:value-of select="@description"/></font>
		<xsl:for-each select="*">
			<xsl:if test="name() = 'allowedvalue'">
		[	<font size="2"><xsl:value-of select="."/></font> ]
			</xsl:if>
		</xsl:for-each>
	</td>
	</tr>
	</xsl:for-each>
	
</xsl:for-each>
</xsl:for-each>
	</table><br />
</xsl:for-each>

<xsl:for-each select="xacc:project">
<table width="100%" style="border-style: 1px" cellspacing="0" cellpadding="2" >
<tr><td colspan="3" style="background-image: none; color: white;  background-color: black"><font size="4" > <strong>Project: <xsl:value-of select="@name" /> </strong></font></td></tr>
<tr bgcolor="lightblue"><td colspan="3"><font size="2"><em><xsl:value-of select="@description"/></em></font></td></tr>
<tr bgcolor="beige"><td width="250px"><font size="2"><strong>ID</strong></font></td><td colspan="2"><font size="2"><xsl:value-of select="@id"/></font> </td></tr>
<tr><td colspan="3" style="background-image: none; color: white;  background-color: gray"><font size="3"><strong>Actions</strong></font></td></tr>

<xsl:for-each select="xacc:action">
<tr bgcolor="beige"><td width="250px" colspan="3" ><font size="2"><xsl:value-of select="@ref"/></font></td></tr>
</xsl:for-each>
</table><br />
</xsl:for-each>

<table width="100%" style="border-style: 1px" cellspacing="0" cellpadding="2" >
<tr><td colspan="3" style="background-image: none; color: white;  background-color: black"><font size="4" ><strong>Plugins</strong></font></td></tr>

<xsl:for-each select="xacc:plugin">
  <tr bgcolor="beige"><td colspan="3"><font size="2"><em><xsl:value-of select="."/></em></font></td></tr>
</xsl:for-each>

</table><br />

<table width="100%" style="border-style: 1px" cellspacing="0" cellpadding="2" >
<tr><td colspan="3" style="background-image: none; color: white;  background-color: black"><font size="4" ><strong>Tools</strong></font></td></tr>

<xsl:for-each select="xacc:tool">
<tr bgcolor="beige"><td width="250px"><font size="2"><strong><xsl:value-of select="@name"/></strong></font></td><td colspan="2"><font size="2"><xsl:value-of select="@command"/></font> </td></tr>
</xsl:for-each>

</table><br />


<xsl:for-each select="xacc:language">
<table width="100%" style="border-style: 1px" cellspacing="0" cellpadding="2" >
<tr><td colspan="3" style="background-image: none; color: white;  background-color: black"><font size="4" ><strong>Language: <xsl:value-of select="@name"/></strong></font></td></tr>
<xsl:for-each select="xacc:action">
  <tr bgcolor="beige"><td colspan="3"><font size="2"><xsl:value-of select="@ref"/></font></td></tr>
</xsl:for-each>
</table><br />
</xsl:for-each>




</body>
</html>

</xsl:template>
</xsl:stylesheet>
  