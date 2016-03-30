/*globals xsd, rdf, rdfs, owl, hydra, ursa, shacl, guid, namespace, UriTemplate, joice, jsonld */
(function(namespace) {
    "use strict";

    var getById = function(id) {
        for (var index = 0; index < this.length; index++) {
            if (this[index]["@id"] === id) {
                return this[index];
            }
        }

        return null;
    };

    var parseDate = function(dateTime) {
        var timeZone = null;
        var position;
        if ((position = dateTime.lastIndexOf("+")) !== -1) {
            timeZone = dateTime.substr(position + 1);
            dateTime = dateTime.substr(0, position);
        }
        else if ((position = dateTime.lastIndexOf("Z")) !== -1) {
            timeZone = "00:00";
            dateTime = dateTime.substr(0, position);
        }

        var date;
        var time = "00:00:00.000";
        if ((position = dateTime.indexOf("T")) !== -1) {
            date = dateTime.substr(0, position);
            time = dateTime.substr(position + 1);
        }
        else {
            date = dateTime;
        }

        date = date.split("-");
        time = time.split(":");
        var result = new Date(parseInt(date[0]), parseInt(date[1] - 1), parseInt(date[2]), parseInt(time[0]), parseInt(time[1]),
            (time.length > 2 ? parseInt(time[2].split(".")[0]) : 0), ((time.length > 2) && (time[2].indexOf(".") !== -1) ? time[2].split(".")[1] : 0));
        if (timeZone !== null) {
            timeZone = timeZone.split(":");
            result.setTime(result.getTime() + (parseInt(timeZone[0]) * 60 * 1000) + (parseInt(timeZone[1]) * 1000));
        }

        return result;
    };

    var parseValue = function(type) {
        if ((this === null) || (typeof(this) !== "string")) {
            return null;
        }

        if ((this === "") && (type === xsd.string)) {
            return "";
        }

        switch (type) {
            case xsd.boolean:
                return ((this === "") || (this === "false") ? false : true);
            case xsd.byte:
            case xsd.unsignedByte:
            case xsd.short:
            case xsd.unsignedShort:
            case xsd.int:
            case xsd.unsignedInt:
            case xsd.long:
            case xsd.unsignedLong:
            case xsd.integer:
            case xsd.nonPositiveInteger:
            case xsd.nonNegativeInteger:
            case xsd.negativeInteger:
            case xsd.positiveInteger:
            case xsd.gYear:
            case xsd.gMonth:
            case xsd.gDay:
                return (this === "" ? 0 : parseInt(this));
            case xsd.float:
            case xsd.double:
            case xsd.decimal:
                return (this === "" ? 0 : parseFloat(this));
            case xsd.dateTime:
                return (this === "" ? new Date(0, 1, 1, 0, 0, 0) : parseDate(this));
        }

        return this;
    };

    var getValue = function(property) {
        if ((!this[property]) || (this[property].length === 0)) {
            return null;
        }

        var result = (this[property][0]["@value"] !== undefined ? this[property][0]["@value"] : this[property][0]["@id"]);
        if (this[property][0]["@type"] === undefined) {
            return result;
        }

        if ((this[property][0]["@type"] !== xsd.string) && (typeof(result) === "string")) {
            result = parseValue.call(result, this[property][0]["@type"]);
        }

        return result;
    };

    var getValues = function(property) {
        var result = [];
        if (this[property]) {
            for (var index = 0; index < this[property].length; index++) {
                var value = this[property][index]["@value"] || this[property][index]["@id"];
                if ((this[property][index]["@type"] !== undefined) && (this[property][index]["@type"] !== xsd.string) && (typeof(value) === "string")) {
                    value = parseValue.call(value, this[property][index]["@type"]);
                }

                if (value !== null) {
                    result.push(value);
                }
            }
        }

        return result;
    };

    var canOverrideSuperClassId = function(superClassId) {
        return (superClassId === null) || ((superClassId === hydra.Collection) && (this["@id"] === rdf.List)) || ((isBlankNode.call(superClassId)) && (!isBlankNode(this)));
    };

    var isBlankNode = function(resource) {
        return (resource ? resource["@id"] : this).indexOf("_:") === 0;
    };

    var isRestriction = function(resource) {
        return !!((resource[owl.onProperty]) || (resource[owl.allValuesFrom]));
    };

    var composeClass = function($class, graph, levels, level) {
        levels = levels || [];
        level = level || 0;
        var superClasses = [];
        var superClassId = ((this["@id"]) && (this["@id"].indexOf("_:") === 0) ? this["@id"] : null);
        for (var property in $class) {
            if (!$class.hasOwnProperty(property)) {
                continue;
            }

            switch (property) {
                case "@id":
                    if (!this["@id"]) {
                        this["@id"] = $class["@id"];
                    }

                    if ((canOverrideSuperClassId.call($class, superClassId)) && (!isBlankNode($class)) && (!isRestriction($class)) && (this["@id"] !== $class["@id"])) {
                        superClassId = $class["@id"];
                    }

                    break;
                case "@type":
                    if (!this["@type"]) {
                        this["@type"] = $class["@type"];
                    }
                    else {
                        for (var typeIndex = 0; typeIndex < $class["@type"].length; typeIndex++) {
                            this["@type"].push($class["@type"][typeIndex]);
                        }
                    }

                    break;
                case rdfs.subClassOf:
                    superClasses = $class[rdfs.subClassOf];
                    break;
                default:
                    if ((!this[property]) || ((this[property]) && (levels[property]) && (levels[property] > level))) {
                        this[property] = $class[property];
                        levels[property] = level;
                    }

                    break;
            }
        }

        for (var index = 0; index < superClasses.length; index++) {
            var superSuperClassId = composeClass.call(this, graph.getById(superClasses[index]["@id"]), graph, levels, level + 1);
            if ((superClassId === null) || ((superSuperClassId === hydra.Collection) && (superClassId !== rdf.List)) || (superSuperClassId === rdf.List)) {
                superClassId = superSuperClassId;
            }
        }

        return superClassId;
    };

    var getClass = function(owner, graph) {
        var $class = {};
        var targetClass = $class;
        var superClassId = composeClass.call($class, this, graph);
        var isEnumerable = false;
        var isList = false;
        if (superClassId !== null) {
            switch (superClassId) {
                case hydra.Collection:
                    isEnumerable = true;
                    targetClass = graph.getById($class[owl.allValuesFrom][0]["@id"]);
                    break;
                case rdf.List:
                    isEnumerable = isList = true;
                    targetClass = graph.getById($class[owl.allValuesFrom][0]["@id"]);
                    break;
                default:
                    targetClass = graph.getById(superClassId);
                    break;
            }
        }

        var result = owner.apiDocumentation.knownTypes.getById(targetClass["@id"]);
        if (result === null) {
            var Ctor = ((targetClass["@id"].indexOf(xsd) === -1) && (targetClass["@id"].indexOf(guid) === -1) ? Class : DataType);
            result = new Ctor(owner.apiDocumentation || owner, targetClass, graph);
        }

        if ($class === targetClass) {
            return result;
        }

        result = result.subClass($class["@id"]);
        Type.prototype.constructor.call(result, owner, $class, graph);
        result.maxOccurances = (isEnumerable ? Number.MAX_VALUE : result.maxOccurances);
        result.isList = isList;
        return result;
    };

    var createLiteralValue = function(type) {
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

    var getOperations = function(resource, graph, predicate) {
        if (predicate === undefined) {
            predicate = hydra.supportedOperation;
        }

        var index;
        if (resource[predicate]) {
            for (index = 0; index < resource[predicate].length; index++) {
                var supportedOperation = graph.getById(resource[predicate][index]["@id"]);
                this.supportedOperations.push(new Operation(this, supportedOperation, null, graph));
            }
        }

        for (var propertyName in resource) {
            if ((resource.hasOwnProperty(propertyName)) && (propertyName.match(/^[a-zA-Z][a-zA-Z0-9\+\-\.]*:/) !== null) &&
            (propertyName.indexOf(xsd) === -1) && (propertyName.indexOf(rdf) === -1) && (propertyName.indexOf(rdfs) === -1) &&
            (propertyName.indexOf(owl) === -1) && (propertyName.indexOf(guid) === -1) && (propertyName.indexOf(hydra) === -1) &&
            (propertyName.indexOf(shacl) === -1) && (propertyName.indexOf(ursa) === -1)) {
                var property = graph.getById(propertyName);
                if ((!property) || (property["@type"].indexOf(hydra.TemplatedLink) === -1) || (!property[hydra.supportedOperation])) {
                    continue;
                }

                var operation = graph.getById(property[hydra.supportedOperation][0]["@id"]);
                var templates = resource[propertyName];
                for (index = 0; index < templates.length; index++) {
                    var template = graph.getById(templates[index]["@id"]);
                    if (template["@type"].indexOf(hydra.IriTemplate) !== -1) {
                        this.supportedOperations.push(new Operation(this, operation, template, graph));
                    }
                }
            }
        }
    };
    
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
            this.id = (!isBlankNode(resource) ? resource["@id"] : this.id || "");
            this.label = (getValue.call(resource, rdfs.label) || this.label) || "";
            this.description = (getValue.call(resource, rdfs.comment) || this.description) || "";
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
        while ((result !== null) && (!(result instanceof ApiDocumentation))) {
            result = result.owner;
        }

        return result;
    } });
    ApiMember.toString = function() { return "ursa.model.ApiMember"; };

    /**
     * Collection of {@link ursa.model.ApiMember} instances.
     * @memberof ursa.model
     * @name ApiMemberCollection
     * @public
     * @extends {Array}
     * @class
     * @param {ursa.model.ApiMember} owner Owner if this instance being created.
     */
    var ApiMemberCollection = namespace.ApiMemberCollection = function(owner) {
        this._owner = owner || null;
    };
    ApiMemberCollection.prototype = [];
    ApiMemberCollection.prototype._owner = null;
    /**
     * Searches the collection for a member with a given id.
     * @memberof ursa.model.ApiMemberCollection
     * @instance
     * @public
     * @method getById
     * @param {string} id Id of the member to be found.
     * @returns {ursa.model.ApiMember} Instance of the {@link ursa.model.ApiMember} with given id if found; otherwise null.
     */
    ApiMemberCollection.prototype.getById = function(id) { return this.getByProperty(id, "id"); };
    /**
     * Searches the collection for a member with a given property value.
     * @memberof ursa.model.ApiMemberCollection
     * @instance
     * @public
     * @method getByProperty
     * @param {string} value Value of the member's property to be found.
     * @param {string} [property] Property to be compared to of the member to be found. If not value is provided a default of 'id' is used.
     * @returns {ursa.model.ApiMember} Instance of the {@link ursa.model.ApiMember} with given id if found; otherwise null.
     */
    ApiMemberCollection.prototype.getByProperty = function(value, property) {
        if ((property === undefined) || (property === null) || (typeof(property) !== "string")) {
            property = "id";
        }

        if ((value === undefined) || (value === null) || (typeof(property) !== "string")) {
            return null;
        }

        for (var index = 0; index < this.length; index++) {
            if (this[index][property] === value) {
                return this[index];
            }
        }

        return null;
    };
    ApiMemberCollection.toString = function() { return "ursa.model.ApiMemberCollection"; };

    /**
     * Describes a class' supported property.
     * @memberof ursa.model
     * @name SupportedProperty
     * @public
     * @extends {ursa.model.ApiMember}
     * @class
     * @param {ursa.model.ApiMember} owner Owner if this instance being created.
     * @param {object} [resource] JSON-LD resource describing this API member.
     * @param {object} [graph] JSON-LD graph of resources.
     */
    var SupportedProperty = (namespace.SupportedProperty = function(owner, supportedProperty, graph) {
        ApiMember.prototype.constructor.apply(this, arguments);
        var property = supportedProperty[hydra.property];
        if ((property === undefined) || (property === null) || (!(property instanceof Array)) || (property.length === 0)) {
            throw new joice.ArgumentOutOfRangeException("supportedProperty");
        }

        property = graph.getById(this.property = property[0]["@id"]);
        var range = (property[rdfs.range] ? property[rdfs.range][0] : null);
        if (range !== null) {
            this.range = getClass.call(graph.getById(range["@id"]), this, graph);
        }

        this.key = (property["@type"] || []).indexOf(owl.InverseFunctionalProperty) !== -1;
        this.label = (this.label || getValue.call(property, rdfs.label)) || "";
        this.description = (this.description || getValue.call(property, rdfs.comment)) || "";
        this.readable = getValue.call(supportedProperty, hydra.readable) || true;
        this.writeable = getValue.call(supportedProperty, hydra.writeable) || true;
        this.sortable = (this.maxOccurances > 1) && (this.range !== null) && (this.range.isList);
        this.required = (getValue.call(supportedProperty, hydra.required) || this.required) || false;
        this.supportedOperations = [];
        getOperations.call(this, supportedProperty, graph, hydra.operation);
    })[":"](ApiMember);
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
     * @member {number} minOccurances
     * @default 0
     */
    Object.defineProperty(SupportedProperty.prototype, "minOccurances", { get: function() { return Math.max(this.required ? 1 : 0, this.range ? this.range.minOccurances : 0); } });
    /**
     * Max occurances of the property value.
     * @memberof ursa.model.SupportedProperty
     * @instance
     * @public
     * @member {number} maxOccurances
     * @default 1
     */
    Object.defineProperty(SupportedProperty.prototype, "maxOccurances", { get: function() { return (this.range ? this.range.maxOccurances : 1); } });
    /**
     * Gets flag indicating whether the class represents a list in terms of rdf:List.
     * @memberof ursa.model.SupportedProperty
     * @instance
     * @public
     * @member {ursa.model.Class} base
     */
    Object.defineProperty(SupportedProperty.prototype, "isList", { get: function() { return (this.range ? this.range.isList : false); } });
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
    SupportedProperty.prototype.createInstance = function() {
        return createLiteralValue.call(this, this.range);
    };
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
        ApiMember.prototype.constructor.apply(this, arguments);
        this.variable = getValue.call(mapping, hydra.variable);
        this.property = getValue.call(mapping, hydra.property);
        this.required = getValue.call(mapping, hydra.required) || false;
    })[":"](ApiMember);
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
    Mapping.toString = function() { return "ursa.model.Mapping"; };

    var _Operation = {};
    /**
     * Describes an ReST operation.
     * @memberof ursa.model
     * @name Operation
     * @public
     * @extends {ursa.model.ApiMember}
     * @class
     * @param {ursa.model.ApiMember} owner Owner if this instance being created.
     * @param {object} [supportedOperation] JSON-LD resource describing this API member.
     * @param {object} [template] JSON-LD resource describing this API member.
     * @param {object} [graph] JSON-LD graph of resources.
     */
    var Operation = (namespace.Operation = function(owner, supportedOperation, template, graph) {
        ApiMember.prototype.constructor.apply(this, arguments);
        if ((template) && (template[hydra.template])) {
            if ((this.url = template[hydra.template][0]["@value"]).match(/^[a-zA-Z][a-zA-Z0-9\+\-\.]*/) === null) {
                var apiDocumentation = this.apiDocumentation;
                var entryPoint = ((apiDocumentation.entryPoints) && (apiDocumentation.entryPoints.length > 0) ? apiDocumentation.entryPoints[0] : window.location.href);
                this.url = (entryPoint.charAt(entryPoint.length - 1) === "/" ? entryPoint.substr(0, entryPoint.length - 1) : entryPoint) + this.url;
            }

            this.mappings = new ApiMemberCollection(this);
            for (var index = 0; index < template[hydra.mapping].length; index++) {
                this.mappings.push(new Mapping(this, graph.getById(template[hydra.mapping][index]["@id"]), graph));
            }
        }
        else {
            this.url = this.id;
            this.mappings = null;
        }

        this.returns = [];
        this.expects = [];
        this.methods = getValues.call(supportedOperation, hydra.method);
        this.statusCodes = getValues.call(supportedOperation, hydra.statusCode);
        if ((this.mediaTypes = getValues.call(supportedOperation, ursa.mediaType)).length === 0) {
            this.mediaTypes.push(EntityFormat.ApplicationLdJson);
            this.mediaTypes.push(EntityFormat.ApplicationJson);
        }
        else {
            this.mediaTypes.sort(function(leftOperand, rightOperand) {
                return (leftOperand.indexOf("json") !== -1 ? -1 : (rightOperand.indexOf("json") !== -1 ? 1 : 0));
            });
        }

        _Operation.setupTypeCollection.call(this, supportedOperation[hydra.returns], this.returns, graph);
        _Operation.setupTypeCollection.call(this, supportedOperation[hydra.expects], this.expects, graph);
    })[":"](ApiMember);
    /**
     * List of allowed HTTP verbs for this operation.
     * @memberof ursa.model.Mapping
     * @instance
     * @public
     * @member {Array.<string>} methods
     */
    Operation.prototype.methods = null;
    /**
     * List of expected types.
     * @memberof ursa.model.Mapping
     * @instance
     * @public
     * @member {Array.<ursa.model.Type>} expects
     */
    Operation.prototype.expects = null;
    /**
     * List of returned types.
     * @memberof ursa.model.Mapping
     * @instance
     * @public
     * @member {Array.<ursa.model.Type>} returns
     */
    Operation.prototype.returns = null;
    /**
     * List of returned HTTP status codes.
     * @memberof ursa.model.Mapping
     * @instance
     * @public
     * @member {Array.<number>} statusCodes
     */
    Operation.prototype.statusCodes = null;
    /**
     * List of returned acceptable media types.
     * @memberof ursa.model.Mapping
     * @instance
     * @public
     * @member {Array.<string>} mediaTypes
     */
    Operation.prototype.mediaTypes = null;
    /**
     * Url of this operation.
     * @memberof ursa.model.Mapping
     * @instance
     * @public
     * @member {string} url
     */
    Operation.prototype.url = null;
    /**
     * Collection of IRI template mappings if any. If there is no template associated this member is null.
     * @memberof ursa.model.Mapping
     * @instance
     * @public
     * @member {string} mappings
     */
    Operation.prototype.mappings = null;
    /**
     * Creates an operation call URL. This method parses the url for any template mappings and fills it with values.
     * @memberof ursa.model.Operation
     * @instance
     * @public
     * @method createCallUrl
     * @param {object} instance Object with values to be used when expanding the template.
     * @returns {string} Operation's call URL.
     */
    Operation.prototype.createCallUrl = function(instance) {
        var result = this.url;
        if (this.mappings !== null) {
            var input = {};
            for (var index = 0; index < this.mappings.length; index++) {
                var mapping = this.mappings[index];
                var propertyValue = null;
                if ((instance !== undefined) && (instance !== null)) {
                    if (mapping.property !== null) {
                        propertyValue = instance[mapping.propertyName(this)];
                    }

                    if ((propertyValue !== undefined) && (propertyValue !== null) && (propertyValue instanceof Array)) {
                        propertyValue = propertyValue[0];
                    }

                    if ((this.isRdf) && (propertyValue !== undefined) && (propertyValue !== null)) {
                        propertyValue = propertyValue["@value"] || propertyValue["@id"];
                    }
                }

                if ((propertyValue !== undefined) && (propertyValue !== null)) {
                    input[mapping.variable] = propertyValue;
                }
            }

            result = new UriTemplate(result).fill(input);
        }

        var indexOf = -1;
        if ((indexOf = result.indexOf("#")) !== -1) {
            result = result.substr(0, indexOf);
        }

        return result;
    };
    /**
     * Gets a flag indicating whether the operation accepts JSON-LD or not.
     * @memberof ursa.model.Operation
     * @instance
     * @public
     * @readonly
     * @member {boolean} isRdf
     */
    Object.defineProperty(Operation.prototype, "isRdf", { get: function() { return this.mediaTypes.indexOf(EntityFormat.ApplicationLdJson) !== -1; } });
    Operation.toString = function() { return "ursa.model.Operation"; };
    _Operation.setupTypeCollection = function(source, target, graph) {
        if (!source) {
            return;
        }

        for (var index = 0; index < source.length; index++) {
            target.push(getClass.call(graph.getById(source[index]["@id"]), this, graph));
        }
    };

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
    var Type = (namespace.Type = function(owner, resource, graph) {
        if ((arguments.length === 1) && (owner instanceof Type)) {
            ApiMember.prototype.constructor.apply(this, arguments);
            this.minOccurances = owner.minOccurances;
            this.maxOccurances = owner.maxOccurances;
            this.isList = owner.isList;
            return;
        }

        this.minOccurances = 0;
        this.maxOccurances = 1;
        ApiMember.prototype.constructor.call(this, owner, resource, graph);
        var subClassOf = resource[rdfs.subClassOf] || [];
        for (var index = 0; index < subClassOf.length; index++) {
            if (subClassOf[index]["@id"] === rdf.List) {
                this.isList = true;
                break;
            }
        }
    })[":"](ApiMember);
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
    Type.toString = function() { return "ursa.model.Type"; };

    /**
     * Describes a datatype.
     * @memberof ursa.model
     * @name DataType
     * @public
     * @extends {ursa.model.ApiMember}
     * @class
     * @param {ursa.model.ApiMember} owner Owner if this instance being created.
     * @param {object} [resource] JSON-LD resource describing this API member.
     */
    var DataType = (namespace.DataType = function(owner, resource, graph) {
        if ((arguments.length === 1) && (owner instanceof DataType)) {
            Type.prototype.constructor.apply(this, arguments);
            return;
        }

        var flattenedClassDefinition = {};
        composeClass.call(flattenedClassDefinition, resource, graph);
        Type.prototype.constructor.call(this, owner, flattenedClassDefinition, graph);
    })[":"](Type);
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
    DataType.toString = function() { return "ursa.model.DataType"; };

    var _Class = {};
    /**
     * Describes a class.
     * @memberof ursa.model
     * @name Class
     * @public
     * @extends {ursa.model.ApiMember}
     * @class
     * @param {ursa.model.ApiMember} owner Owner if this instance being created.
     * @param {object} [supportedOperation] JSON-LD resource describing this API member.
     * @param {object} [graph] JSON-LD graph of resources.
     * @param {boolean} [deferredInitialization] Defers properties and operations initialization so the class can be dereferenced multiple times.
     */
    var Class = (namespace.Class = function(owner, supportedClass, graph, deferredInitialization) {
        if ((arguments.length === 1) && (owner instanceof Class)) {
            Type.prototype.constructor.apply(this, arguments);
            _Class.initializeSubClass.call(this, owner.id);
            return;
        }

        var flattenedClassDefinition = {};
        var superClassId = composeClass.call(flattenedClassDefinition, supportedClass, graph);
        Type.prototype.constructor.call(this, owner, flattenedClassDefinition, graph);
        if ((superClassId !== null) && (superClassId !== supportedClass["@id"])) {
            _Class.initializeSubClass.call(this, superClassId);
        }
        else {
            this.supportedProperties = new ApiMemberCollection(this);
            this.supportedOperations = [];
        }

        if (!(typeof(deferredInitialization) !== "boolean" ? false : deferredInitialization)) {
            _Class.completeInitialization.call(this, flattenedClassDefinition, graph);
        }
    })[":"](Type);
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
        if ((operation === undefined) || (operation === null) || (!(operation instanceof Operation))) {
            operation = (this.owner instanceof Operation ? this.owner : null);
        }

        var entityFormat = ((operation !== null) && (operation.isRdf) ? EntityFormat.RDF : EntityFormat.JSON);
        return (entityFormat === EntityFormat.RDF ? _Class.createRdfInstance.call(this, operation) : _Class.createJsonInstance.call(this, operation));
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

        return (operation.isRdf ? SupportedProperty.Subject : null);
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
        var key = (operation.isRdf ? SupportedProperty.Subject : null);
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
    Class.prototype.subClass = function(id) {
        var result = new Class(this);
        result.id = id;
        return result;
    };
    Class.toString = function() { return "ursa.model.Class"; };
    _Class.createJsonInstance = function(operation) {
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
    _Class.createRdfInstance = function(operation) {
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
    _Class.initializeProperties = function(supportedClass, graph, subClassOf) {
        if (supportedClass[hydra.supportedProperty]) {
            for (var index = 0; index < supportedClass[hydra.supportedProperty].length; index++) {
                var supportedPropertyResource = graph.getById(supportedClass[hydra.supportedProperty][index]["@id"]);
                var supportedProperty = new SupportedProperty(this, supportedPropertyResource, graph);
                if (supportedProperty.range === null) {
                    this.supportedProperties.push(supportedProperty);
                    continue;
                }

                for (var restrictionIndex = 0; restrictionIndex < subClassOf.length; restrictionIndex++) {
                    var restriction = graph.getById(subClassOf[restrictionIndex]["@id"]);
                    if ((!restriction["@type"]) || (restriction["@type"].indexOf(owl.Restriction) === -1) ||
                        (getValue.call(restriction, owl.onProperty) !== supportedProperty.property)) {
                        continue;
                    }

                    var maxCardinality = getValue.call(restriction, owl.maxCardinality);
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
    _Class.completeInitialization = function(supportedClass, graph) {
        var subClassOf = supportedClass[rdfs.subClassOf] || [];
        if (this.baseId === null) {
            _Class.initializeProperties.call(this, supportedClass, graph, subClassOf);
        }

        getOperations.call(this, supportedClass, graph);
    };
    _Class.getBase = function() {
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
        Object.defineProperty(this, "base", { get: function() { return (base === null ? base = _Class.getBase.call(this) : base); } });
        Object.defineProperty(this, "supportedProperties", { get: function() { return (this.base !== null ? this.base.supportedProperties : null); } });
        Object.defineProperty(this, "supportedOperations", { get: function() { return (this.base !== null ? this.base.supportedOperations : null); } });
    };
    /**
     * Instace of the class that represents any kind of type.
     * @memberof ursa.model.Class
     * @static
     * @public
     * @member {ursa.model.Class} Thing
     */
    Class.Thing = new Class(null, { "@id": owl.Thing });

    /**
     * Describes an entity formats.
     * @memberof ursa.model
     * @name EntityFormat
     * @public
     * @class
     */
    var EntityFormat = namespace.EntityFormat = function(mediaTypes) {
        this._mediaTypes = [];
        var values = (mediaTypes instanceof Array ? mediaTypes : arguments);
        for (var index = 0; index < values.length; index++) {
            this._mediaTypes.push(values[index]);
        }
    };
    EntityFormat.prototype._mediaTypes = null;
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

    var JsonLdProcessor = namespace.JsonLdProcessor = function(promiseProvider) {
        Function.requiresArgument("promiseProvider", promiseProvider, ursa.IPromiseProvider);
        this._promiseProvider = promiseProvider;
    };
    Object.defineProperty(JsonLdProcessor.prototype, "promiseProvider", { enumerable: false, configurable: false, writable: true, value: null });
    JsonLdProcessor.prototype.expand = function(graph) {
        var deferred = this._promiseProvider.defer();
        jsonld.expand(graph, function(error, expanded) {
            if (error) {
                deferred.reject(error);
            }

            deferred.resolve(expanded);
        });
        return deferred.promise;
    };
    JsonLdProcessor.toString = function() { return "ursa.model.JsonLdProcessor"; };

    var ApiDocumentationProvider = namespace.ApiDocumentationProvider = function(jsonLdProcessor, httpService, promiseProvider) {
        Function.requiresArgument("jsonLdProcessor", jsonLdProcessor, ursa.JsonLDProcessor);
        Function.requiresArgument("httpService", httpService, ursa.web.HttpService);
        Function.requiresArgument("promiseProvider", promiseProvider, ursa.IPromiseProvider);
        this._jsonLdProcessor = jsonLdProcessor;
        this._httpService = httpService;
        this._promiseProvider = promiseProvider;
    };
    Object.defineProperty(ApiDocumentationProvider.prototype, "_jsonLdProcessor", { enumerable: false, configurable: false, writable: true, value: null });
    Object.defineProperty(ApiDocumentationProvider.prototype, "_httpService", { enumerable: false, configurable: false, writable: true, value: null });
    Object.defineProperty(ApiDocumentationProvider.prototype, "_promiseProvider", { enumerable: false, configurable: false, writable: true, value: null });
    ApiDocumentationProvider.prototype.load = function(entryPoint) {
        if ((entryPoint === undefined) || (entryPoint === null)) {
            entryPoint = window.location.href;
        }

        if (entryPoint instanceof String) {
            entryPoint = entryPoint.toString();
        }

        if (typeof(entryPoint) !== "string") {
            throw new joice.ArgumentOutOfRangeException("entryPoint");
        }

        if ((entryPoint.length === 0) || (entryPoint.match(/^http[s]?:\/\//i) === null)) {
            throw new joice.ArgumentOutOfRangeExceptio("entryPoint");
        }

        var that = this;
        var deferred = this._promiseProvider.defer();
        var request = new ursa.web.HttpRequest("OPTIONS", entryPoint, { "Accept": ["application/ld+json", "application/json"] });
        this._httpService.sendRequest(request).
            then(function(response) { return _ApiDocumentationProvider.onLoad.call(that, deferred, response); }).
            catch(function(response) { return _ApiDocumentationProvider.onError.call(that, deferred, response); });
        return deferred.promise;
    };
    ApiDocumentationProvider.toString = function() { return "ursa.model.ApiDocumentationProvider"; };
    var _ApiDocumentationProvider = {};
    _ApiDocumentationProvider.onExpanded = function(deferred, expanded) {
        deferred.resolve(new ApiDocumentation(expanded));
    };
    _ApiDocumentationProvider.onExpansionError = function(deferred, error) {
        deferred.reject(error);
    };
    _ApiDocumentationProvider.onError = function(deferred, response) {
        deferred.reject(response.data);
    };
    _ApiDocumentationProvider.onLoad = function(deferred, response) {
        var that = this;
        this._jsonLdProcessor.expand(response.data).
            then(function(expanded) { _ApiDocumentationProvider.onExpanded.call(that, deferred, expanded); }).
            catch(function(error) { _ApiDocumentationProvider.onExpansionError.call(that, deferred, error); });
    };

    /**
     * Provides an enumeration of HTTP status codes.
     * @memberof ursa.model
     * @name HttpStatusCodes
     * @public
     * @class
     */
    namespace.HttpStatusCodes = {
        /**
         * Continue indicates that the client can continue with its request.
         * @memberof ursa.model.HttpStatusCodes
         * @name Continue
         * @static
         * @public
         * @readonly
         * @member {number} Continue
         */
        Continue: 100,
        /**
         * SwitchingProtocols indicates that the protocol version or protocol is being changed.
         * @memberof ursa.model.HttpStatusCodes
         * @name SwitchingProtocols
         * @static
         * @public
         * @readonly
         * @member {number} SwitchingProtocols
         */
        SwitchingProtocols: 101,

        /**
         * OK indicates that the request succeeded and that the requested information is in the response. This is the most common status code to receive.
         * @memberof ursa.model.HttpStatusCodes
         * @name OK
         * @static
         * @public
         * @readonly
         * @member {number} OK
         */
        OK: 200,
        /**
         * Created indicates that the request resulted in a new resource created before the response was sent.
         * @memberof ursa.model.HttpStatusCodes
         * @name Created
         * @static
         * @public
         * @readonly
         * @member {number} Created
         */
        Created: 201,
        /**
         * Accepted indicates that the request has been accepted for further processing.
         * @memberof ursa.model.HttpStatusCodes
         * @name Accepted
         * @static
         * @public
         * @readonly
         * @member {number} Accepted
         */
        Accepted: 202,
        /**
         * NonAuthoritativeInformation indicates that the returned metainformation is from a cached copy instead of the origin server and therefore may be incorrect.
         * @memberof ursa.model.HttpStatusCodes
         * @name NonAuthoritativeInformation
         * @static
         * @public
         * @readonly
         * @member {number} NonAuthoritativeInformation
         */
        NonAuthoritativeInformation: 203,
        /**
         * NoContent indicates that the request has been successfully processed and that the response is intentionally blank.
         * @memberof ursa.model.HttpStatusCodes
         * @name NoContent
         * @static
         * @public
         * @readonly
         * @member {number} NoContent
         */
        NoContent: 204,
        /**
         * ResetContent indicates that the client should reset (not reload) the current resource.
         * @memberof ursa.model.HttpStatusCodes
         * @name ResetContent
         * @static
         * @public
         * @readonly
         * @member {number} ResetContent
         */
        ResetContent: 205,
        /**
         * PartialContent indicates that the response is a partial response as requested by a GET request that includes a byte range.
         * @memberof ursa.model.HttpStatusCodes
         * @name PartialContent
         * @static
         * @public
         * @readonly
         * @member {number} PartialContent
         */
        PartialContent: 206,

        /**
         * MultipleChoices indicates that the requested information has multiple representations. The default action is to treat this status as a redirect and follow the contents of the Location header associated with this response.
         * @memberof ursa.model.HttpStatusCodes
         * @name MultipleChoices
         * @static
         * @public
         * @readonly
         * @member {number} MultipleChoices
         */
        MultipleChoices: 300,
        /**
         * Ambiguous indicates that the requested information has multiple representations. The default action is to treat this status as a redirect and follow the contents of the Location header associated with this response.
         * @memberof ursa.model.HttpStatusCodes
         * @name Ambiguous
         * @static
         * @public
         * @readonly
         * @member {number} Ambiguous
         */
        Ambiguous: 300,
        /**
         * Moved indicates that the requested information has been moved to the URI specified in the Location header. The default action when this status is received is to follow the Location header associated with the response. When the original request method was POST, the redirected request will use the GET method.
         * @memberof ursa.model.HttpStatusCodes
         * @name Moved
         * @static
         * @public
         * @readonly
         * @member {number} Moved
         */
        Moved: 301,
        /**
         * MovedPermanently indicates that the requested information has been moved to the URI specified in the Location header. The default action when this status is received is to follow the Location header associated with the response.
         * @memberof ursa.model.HttpStatusCodes
         * @name MovedPermanently
         * @static
         * @public
         * @readonly
         * @member {number} MovedPermanently
         */
        MovedPermanently: 301,
        /**
         * Redirect indicates that the requested information is located at the URI specified in the Location header. The default action when this status is received is to follow the Location header associated with the response. When the original request method was POST, the redirected request will use the GET method.
         * @memberof ursa.model.HttpStatusCodes
         * @name Redirect
         * @static
         * @public
         * @readonly
         * @member {number} Redirect
         */
        Redirect: 302,
        /**
         * Found indicates that the requested information is located at the URI specified in the Location header. The default action when this status is received is to follow the Location header associated with the response. When the original request method was POST, the redirected request will use the GET method.
         * @memberof ursa.model.HttpStatusCodes
         * @name Found
         * @static
         * @public
         * @readonly
         * @member {number} Found
         */
        Found: 302,
        /**
         * SeeOther automatically redirects the client to the URI specified in the Location header as the result of a POST. The request to the resource specified by the Location header will be made with a GET.
         * @memberof ursa.model.HttpStatusCodes
         * @name SeeOther
         * @static
         * @public
         * @readonly
         * @member {number} SeeOther
         */
        SeeOther: 303,
        /**
         * RedirectMethod automatically redirects the client to the URI specified in the Location header as the result of a POST. The request to the resource specified by the Location header will be made with a GET.
         * @memberof ursa.model.HttpStatusCodes
         * @name RedirectMethod
         * @static
         * @public
         * @readonly
         * @member {number} RedirectMethod
         */
        RedirectMethod: 303,
        /**
         * NotModified indicates that the client's cached copy is up to date. The contents of the resource are not transferred.
         * @memberof ursa.model.HttpStatusCodes
         * @name NotModified
         * @static
         * @public
         * @readonly
         * @member {number} NotModified
         */
        NotModified: 304,
        /**
         * UseProxy indicates that the request should use the proxy server at the URI specified in the Location header.
         * @memberof ursa.model.HttpStatusCodes
         * @name UseProxy
         * @static
         * @public
         * @readonly
         * @member {number} UseProxy
         */
        UseProxy: 305,
        /**
         * Unused is a proposed extension to the HTTP/1.1 specification that is not fully specified.
         * @memberof ursa.model.HttpStatusCodes
         * @name Unused
         * @static
         * @public
         * @readonly
         * @member {number} Unused
         */
        Unused: 306,
        /**
         * RedirectKeepVerb indicates that the request information is located at the URI specified in the Location header. The default action when this status is received is to follow the Location header associated with the response. When the original request method was POST, the redirected request will also use the POST method.
         * @memberof ursa.model.HttpStatusCodes
         * @name RedirectKeepVerb
         * @static
         * @public
         * @readonly
         * @member {number} RedirectKeepVerb
         */
        RedirectKeepVerb: 307,
        /**
         * TemporaryRedirect indicates that the request information is located at the URI specified in the Location header. The default action when this status is received is to follow the Location header associated with the response. When the original request method was POST, the redirected request will also use the POST method.
         * @memberof ursa.model.HttpStatusCodes
         * @name TemporaryRedirect
         * @static
         * @public
         * @readonly
         * @member {number} TemporaryRedirect
         */
        TemporaryRedirect: 307,

        /**
         * BadRequest indicates that the request could not be understood by the server. BadRequest is sent when no other error is applicable, or if the exact error is unknown or does not have its own error code.
         * @memberof ursa.model.HttpStatusCodes
         * @name BadRequest
         * @static
         * @public
         * @readonly
         * @member {number} BadRequest
         */
        BadRequest: 400,
        /**
         * Unauthorized indicates that the requested resource requires authentication. The WWW-Authenticate header contains the details of how to perform the authentication.
         * @memberof ursa.model.HttpStatusCodes
         * @name Unauthorized
         * @static
         * @public
         * @readonly
         * @member {number} Unauthorized
         */
        Unauthorized: 401,
        /**
         * PaymentRequired is reserved for future use.
         * @memberof ursa.model.HttpStatusCodes
         * @name PaymentRequired
         * @static
         * @public
         * @readonly
         * @member {number} PaymentRequired
         */
        PaymentRequired: 402,
        /**
         * Forbidden indicates that the server refuses to fulfill the request.
         * @memberof ursa.model.HttpStatusCodes
         * @name Forbidden
         * @static
         * @public
         * @readonly
         * @member {number} Forbidden
         */
        Forbidden: 403,
        /**
         * NotFound indicates that the requested resource does not exist on the server.
         * @memberof ursa.model.HttpStatusCodes
         * @name NotFound
         * @static
         * @public
         * @readonly
         * @member {number} NotFound
         */
        NotFound: 404,
        /**
         * MethodNotAllowed indicates that the request method (POST or GET) is not allowed on the requested resource.
         * @memberof ursa.model.HttpStatusCodes
         * @name MethodNotAllowed
         * @static
         * @public
         * @readonly
         * @member {number} MethodNotAllowed
         */
        MethodNotAllowed: 405,
        /**
         * NotAcceptable indicates that the client has indicated with Accept headers that it will not accept any of the available representations of the resource.
         * @memberof ursa.model.HttpStatusCodes
         * @name NotAcceptable
         * @static
         * @public
         * @readonly
         * @member {number} NotAcceptable
         */
        NotAcceptable: 406,
        /**
         * ProxyAuthenticationRequired indicates that the requested proxy requires authentication. The Proxy-authenticate header contains the details of how to perform the authentication.
         * @memberof ursa.model.HttpStatusCodes
         * @name ProxyAuthenticationRequired
         * @static
         * @public
         * @readonly
         * @member {number} ProxyAuthenticationRequired
         */
        ProxyAuthenticationRequired: 407,
        /**
         * RequestTimeout indicates that the client did not send a request within the time the server was expecting the request.
         * @memberof ursa.model.HttpStatusCodes
         * @name RequestTimeout
         * @static
         * @public
         * @readonly
         * @member {number} RequestTimeout
         */
        RequestTimeout: 408,
        /**
         * Conflict indicates that the request could not be carried out because of a conflict on the server.
         * @memberof ursa.model.HttpStatusCodes
         * @name Conflict
         * @static
         * @public
         * @readonly
         * @member {number} Conflict
         */
        Conflict: 409,
        /**
         * Gone indicates that the requested resource is no longer available.
         * @memberof ursa.model.HttpStatusCodes
         * @name Gone
         * @static
         * @public
         * @readonly
         * @member {number} Gone
         */
        Gone: 410,
        /**
         * LengthRequired indicates that the required Content-length header is missing.
         * @memberof ursa.model.HttpStatusCodes
         * @name LengthRequired
         * @static
         * @public
         * @readonly
         * @member {number} LengthRequired
         */
        LengthRequired: 411,
        /**
         * PreconditionFailed indicates that a condition set for this request failed, and the request cannot be carried out. Conditions are set with conditional request headers like If-Match, If-None-Match, or If-Unmodified-Since.
         * @memberof ursa.model.HttpStatusCodes
         * @name PreconditionFailed
         * @static
         * @public
         * @readonly
         * @member {number} PreconditionFailed
         */
        PreconditionFailed: 412,
        /**
         * RequestEntityTooLarge indicates that the request is too large for the server to process.
         * @memberof ursa.model.HttpStatusCodes
         * @name RequestEntityTooLarge
         * @static
         * @public
         * @readonly
         * @member {number} RequestEntityTooLarge
         */
        RequestEntityTooLarge: 413,
        /**
         * RequestUriTooLong indicates that the URI is too long.
         * @memberof ursa.model.HttpStatusCodes
         * @name RequestUriTooLong
         * @static
         * @public
         * @readonly
         * @member {number} RequestUriTooLong
         */
        RequestUriTooLong: 414,
        /**
         * UnsupportedMediaType indicates that the request is an unsupported type.
         * @memberof ursa.model.HttpStatusCodes
         * @name UnsupportedMediaType
         * @static
         * @public
         * @readonly
         * @member {number} UnsupportedMediaType
         */
        UnsupportedMediaType: 415,
        /**
         * RequestedRangeNotSatisfiable indicates that the range of data requested from the resource cannot be returned, either because the beginning of the range is before the beginning of the resource, or the end of the range is after the end of the resource.
         * @memberof ursa.model.HttpStatusCodes
         * @name RequestedRangeNotSatisfiable
         * @static
         * @public
         * @readonly
         * @member {number} RequestedRangeNotSatisfiable
         */
        RequestedRangeNotSatisfiable: 416,
        /**
         * ExpectationFailed indicates that an expectation given in an Expect header could not be met by the server.
         * @memberof ursa.model.HttpStatusCodes
         * @name ExpectationFailed
         * @static
         * @public
         * @readonly
         * @member {number} ExpectationFailed
         */
        ExpectationFailed: 417,
        /**
         * UpgradeRequired indicates that the client should switch to a different protocol such as TLS/1.0.
         * @memberof ursa.model.HttpStatusCodes
         * @name UpgradeRequired
         * @static
         * @public
         * @readonly
         * @member {number} UpgradeRequired
         */
        UpgradeRequired: 426,

        /**
         * InternalServerError indicates that a generic error has occurred on the server.
         * @memberof ursa.model.HttpStatusCodes
         * @name InternalServerError
         * @static
         * @public
         * @readonly
         * @member {number} InternalServerError
         */
        InternalServerError: 500,
        /**
         * NotImplemented indicates that the server does not support the requested function.
         * @memberof ursa.model.HttpStatusCodes
         * @name NotImplemented
         * @static
         * @public
         * @readonly
         * @member {number} NotImplemented
         */
        NotImplemented: 501,
        /**
         * BadGateway indicates that an intermediate proxy server received a bad response from another proxy or the origin server.
         * @memberof ursa.model.HttpStatusCodes
         * @name BadGateway
         * @static
         * @public
         * @readonly
         * @member {number} BadGateway
         */
        BadGateway: 502,
        /**
         * ServiceUnavailable indicates that the server is temporarily unavailable, usually due to high load or maintenance.
         * @memberof ursa.model.HttpStatusCodes
         * @name ServiceUnavailable
         * @static
         * @public
         * @readonly
         * @member {number} ServiceUnavailable
         */
        ServiceUnavailable: 503,
        /**
         * GatewayTimeout indicates that an intermediate proxy server timed out while waiting for a response from another proxy or the origin server.
         * @memberof ursa.model.HttpStatusCodes
         * @name GatewayTimeout
         * @static
         * @public
         * @readonly
         * @member {number} GatewayTimeout
         */
        GatewayTimeout: 504,
        /**
         * HttpVersionNotSupported indicates that the requested HTTP version is not supported by the server.
         * @memberof ursa.model.HttpStatusCodes
         * @name HttpVersionNotSupported
         * @static
         * @public
         * @readonly
         * @member {number} HttpVersionNotSupported
         */
        HttpVersionNotSupported: 505
    };

    /**
     * Describes an API documentation.
     * @memberof ursa.model
     * @name ApiDocumentation
     * @public
     * @extends {ursa.model.ApiMember}
     * @class
     * @param {object} graph JSON-LD graph of resources.
     */
    var ApiDocumentation = namespace.ApiDocumentation = function(graph) {
        if ((graph === undefined) || (graph === null) || (!(graph instanceof Array))) {
            throw new joice.ArgumentOutOfRangeExceptio("graph");
        }

        ApiMember.prototype.constructor.call(this, null, null);
        var index;
        this.owner = null;
        this.entryPoints = [];
        this.supportedClasses = new ApiMemberCollection(this);
        this.knownTypes = new ApiMemberCollection(this);
        graph.getById = function (id) { return getById.call(graph, id); };
        var apiDocumentation = null;
        for (index = 0; index < graph.length; index++) {
            var resource = graph[index];
            if (resource["@type"].indexOf(hydra.ApiDocumentation) !== -1) {
                apiDocumentation = resource;
                this.id = resource["@id"];
                this.title = getValue.call(resource, hydra.title) || "";
                this.description = getValue.call(resource, hydra.description) || "";
                this.entryPoints = getValues.call(resource, hydra.entrypoint);
                if (this.entryPoints.length === 0) {
                    this.entryPoints.push(this.id.match(/^http[s]*:\/\/[^\/]+\//)[0]);
                }

                break;
            }
        }

        if ((apiDocumentation !== null) && (apiDocumentation[hydra.supportedClass] instanceof Array)) {
            for (index = 0; index < apiDocumentation[hydra.supportedClass].length; index++) {
                var supportedClassDefinition = graph.getById(apiDocumentation[hydra.supportedClass][index]["@id"]);
                var supportedClass = new Class(this, supportedClassDefinition, graph, true);
                this.supportedClasses.push(supportedClass);
                this.knownTypes.push(supportedClass);
                _Class.completeInitialization.call(supportedClass, supportedClassDefinition, graph);
            }
        }
    };
    ApiDocumentation[":"](ApiMember);
    /**
     * List of supported classes.
     * @memberof ursa.model.ApiDocumentation
     * @instance
     * @public
     * @member {Array.<ursa.model.Class>} supportedClasses
     */
    ApiDocumentation.prototype.supportedClasses = null;
    /**
     * List of known types.
     * @memberof ursa.model.ApiDocumentation
     * @instance
     * @public
     * @member {Array.<ursa.model.Class>} knownTypes
     */
    ApiDocumentation.prototype.knownTypes = null;
    /**
     * List of entry point Urls.
     * @memberof ursa.model.ApiDocumentation
     * @instance
     * @public
     * @member {Array.<string>} entryPoints
     */
    ApiDocumentation.prototype.entryPoints = null;
}(namespace("ursa.model")));