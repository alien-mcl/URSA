/*globals xsd, rdf, rdfs,owl, hydra, ursa, shacl, guid, namespace, UriTemplate */
(function(namespace) {
    "use strict";

    var _ctor = "_ctor";
    var invalidArgumentPassed = "Invalid {0} passed.";

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

        var date = "0000-01-01";
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
            case xsd.unsignedInt:
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

    var getClass = function(owner, graph) {
        var $class = this;
        var originalClass = this;
        var result = null;
        if (($class["@id"].charAt(0) === "_") && (getValue.call($class, rdfs.subClassOf))) {
            $class = graph.getById(getValue.call($class, rdfs.subClassOf));
        }

        if (($class["@id"].indexOf(xsd) === -1) && ($class["@id"].indexOf(guid) === -1)) {
            if (((owner instanceof SupportedProperty) || (owner instanceof Operation)) && (owner.owner !== null) && (owner.owner.id === $class["@id"])) {
                (result = owner.owner.clone()).owner = owner;
                if ($class !== originalClass) {
                    TypeConstrainedApiMember.prototype.constructor.call(result, owner, originalClass);
                }

                return result;
            }

            result = new Class(owner, $class, graph);
            if ($class !== originalClass) {
                TypeConstrainedApiMember.prototype.constructor.call(result, owner, originalClass);
            }
        }
        else {
            result = new DataType(owner, $class);
            if ($class !== originalClass) {
                TypeConstrainedApiMember.prototype.constructor.call(result, owner, originalClass);
            }
        }

        return result;
    };

    var createLiteralValue = function(type) {
        var result = null;
        switch (type.id) {
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
        if ((arguments.length === 1) && (arguments[0] instanceof ApiMember)) {
            ApiMember.prototype.clone.call(this, owner);
            return;
        }

        if ((arguments.length === 0) || (arguments[0] !== _ctor)) {
            this.owner = owner || null;
            if ((resource !== null) && (typeof(resource) === "object")) {
                this.id = (resource["@id"].charAt(0) !== "_" ? resource["@id"] : this.id || "");
                this.label = (getValue.call(resource, rdfs.label) || this.label) || "";
                this.description = (getValue.call(resource, rdfs.comment) || this.description) || "";
            }
        }
    };
    ApiMember.prototype.constructor = ApiMember;
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
    /**
     * Clones a given API member instance.
     * @memberof ursa.model.ApiMember
     * @instance
     * @public
     * @method clone
     * @param {ursa.model.ApiMember} source Source instance to clone from.
     * @returns {ursa.model.ApiMember} Instance initialized from a given source.
     */
    ApiMember.prototype.clone = function(source) {
        this.id = source.id;
        this.label = source.label;
        this.description = source.description;
        this.owner = source.owner;
    };
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
        if ((arguments === 0) || (arguments[0] !== _ctor)) {
            this._owner = owner || null;
        }
    };
    ApiMemberCollection.prototype = [];
    ApiMemberCollection.prototype.constructor = ApiMemberCollection;
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

    /**
     * Marks an API member as a type constrained.
     * @memberof ursa.model
     * @name TypeConstrainedApiMember
     * @public
     * @extends {ursa.model.ApiMember}
     * @class
     * @param {ursa.model.ApiMember} owner Owner if this instance being created.
     * @param {object} [resource] JSON-LD resource describing this API member.
     */
    var TypeConstrainedApiMember = namespace.TypeConstrainedApiMember = function(owner, resource) {
        ApiMember.prototype.constructor.apply(this, arguments);
        if ((arguments.length === 1) && (arguments[0] instanceof TypeConstrainedApiMember)) {
            TypeConstrainedApiMember.prototype.clone.call(this, owner);
            return;
        }

        if ((arguments.length === 0) || (arguments[0] !== _ctor)) {
            var value;
            this.required = (getValue.call(resource, hydra.required) || this.required) || false;
            this.minOccurances = (this.required ? 1 : 0);
            this.maxOccurances = (((value = getValue.call(resource, ursa.singleValue)) !== undefined ? value : false) ? 1 : this.maxOccurances);
        }
    };
    TypeConstrainedApiMember.prototype = new ApiMember(_ctor);
    TypeConstrainedApiMember.prototype.constructor = TypeConstrainedApiMember;
    /**
     * Min occurances of the instance.
     * @memberof ursa.model.TypeConstrainedApiMember
     * @instance
     * @public
     * @member {number} minOccurances
     * @default 0
     */
    TypeConstrainedApiMember.prototype.minOccurances = 0;
    /**
     * Max occurances of the instance.
     * @memberof ursa.model.TypeConstrainedApiMember
     * @instance
     * @public
     * @member {number} maxOccurances
     * @default Number.MAX_VALUE
     */
    TypeConstrainedApiMember.prototype.maxOccurances = Number.MAX_VALUE;
    /**
     * Marks that an instance is required.
     * @memberof ursa.model.TypeConstrainedApiMember
     * @instance
     * @public
     * @member {boolean} required
     * @default false
     */
    TypeConstrainedApiMember.prototype.required = false;
    TypeConstrainedApiMember.prototype.clone = function(source) {
        ApiMember.prototype.clone.apply(this, arguments);
        this.required = source.required;
        this.minOccurances = source.minOccurances;
        this.maxOccurances = source.maxOccurances;
    };

    /**
     * Marks an API member as a range type constrained.
     * @memberof ursa.model
     * @name RangeTypeConstrainedApiMember
     * @public
     * @extends {ursa.model.TypeConstrainedApiMember}
     * @class
     * @param {ursa.model.ApiMember} owner Owner if this instance being created.
     * @param {object} [resource] JSON-LD resource describing this API member.
     * @param {object} [graph] JSON-LD graph of resources.
     */
    var RangeTypeConstrainedApiMember = namespace.RangeTypeConstrainedApiMember = function(owner, resource, graph) {
        TypeConstrainedApiMember.prototype.constructor.apply(this, arguments);
        if ((arguments.length === 1) && (arguments[0] instanceof RangeTypeConstrainedApiMember)) {
            RangeTypeConstrainedApiMember.prototype.clone.call(this, owner);
            return;
        }

        if ((arguments.length === 0) || (arguments[0] !== _ctor)) {
            var property = resource[hydra.property];
            if ((property === undefined) || (property === null) || (!(property instanceof Array)) || (property.length === 0)) {
                throw new Error(invalidArgumentPassed.replace("{0}", "resource"));
            }

            property = graph.getById(this.property = property[0]["@id"] || this.property);
            var range = (property[rdfs.range] ? property[rdfs.range][0] : null);
            if (range !== null) {
                range = graph.getById(range["@id"]);
                var value;
                this.maxOccurances = (((value = getValue.call(range, ursa.singleValue)) !== undefined ? value : false) ? 1 : this.maxOccurances);
                this.range = getClass.call(range, this, graph);
            }
        }
    };
    RangeTypeConstrainedApiMember.prototype = new TypeConstrainedApiMember(_ctor);
    RangeTypeConstrainedApiMember.prototype.constructor = RangeTypeConstrainedApiMember;
    /**
     * Range of values.
     * @memberof ursa.model.RangeTypeConstrainedApiMember
     * @instance
     * @public
     * @member {ursa.model.ApiMember} range
     */
    RangeTypeConstrainedApiMember.prototype.range = null;
    /**
     * Target instance property.
     * @memberof ursa.model.RangeTypeConstrainedApiMember
     * @instance
     * @public
     * @member {string} property
     */
    RangeTypeConstrainedApiMember.prototype.property = null;
    /**
     * Searches the collection for a member with a given property value.
     * @memberof ursa.model.RangeTypeConstrainedApiMember
     * @instance
     * @public
     * @method propertyName
     * @param {ursa.model.Operation} operation Operation in which of context to create a property name. Operation is used to determine whether it accepts RDF payloads or not.
     * @returns {string} Name of the property suitable for given operation.
     */
    RangeTypeConstrainedApiMember.prototype.propertyName = function(operation) {
        if (!operation.isRdf) {
            var parts = this.property.split(/[^a-zA-Z0-9_]/);
            return parts[parts.length - 1];
        }

        return this.property;
    };
    RangeTypeConstrainedApiMember.prototype.clone = function(source) {
        TypeConstrainedApiMember.prototype.clone.apply(this, arguments);
        this.range = (source.range === null ? null : (source.range instanceof Class ? new Class(source.range) : new DataType(source.range)));
        this.property = source.property;
    };

    /**
     * Describes a class' supported property.
     * @memberof ursa.model
     * @name SupportedProperty
     * @public
     * @extends {ursa.model.RangeTypeConstrainedApiMember}
     * @class
     * @param {ursa.model.ApiMember} owner Owner if this instance being created.
     * @param {object} [resource] JSON-LD resource describing this API member.
     * @param {object} [graph] JSON-LD graph of resources.
     */
    var SupportedProperty = namespace.SupportedProperty = function(owner, supportedProperty, graph) {
        RangeTypeConstrainedApiMember.prototype.constructor.apply(this, arguments);
        if ((arguments.length === 1) && (arguments[0] instanceof SupportedProperty)) {
            this.supportedOperations = [];
            SupportedProperty.prototype.clone.call(this, owner);
            return;
        }

        if ((arguments.length === 0) || (arguments[0] !== _ctor)) {
            var property = graph.getById(this.property);
            this.key = (property["@type"] || []).indexOf(owl.InverseFunctionalProperty) !== -1;
            this.label = (this.label || getValue.call(property, rdfs.label)) || "";
            this.description = (this.description || getValue.call(property, rdfs.comment)) || "";
            this.readable = getValue.call(supportedProperty, hydra.readable) || true;
            this.writeable = getValue.call(supportedProperty, hydra.writeable) || true;
            this.supportedOperations = [];
            getOperations.call(this, supportedProperty, graph, hydra.operation);
        }
    };
    SupportedProperty.prototype = new RangeTypeConstrainedApiMember(_ctor);
    SupportedProperty.prototype.constructor = SupportedProperty;
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
    SupportedProperty.prototype.clone = function(source) {
        if (arguments.length === 0) {
            return new SupportedProperty(this);
        }

        RangeTypeConstrainedApiMember.prototype.clone.apply(this, arguments);
        this.readable = source.readable;
        this.writeable = source.writeable;
        this.required = source.required;
        for (var index = 0; index < source.supportedOperations.length; index++) {
            this.supportedOperations.push(new Operation(source.supportedOperations[index]));
        }
    };
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
     * Describes an IRI template mapping.
     * @memberof ursa.model
     * @name Mapping
     * @public
     * @extends {ursa.model.TypeConstrainedApiMember}
     * @class
     * @param {ursa.model.ApiMember} owner Owner if this instance being created.
     * @param {object} [resource] JSON-LD resource describing this API member.
     */
    var Mapping = namespace.Mapping = function(owner, mapping) {
        TypeConstrainedApiMember.prototype.constructor.apply(this, arguments);
        if ((arguments.length === 1) && (arguments[0] instanceof Mapping)) {
            Mapping.prototype.clone.call(this, owner);
            return;
        }

        if ((arguments.length === 0) || (arguments[0] !== _ctor)) {
            this.variable = getValue.call(mapping, hydra.variable);
            this.property = getValue.call(mapping, hydra.property);
        }
    };
    Mapping.prototype = new TypeConstrainedApiMember(_ctor);
    Mapping.prototype.constructor = Mapping;
    /**
     * Name of the template variable.
     * @memberof ursa.model.Mapping
     * @instance
     * @public
     * @member {string} variable
     */
    Mapping.prototype.variable = null;
    /**
     * Max occurances of the instance.
     * @memberof ursa.model.Mapping
     * @instance
     * @public
     * @member {number} maxOccurances
     * @default 1
     */
    Mapping.prototype.maxOccurances = 1;
    /**
     * Property bound on the server side associated with this variable mapping.
     * @memberof ursa.model.Mapping
     * @instance
     * @public
     * @member {string} property
     */
    Mapping.prototype.property = null;
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
    Mapping.prototype.clone = function(source) {
        if (arguments.length === 0) {
            return new Mapping(this);
        }

        TypeConstrainedApiMember.prototype.clone.apply(this, arguments);
        this.variable = source.variable;
        this.property = source.property;
    };

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
    var Operation = namespace.Operation = function(owner, supportedOperation, template, graph) {
        ApiMember.prototype.constructor.apply(this, arguments);
        if ((arguments.length === 1) && (arguments[0] instanceof Operation)) {
            this.returns = [];
            this.expects = [];
            this.statusCodes = [];
            this.mediaTypes = [];
            this.methods = [];
            if (owner.mappings !== null) {
                this.mappings = new ApiMemberCollection(this);
            }

            Operation.prototype.clone.call(this, owner);
            return;
        }

        if ((arguments.length === 0) || (arguments[0] !== _ctor)) {
            var index;
            if ((template) && (template[hydra.template])) {
                if ((this.url = template[hydra.template][0]["@value"]).match(/^[a-zA-Z][a-zA-Z0-9\+\-\.]*/) === null) {
                    var apiDocumentation = this.apiDocumentation;
                    var entryPoint = ((apiDocumentation.entryPoints) && (apiDocumentation.entryPoints.length > 0) ? apiDocumentation.entryPoints[0] : window.location.href);
                    this.url = (entryPoint.charAt(entryPoint.length - 1) === "/" ? entryPoint.substr(0, entryPoint.length - 1) : entryPoint) + this.url;
                }

                this.mappings = new ApiMemberCollection(this);
                for (index = 0; index < template[hydra.mapping].length; index++) {
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

            var setupTypeCollection = function(source, target) {
                if (!source) {
                    return;
                }

                for (index = 0; index < source.length; index++) {
                    target.push(getClass.call(graph.getById(source[index]["@id"]), this, graph));
                }
            };
            setupTypeCollection.call(this, supportedOperation[hydra.returns], this.returns);
            setupTypeCollection.call(this, supportedOperation[hydra.expects], this.expects);
        }
    };
    Operation.prototype = new ApiMember(_ctor);
    Operation.prototype.constructor = Operation;
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
     * @member {Array.<ursa.model.TypeConstrainedApiMember>} expects
     */
    Operation.prototype.expects = null;
    /**
     * List of returned types.
     * @memberof ursa.model.Mapping
     * @instance
     * @public
     * @member {Array.<ursa.model.TypeConstrainedApiMember>} returns
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

                    if ((this.isRdf) && (propertyValue !== null)) {
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
    Operation.prototype.clone = function(source) {
        if (arguments.length === 0) {
            return new Operation(this);
        }

        ApiMember.prototype.clone.apply(this, arguments);
        var index;
        this.url = source.url;
        for (index = 0; index < source.methods.length; index++) {
            this.methods.push(source.methods[index]);
        }

        for (index = 0; index < source.statusCodes.length; index++) {
            this.statusCodes.push(source.statusCodes[index]);
        }

        for (index = 0; index < source.mediaTypes.length; index++) {
            this.mediaTypes.push(source.mediaTypes[index]);
        }

        for (index = 0; index < source.returns.length; index++) {
            this.returns.push(source.returns[index] instanceof Class ? new Class(source.returns[index]) : new DataType(source.returns[index]));
        }

        for (index = 0; index < source.expects.length; index++) {
            this.expects.push(source.expects[index] instanceof Class ? new Class(source.expects[index]) : new DataType(source.expects[index]));
        }

        if (source.mappings !== null) {
            for (index = 0; index < source.mappings.length; index++) {
                this.mappings.push(new Mapping(source.mappings[index]));
            }
        }
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

    /**
     * Describes a datatype.
     * @memberof ursa.model
     * @name DataType
     * @public
     * @extends {ursa.model.TypeConstrainedApiMember}
     * @class
     * @param {ursa.model.ApiMember} owner Owner if this instance being created.
     * @param {object} [resource] JSON-LD resource describing this API member.
     */
    var DataType = namespace.DataType = function() {
        TypeConstrainedApiMember.prototype.constructor.apply(this, arguments);
    };
    DataType.prototype = new TypeConstrainedApiMember(_ctor);
    DataType.prototype.constructor = DataType;
    DataType.prototype.clone = function() {
        if (arguments.length === 0) {
            return new DataType(this);
        }

        TypeConstrainedApiMember.prototype.clone.apply(this, arguments);
    };

    /**
     * Describes a class.
     * @memberof ursa.model
     * @name Class
     * @public
     * @extends {ursa.model.TypeConstrainedApiMember}
     * @class
     * @param {ursa.model.ApiMember} owner Owner if this instance being created.
     * @param {object} [supportedOperation] JSON-LD resource describing this API member.
     * @param {object} [graph] JSON-LD graph of resources.
     */
    var Class = namespace.Class = function(owner, supportedClass, graph) {
        TypeConstrainedApiMember.prototype.constructor.apply(this, arguments);
        var index;
        if ((arguments.length === 1) && (arguments[0] instanceof Class)) {
            this.supportedProperties = new ApiMemberCollection(this);
            this.supportedOperations = [];
            Class.prototype.clone.call(this, owner);
            return;
        }

        if ((arguments.length === 0) || (arguments[0] !== _ctor)) {
            this.supportedProperties = new ApiMemberCollection(this);
            this.supportedOperations = [];
            var subClassOf = supportedClass[rdfs.subClassOf] || [];
            if (supportedClass[hydra.supportedProperty]) {
                for (index = 0; index < supportedClass[hydra.supportedProperty].length; index++) {
                    var supportedPropertyResource = graph.getById(supportedClass[hydra.supportedProperty][index]["@id"]);
                    var supportedProperty = new SupportedProperty(this, supportedPropertyResource, graph);
                    for (var restrictionIndex = 0; restrictionIndex < subClassOf.length; restrictionIndex++) {
                        var restriction = graph.getById(subClassOf[restrictionIndex]["@id"]);
                        if ((restriction["@type"]) && (restriction["@type"].indexOf(owl.Restriction) !== -1) &&
                            (getValue.call(restriction, owl.onProperty) === supportedProperty.property)) {
                            var maxCardinality = getValue.call(restriction, owl.maxCardinality);
                            supportedProperty.maxOccurances = (maxCardinality !== null ? maxCardinality : supportedProperty.maxOccurances);
                        }
                    }

                    this.supportedProperties.push(supportedProperty);
                }
            }

            getOperations.call(this, supportedClass, graph);
        }
    };
    Class.prototype = new TypeConstrainedApiMember(_ctor);
    Class.prototype.constructor = Class;
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
        return (entityFormat === EntityFormat.RDF ? classCreateRdfInstance.call(this, operation) : classCreateJsonInstance.call(this, operation));
    };
    var classCreateJsonInstance = function(operation) {
        var result = {};
        for (var index = 0; index < this.supportedProperties.length; index++) {
            var supportedProperty = this.supportedProperties[index];
            var newValue = result[supportedProperty.propertyName(operation)] = (supportedProperty.maxOccurances > 1 ? [] : null);
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
    var classCreateRdfInstance = function() {
        var result = { "@id": "_:bnode" + Math.random().toString().replace(".", "").substr(1) };
        if (this.id.indexOf("_") !== 0) {
            result["@type"] = this.id;
        }

        for (var index = 0; index < this.supportedProperties.length; index++) {
            var supportedProperty = this.supportedProperties[index];
            var newValue = result[supportedProperty.property] = [];
            if (supportedProperty.minOccurances > 0) {
                var value = supportedProperty.createInstance();
                if (value !== null) {
                    for (var itemIndex = 0; itemIndex < supportedProperty.minOccurances; itemIndex++) {
                        newValue.push({ "@value": value, "@type": supportedProperty.range.id });
                    }
                }
            }

            if ((newValue.length === 0) && (supportedProperty.maxOccurances ===1 )) {
                delete result[supportedProperty.property];
            }
        }

        return result;
    };
    Class.prototype.clone = function(source) {
        if (arguments.length === 0) {
            return new Class(this);
        }

        TypeConstrainedApiMember.prototype.clone.apply(this, arguments);
        var index;
        for (index = 0; index < source.supportedProperties.length; index++) {
            this.supportedProperties.push(new SupportedProperty(source.supportedProperties[index]));
        }

        for (index = 0; index < source.supportedOperations.length; index++) {
            this.supportedOperations.push(new Operation(source.supportedOperations[index]));
        }
    };

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
    EntityFormat.prototype.constructor = EntityFormat;
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

    /**
     * Provides an enumeration of HTTP status codes.
     * @memberof ursa.model
     * @name HttpStatusCodes
     * @public
     * @readonly
     * @enum {number}
     */
    var HttpStatusCodes = namespace.HttpStatusCodes = {
        /** Continue indicates that the client can continue with its request. */
        Continue: 100,
        /** SwitchingProtocols indicates that the protocol version or protocol is being changed. */
        SwitchingProtocols: 101,

        /** OK indicates that the request succeeded and that the requested information is in the response. This is the most common status code to receive. */
        OK: 200,
        /** Created indicates that the request resulted in a new resource created before the response was sent. */
        Created: 201,
        /** Accepted indicates that the request has been accepted for further processing. */
        Accepted: 202,
        /** NonAuthoritativeInformation indicates that the returned metainformation is from a cached copy instead of the origin server and therefore may be incorrect. */
        NonAuthoritativeInformation: 203,
        /** NoContent indicates that the request has been successfully processed and that the response is intentionally blank. */
        NoContent: 204,
        /** ResetContent indicates that the client should reset (not reload) the current resource. */
        ResetContent: 205,
        /** PartialContent indicates that the response is a partial response as requested by a GET request that includes a byte range. */
        PartialContent: 206,

        /** MultipleChoices indicates that the requested information has multiple representations. The default action is to treat this status as a redirect and follow the contents of the Location header associated with this response. */
        MultipleChoices: 300,
        /** Ambiguous indicates that the requested information has multiple representations. The default action is to treat this status as a redirect and follow the contents of the Location header associated with this response. */
        Ambiguous: 300,
        /** Moved indicates that the requested information has been moved to the URI specified in the Location header. The default action when this status is received is to follow the Location header associated with the response. When the original request method was POST, the redirected request will use the GET method. */
        Moved: 301,
        /** MovedPermanently indicates that the requested information has been moved to the URI specified in the Location header. The default action when this status is received is to follow the Location header associated with the response. */
        MovedPermanently: 301,
        /** Redirect indicates that the requested information is located at the URI specified in the Location header. The default action when this status is received is to follow the Location header associated with the response. When the original request method was POST, the redirected request will use the GET method. */
        Redirect: 302,
        /** Found indicates that the requested information is located at the URI specified in the Location header. The default action when this status is received is to follow the Location header associated with the response. When the original request method was POST, the redirected request will use the GET method. */
        Found: 302,
        /** SeeOther automatically redirects the client to the URI specified in the Location header as the result of a POST. The request to the resource specified by the Location header will be made with a GET. */
        SeeOther: 303,
        /** RedirectMethod automatically redirects the client to the URI specified in the Location header as the result of a POST. The request to the resource specified by the Location header will be made with a GET. */
        RedirectMethod: 303,
        /** NotModified indicates that the client's cached copy is up to date. The contents of the resource are not transferred. */
        NotModified: 304,
        /** UseProxy indicates that the request should use the proxy server at the URI specified in the Location header. */
        UseProxy: 305,
        /** Unused is a proposed extension to the HTTP/1.1 specification that is not fully specified. */
        Unused: 306,
        /** RedirectKeepVerb indicates that the request information is located at the URI specified in the Location header. The default action when this status is received is to follow the Location header associated with the response. When the original request method was POST, the redirected request will also use the POST method. */
        RedirectKeepVerb: 307,
        /** TemporaryRedirect indicates that the request information is located at the URI specified in the Location header. The default action when this status is received is to follow the Location header associated with the response. When the original request method was POST, the redirected request will also use the POST method. */
        TemporaryRedirect: 307,

        /** BadRequest indicates that the request could not be understood by the server. BadRequest is sent when no other error is applicable, or if the exact error is unknown or does not have its own error code. */
        BadRequest: 400,
        /** Unauthorized indicates that the requested resource requires authentication. The WWW-Authenticate header contains the details of how to perform the authentication. */
        Unauthorized: 401,
        /** PaymentRequired is reserved for future use. */
        PaymentRequired: 402,
        /** Forbidden indicates that the server refuses to fulfill the request. */
        Forbidden: 403,
        /** NotFound indicates that the requested resource does not exist on the server. */
        NotFound: 404,
        /** MethodNotAllowed indicates that the request method (POST or GET) is not allowed on the requested resource. */
        MethodNotAllowed: 405,
        /** NotAcceptable indicates that the client has indicated with Accept headers that it will not accept any of the available representations of the resource. */
        NotAcceptable: 406,
        /** ProxyAuthenticationRequired indicates that the requested proxy requires authentication. The Proxy-authenticate header contains the details of how to perform the authentication. */
        ProxyAuthenticationRequired: 407,
        /** RequestTimeout indicates that the client did not send a request within the time the server was expecting the request. */
        RequestTimeout: 408,
        /** Conflict indicates that the request could not be carried out because of a conflict on the server. */
        Conflict: 409,
        /** Gone indicates that the requested resource is no longer available. */
        Gone: 410,
        /** LengthRequired indicates that the required Content-length header is missing. */
        LengthRequired: 411,
        /** PreconditionFailed indicates that a condition set for this request failed, and the request cannot be carried out. Conditions are set with conditional request headers like If-Match, If-None-Match, or If-Unmodified-Since. */
        PreconditionFailed: 412,
        /** RequestEntityTooLarge indicates that the request is too large for the server to process. */
        RequestEntityTooLarge: 413,
        /** RequestUriTooLong indicates that the URI is too long. */
        RequestUriTooLong: 414,
        /** UnsupportedMediaType indicates that the request is an unsupported type. */
        UnsupportedMediaType: 415,
        /** RequestedRangeNotSatisfiable indicates that the range of data requested from the resource cannot be returned, either because the beginning of the range is before the beginning of the resource, or the end of the range is after the end of the resource. */
        RequestedRangeNotSatisfiable: 416,
        /** ExpectationFailed indicates that an expectation given in an Expect header could not be met by the server. */
        ExpectationFailed: 417,
        /** UpgradeRequired indicates that the client should switch to a different protocol such as TLS/1.0. */
        UpgradeRequired: 426,

        /** InternalServerError indicates that a generic error has occurred on the server. */
        InternalServerError: 500,
        /** NotImplemented indicates that the server does not support the requested function. */
        NotImplemented: 501,
        /** BadGateway indicates that an intermediate proxy server received a bad response from another proxy or the origin server. */
        BadGateway: 502,
        /** ServiceUnavailable indicates that the server is temporarily unavailable, usually due to high load or maintenance. */
        ServiceUnavailable: 503,
        /** GatewayTimeout indicates that an intermediate proxy server timed out while waiting for a response from another proxy or the origin server. */
        GatewayTimeout: 504,
        /** HttpVersionNotSupported indicates that the requested HTTP version is not supported by the server. */
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
            throw new Error(invalidArgumentPassed.replace("{0}", "graph"));
        }

        ApiMember.prototype.constructor.call(this, null, null);
        if ((arguments.length === 0) || (arguments[0] !== _ctor)) {
            var index;
            this.owner = null;
            this.entryPoints = [];
            this.supportedClasses = [];
            graph.getById = function(id) { return getById.call(graph, id); };
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
                    this.supportedClasses.push(new Class(this, graph.getById(apiDocumentation[hydra.supportedClass][index]["@id"]), graph));
                }
            }
        }
    };
    ApiDocumentation.prototype = new ApiMember(_ctor);
    ApiDocumentation.prototype.constructor = ApiDocumentation;
    /**
     * List of supported classes.
     * @memberof ursa.model.ApiDocumentation
     * @instance
     * @public
     * @member {Array.<ursa.model.Class>} supportedClasses
     */
    ApiDocumentation.prototype.supportedClasses = null;
    /**
     * List of entry point Urls.
     * @memberof ursa.model.ApiDocumentation
     * @instance
     * @public
     * @member {Array.<string>} entryPoints
     */
    ApiDocumentation.prototype.entryPoints = null;
    /**
     * Throws an exception as this method is not supported.
     * @memberof ursa.model.ApiDocumentation
     * @instance
     * @public
     * @method clone
     */
    ApiDocumentation.prototype.clone = function() { throw "Not supported."; };
}(namespace("ursa.model")));