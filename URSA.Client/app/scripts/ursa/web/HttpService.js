/*globals ursa, namespace */
(function(namespace) {
    "use strict";

    /**
     * Provides an HTTP communication abstraction layer.
     * @memberof ursa.web
     * @name HttpService
     * @public
     * @abstract
     * @class
     */
    var HttpService = namespace.HttpService = function() {};

    /**
     * Sends a request.
     * @memberof ursa.web.HttpService
     * @public
     * @abstract
     * @method sendRequest
     * @param {ursa.web.HttpRequest} request The request.
     * @returns {ursa.IPromise<ursa.web.HttpResponse>} The response.
     */
    HttpService.prototype.sendRequest = function(request) {
        Function.requiresArgument("request", request, ursa.web.HttpRequest);
        return null;
    };

    HttpService.toString = function() { return "ursa.web.HttpService"; };
}(namespace("ursa.web")));