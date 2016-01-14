/*globals application */
(function() {
    "use strict";
    application.
    factory("authenticationInterceptor", ["$q", "$location", "configuration", function($q, $location, configuration) {
        return {
            response: function(response) {
                if (response.status === ursa.model.HttpStatusCodes.Unauthorized) {
                    $q.reject(response);
                }

                return response;
            },
            responseError: function(response) {
                if (response.status === ursa.model.HttpStatusCodes.Unauthorized) {
                    configuration.challenge = ((response.headers("WWW-Authenticate")) || (response.headers("X-WWW-Authenticate"))).split(/[ ;]/g)[0].toLowerCase();
                }

                return $q.reject(response);
            }
        };
    }]).
    factory("authentication", ["$http", "$q", "$location", "configuration", "base64", function($http, $q, $location, configuration, base64) {
        return {
            basic: function(userName, password) {
                return $q.when("Basic " + base64.encode(userName + ":" + password));
            },
            use: function(authorization) {
                $http.defaults.headers.common.Authorization = authorization;
            },
            reset: function() {
                delete $http.defaults.headers.common.Authorization;
            }
        };
    }]).
    config(["$httpProvider", function($httpProvider) {
        $httpProvider.defaults.headers.common["X-Requested-With"] = "XMLHttpRequest";
        $httpProvider.interceptors.push("authenticationInterceptor");
    }]);
}());