/*globals xsd, ursa, guid, odata, namespace */
(function(namespace) {
    "use strict";

    var AngularHttpService = (namespace.AngularHttpService = function($q, $http) {
        Function.requiresArgument("$q", $q);
        Function.requiresArgument("$http", $http);
        if (typeof($q.defer) !== "function") {
            throw new joice.ArgumentOutOfRangeException("$q");
        }

        if (typeof($http.get) !== "function") {
            throw new joice.ArgumentOutOfRangeException("$http");
        }

        this._q = $q;
        this._http = $http;
    })[":"](ursa.web.HttpService);
    AngularHttpService.prototype.sendRequest = function(request) {
        ursa.web.HttpService.prototype.sendRequest.apply(this, arguments);
        var that = this;
        var deferred = this._q.defer();
        this._http(request).
            then(function(result) { deferred.resolve(_AngularHttpService.processResponse.call(that, request, result)); }).
            catch(function(result) { deferred.reject(_AngularHttpService.processResponse.call(that, request, result)); });
        return deferred.promise;
    };
    Object.defineProperty(AngularHttpService.prototype, "_http", { enumerable: false, configurable: false, writable: true, value: null });
    Object.defineProperty(AngularHttpService.prototype, "_q", { enumerable: false, configurable: false, writable: true, value: null });
    AngularHttpService.toString = function() { return "ursa.web.AngularHttpService"; };
    var _AngularHttpService = {};
    _AngularHttpService.processResponse = function(request, response) {
        var that = this;
        var result = new ursa.web.HttpResponse(request.method, request.url, response.status, response.statusText, {}, response.data);
        Object.defineProperty(result, "headers", { get: function() { return _AngularHttpService.processHeaders.call(that, response.headers()); } });
        return result;
    };
    _AngularHttpService.processHeaders = function(headers) {
        for (var property in headers) {
            var header = property.replace("www", "WWW").replace(/(^[a-z])|(-[a-z])/g, function(match) {
                return match.toUpperCase();
            });
            headers[header] = headers[property];
        }

        return headers;
    };

    var AngularAuthenticationProvider = (namespace.AngularAuthenticationProvider = function(base64Encoder, promiseProvider, $http) {
        ursa.web.AuthenticationProvider.prototype.constructor.apply(this, arguments);
        if (typeof($http.get) !== "function") {
            throw new joice.ArgumentOutOfRangeException("$http");
        }

        this._http = $http;
    })[":"](ursa.web.AuthenticationProvider);
    AngularAuthenticationProvider.prototype.use = function(authorization) {
        ursa.web.AuthenticationProvider.prototype.use.apply(this, arguments);
        this._http.defaults.headers.common.Authorization = authorization;
    };
    AngularAuthenticationProvider.prototype.reset = function() {
        ursa.web.AuthenticationProvider.prototype.reset.apply(this, arguments);
        delete this._http.defaults.headers.common.Authorization;
    };
    AngularAuthenticationProvider.toString = function() { return "ursa.web.angular.AngularAuthenticationProvider"; };
    Object.defineProperty(AngularAuthenticationProvider.prototype, "_http", { enumerable: false, configurable: false, writable: true, value: null });
}(namespace("ursa.web.angular")));
(function(namespace) {
    "use strict";

    var AngularViewScope = namespace.AngularViewScope = {};
    AngularViewScope.createInstance = function($scope) {
        if (($scope === null) || ($scope.rootScope) && ($scope.parentScope) && ($scope.onEvent) && ($scope.emitEvent) && ($scope.broadcastEvent)) {
            return $scope;
        }

        $scope.rootScope = ($scope.$root === $scope ? this : AngularViewScope.createInstance($scope.$root));
        $scope.parentScope = ($scope.$parent === $scope.$root ? $scope.rootScope : AngularViewScope.createInstance($scope.$parent));
        if (typeof($scope.updateView) !== "function") {
            $scope.updateView = function() { if (!$scope.$root.$$phase) { $scope.$root.$apply(); } };
        }

        if (typeof($scope.onEvent) !== "function") {
            $scope.onEvent = function() { return $scope.$on.apply($scope, arguments); };
        }

        if (typeof($scope.emitEvent) !== "function") {
            $scope.emitEvent = function() { return $scope.$emit.apply($scope, arguments); };
        }

        if (typeof($scope.broadcastEvent) !== "function") {
            $scope.broadcastEvent = function() { return $scope.$broadcast.apply($scope, arguments); };
        }

        return $scope;
    };
    AngularViewScope.toString = function() { return "ursa.web.AngularViewScope"; };
}(namespace("ursa.view.angular")));
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