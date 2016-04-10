/*globals namespace, joice */
(function(namespace) {
    "use strict";

    /**
     * Promise provider.
     * @memberof ursa
     * @name IPromiseProvider
     * @public
     * @abstract
     * @class
     */
    var IPromiseProvider = namespace.IPromiseProvider = function () {
        throw new joice.InvalidOperationException("Cannot instantiate interface ursa.IPromiseProvider.");
    };

    /**
     * Wraps a constant value with a deferred facility.
     * @memberof ursa.IPromiseProvider
     * @public
     * @abstract
     * @method when
     * @param {object} result Result to be used as promise resolution.
     */
    IPromiseProvider.when = function() {};

    /**
     * Creates a promise.
     * @memberof ursa.IPromiseProvider
     * @public
     * @abstract
     * @method defer
     */
    IPromiseProvider.defer = function() {};

    IPromiseProvider.toString = function() { return "ursa.IPromiseProvider"; };
}(namespace("ursa")));