/*globals namespace */
(function (namespace) {
    "use strict";

    /**
     * Abstract encoding facility.
     * @memberof ursa
     * @name Encoder
     * @public
     * @abstract
     * @class
     */
    var Encoder = namespace.Encoder = function() {};

    /**
     * Encodes a given input.
     * @memberof ursa.Encoder
     * @public
     * @abstract
     * @method encode
     * @param {string} input Input to be encoded.
     * @returns {string} Encoded input.
     */
    Encoder.prototype.encode = function(input) {
        Function.requiresArgument("input", input, "string");
        return null;
    };

    /**
     * Decodes a given input.
     * @memberof ursa.Encoder
     * @public
     * @abstract
     * @method decode
     * @param {string} input Input to be decoded.
     * @returns {string} Decoded input.
     */
    Encoder.prototype.decode = function(input) {
        Function.requiresArgument("input", input, "string");
        return null;
    };

    Encoder.toString = function() { return "ursa.Encoder"; };
}(namespace("ursa")));