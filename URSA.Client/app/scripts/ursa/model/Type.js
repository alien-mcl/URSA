/*globals namespace, ursa, rdf, rdfs */
(function(namespace) {
    "use strict";

    /**
     * Describes an abstract type.
     * @memberof ursa.model
     * @name Type
     * @public
     * @extends {ursa.model.ApiMember}
     * @class
     * @param {ursa.model.ApiMember} owner Owner if this instance being created.
     * @param {object} [resource] JSON-LD resource describing this API member.
     */
    var Type = (namespace.Type = function(owner, resource) {
        if ((arguments.length === 1) && (owner instanceof Type)) {
            ursa.model.ApiMember.prototype.constructor.apply(this, arguments);
            this.minOccurances = owner.minOccurances;
            this.maxOccurances = owner.maxOccurances;
            this.isList = owner.isList;
            return;
        }

        this.minOccurances = 0;
        this.maxOccurances = 1;
        ursa.model.ApiMember.prototype.constructor.call(this, owner, resource);
        var subClassOf = resource[rdfs.subClassOf] || [];
        for (var index = 0; index < subClassOf.length; index++) {
            if (subClassOf[index]["@id"] === rdf.List) {
                this.isList = true;
                break;
            }
        }
    })[":"](ursa.model.ApiMember);

    /**
     * Min occurances of the instance.
     * @memberof ursa.model.Class
     * @instance
     * @public
     * @member {number} minOccurances
     * @default 0
     */
    Type.prototype.minOccurances = 0;

    /**
     * Max occurances of the instance.
     * @memberof ursa.model.Class
     * @instance
     * @public
     * @member {number} maxOccurances
     * @default 1
     */
    Type.prototype.maxOccurances = 1;

    /**
     * Gets the super-class identifier of a given type if any.
     * @memberof ursa.model.Type
     * @instance
     * @public
     * @member {string} baseId
     */
    Type.prototype.baseId = null;

    /**
     * Gets flag indicating whether the class represents a list in terms of rdf:List.
     * @memberof ursa.model.Class
     * @instance
     * @public
     * @member {ursa.model.Class} base
     */
    Type.prototype.isList = false;

    /**
     * Gets flag indicating whether the class represents a list in terms of rdf:List.
     * @memberof ursa.model.SupportedProperty
     * @instance
     * @public
     * @member {ursa.model.Class} base
     */
    Object.defineProperty(Type.prototype, "typeId", { get: function() { return this.baseId || this.id; } });

    Type.toString = function () { return "ursa.model.Type"; };
}(namespace("ursa.model")));