/*globals joice, ursa */
(function() {
    "use strict";

    var container = new joice.Container();
    container.register(joice.Component.for(ursa.model.JsonLdProcessor).implementedBy(ursa.model.JsonLdProcessor).lifestyleSingleton());
    container.register(joice.Component.for(ursa.Base64Encoder).implementedBy(ursa.Base64Encoder).lifestyleSingleton());
    container.register(joice.Classes.implementing(ursa.model.FilterExpressionProvider).lifestyleSingleton());
    container.register(joice.Component.for(ursa.model.FilterProvider).implementedBy(ursa.model.FilterProvider).lifestyleSingleton());
    container.register(joice.Classes.implementing(ursa.view.ViewRenderer));
    container.register(joice.Component.for(ursa.view.ViewRendererProvider).implementedBy(ursa.view.ViewRendererProvider).lifestyleSingleton());
    container.register(joice.Component.for(ursa.model.ApiDocumentationProvider).implementedBy(ursa.model.ApiDocumentationProvider).lifestyleSingleton());
    var module;
    try {
        module = angular.module("ursa");
    }
    catch (exception) {
        module = angular.module("ursa", []);
    }

    var injector = angular.injector(["ng", "ursa"]);
    module.
    config(function($httpProvider) {
        $httpProvider.defaults.headers.common["X-Requested-With"] = "XMLHttpRequest";
        $httpProvider.interceptors.push("authenticationInterceptor");
        container.register(joice.Component.for(ursa.IPromiseProvider).named("ursa.angular.AngularPromiseProvider").usingFactoryMethod(function() {
            var result;
            injector.invoke(["$q", function($q) { result = $q; }]);
            return result;
        }).lifestyleSingleton());
        container.register(joice.Component.for(ursa.web.HttpService).named("ursa.web.angular.AngularHttpService").usingFactoryMethod(function() {
            var httpService;
            var qService;
            injector.invoke(["$q", "$http", function($q, $http) { qService = $q; httpService = $http; }]);
            return new ursa.web.angular.AngularHttpService(qService, httpService);
        }).lifestyleSingleton());
        container.register(joice.Component.for(ursa.web.AuthenticationProvider).named("ursa.web.angular.AuthenticationProvider").usingFactoryMethod(function() {
            var httpService;
            injector.invoke(["$http", function($http) { httpService = $http; }]);
            return new ursa.web.angular.AngularAuthenticationProvider(container.resolve(ursa.Base64Encoder), container.resolve(ursa.IPromiseProvider), httpService);
        }).lifestyleSingleton());
    }).
    factory("authenticationInterceptor", function($q, $location, configuration) {
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
    }).
    factory("viewRendererProvider", function() { return container.resolve(ursa.view.ViewRendererProvider); }).
    factory("hydraApiDocumentation", function() { return container.resolve(ursa.model.ApiDocumentationProvider); });
}());