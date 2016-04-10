/*globals namespace, ursa, rdfs */
(function(namespace) {
    "use strict";

    /**
     * Ultimate ResT API documentation data model namespace.
     * @namespace ursa.model
     */

    /**
     * Abstract of an API member.
     * @memberof ursa.model
     * @name ApiMember
     * @protected
     * @abstract
     * @class
     * @param {ursa.model.ApiMember} owner Owner if this instance being created. If no other parameters are passed this parameter is considered as a source to copy from.
     * @param {object} [resource] JSON-LD resource describing this API member.
     */
    var ApiMember = namespace.ApiMember = function(owner, resource) {
        if ((arguments.length === 1) && (owner instanceof ApiMember)) {
            this.id = owner.id;
            this.label = owner.id;
            this.description = owner.description;
            return;
        }

        this.owner = owner || null;
        if ((resource !== null) && (typeof(resource) === "object")) {
            this.id = (!namespace.isBlankNode(resource) ? resource["@id"] : this.id || "");
            this.label = (namespace.getValue.call(resource, rdfs.label) || this.label) || "";
            this.description = (namespace.getValue.call(resource, rdfs.comment) || this.description) || "";
        }
    };

    /**
     * Owner of this member instance.
     * @memberof ursa.model.ApiMember
     * @instance
     * @protected
     * @member {ursa.model.ApiMember} owner
     */
    ApiMember.prototype.owner = null;

    /**
     * Identifier of this API member.
     * @memberof ursa.model.ApiMember
     * @instance
     * @public
     * @member {string} id
     */
    ApiMember.prototype.id = null;

    /**
     * Label of this API member.
     * @memberof ursa.model.ApiMember
     * @instance
     * @public
     * @member {string} label
     */
    ApiMember.prototype.label = null;

    /**
     * Description of this API member.
     * @memberof ursa.model.ApiMember
     * @instance
     * @public
     * @member {string} description
     */
    ApiMember.prototype.description = null;

    ApiMember.prototype.toString = function() { return this.id; };

    /**
     * Searches the API members owner hierarchy for a parent of given type.
     * @memberof ursa.model.ApiMember
     * @instance
     * @public
     * @method parentOfType
     * @param {function} type Type of which to find the parent.
     * @returns {ursa.model.ApiMember} Parent of given type if found; otherwise null.
     */
    ApiMember.prototype.parentOfType = function(type) {
        if (!(type instanceof ApiMember)) {
            return null;
        }

        var result = this.owner;
        while ((result !== null) && (!(result instanceof type))) {
            result = result.owner;
        }

        return result;
    };

    /**
     * Gets the instance of the {@link ursa.model.ApiDocumentation} owning this instance.
     * @memberof ursa.model.ApiMember
     * @instance
     * @public
     * @readonly
     * @member {ursa.model.ApiDocumentation} apiDocumentation
     */
    Object.defineProperty(ApiMember.prototype, "apiDocumentation", { get: function() {
        var result = this;
        while ((result !== null) && (!(result instanceof ursa.model.ApiDocumentation))) {
            result = result.owner;
        }

        return result;
    } });

    ApiMember.toString = function() { return "ursa.model.ApiMember"; };
}(namespace("ursa.model")));