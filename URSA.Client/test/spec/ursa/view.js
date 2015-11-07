/// <reference path="/scripts/_references.js"/>
//ReSharperReporter.prototype.jasmineDone = function() { };
(function() {
    "use strict";

    var apiDocumentation = new ursa.model.ApiDocumentation(window.apiDocumentation);
    var renderer = null;
    var view = null;
    var scope = null;
    var apiMember = null;
    var httpResult = null;
    var http = function() { return httpResult; };
    var jsonld = {};

    describe("Given instance of the view generator engine's", function() {
        beforeEach(function() {
            jasmine.addMatchers(matchers);
            scope = {
                targetInstance: "newInstance",
                operation: apiDocumentation.supportedClasses[0].supportedOperations[0]
            };
        });

        describe("ursa.view.SupportedPropertyRenderer class", function() {
            beforeEach(function () {
                renderer = new ursa.view.SupportedPropertyRenderer();
            });

            describe("when rendering an xsd:int based property", function() {
                beforeEach(function() {
                    view = renderer.render(scope, null, null, apiMember = apiDocumentation.supportedClasses[0].supportedProperties[1], null);
                });

                it("it should render it's view correctly", function () {
                    var expected = String.format(
                        "<div class=\"input-group\">" +
                            "<span class=\"input-group-addon\">{0}</span>" +
                            "<input class=\"form-control\" ng-model=\"{1}['{3}'][0]['@value']\" ng-readonly=\"supportedPropertyKeys['{2}'] || supportedPropertyNulls['{2}']\" " +
                            "type=\"number\" step=\"1\" min=\"-2147483648\" max=\"2147483647\" placeholder=\"[-2147483648, 2147483647]\" />" +
                            "<span class=\"input-group-addon\"><input type=\"checkbox\" title=\"Null\" checked ng-model=\"supportedPropertyNulls['{2}']\" ng-change=\"onIsNullCheckedChanged('{2}')\" /></span>" +
                        "</div>",
                        apiMember.label,
                        scope.targetInstance,
                        apiMember.id,
                        apiMember.property);
                    expect(view).toBe(expected);
                });
            });

            describe("when rendering an multiple xsd:string values based property", function () {
                beforeEach(function () {
                    view = renderer.render(scope, null, null, apiMember = apiDocumentation.supportedClasses[0].supportedProperties[2], null);
                });

                it("it should render it's view correctly", function () {
                    var expected = String.format(
                        "<div class=\"input-group\" ng-repeat=\"value in {1}['{3}'] track by value\">" +
                            "<span class=\"input-group-addon\">{0}</span>" +
                            "<input class=\"form-control\" ng-model=\"{1}['{3}'][$index]['@value']\" ng-readonly=\"supportedPropertyKeys['{2}'] || supportedPropertyNulls['{2}']\" type=\"text\" placeholder=\"text\" />" +
                            "<span class=\"input-group-btn\"><button class=\"btn btn-default\" ng-click=\"removePropertyItem('{2}', $index)\"><span class=\"glyphicon glyphicon-remove\"></span></button></span>" +
                        "</div>" +
                        "<div class=\"input-group\">" + 
                            "<span class=\"input-group-addon\">{0}</span>" +
                            "<input class=\"form-control\" ng-model=\"supportedPropertyNewValues['{3}']['@value']\" ng-readonly=\"supportedPropertyKeys['{2}'] || supportedPropertyNulls['{2}']\" type=\"text\" placeholder=\"text\" />" +
                            "<span class=\"input-group-btn\"><button class=\"btn btn-default\" ng-click=\"addPropertyItem('{2}')\"><span class=\"glyphicon glyphicon-plus\"></span></button></span>" +
                        "</div>",
                        apiMember.label,
                        scope.targetInstance,
                        apiMember.id,
                        apiMember.property);
                    expect(view).toBe(expected);
                });
            });
        });

        describe("ursa.view.OperationRenderer class", function() {
            beforeEach(function() {
                renderer = new ursa.view.OperationRenderer();
            });

            describe("when rendering an entity collection operation", function() {
                beforeEach(function() {
                    apiMember = apiDocumentation.supportedClasses[0].supportedOperations[1];
                    httpResult = {
                        then: function(handler) {
                            handler({ headers:function() { return "application/json"; }, data: [apiDocumentation.supportedClasses[0].createInstance(apiMember)] });
                        }
                    };

                    view = renderer.render(scope, http, jsonld, apiMember, null);
                });

                it("it should render it's view correctly", function() {
                    var expected = String.format(
                        "<table class=\"table table-condensed table-bordered table-hover\">" +
                            "<tr>" +
                                "<th ng-repeat=\"supportedProperty in supportedProperties track by supportedProperty.id\" ng-hide=\"supportedProperty.key\">{{ supportedProperty.label }}</th>" +
                                "<th><select ng-model=\"itemsPerPage\" ng-change=\"list(1)\" ng-options=\"take for take in itemsPerPageList track by take\"></select></th>" +
                            "</tr>" +
                            "<tr ng-repeat-start=\"entity in entities track by entity['{0}'][0]['@value']\" ng-hide=\"entityEquals(entity)\" title=\"{{ entity['{0}'][0]['@value'] | asId }}\">" +
                                "<td ng-repeat=\"supportedProperty in supportedProperties track by supportedProperty.id\" ng-hide=\"supportedProperty.key\">{{ getPropertyValue(entity, supportedProperty, operation) | stringify }}</td>" +
                                "<td><div class=\"btn-block\">" +
                                    "<button class=\"btn btn-default\" title=\"Edit\" ng-click=\"get(entity)\"><span class=\"glyphicon glyphicon-pencil\"></span></button>" +
                                "</div></td>" +
                            "</tr>" +
                            "<tr ng-repeat-end ng-show=\"entityEquals(entity)\">" +
                                "<td colspan=\"{1}\"><ursa-api-member-view api-member=\"operation.owner\" target-instance=\"editedEntity\"></ursa-api-member-view></td>" +
                                "<td><div class=\"btn-block\">" +
                                    "<button class=\"btn btn-default\" title=\"Cancel\" ng-click=\"cancel()\"><span class=\"glyphicon glyphicon-repeat\"></span></button>" +
                                "</div></td>" +
                            "</tr>" +
                            "<tr ng-hide=\"editedEntity !== null\">" +
                                "<td colspan=\"{1}\"><ursa-api-member-view api-member=\"operation.owner\" target-instance=\"newInstance\"></ursa-api-member-view></td>" +
                                "<td><div class=\"btn-block\">" +
                                "</div></td>" +
                            "</tr>" +
                            "<tr>" +
                                "<td colspan=\"{2}\"><nav><ul class=\"pagination\">" +
                                    "<li ng-hide=\"currentPage <= 1\"><a href ng-click=\"list(currentPage - 1)\">&laquo;</a></li>" +
                                    "<li ng-repeat=\"page in pages\"><a href ng-click=\"list(page)\">{{ page }}</a></li>" +
                                    "<li ng-hide=\"currentPage >= pages.length\"><a href ng-click=\"list(currentPage + 1)\">&raquo;</a></li>" +
                                "</ul></nav></td>" +
                            "</tr>" +
                        "</table>",
                        apiDocumentation.supportedClasses[0].supportedProperties[3].property,
                        apiDocumentation.supportedClasses[0].supportedProperties.length - 2,
                        apiDocumentation.supportedClasses[0].supportedProperties.length - 1);
                    expect(view).toBe(expected);
                });
            });
        });
    });
}());