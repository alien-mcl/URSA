/*globals namespace, ursa, xsd, rdf, rdfs, owl, hydra, shacl, guid */
(function(namespace) {
    "use strict";

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

    var getValue = function (property) {
        if ((!this[property]) || (this[property].length === 0)) {
            return null;
        }

        var result = (this[property][0]["@value"] !== undefined ? this[property][0]["@value"] : this[property][0]["@id"]);
        if (this[property][0]["@type"] === undefined) {
            return result;
        }

        if ((this[property][0]["@type"] !== xsd.string) && (typeof (result) === "string")) {
            result = parseValue.call(result, this[property][0]["@type"]);
        }

        return result;
    };

    var getValues = function (property) {
        var result = [];
        if (this[property]) {
            for (var index = 0; index < this[property].length; index++) {
                var value = this[property][index]["@value"] || this[property][index]["@id"];
                if ((this[property][index]["@type"] !== undefined) && (this[property][index]["@type"] !== xsd.string) && (typeof (value) === "string")) {
                    value = parseValue.call(value, this[property][index]["@type"]);
                }

                if (value !== null) {
                    result.push(value);
                }
            }
        }

        return result;
    };

    var isBlankNode = function(resource) { return ((resource ? resource["@id"] : this).indexOf("_:") === 0); };

    var canOverrideSuperClassId = function(superClassId) {
        return (superClassId === null) || ((superClassId === hydra.Collection) && (this["@id"] === rdf.List)) || ((isBlankNode.call(superClassId)) && (!isBlankNode(this)));
    };

    var isRestriction = function(resource) { return !!((resource[owl.onProperty]) || (resource[owl.allValuesFrom])); };

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
            var Ctor = ((targetClass["@id"].indexOf(xsd) === -1) && (targetClass["@id"].indexOf(guid) === -1) ? ursa.model.Class : ursa.model.DataType);
            result = new Ctor(owner.apiDocumentation || owner, targetClass, graph);
        }

        if ($class === targetClass) {
            return result;
        }

        result = result.subClass($class["@id"]);
        ursa.model.Type.prototype.constructor.call(result, owner, $class, graph);
        result.maxOccurances = (isEnumerable ? Number.MAX_VALUE : result.maxOccurances);
        result.isList = isList;
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
                this.supportedOperations.push(new ursa.model.Operation(this, supportedOperation, null, graph));
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
                        this.supportedOperations.push(new ursa.model.Operation(this, operation, template, graph));
                    }
                }
            }
        }
    };

    Object.defineProperty(namespace, "getValue", { enumerable: false, configurable: false, writable: true, value: getValue });
    Object.defineProperty(namespace, "getValues", { enumerable: false, configurable: false, writable: true, value: getValues });
    Object.defineProperty(namespace, "isBlankNode", { enumerable: false, configurable: false, writable: true, value: isBlankNode });
    Object.defineProperty(namespace, "composeClass", { enumerable: false, configurable: false, writable: true, value: composeClass });
    Object.defineProperty(namespace, "getClass", { enumerable: false, configurable: false, writable: true, value: getClass });
    Object.defineProperty(namespace, "getOperations", { enumerable: false, configurable: false, writable: true, value: getOperations });
}(namespace("ursa.model")));