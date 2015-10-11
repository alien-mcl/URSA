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
        var flavours = {
            "C#": {
                "http://www.w3.org/2001/XMLSchema#string": "string",
                "http://www.w3.org/2001/XMLSchema#string[]": "string[]",
                "http://www.w3.org/2001/XMLSchema#boolean": "bool",
                "http://www.w3.org/2001/XMLSchema#boolean[]": "bool[]",
                "http://www.w3.org/2001/XMLSchema#byte": "sbyte",
                "http://www.w3.org/2001/XMLSchema#byte[]": "sbyte[]",
                "http://www.w3.org/2001/XMLSchema#unsignedByte": "byte",
                "http://www.w3.org/2001/XMLSchema#unsignedByte[]": "byte[]",
                "http://www.w3.org/2001/XMLSchema#short": "short",
                "http://www.w3.org/2001/XMLSchema#short[]": "short[]",
                "http://www.w3.org/2001/XMLSchema#unsignedShort": "ushort",
                "http://www.w3.org/2001/XMLSchema#unsignedShort[]": "ushort[]",
                "http://www.w3.org/2001/XMLSchema#int": "int",
                "http://www.w3.org/2001/XMLSchema#int[]": "int[]",
                "http://www.w3.org/2001/XMLSchema#unsignedInt": "uint",
                "http://www.w3.org/2001/XMLSchema#unsignedInt[]": "uint[]",
                "http://www.w3.org/2001/XMLSchema#long": "long",
                "http://www.w3.org/2001/XMLSchema#long[]": "long[]",
                "http://www.w3.org/2001/XMLSchema#unsignedLong": "ulong",
                "http://www.w3.org/2001/XMLSchema#unsignedLong[]": "ulong[]",
                "http://www.w3.org/2001/XMLSchema#float": "float",
                "http://www.w3.org/2001/XMLSchema#float[]": "float[]",
                "http://www.w3.org/2001/XMLSchema#double": "double",
                "http://www.w3.org/2001/XMLSchema#double[]": "double[]",
                "http://www.w3.org/2001/XMLSchema#decimal": "decimal",
                "http://www.w3.org/2001/XMLSchema#decimal[]": "decimal[]",
                "http://www.w3.org/2001/XMLSchema#dateTime": "System.DateTime",
                "http://www.w3.org/2001/XMLSchema#dateTime[]": "System.DateTime[]",
                "http://www.w3.org/2001/XMLSchema#hexBinary": "byte[]",
                "http://openguid.net/rdf#guid": "System.Guid",
                "http://openguid.net/rdf#guid[]": "System.Guid[]",
                "default": function(type) { return type.replace(/^urn:((net|net-enumerable|net-collection|net-list|hydra):)?/, ""); }
            },
            "Java": {
                "http://www.w3.org/2001/XMLSchema#string": "String",
                "http://www.w3.org/2001/XMLSchema#string[]": "String[]",
                "http://www.w3.org/2001/XMLSchema#boolean": "boolean",
                "http://www.w3.org/2001/XMLSchema#boolean[]": "boolean[]",
                "http://www.w3.org/2001/XMLSchema#byte": "byte",
                "http://www.w3.org/2001/XMLSchema#byte[]": "byte[]",
                "http://www.w3.org/2001/XMLSchema#unsignedByte": "short",
                "http://www.w3.org/2001/XMLSchema#unsignedByte[]": "short[]",
                "http://www.w3.org/2001/XMLSchema#short": "short",
                "http://www.w3.org/2001/XMLSchema#short[]": "short[]",
                "http://www.w3.org/2001/XMLSchema#unsignedShort": "int",
                "http://www.w3.org/2001/XMLSchema#unsignedShort[]": "int[]",
                "http://www.w3.org/2001/XMLSchema#int": "int",
                "http://www.w3.org/2001/XMLSchema#int[]": "int[]",
                "http://www.w3.org/2001/XMLSchema#unsignedInt": "long",
                "http://www.w3.org/2001/XMLSchema#unsignedInt[]": "long[]",
                "http://www.w3.org/2001/XMLSchema#long": "long",
                "http://www.w3.org/2001/XMLSchema#long[]": "long[]",
                "http://www.w3.org/2001/XMLSchema#unsignedLong": "long",
                "http://www.w3.org/2001/XMLSchema#unsignedLong[]": "long[]",
                "http://www.w3.org/2001/XMLSchema#float": "float",
                "http://www.w3.org/2001/XMLSchema#float[]": "float[]",
                "http://www.w3.org/2001/XMLSchema#double": "double",
                "http://www.w3.org/2001/XMLSchema#double[]": "double[]",
                "http://www.w3.org/2001/XMLSchema#decimal": "double",
                "http://www.w3.org/2001/XMLSchema#decimal[]": "double[]",
                "http://www.w3.org/2001/XMLSchema#dateTime": "java.util.Date",
                "http://www.w3.org/2001/XMLSchema#dateTime[]": "java.util.Date[]",
                "http://www.w3.org/2001/XMLSchema#hexBinary": "byte[]",
                "http://openguid.net/rdf#guid": "java.util.UUID",
                "http://openguid.net/rdf#guid[]": "java.util.UUID[]",
                "default": function(type) { return type.replace(/^urn:((net|net-enumerable|net-collection|net-list|hydra):)?/, ""); }
            },
            "UML": {
                "http://www.w3.org/2001/XMLSchema#string": "String",
                "http://www.w3.org/2001/XMLSchema#string[]": "String[]",
                "http://www.w3.org/2001/XMLSchema#boolean": "Boolean",
                "http://www.w3.org/2001/XMLSchema#boolean[]": "Boolean[]",
                "http://www.w3.org/2001/XMLSchema#byte": "Integer",
                "http://www.w3.org/2001/XMLSchema#byte[]": "Integer[]",
                "http://www.w3.org/2001/XMLSchema#unsignedByte": "Integer",
                "http://www.w3.org/2001/XMLSchema#unsignedByte[]": "Integer[]",
                "http://www.w3.org/2001/XMLSchema#short": "Integer",
                "http://www.w3.org/2001/XMLSchema#short[]": "Integer[]",
                "http://www.w3.org/2001/XMLSchema#unsignedShort": "Integer",
                "http://www.w3.org/2001/XMLSchema#unsignedShort[]": "Integer[]",
                "http://www.w3.org/2001/XMLSchema#int": "Integer",
                "http://www.w3.org/2001/XMLSchema#int[]": "Integer[]",
                "http://www.w3.org/2001/XMLSchema#unsignedInt": "Integer",
                "http://www.w3.org/2001/XMLSchema#unsignedInt[]": "Integer[]",
                "http://www.w3.org/2001/XMLSchema#long": "Integer",
                "http://www.w3.org/2001/XMLSchema#long[]": "Integer[]",
                "http://www.w3.org/2001/XMLSchema#unsignedLong": "Integer",
                "http://www.w3.org/2001/XMLSchema#unsignedLong[]": "Integer[]",
                "http://www.w3.org/2001/XMLSchema#float": "Real",
                "http://www.w3.org/2001/XMLSchema#float[]": "Real[]",
                "http://www.w3.org/2001/XMLSchema#double": "UnlimitedNatural",
                "http://www.w3.org/2001/XMLSchema#double[]": "UnlimitedNatural[]",
                "http://www.w3.org/2001/XMLSchema#decimal": "UnlimitedNatural",
                "http://www.w3.org/2001/XMLSchema#decimal[]": "UnlimitedNatural[]",
                "http://www.w3.org/2001/XMLSchema#dateTime": "DateTime",
                "http://www.w3.org/2001/XMLSchema#dateTime[]": "DateTime[]",
                "http://www.w3.org/2001/XMLSchema#hexBinary": "Binary",
                "http://openguid.net/rdf#guid": "GUID",
                "http://openguid.net/rdf#guid[]": "GUID[]",
                "default": function(type) { return type.replace(/^urn:((net|net-enumerable|net-collection|net-list|hydra):)?/, ""); }
            }
        };
        var mapType = function(type, flavour, test) {
            var flavour = flavours[flavour];
            var result = flavour[type];
            if (result === undefined) {
                return flavour["default"](type);
            }
            
            return result;
        };
            
        var createMethod = function(supportedClass, supportedOperation, currentFlavour) {
            var returns = "void";
            if (supportedOperation.returns.length > 0) {
                returns = "";
                if (supportedOperation.returns.length > 1) {
                    returns = supportedOperation.label + "Result";
                }
                else {
                    returns = mapType(supportedOperation.returns[0], currentFlavour);
                    if (returns.replace(/\[\]$/, "") === mapType(supportedClass["@id"], currentFlavour)) {
                        returns = supportedClass.label + (returns.replace(/\[\]$/, "") === returns ? "" : "[]");
                    }
                }
            }
                
            var parameters = "";
            if ((supportedOperation.template) && (supportedOperation.mappings)) {
                for (var index = 0; index < supportedOperation.mappings.length; index++) {
                    var mapping = supportedOperation.mappings[index];
                    var parameterType = "object";
                    if (mapping.property) {
                        parameterType = mapType(mapping.property.type, currentFlavour);
                    }
                        
                    if (parameterType.replace(/\[\]$/, "") === mapType(supportedClass["@id"], currentFlavour)) {
                        parameterType = supportedClass.label + (parameterType.replace(/\[\]$/, "") === parameterType ? "" : "[]");
                    }

                    parameters += (currentFlavour === "UML" ? mapping.variable + ": " + parameterType : parameterType + " " + mapping.variable) + ", ";
                }
                    
                parameters = parameters.substr(0, parameters.length - 2);
            }
                
            var arguments = "";
            for (var index = 0; index < supportedOperation.expects.length; index++) {
                var expected = supportedOperation.expects[index];
                var expectedType = mapType(expected.type, currentFlavour);
                if (expectedType.replace(/\[\]$/, "") === mapType(supportedClass["@id"], currentFlavour)) {
                    expectedType = supportedClass.label + (expectedType.replace(/\[\]$/, "") === expectedType ? "" : "[]");
                }

                arguments += (currentFlavour === "UML" ? expected.variable + ": " + expectedType : expectedType + " " + expected.variable) + ", ";
            }
                
            if (arguments.length > 0) {
                arguments = arguments.substr(0, arguments.length - 2);
            }

            var methodName = (currentFlavour === "Java" ? supportedOperation.label.charAt(0).toLowerCase() + supportedOperation.label.substr(1) : supportedOperation.label);
            var result = (currentFlavour === "UML" ?
                methodName + "(" + parameters + ((parameters.length > 0) && (arguments.length > 0) ? ", " : "") + arguments + ")" + (returns === "void" ? "" : ": " + returns) :
                returns + " " + methodName + "(" + parameters + ((parameters.length > 0) && (arguments.length > 0) ? ", " : "") + arguments + ")");
            return result;
        };
            
        var createProperty = function(supportedClass, supportedProperty, currentFlavour) {
            var result = mapType(supportedProperty.type, currentFlavour) + " " + supportedProperty.label;
            switch (currentFlavour) {
                case "C#": result += " " + (supportedProperty.writeOnly ? "{ set; }" : (supportedProperty.readOnly ? "{ get; }" : "{ get; set; }")); break;
                case "Java": result = (supportedProperty.writeOnly ? "@Setter" : (supportedProperty.readOnly ? "@Getter" : "@Getter @Setter")) + " " + result; break;
                case "UML": result = (supportedProperty.writeOnly ? "<<writeonly>>" : (supportedProperty.readOnly ? "<<readonly>>" : "<<property>>")) + " " + result; break; 
            }
            
            return result;
        };
        
        var createMember = function(supportedClass, supportedMember, flavour) {
            if ((supportedClass == null) || (supportedMember == null)) {
                return "";
            }
            
            return (supportedMember["@type"] === hydra.supportedProperty ? createProperty(supportedClass, supportedMember, flavour): createMethod(supportedClass, supportedMember, flavour));
        };
        
        var defaultFlavour = "C#";
        var apiDocumentation = angular.module("ApiDocumentation", ["ngRoute"]).
        controller("MainMenu", ["$scope", function($scope) {
            $scope.currentFlavour = defaultFlavour;
            $scope.changeFlavour = function(newFlavour) {
                $scope.$emit("FlavourChanged", $scope.currentFlavour = newFlavour);
                event.preventDefault();
                event.stopPropagation();
            };
        }]).
        controller("SupportedClasses", ["$scope", function($scope) {
            $scope.supportedClasses = supportedClasses;
            $scope.createMember = createMember;
            $scope.currentFlavour = defaultFlavour;
            $scope.showMember = function(event, supportedClass, supportedMember) {
                $scope.$emit("MemberSelected", supportedClass, supportedMember);
                event.preventDefault();
                event.stopPropagation();
            };
            $scope.$root.$on("FlavourChanged", function(e, newFlavour) {
                $scope.currentFlavour = newFlavour;
            });
        }]).
        controller("MemberDescription", ["$scope", function($scope) {
            $scope.currentClass = null;
            $scope.currentMember = null;
            $scope.createMember = createMember;
            $scope.currentFlavour = defaultFlavour;
            $scope.mapType = function(supportedClass, type, currentFlavour) {
                var result = mapType(type, currentFlavour);
                if (type.replace(/\[\]$/, "") === mapType(supportedClass["@id"], currentFlavour)) {
                    result = supportedClass.label + (result.replace(/\[\]$/, "") === result ? "" : "[]");
                }
                
                return result;
            };
            $scope.isMethod = function(member) {
                return (member) && (member['@type'] === hydra.supportedOperation);
            };
            $scope.returns = function(member) {
                return (member) && (((member.returns) && (member.returns.length > 0)) || (member.type));
            };
            $scope.$root.$on("MemberSelected", function(e, currentClass, currentMember) {
                $scope.currentClass = currentClass;
                $scope.currentMember = currentMember;
            });
            $scope.$root.$on("FlavourChanged", function(e, newFlavour) {
                $scope.currentFlavour = newFlavour;
            });
        }]);
                ]]></script>
            </head>
            <body ng-app="ApiDocumentation">
                <nav class="navbar navbar-default" ng-controller="MainMenu">
                    <div class="container-fluid">
                        <div class="navbar-header">
                            <button type="button" class="navbar-toggle collapsed" data-toggle="collapse" data-target="#main-menu" aria-expanded="false">
                                <span class="sr-only">Toggle navigation</span>
                                <span class="icon-bar"></span>
                                <span class="icon-bar"></span>
                                <span class="icon-bar"></span>
                            </button>
                            <span class="navbar-brand">API Documentation</span>
                        </div>
                        <div class="collapse navbar-collapse" id="main-menu">
                            <ul class="nav navbar-nav">
                                <li ng-class="currentFlavour === 'C#' ? 'active' : ''"><a href="#" ng-click="changeFlavour('C#')">C#</a></li>
                                <li ng-class="currentFlavour === 'Java' ? 'active' : ''"><a href="#" ng-click="changeFlavour('Java')">Java</a></li>
                                <li ng-class="currentFlavour === 'UML'"><a href="#" ng-click="changeFlavour('UML')">UML</a></li>
                            </ul>
                        </div>
                    </div>
                </nav>
                <div class="container-fluid">
                    <div class="col-sm-4 col-xs-12" ng-controller="SupportedClasses">
                        <nav class="panel-group" id="SupportedClasses">
                            <div class="panel panel-default" ng-repeat="supportedClass in supportedClasses">
                                <div class="panel-heading">
                                    <h4 class="panel-title">
                                        <a data-toggle="collapse" data-parent="#SupportedClasses" href="#collapse{{{{ $index }}}}">{{ supportedClass.label }}</a>
                                    </h4>
                                </div>
                                <div id="collapse{{{{ $index }}}}" class="panel-collapse collapse">
                                    <div class="panel-body">
                                        <ul class="list-group">
                                            <a href="#" class="list-group-item list-group-item-property" ng-repeat="supportedProperty in supportedClass.supportedProperties" ng-click="showMember($event, supportedClass, supportedProperty)">
                                                {{ createMember(supportedClass, supportedProperty, currentFlavour) }}
                                            </a>
                                            <a href="#" class="list-group-item list-group-item-method" ng-repeat="supportedOperation in supportedClass.supportedOperations" ng-click="showMember($event, supportedClass, supportedOperation)">
                                                {{ createMember(supportedClass, supportedOperation, currentFlavour) }}
                                            </a>
                                        </ul>
                                    </div>
                                </div>
                            </div>
                        </nav>
                    </div>
                    <div class="col-sm-8 col-xs-12" ng-controller="MemberDescription" ng-show="currentMember">
                        <div class="panel panel-default">
                            <div class="panel-heading">{{ createMember(currentClass, currentMember, currentFlavour) }}</div>
                            <div class="panel-body">
                                <p ng-show="currentMember.description">{{ currentMember.description }}</p>
                                <table class="table" ng-show="isMethod(currentMember)">
                                    <tr>
                                        <th>Name</th>
                                        <th>Type</th>
                                        <th>Description</th>
                                    </tr>
                                    <tr ng-repeat="mapping in currentMember.mappings">
                                        <td>{{ mapping.variable }}</td>
                                        <td ng-attr-title="mapping.property ? mapping.property.type : ''">{{ mapping.property ? mapType(currentClass, mapping.property.type, currentFlavour) : "object" }}</td>
                                        <td>{{ mapping.description }}</td>
                                    </tr>
                                    <tr ng-repeat="expected in currentMember.expects">
                                        <td>{{ expected.variable }}</td>
                                        <td ng-attr-title="expected.type">{{ mapType(currentClass, expected.type, currentFlavour) }}</td>
                                        <td></td>
                                    </tr>
                                </table>
                                <p ng-show="returns(currentMember)">
                                    {{ currentMember.type ? "Type:" : "Returns:" }}
                                    {{ returns(currentMember) ? mapType(currentClass, currentMember.type || currentMember.returns[0], currentFlavour) : "" }}
                                </p>
                            </div>
                        </div>
                    </div>
                </div>
            </body>
        </html>
    </xsl:template>

    <xsl:template match="hydra:ApiDocumentation">
        window.hydra = new String("http://www.w3.org/ns/hydra/core#");
        hydra.supportedProperty = hydra + "supportedProperty";
        var supportedClasses = [];<xsl:for-each select="hydra:supportedClasses"><xsl:variable name="id" select="@rdf:resource" />
        supportedClasses.push(<xsl:call-template name="hydra-Class">
                <xsl:with-param name="class" select="/rdf:RDF/hydra:Class[@rdf:about = $id]|/rdf:RDF/*[rdf:type/@rdf:resource = '&hydra;Class' and @rdf:about = $id]" />
            </xsl:call-template>);
        supportedClasses[supportedClasses.length - 1].supportedOperations.pop();</xsl:for-each>
    </xsl:template>

    <xsl:template name="hydra-Class">
        <xsl:param name="class" />
            {
                "@id": "<xsl:value-of select="$class/@rdf:about" />",
                "label": "<xsl:value-of select="$class/rdfs:label" />",
                "supportedProperties": [<xsl:for-each select="$class/hydra:supportedProperty">
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
                "supportedOperations": [<xsl:for-each select="/rdf:RDF/hydra:Operation[@rdf:about = $class/*/@rdf:resource]|/rdf:RDF/*[rdf:type/@rdf:resource = '&hydra;Operation' and @rdf:about = $class/*/@rdf:resource]">
                    <xsl:variable name="id" select="@rdf:resource|@rdf:about" />
                    <xsl:variable name="localName" select="local-name($class/*[@rdf:resource = $id])" />
                    <xsl:variable name="namespace" select="namespace-uri($class/*[@rdf:resource = $id])" />
                    <xsl:variable name="name">
                        <xsl:choose>
                            <xsl:when test="$localName != 'supportedProperty' and $namespace != '&hydra;'">
                                <xsl:value-of select="concat($namespace, $localName)" />
                            </xsl:when>
                        </xsl:choose>
                    </xsl:variable>
                    <xsl:call-template name="hydra-SupportedOperation">
                        <xsl:with-param name="supportedOperation" select="/rdf:RDF/hydra:Operation[@rdf:about = $id]|/rdf:RDF/*[rdf:type/@rdf:resource = '&hydra;Operation' and @rdf:about = $id]" />
                        <xsl:with-param name="template" select="/rdf:RDF/hydra:IriTemplate[@rdf:about = $name]|/rdf:RDF/*[rdf:type/@rdf:resource = '&hydra;IriTemplate' and @rdf:about = $name]" />
                    </xsl:call-template>,</xsl:for-each><xsl:for-each select="$class/*[@rdf:resource]">
                    <xsl:variable name="templatedLink" select="concat(namespace-uri(current()), local-name(current()))" />
                    <xsl:variable name="iriTemplate" select="@rdf:resource" />
                    <xsl:for-each select="/rdf:RDF/hydra:TemplatedLink[@rdf:about = $templatedLink]/hydra:operation">
                        <xsl:variable name="operation" select="@rdf:resource" />
                        <xsl:for-each select="/rdf:RDF/hydra:Operation[@rdf:about = $operation]|/rdf:RDF/*[rdf:type/@rdf:resource = '&hydra;Operation' and @rdf:about = $operation]">
                            <xsl:variable name="id" select="@rdf:resource|@rdf:about" />
                            <xsl:call-template name="hydra-SupportedOperation">
                                <xsl:with-param name="supportedOperation" select="/rdf:RDF/hydra:Operation[@rdf:about = $id]|/rdf:RDF/*[rdf:type/@rdf:resource = '&hydra;Operation' and @rdf:about = $id]" />
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
                        "@type": hydra.supportedProperty,
                        "label": "<xsl:value-of select="$property/rdfs:label" />",<xsl:if test="$property/rdfs:comment">
                        "description": "<xsl:value-of select="$property/rdfs:comment/text()"/>",</xsl:if>
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
                        "@type": hydra.supportedOperation,
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
                                >"
                            }<xsl:if test="position() != last()">, </xsl:if></xsl:for-each>
                        ],
                        "mappings": [<xsl:for-each select="/rdf:RDF/*[@rdf:about = $template/hydra:mapping/@rdf:resource]">
                            {
                                "required": <xsl:choose><xsl:when test="hydra:required"><xsl:value-of select="hydra:required"/></xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
                                "variable": "<xsl:value-of select="hydra:variable" />",
                                "description": "<xsl:value-of select="rdfs:comment" />"<xsl:if test="hydra:property">,
                                "property": { <xsl:variable name="current" select="hydra:property/@rdf:resource" /><xsl:variable name="targetProperty" select="/rdf:RDF/rdf:Property[@rdf:about = $current]|/rdf:RDF/*[rdf:type[@rdf:resource = '&rdf;Property'] and @rdf:about = $current]" />
                                    "@id": "<xsl:value-of select="hydra:property/@rdf:resource" />",
                                    "type": "<xsl:value-of select="$targetProperty/rdfs:range/@rdf:resource" />",
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