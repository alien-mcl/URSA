/// <reference path="/scripts/_references.js"/>
//ReSharperReporter.prototype.jasmineDone = function() { };
/*globals ursa, matchers */
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
                operation: apiDocumentation.supportedClasses[0].supportedOperations[0],
                $on: function() { },
                $emit: function() { },
                $broadcast: function() { }
            };
            scope.$root = scope;
        });

        describe("ursa.view.SupportedPropertyRenderer class", function() {
            beforeEach(function () {
                renderer = new ursa.view.SupportedPropertyRenderer();
            });

            describe("when rendering an xsd:int based property", function() {
                beforeEach(function () {
                    renderer.initialize(apiMember = apiDocumentation.supportedClasses[0].supportedProperties[1], null, null, null);
                    view = renderer.render(scope);
                });

                it("it should render it's view correctly", function () {
                    var expected = String.format(
                        "<div class=\"input-group\">" +
                            "<span ng-class=\"styleFor('{2}', null)\">{0}</span>" +
                            "<input class=\"form-control\" ng-model=\"{1}['{3}'][0]['@value']\" name=\"{3}\" ng-disabled=\"isPropertyReadonly('{2}')\" " +
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
                    renderer.initialize(apiMember = apiDocumentation.supportedClasses[0].supportedProperties[2], null, null, null);
                    view = renderer.render(scope);
                });

                it("it should render it's view correctly", function () {
                    var expected = String.format(
                        "<div class=\"input-group\" ng-repeat=\"value in {1}['{3}']\">" +
                            "<span ng-class=\"styleFor('{2}', $index)\">{0}</span>" +
                            "<input class=\"form-control\" ng-model=\"{1}['{3}'][$index]['@value']\" ng-attr-name=\"{{'{3}_' + $index}}\" ng-disabled=\"isPropertyReadonly('{2}')\" type=\"text\" placeholder=\"text\" />" +
                            "<span class=\"input-group-btn\"><button class=\"btn btn-default\" ng-click=\"removePropertyItem('{2}', $index)\"><span class=\"glyphicon glyphicon-remove\"></span></button></span>" +
                        "</div>" +
                        "<div class=\"input-group\">" + 
                            "<span ng-class=\"styleFor('{2}', -1)\">{0}</span>" +
                            "<input class=\"form-control\" ng-model=\"supportedPropertyNewValues['{3}']['@value']\" name=\"{3}_new\" ng-disabled=\"isPropertyReadonly('{2}')\" type=\"text\" placeholder=\"text\" />" +
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

            describe("when rendering an entity collection operation", function () {
                var entityId = "http://temp.uri/resource/id";
                var httpResultTransformer = function (data) { return [data]; };
                var status = ursa.model.HttpStatusCodes.OK;
                beforeEach(function() {
                    apiMember = apiDocumentation.supportedClasses[0].supportedOperations[1];
                    httpResult = {
                        then: function (handler) {
                            if (status !== ursa.model.HttpStatusCodes.OK) {
                                return httpResult;
                            }

                            var entity = apiDocumentation.supportedClasses[0].createInstance(apiMember);
                            entity["@id"] = entityId;
                            handler({ headers: function() { return "application/json"; }, data: httpResultTransformer(entity) });
                            return httpResult;
                        },
                        catch: function(handler) {
                            handler({ headers: function() { return "Basic"; }, status: status });
                            return httpResult;
                        }
                    };

                    var filterProvider = new ursa.model.FilterProvider();
                    filterProvider.resolve = function () { return null; };
                    filterProvider.providesCapabilities = function() { return true; };
                    renderer.initialize(apiMember, http, jsonld, null, filterProvider);
                    view = renderer.render(scope);
                });

                it("it should render it's view correctly", function() {
                    var expected = String.format(
                        "<table class=\"table table-condensed table-bordered table-hover\">" +
                            "<tr>" +
                                "<th ng-repeat=\"supportedProperty in supportedProperties track by supportedProperty.id\" ng-hide=\"supportedProperty.key\">" +
                                    "<span class=\"form-control\" ng-show=\"supportedProperties.getById(supportedProperty.id) === null\">{{ supportedProperty.label }}</span>" +
                                    "<input class=\"form-control\" ng-hide=\"supportedProperties.getById(supportedProperty.id) == null\" type=\"text\" ng-model=\"filters[supportedProperty.property]\" placeholder=\"{{ supportedProperty.label }}\" ng-change=\"list()\"/>" +
                                "</th>" +
                                "<th><select class=\"form-control\" ng-model=\"filters.itemsPerPage\" ng-change=\"list(1)\" ng-options=\"take for take in itemsPerPageList track by take\"></select></th>" +
                            "</tr>" +
                            "<tr ng-repeat-start=\"entity in entities track by entity['{0}'][0]['@value']\" ng-hide=\"entityEquals(entity)\" title=\"{{ entity['{0}'][0]['@value'] | asId }}\">" +
                                "<td ng-repeat=\"supportedProperty in supportedProperties track by supportedProperty.id\" ng-hide=\"supportedProperty.key\">{{ getPropertyValue(entity, supportedProperty, operation) | stringify }}</td>" +
                                "<td><div class=\"btn-block\">" +
                                    "<button class=\"btn btn-default\" title=\"Edit\" ng-click=\"get(entity)\"><span class=\"glyphicon glyphicon-pencil\"></span></button>" +
                                "</div></td>" +
                            "</tr>" +
                            "<tr ng-repeat-end ng-show=\"entityEquals(entity)\" ng-init=\"initialize($index)\">" +
                                "<td colspan=\"{1}\"><div><ursa-api-member-view api-member=\"operation.owner\" target-instance=\"editedEntity\" unique-id=\"{{ uniqueId[$index] }}\"></ursa-api-member-view></div></td>" +
                                "<td><div class=\"btn-block\">" +
                                    "<button class=\"btn btn-default\" title=\"Cancel\" ng-click=\"cancel()\"><span class=\"glyphicon glyphicon-repeat\"></span></button>" +
                                "</div></td>" +
                            "</tr>" +
                            "<tr ng-hide=\"editedEntity !== null\" ng-init=\"initialize(-1)\">" +
                                "<td colspan=\"{1}\"><div ng-show=\"footerVisible\"><ursa-api-member-view api-member=\"operation.owner\" target-instance=\"newInstance\" unique-id=\"{{ uniqueId.footer }}\"></ursa-api-member-view></div></td>" +
                                "<td><div class=\"btn-block\">" +
                                "</div></td>" +
                            "</tr>" +
                            "<tr>" +
                                "<td colspan=\"{2}\"><nav><ul class=\"pagination\">" +
                                    "<li ng-hide=\"filters.currentPage <= 1\"><a href ng-click=\"list(filters.currentPage - 1)\">&laquo;</a></li>" +
                                    "<li ng-repeat=\"page in pages\"><a href ng-click=\"list(page)\">{{ page }}</a></li>" +
                                    "<li ng-hide=\"filters.currentPage >= pages.length\"><a href ng-click=\"list(filters.currentPage + 1)\">&raquo;</a></li>" +
                                "</ul></nav></td>" +
                            "</tr>" +
                        "</table>",
                        apiDocumentation.supportedClasses[0].supportedProperties[3].property,
                        apiDocumentation.supportedClasses[0].supportedProperties.length - 2,
                        apiDocumentation.supportedClasses[0].supportedProperties.length - 1);

                    expect(view).toBe(expected);
                });

                it("it should load a selected entity", function () {
                    httpResultTransformer = function(entity) { return entity; };

                    scope.get({ "@id": entityId });

                    expect(scope.editedEntity).not.toBe(null);
                    expect(scope.editedEntity["@id"]).toBe(entityId);
                });

                it("it should prompt for authentication", function() {
                    httpResultTransformer = function (entity) { return entity; };
                    status = ursa.model.HttpStatusCodes.Unauthorized;
                    var eventRaised;
                    scope.$root.$broadcast = function(event) { eventRaised = event; };

                    scope.get({ "@id": entityId });

                    expect(eventRaised).toBe(ursa.view.Events.AuthenticationRequired);
                });
            });
        });
    });
}());