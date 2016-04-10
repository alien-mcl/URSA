/*globals namespace, joice, ursa */
(function(namespace) {
    "use strict";

    var _ApiDocumentationProvider = {};
    
    /**
     * Provides an API documentation.
     * @memberof ursa.model
     * @name ApiDocumentationProvider
     * @public
     * @class
     * @param {ursa.model.JsonLdProcessor} jsonLdProcessor JSON-LD processor.
     * @param {ursa.web.HttpService} httpService HTTP connection infrastructure.
     * @param {ursa.IPromiseProvider} promiseProvider Promise providing facility.
     */
    var ApiDocumentationProvider = namespace.ApiDocumentationProvider = function(jsonLdProcessor, httpService, promiseProvider) {
        Function.requiresArgument("jsonLdProcessor", jsonLdProcessor, ursa.model.JsonLdProcessor);
        Function.requiresArgument("httpService", httpService, ursa.web.HttpService);
        Function.requiresArgument("promiseProvider", promiseProvider, ursa.IPromiseProvider);
        this._jsonLdProcessor = jsonLdProcessor;
        this._httpService = httpService;
        this._promiseProvider = promiseProvider;
    };

    Object.defineProperty(ApiDocumentationProvider.prototype, "_jsonLdProcessor", { enumerable: false, configurable: false, writable: true, value: null });
    Object.defineProperty(ApiDocumentationProvider.prototype, "_httpService", { enumerable: false, configurable: false, writable: true, value: null });
    Object.defineProperty(ApiDocumentationProvider.prototype, "_promiseProvider", { enumerable: false, configurable: false, writable: true, value: null });
    
    /**
     * Loads an API documentation for a given entry point.
     * @memberof ursa.model.ApiDocumentationProvider
     * @instance
     * @public
     * @method load
     * @param {string} entryPoint API entry point.
     * @param {string} [method] HTTP method to be used, which is OPTIONS by default.
     * @returns {ursa.IPromise.<ursa.model.ApiDocumentation>} API documentation.
     */
    ApiDocumentationProvider.prototype.load = function(entryPoint, method) {
        entryPoint = (entryPoint ? entryPoint.toString() : window.location.href);
        if ((entryPoint.length === 0) || (entryPoint.match(/^http[s]?:\/\//i) === null)) {
            throw new joice.ArgumentOutOfRangeExceptio("entryPoint");
        }

        method = (method || "OPTIONS").toString();
        var that = this;
        var deferred = this._promiseProvider.defer();
        var request = new ursa.web.HttpRequest(method, entryPoint, { "Accept": ["application/ld+json", "application/json"] });
        this._httpService.sendRequest(request).
            then(function(response) { return _ApiDocumentationProvider.onLoad.call(that, deferred, response); }).
            catch(function(response) { return _ApiDocumentationProvider.onError.call(that, deferred, response); });
        return deferred.promise;
    };

    ApiDocumentationProvider.toString = function() { return "ursa.model.ApiDocumentationProvider"; };

    _ApiDocumentationProvider.onExpanded = function(deferred, expanded) {
        var result = new ursa.model.ApiDocumentation(expanded);
        deferred.resolve(result);
        return result;
    };

    _ApiDocumentationProvider.onExpansionError = function(deferred, error) {
        deferred.reject(error);
        return error;
    };

    _ApiDocumentationProvider.onError = function(deferred, response) {
        deferred.reject(response.data);
        return response;
    };

    _ApiDocumentationProvider.onLoad = function(deferred, response) {
        var that = this;
        return this._jsonLdProcessor.expand(response.data).
            then(function(expanded) { return _ApiDocumentationProvider.onExpanded.call(that, deferred, expanded); }).
            catch(function(error) { return _ApiDocumentationProvider.onExpansionError.call(that, deferred, error); });
    };
}(namespace("ursa.model")));