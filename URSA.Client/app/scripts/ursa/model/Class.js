/*globals namespace, ursa, xsd, rdfs, owl, hydra */
(function(namespace) {
    "use strict";

    var _Class = {};

    /**
     * Describes a class.
     * @memberof ursa.model
     * @name Class
     * @public
     * @extends {ursa.model.Type}
     * @class
     * @param {ursa.model.ApiMember} owner Owner if this instance being created.
     * @param {object} [supportedClass] JSON-LD resource describing this API member.
     * @param {ursa.IGraph} [graph] JSON-LD graph of resources.
     * @param {object} [ref] Reference that will get a lazy class initializer.
     * @param {function} [ref.lazyInitializer] Defers properties and operations initialization so the class can be dereferenced multiple times.
     */
    var Class = (namespace.Class = function(owner, supportedClass, graph, ref) {
        if ((arguments.length === 1) && (owner instanceof Class)) {
            ursa.model.Type.prototype.constructor.apply(this, arguments);
            _Class.initializeSubClass.call(this, owner.id);
            return;
        }

        var flattenedClassDefinition = {};
        var superClassId = namespace.composeClass.call(flattenedClassDefinition, supportedClass, graph);
        ursa.model.Type.prototype.constructor.call(this, owner, flattenedClassDefinition, graph);
        if ((superClassId !== null) && (superClassId !== supportedClass["@id"])) {
            _Class.initializeSubClass.call(this, superClassId);
        }
        else {
            this.supportedProperties = new ursa.model.ApiMemberCollection(this);
            this.supportedOperations = [];
        }

        if (!ref) {
            _Class.completeInitialization.call(this, flattenedClassDefinition, graph);
        }
        else {
            var that = this;
            ref.lazyInitializer = function() { _Class.completeInitialization.apply(that, arguments); };
        }
    })[":"](ursa.model.Type);

    /**
     * List of supported properties.
     * @memberof ursa.model.Class
     * @instance
     * @public
     * @member {ursa.model.ApiMemberCollection.<ursa.model.SupportedProperty>} supportedProperties
     */
    Class.prototype.supportedProperties = null;

    /**
     * List of supported operations.
     * @memberof ursa.model.Class
     * @instance
     * @public
     * @member {Array.<ursa.model.Operation>} supportedOperations
     */
    Class.prototype.supportedOperations = null;

    /**
     * Gets the super-class instance of a given class if any.
     * @memberof ursa.model.Class
     * @instance
     * @public
     * @member {ursa.model.Class} base
     */
    Class.prototype.base = null;

    /**
     * Creates an instance of this class.
     * @memberof ursa.model.Class
     * @instance
     * @public
     * @method createInstance
     * @param {ursa.model.Operation} operation Operation in which of context to create a property name. Operation is used to determine whether it accepts RDF payloads or not.
     * @returns {object} Instance of this class.
     */
    Class.prototype.createInstance = function(operation) {
        if ((operation === undefined) || (operation === null) || (!(operation instanceof ursa.model.Operation))) {
            operation = (this.owner instanceof ursa.model.Operation ? this.owner : null);
        }

        var entityFormat = ((operation !== null) && (operation.isRdf) ? ursa.model.EntityFormat.RDF : ursa.model.EntityFormat.JSON);
        return (entityFormat === ursa.model.EntityFormat.RDF ? _Class.createRdfInstance.call(this, operation) : _Class.createJsonInstance.call(this, operation));
    };

    /**
     * Gets a property that uniquely identifies a resource.
     * @memberof ursa.model.Class
     * @instance
     * @public
     * @method getKeyProperty
     * @param {ursa.model.Operation} operation Operation that holds details about RDF requirement.
     * @returns {ursa.model.SupportedProperty} Key property of an instance of this class.
     */
    Class.prototype.getKeyProperty = function(operation) {
        for (var index = 0; index < this.supportedProperties.length; index++) {
            var supportedProperty = this.supportedProperties[index];
            if (supportedProperty.key) {
                return supportedProperty;
            }
        }

        return (operation.isRdf ? ursa.model.SupportedProperty.Subject : null);
    };

    /**
     * Gets a property that should be used as an display name of the instance, i.e. for lists.
     * @memberof ursa.model.Class
     * @instance
     * @public
     * @method getInstanceDisplayNameProperty
     * @param {ursa.model.Operation} operation Operation that holds details about RDF requirement.
     * @returns {ursa.model.SupportedProperty} Display name property of an instance of this class.
     */
    Class.prototype.getInstanceDisplayNameProperty = function(operation) {
        var key = (operation.isRdf ? ursa.model.SupportedProperty.Subject : null);
        for (var index = 0; index < this.supportedProperties.length; index++) {
            var supportedProperty = this.supportedProperties[index];
            if (supportedProperty.key) {
                key = supportedProperty;
            }

            if ((supportedProperty.property === rdfs.label) || ((supportedProperty.range === xsd.string) && (supportedProperty.maxOccurances === 1))) {
                return supportedProperty;
            }
        }

        return key;
    };

    /**
     * Sub-classes a given class.
     * @memberof ursa.model.Class
     * @instance
     * @public
     * @method subClass
     * @param {string} id Sub-class identifier.
     * @returns {ursa.model.Class} Sub-class.
     */
    Class.prototype.subClass = function (id) {
        var result = new Class(this);
        result.id = id;
        return result;
    };

    Class.toString = function () { return "ursa.model.Class"; };

    _Class.createJsonInstance = function (operation) {
        var result = {};
        for (var index = 0; index < this.supportedProperties.length; index++) {
            var supportedProperty = this.supportedProperties[index];
            var newValue = supportedProperty.initializeInstance(operation, result);
            if (supportedProperty.minOccurances > 0) {
                var value = supportedProperty.createInstance();
                if (value !== null) {
                    if (supportedProperty.maxOccurances > 1) {
                        for (var itemIndex = 0; itemIndex < supportedProperty.minOccurances; itemIndex++) {
                            newValue.push(value);
                        }
                    }
                    else {
                        result[supportedProperty.propertyName(operation)] = value;
                    }
                }
            }
        }

        return result;
    };

    _Class.createRdfInstance = function (operation) {
        var result = { "@id": "_:bnode" + Math.random().toString().replace(".", "").substr(1) };
        if (this.id.indexOf("_") !== 0) {
            result["@type"] = this.id;
        }

        for (var index = 0; index < this.supportedProperties.length; index++) {
            var supportedProperty = this.supportedProperties[index];
            var newValue = supportedProperty.initializeInstance(operation, result);
            if (supportedProperty.minOccurances > 0) {
                var value = supportedProperty.createInstance();
                if (value !== null) {
                    for (var itemIndex = 0; itemIndex < supportedProperty.minOccurances; itemIndex++) {
                        newValue.push({ "@value": value, "@type": supportedProperty.range.id });
                    }
                }
            }
        }

        return result;
    };

    _Class.initializeProperties = function (supportedClass, graph, subClassOf) {
        if (supportedClass[hydra.supportedProperty]) {
            for (var index = 0; index < supportedClass[hydra.supportedProperty].length; index++) {
                var supportedPropertyResource = graph.getById(supportedClass[hydra.supportedProperty][index]["@id"]);
                var supportedProperty = new ursa.model.SupportedProperty(this, supportedPropertyResource, graph);
                if (supportedProperty.range === null) {
                    this.supportedProperties.push(supportedProperty);
                    continue;
                }

                for (var restrictionIndex = 0; restrictionIndex < subClassOf.length; restrictionIndex++) {
                    var restriction = graph.getById(subClassOf[restrictionIndex]["@id"]);
                    if ((!restriction["@type"]) || (restriction["@type"].indexOf(owl.Restriction) === -1) ||
                        (namespace.getValue.call(restriction, owl.onProperty) !== supportedProperty.property)) {
                        continue;
                    }

                    var maxCardinality = namespace.getValue.call(restriction, owl.maxCardinality);
                    if (maxCardinality !== null) {
                        if (maxCardinality !== supportedProperty.range.maxOccurances) {
                            supportedProperty.range = supportedProperty.range.subClass(restriction["@id"]);
                        }

                        supportedProperty.range.maxOccurances = maxCardinality;
                    }

                    subClassOf.splice(restrictionIndex, 1);
                }

                this.supportedProperties.push(supportedProperty);
            }
        }
    };

    _Class.completeInitialization = function (supportedClass, graph) {
        var subClassOf = supportedClass[rdfs.subClassOf] || [];
        if (this.baseId === null) {
            _Class.initializeProperties.call(this, supportedClass, graph, subClassOf);
        }

        namespace.getOperations.call(this, supportedClass, graph);
    };

    _Class.getBase = function () {
        if (this.baseId === null) {
            return null;
        }

        for (var index = 0; index < this.apiDocumentation.knownTypes.length; index++) {
            var knownType = this.apiDocumentation.knownTypes[index];
            if (knownType.id === this.baseId) {
                return knownType;
            }
        }

        return null;
    };

    _Class.initializeSubClass = function(superClassId) {
        this.baseId = superClassId;
        var base = null;
        Object.defineProperty(this, "base", { get: function () { return (base === null ? base = _Class.getBase.call(this) : base); } });
        Object.defineProperty(this, "supportedProperties", { get: function () { return (this.base !== null ? this.base.supportedProperties : null); } });
        Object.defineProperty(this, "supportedOperations", { get: function () { return (this.base !== null ? this.base.supportedOperations : null); } });
    };

    /**
     * Instace of the class that represents any kind of type.
     * @memberof ursa.model.Class
     * @static
     * @public
     * @member {ursa.model.Class} Thing
     */
    Class.Thing = new Class(null, { "@id": owl.Thing });
}(namespace("ursa.model")));