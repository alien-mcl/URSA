/*globals namespace, joice */
(function(namespace) {
    "use strict";

    /**
     * Promise providing facility.
     * @memberof ursa
     * @name IDeferred
     * @public
     * @abstract
     * @class
     */
    var IDeferred = namespace.IDeferred = function() {
        throw new joice.InvalidOperationException("Cannot instantiate interface ursa.IDeferred.");
    };

    /**
     * Gets a deferred promise.
     * @memberof ursa.IDeferred
     * @public
     * @abstract
     * @member {ursa.IPromise<object>} promise
     */
    IDeferred.promise = null;

    /**
     * Resolves a promise.
     * @memberof ursa.IDeferred
     * @public
     * @abstract
     * @method resolve
     * @param {object} result Result to be used as promise resolution.
     */
    IDeferred.resolve = function() {};

    /**
     * Rejects a promise.
     * @memberof ursa.IDeferred
     * @public
     * @abstract
     * @method reject
     * @param {object} result Result to be used as promise resolution.
     */
    IDeferred.reject = function() {};

    IDeferred.toString = function() { return "ursa.IPromise"; };
}(namespace("ursa")));