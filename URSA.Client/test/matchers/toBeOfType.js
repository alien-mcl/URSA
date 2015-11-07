(function() {
    "use strict";
    if (window.matchers === undefined) {
        window.matchers = {};
    }

    window.matchers.toBeOfType = function() {
        return {
            compare: function(actual, expected) {
                if (typeof (expected) !== "function") {
                    expected = function() {};
                }

                var result = {};
                result.pass = actual instanceof expected;
                if (result.pass) {
                    result.message = "Expected " + actual + " to be instance of " + expected;
                }
                else {
                    result.message = "Expected " + actual + " to be instance of " + expected + ", but was not";
                }

                return result;
            }
        }
    };
}());