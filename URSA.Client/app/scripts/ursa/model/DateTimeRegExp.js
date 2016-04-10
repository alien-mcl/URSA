/*globals namespace, ursa, xsd */
(function (namespace) {
    "use strict";

    /**
     * Represents a date-time regular expression transforming a date/time string into an OData filter.
     * @memberof ursa.model
     * @name DateTimeRegExp
     * @public
     * @class
     * @param {RegExp} regex Actual regular expression.
     * @param {string} format Format string.
     * @param {number} year Position of the year in the regular expression.
     * @param {number} month Position of the month in the regular expression.
     * @param {number} day Position of the day in the regular expression.
     * @param {number} hour Position of the hour in the regular expression.
     * @param {number} minute Position of the minute in the regular expression.
     * @param {number} second Position of the second in the regular expression.
     */
    var DateTimeRegExp = namespace.DateTimeRegExp = function(regex, format, year, month, day, hour, minute, second) {
        this._regex = regex;
        this.format = format;
        this.year = year || -1;
        this.month = month || -1;
        this.day = day || -1;
        this.hour = hour || -1;
        this.minute = minute || -1;
        this.second = second || -1;
    };

    Object.defineProperty(DateTimeRegExp.prototype, "_regex", { enumerable: false, configurable: false, writable: true, value: null });

    /**
     * The filter format string.
     * @memberof ursa.model.DateTimeRegExp
     * @instance
     * @public
     * @member {string} format
     */
    DateTimeRegExp.prototype.format = null;

    /**
     * Position of the year in the regular expression.
     * @memberof ursa.model.DateTimeRegExp
     * @instance
     * @public
     * @member {number} year
     */
    DateTimeRegExp.prototype.year = -1;

    /**
     * Position of the month in the regular expression.
     * @memberof ursa.model.DateTimeRegExp
     * @instance
     * @public
     * @member {number} month
     */
    DateTimeRegExp.prototype.month = -1;

    /**
     * Position of the day in the regular expression.
     * @memberof ursa.model.DateTimeRegExp
     * @instance
     * @public
     * @member {number} day
     */
    DateTimeRegExp.prototype.day = -1;

    /**
     * Position of the hour in the regular expression.
     * @memberof ursa.model.DateTimeRegExp
     * @instance
     * @public
     * @member {number} hour
     */
    DateTimeRegExp.prototype.hour = -1;

    /**
     * Position of the minute in the regular expression.
     * @memberof ursa.model.DateTimeRegExp
     * @instance
     * @public
     * @member {number} minute
     */
    DateTimeRegExp.prototype.minute = -1;

    /**
     * Position of the second in the regular expression.
     * @memberof ursa.model.DateTimeRegExp
     * @instance
     * @public
     * @member {number} second
     */
    DateTimeRegExp.prototype.second = -1;

    /**
     * Tests whether the given value matches this regular expression.
     * @memberof ursa.model.DateTimeRegExp
     * @instance
     * @public
     * @method test
     * @param {string} value Value to test.
     * @returns {boolean} True if the value matches; otherwise false.
     */
    DateTimeRegExp.prototype.test = function(value) { return this._regex.test(value); };

    /**
     * Executes a regular expresion against a given value.
     * @memberof ursa.model.DateTimeRegExp
     * @instance
     * @public
     * @method exec
     * @param {string} value Value to use in regular expression matching.
     * @returns {RegExpExecArray} Regular expression matches.
     */
    DateTimeRegExp.prototype.exec = function (value) { return this._regex.exec(value); };

    /**
     * Matches full date and time.
     * @memberof ursa.model.DateTimeRegExp
     * @static
     * @public
     * @member {ursa.model.DateTimeRegExp} YearMonthDayHourMinuteSecondRegex
     */
    DateTimeRegExp.YearMonthDayHourMinuteSecondRegex = new DateTimeRegExp(
        /^([\-0-9]{4})[^0-9]+([0-1]?[0-9])[^0-9]+([0-3]?[0-9])[^0-9]+([0-2]?[0-9])[^0-9]+([0-5]?[0-9])[^0-9]+([0-5]?[0-9])$/,
        "({property} eq DateTime'{year}-{month}-{day}T{hour}:{minute}:{second})", 1, 2, 3, 4, 5, 6);

    /**
     * Matches full date and time without seconds.
     * @memberof ursa.model.DateTimeRegExp
     * @static
     * @public
     * @member {ursa.model.DateTimeRegExp} YearMonthDayHourMinuteRegex
     */
    DateTimeRegExp.YearMonthDayHourMinuteRegex = new DateTimeRegExp(
        /^([\-0-9]{4})[^0-9]+([0-1]?[0-9])[^0-9]+([0-3]?[0-9])[^0-9]+([0-2]?[0-9])[^0-9]+([0-5]?[0-9])$/,
        "({property} ge DateTime'{year}-{month}-{day}T{hour}:{minute}:00.000 and {property} le DateTime'{year}-{month}-{day}T{hour}:{minute}:59.000)", 1, 2, 3, 4, 5);

    /**
     * Matches full date and time without minutes and seconds.
     * @memberof ursa.model.DateTimeRegExp
     * @static
     * @public
     * @member {ursa.model.DateTimeRegExp} YearMonthDayHourRegex
     */
    DateTimeRegExp.YearMonthDayHourRegex = new DateTimeRegExp(
        /^([\-0-9]{4})[^0-9]+([0-1]?[0-9])[^0-9]+([0-3]?[0-9])[^0-9]+([0-2]?[0-9])$/,
        "({property} ge DateTime'{year}-{month}-{day}T{hour}:00:00.000 and {property} le DateTime'{year}-{month}-{day}T{hour}:59:59.000)", 1, 2, 3, 4);

    /**
     * Matches full date starting with day and time.
     * @memberof ursa.model.DateTimeRegExp
     * @static
     * @public
     * @member {ursa.model.DateTimeRegExp} DayMonthYearHourMinuteSecondRegex
     */
    DateTimeRegExp.DayMonthYearHourMinuteSecondRegex = new DateTimeRegExp(
        /^([0-3]?[0-9])[^0-9]+([0-1]?[0-9])[^0-9]+([0-9]{4})[^0-9]+([0-2]?[0-9])[^0-9]+([0-5]?[0-9])[^0-9]+([0-5]?[0-9])$/,
        "({property} eq DateTime'{year}-{month}-{day}T{hour}:{minute}:{second})", 3, 2, 1, 4, 5, 6);

    /**
     * Matches full date starting with day and time without seconds.
     * @memberof ursa.model.DateTimeRegExp
     * @static
     * @public
     * @member {ursa.model.DateTimeRegExp} DayMonthYearHourMinuteRegex
     */
    DateTimeRegExp.DayMonthYearHourMinuteRegex = new DateTimeRegExp(
        /^([0-3]?[0-9])[^0-9]+([0-1]?[0-9])[^0-9]+([0-9]{4})[^0-9]+([0-2]?[0-9])[^0-9]+([0-5]?[0-9])$/,
        "({property} ge DateTime'{year}-{month}-{day}T{hour}:{minute}:00.000 and {property} le DateTime'{year}-{month}-{day}T{hour}:{minute}:59.000)", 3, 2, 1, 4, 5);

    /**
     * Matches full date starting with day and time without minutes and seconds.
     * @memberof ursa.model.DateTimeRegExp
     * @static
     * @public
     * @member {ursa.model.DateTimeRegExp} DayMonthYearHourRegex
     */
    DateTimeRegExp.DayMonthYearHourRegex = new DateTimeRegExp(
        /^([0-3]?[0-9])[^0-9]+([0-1]?[0-9])[^0-9]+([0-9]{4})[^0-9]+([0-2]?[0-9])$/,
        "({property} ge DateTime'{year}-{month}-{day}T{hour}:00:00.000 and {property} le DateTime'{year}-{month}-{day}T{hour}:59:59.000)", 3, 2, 1, 4);

    /**
     * Matches full date only.
     * @memberof ursa.model.DateTimeRegExp
     * @static
     * @public
     * @member {ursa.model.DateTimeRegExp} YearMonthDayRegex
     */
    DateTimeRegExp.YearMonthDayRegex = new DateTimeRegExp(/^([\-0-9]{4})[^0-9]+([0-1]?[0-9])[^0-9]+([0-3]?[0-9])$/,
        "({property} ge DateTime'{year}-{month}-{day}T00:00:00 and {value} le DateTime'{year}-{month}-{day}T:00:00:00')", 1, 2, 3);

    /**
     * Matches full date only starting with day.
     * @memberof ursa.model.DateTimeRegExp
     * @static
     * @public
     * @member {ursa.model.DateTimeRegExp} DayMonthYearRegex
     */
    DateTimeRegExp.DayMonthYearRegex = new DateTimeRegExp(/^([0-3]?[0-9])[^0-9]+([0-1]?[0-9])[^0-9]+([0-9]{4})$/,
        "({property} ge DateTime'{year}-{month}-{day}T00:00:00 and {property} lt DateTime'{year}-{month}-{day}T:00:00:00')", 3, 2, 1);

    /**
     * Matches year and month only.
     * @memberof ursa.model.DateTimeRegExp
     * @static
     * @public
     * @member {ursa.model.DateTimeRegExp} YearMonthRegex
     */
    DateTimeRegExp.YearMonthRegex = new DateTimeRegExp(/^([\-0-9]{4})[^0-9]+([0-1]?[0-9])$/, "(year({property}) eq {year} and month({property}) eq {month})", 1, 2);

    /**
     * Matches month and year only.
     * @memberof ursa.model.DateTimeRegExp
     * @static
     * @public
     * @member {ursa.model.DateTimeRegExp} MonthYearRegex
     */
    DateTimeRegExp.MonthYearRegex = new DateTimeRegExp(/^([0-1]?[0-9])[^0-9]+([0-9]{4})$/, "(year({property}) eq {year} and month({property}) eq {month})", 2, 1);

    /**
     * Matches a year only.
     * @memberof ursa.model.DateTimeRegExp
     * @static
     * @public
     * @member {ursa.model.DateTimeRegExp} YearRegex
     */
    DateTimeRegExp.YearRegex = new DateTimeRegExp(/^([\-0-9]{4})$/, "(year({property}) eq {year})", 1);

    /**
     * Matches a month only.
     * @memberof ursa.model.DateTimeRegExp
     * @static
     * @public
     * @member {ursa.model.DateTimeRegExp} MonthRegex
     */
    DateTimeRegExp.MonthRegex = new DateTimeRegExp(/^([0-1]?[0-9])$/, "(month({property}) eq {month})", -1, 1);

    /**
     * Matches a day only.
     * @memberof ursa.model.DateTimeRegExp
     * @static
     * @public
     * @member {ursa.model.DateTimeRegExp} DayRegex
     */
    DateTimeRegExp.DayRegex = new DateTimeRegExp(/^([0-3]?[0-9])$/, "(day({property}) eq {day})", -1, -1, 1);

    /**
     * Matches full time only.
     * @memberof ursa.model.DateTimeRegExp
     * @static
     * @public
     * @member {ursa.model.DateTimeRegExp} HourMinuteSecondRegex
     */
    DateTimeRegExp.HourMinuteSecondRegex = new DateTimeRegExp(/^([0-2]?[0-9])[^0-9]+([0-5]?[0-9])[^0-9]+([0-5]?[0-9])$/,
        "(hour({property} eq {hour} and minute({property} eq {minute} and second({property}) eq {second})", -1, -1, -1, 1, 2, 3);

    /**
     * Matches hours and minutes only.
     * @memberof ursa.model.DateTimeRegExp
     * @static
     * @public
     * @member {ursa.model.DateTimeRegExp} HourMinuteRegex
     */
    DateTimeRegExp.HourMinuteRegex = new DateTimeRegExp(/^([0-2]?[0-9])[^0-9]+([0-5]?[0-9])$/, "(hour({property} eq {hour} and minute({property} eq {minute})", -1, -1, -1, 1, 2);

    /**
     * Matches hours only.
     * @memberof ursa.model.DateTimeRegExp
     * @static
     * @public
     * @member {ursa.model.DateTimeRegExp} HourRegex
     */
    DateTimeRegExp.HourRegex = new DateTimeRegExp(/^([0-2]?[0-9])$/, "(hour({property} eq {hour})", -1, -1, -1, 1);

    /**
     * Provides a cummulative regular expression for xsd:dateTime.
     * @memberof ursa.model.DateTimeRegExp
     * @static
     * @public
     * @member {RegExpArray.<ursa.model.DateTimeRegExp>} xsd.dateTime
     */
    DateTimeRegExp[xsd.dateTime] = new ursa.model.RegExpArray(
        DateTimeRegExp.YearMonthDayHourMinuteSecondRegex,
        DateTimeRegExp.YearMonthDayHourMinuteRegex,
        DateTimeRegExp.YearMonthDayHourRegex,
        DateTimeRegExp.DayMonthYearHourMinuteSecondRegex,
        DateTimeRegExp.DayMonthYearHourMinuteRegex,
        DateTimeRegExp.DayMonthYearHourRegex,
        DateTimeRegExp.YearMonthDayRegex,
        DateTimeRegExp.DayMonthYearRegex,
        DateTimeRegExp.YearMonthRegex,
        DateTimeRegExp.MonthYearRegex,
        DateTimeRegExp.YearRegex,
        DateTimeRegExp.MonthRegex,
        DateTimeRegExp.DayRegex);

    /**
     * Provides a cummulative regular expression for xsd:date.
     * @memberof ursa.model.DateTimeRegExp
     * @static
     * @public
     * @member {RegExpArray.<ursa.model.DateTimeRegExp>} xsd.date
     */
    DateTimeRegExp[xsd.date] = new ursa.model.RegExpArray(
        DateTimeRegExp.YearMonthDayRegex,
        DateTimeRegExp.DayMonthYearRegex,
        DateTimeRegExp.YearMonthRegex,
        DateTimeRegExp.MonthYearRegex,
        DateTimeRegExp.YearRegex,
        DateTimeRegExp.MonthRegex,
        DateTimeRegExp.DayRegex);

    /**
     * Provides a cummulative regular expression for xsd:time.
     * @memberof ursa.model.DateTimeRegExp
     * @static
     * @public
     * @member {RegExpArray.<ursa.model.DateTimeRegExp>} xsd.time
     */
    DateTimeRegExp[xsd.time] = new ursa.model.RegExpArray(DateTimeRegExp.HourMinuteSecondRegex, DateTimeRegExp.HourMinuteRegex, DateTimeRegExp.HourRegex);

}(namespace("ursa.model")));