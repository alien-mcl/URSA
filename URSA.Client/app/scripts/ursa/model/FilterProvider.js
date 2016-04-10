/*globals namespace, ursa, joice */
(function (namespace) {
    "use strict";

    /**
     * Provides filter expression providers.
     * @memberof ursa.model
     * @name FilterProvider
     * @public
     * @class
     */
    var FilterProvider = namespace.FilterProvider = function(arrayOfFilterExpressionProvider) {
        Function.requiresArgument("filterExpressionProvider", arrayOfFilterExpressionProvider, Array);
        if (arrayOfFilterExpressionProvider.length === 0) {
            throw new joice.ArgumentOutOfRangeException("arrayOfFilterExpressionProvider");
        }

        this._filterExpressionProviders = arrayOfFilterExpressionProvider;
    };

    Object.defineProperty(FilterProvider.prototype, "_filterExpressionProviders", { enumerable: false, configurable: false, writable: true, value: null });

    /**
     * Resolves a filter suitable for given property Uri.
     * @instance
     * @public
     * @method resolve
     * @param {string} predicate Property uri carrying filter expression.
     * @returns {ursa.model.FilterExpressionProvider} Instance of the applicable filter expression provider or null if no suitable ones found.
     */
    FilterProvider.prototype.resolve = function(predicate) {
        Function.requiresArgument("predicate", predicate, "string");
        if (predicate.length === 0) {
            throw new joice.ArgumentOutOfRangeException("predicate");
        }

        for (var index = 0; index < this._filterExpressionProviders.length; index++) {
            var instance = this._filterExpressionProviders[index];
            if (instance.isApplicableTo(predicate)) {
                return instance;
            }
        }

        return null;
    };

    /**
     * Checks if any of the filter expression providers registered provides given capabilities for given mappints.
     * @instance
     * @public
     * @method providesCapabilities
     * @param {ursa.model.ApiMemberCollection<ursa.model.Mapping>} mappings Mappings for which to check capabilities.
     * @param {number} capabilities Capabilities to check against.
     * @returns {boolean} True if any of the registered filter expression providers supports given capabilities; otherwise false.
     */
    FilterProvider.prototype.providesCapabilities = function(mappings, capabilities) {
        Function.requiresArgument("mappings", mappings, ursa.model.ApiMemberCollection);
        Function.requiresArgument("capabilities", capabilities, "number");
        for (var index = 0; index < this._filterExpressionProviders.length; index++) {
            var instance = this._filterExpressionProviders[index];
            if (instance.capabilities(mappings) === capabilities) {
                return true;
            }
        }

        return false;
    };

    FilterProvider.toString = function() { return "ursa.model.FilterProvider"; };
}(namespace("ursa.model")));