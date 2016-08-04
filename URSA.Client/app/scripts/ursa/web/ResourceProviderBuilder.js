/*globals ursa, namespace */
(function(namespace) {
    "use strict";

    /**
     * Provides resources providers.
     * @memberof ursa.web
     * @name ResourceProviderBuilder
     * @public
     * @class
     * @param http {ursa.web.HttpService} HTTP cummunication facility.
     */
    var ResourceProviderBuilder = namespace.ResourceProviderBuilder = function(httpService, filterProvider, promiseProvider) {
        Function.requires("httpService", httpService, ursa.web.HttpService);
        Function.requiresArgument("filterProvider", filterProvider, ursa.model.FilterProvider);
        Function.requiresArgument("promiseProvider", promiseProvider, ursa.IPromiseProvider);
        this.httpService = httpService;
        this.filterProvider = filterProvider;
        this.promiseProvider = promiseProvider;
    };
    
    /**
     * Gets all the resources.
     * @memberof ursa.web.ResourceProviderBuilder
     * @public
     * @method createFor
     * @param supportedClass {ursa.model.Type} Supported class for the resource provider.
     * @returns {ursa.web.ResourcesProvider} The resource provider.
     */
    ResourceProvider.prototype.createFor = function(supportedClass) {
        Function.requires("supportedClass", supportedClass, ursa.model.Type);
        return new ursa.web.ResourceProvider(supportedClass, this.httpService, this.filterProvider, this.promiseProvider);
    };

    Object.defineProperty(ResourceProvider.prototype, "httpService", { enumerable: false, configurable: false, writable: true, value: null });

    Object.defineProperty(ResourceProvider.prototype, "filterProvider", { enumerable: false, configurable: false, writable: true, value: null });

    Object.defineProperty(ResourceProvider.prototype, "promiseProvider", { enumerable: false, configurable: false, writable: true, value: null });

    ResourceProviderBuilder.toString = function () { return "ursa.web.ResourceProviderBuilder"; };
}(namespace("ursa.web")));