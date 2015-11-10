/*globals jsonld */
(function() {
    "use strict";

    var invalidArgumentPassed = "Invalid {0} passed.";

    var JsonLdProcessor = function($q) {
        if (($q === undefined) || ($q === null)) {
            throw new Error(invalidArgumentPassed.replace("{0}", "$q"));
        }

        this.$q = $q;
    };
    JsonLdProcessor.prototype.constructor = JsonLdProcessor;
    JsonLdProcessor.prototype.$q = null;
    JsonLdProcessor.prototype.expand = function(graph) {
        var deferred = this.$q.defer();
        jsonld.expand(graph, function(error, expanded) {
            if (error) {
                deferred.reject(error);
            }

            deferred.resolve(expanded);
        });
        return deferred.promise;
    };

    angular.module("jsonld", []).
    factory("jsonld", ["$q", function($q) {
        return new JsonLdProcessor($q);
    }]);
}());