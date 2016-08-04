/*globals namespace, joice, ursa */
(function(namespace) {
    "use strict";

    var _AngularHttpService = {};

    /**
     * Provides an AngularJS based HTTP communication abstraction layer.
     * @memberof ursa.web.angular
     * @name AngularHttpService
     * @public
     * @class
     * @extends {ursa.web.HttpService}
     * @param {angular.IQService} $q The Q service.
     * @param {angular.IHttpService} $http The HTTP service.
     */
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

    Object.defineProperty(AngularHttpService.prototype, "_http", { enumerable: false, configurable: false, writable: true, value: null });
    Object.defineProperty(AngularHttpService.prototype, "_q", { enumerable: false, configurable: false, writable: true, value: null });

    AngularHttpService.prototype.sendRequest = function(request) {
        ursa.web.HttpService.prototype.sendRequest.apply(this, arguments);
        var that = this;
        var deferred = this._q.defer();
        this._http(request).
            then(function(result) { return deferred.resolve(_AngularHttpService.processResponse.call(that, request, result)); }).
            catch(function(result) { return deferred.reject(_AngularHttpService.processResponse.call(that, request, result)); });
        return deferred.promise;
    };

    AngularHttpService.toString = function() { return "ursa.web.angular.AngularHttpService"; };
    
    _AngularHttpService.processResponse = function(request, response) {
        var that = this;
        var result = new ursa.web.HttpResponse(request, response.status, response.statusText, {}, response.data);
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
}(namespace("ursa.web.angular")));