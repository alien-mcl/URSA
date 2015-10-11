/*globals application */
(function() {
"use strict";

/**
 * @ngdoc function
 * @name ursaclientApp.controller:MainCtrl
 * @description
 * # MainCtrl
 * Controller of the ursaclientApp
 */
application.
controller("MainCtrl", ["configuration", "hydraApiDocumentation", function(configuration, apiDocumentationProvider) {
    apiDocumentationProvider.load(configuration.entryPoint).
        then(function(apiDocumentation) {
            console.log(apiDocumentation);
        });
}]);
}());