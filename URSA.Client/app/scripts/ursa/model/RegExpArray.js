/*globals namespace */
(function (namespace) {
    "use strict";

    var properties = { year: 4, month: 2, day: 2, hour: 2, minute: 2, second: 2 };

    /**
     * Represents a cummilative regular expression array.
     * @memberof ursa.model
     * @name RegExpArray
     * @public
     * @class
     */
    var RegExpArray = namespace.RegExpArray = function() {
        Array.prototype.constructor.apply(this, arguments);
    };
    RegExpArray.prototype = [];

    /**
     * Generates an OData filter from regular expressions and their matches.
     * @memberof ursa.model.RegExpArray
     * @instance
     * @public
     * @method
     * @param {ursa.model.SupportedProperty} supportedProperty Supported property to be used as filter.
     * @param {string} value Value of the supported property.
     * @returns {string} OData filter for a given supported property.
     */
    RegExpArray.prototype.generateExpression = function(supportedProperty, value) {
        for (var index = 0; index < this.length; index++) {
            var regex = this[index];
            var match = regex.exec(value);
            if ((match !== null) && (match.length > 0)) {
                var result = regex.format.replace("{property}", "<" + supportedProperty.property + ">");
                for (var property in properties) {
                    var propertyValue = "";
                    if (regex[property] !== -1) {
                        propertyValue = match[regex[property]];
                        while (propertyValue.length < properties[property]) {
                            propertyValue = "0" + propertyValue;
                        }
                    }

                    result = result.replace(new RegExp("{" + property + "}"), propertyValue);
                }

                return result;
            }
        }

        return null;
    };
}(namespace("ursa.model")));