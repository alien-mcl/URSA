/*globals ursa, angular */
(function(namespace) {
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

    var ApiMemberViewDirective = namespace.ApiMemberView = function(parser, animate, anchorScroll, viewRendererProvider) {
        var that = this;
        this._parser = parser;
        this._animate = animate;
        this._anchorScoll = anchorScroll;
        this._viewRendererProvider = viewRendererProvider;
        this.restrict = "E";
        this.priority = 400;
        this.terminal = true;
        this.transclude = "element";
        this.scope = {
            apiMember: "=",
            targetInstance: "@",
            uniqueId: "@"
        };
        this.controller = angular.noop;
        this.compile = function(element, attr) { return _ApiMemberViewDirective.compileInternal.call(that, element, attr); };
    };
    ApiMemberViewDirective.prototype.restrict = null;
    ApiMemberViewDirective.prototype.priority = 0;
    ApiMemberViewDirective.prototype.terminal = false;
    ApiMemberViewDirective.prototype.transclude = null;
    ApiMemberViewDirective.prototype.scope = null;
    ApiMemberViewDirective.prototype.controller = null;
    ApiMemberViewDirective.prototype.compile = function() {};
    Object.defineProperty(ApiMemberViewDirective.prototype, "_parser", { enumerable: false, configurable: false, writable: true, value: null });
    Object.defineProperty(ApiMemberViewDirective.prototype, "_animate", { enumerable: false, configurable: false, writable: true, value: null });
    Object.defineProperty(ApiMemberViewDirective.prototype, "_anchorScroll", { enumerable: false, configurable: false, writable: true, value: null });
    Object.defineProperty(ApiMemberViewDirective.prototype, "_viewRendererProvider", { enumerable: false, configurable: false, writable: true, value: null });
    var _ApiMemberViewDirective = {};
    _ApiMemberViewDirective.compileInternal = function(element, attr) {
        var that = this;
        return function(scope, $element, $attr, ctrl, $transclude) {
            _ApiMemberViewDirective.linkInternal.call(that, scope, $element, $attr, ctrl, $transclude, attr.autoscroll);
        };
    };
    _ApiMemberViewDirective.cleanupLastIncludeContent = function(context, dontClearTemplate) {
        if (context.previousElement) {
            context.previousElement.remove();
            context.previousElement = null;
        }

        if (context.currentScope) {
            context.currentScope.$destroy();
            context.currentScope = null;
        }

        if (context.currentElement) {
            this._animate.leave(context.currentElement).then(function() { context.previousElement = null; });
            context.previousElement = context.currentElement;
            context.currentElement = null;
        }

        if (!!!dontClearTemplate) {
            context.ctrl.template = null;
        }
    };
    _ApiMemberViewDirective.validateApiMember = function(apiMember, oldValue, context) {
        if (!(apiMember instanceof ursa.model.ApiMember)) {
            _ApiMemberViewDirective.cleanupLastIncludeContent.call(this, context, false);
            return false;
        }
        else if ((apiMember !== oldValue) && (oldValue instanceof ursa.model.ApiMember)) {
            if (apiMember.id !== oldValue.id) {
                _ApiMemberViewDirective.cleanupLastIncludeContent.call(this, context, false);
            }
            else {
                return false;
            }
        }
        else if ((apiMember === oldValue) && (context.ctrl.template !== undefined)) {
            return false;
        }

        return true;
    };
    _ApiMemberViewDirective.onApiMemberChanged = function(scope, apiMember, oldValue, context) {
        if (!_ApiMemberViewDirective.validateApiMember.call(this, apiMember, oldValue, context)) {
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

        var viewRenderer = this._viewRendererProvider.createRenderer(apiMember);
        if (viewRenderer !== null) {
            context.ctrl.template = viewRenderer.render(newScope);
        }
        else {
            context.ctrl.template = "Error generating view for api member.";
        }

        var that = this;
        var clone = context.transclude(newScope, function(clone) {
            _ApiMemberViewDirective.cleanupLastIncludeContent.call(that, context, true);
            that._animate.enter(clone, null, context.element).then(function () { _ApiMemberViewDirective.onAnimationComplete.call(that, scope, context); });
        });
        context.currentScope = newScope;
        context.currentElement = clone;
        context.currentScope.$emit("apiMemberViewGenerated", apiMember);
    };
    _ApiMemberViewDirective.onAnimationComplete = function(scope, context) {
        if ((angular.isDefined(context.autoScrollExp)) && ((!context.autoScrollExp) || (scope.$eval(context.autoScrollExp)))) {
            this._anchorScroll();
        }
    }
    _ApiMemberViewDirective.linkInternal = function(scope, $element, $attr, ctrl, $transclude, autoScrollExp) {
        var context = {
            element: $element,
            currentScope: null,
            previousElement: null,
            currentElement: null,
            ctrl: ctrl,
            autoScrollExp: autoScrollExp,
            transclude: $transclude
        };
        Object.defineProperty(scope, "_ursaApiMemberView", { enumerable: false, configurable: false, value: true, writable: false });
        var that = this;
        scope.$watch("apiMember", function(apiMember, oldValue) { _ApiMemberViewDirective.onApiMemberChanged.call(that, scope, apiMember, oldValue, context); });
    };

    var ApiMemberViewPostDirective = namespace.ApiMemberViewPost = function(compile) {
        var that = this;
        this.restrict = "E";
        this.priority = -400;
        this.require = "ursaApiMemberView";
        this.link = function(scope, element, attr, ctrl) {
            _ApiMemberViewPostDirective.linkInternal.call(that, scope, element, attr, ctrl);
        };
        this._compile = compile;
    };
    ApiMemberViewPostDirective.prototype.restrict = null;
    ApiMemberViewPostDirective.prototype.priority = 0;
    ApiMemberViewPostDirective.prototype.require = null;
    ApiMemberViewPostDirective.prototype.link = function() {};
    Object.defineProperty(ApiMemberViewPostDirective.prototype, "_compiler", { enumerable: false, configurable: false, writable: true, value: null });
    var _ApiMemberViewPostDirective = {};
    _ApiMemberViewPostDirective.linkInternal = function(scope, element, attr, ctrl) {
        if (ctrl.template !== null) {
            element.html(ctrl.template);
            this._compile(element.contents())(scope);
        }
    };

    var module;
    try {
        module = angular.module("ursa");
    }
    catch (exception) {
        module = angular.module("ursa", []);
    }

    module.
    directive("ursaApiMemberView", ["$parse", "$animate", "$anchorScroll", "viewRendererProvider", function($parse, $animate, $anchorScroll, viewRendererProvider) {
        return new ApiMemberViewDirective($parse, $animate, $anchorScroll, viewRendererProvider);
    }]).
    directive("ursaApiMemberView", ["$compile", function($compile) { return new ApiMemberViewPostDirective($compile); }]);
}(namespace("ursa.angular")));