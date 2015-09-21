<?xml version="1.0" encoding="utf-8"?>
<!DOCTYPE xsl:stylesheet[
    <!ENTITY rdf 'http://www.w3.org/1999/02/22-rdf-syntax-ns#'>
    <!ENTITY rdfs 'http://www.w3.org/2000/01/rdf-schema#'>
    <!ENTITY owl 'http://www.w3.org/2002/07/owl#'>
    <!ENTITY xsd 'http://www.w3.org/2001/XMLSchema#'>
    <!ENTITY hydra 'http://www.w3.org/ns/hydra/core#'>
    <!ENTITY ursa 'http://github.io/ursa/vocabulary#'>
]>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
    xmlns:rdfs="http://www.w3.org/2000/01/rdf-schema#" xmlns:xsd="http://www.w3.org/2001/XMLSchema#" xmlns:hydra="http://www.w3.org/ns/hydra/core#" 
    xmlns:owl="http://www.w3.org/2002/07/owl#" xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#" xmlns:ursa="http://github.io/ursa/vocabulary#">
    <xsl:output method="html" indent="yes" encoding="utf-8" omit-xml-declaration="yes" media-type="text/html" />

    <xsl:template match="/rdf:RDF">
        <xsl:text disable-output-escaping='yes'>&lt;!DOCTYPE html></xsl:text>
        <html>
            <head>
                <title></title>
                <meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no" />
                <style type="text/css"><![CDATA[
                    .list-group-item.list-group-item-property { padding-left:30px; }
                    .list-group-item.list-group-item-property:before { position:absolute; left:12px; top:0px; width:16px; height:100%; content:' '; background:url(/property) center no-repeat; }
                    .list-group-item.list-group-item-method { padding-left:30px; }
                    .list-group-item.list-group-item-method:before { position:absolute; left:15px; top:0px; width:16px; height:100%; content:' '; background:url(/method) center no-repeat; }
                ]]></style>
                <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/3.3.2/css/bootstrap.min.css"></link>
                <script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/jquery/2.1.3/jquery.min.js"></script>
                <script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/3.3.2/js/bootstrap.min.js"></script>
                <script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/angular.js/1.3.14/angular.min.js"></script>
                <script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/angular.js/1.3.14/angular-route.min.js"></script>
                <script type="text/javascript"><xsl:apply-templates select="hydra:ApiDocumentation" /><![CDATA[
        var apiDocumentation = angular.module("ApiDocumentation", ["ngRoute"]).
        controller("SupportedClasses", ["$scope", function($scope) {
            $scope.supportedClasses = supportedClasses;
            $scope.mapType = function(type) {
                switch (type) {
                    case "http://www.w3.org/2001/XMLSchema#string": return "string";
                    case "http://www.w3.org/2001/XMLSchema#string[]": return "string[]";
                    case "http://www.w3.org/2001/XMLSchema#byte": return "sbyte";
                    case "http://www.w3.org/2001/XMLSchema#byte[]": return "sbyte[]";
                    case "http://www.w3.org/2001/XMLSchema#unsignedByte": return "byte";
                    case "http://www.w3.org/2001/XMLSchema#unsignedByte[]": return "byte[]";
                    case "http://www.w3.org/2001/XMLSchema#short": return "short";
                    case "http://www.w3.org/2001/XMLSchema#short[]": return "short[]";
                    case "http://www.w3.org/2001/XMLSchema#unsignedShort": return "ushort";
                    case "http://www.w3.org/2001/XMLSchema#unsignedShort[]": return "ushort[]";
                    case "http://www.w3.org/2001/XMLSchema#int": return "int";
                    case "http://www.w3.org/2001/XMLSchema#int[]": return "int[]";
                    case "http://www.w3.org/2001/XMLSchema#unsignedInt": return "uint";
                    case "http://www.w3.org/2001/XMLSchema#unsignedInt[]": return "uint[]";
                    case "http://www.w3.org/2001/XMLSchema#long": return "long";
                    case "http://www.w3.org/2001/XMLSchema#long[]": return "long[]";
                    case "http://www.w3.org/2001/XMLSchema#unsignedLong": return "ulong";
                    case "http://www.w3.org/2001/XMLSchema#unsignedLong[]": return "ulong[]";
                    case "http://www.w3.org/2001/XMLSchema#float": return "float";
                    case "http://www.w3.org/2001/XMLSchema#float[]": return "float[]";
                    case "http://www.w3.org/2001/XMLSchema#double": return "double";
                    case "http://www.w3.org/2001/XMLSchema#double[]": return "double[]";
                    case "http://www.w3.org/2001/XMLSchema#decimal": return "decimal";
                    case "http://www.w3.org/2001/XMLSchema#decimal[]": return "decimal[]";
                    case "http://www.w3.org/2001/XMLSchema#dateTime": return "System.DateTime";
                    case "http://www.w3.org/2001/XMLSchema#dateTime[]": return "System.DateTime[]";
                    case "http://www.w3.org/2001/XMLSchema#hexBinary": return "byte";
                    case "http://openguid.net/rdf#guid": return "System.Guid";
                    case "http://openguid.net/rdf#guid[]": return "System.Guid[]";
                    default: return type.replace(/^urn:((net|net-enumerable|net-collection|net-list|hydra):)?/,"");
                }
            };
            
            $scope.createMethod = function(supportedClass, supportedOperation) {
                var returns = "void";
                if (supportedOperation.returns.length > 0) {
                    returns = "";
                    if (supportedOperation.returns.length > 1) {
                        returns = supportedOperation.label + "Result";
                    }
                    else {
                        returns = this.mapType(supportedOperation.returns[0]);
                        if (returns.replace(/\[\]$/, "") === this.mapType(supportedClass["@id"])) {
                            returns = supportedClass.label + (returns.replace(/\[\]$/, "") == returns ? "" : "[]");
                        }
                    }
                }
                
                var parameters = "";
                if ((supportedOperation.template) && (supportedOperation.mappings)) {
                    for (var index = 0; index < supportedOperation.mappings.length; index++) {
                        var mapping = supportedOperation.mappings[index];
                        var parameterType = "object";
                        if (mapping.property) {
                            parameterType = this.mapType(mapping.property.type);
                        }
                        
                        parameters += parameterType + " " + mapping.variable + ", ";
                    }
                    
                    parameters = parameters.substr(0, parameters.length - 2);
                }
                
                var arguments = "";
                for (var index = 0; index < supportedOperation.expects.length; index++) {
                    var expected = supportedOperation.expects[index];
                    arguments += this.mapType(expected.type) + " " + expected.variable + ", ";
                }
                
                if (arguments.length > 0) {
                    arguments = arguments.substr(0, arguments.length - 2);
                }
                
                var result = returns + " " + supportedOperation.label + "(" + parameters + ((parameters.length > 0) && (arguments.length > 0) ? ", " : "") + arguments + ")";
                return result;
            };
            
            $scope.createProperty = function(supportedClass, supportedProperty) {
                return this.mapType(supportedProperty.type) + " " + supportedProperty.label;
            }
        }]);
                ]]></script>
            </head>
            <body ng-app="ApiDocumentation">
                <div class="container-fluid">
                    <div class="col-sm-4 col-xs-12">
                        <nav class="panel-group" ng-controller="SupportedClasses" id="SupportedClasses">
                            <div class="panel panel-default" ng-repeat="supportedClass in supportedClasses">
                                <div class="panel-heading">
                                    <h4 class="panel-title">
                                        <a data-toggle="collapse" data-parent="#SupportedClasses" href="#collapse{{{{ $index }}}}">{{ supportedClass.label }}</a>
                                    </h4>
                                </div>
                                <div id="collapse{{{{ $index }}}}" class="panel-collapse collapse in">
                                    <div class="panel-body">
                                        <ul class="list-group">
                                            <li class="list-group-item list-group-item-property" ng-repeat="supportedProperty in supportedClass.supportedProperties">{{ createProperty(supportedClass, supportedProperty) }}</li>
                                            <li class="list-group-item list-group-item-method" ng-repeat="supportedOperation in supportedClass.supportedOperations">{{ createMethod(supportedClass, supportedOperation) }}</li>
                                        </ul>
                                    </div>
                                </div>
                            </div>
                        </nav>
                    </div>
                    <div class="col-sm-8 col-xs-12"></div>
                </div>
            </body>
        </html>
    </xsl:template>

    <xsl:template match="hydra:ApiDocumentation">
        var supportedClasses = [];<xsl:for-each select="hydra:supportedClasses"><xsl:variable name="id" select="@rdf:resource" />
        supportedClasses.push(<xsl:call-template name="hydra-Class">
                <xsl:with-param name="class" select="/rdf:RDF/hydra:Class[@rdf:about = $id]|/rdf:RDF/*[rdf:type/@rdf:resource = '&hydra;Class' and @rdf:about = $id]" />
            </xsl:call-template><xsl:if test="position() != last()">,</xsl:if>
        );
        supportedClasses[supportedClasses.length - 1].supportedOperations.pop();
        </xsl:for-each>
    </xsl:template>

    <xsl:template name="hydra-Class">
        <xsl:param name="class" />
            {
                "@id": "<xsl:value-of select="$class/@rdf:about" />",
                "label": "<xsl:value-of select="$class/rdfs:label" />",
                "supportedProperties": [<xsl:for-each select="$class/hydra:supportedProperties">
                    <xsl:variable name="id" select="@rdf:resource" />
                    <xsl:variable name="supportedProperty" select="/rdf:RDF/hydra:SupportedProperty[@rdf:about = $id]|/rdf:RDF/*[rdf:type/@rdf:resource = '&hydra;SupportedProperty' and @rdf:about = $id]" />
                    <xsl:variable name="property" select="/rdf:RDF/rdf:Property[@rdf:about = $supportedProperty/hydra:property/@rdf:resource]|/rdf:RDF/*[rdf:type/@rdf:resource = '&rdf;Property' and @rdf:about = $supportedProperty/hydra:property/@rdf:resource]" />
                    <xsl:variable name="isEnumerable">
                        <xsl:choose>
                            <xsl:when test="/rdf:RDF/owl:Restriction[owl:onProperty[@rdf:resource = $property/@rdf:about] and owl:maxCardinality/text() = '1']">false</xsl:when>
                            <xsl:otherwise>true</xsl:otherwise>
                        </xsl:choose>
                    </xsl:variable>
                    <xsl:call-template name="hydra-SupportedProperty">
                        <xsl:with-param name="supportedProperty" select="$supportedProperty" />
                        <xsl:with-param name="isEnumerable" select="$isEnumerable" />
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
                        <xsl:with-param name="template" select="/rdf:RDF/hydra:IriTemplate[@rdf:about = $name]|/rdf:RDF/*[rdf:type/@rdf:resource = '&hydra;IriTemplate' and @rdf:about = $name]" />
                    </xsl:call-template>,</xsl:for-each><xsl:for-each select="$class/*[@rdf:resource]">
                    <xsl:variable name="templatedLink" select="concat(namespace-uri(current()), local-name(current()))" />
                    <xsl:variable name="iriTemplate" select="@rdf:resource" />
                    <xsl:for-each select="/rdf:RDF/hydra:TemplatedLink[@rdf:about = $templatedLink]/hydra:operation">
                        <xsl:variable name="operation" select="@rdf:resource" />
                        <xsl:for-each select="/rdf:RDF/hydra:Operation[@rdf:about = $operation]">
                            <xsl:variable name="id" select="@rdf:resource|@rdf:about" />
                            <xsl:call-template name="hydra-SupportedOperation">
                                <xsl:with-param name="supportedOperation" select="/rdf:RDF/hydra:Operation[@rdf:about = $id]" />
                                <xsl:with-param name="template" select="/rdf:RDF/hydra:IriTemplate[@rdf:about = $iriTemplate]|/rdf:RDF/*[rdf:type/@rdf:resource = '&hydra;IriTemplate' and @rdf:about = $iriTemplate]" />
                            </xsl:call-template>,</xsl:for-each></xsl:for-each></xsl:for-each>
                    {
                    }
                ]
                }</xsl:template>

    <xsl:template name="hydra-SupportedProperty">
        <xsl:param name="supportedProperty" />
        <xsl:param name="isEnumerable" />
        <xsl:variable name="property" select="/rdf:RDF/rdf:Property[@rdf:about = $supportedProperty/hydra:property/@rdf:resource]|/rdf:RDF/*[rdf:type/@rdf:resource = '&rdf;Property' and @rdf:about = $supportedProperty/hydra:property/@rdf:resource]" />
                    {
                        "@id": "<xsl:value-of select="$supportedProperty/@rdf:about" />",
                        "label": "<xsl:value-of select="$property/rdfs:label" />",<xsl:if test="$supportedProperty/rdfs:comment">
                        "description": "<xsl:value-of select="$supportedProperty/rdfs:comment/text()"/>",</xsl:if>
                        "type": "<xsl:call-template name="type"><xsl:with-param name="type" select="$property/rdfs:range/@rdf:resource" /><xsl:with-param name="isEnumerable" select="$isEnumerable" /></xsl:call-template>",
                        "property": "<xsl:value-of select="$property/@rdf:about" />",
                        "readonly": <xsl:choose><xsl:when test="$supportedProperty/hydra:readonly"><xsl:value-of select="$supportedProperty/hydra:readonly"/></xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
                        "writeonly": <xsl:choose><xsl:when test="$supportedProperty/hydra:writeonly"><xsl:value-of select="$supportedProperty/hydra:writeonly"/></xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>
                    }</xsl:template>
    
    <xsl:template name="hydra-SupportedOperation">
        <xsl:param name="supportedOperation" />
        <xsl:param name="template" />
        <xsl:variable name="isEnumerable">
            <xsl:choose>
                <xsl:when test="/rdf:RDF/rdf:Property[@rdf:about = /rdf:RDF/hydra:IriTemplateMapping[@rdf:about = $template/hydra:mapping/@rdf:resource]/hydra:property/@rdf:resource and rdf:type/@rdf:resource = '&owl;InverseFunctionalProperty']">false</xsl:when>
                <xsl:otherwise>true</xsl:otherwise>
            </xsl:choose>
        </xsl:variable>
                    {
                        "@id": "<xsl:value-of select="$supportedOperation/@rdf:about" />",
                        "label": "<xsl:value-of select="$supportedOperation/rdfs:label" />",<xsl:if test="$template">
                        "template": "<xsl:value-of select="$template/hydra:template" />",</xsl:if><xsl:if test="$supportedOperation/rdfs:comment">
                        "description": "<xsl:value-of select="$supportedOperation/rdfs:comment/text()"/>",</xsl:if>
                        "returns": [<xsl:for-each select="$supportedOperation/hydra:returns">"<xsl:variable name="current" select="./@rdf:nodeID" 
                            /><xsl:variable name="resource" select="/rdf:RDF/*[@rdf:nodeID = $current]" 
                            /><xsl:variable name="enumerable"><xsl:choose><xsl:when test="$resource/ursa:singleValue/text() = 'true'">false</xsl:when><xsl:otherwise>true</xsl:otherwise></xsl:choose></xsl:variable><xsl:call-template name="type"
                                ><xsl:with-param name="type" select="$resource/ursa:resourceType/@rdf:resource" 
                                /><xsl:with-param name="isEnumerable" select="$enumerable" 
                                /></xsl:call-template
                            >"<xsl:if test="position() != last()">, </xsl:if></xsl:for-each>],
                        "expects": [<xsl:for-each select="$supportedOperation/hydra:expects">
                            {   <xsl:variable name="current" select="./@rdf:nodeID" />
                                <xsl:variable name="resource" select="/rdf:RDF/*[@rdf:nodeID = $current]" />
                                <xsl:variable name="enumerable"><xsl:choose><xsl:when test="$resource/ursa:singleValue/text() = 'true'">false</xsl:when><xsl:otherwise>true</xsl:otherwise></xsl:choose></xsl:variable>
                                "variable": "<xsl:value-of select="$resource/rdfs:label" />",
                                "type": "<xsl:call-template name="type"
                                    ><xsl:with-param name="type" select="$resource/ursa:resourceType/@rdf:resource" 
                                    /><xsl:with-param name="isEnumerable" select="$enumerable" 
                                    /></xsl:call-template
                                >"<xsl:if test="position() != last()">, </xsl:if>
                            }</xsl:for-each>
                        ],
                        "mappings": [<xsl:for-each select="/rdf:RDF/*[@rdf:about = $template/hydra:mapping/@rdf:resource]">
                            {
                                "required": <xsl:choose><xsl:when test="hydra:required"><xsl:value-of select="hydra:required"/></xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
                                "variable": "<xsl:value-of select="hydra:variable" />"<xsl:if test="hydra:property">,
                                "property": { <xsl:variable name="current" select="hydra:property/@rdf:resource" />
                                    "@id": "<xsl:value-of select="hydra:property/@rdf:resource" />",
                                    "type": "<xsl:value-of select="/rdf:RDF/rdf:Property[@rdf:about = $current]/rdfs:range/@rdf:resource|/rdf:RDF/*[rdf:type[@rdf:resource = '&rdf;Property'] and @rdf:about = $current]/rdfs:range/@rdf:resource"/>"
                                }</xsl:if>
                            }<xsl:if test="position() != last()">,</xsl:if>
                        </xsl:for-each>
                        ],
                        "method": [<xsl:for-each select="$supportedOperation/hydra:method">
                            "<xsl:value-of select="." />"<xsl:if test="position() != last()">,</xsl:if>
            </xsl:for-each>
                        ]
                    }</xsl:template>

    <xsl:template name="type">
        <xsl:param name="type" />
        <xsl:param name="isEnumerable" />
        <xsl:value-of select="$type" /><xsl:if test="$isEnumerable = 'true'">[]</xsl:if>
    </xsl:template>
</xsl:stylesheet>