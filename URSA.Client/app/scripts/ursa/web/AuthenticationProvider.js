/*globals ursa, namespace, joice */
(function(namespace) {
    "use strict";

    /**
     * Provides an HTTP authentication abstract facility.
     * @memberof ursa.web
     * @name AuthenticationProvider
     * @public
     * @abstract
     * @class
     * @param {ursa.Base64Encoder} base64Encoder Base64 encoding facility.
     * @param {ursa.IPromiseProvider} promiseProvider The promise provider.
     */
    var AuthenticationProvider = namespace.AuthenticationProvider = function(base64Encoder, promiseProvider) {
        Function.requiresArgument("base64Encoder", base64Encoder, ursa.Base64Encoder);
        Function.requiresArgument("promiseProvider", promiseProvider, ursa.IPromiseProvider);
        this.base64Encoder = base64Encoder;
        this.promiseProvider = promiseProvider;
    };

    Object.defineProperty(AuthenticationProvider.prototype, "base64Encoder", { enumerable: false, configurable: false, writable: true, value: null });
    Object.defineProperty(AuthenticationProvider.prototype, "promiseProvider", { enumerable: false, configurable: false, writable: true, value: null });

    /**
     * Authenticates user withing a given scheme.
     * @memberof ursa.web.AuthenticationProvider
     * @public
     * @method authenticate
     * @param {string} scheme Authentication scheme.
     * @param {string} userName User name.
     * @param {string} password The password.
     * @returns {ursa.IPromise<string>} Deferred basic authorization token string.
     */
    AuthenticationProvider.prototype.authenticate = function(scheme, userName, password) {
        switch (scheme.toLowerCase()) {
            case "basic":
                return this.basicAuthentication(userName, password);
            default:
                throw new joice.InvalidOperationException(String.format("Unsupported chellenge scheme '{0}'.", scheme));
        }
    };

    /**
     * Authenticates user withing a basic scheme.
     * @memberof ursa.web.AuthenticationProvider
     * @public
     * @method basicAuthentication
     * @param {string} userName User name.
     * @param {string} password The password.
     * @returns {ursa.IPromise<string>} Deferred basic authorization token string.
     */
    AuthenticationProvider.prototype.basicAuthentication = function(userName, password) {
        var that = this;
        return this.promiseProvider.when("Basic " + that.base64Encoder.encode(userName + ":" + password));
    };

    /**
     * Sets the authentication provider to use a given authorization token for further requests.
     * @memberof ursa.web.AuthenticationProvider
     * @public
     * @abstract
     * @method use
     * @param {string} authorization Authorization token string.
     */
    AuthenticationProvider.prototype.use = function(authorization) {
        Function.requiresArgument("authorization", authorization, "string");
    };

    /**
     * Resets the provider from all earlier used authorization tokens.
     * @memberof ursa.web.AuthenticationProvider
     * @public
     * @abstract
     * @method reset
     */
    AuthenticationProvider.prototype.reset = function() {};
   
    AuthenticationProvider.toString = function() { return "ursa.web.AuthenticationProvider"; };
}(namespace("ursa.web")));