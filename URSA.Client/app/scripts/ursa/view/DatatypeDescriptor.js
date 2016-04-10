/*globals namespace */
(function(namespace) {
    "use strict";

    /**
     * Describes a datatype features.
     * @memberof ursa.view
     * @name DatatypeDescriptor
     * @protected
     * @class
     * @param {string} type Type of the input.
     * @param {number} [step] Numeric step of the input. Use null for no step.
     * @param {number} [min] Min numeric value. Use null for no min.
     * @param {number} [max] Max numeric value. Use null for no max.
     * @param {string} [placeholder] Placeholder text.
     * @param {string} [pattern] Regular expression pattern.
     */
    var DatatypeDescriptor = namespace.DatatypeDescriptor = function(type, step, min, max, placeholder, pattern) {
        Function.requiresOptionalArgument("type", type, "string");
        Function.requiresOptionalArgument("step", step, "number");
        Function.requiresOptionalArgument("min", min, "number");
        Function.requiresOptionalArgument("max", max, "number");
        Function.requiresOptionalArgument("placeholder", placeholder, "string");
        Function.requiresOptionalArgument("pattern", pattern, "string");
        this.type = type;
        this.step = step;
        this.min = min;
        this.max = max;
        this.placeholder = placeholder;
        this.pattern = pattern;
    };

    /**
     * Checks if a given value is in range of this datatype descriptor values range.
     * @instance
     * @public
     * @method isInRange
     * @param {object} value Value to check.
     * @returns {boolean} True if the value is in a valid range; otherwise false.
     */
    DatatypeDescriptor.prototype.isInRange = function(value) {
        if (this.pattern !== null) {
            return (typeof (value) === "string") && (new RegExp(this.pattern).test(value));
        }

        return (typeof (value) === "number") && (!isNaN(value)) &&
            (((this.min !== null) && (value >= this.min)) || (this.min === null)) &&
            (((this.max !== null) && (value <= this.max)) || (this.max === null));
    };

    /**
     * Type of the input.
     * @memberof ursa.view.DatatypeDescriptor
     * @instance
     * @public
     * @member {string} type
     */
    DatatypeDescriptor.prototype.type = null;

    /**
     * Numeric step of the input. Use null for no step.
     * @memberof ursa.view.DatatypeDescriptor
     * @instance
     * @public
     * @member {number} step
     * @default null
     */
    DatatypeDescriptor.prototype.step = null;

    /**
     * Min numeric value.
     * @memberof ursa.view.DatatypeDescriptor
     * @instance
     * @public
     * @member {number} min
     * @default null
     */
    DatatypeDescriptor.prototype.min = null;

    /**
     * Max numeric value.
     * @memberof ursa.view.DatatypeDescriptor
     * @instance
     * @public
     * @member {number} max
     * @default null
     */
    DatatypeDescriptor.prototype.max = null;

    /**
     * Placeholder text.
     * @memberof ursa.view.DatatypeDescriptor
     * @instance
     * @public
     * @member {string} placeholder
     * @default null
     */
    DatatypeDescriptor.prototype.placeholder = null;

    /**
     * Regular expression pattern.
     * @memberof ursa.view.DatatypeDescriptor
     * @instance
     * @public
     * @member {string} pattern
     * @default null
     */
    DatatypeDescriptor.prototype.pattern = null;
}(namespace("ursa.view")));