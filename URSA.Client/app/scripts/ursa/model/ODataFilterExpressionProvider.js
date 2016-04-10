/*globals namespace, ursa, xsd, odata, guid */
(function (namespace) {
    "use strict";

    var _ODataFilterExpressionProvider = {};

    /**
     * Provides an OData compatibale filtering expression.
     * @memberof ursa.model
     * @name ODataFilterExpressionProvider
     * @public
     * @class
     */
    var ODataFilterExpressionProvider = (namespace.ODataFilterExpressionProvider = function() {
        ursa.model.FilterExpressionProvider.prototype.constructor.apply(this, arguments);
    })[":"](ursa.model.FilterExpressionProvider);

    ODataFilterExpressionProvider.prototype.createExpression = function(mapping, filter, property, value) {
        var supportedProperty = filter.supportedProperties.getByProperty(property, "property");
        if ((supportedProperty === null) || (supportedProperty.range === null) || (!(supportedProperty.range instanceof ursa.model.DataType))) {
            switch (property) {
                case "currentPage":
                    return (mapping.property === odata.skip ? ((filter.currentPage - 1) * filter.itemsPerPage).toString() : null);
                case "itemsPerPage":
                    return (mapping.property === odata.top ? filter.itemsPerPage.toString() : null);
                default:
                    return null;
            }
        }

        if (mapping.property !== odata.filter) {
            return null;
        }

        return _ODataFilterExpressionProvider.createFilterExpression.call(this, supportedProperty, value);
    };

    ODataFilterExpressionProvider.prototype.isApplicableTo = function(uri) { return (uri === odata.filter) || (uri === odata.top) || (uri === odata.skip); };

    ODataFilterExpressionProvider.prototype.capabilities = function(mappings) {
        ursa.model.FilterExpressionProvider.prototype.capabilities.apply(this, arguments);
        if ((mappings.getByProperty(odata.skip, "property")) && (mappings.getByProperty(odata.top, "property"))) {
            return ursa.model.FilterExpressionProvider.SupportsPaging;
        }

        return 0;
    };

    ODataFilterExpressionProvider.toString = function() { return "ursa.model.ODataFilterExpressionProvider"; };

    _ODataFilterExpressionProvider.createFilterExpression = function(supportedProperty, value) {
        switch (supportedProperty.range.id) {
            case xsd.string:
                return String.format("(indexOf(<{0}>, '{1}') ge 0)", supportedProperty.property, value.toString().replace(/'/g, "\\'"));
            case xsd.boolean:
                value = value.toString().toLowerCase();
                return ((value === "true") || (value === "1") || (value === "yes") || (value === "on") ? String.format("(<{0}> eq true)", supportedProperty.property) :
                ((value === "false") || (value === "0") || (value === "no") || (value === "off") ? String.format("(<{0}> eq false)", supportedProperty.property) : ""));
            case xsd.byte:
            case xsd.unsignedByte:
            case xsd.short:
            case xsd.unsignedShort:
            case xsd.int:
            case xsd.unsignedInt:
            case xsd.long:
            case xsd.unsignedLong:
            case xsd.integer:
            case xsd.nonPositiveInteger:
            case xsd.nonNegativeInteger:
            case xsd.positiveInteger:
            case xsd.negativeInteger:
            case xsd.float:
            case xsd.double:
            case xsd.decimal:
                return (ursa.view.SupportedPropertyRenderer.dataTypes[supportedProperty.range.id].isInRange((typeof(value) === "number" ? value : value = parseInt(value.toString()))) ?
                    String.format("(<{0}> eq {1})", supportedProperty.property, value) : "");
            case xsd.dateTime:
            case xsd.time:
            case xsd.date:
                return ursa.model.DateTimeRegExp[supportedProperty.range.id].generateExpression(supportedProperty, value.toString());
            case xsd.gYear:
            case xsd.gMonth:
            case xsd.gDay:
            case xsd.gYearMonth:
                return (ursa.view.SupportedPropertyRenderer.dataTypes[supportedProperty.range.id].isInRange((typeof(value) === "number" ? value : value = parseInt(value.toString()))) ?
                    String.format("(<{0}> eq {1})", supportedProperty.property, value) : "");
            case guid.guid:
                return (ursa.view.SupportedPropertyRenderer.dataTypes[supportedProperty.range.id].isInRange(value.toString()) ?
                    String.format("(<{0}> eq guid('{1}'))", supportedProperty.property, value) : "");
            default:
                return null;
        }
    };
}(namespace("ursa.model")));