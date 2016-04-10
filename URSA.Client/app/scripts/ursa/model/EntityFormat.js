/*globals namespace */
(function (namespace) {
    "use strict";

    /**
     * Describes an entity formats.
     * @memberof ursa.model
     * @name EntityFormat
     * @public
     * @class
     * @param {Array.<string>} mediaTypes Media types of this entity format.
     */
    var EntityFormat = namespace.EntityFormat = function(mediaTypes) {
        this._mediaTypes = [];
        var values = (mediaTypes instanceof Array ? mediaTypes : arguments);
        for (var index = 0; index < values.length; index++) {
            this._mediaTypes.push(values[index]);
        }
    };

    Object.defineProperty(EntityFormat.prototype, "_mediaTypes", { enumerable: false, configurable: false, writable: true, value: null });

    /**
     * Refers to 'application/json' media type.
     * @memberof ursa.model.EntityFormat
     * @static
     * @public
     * @readonly
     * @member {string} ApplicationJson
     */
    EntityFormat.ApplicationJson = "application/json";

    /**
     * Refers to 'application/ld+json' media type.
     * @memberof ursa.model.EntityFormat
     * @static
     * @public
     * @readonly
     * @member {string} ApplicationLdJson
     */
    EntityFormat.ApplicationLdJson = "application/ld+json";

    /**
     * Points to an RDF compliant entity format.
     * @memberof ursa.model.EntityFormat
     * @static
     * @public
     * @member {ursa.model.EntityFormat} RDF
     */
    (EntityFormat.RDF = new EntityFormat(EntityFormat.ApplicationLdJson)).toString = function() { return "ursa.model.EntityFormat.RDF"; };

    /**
     * Points to an plain old JSON compliant entity format.
     * @memberof ursa.model.EntityFormat
     * @static
     * @public
     * @member {ursa.model.EntityFormat} JSON
     */
    (EntityFormat.JSON = new EntityFormat(EntityFormat.ApplicationJson)).toString = function() { return "ursa.model.EntityFormat.JSON"; };

    EntityFormat.toString = function() { return "ursa.model.EntityFormat"; };
}(namespace("ursa.model")));