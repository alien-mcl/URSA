/*globals namespace, ursa, xsd, guid */
(function(namespace) {
    "use strict";

    var _SupportedPropertyRenderer = {};

    /**
     * Default renderer for {@link ursa.model.SupportedProperty}.
     * @memberof ursa.view
     * @name SupportedPropertyRenderer
     * @protected
     * @class
     * @extends ursa.view.ViewRenderer
     */
    var SupportedPropertyRenderer = (namespace.SupportedPropertyRenderer = function(httpService, jsonLdProcessor, authenticationProvider, filterProvider) {
        ursa.view.ViewRenderer.prototype.constructor.call(this, httpService, jsonLdProcessor, authenticationProvider, filterProvider);
    })[":"](ursa.view.ViewRenderer);

    SupportedPropertyRenderer.prototype.isApplicableTo = function (apiMember) {
        ursa.view.ViewRenderer.prototype.isApplicableTo.apply(this, arguments);
        return apiMember instanceof ursa.model.SupportedProperty;
    };

    SupportedPropertyRenderer.prototype.initialize = function (apiMember) {
        Function.requiresArgument("apiMember", apiMember, ursa.model.SupportedProperty);
        ursa.view.ViewRenderer.prototype.initialize.apply(this, arguments);
    };

    SupportedPropertyRenderer.prototype.render = function(scope, classNames) {
        ursa.view.ViewRenderer.prototype.render.apply(this, arguments);
        if (!scope.supportedPropertyNewValues) {
            _SupportedPropertyRenderer.setupPropertyScope.call(this, scope);
        }

        var context = _SupportedPropertyRenderer.initialize.call(this, scope, classNames);
        var result = _SupportedPropertyRenderer.renderField.call(this, scope, context);
        if (this.apiMember.maxOccurances === 1) {
            var isNull = (!(scope.supportedPropertyNulls[this.apiMember.id] = (this.apiMember.minOccurances === 0)) ? "" : String.format(
                "<span class=\"input-group-addon\">" +
                "<input type=\"checkbox\" title=\"Null\" checked ng-model=\"supportedPropertyNulls['{0}']\" ng-change=\"onIsNullCheckedChanged('{0}')\" />" +
                "</span>", this.apiMember.id));
            return String.format("<div class=\"input-group\"><span ng-class=\"styleFor('{3}', null)\">{0}</span>{1}{2}</div>", this.apiMember.label, result, isNull, this.apiMember.id);
        }

        return _SupportedPropertyRenderer.renderCollection.call(this, scope, context, result);
    };

    // TODO: Add validation when adding item to collection
    _SupportedPropertyRenderer.initialize = function(scope, classNames) {
        if (this.apiMember.key) {
            scope.supportedPropertyKeys[this.apiMember.id] = true;
        }

        if (!this.apiMember.writeable) {
            scope.supportedPropertyReadonly[this.apiMember.id] = true;
        }

        var result = {
            classNames: String.format("class=\"form-control{0}\" ", (typeof (classNames) === "string" ? classNames : "")),
            propertyName: this.apiMember.propertyName(scope.operation),
            propertySelector: scope.targetInstance +
            (!scope.operation.isRdf ? "['{0}']" : "['{0}']" + (this.apiMember.maxOccurances > 1 ?
            ((this.apiMember.range) && (this.apiMember.isList) ? "[0]['@list']" : "") :
            (this.apiMember.range instanceof ursa.model.DataType ? "[0]['@value']" : "[0]['@id']")))
        };
        result.literalSelector = String.format(result.propertySelector, result.propertyName) + "[$index]";
        result.valueSelector = (this.apiMember.maxOccurances <= 1 ? result.propertySelector : result.literalSelector + (!scope.operation.isRdf ? "" :
            (this.apiMember.range instanceof ursa.model.DataType ? "['@value']" : "")));
        return result;
    };

    _SupportedPropertyRenderer.renderField = function(scope, context) {
        var operation = null;
        var controlType = ((this.apiMember.range) && (SupportedPropertyRenderer.dataTypes[this.apiMember.range.typeId]) ? "input" : "select");
        if ((controlType === "select") && ((operation = namespace.findEntityCrudOperation.call(this, "GET", true)) === null)) {
            controlType = "input";
        }

        var format = String.format(
            "<{0} {1}ng-model=\"{2}\" name=\"{5}\" ng-disabled=\"isPropertyReadonly('{3}')\" ",
            controlType,
            context.classNames,
            (controlType === "select" ? context.valueSelector.replace(/\['@id'\]$/, "") : context.valueSelector),
            this.apiMember.id,
            ((this.apiMember.required) && (!this.apiMember.key) ? "required " : ""),
            context.propertyName);
        var parameters = [context.propertyName];
        var dataType = (this.apiMember.range ? SupportedPropertyRenderer.dataTypes[this.apiMember.range.typeId] : null) || { type: "text" };
        for (var property in dataType) {
            if ((dataType.hasOwnProperty(property)) && (dataType[property] !== undefined) && (dataType[property] !== null)) {
                format += (property === "pattern" ? "ng-" + property : property) + "=\"{" + parameters.length + "}\" ";
                parameters.push(property === "pattern" ? "/" + dataType[property] + "/" : dataType[property]);
            }
        }

        var closure = "/>";
        if (controlType === "select") {
            var displayName = this.apiMember.owner.getInstanceDisplayNameProperty(operation);
            scope.supportedPropertyTypeValues[this.apiMember.range.typeId] = [];
            _SupportedPropertyRenderer.loadItems.call(this, scope, this.apiMember.range);
            closure = String.format(
                " ng-focus=\"initialize('{4}')\" ng-options=\"item as item['{0}']{3} for item in supportedPropertyTypeValues['{1}'] track by item['{2}']\"></select>",
                displayName.propertyName(operation),
                this.apiMember.range.typeId,
                (operation.isRdf ? "@id" : this.apiMember.owner.getKeyProperty(operation).propertyName(operation)),
                (operation.isRdf ? "[0]['@value']" : ""),
                this.apiMember.id);
        }

        format += closure;
        parameters.splice(0, 0, format);
        return String.format.apply(window, parameters);
    };

    _SupportedPropertyRenderer.renderCollection = function(scope, context, field) {
        scope.supportedPropertyNewValues[context.propertyName] = this.apiMember.createInstance();
        var listControls = "";
        if (this.apiMember.isList) {
            listControls = "<button class=\"btn\" ng-disabled=\"$index === 0\" ng-click=\"movePropertyItem('{3}', $index, -1)\"><span class=\"glyphicon glyphicon-arrow-up\"></span></button>" +
                "<button class=\"btn\" ng-disabled=\"$index === {1}.length - 1\" ng-click=\"movePropertyItem('{3}', $index, 1)\"><span class=\"glyphicon glyphicon-arrow-down\"></span></button>";
        }

        var itemField = field.replace(" name=\"" + context.propertyName + "\"", " ng-attr-name=\"{{'" + context.propertyName + "_' + $index}}\"");
        var footerField = field.replace(context.literalSelector, "supportedPropertyNewValues['" + context.propertyName + "']")
            .replace(" name=\"" + context.propertyName + "\"", " name=\"" + context.propertyName + "_new\"");
        return String.format(
            "<div class=\"input-group\" ng-repeat=\"value in {1}\">" +
            "<span ng-class=\"styleFor('{3}', $index)\">{0}</span>" +
            "{2}" +
            "<span class=\"input-group-btn\">" +
            listControls +
            "<button class=\"btn btn-default\" ng-click=\"removePropertyItem('{3}', $index)\"><span class=\"glyphicon glyphicon-remove\"></span></button>" +
            "</span>" +
            "</div>" +
            "<div class=\"input-group\">" +
            "<span ng-class=\"styleFor('{3}', -1)\">{0}</span>" +
            "{4}" +
            "<span class=\"input-group-btn\"><button class=\"btn btn-default\" ng-click=\"addPropertyItem('{3}')\"><span class=\"glyphicon glyphicon-plus\"></span></button></span>" +
            "</div>",
            this.apiMember.label,
            String.format(context.propertySelector, context.propertyName),
            itemField,
            this.apiMember.id,
            footerField);
    };

    _SupportedPropertyRenderer.setupPropertyScope = function(scope) {
        var that = this;
        scope.editedEntityNulls = {};
        scope.supportedPropertyNewValues = {};
        scope.supportedPropertyNulls = {};
        scope.supportedPropertyKeys = {};
        scope.supportedPropertyReadonly = {};
        scope.supportedPropertyTypeValues = {};
        scope.supportedProperties = this.apiMember.owner.supportedProperties;
        scope.isPropertyReadonly = function (supportedPropertyId) { return _SupportedPropertyRenderer.isPropertyReadonly.call(that, scope, supportedPropertyId); };
        scope.styleFor = function (supportedPropertyId, index) { return _SupportedPropertyRenderer.styleFor.call(that, scope, supportedPropertyId, index); };
        scope.addPropertyItem = function (supportedPropertyId) { _SupportedPropertyRenderer.addPropertyItem.call(that, scope, supportedPropertyId); };
        scope.removePropertyItem = function (supportedPropertyId, index) { _SupportedPropertyRenderer.removePropertyItem.call(that, scope, supportedPropertyId, index); };
        scope.onIsNullCheckedChanged = function (supportedPropertyId, e) { _SupportedPropertyRenderer.onIsNullCheckedChanged.call(that, scope, supportedPropertyId, e); };
        scope.movePropertyItem = function (supportedPropertyId, index, direction) { _SupportedPropertyRenderer.movePropertyItem.call(that, scope, supportedPropertyId, index, direction); };
        scope.initialize = function (supportedPropertyId) { _SupportedPropertyRenderer.propertyInitialize.call(that, scope, supportedPropertyId); };
        var entityEventHandler = function (e, instance, type) { _SupportedPropertyRenderer.entityEvent.call(that, scope, type); };
        scope.onEvent(ursa.view.Events.EntityLoaded, function (e, instance, type) { _SupportedPropertyRenderer.entityLoaded.call(that, scope, type); });
        scope.onEvent(ursa.view.Events.EntityCreated, entityEventHandler);
        scope.onEvent(ursa.view.Events.EntityModified, entityEventHandler);
        scope.onEvent(ursa.view.Events.EntityRemoved, entityEventHandler);
    };

    _SupportedPropertyRenderer.entityLoaded = function(scope, type) {
        scope.supportedPropertyNewValues = {};
        if (type.typeId === this.apiMember.owner.typeId) {
            for (var index = 0; index < this.apiMember.owner.supportedProperties.length; index++) {
                _SupportedPropertyRenderer.isPropertyReadonly.call(this, scope, this.apiMember.owner.supportedProperties[index].id);
            }
        }
    };

    _SupportedPropertyRenderer.entityEvent = function(scope, type) {
        scope.supportedPropertyNewValues = {};
        if (type.typeId === this.apiMember.owner.typeId) {
            _SupportedPropertyRenderer.loadItems.call(this, scope, type);
        }
    };

    _SupportedPropertyRenderer.isPropertyReadonly = function(scope, supportedPropertyId) {
        var supportedProperty = scope.supportedProperties.getById(supportedPropertyId);
        var propertyName = supportedProperty.propertyName(scope.operation);
        var instance = scope[scope.targetInstance];
        if (instance !== null) {
            var value = instance[propertyName] || null;
            if ((value !== null) && (instance[propertyName] instanceof Array)) {
                value = ((scope.operation.isRdf) && (supportedProperty.range.isList) ?
                    instance[propertyName][0]["@list"] : instance[propertyName]);
                if (value.length === 0) {
                    value = null;
                }
            }

            if (value !== null) {
                delete scope.supportedPropertyNulls[supportedPropertyId];
            }
        }

        return scope.supportedPropertyKeys[supportedPropertyId] ||
            scope.supportedPropertyNulls[supportedPropertyId] ||
            scope.supportedPropertyReadonly[supportedPropertyId];
    };

    _SupportedPropertyRenderer.styleFor = function(scope, supportedPropertyId, index) {
        var result = "input-group-addon";
        var supportedProperty = scope.supportedProperties.getById(supportedPropertyId);
        if ((!supportedProperty.required) || (supportedProperty.key)) {
            return result;
        }

        var propertyName = supportedProperty.propertyName(scope.operation);
        var currentValue = scope[scope.targetInstance] || null;
        switch (index) {
            case null:
                if (currentValue !== null) {
                    currentValue = currentValue[propertyName];
                    currentValue = (currentValue.length > 0 ? currentValue[0]["@value"] : null);
                }

                break;
            case -1:
                currentValue = scope.supportedPropertyNewValues[propertyName];
                break;
            default:
                if (currentValue !== null) {
                    currentValue = currentValue[propertyName];
                    currentValue = (currentValue.length > 0 ? currentValue[index]["@value"] : null);
                }

                break;
        }

        return result + ((currentValue === undefined) || (currentValue === null) || ((typeof (currentValue) === "string") && (currentValue.length === 0)) ? " danger" : "");
    };

    _SupportedPropertyRenderer.addPropertyItem = function(scope, supportedPropertyId) {
        var supportedProperty = scope.supportedProperties.getById(supportedPropertyId);
        var propertyName = supportedProperty.propertyName(scope.operation);
        var isRdfList = ((scope.operation.isRdf) && (supportedProperty.isList));
        var propertyItems = scope[scope.targetInstance][propertyName] || (scope[scope.targetInstance][propertyName] = (isRdfList ? [{ "@list": [] }] : []));
        if ((propertyItems = (isRdfList ? propertyItems[0]["@list"] : propertyItems)).indexOf(scope.supportedPropertyNewValues[propertyName]) === -1) {
            propertyItems.push(scope.supportedPropertyNewValues[propertyName]);
        }

        scope.supportedPropertyNewValues[propertyName] = supportedProperty.createInstance();
    };

    _SupportedPropertyRenderer.removePropertyItem = function(scope, supportedPropertyId, index) {
        var supportedProperty = scope.supportedProperties.getById(supportedPropertyId);
        scope[scope.targetInstance][supportedProperty.propertyName(scope.operation)].splice(index, 1);
    };

    _SupportedPropertyRenderer.onIsNullCheckedChanged = function(scope, supportedPropertyId) {
        var supportedProperty = scope.supportedProperties.getById(supportedPropertyId);
        var propertyName = supportedProperty.propertyName(scope.operation);
        var instance = scope[scope.targetInstance];
        var value = instance[propertyName] || null;
        if ((value !== null) && (instance[propertyName] instanceof Array)) {
            value = ((scope.operation.isRdf) && (supportedProperty.range.isList) ?
                instance[propertyName][0]["@list"] : instance[propertyName]);
            if (value.length === 0) {
                value = null;
            }
        }

        delete instance[propertyName];
        if (value === null) {
            supportedProperty.initializeInstance(scope.operation, instance);
        }
    };

    _SupportedPropertyRenderer.movePropertyItem = function(scope, supportedPropertyId, index, direction) {
        var supportedProperty = scope.supportedProperties.getById(supportedPropertyId);
        var propertyName = supportedProperty.propertyName(scope.operation);
        var isRdfList = ((scope.operation.isRdf) && (supportedProperty.range.isList));
        var propertyItems = scope[scope.targetInstance][propertyName];
        var propertyItem = (propertyItems = (isRdfList ? propertyItems[0]["@list"] : propertyItems))[index];
        propertyItems[index] = propertyItems[index + direction];
        propertyItems[index + direction] = propertyItem;
    };

    _SupportedPropertyRenderer.propertyInitialize = function(scope, supportedPropertyId) {
        var supportedProperty = scope.supportedProperties.getById(supportedPropertyId);
        var operation = namespace.findEntityCrudOperation.call(this, "GET");
        supportedProperty.initializeInstance(operation, scope[scope.targetInstance]);
    };

    _SupportedPropertyRenderer.loadItems = function(scope, type) {
        var that = this;
        var operation = namespace.findEntityCrudOperation.call({ apiMember: type }, "GET", true);
        var request = new ursa.web.HttpRequest("GET", operation.createCallUrl(), { Accept: operation.mediaTypes.join() });
        this.httpService.sendRequest(request).
            then(function (response) {
                if ((response.headers["Content-Type"] || "*/*").indexOf(ursa.model.EntityFormat.ApplicationLdJson) === 0) {
                    that.jsonLdProcessor.expand(response.data).
                        then(function (expanded) {
                            scope.supportedPropertyTypeValues[type.typeId] = expanded;
                        });
                }
                else {
                    scope.supportedPropertyTypeValues[type.typeId] = response.data;
                }
            });
    };

    /**
     * Map of XSD data types and their description.
     * @memberof ursa.view.SupportedPropertyRenderer
     * @static
     * @public
     * @member {object} viewRenderers
     */
    SupportedPropertyRenderer.dataTypes = {};

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#unsignedByte"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.unsignedByte] = new ursa.view.DatatypeDescriptor("number", 1, 0, 255, "[0, 255]");

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#byte"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.byte] = new ursa.view.DatatypeDescriptor("number", 1, -127, 128, "[-127, 128]");

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#unsignedShort"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.unsignedShort] = new ursa.view.DatatypeDescriptor("number", 1, 0, 65535, "[-0, 65535]");

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#unsignedShort"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.short] = new ursa.view.DatatypeDescriptor("number", 1, -32768, 32767, "[-32768, 32767]");

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#short"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.unsignedInt] = new ursa.view.DatatypeDescriptor("number", 1, 0, 4294967295, "[0, 4294967295]");

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#unsignedInt"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.int] = new ursa.view.DatatypeDescriptor("number", 1, -2147483648, 2147483647, "[-2147483648, 2147483647]");

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#int"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.unsignedLong] = new ursa.view.DatatypeDescriptor("number", 1, 0, 18446744073709551615, "[0, 18446744073709551615]");

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#unsignedLong"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.long] = new ursa.view.DatatypeDescriptor("number", 1, -9223372036854775808, 9223372036854775807, "[-9223372036854775808, 9223372036854775807]");

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#long"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.nonPositiveInteger] = new ursa.view.DatatypeDescriptor("number", 1, null, 0, "[-&infin;, 0]");

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#nonPositiveInteger"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.positiveInteger] = new ursa.view.DatatypeDescriptor("number", 1, 1, null, "[1, &infin;]");

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#positiveInteger"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.nonNegativeInteger] = new ursa.view.DatatypeDescriptor("number", 1, 0, null, "[0, &infin;]");

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#nonNegativeInteger"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.negativeInteger] = new ursa.view.DatatypeDescriptor("number", 1, null, -1, "[-&infin;, -1]");

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#negativeInteger"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.integer] = new ursa.view.DatatypeDescriptor("number", 1, null, null, "[-&infin;, &infin;]");

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#integer"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.decimal] = new ursa.view.DatatypeDescriptor("number", null, null, null, "[-&infin;, &infin;]");

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#decimal"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.double] = new ursa.view.DatatypeDescriptor("number", null, Number.MIN_VALUE, Number.MAX_VALUE, "[-" + Number.MIN_VALUE + ", " + Number.MAX_VALUE + "]");

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#double"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.float] = new ursa.view.DatatypeDescriptor("number", null, -3.4028235E38, 3.4028235E38, "[-3.4028235E38, 3.4028235E38]");

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#float"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.boolean] = new ursa.view.DatatypeDescriptor("checkbox");

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#boolean"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.dateTime] = new ursa.view.DatatypeDescriptor("datetime");

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#dateTime"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.time] = new ursa.view.DatatypeDescriptor("time");

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#time"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.date] = new ursa.view.DatatypeDescriptor("date");

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#date"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.gYear] = new ursa.view.DatatypeDescriptor("number", 1, null, null, "YYYY");

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#gYear"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.gMonth] = new ursa.view.DatatypeDescriptor("number", 1, 1, 12, "MM");

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#gMonth"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.gDay] = new ursa.view.DatatypeDescriptor("number", 1, 1, 31, "DD");

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#gDay"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.gYearMonth] = new ursa.view.DatatypeDescriptor("month");

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#gYearMonth"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.string] = new ursa.view.DatatypeDescriptor("text", null, null, null, "text");

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#string"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.anyUri] = new ursa.view.DatatypeDescriptor("url", null, null, null, "i.e. http://my.own.url/");

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#anyUri"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.hexBinary] = new ursa.view.DatatypeDescriptor("file");

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#hexBinary"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.base64Binary] = new ursa.view.DatatypeDescriptor("file");

    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#base64Binary"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[guid.guid] = new ursa.view.DatatypeDescriptor("text", null, null, null, "00000000-0000-0000-0000-000000000000", "^[A-Fa-f0-9]{8}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{12}$");

    SupportedPropertyRenderer.toString = function () { return "ursa.view.SupportedPropertyRenderer"; };
}(namespace("ursa.view")));