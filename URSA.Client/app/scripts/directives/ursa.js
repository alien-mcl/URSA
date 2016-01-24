/*globals ursa, angular */
(function() {
    "use strict";

    var getScopePropertyDeep = function(selector) {
        var currentScope = this;
        while (currentScope !== null) {
            if ((currentScope[selector] !== undefined) && (currentScope._ursaApiMemberView)) {
                return currentScope;
            }

            currentScope = currentScope.$parent;
        }

        return undefined;
    };

    var module;
    try {
        module = angular.module("ursa");
    }
    catch (exception) {
        module = angular.module("ursa", []) ;
    }

    module.
    directive("ursaApiMemberView", ["$parse", "$animate", "$anchorScroll", "viewRendererProvider", function ($parse, $animate, $anchorScroll, viewRendererProvider) {
        return {
            restrict: "E",
            priority: 400,
            terminal: true,
            transclude: "element",
            scope: {
                apiMember: "=",
                targetInstance: "@",
                uniqueId: "@"
            },
            controller: angular.noop,
            compile: function(element, attr) {
                var autoScrollExp = attr.autoscroll;
                return function(scope, $element, $attr, ctrl, $transclude) {
                    var currentScope;
                    var previousElement;
                    var currentElement;
                    var cleanupLastIncludeContent = function(dontClearTemplate) {
                        if (previousElement) {
                            previousElement.remove();
                            previousElement = null;
                        }

                        if (currentScope) {
                            currentScope.$destroy();
                            currentScope = null;
                        }

                        if (currentElement) {
                            $animate.leave(currentElement).then(function() { previousElement = null; });
                            previousElement = currentElement;
                            currentElement = null;
                        }

                        if (!!!dontClearTemplate) {
                            ctrl.template = null;
                        }
                    };
                    Object.defineProperty(scope, "_ursaApiMemberView", { enumerable: false, configurable: false, value: true, writable: false });
                    scope.$watch("apiMember", function ursaApiMemberViewWatchAction(apiMember, oldValue) {
                        var afterAnimation = function() {
                            if ((angular.isDefined(autoScrollExp)) && ((!autoScrollExp) || (scope.$eval(autoScrollExp)))) {
                                $anchorScroll();
                            }
                        };
                        if (!(apiMember instanceof ursa.model.ApiMember)) {
                            cleanupLastIncludeContent();
                            return;
                        }
                        else if ((apiMember !== oldValue) && (oldValue instanceof ursa.model.ApiMember)) {
                            if (apiMember.id !== oldValue.id) {
                                cleanupLastIncludeContent();
                            }
                            else {
                                return;
                            }
                        }
                        else if ((apiMember === oldValue) && (ctrl.template !== undefined)) {
                            return;
                        }

                        var newScope = scope.$new();
                        var parentScope = getScopePropertyDeep.call(scope, "newInstance");
                        if (parentScope !== undefined) {
                            parentScope.$watch("newInstance", function(newValue) { newScope.newInstance = newValue; });
                            newScope.newInstance = parentScope.newInstance;
                        }

                        parentScope = getScopePropertyDeep.call(scope, "editedEntity");
                        if (parentScope !== undefined) {
                            parentScope.$watch("editedEntity", function(newValue) { newScope.editedEntity = newValue; });
                            newScope.editedEntity = parentScope.editedEntity;
                        }

                        parentScope = getScopePropertyDeep.call(scope, "operation");
                        if (parentScope !== undefined) {
                            parentScope.$watch("operation", function(newValue) { newScope.operation = newValue; });
                            newScope.operation = parentScope.operation;
                        }

                        var viewRenderer = viewRendererProvider.createRenderer(apiMember);
                        if (viewRenderer !== null) {
                            ctrl.template = viewRenderer.render(newScope);
                        }
                        else {
                            ctrl.template = "Error generating view for api member.";
                        }

                        var clone = $transclude(newScope, function(clone) {
                            cleanupLastIncludeContent(true);
                            $animate.enter(clone, null, $element).then(afterAnimation);
                        });
                        currentScope = newScope;
                        currentElement = clone;
                        currentScope.$emit("apiMemberViewGenerated", apiMember);
                    });
                };
            }
        };
    }]).
    directive("ursaApiMemberView", ["$compile", function($compile) {
        return {
            restrict: "E",
            priority: -400,
            require: "ursaApiMemberView",
            link: function(scope, $element, $attr, ctrl) {
                if (ctrl.template !== null) {
                    $element.html(ctrl.template);
                    $compile($element.contents())(scope);
                }
            }
        };
    }]);
}());