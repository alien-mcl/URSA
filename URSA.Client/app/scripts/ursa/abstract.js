/*globals xsd, ursa, guid, odata, namespace */
(function(namespace) {
    "use strict";

    var IPromise = namespace.IPromise = function() {
        throw new joice.InvalidOperationException("Cannot instantiate interface ursa.IPromise.");
    };
    IPromise.then = function() {};
    IPromise.catch = function() {};
    IPromise.toString = function() { return "ursa.IPromise"; };

    var IDeferred = namespace.IDeferred = function() {
        throw new joice.InvalidOperationException("Cannot instantiate interface ursa.IDeferred.");
    };
    IDeferred.promise = null;
    IDeferred.resolve = function() {};
    IDeferred.reject = function() {};
    IDeferred.toString = function() { return "ursa.IPromise"; };

    var IPromiseProvider = namespace.IPromiseProvider = function() {
        throw new joice.InvalidOperationException("Cannot instantiate interface ursa.IPromiseProvider.");
    };
    IPromiseProvider.when = function() {};
    IPromiseProvider.defer = function() {};
    IPromiseProvider.toString = function() { return "ursa.IPromiseProvider"; };

    var Encoder = namespace.Encoder = function() {};
    Encoder.prototype.encode = function(input) {
        Function.requiresArgument("input", input, "string");
        return null;
    };
    Encoder.prototype.encode = function(input) {
        Function.requiresArgument("input", input, "string");
        return null;
    };
    Encoder.toString = function() { return "ursa.Encoder"; };

    var keyStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
    var Base64Encoder = (namespace.Base64Encoder = function() {})[":"](Encoder);
    Base64Encoder.prototype.encode = function(input) {
        Encoder.prototype.encode.apply(this, arguments);
        var output = "";
        var chr1, chr2, chr3;
        var enc1, enc2, enc3, enc4;
        var i = 0;
        do {
            chr1 = input.charCodeAt(i++);
            chr2 = input.charCodeAt(i++);
            chr3 = input.charCodeAt(i++);
            enc1 = chr1 >> 2;
            enc2 = ((chr1 & 3) << 4) | (chr2 >> 4);
            enc3 = ((chr2 & 15) << 2) | (chr3 >> 6);
            enc4 = chr3 & 63;

            if (isNaN(chr2)) {
                enc3 = enc4 = 64;
            }
            else if (isNaN(chr3)) {
                enc4 = 64;
            }

            output = output + keyStr.charAt(enc1) + keyStr.charAt(enc2) + keyStr.charAt(enc3) + keyStr.charAt(enc4);
        }
        while (i < input.length);
        return output;
    };
    Base64Encoder.prototype.decode = function(input) {
        Encoder.prototype.decode.apply(this, arguments);
        if (/[^A-Za-z0-9\+\/\=]/g.exec(input)) {
            throw new joice.ArgumentOutOfRangeException("input");
        }

        input = input.replace(/[^A-Za-z0-9\+\/\=]/g, "");
        var output = "";
        var chr1, chr2, chr3;
        var enc1, enc2, enc3, enc4;
        var i = 0;
        do {
            enc1 = keyStr.indexOf(input.charAt(i++));
            enc2 = keyStr.indexOf(input.charAt(i++));
            enc3 = keyStr.indexOf(input.charAt(i++));
            enc4 = keyStr.indexOf(input.charAt(i++));
            chr1 = (enc1 << 2) | (enc2 >> 4);
            chr2 = ((enc2 & 15) << 4) | (enc3 >> 2);
            chr3 = ((enc3 & 3) << 6) | enc4;
            output = output + String.fromCharCode(chr1);

            if (enc3 !== 64) {
                output = output + String.fromCharCode(chr2);
            }

            if (enc4 !== 64) {
                output = output + String.fromCharCode(chr3);
            }
        }
        while (i < input.length);
        return output;
    };
    Base64Encoder.toString = function() { return "ursa.Base64Encoder"; };
}(namespace("ursa")));
(function(namespace) {
    "use strict";

    var HttpRequest = namespace.HttpRequest = function(method, url, headers) {
        Function.requiresArgument("method", method, "string");
        Function.requiresArgument("url", url, "string");
        Function.requiresOptionalArgument("headers", headers, Object);
        if (!url.match(/^http[s]?:\/\//)) {
            throw new joice.ArgumentOutOfRangeException("url");
        }

        this.url = url;
        this.method = method;
        this.headers = headers || {};
    };
    HttpRequest.prototype.url = null;
    HttpRequest.prototype.method = null;
    HttpRequest.prototype.headers = null;
    HttpRequest.prototype.toString = function() { return String.format("{0} {1}", this.method, this.url); };
    HttpRequest.toString = function() { return "ursa.web.HttpRequest"; };

    var HttpResponse = namespace.HttpResponse = function(method, url, status, statusText, headers, data) {
        Function.requiresArgument("method", method, "string");
        Function.requiresArgument("url", url, "string");
        Function.requiresArgument("status", status, "number");
        Function.requiresArgument("statusText", statusText, "string");
        Function.requiresOptionalArgument("headers", headers, Object);
        if (!url.match(/^http[s]?:\/\//)) {
            throw new joice.ArgumentOutOfRangeException("url");
        }

        this.url = url;
        this.method = method;
        this.status = status;
        this.statusText = statusText;
        this.headers = headers || {};
        this.data = data || null;
    };
    HttpResponse.prototype.url = null;
    HttpResponse.prototype.method = null;
    HttpResponse.prototype.status = 0;
    HttpResponse.prototype.statusText = null;
    HttpResponse.prototype.headers = null;
    HttpResponse.prototype.data = null;
    HttpResponse.prototype.toString = function() { return (this.data ? this.data : String.format("Content-Type: {0}", this.headers["Content-Type"])); };
    HttpResponse.toString = function() { return "ursa.web.HttpResponse"; }

    var HttpService = namespace.HttpService = function() { };
    HttpService.prototype.sendRequest = function(request) {
        Function.requiresArgument("request", request, HttpRequest);
        return null;
    };
    HttpService.toString = function() { return "ursa.web.HttpService"; }

    var AuthenticationProvider = namespace.AuthenticationProvider = function(base64Encoder, promiseProvider) {
        Function.requiresArgument("base64Encoder", base64Encoder, ursa.Base64Encoder);
        Function.requiresArgument("promiseProvider", promiseProvider, ursa.IPromiseProvider);
        this._base64Encoder = base64Encoder;
        this._promiseProvider = promiseProvider;
    };
    Object.defineProperty(AuthenticationProvider.prototype, "_base64Encoder", { enumerable: false, configurable: false, writable: true, value: null });
    Object.defineProperty(AuthenticationProvider.prototype, "_promiseProvider", { enumerable: false, configurable: false, writable: true, value: null });
    AuthenticationProvider.prototype.authenticate = function(scheme, userName, password) {
        switch (scheme.toLowerCase()) {
            case "basic":
                return this.basicAuthentication(userName, password);
            default:
                throw new joice.InvalidOperationException(String.format("Unsupported chellenge scheme '{0}'.", scheme));
        }
    };
    AuthenticationProvider.prototype.basicAuthentication = function(userName, password) {
        var that = this;
        return this._promiseProvider.when("Basic " + that._base64Encoder.encode(userName + ":" + password));
    };
    AuthenticationProvider.prototype.use = function(authorization) {
        Function.requiresArgument("authorization", authorization, "string");
    };
    AuthenticationProvider.prototype.reset = function() {};
    AuthenticationProvider.toString = function() { return "ursa.web.AuthenticationProvider"; };
}(namespace("ursa.web")));
(function(namespace) {
    "use strict";

    var IViewScope = namespace.IViewScope = function() {
        throw new joice.InvalidOperationException("Cannot instantiate interface ursa.view.IViewScope.");
    };
    IViewScope.parentScope = null;
    IViewScope.rootScope = null;
    IViewScope.updateView = function() {};
    IViewScope.onEvent = function(eventName, eventHandler) {
        Function.requiresArgument("eventName", eventName, "string");
        Function.requiresArgument("eventHandler", eventHandler, Function);
    };
    IViewScope.emitEvent = function(eventName) {
        Function.requiresArgument("eventName", eventName, "string");
    };
    IViewScope.broadcastEvent = function(eventName) {
        Function.requiresArgument("eventName", eventName, "string");
    };
    IViewScope.toString = function() { return "ursa.web.IViewScope"; }
}(namespace("ursa.view")));