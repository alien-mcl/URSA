/*globals application */
(function() {
    "use strict";

    /**
     * @ngdoc function
     * @name ursaclientApp.controller:AuthCtrl
     * @description
     * # basicAuthentication
     * Directicve of the ursaclientApp
     */
    application.
    directive("basicAuthentication", function() {
        return {
            restrict: "E",
            scope: {
                showOnEvent: "@",
                hideOnEvent: "@",
                notifyWithEvent: "@",
                errorOnEvent: "@"
            },
            templateUrl: "/views/auth.html",
            link: function($scope) {
                $scope.visible = false;
                $scope.submitted = false;
                $scope.message = "";
                $scope.userName = "";
                $scope.password = "";
                $scope.onLoginClick = function() {
                    if (($scope.userName) && ($scope.password)) {
                        $scope.$root.$broadcast($scope.notifyWithEvent, $scope.userName, $scope.password);
                    }
                };
                $scope.$on($scope.showOnEvent, function() {
                    $scope.visible = true;
                });
                $scope.$on($scope.hideOnEvent, function() {
                    $scope.visible = false;
                });
                $scope.$on($scope.errorOnEvent, function(e, error) {
                    $scope.message = error;
                });
            }
        };
    });
}());