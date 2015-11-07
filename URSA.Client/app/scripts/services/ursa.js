/*globals jsonld, ursa */
(function() {
    "use strict";

    var invalidArgumentPassed = "Invalid {0} passed.";
    var rdfMediaTypes = "application/ld+json, application/json";

    var ApiDocumentationService = function($http, $q) {
        if (($http === undefined) || ($http === null)) {
            throw new Error(invalidArgumentPassed.replace("{0}", "$http"));
        }

        if (($q === undefined) || ($q === null)) {
            throw new Error(invalidArgumentPassed.replace("{0}", "$q"));
        }

        this.$http = $http;
        this.$q = $q;
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

        var deferred = this.$q.defer();
        var onExpanded = function(error, expanded) {
            if (error) {
                throw new Error(error);
            }

            deferred.resolve(new ursa.model.ApiDocumentation(expanded));
        };

        this.$http({ method: "OPTIONS", url: entryPoint, headers: { "Accept": rdfMediaTypes } }).
            then(function(response) { jsonld.expand(response.data, onExpanded); }).
            catch(function(response) { deferred.reject(response.data); });
        return deferred.promise;
    };
    ApiDocumentationService.prototype.$http = null;
    ApiDocumentationService.prototype.$q = null;

    var module;
    try {
        module = angular.module("ursa");
    }
    catch (exception) {
        module = angular.module("ursa", []) ;
    }

    module.
    factory("hydraApiDocumentation", ["$http", "$q", function hydraApiDocumentationFactory($http, $q) {
        return new ApiDocumentationService($http, $q);
    }]);
}());