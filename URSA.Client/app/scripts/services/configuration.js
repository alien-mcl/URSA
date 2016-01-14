/*globals application */
(function() {
    "use strict";
    var config = {
        entryPoint: "http://localhost:51509/api",
        challenge: "basic"
    };

    application.
    factory("configuration", function() {
        return config;
    });
}());