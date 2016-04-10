/*globals namespace, joice, ursa */
(function(namespace) {
    "use strict";

    /**
     * Provides an AngularJS based authentication provider.
     * @memberof ursa.web.angular
     * @name AngularAuthenticationProvider
     * @public
     * @class
     * @extends {ursa.web.AuthenticationProvider}
     * @param {ursa.Base64Encoder} base64Encoder The Base64 encoding facility.
     * @param {ursa.IPromiseProvider} promiseProvider The promise provider.
     * @param {angular.IHttpService} $http The HTTP service.
     */
    var AngularAuthenticationProvider = (namespace.AngularAuthenticationProvider = function(base64Encoder, promiseProvider, $http) {
        ursa.web.AuthenticationProvider.prototype.constructor.apply(this, arguments);
        if (typeof($http.get) !== "function") {
            throw new joice.ArgumentOutOfRangeException("$http");
        }

        this._http = $http;
    })[":"](ursa.web.AuthenticationProvider);

    Object.defineProperty(AngularAuthenticationProvider.prototype, "_http", { enumerable: false, configurable: false, writable: true, value: null });
    
    AngularAuthenticationProvider.prototype.use = function(authorization) {
        ursa.web.AuthenticationProvider.prototype.use.apply(this, arguments);
        this._http.defaults.headers.common.Authorization = authorization;
    };
    
    AngularAuthenticationProvider.prototype.reset = function() {
        ursa.web.AuthenticationProvider.prototype.reset.apply(this, arguments);
        delete this._http.defaults.headers.common.Authorization;
    };
    
    AngularAuthenticationProvider.toString = function() { return "ursa.web.angular.AngularAuthenticationProvider"; };
}(namespace("ursa.web.angular")));