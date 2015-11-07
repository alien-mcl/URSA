/*globals application */
(function() {
    "use strict";
    application.filter("asId", function() {
        return function(value) {
            if (value.match(/^[a-zA-Z][a-zA-Z0-9\-\+\.]*:/)) {
                var position = value.indexOf("#");
                if ((position !== -1) && (position !== value.length - 1)) {
                    return value.substr(position + 1);
                }

                position = value.indexOf("?");
                if (position !== -1) {
                    value = value.substr(0, position);
                }

                position = value.lastIndexOf("/");
                if ((position !== -1) && (position !== value.length - 1)) {
                    return value.substr(position + 1);
                }

                position = value.lastIndexOf(":");
                if ((position !== -1) && (position !== value.length - 1)) {
                    return value.substr(position + 1);
                }
            }
            else {
                return value;
            }

            return value;
        };
    });
}());