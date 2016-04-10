/*globals namespace, ursa */
(function(namespace) {
    "use strict";

    /**
     * Default renderer for {@link ursa.model.Class}.
     * @memberof ursa.view
     * @name ClassRenderer
     * @protected
     * @class
     * @extends ursa.view.ViewRenderer
     * @param {ursa.web.HttpService} httpService HTTP communication facility.
     * @param {ursa.model.JsonLdProcessor} jsonLdProcessor JSON-LD processor.
     * @param {ursa.web.AuthenticationProvider} authenticationProvider Authentication provider.
     * @param {ursa.model.FilterProvider} filterProvider Filter provider.
     * @param {ursa.view.ViewRendererProvider} viewRendererProvider View renderer provider.
     */
    var ClassRenderer = (namespace.ClassRenderer = function(httpService, jsonLdProcessor, authenticationProvider, filterProvider, viewRendererProvider) {
        ursa.view.ViewRenderer.prototype.constructor.call(this, httpService, jsonLdProcessor, authenticationProvider, filterProvider);
        Function.requiresArgument("viewRendererProvider", viewRendererProvider, ursa.view.ViewRendererProvider);
        this._viewRendererProvider = viewRendererProvider;
    })[":"](ursa.view.ViewRenderer);

    Object.defineProperty(ClassRenderer.prototype, "_viewRendererProvider", { enumerable: false, configurable: false, writable: true, value: null });

    ClassRenderer.prototype.isApplicableTo = function(apiMember) {
        ursa.view.ViewRenderer.prototype.isApplicableTo.apply(this, arguments);
        return apiMember instanceof ursa.model.Class;
    };

    ClassRenderer.prototype.initialize = function(apiMember) {
        Function.requiresArgument("apiMember", apiMember, ursa.model.Class);
        ursa.view.ViewRenderer.prototype.initialize.apply(this, arguments);
    };

    ClassRenderer.prototype.render = function(scope, classNames) {
        ursa.view.ViewRenderer.prototype.render.apply(this, arguments);
        var result = String.format(
            "<div class=\"panel{0}\"><div class=\"panel-body\"><form{1}>",
            (typeof (classNames) === "string" ? " " + classNames : ""),
            (scope.uniqueId ? " name=\"" + scope.uniqueId + "\"" : ""));
        for (var index = 0; index < this.apiMember.supportedProperties.length; index++) {
            var supportedProperty = this.apiMember.supportedProperties[index];
            if (!supportedProperty.readable) {
                continue;
            }

            var viewRenderer = this._viewRendererProvider.createRenderer(supportedProperty);
            if (viewRenderer === null) {
                continue;
            }

            result += viewRenderer.render(scope);
        }

        result += "</form></div></div>";
        return result;
    };

    ClassRenderer.toString = function() { return "ursa.view.ClassRenderer"; };
}(namespace("ursa.view")));