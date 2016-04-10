/*globals namespace, joice, ursa, hydra, xsd, rdf, rdfs, owl, guid */
(function(namespace) {
    "use strict";

    var _SupportedProperty = {};

    /**
     * Describes a class' supported property.
     * @memberof ursa.model
     * @name SupportedProperty
     * @public
     * @extends {ursa.model.ApiMember}
     * @class
     * @param {ursa.model.ApiMember} owner Owner if this instance being created.
     * @param {object} [supportedProperty] JSON-LD resource describing this API member.
     * @param {ursa.IGraph} [graph] JSON-LD graph of resources.
     */
    var SupportedProperty = (namespace.SupportedProperty = function(owner, supportedProperty, graph) {
        Function.requiresArgument("supportedProperty", supportedProperty);
        Function.requiresArgument("graph", graph, ursa.IGraph);
        ursa.model.ApiMember.prototype.constructor.apply(this, arguments);
        var property = supportedProperty[hydra.property];
        if ((property === undefined) || (property === null) || (!(property instanceof Array)) || (property.length === 0)) {
            throw new joice.ArgumentOutOfRangeException("supportedProperty");
        }

        property = graph.getById(this.property = property[0]["@id"]);
        var range = (property[rdfs.range] ? property[rdfs.range][0] : null);
        if (range !== null) {
            this.range = namespace.getClass.call(graph.getById(range["@id"]), this, graph);
        }

        this.key = (property["@type"] || []).indexOf(owl.InverseFunctionalProperty) !== -1;
        this.label = (this.label || namespace.getValue.call(property, rdfs.label)) || "";
        this.description = (this.description || namespace.getValue.call(property, rdfs.comment)) || "";
        this.readable = namespace.getValue.call(supportedProperty, hydra.readable) || true;
        this.writeable = namespace.getValue.call(supportedProperty, hydra.writeable) || true;
        this.sortable = (this.maxOccurances > 1) && (this.range !== null) && (this.range.isList);
        this.required = (namespace.getValue.call(supportedProperty, hydra.required) || this.required) || false;
        this.supportedOperations = [];
        namespace.getOperations.call(this, supportedProperty, graph, hydra.operation);
    })[":"](ursa.model.ApiMember);

    /**
     * Marks a property as readable.
     * @memberof ursa.model.SupportedProperty
     * @instance
     * @public
     * @member {boolean} readable
     * @default true
     */
    SupportedProperty.prototype.readable = true;

    /**
     * Marks a property as writeable.
     * @memberof ursa.model.SupportedProperty
     * @instance
     * @public
     * @member {boolean} writeable
     * @default true
     */
    SupportedProperty.prototype.writeable = true;

    /**
     * Marks that an instance is required.
     * @memberof ursa.model.SupportedProperty
     * @instance
     * @public
     * @member {boolean} required
     * @default false
     */
    SupportedProperty.prototype.required = false;

    /**
     * Range of values.
     * @memberof ursa.model.SupportedProperty
     * @instance
     * @public
     * @member {ursa.model.ApiMember} range
     */
    SupportedProperty.prototype.range = null;

    /**
     * Marks a property as sortable.
     * @memberof ursa.model.SupportedProperty
     * @instance
     * @public
     * @member {boolean} sortable
     * @default false
     */
    SupportedProperty.prototype.sortable = false;

    /**
     * Defines a given property as a instance's primary key.
     * @memberof ursa.model.SupportedProperty
     * @instance
     * @public
     * @member {boolean} key
     * @default false
     */
    SupportedProperty.prototype.key = false;

    /**
     * List of supported operations.
     * @memberof ursa.model.SupportedProperty
     * @instance
     * @public
     * @member {Array.<ursa.model.Operation>} supportedOperations
     */
    SupportedProperty.prototype.supportedOperations = null;

    /**
     * Min occurances of the property value.
     * @memberof ursa.model.SupportedProperty
     * @instance
     * @public
     * @readonly
     * @member {number} minOccurances
     * @default 0
     */
    Object.defineProperty(SupportedProperty.prototype, "minOccurances", { get: function () { return Math.max(this.required ? 1 : 0, this.range ? this.range.minOccurances : 0); } });

    /**
     * Max occurances of the property value.
     * @memberof ursa.model.SupportedProperty
     * @instance
     * @public
     * @readonly
     * @member {number} maxOccurances
     * @default 1
     */
    Object.defineProperty(SupportedProperty.prototype, "maxOccurances", { get: function () { return (this.range ? this.range.maxOccurances : 1); } });

    /**
     * Gets flag indicating whether the class represents a list in terms of rdf:List.
     * @memberof ursa.model.SupportedProperty
     * @instance
     * @public
     * @readonly
     * @member {ursa.model.Class} base
     */
    Object.defineProperty(SupportedProperty.prototype, "isList", { get: function () { return (this.range ? this.range.isList : false); } });

    /**
     * Target instance property.
     * @memberof ursa.model.SupportedProperty
     * @instance
     * @public
     * @member {string} property
     */
    SupportedProperty.prototype.property = null;

    /**
     * Creates an instance of value accepted by this property.
     * @memberof ursa.model.SupportedProperty
     * @instance
     * @public
     * @method createInstance
     * @returns {object} Value matching a range of this property.
     */
    SupportedProperty.prototype.createInstance = function () { return _SupportedProperty.createLiteralValue.call(this, this.range); };

    /**
     * Searches the collection for a member with a given property value.
     * @memberof ursa.model.SupportedProperty
     * @instance
     * @public
     * @method propertyName
     * @param {ursa.model.Operation} operation Operation in which of context to create a property name. Operation is used to determine whether it accepts RDF payloads or not.
     * @returns {string} Name of the property suitable for given operation.
     */
    SupportedProperty.prototype.propertyName = function(operation) {
        if (!operation.isRdf) {
            var parts = this.property.split(/[^a-zA-Z0-9_]/);
            return parts[parts.length - 1];
        }

        return this.property;
    };

    /**
     * Initializes a supported property in a given instance.
     * @memberof ursa.model.SupportedProperty
     * @instance
     * @public
     * @method initializeInstance
     * @param {ursa.model.Operation} operation Operation in which of context to create a property name. Operation is used to determine whether it accepts RDF payloads or not.
     * @param {object} instance Object instance to initialize a property on.
     * @returns {object} Value initialized.
     */
    SupportedProperty.prototype.initializeInstance = function(operation, instance) {
        var propertyName = this.propertyName(operation);
        var value = (instance ? instance[propertyName] : undefined);
        if (!value) {
            value = (!operation.isRdf ? (this.minOccurances > 1 ? [] : null) : (this.isList ? [{ "@list": [] }] : []));
            if (instance) {
                instance[propertyName] = value;
            }
        }

        return value;
    };

    var subject = { "@id": "urn:name:subject" };
    var subjectGraph = [subject, { "@id": rdf.subject }];
    subjectGraph.getById = function(id) { return (id === rdf.subject ? subjectGraph[1] : null); };
    subject[hydra.property] = [subjectGraph[1]];

    /**
     * Instace of the property that represents an RDF resource identifier.
     * @memberof ursa.model.SupportedProperty
     * @static
     * @public
     * @member {ursa.model.SupportedProperty} Subject
     */
    SupportedProperty.Subject = new SupportedProperty(null, subject, subjectGraph);

    SupportedProperty.toString = function() { return "ursa.model.SupportedProperty"; };

    _SupportedProperty.createLiteralValue = function(type) {
        var result = null;
        switch (type.typeId) {
            case xsd.anyUri:
            case xsd.string: result = ""; break;
            case xsd.boolean: result = false; break;
            case xsd.byte:
            case xsd.unsignedByte:
            case xsd.short:
            case xsd.unsignedShort:
            case xsd.int:
            case xsd.unsignedInt:
            case xsd.long:
            case xsd.unsignedLong:
            case xsd.integer: result = 0; break;
            case xsd.float:
            case xsd.double:
            case xsd.decimal: result = 0.0; break;
            case xsd.time:
            case xsd.date:
            case xsd.dateTime: result = new Date(); break;
            case xsd.gYearMonth: result = new Date(new Date().getFullYear(), new Date().getMonth()); break;
            case xsd.gYear: result = new Date().getFullYear(); break;
            case xsd.gMonth: result = new Date().getMonth() + 1; break;
            case xsd.gDay: result = new Date().getDate(); break;
            case guid.guid: result = "00000000-0000-0000-0000-000000000000"; break;
        }

        return result;
    };
}(namespace("ursa.model")));