/*globals namespace, ursa */
(function(namespace) {
    "use strict";

    /**
     * Ultimate ResT API documentation view renderers namespace.
     * @namespace ursa.view
     */

    /**
     * Abstract of a view renderer.
     * @memberof ursa.view
     * @name ViewRenderer
     * @protected
     * @abstract
     * @class
     * @param {ursa.web.HttpService} httpService HTTP async communication provider.
     * @param {ursa.model.JsonLdProcessor} jsonLdProcessor JSON-LD processor.
     * @param {ursa.web.AuthenticationProvider} authenticationProvider Authentication provider.
     * @param {ursa.model.FilterProvider} filterProvider Filter provider.
     */
    var ViewRenderer = namespace.ViewRenderer = function(httpService, jsonLdProcessor, authenticationProvider, filterProvider) {
        Function.requiresArgument("httpService", httpService, ursa.web.HttpService);
        Function.requiresArgument("jsonLdProcessor", jsonLdProcessor, ursa.model.JsonLdProcessor);
        Function.requiresArgument("authenticationProvider", authenticationProvider, ursa.web.AuthenticationProvider);
        Function.requiresArgument("filterProvider", filterProvider, ursa.model.FilterProvider);
        this.httpService = httpService;
        this.jsonLdProcessor = jsonLdProcessor;
        this.authenticationProvider = authenticationProvider;
        this.filterProvider = filterProvider;
    };

    ViewRenderer.prototype.apiMember = null;

    /**
     * The HTTP communication facility.
     * @memberof ursa.view.ViewRenderer
     * @instance
     * @protected
     * @member {ursa.web.HttpService} httpService
     */
    Object.defineProperty(ViewRenderer.prototype, "httpService", { enumerable: false, configurable: false, writable: true, value: null });

    /**
     * JSON-LD processing facility.
     * @memberof ursa.view.ViewRenderer
     * @instance
     * @protected
     * @member {ursa.model.JsonLdProcessor} jsonLdProcessor
     */
    Object.defineProperty(ViewRenderer.prototype, "jsonLdProcessor", { enumerable: false, configurable: false, writable: true, value: null });

    /**
     * Authentication provider.
     * @memberof ursa.view.ViewRenderer
     * @instance
     * @protected
     * @member {ursa.model.AuthenticationProvider} authenticationProvider
     */
    Object.defineProperty(ViewRenderer.prototype, "authenticationProvider", { enumerable: false, configurable: false, writable: true, value: null });

    /**
     * Filter provider.
     * @memberof ursa.view.ViewRenderer
     * @instance
     * @protected
     * @member {ursa.model.FilterProvider} filterProvider
     */
    Object.defineProperty(ViewRenderer.prototype, "filterProvider", { enumerable: false, configurable: false, writable: true, value: null });

    /**
     * Initializes a renderer with dependencies.
     * @memberof ursa.view.ViewRenderer
     * @instance
     * @public
     * @method initialize
     * @param {ursa.model.ApiMember} apiMember API member to check for compatiblity.
     */
    ViewRenderer.prototype.initialize = function(apiMember) {
        Function.requiresArgument("apiMember", apiMember, ursa.model.ApiMember);
        this.apiMember = apiMember;
    };
    /**
     * Determines whether this renderer is applicable for given {ursa.model.ApiMember} instance.
     * @memberof ursa.view.ViewRenderer
     * @instance
     * @public
     * @method isApplicableTo
     * @param {ursa.model.ApiMember} apiMember API member to check for compatiblity.
     */
    ViewRenderer.prototype.isApplicableTo = function(apiMember) {
        Function.requiresArgument("apiMember", apiMember, ursa.model.ApiMember);
        return false;
    };
    /**
     * Renders a view for given API member.
     * @memberof ursa.view.ViewRenderer
     * @instance
     * @public
     * @method render
     * @param {ursa.view.IViewScope} scope Target scope.
     * @param {string} [classNames] CSS class names to be added to the view.
     */
    ViewRenderer.prototype.render = function(scope, classNames) {
        Function.requiresArgument("scope", scope, ursa.view.IViewScope);
        Function.requiresOptionalArgument("classNames", classNames, "string");
        return String.format("<div{0}></div>", (typeof (classNames) === "string" ? String.format(" class=\"{0}\"", classNames) : ""));
    };

    ViewRenderer.toString = function () { return "ursa.view.ViewRenderer"; };
}(namespace("ursa.view")));