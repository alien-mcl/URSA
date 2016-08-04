/*globals namespace, joice */
(function(namespace) {
    "use strict";

    /**
     * Represents an HTTP response.
     * @memberof ursa.web
     * @name HttpResponse
     * @public
     * @class
     * @param {ursa.web.HttpRequest} request HTTP request.
     * @param {string} url Source Url of the response.
     * @param {number} status Status of the response.
     * @param {string} statusText Text of the status of the response.
     * @param {object} headers Optional response headers.
     * @param {object} data Body of the response.
     */
    var HttpResponse = namespace.HttpResponse = function(request, status, statusText, headers, data) {
        Function.requiresArgument("request", request, ursa.web.HttpRequest);
        Function.requiresArgument("status", status, "number");
        Function.requiresArgument("statusText", statusText, "string");
        Function.requiresOptionalArgument("headers", headers, Object);
        if (!url.match(/^http[s]?:\/\//)) {
            throw new joice.ArgumentOutOfRangeException("url");
        }

        this.request = request;
        this.url = request.url;
        this.method = request.method;
        this.status = status;
        this.statusText = statusText;
        this.headers = headers || {};
        this.data = data || null;
    };

    /**
     * Gets a request object.
     * @memberof ursa.web.HttpResponse
     * @public
     * @member {ursa.web.HttpRequest} request
     */
    HttpResponse.prototype.request = null;

    /**
     * Gets a response url.
     * @memberof ursa.web.HttpResponse
     * @public
     * @member {string} url
     */
    Object.defineProperty(HttpResponse.prototype, "url", { enumerable: true, configurable: false, get: function() { return this.request.url; } });

    /**
     * Gets a response method.
     * @memberof ursa.web.HttpResponse
     * @public
     * @member {string} method
     */
    Object.defineProperty(HttpResponse.prototype, "method", { enumerable: true, configurable: false, get: function() { return this.request.method; } });

    /**
     * Gets a response status.
     * @memberof ursa.web.HttpResponse
     * @public
     * @member {number} status
     */
    HttpResponse.prototype.status = 0;

    /**
     * Gets a response status text.
     * @memberof ursa.web.HttpResponse
     * @public
     * @member {string} statusText
     */
    HttpResponse.prototype.statusText = null;

    /**
     * Gets a response headers.
     * @memberof ursa.web.HttpResponse
     * @public
     * @member {object} headers
     */
    HttpResponse.prototype.headers = null;

    /**
     * Gets a response body.
     * @memberof ursa.web.HttpResponse
     * @public
     * @member {object} data
     */
    HttpResponse.prototype.data = null;

    HttpResponse.prototype.toString = function() { return (this.data ? this.data : String.format("Content-Type: {0}", this.headers["Content-Type"])); };

    HttpResponse.toString = function() { return "ursa.web.HttpResponse"; };
}(namespace("ursa.web")));