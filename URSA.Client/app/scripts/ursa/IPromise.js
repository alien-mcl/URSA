/*globals namespace, joice */
(function(namespace) {
    "use strict";

    /**
     * A deferred promise abstract.
     * @memberof ursa
     * @name IPromise
     * @public
     * @abstract
     * @class
     */
    var IPromise = namespace.IPromise = function() {
        throw new joice.InvalidOperationException("Cannot instantiate interface ursa.IPromise.");
    };

    /**
     * Attaches an another processing routine for sucessfuly resolved promise.
     * @memberof ursa.IPromise
     * @public
     * @abstract
     * @method then
     * @param {object} result Result of the promise.
     * @returns {object | ursa.IPromise.<object>} Either a result of the processing or another promise.
     */
    IPromise.then = function() {};

    /**
     * Attaches an another processing routine for a rejected promise.
     * @memberof ursa.IPromise
     * @public
     * @abstract
     * @method catch
     * @param {object} result Result of the rejected promise.
     * @returns {object | ursa.IPromise.<object>} Either a result of the processing or another promise.
     */
    IPromise.catch = function() {};

    IPromise.toString = function() { return "ursa.IPromise"; };
}(namespace("ursa")));