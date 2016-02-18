(function (namespace) {
    "use strict";

    var _ctor = "ctor";
    var invalidArgumentPassed = "Invalid {0} passed.";

    var DateTimeRegExp = function (regex, format, year, month, day, hour, minute, second) {
        this._regex = regex;
        this.format = format;
        this.year = year || -1;
        this.month = month || -1;
        this.day = day || -1;
        this.hour = hour || -1;
        this.minute = minute || -1;
        this.second = second || -1;
    };
    DateTimeRegExp.prototype.constructor = DateTimeRegExp;
    DateTimeRegExp.prototype.format = null;
    DateTimeRegExp.prototype.year = -1;
    DateTimeRegExp.prototype.month = -1;
    DateTimeRegExp.prototype.day = -1;
    DateTimeRegExp.prototype.hour = -1;
    DateTimeRegExp.prototype.minute = -1;
    DateTimeRegExp.prototype.second = -1;
    DateTimeRegExp.prototype._regex = null;
    DateTimeRegExp.prototype.test = function(value) { return this._regex.test(value); }
    DateTimeRegExp.prototype.exec = function(value) { return this._regex.exec(value); };
    var YearMonthDayHourMinuteSecondRegex = new DateTimeRegExp(
        /^([\-0-9]{4})[^0-9]+([0-1]?[0-9])[^0-9]+([0-3]?[0-9])[^0-9]+([0-2]?[0-9])[^0-9]+([0-5]?[0-9])[^0-9]+([0-5]?[0-9])$/,
        "({property} eq DateTime'{year}-{month}-{day}T{hour}:{minute}:{second})", 1, 2, 3, 4, 5, 6);
    var YearMonthDayHourMinuteRegex = new DateTimeRegExp(
        /^([\-0-9]{4})[^0-9]+([0-1]?[0-9])[^0-9]+([0-3]?[0-9])[^0-9]+([0-2]?[0-9])[^0-9]+([0-5]?[0-9])$/,
        "({property} ge DateTime'{year}-{month}-{day}T{hour}:{minute}:00.000 and {property} le DateTime'{year}-{month}-{day}T{hour}:{minute}:59.000)", 1, 2, 3, 4, 5);
    var YearMonthDayHourRegex = new DateTimeRegExp(
        /^([\-0-9]{4})[^0-9]+([0-1]?[0-9])[^0-9]+([0-3]?[0-9])[^0-9]+([0-2]?[0-9])$/,
        "({property} ge DateTime'{year}-{month}-{day}T{hour}:00:00.000 and {property} le DateTime'{year}-{month}-{day}T{hour}:59:59.000)", 1, 2, 3, 4);
    var DayMonthYearHourMinuteSecondRegex = new DateTimeRegExp(
        /^([0-3]?[0-9])[^0-9]+([0-1]?[0-9])[^0-9]+([0-9]{4})[^0-9]+([0-2]?[0-9])[^0-9]+([0-5]?[0-9])[^0-9]+([0-5]?[0-9])$/,
        "({property} eq DateTime'{year}-{month}-{day}T{hour}:{minute}:{second})", 3, 2, 1, 4, 5, 6);
    var DayMonthYearHourMinuteRegex = new DateTimeRegExp(
        /^([0-3]?[0-9])[^0-9]+([0-1]?[0-9])[^0-9]+([0-9]{4})[^0-9]+([0-2]?[0-9])[^0-9]+([0-5]?[0-9])$/,
        "({property} ge DateTime'{year}-{month}-{day}T{hour}:{minute}:00.000 and {property} le DateTime'{year}-{month}-{day}T{hour}:{minute}:59.000)", 3, 2, 1, 4, 5);
    var DayMonthYearHourRegex = new DateTimeRegExp(
        /^([0-3]?[0-9])[^0-9]+([0-1]?[0-9])[^0-9]+([0-9]{4})[^0-9]+([0-2]?[0-9])$/,
        "({property} ge DateTime'{year}-{month}-{day}T{hour}:00:00.000 and {property} le DateTime'{year}-{month}-{day}T{hour}:59:59.000)", 3, 2, 1, 4);
    var YearMonthDayRegex = new DateTimeRegExp(/^([\-0-9]{4})[^0-9]+([0-1]?[0-9])[^0-9]+([0-3]?[0-9])$/,
        "({property} ge DateTime'{year}-{month}-{day}T00:00:00 and {value} le DateTime'{year}-{month}-{day}T:00:00:00')", 1, 2, 3);
    var DayMonthYearRegex = new DateTimeRegExp(/^([0-3]?[0-9])[^0-9]+([0-1]?[0-9])[^0-9]+([0-9]{4})$/,
        "({property} ge DateTime'{year}-{month}-{day}T00:00:00 and {property} lt DateTime'{year}-{month}-{day}T:00:00:00')", 3, 2, 1);
    var YearMonthRegex = new DateTimeRegExp(/^([\-0-9]{4})[^0-9]+([0-1]?[0-9])$/, "(year({property}) eq {year} and month({property}) eq {month})", 1, 2);
    var MonthYearRegex = new DateTimeRegExp(/^([0-1]?[0-9])[^0-9]+([0-9]{4})$/, "(year({property}) eq {year} and month({property}) eq {month})", 2, 1);
    var YearRegex = new DateTimeRegExp(/^([\-0-9]{4})$/, "(year({property}) eq {year})", 1);
    var MonthRegex = new DateTimeRegExp(/^([0-1]?[0-9])$/, "(month({property}) eq {month})", -1, 1);
    var DayRegex = new DateTimeRegExp(/^([0-3]?[0-9])$/, "(day({property}) eq {day})", -1, -1, 1);
    var HourMinuteSecondRegex = new DateTimeRegExp(/^([0-2]?[0-9])[^0-9]+([0-5]?[0-9])[^0-9]+([0-5]?[0-9])$/,
        "(hour({property} eq {hour} and minute({property} eq {minute} and second({property}) eq {second})", -1, -1, -1, 1, 2, 3);
    var HourMinuteRegex = new DateTimeRegExp(/^([0-2]?[0-9])[^0-9]+([0-5]?[0-9])$/, "(hour({property} eq {hour} and minute({property} eq {minute})", -1, -1, -1, 1, 2);
    var HourRegex = new DateTimeRegExp(/^([0-2]?[0-9])$/, "(hour({property} eq {hour})", -1, -1, -1, 1);

    var properties = { year: 4, month: 2, day: 2, hour: 2, minute: 2, second: 2 };
    var RegExpArray = function() {
        Array.prototype.constructor.apply(this, arguments);
    };
    RegExpArray.prototype = [];
    RegExpArray.prototype.constructor = RegExpArray;
    RegExpArray.prototype.generateExpression = function(supportedProperty, value) {
        for (var index = 0; index < this.length; index++) {
            var regex = this[index];
            var match = regex.exec(value);
            if ((match !== null) && (match.length > 0)) {
                var result = regex.format.replace("{property}", "<" + supportedProperty.property + ">");
                for (var property in properties) {
                    var propertyValue = "";
                    if (regex[property] !== -1) {
                        propertyValue = match[regex[property]];
                        while (propertyValue.length < properties[property]) {
                            propertyValue = "0" + propertyValue;
                        }
                    }

                    result = result.replace(new RegExp("{" + property + "}"), propertyValue);
                }

                return result;
            }
        }

        return null;
    };
    DateTimeRegExp[xsd.dateTime] = new RegExpArray(YearMonthDayHourMinuteSecondRegex, YearMonthDayHourMinuteRegex, YearMonthDayHourRegex, DayMonthYearHourMinuteSecondRegex, DayMonthYearHourMinuteRegex, DayMonthYearHourRegex, YearMonthDayRegex, DayMonthYearRegex, YearMonthRegex, MonthYearRegex, YearRegex, MonthRegex, DayRegex);
    DateTimeRegExp[xsd.date] = new RegExpArray(YearMonthDayRegex, DayMonthYearRegex, YearMonthRegex, MonthYearRegex, YearRegex, MonthRegex, DayRegex);
    DateTimeRegExp[xsd.time] = new RegExpArray(HourMinuteSecondRegex, HourMinuteRegex, HourRegex);

    /**
     * Provides a basic description of property filters.
     * @memberof ursa.model
     * @name Filter
     * @public
     * @class
     * @param {ursa.model.ApiMemberCollection<ursa.model.SupportedProperty>} supportedProperties Collection of supported properties suitable for filtering.
     */
    var Filter = namespace.Filter = function(supportedProperties) {
        if ((arguments.length === 0) || (arguments[0] !== _ctor)) {
            if (!(supportedProperties instanceof ursa.model.ApiMemberCollection)) {
                throw String.format(invalidArgumentPassed, "supportedProperties");
            }

            this.supportedProperties = supportedProperties;
        }
    };
    Filter.prototype.constructor = Filter;
    /**
     * Collection of supported properties suitable for filtering.
     * @memberof ursa.model.filter
     * @instance
     * @protected
     * @member {ursa.model.ApiMemberCollection<ursa.model.SupportedProperty>} supportedProperties
     */
    Filter.prototype.supportedProperties = null;

    /**
     * Provides filter expression providers.
     * @memberof ursa.model
     * @name FilterProvider
     * @public
     * @class
     */
    var FilterProvider = namespace.FilterProvider = function() {
        ursa.ComponentProvider.prototype.constructor.call(this, FilterExpressionProvider);
    };
    FilterProvider.prototype = new ursa.ComponentProvider(_ctor);
    FilterProvider.prototype.constructor = FilterProvider;
    /**
     * Resolves a filter suitable for given property Uri.
     * @instance
     * @public
     * @method resolve
     * @param {string} predicate Property uri carrying filter expression.
     * @returns {ursa.model.FilterExpressionProvider} Instance of the applicable filter expression provider or null if no suitable ones found.
     */
    FilterProvider.prototype.resolve = function(predicate) {
        if ((predicate === undefined) || (predicate === null) || (typeof(predicate) !== "string") || (predicate.length === 0)) {
            return null;
        }

        for (var index = 0; index < this.types.length; index++) {
            var instance = ursa.ComponentProvider.prototype.resolve.call(this, this.types[index]);
            if (instance.isApplicableTo(predicate)) {
                return instance;
            }
        }

        return null;
    };

    /**
     * Provides an abstract for filter expression providers.
     * @memberof ursa.model
     * @name FilterExpressionProvider
     * @public
     * @class
     */
    var FilterExpressionProvider = namespace.FilterExpressionProvider = function() { };
    FilterExpressionProvider.prototype.constructor = FilterExpressionProvider;
    /**
     * Creates a filter expression.
     * @instance
     * @public
     * @method createFilter
     * @param {ursa.model.Filter} filter Property filters.
     * @returns {string} Expression filtering entities.
     */
    FilterExpressionProvider.prototype.createFilter = function(filter) {
        if ((filter === undefined) || (filter === null) || (!(filter instanceof Filter))) {
            return "";
        }

        var result = "";
        for (var property in filter) {
            if ((!filter.hasOwnProperty(property)) || (filter[property] === undefined) || (filter[property] === null)) {
                continue;
            }

            var supportedProperty = filter.supportedProperties.getByProperty(property, "property");
            if ((supportedProperty === null) || (supportedProperty.range === null) || (!(supportedProperty.range instanceof ursa.model.DataType))) {
                continue;
            }

            result += this.createExpression(supportedProperty, filter[property]) + " ";
        }

        return result.trim();
    };
    /**
     * Creates a property value expression.
     * @instance
     * @protected
     * @method createExpression
     * @param {ursa.model.SupportedProperty} supportedProperty Supported property.
     * @param {object} value The value.
     * @returns {string} Expression filtering given supported property with a value.
     */
    FilterExpressionProvider.prototype.createExpression = function(supportedProperty, value) { return ""; };
    /**
     * Checks if a given expression provider is applicable for given property Uri.
     * @instance
     * @public
     * @method isApplicableTo
     * @param {string} uri Property Uri carrying a filter expression.
     * @returns {boolean} True if the expression provider is applicable for the given uri; otherwise false.
     */
    FilterExpressionProvider.prototype.isApplicableTo = function(uri) { return false; };
    FilterExpressionProvider.prototype.toString = function() { return "ursa.model.FilterExpressionProvider"; };

    /**
     * Provides an OData compatibale filtering expression.
     * @memberof ursa.model
     * @name ODataFilterExpressionProvider
     * @public
     * @class
     */
    var ODataFilterExpressionProvider = namespace.ODataFilterExpressionProvider = function () {
        FilterExpressionProvider.prototype.constructor.apply(this, arguments);
    };
    ODataFilterExpressionProvider.prototype = new FilterExpressionProvider(_ctor);
    ODataFilterExpressionProvider.prototype.constructor = ODataFilterExpressionProvider;
    ODataFilterExpressionProvider.prototype.createExpression = function(supportedProperty, value) {
        switch (supportedProperty.range.id) {
            case xsd.string:
                return String.format("(indexOf(<{0}>, '{1}') ge 0)", supportedProperty.property, value.toString().replace(/'/g, "\\'"));
            case xsd.boolean:
                value = value.toString().toLowerCase();
                return ((value === "true") || (value === "1") || (value === "yes") || (value === "on") ? String.format("(<{0}> eq true)", supportedProperty.property) :
                    ((value === "false") || (value === "0") || (value === "no") || (value === "off") ?  String.format("(<{0}> eq false)", supportedProperty.property) : ""));
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
            case xsd.positiveInteger:
            case xsd.negativeInteger:
            case xsd.float:
            case xsd.double:
            case xsd.decimal:
                return (ursa.view.SupportedPropertyRenderer.dataTypes[supportedProperty.range.id].isInRange((typeof(value) === "number" ? value : value = parseInt(value.toString()))) ?
                    String.format("(<{0}> eq {1})", supportedProperty.property, value) : "");
            case xsd.dateTime:
            case xsd.time:
            case xsd.date:
                return DateTimeRegExp[supportedProperty.range.id].generateExpression(supportedProperty, value.toString());
            case xsd.gYear:
            case xsd.gMonth:
            case xsd.gDay:
            case xsd.gYearMonth:
                return (ursa.view.SupportedPropertyRenderer.dataTypes[supportedProperty.range.id].isInRange((typeof(value) === "number" ? value : value = parseInt(value.toString()))) ?
                    String.format("(<{0}> eq {1})", supportedProperty.property, value) : "");
            case guid.guid:
                return (ursa.view.SupportedPropertyRenderer.dataTypes[supportedProperty.range.id].isInRange(value.toString()) ?
                    String.format("(<{0}> eq guid('{1}'))", supportedProperty.property, value) : "");
        }
    };
    ODataFilterExpressionProvider.prototype.isApplicableTo = function(uri) { return uri === odata.filter; };
    ODataFilterExpressionProvider.prototype.toString = function() { return "ursa.model.ODataFilterExpressionProvider"; };
}(namespace("ursa.model")));