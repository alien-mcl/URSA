/*globals ursa */
(function () {
    "use strict";

    /**
     * @ngdoc overview
     * @name ursaclientApp
     * @description
     * # ursaclientApp
     *
     * Main module of the application.
     */
    window.application = angular
        .module("ursaclientApp", [
            "ngAnimate",
            "ngCookies",
            "ngMessages",
            "ngResource",
            "ngRoute",
            "ngSanitize",
            "ngTouch",
            "ui.bootstrap",
            "ursa",
            "jsonld"
        ]).
        config(["$routeProvider", function($routeProvider) {
            $routeProvider.
                when("/", { templateUrl: "views/main.html", controller: "MainCtrl", controllerAs: "main" }).
                when("/about", { templateUrl: "views/about.html" }).
                otherwise({ redirectTo: "/" });
        }]).
        filter("trustAsResourceUrl", ["$sce", function($sce) {
            return function(val) {
                return $sce.trustAsResourceUrl(val);
            };
        }]).
        run(["$rootScope", function ($rootScope) {
            $rootScope.authenticationEvent = ursa.view.Events.AuthenticationRequired;
            $rootScope.authenticateEvent = ursa.view.Events.Authenticate;
            $rootScope.authenticatedEvent = ursa.view.Events.Authenticated;
            $rootScope.unauthorizedEvent = ursa.view.Events.AuthenticationFailed;
        }]);
}());