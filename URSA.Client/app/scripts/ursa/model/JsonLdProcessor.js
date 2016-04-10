/*globals namespace, ursa, jsonld */
(function(namespace) {
    "use strict";

    /**
     * Provides a JSON-LD processing facility.
     * @memberof ursa.model
     * @name JsonLdProcessor
     * @public
     * @class
     * @param {ursa.IPromiseProvider} promiseProvider Promise providing facility.
     */
    var JsonLdProcessor = namespace.JsonLdProcessor = function(promiseProvider) {
        Function.requiresArgument("promiseProvider", promiseProvider, ursa.IPromiseProvider);
        this._promiseProvider = promiseProvider;
    };

    Object.defineProperty(JsonLdProcessor.prototype, "_promiseProvider", { enumerable: false, configurable: false, writable: true, value: null });

    /**
     * Expands a given JSON-LD graph.
     * @memberof ursa.model.JsonLdProcessor
     * @instance
     * @public
     * @method expand
     * @param {object} graph JSON-LD RDF graph.
     * @returns {ursa.IPromise.<object>} Expanded graph.
     */
    JsonLdProcessor.prototype.expand = function(graph) {
        var deferred = this._promiseProvider.defer();
        jsonld.expand(graph, function(error, expanded) {
            if (error) {
                deferred.reject(error);
                return error;
            }

            deferred.resolve(expanded);
            return expanded;
        });
        return deferred.promise;
    };

    JsonLdProcessor.toString = function() { return "ursa.model.JsonLdProcessor"; };
}(namespace("ursa.model")));