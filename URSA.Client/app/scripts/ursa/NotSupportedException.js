/*globals ursa, namespace, joice */
(function(namespace) {
    "use strict";

    /**
     * Represents an exception when a operation is not supported.
     * @memberof ursa
     * @name NotSupportedException
     * @public
     * @class
     * @extends joice.Exception
     * @param {string} message Message of the exception.
     */
    var NotSupportedException = (namespace.NotSupportedException = function() {
        joice.Exception.prototype.constructor.apply(this, arguments);
    })[":"](joice.Exception);
    NotSupportedException.toString = function() { return "ursa.NotSupportedException"; }
}(namespace("ursa")));