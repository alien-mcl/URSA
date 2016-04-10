/*globals namespace, ursa */
(function (namespace) {
    "use strict";

    /**
     * Provides a basic description of property filters.
     * @memberof ursa.model
     * @name Filter
     * @public
     * @class
     * @param {ursa.model.ApiMemberCollection<ursa.model.SupportedProperty>} supportedProperties Collection of supported properties suitable for filtering.
     */
    var Filter = namespace.Filter = function(supportedProperties) {
        Function.requiresArgument("supportedProperties", supportedProperties, ursa.model.ApiMemberCollection);
        this.supportedProperties = supportedProperties;
        this.itemsPerPage = 0;
        this.currentPage = 1;
    };

    /**
     * Collection of supported properties suitable for filtering.
     * @memberof ursa.model.Filter
     * @instance
     * @protected
     * @member {ursa.model.ApiMemberCollection<ursa.model.SupportedProperty>} supportedProperties
     */
    Object.defineProperty(Filter.prototype, "supportedProperties", { enumerable: false, configurable: false, writable: true, value: null });

    /**
     * Total entities per single view.
     * @memberof ursa.model.Filter
     * @instance
     * @public
     * @member {number} itemsPerPage
     */
    Filter.prototype.itemsPerPage = 0;

    /**
     * Current page view.
     * @memberof ursa.model.Filter
     * @instance
     * @public
     * @member {number} page
     */
    Filter.prototype.currentPage = 1;

    Filter.toString = function() { return "ursa.model.Filter"; };
}(namespace("ursa.model")));