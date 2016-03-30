/*globals namespace, xsd, guid, ursa, odata, confirm */
(function(namespace) {
    "use strict";

    /**
     * Ultimate ResT API documentation view renderers namespace.
     * @namespace ursa.view
     */

    /**
     * Abstract of a view renderer.
     * @memberof ursa.view
     * @name ViewRenderer
     * @protected
     * @abstract
     * @class
     * @param {ursa.web.HttpService} httpService HTTP async communication provider.
     * @param {ursa.model.JsonLdProcessor} jsonLdProcessor JSON-LD processor.
     * @param {ursa.web.AuthenticationProvider} authenticationProvider Authentication provider.
     * @param {ursa.model.FilterProvider} filterProvider Filter provider.
     */
    var ViewRenderer = namespace.ViewRenderer = function (httpService, jsonLdProcessor, authenticationProvider, filterProvider) {
        Function.requiresArgument("httpService", httpService, ursa.web.HttpService);
        Function.requiresArgument("jsonLdProcessor", jsonLdProcessor, ursa.model.JsonLdProcessor);
        Function.requiresArgument("authenticationProvider", authenticationProvider, ursa.web.AuthenticationProvider);
        Function.requiresArgument("filterProvider", filterProvider, ursa.model.FilterProvider);
        this.httpService = httpService;
        this.jsonLdProcessor = jsonLdProcessor;
        this.authenticationProvider = authenticationProvider;
        this.filterProvider = filterProvider;
    };
    ViewRenderer.prototype.apiMember = null;
    Object.defineProperty(ViewRenderer.prototype, "httpService", { enumerable: false, configurable: false, writable: true, value: null });
    Object.defineProperty(ViewRenderer.prototype, "jsonLdProcessor", { enumerable: false, configurable: false, writable: true, value: null });
    Object.defineProperty(ViewRenderer.prototype, "authenticationProvider", { enumerable: false, configurable: false, writable: true, value: null });
    Object.defineProperty(ViewRenderer.prototype, "filterProvider", { enumerable: false, configurable: false, writable: true, value: null });

    /**
     * Initializes a renderer with dependencies.
     * @memberof ursa.view.ViewRenderer
     * @instance
     * @public
     * @method initialize
     * @param {ursa.model.ApiMember} apiMember API member to check for compatiblity.
     */
    ViewRenderer.prototype.initialize = function(apiMember) {
        Function.requiresArgument("apiMember", apiMember, ursa.model.ApiMember);
        this.apiMember = apiMember;
    };
    /**
     * Determines whether this renderer is applicable for given {ursa.model.ApiMember} instance.
     * @memberof ursa.view.ViewRenderer
     * @instance
     * @public
     * @method isApplicableTo
     * @param {ursa.model.ApiMember} apiMember API member to check for compatiblity.
     */
    ViewRenderer.prototype.isApplicableTo = function(apiMember) {
        Function.requiresArgument("apiMember", apiMember, ursa.model.ApiMember);
        return false;
    };
    /**
     * Renders a view for given API member.
     * @memberof ursa.view.ViewRenderer
     * @instance
     * @public
     * @method render
     * @param {ursa.view.IViewScope} scope Target scope.
     * @param {string} [classNames] CSS class names to be added to the view.
     */
    ViewRenderer.prototype.render = function(scope, classNames) {
        Function.requiresArgument("scope", scope, ursa.view.IViewScope);
        Function.requiresOptionalArgument("classNames", classNames, "string");
        return String.format("<div{0}></div>", (typeof(classNames) === "string" ? String.format(" class=\"{0}\"", classNames) : ""));
    };
    ViewRenderer.toString = function() { return "ursa.view.ViewRenderer"; };

    /**
     * View renderer provider.
     * @memberof ursa.view
     * @name ViewRendererProvider
     * @public
     * @class
     * @param {ursa.IComponentFactory} viewRendererFactory Factory providing ursa.view.ViewRenderer implementations.
     * @param {ursa.web.HttpService} httpService An HTTP async communication service.
     * @param {ursa.model.JsonLdProcessor} jsonLdProcessor JSON-LD service.
     * @param {ursa.web.AuthenticationProvider} authenticationProvider Authentication provider.
     * @param {ursa.model.FilterProvider} filterProvider Filter provider.
     */
    var ViewRendererProvider = namespace.ViewRendererProvider = function(viewRendererFactory) {
        Function.requiresArgument("viewRendererFactory", viewRendererFactory, ursa.IComponentFactory);
        this._viewRenderersFactory = viewRendererFactory;
    };
    Object.defineProperty(ViewRenderer.prototype, "_viewRenderersFactory", { enumerable: false, configurable: false, writable: true, value: null });
    /**
     * Creates a view for given API member.
     * @memberof ursa.view.ViewRendererProvider
     * @instance
     * @public
     * @method createRenderer
     * @param {ursa.model.ApiMember} apiMember Target API member for which to render a view.
     * @returns {ursa.view.ViewRenderer} The view renderer.
     */
    ViewRendererProvider.prototype.createRenderer = function(apiMember) {
        var viewRenderers = this._viewRenderersFactory.resolveAll();
        for (var index = 0; index < viewRenderers.length; index++) {
            var viewRenderer = viewRenderers[index];
            if (viewRenderer.isApplicableTo(apiMember)) {
                viewRenderer.initialize(apiMember);
                return viewRenderer;
            }
        }

        return null;
    };
    ViewRendererProvider.toString = function() { return "ursa.view.ViewRendererProvider"; };

    /**
     * Describes a datatype features.
     * @memberof ursa.view
     * @name DatatypeDescriptor
     * @protected
     * @class
     * @param {string} type Type of the input.
     * @param {number} [step] Numeric step of the input. Use null for no step.
     * @param {number} [min] Min numeric value. Use null for no min.
     * @param {number} [max] Max numeric value. Use null for no max.
     * @param {string} [placeholder] Placeholder text.
     * @param {string} [pattern] Regular expression pattern.
     */
    var DatatypeDescriptor = namespace.DatatypeDescriptor = function(type, step, min, max, placeholder, pattern) {
        Function.requiresOptionalArgument("type", type, "string");
        Function.requiresOptionalArgument("step", step, "number");
        Function.requiresOptionalArgument("min", min, "number");
        Function.requiresOptionalArgument("max", max, "number");
        Function.requiresOptionalArgument("placeholder", placeholder, "string");
        Function.requiresOptionalArgument("pattern", pattern, "string");
        this.type = type;
        this.step = step;
        this.min = min;
        this.max = max;
        this.placeholder = placeholder;
        this.pattern = pattern;
    };
    /**
     * Checks if a given value is in range of this datatype descriptor values range.
     * @instance
     * @public
     * @method isInRange
     * @param {object} value Value to check.
     * @returns {boolean} True if the value is in a valid range; otherwise false.
     */
    DatatypeDescriptor.prototype.isInRange = function(value) {
        if (this.pattern !== null) {
            return (typeof(value) === "string") && (new RegExp(this.pattern).test(value));
        }

        return (typeof(value) === "number") && (!isNaN(value)) &&
            (((this.min !== null) && (value >= this.min)) || (this.min === null)) &&
            (((this.max !== null) && (value <= this.max)) || (this.max === null));
    };
    /**
     * Type of the input.
     * @memberof ursa.view.DatatypeDescriptor
     * @instance
     * @public
     * @member {string} type
     */
    DatatypeDescriptor.prototype.type = null;
    /**
     * Numeric step of the input. Use null for no step.
     * @memberof ursa.view.DatatypeDescriptor
     * @instance
     * @public
     * @member {number} step
     * @default null
     */
    DatatypeDescriptor.prototype.step = null;
    /**
     * Min numeric value.
     * @memberof ursa.view.DatatypeDescriptor
     * @instance
     * @public
     * @member {number} min
     * @default null
     */
    DatatypeDescriptor.prototype.min = null;
    /**
     * Max numeric value.
     * @memberof ursa.view.DatatypeDescriptor
     * @instance
     * @public
     * @member {number} max
     * @default null
     */
    DatatypeDescriptor.prototype.max = null;
    /**
     * Placeholder text.
     * @memberof ursa.view.DatatypeDescriptor
     * @instance
     * @public
     * @member {string} placeholder
     * @default null
     */
    DatatypeDescriptor.prototype.placeholder = null;
    /**
     * Regular expression pattern.
     * @memberof ursa.view.DatatypeDescriptor
     * @instance
     * @public
     * @member {string} pattern
     * @default null
     */
    DatatypeDescriptor.prototype.pattern = null;

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
        ViewRenderer.prototype.constructor.call(this, httpService, jsonLdProcessor, authenticationProvider, filterProvider);
    })[":"](ViewRenderer);
    SupportedPropertyRenderer.prototype.isApplicableTo = function(apiMember) {
        ViewRenderer.prototype.isApplicableTo.apply(this, arguments);
        return apiMember instanceof ursa.model.SupportedProperty;
    };
    SupportedPropertyRenderer.prototype.initialize = function(apiMember) {
        Function.requiresArgument("apiMember", apiMember, ursa.model.SupportedProperty);
        ViewRenderer.prototype.initialize.apply(this, arguments);
    };
    SupportedPropertyRenderer.prototype.render = function(scope, classNames) {
        ViewRenderer.prototype.render.apply(this, arguments);
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
            classNames: String.format("class=\"form-control{0}\" ", (typeof(classNames) === "string" ? classNames : "")),
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
        if ((controlType === "select") && ((operation = _OperationRenderer.findEntityCrudOperation.call(this, "GET", true)) === null)) {
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
        scope.isPropertyReadonly = function(supportedPropertyId) { return _SupportedPropertyRenderer.isPropertyReadonly.call(that, scope, supportedPropertyId); };
        scope.styleFor = function(supportedPropertyId, index) { return _SupportedPropertyRenderer.styleFor.call(that, scope, supportedPropertyId, index); };
        scope.addPropertyItem = function(supportedPropertyId) { _SupportedPropertyRenderer.addPropertyItem.call(that, scope, supportedPropertyId); };
        scope.removePropertyItem = function(supportedPropertyId, index) { _SupportedPropertyRenderer.removePropertyItem.call(that, scope, supportedPropertyId, index); };
        scope.onIsNullCheckedChanged = function(supportedPropertyId, e) { _SupportedPropertyRenderer.onIsNullCheckedChanged.call(that, scope, supportedPropertyId, e); };
        scope.movePropertyItem = function(supportedPropertyId, index, direction) { _SupportedPropertyRenderer.movePropertyItem.call(that, scope, supportedPropertyId, index, direction); };
        scope.initialize = function(supportedPropertyId) { _SupportedPropertyRenderer.propertyInitialize.call(that, scope, supportedPropertyId); };
        var entityEventHandler = function(e, instance, type) { _SupportedPropertyRenderer.entityEvent.call(that, scope, type); };
        scope.onEvent(Events.EntityLoaded, function(e, instance, type) { _SupportedPropertyRenderer.entityLoaded.call(that, scope, type); });
        scope.onEvent(Events.EntityCreated, entityEventHandler);
        scope.onEvent(Events.EntityModified, entityEventHandler);
        scope.onEvent(Events.EntityRemoved, entityEventHandler);
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
                value = ((scope.operation.isRdf) && (supportedProperty.range instanceof ursa.model.Class) && (supportedProperty.range.isList) ?
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

        return result + ((currentValue === undefined) || (currentValue === null) || ((typeof(currentValue) === "string") && (currentValue.length === 0)) ? " danger" : "");
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
            value = ((scope.operation.isRdf) && (supportedProperty.range instanceof ursa.model.Class) && (supportedProperty.range.isList) ?
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
        var isRdfList = ((scope.operation.isRdf) && (supportedProperty.range instanceof ursa.model.Class) && (supportedProperty.range.isList));
        var propertyItems = scope[scope.targetInstance][propertyName];
        var propertyItem = (propertyItems = (isRdfList ? propertyItems[0]["@list"] : propertyItems))[index];
        propertyItems[index] = propertyItems[index + direction];
        propertyItems[index + direction] = propertyItem;
    };
    _SupportedPropertyRenderer.propertyInitialize = function(scope, supportedPropertyId) {
        var supportedProperty = scope.supportedProperties.getById(supportedPropertyId);
        var operation = _OperationRenderer.findEntityCrudOperation.call(this, "GET");
        supportedProperty.initializeInstance(operation, scope[scope.targetInstance]);
    };
    _SupportedPropertyRenderer.loadItems = function(scope, type) {
        var that = this;
        var operation = _OperationRenderer.findEntityCrudOperation.call({ apiMember: type }, "GET", true);
        var request = new ursa.web.HttpRequest("GET", operation.createCallUrl(), { Accept: operation.mediaTypes.join() });
        this.httpService.sendRequest(request).
            then(function(response) {
                if ((response.headers["Content-Type"] || "*/*").indexOf(ursa.model.EntityFormat.ApplicationLdJson) === 0) {
                    that.jsonLdProcessor.expand(response.data).
                        then(function(expanded) {
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
    SupportedPropertyRenderer.dataTypes[xsd.unsignedByte] = new DatatypeDescriptor("number", 1, 0, 255, "[0, 255]");
    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#byte"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.byte] = new DatatypeDescriptor("number", 1, -127, 128, "[-127, 128]");
    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#unsignedShort"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.unsignedShort] = new DatatypeDescriptor("number", 1, 0, 65535, "[-0, 65535]");
    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#unsignedShort"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.short] = new DatatypeDescriptor("number", 1, -32768, 32767, "[-32768, 32767]");
    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#short"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.unsignedInt] = new DatatypeDescriptor("number", 1, 0, 4294967295, "[0, 4294967295]");
    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#unsignedInt"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.int] = new DatatypeDescriptor("number", 1, -2147483648, 2147483647, "[-2147483648, 2147483647]");
    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#int"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.unsignedLong] = new DatatypeDescriptor("number", 1, 0, 18446744073709551615, "[0, 18446744073709551615]");
    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#unsignedLong"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.long] = new DatatypeDescriptor("number", 1, -9223372036854775808, 9223372036854775807, "[-9223372036854775808, 9223372036854775807]");
    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#long"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.nonPositiveInteger] = new DatatypeDescriptor("number", 1, null, 0, "[-&infin;, 0]");
    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#nonPositiveInteger"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.positiveInteger] = new DatatypeDescriptor("number", 1, 1, null, "[1, &infin;]");
    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#positiveInteger"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.nonNegativeInteger] = new DatatypeDescriptor("number", 1, 0, null, "[0, &infin;]");
    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#nonNegativeInteger"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.negativeInteger] = new DatatypeDescriptor("number", 1, null, -1, "[-&infin;, -1]");
    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#negativeInteger"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.integer] = new DatatypeDescriptor("number", 1, null, null, "[-&infin;, &infin;]");
    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#integer"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.decimal] = new DatatypeDescriptor("number", null, null, null, "[-&infin;, &infin;]");
    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#decimal"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.double] = new DatatypeDescriptor("number", null, Number.MIN_VALUE, Number.MAX_VALUE, "[-" + Number.MIN_VALUE + ", " + Number.MAX_VALUE + "]");
    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#double"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.float] = new DatatypeDescriptor("number", null, -3.4028235E38, 3.4028235E38, "[-3.4028235E38, 3.4028235E38]");
    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#float"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.boolean] = new DatatypeDescriptor("checkbox");
    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#boolean"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.dateTime] = new DatatypeDescriptor("datetime");
    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#dateTime"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.time] = new DatatypeDescriptor("time");
    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#time"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.date] = new DatatypeDescriptor("date");
    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#date"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.gYear] = new DatatypeDescriptor("number", 1, null, null, "YYYY");
    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#gYear"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.gMonth] = new DatatypeDescriptor("number", 1, 1, 12, "MM");
    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#gMonth"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.gDay] = new DatatypeDescriptor("number", 1, 1, 31, "DD");
    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#gDay"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.gYearMonth] = new DatatypeDescriptor("month");
    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#gYearMonth"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.string] = new DatatypeDescriptor("text", null, null, null, "text");
    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#string"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.anyUri] = new DatatypeDescriptor("url", null, null, null, "i.e. http://my.own.url/");
    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#anyUri"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.hexBinary] = new DatatypeDescriptor("file");
    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#hexBinary"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[xsd.base64Binary] = new DatatypeDescriptor("file");
    /**
     * @name ursa.view.SupportedPropertyRenderer#"http://www.w3.org/2001/XMLSchema#base64Binary"
     * @type {ursa.view.DatatypeDescriptor}
     */
    SupportedPropertyRenderer.dataTypes[guid.guid] = new DatatypeDescriptor("text", null, null, null, "00000000-0000-0000-0000-000000000000", "^[A-Fa-f0-9]{8}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{12}$");
    SupportedPropertyRenderer.toString = function() { return "ursa.view.SupportedPropertyRenderer"; };

    /**
     * Default renderer for {@link ursa.model.Class}.
     * @memberof ursa.view
     * @name ClassRenderer
     * @protected
     * @class
     * @extends ursa.view.ViewRenderer
     */
    var ClassRenderer = (namespace.ClassRenderer = function(httpService, jsonLdProcessor, authenticationProvider, filterProvider, viewRendererProvider) {
        ViewRenderer.prototype.constructor.call(this, httpService, jsonLdProcessor, authenticationProvider, filterProvider);
        Function.requiresArgument("viewRendererProvider", viewRendererProvider, ursa.view.ViewRendererProvider);
        this._viewRendererProvider = viewRendererProvider;
    })[":"](ViewRenderer);
    Object.defineProperty(ClassRenderer.prototype, "_viewRendererProvider", { enumerable: false, configurable: false, writable: true, value: null });
    ClassRenderer.prototype.isApplicableTo = function(apiMember) {
        ViewRenderer.prototype.isApplicableTo.apply(this, arguments);
        return apiMember instanceof ursa.model.Class;
    };
    ClassRenderer.prototype.initialize = function(apiMember) {
        Function.requiresArgument("apiMember", apiMember, ursa.model.Class);
        ViewRenderer.prototype.initialize.apply(this, arguments);
    };
    ClassRenderer.prototype.render = function(scope, classNames) {
        ViewRenderer.prototype.render.apply(this, arguments);
        var result = String.format(
            "<div class=\"panel{0}\"><div class=\"panel-body\"><form{1}>",
            (typeof(classNames) === "string" ? " " + classNames : ""),
            (scope.uniqueId ? " name=\"" + scope.uniqueId + "\"" : ""));
        for (var index = 0; index < this.apiMember.supportedProperties.length; index++) {
            var supportedProperty = this.apiMember.supportedProperties[index];
            if (!supportedProperty.readable) {
                continue;
            }

            var viewRenderer = this._viewRendererProvider.createRenderer(supportedProperty);
            if (viewRenderer === null) {
                continue;
            }

            result += viewRenderer.render(scope);
        }

        result += "</form></div></div>";
        return result;
    };
    ClassRenderer.toString = function() { return "ursa.view.ClassRenderer"; };

    var bodylessVerbs = ["TRACE"];
    var expectedBodyVerbs = ["POST", "PUT"];

    /**
     * Default renderer for {@link ursa.model.Operation}.
     * @memberof ursa.view
     * @name OperationRenderer
     * @protected
     * @class
     * @extends ursa.view.ViewRenderer
     */
    var OperationRenderer = (namespace.OperationRenderer = function(httpService, jsonLdProcessor, authenticationProvider, filterProvider) {
        ViewRenderer.prototype.constructor.call(this, httpService, jsonLdProcessor, authenticationProvider, filterProvider);
    })[":"](ViewRenderer);
    OperationRenderer.prototype.isApplicableTo = function(apiMember) {
        ViewRenderer.prototype.isApplicableTo.apply(this, arguments);
        return apiMember instanceof ursa.model.Operation;
    };
    OperationRenderer.prototype.initialize = function(apiMember) {
        Function.requiresArgument("apiMember", apiMember, ursa.model.Operation);
        ViewRenderer.prototype.initialize.apply(this, arguments);
    };
    OperationRenderer.prototype.render = function(scope, classNames) {
        ViewRenderer.prototype.render.apply(this, arguments);
        if ((this.apiMember.returns.length === 1) && (this.apiMember.returns[0].maxOccurances === Number.MAX_VALUE)) {
            return _OperationRenderer.renderEntityList.call(this, scope, classNames);
        }

        return "";
    };
    OperationRenderer.toString = function() { return "ursa.view.OperationRenderer"; };
    var _OperationRenderer = {};
    _OperationRenderer.renderEntityList = function(scope, classNames) {
        if (!scope.supportedProperties) {
            _OperationRenderer.setupListScope.call(this, scope);
        }

        var pager = "";
        var pages = "Action";
        if (scope.itemsPerPageList) {
            pager = _OperationRenderer.generatePager.call(this, scope);
            pages = "<select class=\"form-control\" ng-model=\"filters.itemsPerPage\" ng-change=\"list(1)\" ng-options=\"take for take in itemsPerPageList track by take\"></select>";
        }

        var result = String.format(
                "<table class=\"table table-condensed table-bordered table-hover{0}\">", (classNames !== undefined) && (classNames !== null) ? " " + classNames : "") + String.format(
                "<tr>" +
                "<th ng-repeat=\"supportedProperty in supportedProperties track by supportedProperty.id\" ng-hide=\"supportedProperty.key\">" +
                "<span class=\"form-control\" ng-show=\"supportedProperties.getById(supportedProperty.id) === null\">{{ supportedProperty.label }}</span>" +
                "<input class=\"form-control\" ng-hide=\"supportedProperties.getById(supportedProperty.id) == null\" " +
                "type=\"text\" ng-model=\"filters[supportedProperty.property]\" placeholder=\"{{ supportedProperty.label }}\" " +
                "ng-change=\"list()\"/>" +
                "</th>" +
                "<th>{0}</th>" +
                "</tr>", pages) + String.format(
                "<tr ng-repeat-start=\"entity in entities{1}\" ng-hide=\"entityEquals(entity)\"" + (scope.keyProperty !== "" ? " title=\"{{ entity['{0}'] | asId }}\"" : "") + ">" +
                "<td ng-repeat=\"supportedProperty in supportedProperties track by supportedProperty.id\" ng-hide=\"supportedProperty.key\">{{ getPropertyValue(entity, supportedProperty, operation) | stringify }}</td>" +
                "<td><div class=\"btn-block\">" +
                (scope.getOperation !== null ? "<button class=\"btn btn-default\" title=\"Edit\" ng-click=\"get(entity)\"><span class=\"glyphicon glyphicon-pencil\"></span></button>" : "") +
                (scope.deleteOperation !== null ? "<button class=\"btn btn-default\" title=\"Delete\" ng-click=\"delete(entity, $event)\"><span class=\"glyphicon glyphicon-remove\"></span></button>" : "") +
                "</div></td>" +
                "</tr>", scope.keyProperty, (scope.keyProperty !== "" ? " track by entity['" + scope.keyProperty + "']" : "")) + String.format(
                "<tr ng-repeat-end ng-show=\"entityEquals(entity)\" ng-init=\"initialize($index)\">" +
                "<td colspan=\"{0}\"><div><ursa-api-member-view api-member=\"operation.owner\" target-instance=\"editedEntity\" unique-id=\"{{ uniqueId[$index] }}\"></ursa-api-member-view></div></td>" +
                "<td><div class=\"btn-block\">" +
                (scope.updateOperation !== null ? "<button class=\"btn btn-default\" title=\"Update\" ng-disabled=\"isFormDisabled(uniqueId[$index])\" ng-click=\"update(editedEntity)\"><span class=\"glyphicon glyphicon-floppy-disk\"></span></button>" : "") +
                "<button class=\"btn btn-default\" title=\"Cancel\" ng-click=\"cancel()\"><span class=\"glyphicon glyphicon-repeat\"></span></button>" +
                "</div></td>" +
                "</tr>", scope.supportedProperties.length - 1) + String.format(
                "<tr ng-hide=\"editedEntity !== null\" ng-init=\"initialize(-1)\">" +
                "<td colspan=\"{0}\"><div ng-show=\"footerVisible\"><ursa-api-member-view api-member=\"operation.owner\" target-instance=\"newInstance\" unique-id=\"{{ uniqueId.footer }}\"></ursa-api-member-view></div></td>" +
                "<td><div class=\"btn-block\">" +
                (scope.createOperation !== null ? "<button class=\"btn btn-default\" title=\"Create\" ng-disabled=\"isFormDisabled(uniqueId.footer)\" ng-click=\"create(newInstance)\"><span class=\"glyphicon glyphicon-plus\"></span></button>" : "") +
                "</div></td>" +
                "</tr>", scope.supportedProperties.length - 1) +
            pager +
            "</table>";
        scope.list();
        return result;
    };
    _OperationRenderer.generatePager = function(scope) {
        return String.format(
            "<tr>" +
            "<td colspan=\"{0}\"><nav><ul class=\"pagination\">" +
            "<li ng-hide=\"filters.currentPage <= 1\"><a href ng-click=\"list(filters.currentPage - 1)\">&laquo;</a></li>" +
            "<li ng-repeat=\"page in pages\"><a href ng-click=\"list(page)\">{{ page }}</a></li>" +
            "<li ng-hide=\"filters.currentPage >= pages.length\"><a href ng-click=\"list(filters.currentPage + 1)\">&raquo;</a></li>" +
            "</ul></nav>" +
            "</td>" +
            "</tr>", scope.supportedProperties.length);
    };
    _OperationRenderer.setupListScope = function(scope) {
        var that = this;
        scope.uniqueId = [];
        scope.operation = this.apiMember;
        scope.deleteOperation = _OperationRenderer.findEntityCrudOperation.call(this, "DELETE");
        scope.updateOperation = _OperationRenderer.findEntityCrudOperation.call(this, "PUT");
        scope.getOperation = _OperationRenderer.findEntityCrudOperation.call(this, "GET");
        if ((scope.createOperation = _OperationRenderer.findEntityCrudOperation.call(this, "POST")) !== null) {
            scope.newInstance = this.apiMember.owner.createInstance(this.apiMember);
            scope.footerVisible = false;
        }

        scope.keyProperty = (this.apiMember.isRdf ? "@id" : "");
        scope.supportedProperties = new ursa.model.ApiMemberCollection();
        for (var index = 0; index < this.apiMember.owner.supportedProperties.length; index++) {
            var supportedProperty = this.apiMember.owner.supportedProperties[index];
            if (supportedProperty.key) {
                scope.keyProperty = supportedProperty.propertyName(this.apiMember) + (this.apiMember.isRdf ? "'][0]['@value" : "");
            }

            if ((supportedProperty.maxOccurances === 1) && (supportedProperty.readable) && (supportedProperty.range instanceof ursa.model.DataType)) {
                scope.supportedProperties.push(supportedProperty);
            }
        }

        scope.filters = new ursa.model.Filter(scope.supportedProperties);
        if ((scope.operation.mappings !== null) && (this.filterProvider.providesCapabilities(this.apiMember.mappings, ursa.model.FilterExpressionProvider.SupportsPaging))) {
            scope.itemsPerPageList = [scope.filters.itemsPerPage = 10, 20, 50, 100];
            scope.pages = [];
            scope.totalEntities = 0;
        }

        scope.entities = null;
        scope.editedEntity = null;
        scope.getPropertyValue = function(entity, supportedProperty, operation) { return entity[supportedProperty.propertyName(operation)]; };
        scope.cancel = function() { scope.editedEntity = null; };
        scope.list = function(page) { _OperationRenderer.loadEntityList.call(that, scope, scope.operation, null, page); };
        scope.get = function(instance) { _OperationRenderer.loadEntity.call(that, scope, scope.getOperation, instance); };
        scope.create = function(instance) { _OperationRenderer.createEntity.call(that, scope, scope.createOperation, instance); };
        scope.update = function(instance) { _OperationRenderer.updateEntity.call(that, scope, scope.updateOperation, instance); };
        scope.delete = function(instance, e) { _OperationRenderer.deleteEntity.call(that, scope, scope.deleteOperation, instance, e); };
        scope.entityEquals = function(entity) { return _OperationRenderer.entityEquals.call(that, entity, scope.editedEntity, scope.operation); };
        scope.initialize = function(index) { _OperationRenderer.initialize.call(that, scope, index); };
        scope.isFormDisabled = function(name) { return _OperationRenderer.isFormDisabled.call(that, scope, name); };
    };
    _OperationRenderer.findEntityCrudOperation = function(method, listing) {
        listing = (typeof(listing) === "boolean" ? listing : false);
        var supportedOperations = (this.apiMember instanceof ursa.model.Class ? this.apiMember : this.apiMember.owner).supportedOperations;
        for (var index = 0; index < supportedOperations.length; index++) {
            var operation = supportedOperations[index];
            if ((operation.methods.indexOf(method) !== -1) &&
                (((!listing) && (((expectedBodyVerbs.indexOf(method) === -1) && (operation.returns.length > 0) && (operation.returns[0].maxOccurances === 1)) ||
                    ((expectedBodyVerbs.indexOf(method) !== -1) && (operation.expects.length > 0) && (operation.expects[0].maxOccurances === 1)) ||
                    ((method === "DELETE") && (operation.expects.length === 0) && (operation.returns.length === 0)))) ||
                ((listing) && (((expectedBodyVerbs.indexOf(method) === -1) && (operation.returns.length > 0) && (operation.returns[0].maxOccurances === Number.MAX_VALUE)) ||
                    ((expectedBodyVerbs.indexOf(method) !== -1) && (operation.expected.length > 0) && (operation.expected[0].maxOccurances === Number.MAX_VALUE)))))) {
                return operation;
            }
        }

        return null;
    };
    _OperationRenderer.onLoadEntitySuccess = function(scope, createOperation, instance, request, response) {
        var that = this;
        if ((response.headers["Content-Type"] || "*/*").indexOf(ursa.model.EntityFormat.ApplicationLdJson) === 0) {
            that.jsonLdProcessor.expand(response.data).
                then(function(expanded) {
                    scope.rootScope.broadcastEvent(Events.EntityLoaded, scope.editedEntity = (expanded.length > 0 ? expanded[0] : null), createOperation.owner);
                    scope.updateView();
                    return expanded;
                });
        }
        else {
            scope.rootScope.broadcastEvent(Events.EntityLoaded, scope.editedEntity = response.data, createOperation.owner);
        }
    };
    _OperationRenderer.loadEntity = function(scope, getOperation, instance) {
        _OperationRenderer.handleOperation.call(this, scope, getOperation, null, instance, _OperationRenderer.onLoadEntitySuccess, null, _OperationRenderer.loadEntity);
    };
    _OperationRenderer.createEntitySuccess = function(scope, createOperation, instance) {
        scope.newInstance = createOperation.owner.createInstance(createOperation);
        scope.rootScope.broadcastEvent(Events.EntityCreated, instance, createOperation.owner);
        scope.list(1);
        scope.footerVisible = false;
    };
    _OperationRenderer.ammendEntityFailure = function(scope, createOperation, instance, request) {
        if (request.initialInstanceId !== undefined) {
            if (request.initialInstanceId) {
                delete instance["@id"];
            }
            else {
                instance["@id"] = request.initialInstanceId;
            }
        }
    };
    _OperationRenderer.createEntity = function(scope, createOperation, instance) {
        if (!scope.footerVisible) {
            scope.footerVisible = true;
            return;
        }

        _OperationRenderer.handleOperation.call(this, scope, createOperation, null, instance, _OperationRenderer.createEntitySuccess, _OperationRenderer.ammendEntityFailure, _OperationRenderer.createEntity);
    };
    _OperationRenderer.onUpdateEntitySuccess = function(scope, updateOperation, instance) {
        scope.editedEntity = null;
        scope.rootScope.broadcastEvent(Events.EntityModified, instance, updateOperation.owner);
        scope.list();
    };
    _OperationRenderer.updateEntity = function(scope, updateOperation, instance) {
        _OperationRenderer.handleOperation.call(this, scope, updateOperation, null, instance, _OperationRenderer.onUpdateEntitySuccess, _OperationRenderer.ammendEntityFailure, _OperationRenderer.updateEntity);
    };
    _OperationRenderer.onDeleteEntitySuccess = function(scope, deleteOperation, instance) {
        scope.rootScope.broadcastEvent(Events.EntityRemoved, instance, deleteOperation);
        scope.list(1);
    };
    _OperationRenderer.deleteEntity = function(scope, deleteOperation, instance, e) {
        if ((e) && (!confirm("Are you sure you want to delete this item?"))) {
            e.preventDefault();
            e.stopPropagation();
            return;
        }

        _OperationRenderer.handleOperation.call(this, scope, deleteOperation, null, instance, _OperationRenderer.onDeleteEntitySuccess, null, _OperationRenderer.deleteEntity);
    };
    _OperationRenderer.onLoadEntityListSuccess = function(scope, deleteOperation, instance, request, response) {
        var that = this;
        var contentRange = response.headers["Content-Range"];
        if ((contentRange !== undefined) && (contentRange !== null)) {
            var matches = contentRange.match(/^members ([0-9]+)-([0-9]+)\/([0-9]+)/);
            if ((matches !== null) && (matches.length === 4)) {
                var startIndex = parseInt(matches[1]);
                scope.totalEntities = parseInt(matches[3]);
                scope.filters.currentPage = Math.ceil(startIndex / scope.filters.itemsPerPage) + 1;
                scope.pages = [];
                for (var pageIndex = 1; pageIndex <= Math.ceil(scope.totalEntities / scope.filters.itemsPerPage); pageIndex++) {
                    scope.pages.push(pageIndex);
                }
            }
        }

        if ((response.headers["Content-Type"] || "*/*").indexOf(ursa.model.EntityFormat.ApplicationLdJson) === 0) {
            that.jsonLdProcessor.expand(response.data).
                then(function(expanded) {
                    scope.entities = expanded;
                    scope.updateView();
                    return expanded;
                });
        }
        else {
            scope.entities = response.data;
        }
    };
    _OperationRenderer.loadEntityList = function(scope, listOperation, instance, page) {
        var candidateMethod = _OperationRenderer.findEntityListMethod.call(this, listOperation);
        if (candidateMethod === null) {
            return;
        }

        if (typeof(page) === "number") {
            scope.filters.currentPage = page;
        }

        if (listOperation.mappings !== null) {
            for (var index = 0; index < listOperation.mappings.length; index++) {
                var mapping = listOperation.mappings[index];
                var filterExpressionProvider = this.filterProvider.resolve(mapping.property);
                if (filterExpressionProvider === null) {
                    continue;
                }

                var filterExpression = filterExpressionProvider.createFilter(mapping, scope.filters);
                if (filterExpression === null) {
                    continue;
                }

                instance = instance || {};
                instance[mapping.propertyName(listOperation)] = (listOperation.isRdf ? [{ "@value": filterExpression }] : filterExpression);
            }
        }

        _OperationRenderer.handleOperation.call(this, scope, listOperation, candidateMethod, instance, _OperationRenderer.onLoadEntityListSuccess, null, _OperationRenderer.loadEntityList, page);
    };
    _OperationRenderer.entityEquals = function(leftOperand, rightOperand, operation) {
        if ((!leftOperand) || (!rightOperand)) {
            return false;
        }

        if (leftOperand === rightOperand) {
            return true;
        }

        if (!operation.isRdf) {
            var supportedProperties = operation.owner.supportedProperties;
            for (var index = 0; index < supportedProperties.length; index++) {
                var supportedProperty = supportedProperties[index];
                if (supportedProperty.key) {
                    return leftOperand[supportedProperty.propertyName(operation)] === rightOperand[supportedProperty.propertyName(operation)];
                }
            }

            return false;
        }

        return (leftOperand["@id"] === rightOperand["@id"]);
    };
    _OperationRenderer.findEntityListMethod = function(listOperation) {
        var candidateMethod = null;
        for (var index = 0; index < listOperation.methods.length; index++) {
            if ((candidateMethod !== null) && (listOperation.methods[index] !== "GET")) {
                continue;
            }

            candidateMethod = listOperation.methods[index];
            if (listOperation.methods[index] === "GET") {
                break;
            }
        }

        return candidateMethod;
    };
    _OperationRenderer.sanitizeEntity = function(instance, operation) {
        if (!operation.isRdf) {
            return instance;
        }

        for (var property in instance) {
            if ((instance.hasOwnProperty(property)) && (instance[property] instanceof Array)) {
                if (instance[property.length] === 0) {
                    delete instance[property];
                }
                else {
                    for (var index = 0; instance < instance[property].length; index++) {
                        _OperationRenderer.sanitizeEntity.call(this, instance[property][index], operation);
                    }
                }
            }
        }

        return instance;
    };
    _OperationRenderer.initialize = function(scope, index) {
        if (index === -1) {
            scope.uniqueId.footer = "instance_" + Math.random().toString().replace(".", "").substr(1);
        }
        else {
            scope.uniqueId[index] = "instance_" + Math.random().toString().replace(".", "").substr(1);
        }
    };
    _OperationRenderer.isFormDisabled = function(scope, name) {
        var forms = document.getElementsByName(name);
        return (forms.length > 0 ? angular.element(forms[0]).scope()[name].$invalid : true) && (scope.footerVisible);
    };
    _OperationRenderer.handleUnauthorized = function(scope, challenge, callback) {
        if (this._authenticationEventHandler) {
            this._authenticationEventHandler();
            delete this._authenticationEventHandler;
        }

        var that = this;
        this._authenticationEventHandler = scope.onEvent(Events.Authenticate, function(e, userName, password) {
            that.authenticationProvider.authenticate(challenge, userName, password).
                then(function(authorization) {
                    that._authorization = authorization;
                    callback();
                    return authorization;
                });
        });

        scope.rootScope.broadcastEvent(Events.AuthenticationRequired);
    };
    _OperationRenderer.handleAuthorized = function(scope) {
        if (this._authenticationEventHandler) {
            this._authenticationEventHandler();
            delete this._authenticationEventHandler;
        }

        if (this._authorization) {
            this.authenticationProvider.use(this._authorization);
            delete this._authorization;
        }

        scope.rootScope.broadcastEvent(Events.Authenticated);
    };
    _OperationRenderer.prepareRequest = function(operation, methodOverride, instance) {
        var url = operation.createCallUrl(instance);
        var request = new ursa.web.HttpRequest(methodOverride || operation.methods[0], url, { Accept: operation.mediaTypes.join() });
        request.initialInstanceId = undefined;
        if ((operation.mediaTypes.length > 0) && (bodylessVerbs.indexOf(operation.methods[0]) === -1)) {
            request.headers["Content-Type"] = operation.mediaTypes[0];
        }

        if ((operation.mappings !== null) && (operation.mappings.getByProperty(odata.skip, "property") !== null) &&
            (operation.mappings.getByProperty(odata.top, "property") !== null)) {
            request.headers["Accept-Ranges"] = "members";
        }

        if (instance) {
            if (operation.isRdf) {
                if (instance["@id"] === undefined) {
                    request.initialInstanceId = true;
                    instance["@id"] = url;
                }
                else if ((instance["@id"] === null) || (instance["@id"] === "") || (instance["@id"].indexOf("_:") === 0)) {
                    request.initialInstanceId = instance["@id"];
                    instance["@id"] = url;
                }
            }

            request.data = JSON.stringify(_OperationRenderer.sanitizeEntity.call(this, instance, operation));
        }

        if (this._authorization) {
            request.headers.Authorization = this._authorization;
        }

        return request;
    };
    _OperationRenderer.handleOperation = function(scope, operation, methodOverride, instance, success, failure, callbackMethod, context) {
        var that = this;
        var request = _OperationRenderer.prepareRequest.call(this, operation, methodOverride, instance);
        var promise = this.httpService.sendRequest(request).
            then(function(response) {
                if (typeof(success) === "function") {
                    success.call(that, scope, operation, instance, request, response);
                }

                _OperationRenderer.handleAuthorized.call(that, scope);
                scope.updateView();
                return response;
            });

        promise.catch(function(response) {
            if (typeof(failure) === "function") {
                failure.call(that, scope, operation, instance, request, response);
            }

            if (response.status === ursa.model.HttpStatusCodes.Unauthorized) {
                if (that._authenticationEventHandler) {
                    scope.rootScope.broadcastEvent(Events.AuthenticationFailed, response.statusText);
                }
                else {
                    var callback = function() { callbackMethod.call(that, scope, operation, instance, context); };
                    var challenge = ((response.headers["WWW-Authenticate"]) || (response.headers["X-WWW-Authenticate"])).split(/[ ;]/g)[0].toLowerCase();
                    _OperationRenderer.handleUnauthorized.call(that, scope, challenge, callback);
                }
            }

            scope.updateView();
            return response;
        });
    };
    
    /**
     * Enumeration of URSA events.
     * @public
     * @readonly 
     * @enum {string}
     */
    var Events = namespace.Events = {
        /*Notifies that the authentication is required.*/
        AuthenticationRequired: "AuthenticationRequired",
        /*Notifies that the authentication failed.*/
        AuthenticationFailed: "AuthenticationFailed",
        /*Notifies that the authentication should occur.*/
        Authenticate: "Authenticate",
        /*Notifies that the authentication was successful.*/
        Authenticated: "Authenticated",
        /*Notifies that an entity has been created. */
        EntityCreated: "EntityCreated",
        /*Notifies that an entity has been removed. */
        EntityRemoved: "EntityRemoved",
        /*Notifies that an entity has been modified. */
        EntityModified: "EntityModified",
        /*Notifies that an entity has been loaded. */
        EntityLoaded: "EntityLoaded"
    };
}(namespace("ursa.view")));