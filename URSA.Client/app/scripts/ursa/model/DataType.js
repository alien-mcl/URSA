/*globals namespace, ursa */
(function(namespace) {
    "use strict";

    /**
     * Describes a datatype.
     * @memberof ursa.model
     * @name DataType
     * @public
     * @extends {ursa.model.Type}
     * @class
     * @param {ursa.model.ApiMember} owner Owner if this instance being created.
     * @param {object} [resource] JSON-LD resource describing this API member.
     * @param {ursa.IGraph} [graph] JSON-LD graph of resources.
     */
    var DataType = (namespace.DataType = function(owner, resource, graph) {
        if ((arguments.length === 1) && (owner instanceof DataType)) {
            ursa.model.Type.prototype.constructor.apply(this, arguments);
            return;
        }

        Function.requiresArgument("graph", graph, ursa.IGraph);
        var flattenedClassDefinition = {};
        namespace.composeClass.call(flattenedClassDefinition, resource, graph);
        ursa.model.Type.prototype.constructor.call(this, owner, flattenedClassDefinition, graph);
    })[":"](ursa.model.Type);

    /**
     * Inherits from this data type.
     * @memberof ursa.model.DataType
     * @instance
     * @public
     * @method subClass
     * @param {string} id Sub-class identifier.
     * @returns {ursa.model.DataType} Inheriting data type.
     */
    DataType.prototype.subClass = function(id) {
        var result = new DataType(this);
        result.baseId = this.id;
        result.id = id;
        return result;
    };

    DataType.toString = function () { return "ursa.model.DataType"; };
}(namespace("ursa.model")));