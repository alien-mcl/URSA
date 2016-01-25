/*globals jsonld, ursa */
(function() {
    "use strict";

    var invalidArgumentPassed = "Invalid {0} passed.";
    var rdfMediaTypes = "application/ld+json, application/json";

    var ApiDocumentationService = function(http, promise) {
        if ((http === undefined) || (http === null)) {
            throw new Error(invalidArgumentPassed.replace("{0}", "http"));
        }

        if ((promise === undefined) || (promise === null)) {
            throw new Error(invalidArgumentPassed.replace("{0}", "promise"));
        }

        this._http = http;
        this._promise = promise;
    };
    ApiDocumentationService.prototype.constructor = ApiDocumentationService;
    ApiDocumentationService.prototype.load = function(entryPoint) {
        if ((entryPoint === undefined) || (entryPoint === null)) {
            entryPoint = window.location.href;
        }

        if (entryPoint instanceof String) {
            entryPoint = entryPoint.toString();
        }

        if (typeof(entryPoint) !== "string") {
            throw new Error(invalidArgumentPassed.replace("{0}", "entryPoint"));
        }

        if ((entryPoint.length === 0) || (entryPoint.match(/^http[s]?:\/\//i) === null)) {
            throw new Error(invalidArgumentPassed.replace("{0}", "entryPoint"));
        }

        var deferred = this._promise.defer();
        var onExpanded = function(error, expanded) {
            if (error) {
                throw new Error(error);
            }

            deferred.resolve(new ursa.model.ApiDocumentation(expanded));
        };

        this._http({ method: "OPTIONS", url: entryPoint, headers: { "Accept": rdfMediaTypes } }).
            then(function(response) { jsonld.expand(response.data, onExpanded); return response; }).
            catch(function(response) { deferred.reject(response.data); return response; });
        return deferred.promise;
    };
    ApiDocumentationService.prototype._http = null;
    ApiDocumentationService.prototype._promise = null;

    var module;
    try {
        module = angular.module("ursa");
    }
    catch (exception) {
        module = angular.module("ursa", []);
    }

    module.
    factory("hydraApiDocumentation", ["$http", "$q", function hydraApiDocumentationFactory($http, $q) {
        return new ApiDocumentationService($http, $q);
    }]).
    factory("viewRendererProvider", ["$http", "jsonld", "authentication", function viewRendererProvider($http, jsonld, authentication) {
        return new ursa.view.ViewRendererProvider($http, jsonld, authentication);        
    }]);
}());