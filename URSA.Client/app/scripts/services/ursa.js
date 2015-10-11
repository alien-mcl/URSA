/*globals xsd, rdf, rdfs,owl, hydra, ursa, shacl, guid, jsonld */
(function() {
"use strict";
window.xsd = new String("http://www.w3.org/2001/XMLSchema#"); //jshint ignore:line
xsd.string = xsd + "string";
xsd.boolean = xsd + "boolean";
xsd.byte = xsd + "byte";
xsd.short = xsd + "short";
xsd.int = xsd + "int";
xsd.long = xsd + "long";
xsd.float = xsd + "float";
xsd.double = xsd + "double";
xsd.dateTime = xsd + "dateTime";
window.rdf = new String("http://www.w3.org/1999/02/22-rdf-syntax-ns#"); //jshint ignore:line
rdf.Property = rdf + "Property";
window.rdfs = new String("http://www.w3.org/2000/01/rdf-schema#"); //jshint ignore:line
rdfs.Class = rdfs + "Class";
rdfs.subClassOf = rdfs + "subClassOf";
rdfs.range = rdfs + "range";
rdfs.label = rdfs + "label";
rdfs.comment = rdfs + "comment";
window.owl = new String("http://www.w3.org/2002/07/owl#"); //jshint ignore:line
window.hydra = new String("http://www.w3.org/ns/hydra/core#"); //jshint ignore:line
hydra.Resource = hydra + "Resource";
hydra.Class = hydra + "Class";
hydra.Operation = hydra + "Operation";
hydra.SupportedProperty = hydra + "SupportedProperty";
hydra.Link = hydra + "Link";
hydra.TemplatedLink = hydra + "TemplatedLink";
hydra.IriTemplate = hydra + "IriTemplate";
hydra.IriTemplateMapping = hydra + "IriTemplateMapping";
hydra.ApiDocumentation = hydra + "ApiDocumentation";
hydra.CreateResourceOperation = hydra + "CreateResourceOperation";
hydra.ReplaceResourceOperation = hydra + "ReplaceResourceOperation";
hydra.DeleteResourceOperation = hydra + "DeleteResourceOperation";
hydra.entrypoint = hydra + "entrypoint";
hydra.property = hydra + "property";
hydra.supportedProperty = hydra + "supportedProperty";
hydra.supportedOperation = hydra + "supportedOperation";
hydra.readable = hydra + "readable";
hydra.writeable = hydra + "writeable";
hydra.required = hydra + "required";
hydra.expects = hydra + "expects";
hydra.returns = hydra + "returns";
hydra.method = hydra + "method";
hydra.statusCode = hydra + "statusCode";
hydra.operation = hydra + "operation";
hydra.template = hydra + "template";
hydra.mapping = hydra + "mapping";
hydra.variable = hydra + "variable";
hydra.title = hydra + "title";
hydra.description = hydra + "description";
window.ursa = new String("http://github.io/ursa/vocabulary#"); //jshint ignore:line
window.shacl = new String("http://www.w3.org/ns/shacl#"); //jshint ignore:line
window.guid = new String("http://openguid.net/rdf#"); //jshint ignore:line
}());
(function() {
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
        case xsd.short:
        case xsd.int:
        case xsd.long:
            return (this === "" ? 0 : parseInt(this));
        case xsd.float:
        case xsd.double:
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

    var result = this[property][0]["@value"] || this[property][0]["@id"];
    if (typeof(this[property][0]["@type"]) === "string") {
        result = parseValue.call(result, this[property][0]["@type"]);
    }

    return result;
};

var getValues = function(property) {
    var result = [];
    if (this[property]) {
        for (var index = 0; index < this[property].length; index++) {
            var value = this[property][index]["@value"] || this[property][index]["@id"];
            if (typeof(this[property][index]["@type"]) === "string") {
                value = parseValue.call(value, this[property][index]["@type"]);
            }

            if (value !== null) {
                result.push(value);
            }
        }
    }

    return result;
};

var getClass = function(owner, graph, supportedClasses, supportedProperties, supportedOperations, templatedLinks) {
    var result = this;
    if ((result["@id"].charAt(0) === "_") && (getValue.call(result, rdfs.subClassOf))) {
        result = graph.getById(getValue.call(result, rdfs.subClassOf));
    }

    if ((result["@id"].indexOf(xsd) === -1) && (result["@id"].indexOf(guid) === -1)) {
        if (((owner instanceof SupportedProperty) || (owner instanceof Operation)) && (owner.owner.id === result["@id"])) {
            return owner.owner;
        }

        return new Class(owner, result, graph, supportedClasses, supportedProperties, supportedOperations, templatedLinks);
    }
    else {
        return result["@id"];
    }
};

var ApiMember = function(owner, resource) {
    if ((arguments.length === 0) || (arguments[0] !== _ctor)) {
        this.owner = owner || null;
        if ((resource !== null) && (typeof(resource) === "object")) {
            this.id = resource["@id"];
            this.label = getValue.call(resource, rdfs.label) || "";
            this.description = getValue.call(resource, rdfs.comment) || "";
        }
    }
};
ApiMember.prototype.constructor = ApiMember;
ApiMember.prototype.owner = null;
ApiMember.prototype.id = null;
ApiMember.prototype.label = null;
ApiMember.prototype.description = null;
Object.defineProperty(ApiMember.prototype, "apiDocumentation", { get: function() {
    var result = this;
    while ((result !== null) && (!(result instanceof ApiDocumentation))) {
        result = result.owner;
    }

    return result;
} });

var SupportedProperty = function(owner, supportedProperty, graph, supportedClasses, supportedProperties, supportedOperations, templatedLinks) {
    ApiMember.prototype.constructor.call(this, owner, supportedProperty);
    var property = supportedProperty[hydra.property];
    if ((property === undefined) || (property === null) || (!(property instanceof Array)) || (property.length === 0)) {
        throw new Error(invalidArgumentPassed.replace("{0}", supportedProperty));
    }

    property = graph.getById(property[0]["@id"]);
    this.label = (this.label || getValue.call(property, rdfs.label)) || "";
    this.description = (this.description || getValue.call(property, rdfs.comment)) || "";
    this.readable = getValue.call(supportedProperty, hydra.readable) || true;
    this.writeable = getValue.call(supportedProperty, hydra.writeable) || true;
    this.required = getValue.call(supportedProperty, hydra.required) || false;
    this.minOccurances = (this.required ? 1 : 0);
    var range = (property[rdfs.range] ? property[rdfs.range][0] : null);
    if (range !== null) {
        range = graph.getById(range["@id"]);
        this.maxOccurances = (getValue.call(range, ursa.singleValue) || false ? 1 : Number.MAX_VALUE);
        this.range = getClass.call(range, this, graph, supportedClasses, supportedProperties, supportedOperations, templatedLinks);
    }
};
SupportedProperty.prototype = new ApiMember(_ctor);
SupportedProperty.prototype.constructor = SupportedProperty;
SupportedProperty.prototype.readable = true;
SupportedProperty.prototype.writeable = true;
SupportedProperty.prototype.required = true;
SupportedProperty.prototype.minOccurances = 1;
SupportedProperty.prototype.maxOccurances = Number.MAX_VALUE;
SupportedProperty.prototype.range = null;

var Mapping = function(owner, mapping, graph, supportedClasses, supportedProperties, supportedOperations, templatedLinks) {
    ApiMember.prototype.constructor.call(this, owner);
    if ((!mapping[hydra.property]) || (!mapping[hydra.variable])) {
        throw new Error(invalidArgumentPassed.replace("{0}", "mapping"));
    }

    var property = graph.getById(mapping[hydra.property][0]["@id"]);
    this.property = property["@id"];
    this.variable = getValue.call(mapping, hydra.variable);
    this.required = getValue.call(mapping, hydra.required) || true;
    this.minOccurances = (this.required ? 1 : 0);
    var range = (property[rdfs.range] ? property[rdfs.range][0] : null);
    if (range !== null) {
        range = graph.getById(range["@id"]);
        this.maxOccurances = (getValue.call(range, ursa.singleValue) || false ? 1 : Number.MAX_VALUE);
        this.range = getClass.call(range, this, graph, supportedClasses, supportedProperties, supportedOperations, templatedLinks);
    }
};
Mapping.prototype = new ApiMember(_ctor);
Mapping.prototype.constructor = Mapping;
Mapping.prototype.variable = null;
Mapping.prototype.property = null;
Mapping.prototype.minOccurances = 0;
Mapping.prototype.maxOccuranges = 1;
Mapping.prototype.range = null;

var Operation = function(owner, supportedOperation, template, graph, supportedClasses, supportedProperties, supportedOperations, templatedLinks) {
    ApiMember.prototype.constructor.call(this, owner, supportedOperation);
    if ((template) && (template[hydra.template])) {
        if ((this.url = template[hydra.template][0]["@value"]).match(/^[a-zA-Z][a-zA-Z0-9\+\-\.]*/) === null) {
            var apiDocumentation = this.apiDocumentation;
            var entryPoint = apiDocumentation.entryPoints[0];
            this.url = (entryPoint.charAt(entryPoint.length - 1) === "/" ? entryPoint.substr(0, entryPoint.length - 1) : entryPoint) + this.url;
        }

        this.mappings = [];
        for (var index = 0; index < template[hydra.mapping].length; index++) {
            this.mappings.push(new Mapping(this, graph.getById(template[hydra.mapping][index]["@id"]), graph, supportedClasses, supportedProperties, supportedOperations, templatedLinks));
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
    var setupTypeCollection = function(source, target) {
        if (!source) {
            return;
        }

        for (var index = 0; index < source.length; index++) {
            target.push(getClass.call(graph.getById(source[index]["@id"]), this, graph, supportedClasses, supportedProperties, supportedOperations, templatedLinks));
        }
    };
    setupTypeCollection.call(this, supportedOperation[hydra.returns], this.returns);
    setupTypeCollection.call(this, supportedOperation[hydra.expects], this.expects);
};
Operation.prototype = new ApiMember(_ctor);
Operation.prototype.constructor = Operation;
Operation.prototype.methods = null;
Operation.prototype.expects = null;
Operation.prototype.returns = null;
Operation.prototype.statusCodes = null;
Operation.prototype.url = null;
Operation.prototype.mappings = null;

var Class = function(owner, supportedClass, graph, supportedClasses, supportedProperties, supportedOperations, templatedLinks) {
    ApiMember.prototype.constructor.call(this, owner, supportedClass);
    var index;
    this.supportedProperties = [];
    this.supportedOperations = [];
    if (supportedClass[hydra.supportedProperty]) {
        for (index = 0; index < supportedClass[hydra.supportedProperty].length; index++) {
            var supportedProperty = supportedProperties.getById(supportedClass[hydra.supportedProperty][index]["@id"]);
            this.supportedProperties.push(new SupportedProperty(this, supportedProperty, graph, supportedClasses, supportedProperties, supportedOperations, templatedLinks));
        }
    }

    if (supportedClass[hydra.supportedOperation]) {
        for (index = 0; index < supportedClass[hydra.supportedOperation].length; index++) {
            var supportedOperation = supportedOperations.getById(supportedClass[hydra.supportedOperation][index]["@id"]);
            this.supportedOperations.push(new Operation(this, supportedOperation, null, graph, supportedClasses, supportedProperties, supportedOperations, templatedLinks));
        }
    }

    for (var propertyName in supportedClass) {
        if ((supportedClass.hasOwnProperty(propertyName)) && (propertyName.match(/^[a-zA-Z][a-zA-Z0-9\+\-\.]*:/) !== null) &&
            (propertyName.indexOf(xsd) === -1) && (propertyName.indexOf(rdf) === -1) && (propertyName.indexOf(rdfs) === -1) &&
            (propertyName.indexOf(owl) === -1) && (propertyName.indexOf(guid) === -1) && (propertyName.indexOf(hydra) === -1) &&
            (propertyName.indexOf(shacl) === -1) && (propertyName.indexOf(ursa) === -1)) {
            var property = graph.getById(propertyName);
            if ((!property) || (property["@type"].indexOf(hydra.TemplatedLink) === -1) || (!property[hydra.operation])) {
                continue;
            }

            var operation = graph.getById(property[hydra.operation][0]["@id"]);
            var templates = supportedClass[propertyName];
            for (index = 0; index < templates.length; index++) {
                var template = graph.getById(templates[index]["@id"]);
                if (template["@type"].indexOf(hydra.IriTemplate) !== -1) {
                    this.supportedOperations.push(new Operation(this, operation, template, graph, supportedClasses, supportedProperties, supportedOperations, templatedLinks));
                }
            }
        }
    }
};
Class.prototype = new ApiMember(_ctor);
Class.prototype.constructor = Class;
Class.prototype.supportedProperties = null;

var ApiDocumentation = function(graph) {
    if ((graph === undefined) || (graph === null) || (!(graph instanceof Array))) {
        throw new Error(invalidArgumentPassed.replace("{0}", "graph"));
    }

    ApiMember.prototype.constructor.call(this, null, null);
    var index;
    this.owner = null;
    this.entryPoints = [];
    this.supportedClasses = [];
    graph.getById = function(id) { return getById.call(graph, id); };
    var supportedClasses = [];
    supportedClasses.getById = function(id) { return getById.call(supportedClasses, id); };
    var supportedOperations = [];
    supportedOperations.getById = function(id) { return getById.call(supportedOperations, id); };
    var supportedProperties = [];
    supportedProperties.getById = function(id) { return getById.call(supportedProperties, id); };
    var templatedLinks = [];
    templatedLinks.getById = function(id) { return getById.call(templatedLinks, id); };
    for (index = 0; index < graph.length; index++) {
        var resource = graph[index];
        if ((resource["@type"].indexOf(hydra.Class) !== -1) && (resource["@id"].charAt(0) !== "_") &&
            (resource["@id"].indexOf(xsd) === -1) && (resource["@id"].indexOf(guid) === -1)) {
            supportedClasses.push(resource);
        }
        else if (resource["@type"].indexOf(hydra.SupportedProperty) !== -1) {
            supportedProperties.push(resource);
        }
        else if (resource["@type"].indexOf(hydra.TemplatedLink) !== -1) {
            templatedLinks.push(resource);
        }
        else if (resource["@type"].indexOf(hydra.Operation) !== -1) {
            supportedOperations.push(resource);
        }
        else if (resource["@type"].indexOf(hydra.ApiDocumentation) !== -1) {
            this.id = resource["@id"];
            this.title = getValue.call(resource, hydra.title) || "";
            this.description = getValue.call(resource, hydra.description) || "";
            this.entryPoints = getValues.call(resource, hydra.entrypoint);
            if (this.entryPoints.length === 0) {
                this.entryPoints.push(this.id.match(/^http[s]*:\/\/[^\/]+\//)[0]);
            }
        }
    }

    for (index = 0; index < supportedClasses.length; index++) {
        this.supportedClasses.push(new Class(this, supportedClasses[index], graph, supportedClasses, supportedProperties, supportedOperations));
    }
};
ApiDocumentation.prototype = new ApiMember(_ctor);
ApiDocumentation.prototype.constructor = ApiDocumentation;
ApiDocumentation.prototype.supportedClasses = null;
ApiDocumentation.prototype.entryPoints = null;

var ApiDocumentationService = function($http, $q) {
    if (($http === undefined) || ($http === null)) {
        throw new Error(invalidArgumentPassed.replace("{0}", "$http"));
    }

    if (($q === undefined) || ($q === null)) {
        throw new Error(invalidArgumentPassed.replace("{0}", "$q"));
    }

    this.$http = $http;
    this.$q = $q;
};
ApiDocumentationService.prototype.constructor = ApiDocumentationService;
ApiDocumentationService.prototype.load = function(entryPoint) {
    if ((entryPoint === undefined) || (entryPoint === null)) {
        entryPoint = window.location.href;
    }

    if (entryPoint instanceof String) {
        entryPoint = entryPoint.toString();
    }

    if (typeof(entryPoint) !== "string") {
        throw new Error(invalidArgumentPassed.replace("{0}", "entryPoint"));
    }

    if ((entryPoint.length === 0) || (entryPoint.match(/^http[s]?:\/\//i) === null)) {
        throw new Error(invalidArgumentPassed.replace("{0}", "entryPoint"));
    }

    var deferred = this.$q.defer();
    var onExpanded = function(error, expanded) {
        if (error) {
            throw new Error(error);
        }

        deferred.resolve(new ApiDocumentation(expanded));
    };
    this.$http({ method: "OPTIONS", url: entryPoint, headers: { "Accept": "application/ld+json" } }).
        then(function(response) { jsonld.expand(response.data, onExpanded); }).
        catch(function(response) { deferred.reject(response.data); });
    return deferred.promise;
};
ApiDocumentationService.prototype.$http = null;
ApiDocumentationService.prototype.$q = null;

angular.module("ursa", []).
factory("hydraApiDocumentation", ["$http", "$q", function hydraApiDocumentationFactory($http, $q) {
    return new ApiDocumentationService($http, $q);
}]);
}());