/*globals namespace, ursa */
(function(namespace) {
    "use strict";

    /**
     * Provides an abstract for filter expression providers.
     * @memberof ursa.model
     * @name FilterExpressionProvider
     * @public
     * @class
     */
    var FilterExpressionProvider = namespace.FilterExpressionProvider = function() { };

    /**
     * Creates a filter expression.
     * @instance
     * @public
     * @method createFilter
     * @param {ursa.model.Mapping} mappings Mappings for which to provide capabilities.
     * @param {ursa.model.Filter} filter Property filters.
     * @returns {string} Expression filtering entities.
     */
    FilterExpressionProvider.prototype.createFilter = function(mapping, filter) {
        if ((filter === undefined) || (filter === null) || (!(filter instanceof ursa.model.Filter))) {
            return null;
        }

        var result = null;
        for (var property in filter) {
            if ((!filter.hasOwnProperty(property)) || (filter[property] === undefined) || (filter[property] === null)) {
                continue;
            }

            var filterExpression = this.createExpression(mapping, filter, property, filter[property]);
            if (filterExpression !== null) {
                result = (result === null ? "" : result) + filterExpression + " ";
            }
        }

        return (result !== null ? result.trim() : null);
    };

    /**
     * Creates a property value expression.
     * @instance
     * @protected
     * @method createExpression
     * @param {ursa.model.Mapping} mappings Mappings for which to provide capabilities.
     * @param {ursa.model.Filter} filter The filters.
     * @param {string} property Current filter property.
     * @param {object} value Current filter value.
     * @returns {string} Expression filtering given supported property with a value.
     */
    FilterExpressionProvider.prototype.createExpression = function() { return ""; };

    /**
     * Checks if a given expression provider is applicable for given property Uri.
     * @instance
     * @public
     * @method isApplicableTo
     * @param {string} uri Property Uri carrying a filter expression.
     * @returns {boolean} True if the expression provider is applicable for the given uri; otherwise false.
     */
    FilterExpressionProvider.prototype.isApplicableTo = function() { return false; };

    /**
     * Provides capabilities supported by the filter expression provider.
     * @instance
     * @public
     * @method capabilities
     * @param {ursa.model.ApiMemberCollection<ursa.model.Mapping>} mappings Mappings for which to provide capabilities.
     * @returns Filter expression provider capaibilities for given mappings.
     */
    FilterExpressionProvider.prototype.capabilities = function(mappings) {
        Function.requiresArgument("mappings", mappings, ursa.model.ApiMemberCollection);
        return 0;
    };

    FilterExpressionProvider.toString = function() { return "ursa.model.FilterExpressionProvider"; };

    /**
     * Signals that the filter expression provider supports paging.
     * @memberof ursa.model.FilterExpressionProvider
     * @static
     * @public
     * @member {number} SupportsPaging
     */
    FilterExpressionProvider.SupportsPaging = 0x00000001;
}(namespace("ursa.model")));