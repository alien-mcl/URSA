/*globals namespace, joice */
(function(namespace) {
    "use strict";

    /**
     * Provides an MVC view scope abstraction layer.
     * @memberof ursa.view
     * @name IViewScope
     * @public
     * @abstract
     * @class
     */
    var IViewScope = namespace.IViewScope = function() {
        throw new joice.InvalidOperationException("Cannot instantiate interface ursa.view.IViewScope.");
    };

    /**
     * Gets a parent scope.
     * @memberof ursa.view.IViewScope
     * @public
     * @abstract
     * @member {ursa.view.IViewScope} parentScope
     */
    IViewScope.parentScope = null;

    /**
     * Gets a root scope.
     * @memberof ursa.view.IViewScope
     * @public
     * @abstract
     * @member {ursa.view.IViewScope} rootScope
     */
    IViewScope.rootScope = null;

    /**
     * Forces the view engine to refresh.
     * @memberof ursa.view.IViewScope
     * @public
     * @abstract
     * @method updateView
     */
    IViewScope.updateView = function() {};

    /**
     * Adds an event handler.
     * @memberof ursa.view.IViewScope
     * @public
     * @abstract
     * @method onEvent
     * @param {string} eventName Name of the event to attach the handler to.
     * @param {function} eventHandler The event handler.
     * @returns {function} Event removal delegate.
     */
    IViewScope.onEvent = function(eventName, eventHandler) {
        Function.requiresArgument("eventName", eventName, "string");
        Function.requiresArgument("eventHandler", eventHandler, Function);
    };

    /**
     * Raises an event upward the scope hierarchy (bubbling).
     * @memberof ursa.view.IViewScope
     * @public
     * @abstract
     * @method emitEvent
     * @param {string} eventName Name of the event to attach the handler to.
     */
    IViewScope.emitEvent = function(eventName) {
        Function.requiresArgument("eventName", eventName, "string");
    };

    /**
     * Raises an event downward the scope hierarchy (tunelling).
     * @memberof ursa.view.IViewScope
     * @public
     * @abstract
     * @method broadcastEvent
     * @param {string} eventName Name of the event to attach the handler to.
     */
    IViewScope.broadcastEvent = function(eventName) {
        Function.requiresArgument("eventName", eventName, "string");
    };

    IViewScope.toString = function() { return "ursa.web.IViewScope"; };
}(namespace("ursa.view")));