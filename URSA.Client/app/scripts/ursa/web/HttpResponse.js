/*globals namespace, joice */
(function(namespace) {
    "use strict";

    /**
     * Represents an HTTP response.
     * @memberof ursa.web
     * @name HttpResponse
     * @public
     * @class
     * @param {string} method An HTTP method.
     * @param {string} url Source Url of the response.
     * @param {number} status Status of the response.
     * @param {string} statusText Text of the status of the response.
     * @param {object} headers Optional response headers.
     * @param {object} data Body of the response.
     */
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

    /**
     * Gets a response url.
     * @memberof ursa.web.HttpResponse
     * @public
     * @member {string} url
     */
    HttpResponse.prototype.url = null;

    /**
     * Gets a response method.
     * @memberof ursa.web.HttpResponse
     * @public
     * @member {string} method
     */
    HttpResponse.prototype.method = null;

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