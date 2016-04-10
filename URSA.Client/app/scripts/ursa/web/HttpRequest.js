/*globals namespace, joice */
(function(namespace) {
    "use strict";

    /**
     * Represents an HTTP request.
     * @memberof ursa.web
     * @name HttpRequest
     * @public
     * @class
     * @param {string} method An HTTP method.
     * @param {string} url Target Url of the request.
     * @param {object} headers Optional request headers.
     */
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

    /**
     * Gets a request url.
     * @memberof ursa.web.HttpRequest
     * @public
     * @member {string} url
     */
    HttpRequest.prototype.url = null;

    /**
     * Gets a request method.
     * @memberof ursa.web.HttpRequest
     * @public
     * @member {string} method
     */
    HttpRequest.prototype.method = null;

    /**
     * Gets a request headers.
     * @memberof ursa.web.HttpRequest
     * @public
     * @member {object} headers
     */
    HttpRequest.prototype.headers = null;

    HttpRequest.prototype.toString = function() { return String.format("{0} {1}", this.method, this.url); };

    HttpRequest.toString = function() { return "ursa.web.HttpRequest"; };
}(namespace("ursa.web")));