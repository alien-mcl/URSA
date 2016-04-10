/*globals namespace, ursa, hydra */
(function (namespace) {
    "use strict";

    /**
     * Describes an IRI template mapping.
     * @memberof ursa.model
     * @name Mapping
     * @public
     * @extends {ursa.model.ApiMember}
     * @class
     * @param {ursa.model.ApiMember} owner Owner if this instance being created.
     * @param {object} [resource] JSON-LD resource describing this API member.
     */
    var Mapping = (namespace.Mapping = function(owner, mapping) {
        ursa.model.ApiMember.prototype.constructor.apply(this, arguments);
        this.variable = namespace.getValue.call(mapping, hydra.variable);
        this.property = namespace.getValue.call(mapping, hydra.property);
        this.required = namespace.getValue.call(mapping, hydra.required) || false;
    })[":"](ursa.model.ApiMember);

    /**
     * Name of the template variable.
     * @memberof ursa.model.Mapping
     * @instance
     * @public
     * @member {string} variable
     */
    Mapping.prototype.variable = null;

    /**
     * Property bound on the server side associated with this variable mapping.
     * @memberof ursa.model.Mapping
     * @instance
     * @public
     * @member {string} property
     */
    Mapping.prototype.property = null;

    /**
     * Marks that a variable replacement is required.
     * @memberof ursa.model.Mapping
     * @instance
     * @public
     * @member {boolean} required
     * @default false
     */
    Mapping.prototype.required = false;

    /**
     * Searches the collection for a member with a given property value.
     * @memberof ursa.model.Mapping
     * @instance
     * @public
     * @method propertyName
     * @param {ursa.model.Operation} operation Operation in which of context to create a property name. Operation is used to determine whether it accepts RDF payloads or not.
     * @returns {string} Name of the property suitable for given operation.
     */
    Mapping.prototype.propertyName = function(operation) {
        if (!(operation || this.owner).isRdf) {
            var parts = this.property.split(/[^a-zA-Z0-9_]/);
            return parts[parts.length - 1];
        }

        return this.property;
    };

    Mapping.toString = function () { return "ursa.model.Mapping"; };
}(namespace("ursa.model")));