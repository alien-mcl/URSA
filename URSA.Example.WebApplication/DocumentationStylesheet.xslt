<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
    xmlns:rdfs="http://www.w3.org/2000/01/rdf-schema#" xmlns:xsd="http://www.w3.org/2001/XMLSchema#" xmlns:hydra="http://www.w3.org/ns/hydra/core#" xmlns:owl="http://www.w3.org/2002/07/owl#" xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#">
    <xsl:output method="html" indent="yes" encoding="utf-8" omit-xml-declaration="yes" media-type="text/html" />

    <xsl:template match="/rdf:RDF">
        <xsl:text disable-output-escaping='yes'>&lt;!DOCTYPE html></xsl:text>
        <html>
            <head>
                <title></title>
                <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/3.3.2/css/bootstrap.min.css"></link>
                <script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/3.3.2/js/bootstrap.min.js"></script>
                <script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/angular.js/1.3.14/angular.min.js"></script>
                <script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/angular.js/1.3.14/angular-route.min.js"></script>
                <script type="text/javascript">
                    <xsl:apply-templates select="hydra:ApiDocumentation" />
        var apiDocumentation = angular.module("apiDocumentation", ["ngRoute"]).
        controller("SupportedClasses", ["$scope", function($scope) {
            $scope.items = supportedClasses;
        }]);
                </script>
            </head>
            <body>
                <div ng-app="apiDocumentation">
                    <nav ng-controller="SupportedClasses">
                        <ul>
                            <li ng-repeat="item in items">{{ item.label }}</li>
                        </ul>
                    </nav>
                </div>
            </body>
        </html>
    </xsl:template>

    <xsl:template match="hydra:ApiDocumentation">
        var supportedClasses = [];<xsl:for-each select="hydra:supportedClasses"><xsl:variable name="id" select="@rdf:resource" />
        supportedClasses.push(<xsl:call-template name="hydra-Class">
                <xsl:with-param name="class" select="/rdf:RDF/hydra:Class[@rdf:about = $id]|/rdf:RDF/*[rdf:type/@rdf:resource = 'http://www.w3.org/ns/hydra/core#Class' and @rdf:about = $id]" />
            </xsl:call-template><xsl:if test="position() != last()">,</xsl:if>
        );
        </xsl:for-each>
    </xsl:template>

    <xsl:template name="hydra-Class">
        <xsl:param name="class" />
            {
                "@id": "<xsl:value-of select="$class/@rdf:about" />",
                "label": "<xsl:value-of select="$class/rdfs:label" />",
                "supportedProperties": [<xsl:for-each select="$class/hydra:supportedProperties">
                    <xsl:variable name="id" select="@rdf:resource" />
                    <xsl:call-template name="hydra-SupportedProperty">
                        <xsl:with-param name="supportedProperty" select="/rdf:RDF/hydra:SupportedProperty[@rdf:about = $id]|/rdf:RDF/*[rdf:type/@rdf:resource = 'http://www.w3.org/ns/hydra/core#SupportedProperty' and @rdf:about = $id]" />
                    </xsl:call-template><xsl:if test="position() != last()">,</xsl:if>
                </xsl:for-each>
                ],
                "supportedOperations": [<xsl:for-each select="/rdf:RDF/hydra:Operation[@rdf:about = $class/*/@rdf:resource]">
                    <xsl:variable name="id" select="@rdf:resource|@rdf:about" />
                    <xsl:variable name="localName" select="local-name($class/*[@rdf:resource = $id])" />
                    <xsl:variable name="namespace" select="namespace-uri($class/*[@rdf:resource = $id])" />
                    <xsl:variable name="name" select="concat($namespace, $localName)" />
                    <xsl:call-template name="hydra-SupportedOperation">
                        <xsl:with-param name="supportedOperation" select="/rdf:RDF/hydra:Operation[@rdf:about = $id]" />
                        <xsl:with-param name="template" select="/rdf:RDF/hydra:IriTemplate[@rdf:about = $name]|/rdf:RDF/*[rdf:type/@rdf:resource = 'http://www.w3.org/ns/hydra/core#IriTemplate' and @rdf:about = $name]" />
                    </xsl:call-template><xsl:if test="position() != last()">,</xsl:if>
                </xsl:for-each>
                ]
            }</xsl:template>

    <xsl:template name="hydra-SupportedProperty">
        <xsl:param name="supportedProperty" />
        <xsl:variable name="property" select="/rdf:RDF/rdfs:Property[@rdf:about = $supportedProperty/hydra:property/@rdf:resource]|/rdf:RDF/*[rdf:type/@rdf:resource = 'http://www.w3.org/2000/01/rdf-schema#Property' and @rdf:about = $supportedProperty/hydra:property/@rdf:resource]" />
                    {
                        "@id": "<xsl:value-of select="$supportedProperty/@rdf:about" />",
                        "label": "<xsl:value-of select="$property/rdfs:label" />",
                        "type": "<xsl:value-of select="$property/rdfs:range/@rdf:resource" />",
                        "readonly": <xsl:choose><xsl:when test="$supportedProperty/hydra:readonly"><xsl:value-of select="$supportedProperty/hydra:readonly"/></xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
                        "writeonly": <xsl:choose><xsl:when test="$supportedProperty/hydra:writeonly"><xsl:value-of select="$supportedProperty/hydra:writeonly"/></xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>
                    }</xsl:template>
    
    <xsl:template name="hydra-SupportedOperation">
        <xsl:param name="supportedOperation" />
        <xsl:param name="template" />
                    {
                        "@id": "<xsl:value-of select="$supportedOperation/@rdf:about" />",
                        "label": "<xsl:value-of select="$supportedOperation/rdfs:label" />",<xsl:if test="$template">
                        "template": "<xsl:value-of select="$template/hydra:template" />",</xsl:if><xsl:if test="$supportedOperation/hydra:expects">
                        "expects": "<xsl:value-of select="$supportedOperation/hydra:expects/@rdf:resource" />",</xsl:if>
                        "mappings": [<xsl:for-each select="/rdf:RDF/*[@rdf:about = $template/hydra:mapping/@rdf:resource]">
                            {
                                "required": <xsl:choose><xsl:when test="hydra:required"><xsl:value-of select="hydra:required"/></xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
                                "variable": "<xsl:value-of select="hydra:variable" />"
                            }<xsl:if test="position() != last()">,</xsl:if>
                        </xsl:for-each>
                        ],
                        "method": [<xsl:for-each select="$supportedOperation/hydra:method">
                            "<xsl:value-of select="." />"<xsl:if test="position() != last()">,</xsl:if>
            </xsl:for-each>
                        ]
                    }</xsl:template>
</xsl:stylesheet>