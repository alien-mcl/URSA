/*globals application, hydra */
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
    controller("MainCtrl", ["$scope", "configuration", "hydraApiDocumentation", function($scope, configuration, apiDocumentationProvider) {
        $scope.apiDocumentation = null;
        $scope.selectedOperation = null;
        $scope.apiDocumentationId = hydra.ApiDocumentation;
        $scope.onOperationClick = function(supportedOperation) {
            $scope.selectedOperation = supportedOperation;
        };
        $scope.filterOperation = function(supportedOperation) {
            return (supportedOperation.methods.indexOf("GET") !== -1) && (supportedOperation.returns.length > 0) &&
                (supportedOperation.returns[0].maxOccurances === Number.MAX_VALUE);
        };
        apiDocumentationProvider.load(configuration.entryPoint).
            then(function(apiDocumentation) {
                $scope.apiDocumentation = apiDocumentation;
                var index;
                var apiDocumentationClass = null;
                for (index = 0; index < apiDocumentation.supportedClasses.length; index++) {
                    var supportedClass = apiDocumentation.supportedClasses[index];
                    if (supportedClass.id === hydra.ApiDocumentation) {
                        apiDocumentationClass = supportedClass;
                        break;
                    }
                }

                var apiDocumentationOperation = null;
                if (apiDocumentationClass !== null) {
                    for (index = 0; index < apiDocumentationClass.supportedOperations.length; index++) {
                        var supportedOperation = apiDocumentationClass.supportedOperations[index];
                        if (supportedOperation.methods.indexOf("GET") !== -1) {
                            apiDocumentationOperation = supportedOperation;
                            break;
                        }
                    }
                }

                if (apiDocumentationOperation !== null) {
                    $scope.$root.documentationUrl = apiDocumentationOperation.createCallUrl();
                }

                return apiDocumentation;
            });
    }]);
}());