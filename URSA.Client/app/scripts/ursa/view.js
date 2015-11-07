/*globals namespace, container, xsd, guid, ursa, jsonld */
(function (namespace) {
    "use strict";

    var invalidArgumentPassed = "Invalid {0} passed.";

    var _ctor = "_ctor";

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
    var ViewRendererCollection = namespace.ViewRendererCollection = function() { };
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
    ViewRenderer.prototype.render = function(scope, http, jsonld, apiMember, classNames) {
        if ((apiMember === undefined) || (apiMember === null) || (!(apiMember instanceof ursa.model.ApiMember))) {
            throw new Error(invalidArgumentPassed.replace("{0}", "apiMember"));
        }

        return String.format("<div{0}></div>", (typeof(classNames) === "string" ? String.format(" class=\"{0}\"", classNames) : ""));
    };
    var isInitialized = false;
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
    SupportedPropertyRenderer.prototype.render = function(scope, http, jsonld, apiMember, classNames) {
        ViewRenderer.prototype.render.apply(this, arguments);
        if (!(apiMember instanceof ursa.model.SupportedProperty)) {
            throw new Error(invalidArgumentPassed.replace("{0}", "apiMember"));
        }

        if (!scope.supportedPropertyNewValues) {
            this._setupPropertyScope(scope, apiMember);
        }

        classNames = String.format("class=\"form-control{0}\" ", (typeof(classNames) === "string" ? classNames : ""));
        var propertyName = apiMember.propertyName(scope.operation);
        var propertySelector = scope.targetInstance + (!scope.operation.isRdf ? "['{0}']" : "['{0}']" + (apiMember.maxOccurances > 1 ? "" : "[0]['@value']"));
        var literalSelector = (this._isLiteralRange(apiMember) ? String.format(propertySelector, propertyName) + "[$index]" : "value");
        var valueSelector = (apiMember.maxOccurances <= 1 ? propertySelector : literalSelector + (!scope.operation.isRdf ? "" : "['@value']"));
        if (apiMember.key) {
            scope.supportedPropertyKeys[apiMember.id] = true;
        }

        var format = String.format("<input {0}ng-model=\"{1}\" ng-readonly=\"supportedPropertyKeys['{2}'] || supportedPropertyNulls['{2}']\" ", classNames, valueSelector, apiMember.id);
        var parameters = [propertyName];
        var dataType = SupportedPropertyRenderer.dataTypes[apiMember.range.id] || { type: "text" };
        for (var property in dataType) {
            if ((dataType.hasOwnProperty(property)) && (dataType[property] !== undefined) && (dataType[property] !== null)) {
                format += property + "=\"{" + parameters.length + "}\" ";
                parameters.push(dataType[property]);
            }
        }

        format += "/>";
        parameters.splice(0, 0, format);
        var result = String.format.apply(window, parameters);
        if (apiMember.maxOccurances === 1) {
            var isNull = (!(scope.supportedPropertyNulls[apiMember.id] = (apiMember.minOccurances === 0)) ? "" : String.format(
                "<span class=\"input-group-addon\"><input type=\"checkbox\" title=\"Null\" checked ng-model=\"supportedPropertyNulls['{0}']\" ng-change=\"onIsNullCheckedChanged('{0}')\" /></span>", apiMember.id));
            return String.format("<div class=\"input-group\"><span class=\"input-group-addon\">{0}</span>{1}{2}</div>", apiMember.label, result, isNull);
        }

        scope.supportedPropertyNewValues[propertyName] = apiMember.createInstance();
        return String.format(
            "<div class=\"input-group\" ng-repeat=\"value in {1}{4}\">" + 
                "<span class=\"input-group-addon\">{0}</span>" +
                "{2}" +
                "<span class=\"input-group-btn\"><button class=\"btn btn-default\" ng-click=\"removePropertyItem('{3}', $index)\"><span class=\"glyphicon glyphicon-remove\"></span></button></span>" +
            "</div>", apiMember.label, String.format(propertySelector, propertyName), result, apiMember.id, (literalSelector === "value" ? "" : " track by value")) + String.format(
            "<div class=\"input-group\">" + 
                "<span class=\"input-group-addon\">{0}</span>" +
                "{1}" +
                "<span class=\"input-group-btn\"><button class=\"btn btn-default\" ng-click=\"addPropertyItem('{2}')\"><span class=\"glyphicon glyphicon-plus\"></span></button></span>" +
            "</div>", apiMember.label, result.replace(literalSelector, "supportedPropertyNewValues['" + propertyName + "']"), apiMember.id);
    };
    SupportedPropertyRenderer.prototype._setupPropertyScope = function(scope, apiMember) {
        scope.editedEntityNulls = {};
        scope.supportedPropertyNewValues = {};
        scope.supportedPropertyNulls = {};
        scope.supportedPropertyKeys = {};
        scope.supportedProperties = apiMember.owner.supportedProperties;
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
    SupportedPropertyRenderer.prototype._isLiteralRange = function(apiMember) {
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
    SupportedPropertyRenderer.dataTypes[guid.guid] = new DatatypeDescriptor("text", null, null, null, "00000000-0000-0000-0000-000000000000", "^[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}$");

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
    ClassRenderer.prototype.render = function(scope, http, jsonld, apiMember, classNames) {
        ViewRenderer.prototype.render.apply(this, arguments);
        var result = String.format("<div class=\"panel{0}\"><div class=\"panel-body\">", (typeof (classNames) === "string" ? " " + classNames : ""));
        for (var index = 0; index < apiMember.supportedProperties.length; index++) {
            var supportedProperty = apiMember.supportedProperties[index];
            var viewRenderer = ViewRenderer.viewRenderers.find(supportedProperty);
            if (viewRenderer === null) {
                continue;
            }

            var propertyView = viewRenderer.render(scope, http, jsonld, supportedProperty);
            if (scope.targetInstance === "editedEntity") {
                propertyView = propertyView.replace(new RegExp("supportedPropertyNulls", "g"), "editedEntityNulls");
            }

            result += propertyView;
        }

        result += "</div></div>";
        return result;
    };

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
    OperationRenderer.prototype.render = function(scope, http, jsonld, apiMember, classNames) {
        ViewRenderer.prototype.render.apply(this, arguments);
        if (this.isEntityList(apiMember)) {
            return this.renderEntityList(scope, http, jsonld, apiMember, classNames);
        }

        return "";
    };
    OperationRenderer.prototype.isEntityList = function(apiMember) {
        return apiMember.owner.maxOccurances === Number.MAX_VALUE;
    };
    OperationRenderer.prototype.renderEntityList = function(scope, http, jsonld, apiMember, classNames) {
        if (!scope.supportedProperties) {
            this.setupListScope(scope, http, jsonld, apiMember);
        }

        var keyProperty = (apiMember.isRdf ? "@id" : "");
        for (var index = 0; index < apiMember.owner.supportedProperties.length; index++) {
            var supportedProperty = apiMember.owner.supportedProperties[index];
            if (supportedProperty.key) {
                keyProperty = supportedProperty.propertyName(apiMember) + (apiMember.isRdf ? "'][0]['@value" : "");
                break;
            }
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
                "<tr ng-repeat-start=\"entity in entities{1}\" ng-hide=\"entityEquals(entity)\"" + (keyProperty !== "" ? " title=\"{{ entity['{0}'] | asId }}\"" : "") + ">" + 
                    "<td ng-repeat=\"supportedProperty in supportedProperties track by supportedProperty.id\" ng-hide=\"supportedProperty.key\">{{ getPropertyValue(entity, supportedProperty, operation) | stringify }}</td>" +
                    "<td><div class=\"btn-block\">" +
                        (scope.getOperation !== null ? "<button class=\"btn btn-default\" title=\"Edit\" ng-click=\"get(entity)\"><span class=\"glyphicon glyphicon-pencil\"></span></button>" : "") +
                        (scope.deleteOperation !== null ? "<button class=\"btn btn-default\" title=\"Delete\" ng-click=\"delete(entity, $event)\"><span class=\"glyphicon glyphicon-remove\"></span></button>" : "") +
                    "</div></td>" +
                "</tr>", keyProperty, (keyProperty !== "" ? " track by entity['" + keyProperty + "']" : "")) + String.format(
                "<tr ng-repeat-end ng-show=\"entityEquals(entity)\">" +
                    "<td colspan=\"{0}\"><ursa-api-member-view api-member=\"operation.owner\" target-instance=\"editedEntity\"></ursa-api-member-view></td>" +
                    "<td><div class=\"btn-block\">" +
                        (scope.updateOperation !== null ? "<button class=\"btn btn-default\" title=\"Update\" ng-click=\"update(editedEntity)\"><span class=\"glyphicon glyphicon-floppy-disk\"></span></button>" : "") +
                        "<button class=\"btn btn-default\" title=\"Cancel\" ng-click=\"cancel()\"><span class=\"glyphicon glyphicon-repeat\"></span></button>" +
                    "</div></td>" +
                "</tr>", scope.supportedProperties.length - 1) + String.format(
                "<tr ng-hide=\"editedEntity !== null\">" +
                    "<td colspan=\"{0}\"><ursa-api-member-view api-member=\"operation.owner\" target-instance=\"newInstance\"></ursa-api-member-view></td>" +
                    "<td><div class=\"btn-block\">" +
                        (scope.createOperation !== null ? "<button class=\"btn btn-default\" title=\"Create\" ng-click=\"create(newInstance)\"><span class=\"glyphicon glyphicon-plus\"></span></button>" : "") +
                    "</div></td>" +
                "</tr>", scope.supportedProperties.length - 1) +
                pager +
            "</table>";
        scope.list();
        return result;
    };
    OperationRenderer.prototype.setupListScope = function(scope, http, jsonld, apiMember) {
        var that = this;
        scope.deleteOperation = this._findEntityCrudOperation(apiMember, "DELETE");
        scope.updateOperation = this._findEntityCrudOperation(apiMember, "PUT");
        scope.getOperation = this._findEntityCrudOperation(apiMember, "GET");
        if ((scope.createOperation = this._findEntityCrudOperation(apiMember, "POST")) !== null) {
            scope.newInstance = apiMember.owner.createInstance(apiMember);
        }

        if (((scope.operation = apiMember).mappings !== null) && (apiMember.mappings.getByProperty(ursa + "skip", "property")) &&
            (apiMember.mappings.getByProperty(ursa + "take", "property"))) {
            scope.itemsPerPageList = [scope.itemsPerPage = 10, 20, 50, 100];
            scope.pages = [];
            scope.currentPage = 1;
            scope.totalEntities = 0;
        }

        scope.supportedProperties = new ursa.model.ApiMemberCollection();
        for (var index = 0; index < apiMember.owner.supportedProperties.length; index++) {
            var supportedProperty = apiMember.owner.supportedProperties[index];
            if (supportedProperty.maxOccurances === 1) {
                scope.supportedProperties.push(supportedProperty);
            }
        }

        scope.entities = null;
        scope.editedEntity = null;
        scope.getPropertyValue = function(entity, supportedProperty, operation) { return entity[supportedProperty.propertyName(operation)]; };
        scope.cancel = function() { scope.editedEntity = null };
        scope.list = function(page) { that._loadEntityList(scope, http, jsonld, scope.operation, page); };
        scope.get = function(instance) { that._loadEntity(scope, http, jsonld, scope.getOperation, instance); };
        scope.create = function(instance) { that._createEntity(scope, http, jsonld, scope.createOperation, instance); };
        scope.update = function(instance) { that._updateEntity(scope, http, jsonld, scope.updateOperation, instance); };
        scope.delete = function(instance, e) { that._deleteEntity(scope, http, jsonld, scope.deleteOperation, instance, e); };
        scope.entityEquals = function(entity) { return that._entityEquals(entity, scope.editedEntity, scope.operation); };
    };
    OperationRenderer.prototype._findEntityCrudOperation = function(apiMember, method) {
        var supportedOperations = apiMember.owner.supportedOperations;
        for (var index = 0; index < supportedOperations.length; index++) {
            var operation = supportedOperations[index];
            if (operation.methods.indexOf(method) !== -1) {
                return operation;
            }
        }

        return null;
    };
    OperationRenderer.prototype._loadEntity = function(scope, http, jsonld, getOperation, instance) {
        http({ method: getOperation.methods[0], url: getOperation.createCallUrl(instance), headers: { Accept: getOperation.mediaTypes.join() } }).
            then(function(result) {
                if ((result.headers("Content-Type") || "*/*").indexOf(ursa.model.EntityFormat.ApplicationLdJson) === 0) {
                    jsonld.expand(result.data).
                        then(function(expanded) {
                            scope.editedEntity = (expanded.length > 0 ? expanded[0] : null);
                        });
                }
                else {
                    scope.editedEntity = result.data;
                }
            });
    };
    OperationRenderer.prototype._createEntity = function (scope, http, jsonld, createOperation, instance) {
        var initialId = null;
        if (createOperation.isRdf) {
            instance["@id"] = createOperation.createCallUrl(instance).replace(/#.*$/, "");
        }

        http({
                method: createOperation.methods[0],
                url: createOperation.createCallUrl(instance),
                headers: { "Content-Type": createOperation.mediaTypes[0], Accept: createOperation.mediaTypes.join() },
                data: JSON.stringify(this._sanitizeEntity(instance, createOperation))
            }).
            then(function() {
                scope.newInstance = createOperation.owner.createInstance(createOperation);
                scope.list(1);
            }).
            catch(function() {
                if (initialId !== null) {
                    instance["@id"] = initialId;
                }
            });
    };
    OperationRenderer.prototype._updateEntity = function(scope, http, jsonld, updateOperation, instance) {
        http({
                method: updateOperation.methods[0],
                url: updateOperation.createCallUrl(instance),
                headers: { "Content-Type": updateOperation.mediaTypes[0], Accept: updateOperation.mediaTypes.join() },
                data: JSON.stringify(this._sanitizeEntity(instance, updateOperation))
            }).
            then(function() {
                scope.editedEntity = null;
                scope.list();
            });
    };
    OperationRenderer.prototype._deleteEntity = function(scope, http, jsonld, deleteOperation, instance, e) {
        if (!confirm("Are you sure you want to delete this item?")) {
            e.preventDefault();
            e.stopPropagation();
            return;
        }

        http({
                method: deleteOperation.methods[0],
                url: deleteOperation.createCallUrl(instance),
                headers: { Accept: deleteOperation.mediaTypes.join() }
            }).
            then(function() {
                scope.list(1);
            });
    };
    OperationRenderer.prototype._loadEntityList = function(scope, http, jsonld, listOperation, page) {
        var candidateMethod = this._findEntityListMethod(listOperation);
        if (candidateMethod !== null) {
            var instance = null;
            if ((listOperation.mappings != null) && (listOperation.mappings.getByProperty(ursa + "skip", "property") != null) &&
                (listOperation.mappings.getByProperty(ursa + "take", "property") != null)) {
                if (typeof(page) !== "number") {
                    page = scope.currentPage;
                }

                instance = {};
                instance[(listOperation.isRdf ? ursa : "") + "skip"] = (listOperation.isRdf ? [{ "@value": (page - 1) * scope.itemsPerPage }] : (page - 1) * scope.itemsPerPage);
                instance[(listOperation.isRdf ? ursa : "") + "take"] = (listOperation.isRdf ? [{ "@value": scope.itemsPerPage }] : scope.itemsPerPage);
            }

            http({ url: listOperation.createCallUrl(instance), method: candidateMethod, headers: { Accept: listOperation.mediaTypes.join() } }).
                then(function(result) {
                    var contentRange = result.headers("Content-Range");
                    if ((contentRange !== undefined) && (contentRange !== null)) {
                        var matches = contentRange.match(/^members ([0-9]+)-([0-9]+)\/([0-9]+)/);
                        if ((matches !== null) && (matches.length === 4)) {
                            var startIndex = parseInt(matches[1]);
                            scope.totalEntities = parseInt(matches[3]);
                            scope.currentPage = Math.ceil(startIndex / scope.itemsPerPage) + 1;
                            scope.pages = [];
                            for (var pageIndex = 1; pageIndex <= Math.ceil(scope.totalEntities / scope.itemsPerPage); pageIndex++) {
                                scope.pages.push(pageIndex);
                            }
                        }
                    }

                    if ((result.headers("Content-Type") || "*/*").indexOf(ursa.model.EntityFormat.ApplicationLdJson) === 0) {
                        jsonld.expand(result.data).
                            then(function(expanded) {
                                scope.entities = expanded;
                            });
                    }
                    else {
                        scope.entities = result.data;
                    }
                });
        }
    };
    OperationRenderer.prototype._entityEquals = function(leftOperand, rightOperand, operation) {
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
    OperationRenderer.prototype._findEntityListMethod = function(listOperation) {
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
    OperationRenderer.prototype._sanitizeEntity = function(instance, operation) {
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
                        this._sanitizeEntity(instance[property][index], operation);
                    }
                }
            }
        }

        return instance;
    };
}(namespace("ursa.view")));