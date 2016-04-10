/*globals namespace, ursa */
(function(namespace) {
    "use strict";

    /**
     * View renderer provider.
     * @memberof ursa.view
     * @name ViewRendererProvider
     * @public
     * @class
     * @param {ursa.IComponentFactory} viewRendererFactory Factory providing ursa.view.ViewRenderer implementations.
     * @param {ursa.web.HttpService} httpService An HTTP async communication service.
     * @param {ursa.model.JsonLdProcessor} jsonLdProcessor JSON-LD service.
     * @param {ursa.web.AuthenticationProvider} authenticationProvider Authentication provider.
     * @param {ursa.model.FilterProvider} filterProvider Filter provider.
     */
    var ViewRendererProvider = namespace.ViewRendererProvider = function(viewRendererFactory) {
        Function.requiresArgument("viewRendererFactory", viewRendererFactory, ursa.IComponentFactory);
        this._viewRenderersFactory = viewRendererFactory;
    };

    Object.defineProperty(ViewRendererProvider.prototype, "_viewRenderersFactory", { enumerable: false, configurable: false, writable: true, value: null });

    /**
     * Creates a view for given API member.
     * @memberof ursa.view.ViewRendererProvider
     * @instance
     * @public
     * @method createRenderer
     * @param {ursa.model.ApiMember} apiMember Target API member for which to render a view.
     * @returns {ursa.view.ViewRenderer} The view renderer.
     */
    ViewRendererProvider.prototype.createRenderer = function(apiMember) {
        var viewRenderers = this._viewRenderersFactory.resolveAll();
        for (var index = 0; index < viewRenderers.length; index++) {
            var viewRenderer = viewRenderers[index];
            if (viewRenderer.isApplicableTo(apiMember)) {
                viewRenderer.initialize(apiMember);
                return viewRenderer;
            }
        }

        return null;
    };

    ViewRendererProvider.toString = function() { return "ursa.view.ViewRendererProvider"; };
}(namespace("ursa.view")));