/*globals application */
(function() {
    "use strict";
    application.filter("stringify", function() {
        return function (value) {
            if (value instanceof Array) {
                if (value.length > 0) {
                    return (value[0]["@value"] !== undefined ? value[0]["@value"] : value[0]);
                }

                return "";
            }

            return value;
        };
    });
}());