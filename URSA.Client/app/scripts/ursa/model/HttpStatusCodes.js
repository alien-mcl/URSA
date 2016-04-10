/*globals namespace */
(function (namespace) {
    "use strict";

    /**
     * Provides an enumeration of HTTP status codes.
     * @memberof ursa.model
     * @name HttpStatusCodes
     * @public
     * @class
     */
    namespace.HttpStatusCodes = {
        /**
         * Continue indicates that the client can continue with its request.
         * @memberof ursa.model.HttpStatusCodes
         * @name Continue
         * @static
         * @public
         * @readonly
         * @member {number} Continue
         */
        Continue: 100,

        /**
         * SwitchingProtocols indicates that the protocol version or protocol is being changed.
         * @memberof ursa.model.HttpStatusCodes
         * @name SwitchingProtocols
         * @static
         * @public
         * @readonly
         * @member {number} SwitchingProtocols
         */
        SwitchingProtocols: 101,

        /**
         * OK indicates that the request succeeded and that the requested information is in the response. This is the most common status code to receive.
         * @memberof ursa.model.HttpStatusCodes
         * @name OK
         * @static
         * @public
         * @readonly
         * @member {number} OK
         */
        OK: 200,

        /**
         * Created indicates that the request resulted in a new resource created before the response was sent.
         * @memberof ursa.model.HttpStatusCodes
         * @name Created
         * @static
         * @public
         * @readonly
         * @member {number} Created
         */
        Created: 201,

        /**
         * Accepted indicates that the request has been accepted for further processing.
         * @memberof ursa.model.HttpStatusCodes
         * @name Accepted
         * @static
         * @public
         * @readonly
         * @member {number} Accepted
         */
        Accepted: 202,

        /**
         * NonAuthoritativeInformation indicates that the returned metainformation is from a cached copy instead of the origin server and therefore may be incorrect.
         * @memberof ursa.model.HttpStatusCodes
         * @name NonAuthoritativeInformation
         * @static
         * @public
         * @readonly
         * @member {number} NonAuthoritativeInformation
         */
        NonAuthoritativeInformation: 203,

        /**
         * NoContent indicates that the request has been successfully processed and that the response is intentionally blank.
         * @memberof ursa.model.HttpStatusCodes
         * @name NoContent
         * @static
         * @public
         * @readonly
         * @member {number} NoContent
         */
        NoContent: 204,

        /**
         * ResetContent indicates that the client should reset (not reload) the current resource.
         * @memberof ursa.model.HttpStatusCodes
         * @name ResetContent
         * @static
         * @public
         * @readonly
         * @member {number} ResetContent
         */
        ResetContent: 205,

        /**
         * PartialContent indicates that the response is a partial response as requested by a GET request that includes a byte range.
         * @memberof ursa.model.HttpStatusCodes
         * @name PartialContent
         * @static
         * @public
         * @readonly
         * @member {number} PartialContent
         */
        PartialContent: 206,

        /**
         * MultipleChoices indicates that the requested information has multiple representations. The default action is to treat this status as a redirect and follow the contents of the Location header associated with this response.
         * @memberof ursa.model.HttpStatusCodes
         * @name MultipleChoices
         * @static
         * @public
         * @readonly
         * @member {number} MultipleChoices
         */
        MultipleChoices: 300,

        /**
         * Ambiguous indicates that the requested information has multiple representations. The default action is to treat this status as a redirect and follow the contents of the Location header associated with this response.
         * @memberof ursa.model.HttpStatusCodes
         * @name Ambiguous
         * @static
         * @public
         * @readonly
         * @member {number} Ambiguous
         */
        Ambiguous: 300,

        /**
         * Moved indicates that the requested information has been moved to the URI specified in the Location header. The default action when this status is received is to follow the Location header associated with the response. When the original request method was POST, the redirected request will use the GET method.
         * @memberof ursa.model.HttpStatusCodes
         * @name Moved
         * @static
         * @public
         * @readonly
         * @member {number} Moved
         */
        Moved: 301,

        /**
         * MovedPermanently indicates that the requested information has been moved to the URI specified in the Location header. The default action when this status is received is to follow the Location header associated with the response.
         * @memberof ursa.model.HttpStatusCodes
         * @name MovedPermanently
         * @static
         * @public
         * @readonly
         * @member {number} MovedPermanently
         */
        MovedPermanently: 301,

        /**
         * Redirect indicates that the requested information is located at the URI specified in the Location header. The default action when this status is received is to follow the Location header associated with the response. When the original request method was POST, the redirected request will use the GET method.
         * @memberof ursa.model.HttpStatusCodes
         * @name Redirect
         * @static
         * @public
         * @readonly
         * @member {number} Redirect
         */
        Redirect: 302,

        /**
         * Found indicates that the requested information is located at the URI specified in the Location header. The default action when this status is received is to follow the Location header associated with the response. When the original request method was POST, the redirected request will use the GET method.
         * @memberof ursa.model.HttpStatusCodes
         * @name Found
         * @static
         * @public
         * @readonly
         * @member {number} Found
         */
        Found: 302,

        /**
         * SeeOther automatically redirects the client to the URI specified in the Location header as the result of a POST. The request to the resource specified by the Location header will be made with a GET.
         * @memberof ursa.model.HttpStatusCodes
         * @name SeeOther
         * @static
         * @public
         * @readonly
         * @member {number} SeeOther
         */
        SeeOther: 303,

        /**
         * RedirectMethod automatically redirects the client to the URI specified in the Location header as the result of a POST. The request to the resource specified by the Location header will be made with a GET.
         * @memberof ursa.model.HttpStatusCodes
         * @name RedirectMethod
         * @static
         * @public
         * @readonly
         * @member {number} RedirectMethod
         */
        RedirectMethod: 303,

        /**
         * NotModified indicates that the client's cached copy is up to date. The contents of the resource are not transferred.
         * @memberof ursa.model.HttpStatusCodes
         * @name NotModified
         * @static
         * @public
         * @readonly
         * @member {number} NotModified
         */
        NotModified: 304,

        /**
         * UseProxy indicates that the request should use the proxy server at the URI specified in the Location header.
         * @memberof ursa.model.HttpStatusCodes
         * @name UseProxy
         * @static
         * @public
         * @readonly
         * @member {number} UseProxy
         */
        UseProxy: 305,

        /**
         * Unused is a proposed extension to the HTTP/1.1 specification that is not fully specified.
         * @memberof ursa.model.HttpStatusCodes
         * @name Unused
         * @static
         * @public
         * @readonly
         * @member {number} Unused
         */
        Unused: 306,

        /**
         * RedirectKeepVerb indicates that the request information is located at the URI specified in the Location header. The default action when this status is received is to follow the Location header associated with the response. When the original request method was POST, the redirected request will also use the POST method.
         * @memberof ursa.model.HttpStatusCodes
         * @name RedirectKeepVerb
         * @static
         * @public
         * @readonly
         * @member {number} RedirectKeepVerb
         */
        RedirectKeepVerb: 307,

        /**
         * TemporaryRedirect indicates that the request information is located at the URI specified in the Location header. The default action when this status is received is to follow the Location header associated with the response. When the original request method was POST, the redirected request will also use the POST method.
         * @memberof ursa.model.HttpStatusCodes
         * @name TemporaryRedirect
         * @static
         * @public
         * @readonly
         * @member {number} TemporaryRedirect
         */
        TemporaryRedirect: 307,

        /**
         * BadRequest indicates that the request could not be understood by the server. BadRequest is sent when no other error is applicable, or if the exact error is unknown or does not have its own error code.
         * @memberof ursa.model.HttpStatusCodes
         * @name BadRequest
         * @static
         * @public
         * @readonly
         * @member {number} BadRequest
         */
        BadRequest: 400,

        /**
         * Unauthorized indicates that the requested resource requires authentication. The WWW-Authenticate header contains the details of how to perform the authentication.
         * @memberof ursa.model.HttpStatusCodes
         * @name Unauthorized
         * @static
         * @public
         * @readonly
         * @member {number} Unauthorized
         */
        Unauthorized: 401,

        /**
         * PaymentRequired is reserved for future use.
         * @memberof ursa.model.HttpStatusCodes
         * @name PaymentRequired
         * @static
         * @public
         * @readonly
         * @member {number} PaymentRequired
         */
        PaymentRequired: 402,

        /**
         * Forbidden indicates that the server refuses to fulfill the request.
         * @memberof ursa.model.HttpStatusCodes
         * @name Forbidden
         * @static
         * @public
         * @readonly
         * @member {number} Forbidden
         */
        Forbidden: 403,

        /**
         * NotFound indicates that the requested resource does not exist on the server.
         * @memberof ursa.model.HttpStatusCodes
         * @name NotFound
         * @static
         * @public
         * @readonly
         * @member {number} NotFound
         */
        NotFound: 404,

        /**
         * MethodNotAllowed indicates that the request method (POST or GET) is not allowed on the requested resource.
         * @memberof ursa.model.HttpStatusCodes
         * @name MethodNotAllowed
         * @static
         * @public
         * @readonly
         * @member {number} MethodNotAllowed
         */
        MethodNotAllowed: 405,

        /**
         * NotAcceptable indicates that the client has indicated with Accept headers that it will not accept any of the available representations of the resource.
         * @memberof ursa.model.HttpStatusCodes
         * @name NotAcceptable
         * @static
         * @public
         * @readonly
         * @member {number} NotAcceptable
         */
        NotAcceptable: 406,

        /**
         * ProxyAuthenticationRequired indicates that the requested proxy requires authentication. The Proxy-authenticate header contains the details of how to perform the authentication.
         * @memberof ursa.model.HttpStatusCodes
         * @name ProxyAuthenticationRequired
         * @static
         * @public
         * @readonly
         * @member {number} ProxyAuthenticationRequired
         */
        ProxyAuthenticationRequired: 407,

        /**
         * RequestTimeout indicates that the client did not send a request within the time the server was expecting the request.
         * @memberof ursa.model.HttpStatusCodes
         * @name RequestTimeout
         * @static
         * @public
         * @readonly
         * @member {number} RequestTimeout
         */
        RequestTimeout: 408,

        /**
         * Conflict indicates that the request could not be carried out because of a conflict on the server.
         * @memberof ursa.model.HttpStatusCodes
         * @name Conflict
         * @static
         * @public
         * @readonly
         * @member {number} Conflict
         */
        Conflict: 409,

        /**
         * Gone indicates that the requested resource is no longer available.
         * @memberof ursa.model.HttpStatusCodes
         * @name Gone
         * @static
         * @public
         * @readonly
         * @member {number} Gone
         */
        Gone: 410,

        /**
         * LengthRequired indicates that the required Content-length header is missing.
         * @memberof ursa.model.HttpStatusCodes
         * @name LengthRequired
         * @static
         * @public
         * @readonly
         * @member {number} LengthRequired
         */
        LengthRequired: 411,

        /**
         * PreconditionFailed indicates that a condition set for this request failed, and the request cannot be carried out. Conditions are set with conditional request headers like If-Match, If-None-Match, or If-Unmodified-Since.
         * @memberof ursa.model.HttpStatusCodes
         * @name PreconditionFailed
         * @static
         * @public
         * @readonly
         * @member {number} PreconditionFailed
         */
        PreconditionFailed: 412,

        /**
         * RequestEntityTooLarge indicates that the request is too large for the server to process.
         * @memberof ursa.model.HttpStatusCodes
         * @name RequestEntityTooLarge
         * @static
         * @public
         * @readonly
         * @member {number} RequestEntityTooLarge
         */
        RequestEntityTooLarge: 413,

        /**
         * RequestUriTooLong indicates that the URI is too long.
         * @memberof ursa.model.HttpStatusCodes
         * @name RequestUriTooLong
         * @static
         * @public
         * @readonly
         * @member {number} RequestUriTooLong
         */
        RequestUriTooLong: 414,

        /**
         * UnsupportedMediaType indicates that the request is an unsupported type.
         * @memberof ursa.model.HttpStatusCodes
         * @name UnsupportedMediaType
         * @static
         * @public
         * @readonly
         * @member {number} UnsupportedMediaType
         */
        UnsupportedMediaType: 415,

        /**
         * RequestedRangeNotSatisfiable indicates that the range of data requested from the resource cannot be returned, either because the beginning of the range is before the beginning of the resource, or the end of the range is after the end of the resource.
         * @memberof ursa.model.HttpStatusCodes
         * @name RequestedRangeNotSatisfiable
         * @static
         * @public
         * @readonly
         * @member {number} RequestedRangeNotSatisfiable
         */
        RequestedRangeNotSatisfiable: 416,

        /**
         * ExpectationFailed indicates that an expectation given in an Expect header could not be met by the server.
         * @memberof ursa.model.HttpStatusCodes
         * @name ExpectationFailed
         * @static
         * @public
         * @readonly
         * @member {number} ExpectationFailed
         */
        ExpectationFailed: 417,

        /**
         * UpgradeRequired indicates that the client should switch to a different protocol such as TLS/1.0.
         * @memberof ursa.model.HttpStatusCodes
         * @name UpgradeRequired
         * @static
         * @public
         * @readonly
         * @member {number} UpgradeRequired
         */
        UpgradeRequired: 426,

        /**
         * InternalServerError indicates that a generic error has occurred on the server.
         * @memberof ursa.model.HttpStatusCodes
         * @name InternalServerError
         * @static
         * @public
         * @readonly
         * @member {number} InternalServerError
         */
        InternalServerError: 500,

        /**
         * NotImplemented indicates that the server does not support the requested function.
         * @memberof ursa.model.HttpStatusCodes
         * @name NotImplemented
         * @static
         * @public
         * @readonly
         * @member {number} NotImplemented
         */
        NotImplemented: 501,

        /**
         * BadGateway indicates that an intermediate proxy server received a bad response from another proxy or the origin server.
         * @memberof ursa.model.HttpStatusCodes
         * @name BadGateway
         * @static
         * @public
         * @readonly
         * @member {number} BadGateway
         */
        BadGateway: 502,

        /**
         * ServiceUnavailable indicates that the server is temporarily unavailable, usually due to high load or maintenance.
         * @memberof ursa.model.HttpStatusCodes
         * @name ServiceUnavailable
         * @static
         * @public
         * @readonly
         * @member {number} ServiceUnavailable
         */
        ServiceUnavailable: 503,

        /**
         * GatewayTimeout indicates that an intermediate proxy server timed out while waiting for a response from another proxy or the origin server.
         * @memberof ursa.model.HttpStatusCodes
         * @name GatewayTimeout
         * @static
         * @public
         * @readonly
         * @member {number} GatewayTimeout
         */
        GatewayTimeout: 504,

        /**
         * HttpVersionNotSupported indicates that the requested HTTP version is not supported by the server.
         * @memberof ursa.model.HttpStatusCodes
         * @name HttpVersionNotSupported
         * @static
         * @public
         * @readonly
         * @member {number} HttpVersionNotSupported
         */
        HttpVersionNotSupported: 505
    };
}(namespace("ursa.model")));