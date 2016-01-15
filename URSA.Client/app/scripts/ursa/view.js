/*globals namespace, container, xsd, guid, ursa, confirm */
(function (namespace) {
    "use strict";

    var invalidArgumentPassed = "Invalid {0} passed.";

    var _ctor = "_ctor";
    var isInitialized = false;

    /**
     * Ultimate ResT API documentation view renderers namespace.
     * @namespace ursa.view
     */

    /**
     * Collection of {@link ursa.view.ViewRenderer}s.
     * @memberof ursa.view
     * @name ViewRendererCollection
     * @protected
     * @class
     * @extends Array
     */
    var ViewRendererCollection = function() { };
    ViewRendererCollection.prototype = [];
    ViewRendererCollection.prototype.constructor = ViewRendererCollection;
    /**
     * Searches view renderers for the one that is applicable for a given API member.
     * @memberof ursa.view.ViewRendererCollection
     * @instance
     * @public
     * @method find
     * @param {ursa.model.ApiMember} apiMember Target API member for which to find the renderer.
     */
    ViewRendererCollection.prototype.find = function(apiMember) {
        var index;
        if (!isInitialized) {
            var types = container.resolve(ViewRenderer);
            for (index = 0; index < types.length; index++) {
                ViewRenderer.viewRenderers.push(new types[index]());
            }

            isInitialized = true;
        }

        for (index = 0; index < ViewRenderer.viewRenderers.length; index++) {
            if (ViewRenderer.viewRenderers[index].isApplicableTo(apiMember)) {
                return ViewRenderer.viewRenderers[index];
            }
        }

        return null;
    };

    /**
     * Abstract of a view renderer.
     * @memberof ursa.view
     * @name ViewRenderer
     * @protected
     * @abstract
     * @class
     */
    var ViewRenderer = namespace.ViewRenderer = function() { };
    ViewRenderer.prototype.constructor = ViewRenderer;
    /**
     * Determines whether this renderer is applicable for given {ursa.model.ApiMember} instance.
     * @memberof ursa.view.ViewRenderer
     * @instance
     * @public
     * @method isApplicableTo
     * @param {ursa.model.ApiMember} apiMember API member to check for compatiblity.
     */
    ViewRenderer.prototype.isApplicableTo = function(apiMember) {
        if ((apiMember === undefined) || (apiMember === null) || (!(apiMember instanceof ursa.model.ApiMember))) {
            throw new Error(invalidArgumentPassed.replace("{0}", "apiMember"));
        }

        return false;
    };
    /**
     * Renders a view for given API member.
     * @memberof ursa.view.ViewRenderer
     * @instance
     * @public
     * @method render
     * @param {angular.$rootScope.Scope} scope Target scope.
     * @param {angular.$http} http HTTP service.
     * @param {jsonld} jsonld JSON-LD service.
     * @param {ursa.model.ApiMember} apiMember Target API member for which to render a view.
     * @param {string} [classNames] CSS class names to be added to the view.
     */
    ViewRenderer.prototype.render = function(scope, http, jsonld, authentication, apiMember, classNames) {
        if ((apiMember === undefined) || (apiMember === null) || (!(apiMember instanceof ursa.model.ApiMember))) {
            throw new Error(invalidArgumentPassed.replace("{0}", "apiMember"));
        }

        return String.format("<div{0}></div>", (typeof(classNames) === "string" ? String.format(" class=\"{0}\"", classNames) : ""));
    };
    /**
     * Collection of discovered {@link ursa.view.ViewRenderer}s.
     * @memberof ursa.view.ViewRenderer
     * @static
     * @public
     * @member {ursa.view.ViewRendererCollection} viewRenderers
     */
    ViewRenderer.viewRenderers = new ViewRendererCollection();

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
        this.type = type;
        this.step = step;
        this.min = min;
        this.max = max;
        this.placeholder = placeholder;
        this.pattern = pattern;
    };
    DatatypeDescriptor.prototype.constructor = DatatypeDescriptor;
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
    DatatypeDescriptor.prototype.pattern = null;
    /**
     * Regular expression pattern.
     * @memberof ursa.view.DatatypeDescriptor
     * @instance
     * @public
     * @member {string} pattern
     * @default null
     */
    DatatypeDescriptor.prototype.pattern = null;

    /**
     * Default renderer for {@link ursa.model.SupportedProperty}.
     * @memberof ursa.view
     * @name SupportedPropertyRenderer
     * @protected
     * @class
     * @extends ursa.view.ViewRenderer
     */
    var SupportedPropertyRenderer = namespace.SupportedPropertyRenderer = function() {
        ViewRenderer.prototype.constructor.apply(this, arguments);
    };
    SupportedPropertyRenderer.prototype = new ViewRenderer(_ctor);
    SupportedPropertyRenderer.prototype.constructor = SupportedPropertyRenderer;
    SupportedPropertyRenderer.prototype.isApplicableTo = function(apiMember) {
        ViewRenderer.prototype.isApplicableTo.apply(this, arguments);
        return apiMember instanceof ursa.model.SupportedProperty;
    };
    SupportedPropertyRenderer.prototype.render = function(scope, http, jsonld, authentication, apiMember, classNames) {
        ViewRenderer.prototype.render.apply(this, arguments);
        if (!(apiMember instanceof ursa.model.SupportedProperty)) {
            throw new Error(invalidArgumentPassed.replace("{0}", "apiMember"));
        }

        if (!scope.supportedPropertyNewValues) {
            supportedPropertyRendererSetupPropertyScope.call(this, scope, apiMember);
        }

        classNames = String.format("class=\"form-control{0}\" ", (typeof(classNames) === "string" ? classNames : ""));
        var propertyName = apiMember.propertyName(scope.operation);
        var propertySelector = scope.targetInstance + (!scope.operation.isRdf ? "['{0}']" : "['{0}']" + (apiMember.maxOccurances > 1 ? "" : "[0]['@value']"));
        var literalSelector = (supportedPropertyRendererIsLiteralRange.call(this, apiMember) ? String.format(propertySelector, propertyName) + "[$index]" : "value");
        var valueSelector = (apiMember.maxOccurances <= 1 ? propertySelector : literalSelector + (!scope.operation.isRdf ? "" : "['@value']"));
        if (apiMember.key) {
            scope.supportedPropertyKeys[apiMember.id] = true;
        }

        if (!apiMember.writeable) {
            scope.supportedPropertyReadonly[apiMember.id] = true;
        }

        var format = String.format(
            "<input {0}ng-model=\"{1}\" name=\"{4}\" ng-readonly=\"isPropertyReadonly('{2}')\" {3}",
            classNames,
            valueSelector,
            apiMember.id,
            ((apiMember.required) && (!apiMember.key) ? "required " : ""),
            propertyName);
        var parameters = [propertyName];
        var dataType = SupportedPropertyRenderer.dataTypes[apiMember.range.id] || { type: "text" };
        for (var property in dataType) {
            if ((dataType.hasOwnProperty(property)) && (dataType[property] !== undefined) && (dataType[property] !== null)) {
                format += (property === "pattern" ? "ng-" + property : property) + "=\"{" + parameters.length + "}\" ";
                parameters.push(property === "pattern" ? "/" + dataType[property] + "/" : dataType[property]);
            }
        }

        format += "/>";
        parameters.splice(0, 0, format);
        var result = String.format.apply(window, parameters);
        if (apiMember.maxOccurances === 1) {
            var isNull = (!(scope.supportedPropertyNulls[apiMember.id] = (apiMember.minOccurances === 0)) ? "" : String.format(
                "<span class=\"input-group-addon\">" +
                "<input type=\"checkbox\" title=\"Null\" checked ng-model=\"supportedPropertyNulls['{0}']\" ng-change=\"onIsNullCheckedChanged('{0}')\" />" +
                "</span>", apiMember.id));
            return String.format("<div class=\"input-group\"><span ng-class=\"styleFor('{3}', null)\">{0}</span>{1}{2}</div>", apiMember.label, result, isNull, apiMember.id);
        }

        scope.supportedPropertyNewValues[propertyName] = apiMember.createInstance();
        return String.format(
            "<div class=\"input-group\" ng-repeat=\"value in {1}{4}\">" + 
                "<span ng-class=\"styleFor('{3}', $index)\">{0}</span>" +
                "{2}" +
                "<span class=\"input-group-btn\"><button class=\"btn btn-default\" ng-click=\"removePropertyItem('{3}', $index)\"><span class=\"glyphicon glyphicon-remove\"></span></button></span>" +
            "</div>",
            apiMember.label,
            String.format(propertySelector, propertyName),
            result.replace(" name=\"" + propertyName + "\"", " ng-attr-name=\"{{'" + propertyName + "_' + $index}}\""),
            apiMember.id,
            (literalSelector === "value" ? "" : " track by $index")) + String.format(
            "<div class=\"input-group\">" + 
                "<span ng-class=\"styleFor('{2}', -1)\">{0}</span>" +
                "{1}" +
                "<span class=\"input-group-btn\"><button class=\"btn btn-default\" ng-click=\"addPropertyItem('{2}')\"><span class=\"glyphicon glyphicon-plus\"></span></button></span>" +
            "</div>",
            apiMember.label,
            result.replace(literalSelector, "supportedPropertyNewValues['" + propertyName + "']").replace(" name=\"" + propertyName + "\"", " name=\"" + propertyName + "_new\""),
            apiMember.id);
    };
    var supportedPropertyRendererSetupPropertyScope = function(scope, apiMember) {
        scope.editedEntityNulls = {};
        scope.supportedPropertyNewValues = {};
        scope.supportedPropertyNulls = {};
        scope.supportedPropertyKeys = {};
        scope.supportedPropertyReadonly = {};
        scope.supportedProperties = apiMember.owner.supportedProperties;
        scope.isPropertyReadonly = function(supportedPropertyId) {
            var supportedProperty = scope.supportedProperties.getById(supportedPropertyId);
            var propertyName = supportedProperty.propertyName(scope.operation);
            var instance = scope[scope.targetInstance];
            if ((instance !== null) && (instance[propertyName] !== null)) {
                delete scope.supportedPropertyNulls[supportedPropertyId];
            }

            return scope.supportedPropertyKeys[supportedPropertyId] ||
                scope.supportedPropertyNulls[supportedPropertyId] ||
                scope.supportedPropertyReadonly[supportedPropertyId];
        };
        scope.styleFor = function(supportedPropertyId, index) {
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
        scope.addPropertyItem = function(supportedPropertyId) {
            var supportedProperty = scope.supportedProperties.getById(supportedPropertyId);
            var propertyName = supportedProperty.propertyName(scope.operation);
            if (!scope[scope.targetInstance][propertyName]) {
                scope[scope.targetInstance][propertyName] = [];
            }

            scope[scope.targetInstance][propertyName].push(scope.supportedPropertyNewValues[propertyName]);
            scope.supportedPropertyNewValues[propertyName] = supportedProperty.createInstance();
        };
        scope.removePropertyItem = function(supportedPropertyId, index) {
            var supportedProperty = scope.supportedProperties.getById(supportedPropertyId);
            scope[scope.targetInstance][supportedProperty.propertyName(scope.operation)].splice(index, 1);
        };
        scope.onIsNullCheckedChanged = function(supportedPropertyId) {
            var supportedProperty = scope.supportedProperties.getById(supportedPropertyId);
            var propertyName = supportedProperty.propertyName(scope.operation);
            var instance = scope[scope.targetInstance];
            if (instance[propertyName] instanceof Array) {
                delete instance[propertyName];
            }
            else {
                instance[propertyName] = [supportedProperty.createInstance()];
            }
        };
    };
    var supportedPropertyRendererIsLiteralRange = function(apiMember) {
        return ((apiMember.range !== null) && ((apiMember.range.id.indexOf(xsd) === 0) || (apiMember.range.id.indexOf(guid) === 0)));
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

    /**
     * Default renderer for {@link ursa.model.Class}.
     * @memberof ursa.view
     * @name ClassRenderer
     * @protected
     * @class
     * @extends ursa.view.ViewRenderer
     */
    var ClassRenderer = namespace.ClassRenderer = function() {
        ViewRenderer.prototype.constructor.apply(this, arguments);
    };
    ClassRenderer.prototype = new ViewRenderer(_ctor);
    ClassRenderer.prototype.constructor = ClassRenderer;
    ClassRenderer.prototype.isApplicableTo = function(apiMember) {
        ViewRenderer.prototype.isApplicableTo.apply(this, arguments);
        return apiMember instanceof ursa.model.Class;
    };
    ClassRenderer.prototype.render = function(scope, http, jsonld, authentication, apiMember, classNames) {
        ViewRenderer.prototype.render.apply(this, arguments);
        var result = String.format(
            "<div class=\"panel{0}\"><div class=\"panel-body\"><form{1}>",
            (typeof (classNames) === "string" ? " " + classNames : ""),
            (scope.uniqueId ? " name=\"" + scope.uniqueId + "\"" : ""));
        for (var index = 0; index < apiMember.supportedProperties.length; index++) {
            var supportedProperty = apiMember.supportedProperties[index];
            if (!supportedProperty.readable) {
                continue;
            }

            var viewRenderer = ViewRenderer.viewRenderers.find(supportedProperty);
            if (viewRenderer === null) {
                continue;
            }

            var propertyView = viewRenderer.render(scope, http, jsonld, authentication, supportedProperty);
            if (scope.targetInstance === "editedEntity") {
                propertyView = propertyView.replace(new RegExp("supportedPropertyNulls", "g"), "editedEntityNulls");
            }

            result += propertyView;
        }

        result += "</form></div></div>";
        return result;
    };

    var bodylessVerbs = ["DELETE", "HEAD", "OPTIONS"];

    /**
     * Default renderer for {@link ursa.model.Operation}.
     * @memberof ursa.view
     * @name OperationRenderer
     * @protected
     * @class
     * @extends ursa.view.ViewRenderer
     */
    var OperationRenderer = namespace.OperationRenderer = function() {
        ViewRenderer.prototype.constructor.apply(this, arguments);
    };
    OperationRenderer.prototype = new ViewRenderer(_ctor);
    OperationRenderer.prototype.constructor = OperationRenderer;
    OperationRenderer.prototype.isApplicableTo = function(apiMember) {
        ViewRenderer.prototype.isApplicableTo.apply(this, arguments);
        return apiMember instanceof ursa.model.Operation;
    };
    OperationRenderer.prototype.render = function(scope, http, jsonld, authentication, apiMember, classNames) {
        ViewRenderer.prototype.render.apply(this, arguments);
        if (apiMember.owner.maxOccurances === Number.MAX_VALUE) {
            return operationRendererRenderEntityList.call(this, scope, http, jsonld, authentication, apiMember, classNames);
        }

        return "";
    };
    var operationRendererRenderEntityList = function(scope, http, jsonld, authentication, apiMember, classNames) {
        if (!scope.supportedProperties) {
            operationRendererSetupListScope.call(this, scope, http, jsonld, authentication, apiMember);
        }

        var pager = "";
        var pages = "Action";
        if (scope.itemsPerPageList) {
            pager = String.format(
                "<tr>" +
                    "<td colspan=\"{0}\"><nav><ul class=\"pagination\">" +
                        "<li ng-hide=\"currentPage <= 1\"><a href ng-click=\"list(currentPage - 1)\">&laquo;</a></li>" +
                        "<li ng-repeat=\"page in pages\"><a href ng-click=\"list(page)\">{{ page }}</a></li>" +
                        "<li ng-hide=\"currentPage >= pages.length\"><a href ng-click=\"list(currentPage + 1)\">&raquo;</a></li>" +
                    "</ul></nav></td>" +
                "</tr>", scope.supportedProperties.length);
            pages = "<select ng-model=\"itemsPerPage\" ng-change=\"list(1)\" ng-options=\"take for take in itemsPerPageList track by take\"></select>";
        }

        var result = String.format(
            "<table class=\"table table-condensed table-bordered table-hover{0}\">", (classNames !== undefined) && (classNames !== null) ? " " + classNames : "") + String.format(
                "<tr>" +
                    "<th ng-repeat=\"supportedProperty in supportedProperties track by supportedProperty.id\" ng-hide=\"supportedProperty.key\">{{ supportedProperty.label }}</th>" +
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
    var operationRendererSetupListScope = function(scope, http, jsonld, authentication, apiMember) {
        var that = this;
        scope.uniqueId = [];
        scope.deleteOperation = operationRendererFindEntityCrudOperation.call(this, apiMember, "DELETE");
        scope.updateOperation = operationRendererFindEntityCrudOperation.call(this, apiMember, "PUT");
        scope.getOperation = operationRendererFindEntityCrudOperation.call(this, apiMember, "GET");
        if ((scope.createOperation = operationRendererFindEntityCrudOperation.call(this, apiMember, "POST")) !== null) {
            scope.newInstance = apiMember.owner.createInstance(apiMember);
            scope.footerVisible = false;
        }

        if (((scope.operation = apiMember).mappings !== null) && (apiMember.mappings.getByProperty(ursa + "skip", "property")) &&
            (apiMember.mappings.getByProperty(ursa + "take", "property"))) {
            scope.itemsPerPageList = [scope.itemsPerPage = 10, 20, 50, 100];
            scope.pages = [];
            scope.currentPage = 1;
            scope.totalEntities = 0;
        }

        scope.keyProperty = (apiMember.isRdf ? "@id" : "");
        scope.supportedProperties = new ursa.model.ApiMemberCollection();
        for (var index = 0; index < apiMember.owner.supportedProperties.length; index++) {
            var supportedProperty = apiMember.owner.supportedProperties[index];
            if (supportedProperty.key) {
                scope.keyProperty = supportedProperty.propertyName(apiMember) + (apiMember.isRdf ? "'][0]['@value" : "");
            }

            if ((supportedProperty.maxOccurances === 1) && (supportedProperty.readable)) {
                scope.supportedProperties.push(supportedProperty);
            }
        }

        scope.entities = null;
        scope.editedEntity = null;
        scope.getPropertyValue = function(entity, supportedProperty, operation) { return entity[supportedProperty.propertyName(operation)]; };
        scope.cancel = function() { scope.editedEntity = null; };
        scope.list = function(page) { operationRendererLoadEntityList.call(that, scope, http, jsonld, authentication, scope.operation, null, page); };
        scope.get = function(instance) { operationRendererLoadEntity.call(that, scope, http, jsonld, authentication, scope.getOperation, instance); };
        scope.create = function(instance) { operationRendererCreateEntity.call(that, scope, http, jsonld, authentication, scope.createOperation, instance); };
        scope.update = function(instance) { perationRendererUpdateEntity.call(that, scope, http, jsonld, authentication, scope.updateOperation, instance); };
        scope.delete = function(instance, e) { operationRendererDeleteEntity.call(that, scope, http, jsonld, authentication, scope.deleteOperation, instance, e); };
        scope.entityEquals = function(entity) { return operationRendererEntityEquals.call(that, entity, scope.editedEntity, scope.operation); };
        scope.initialize = function(index) { operationRendererInitialize.call(that, scope, index); };
        scope.isFormDisabled = function(name) { return operationRendererIsFormDisabled.call(that, scope, name); };
    };
    var operationRendererFindEntityCrudOperation = function(apiMember, method) {
        var supportedOperations = apiMember.owner.supportedOperations;
        for (var index = 0; index < supportedOperations.length; index++) {
            var operation = supportedOperations[index];
            if (operation.methods.indexOf(method) !== -1) {
                return operation;
            }
        }

        return null;
    };
    var operationRendererOnLoadEntitySuccess = function(scope, http, jsonld, authentication, createOperation, instance, request, response) {
        if ((response.headers("Content-Type") || "*/*").indexOf(ursa.model.EntityFormat.ApplicationLdJson) === 0) {
            jsonld.expand(response.data).
                then(function(expanded) {
                    scope.editedEntity = (expanded.length > 0 ? expanded[0] : null);
                });
        }
        else {
            scope.editedEntity = response.data;
        }
    };
    var operationRendererLoadEntity = function (scope, http, jsonld, authentication, getOperation, instance) {
        operationRendererHandleOperation.call(this, scope, http, jsonld, authentication, getOperation, null, instance, operationRendererOnLoadEntitySuccess, null, operationRendererLoadEntity);
    };
    var operationRendererCreateEntitySuccess = function(scope, http, jsonld, authentication, createOperation) {
        scope.newInstance = createOperation.owner.createInstance(createOperation);
        scope.list(1);
        scope.footerVisible = false;
    };
    var operationRendererCreateEntityFailure = function(scope, http, jsonld, authentication, createOperation, instance, request) {
        if (request._isInstanceIdSet) {
            delete instance["@id"];
        }
    };
    var operationRendererCreateEntity = function(scope, http, jsonld, authentication, createOperation, instance) {
        if (!scope.footerVisible) {
            scope.footerVisible = true;
            return;
        }

        operationRendererHandleOperation.call(this, scope, http, jsonld, authentication, createOperation, null, instance, operationRendererCreateEntitySuccess, operationRendererCreateEntityFailure, operationRendererCreateEntity);
    };
    var operationRendererOnUpdateEntitySuccess = function(scope) {
        scope.editedEntity = null;
        scope.list();
    };
    var operationRendererOnUpdateEntityFailure = function(scope, http, jsonld, authentication, updateOperation, instance, request) {
        if (request._isInstanceIdSet) {
            delete instance["@id"];
        }
    };
    var perationRendererUpdateEntity = function(scope, http, jsonld, authentication, updateOperation, instance) {
        operationRendererHandleOperation.call(this, scope, http, jsonld, authentication, updateOperation, null, instance, operationRendererOnUpdateEntitySuccess, operationRendererOnUpdateEntityFailure, perationRendererUpdateEntity);
    };
    var operationRendererOnDeleteEntitySuccess = function(scope) {
        scope.list(1);
    };
    var operationRendererDeleteEntity = function(scope, http, jsonld, authentication, deleteOperation, instance, e) {
        if ((e) && (!confirm("Are you sure you want to delete this item?"))) {
            e.preventDefault();
            e.stopPropagation();
            return;
        }

        operationRendererHandleOperation.call(this, scope, http, jsonld, authentication, deleteOperation, null, instance, operationRendererOnDeleteEntitySuccess, null, operationRendererDeleteEntity);
    };
    var operationRendererOnLoadEntityListSuccess = function(scope, http, jsonld, authentication, deleteOperation, instance, request, response) {
        var contentRange = response.headers("Content-Range");
        if ((contentRange !== undefined) && (contentRange !== null)) {
            var matches = contentRange.match(/^members ([0-9]+)-([0-9]+)\/([0-9]+)/);
            if ((matches !== null) && (matches.length === 4)) {
                var startIndex = parseInt(matches[1]);
                scope.totalEntities = parseInt(matches[3]);
                scope.currentPage = Math.ceil(startIndex / scope.itemsPerPage) + 1;
                scope.pages = [];
                for (var pageIndex = 1; pageIndex <= Math.ceil(scope.totalEntities / scope.itemsPerPage) ; pageIndex++) {
                    scope.pages.push(pageIndex);
                }
            }
        }

        if ((response.headers("Content-Type") || "*/*").indexOf(ursa.model.EntityFormat.ApplicationLdJson) === 0) {
            jsonld.expand(response.data).
                then(function (expanded) {
                    scope.entities = expanded;
                });
        }
        else {
            scope.entities = response.data;
        }
    };
    var operationRendererLoadEntityList = function (scope, http, jsonld, authentication, listOperation, instance, page) {
        var candidateMethod = operationRendererFindEntityListMethod.call(this, listOperation);
        if (candidateMethod === null) {
            return;
        }

        if ((instance === null) && (listOperation.mappings !== null) && (listOperation.mappings.getByProperty(ursa.skip, "property") !== null) &&
            (listOperation.mappings.getByProperty(ursa.take, "property") !== null)) {
            if (typeof(page) !== "number") {
                page = scope.currentPage;
            }

            instance = {};
            instance[(listOperation.isRdf ? ursa : "") + "skip"] = (listOperation.isRdf ? [{ "@value": (page - 1) * scope.itemsPerPage }] : (page - 1) * scope.itemsPerPage);
            instance[(listOperation.isRdf ? ursa : "") + "take"] = (listOperation.isRdf ? [{ "@value": scope.itemsPerPage }] : scope.itemsPerPage);
        }

        operationRendererHandleOperation.call(this, scope, http, jsonld, authentication, listOperation, candidateMethod, instance, operationRendererOnLoadEntityListSuccess, null, operationRendererLoadEntityList, page);
    };
    var operationRendererEntityEquals = function(leftOperand, rightOperand, operation) {
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
    var operationRendererFindEntityListMethod = function(listOperation) {
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
    var operationRendererSanitizeEntity = function(instance, operation) {
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
                        operationRendererSanitizeEntity.call(this, instance[property][index], operation);
                    }
                }
            }
        }

        return instance;
    };
    var operationRendererInitialize = function(scope, index) {
        if (index === -1) {
            scope.uniqueId.footer = "instance_" + Math.random().toString().replace(".", "").substr(1);
        }
        else {
            scope.uniqueId[index] = "instance_" + Math.random().toString().replace(".", "").substr(1);
        }
    };
    var operationRendererIsFormDisabled = function(scope, name) {
        var forms = document.getElementsByName(name);
        return (forms.length > 0 ? angular.element(forms[0]).scope()[name].$invalid : true) && (scope.footerVisible);
    };
    var operationRendererHandleUnauthorized = function(scope, authentication, challenge, callback) {
        if (this._authenticationEventHandler) {
            this._authenticationEventHandler();
            delete this._authenticationEventHandler;
        }

        var that = this;
        this._authenticationEventHandler = scope.$on(Events.Authenticate, function(e, userName, password) {
            authentication[challenge](userName, password).
                then(function(authorization) {
                    that._authorization = authorization;
                    callback();
                });
        });

        scope.$root.$broadcast(Events.AuthenticationRequired);
    };
    var operationRendererHandleAuthorized = function(scope, authentication) {
        if (this._authenticationEventHandler) {
            this._authenticationEventHandler();
            delete this._authenticationEventHandler;
        }

        if (this._authorization) {
            authentication.use(this._authorization);
            delete this._authorization;
        }

        scope.$root.$broadcast(Events.Authenticated);
    };
    var operationRendererPrepareRequest = function (operation, methodOverride, instance) {
        var request = {
            method: methodOverride || operation.methods[0],
            headers: { Accept: operation.mediaTypes.join() },
            _isInstanceIdSet: false
        };

        if ((operation.mediaTypes.length > 0) && (bodylessVerbs.indexOf(operation.methods[0]) === -1)) {
            request.headers["Content-Type"] = operation.mediaTypes[0];
        }

        var url = operation.createCallUrl(instance);
        if ((operation.isRdf) && ((instance["@id"] === undefined) || (instance["@id"] === null) || (instance["@id"] === ""))) {
            instance["@id"] = url;
            request._isInstanceIdSet = true;
        }

        request.url = url;
        request.data = JSON.stringify(operationRendererSanitizeEntity.call(this, instance, operation));
        if (this._authorization) {
            request.headers.Authorization = this._authorization;
        }

        return request;
    };
    var operationRendererHandleOperation = function(scope, http, jsonld, authentication, operation, methodOverride, instance, success, failure, callbackMethod, context) {
        var that = this;
        var request = operationRendererPrepareRequest.call(this, operation, methodOverride, instance);
        var promise = http(request).
            then(function(response) {
                if (typeof(success) === "function") {
                    success.call(that, scope, http, jsonld, authentication, operation, instance, request, response);
                }

                operationRendererHandleAuthorized.call(that, scope, authentication);
                return response;
            });

        promise.catch(function(response) {
            if (typeof(failure) === "function") {
                failure.call(that, scope, http, jsonld, authentication, operation, instance, request, response);
            }

            if (response.status === ursa.model.HttpStatusCodes.Unauthorized) {
                if (that._authenticationEventHandler) {
                    scope.$root.$broadcast(Events.AuthenticationFailed, response.statusText);
                }
                else {
                    var callback = function() { callbackMethod.call(that, scope, http, jsonld, authentication, operation, instance, context); };
                    var challenge = ((response.headers("WWW-Authenticate")) || (response.headers("X-WWW-Authenticate"))).split(/[ ;]/g)[0].toLowerCase();
                    operationRendererHandleUnauthorized.call(that, scope, authentication, challenge, callback);
                }
            }

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
        Authenticated: "Authenticated"
    };
}(namespace("ursa.view")));