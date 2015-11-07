(function() {
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
        ])
        .config(function($routeProvider) {
            $routeProvider
                .when("/", {
                    templateUrl: "views/main.html",
                    controller: "MainCtrl",
                    controllerAs: "main"
                })
                .when("/about", {
                    templateUrl: "views/about.html",
                    controller: "AboutCtrl",
                    controllerAs: "about"
                })
                .otherwise({
                    redirectTo: "/"
                });
        }).
        filter("trustAsResourceUrl", ["$sce", function($sce) {
            return function(val) {
                return $sce.trustAsResourceUrl(val);
            };
        }]);
}());