/*globals namespace */
(function(namespace) {
    "use strict";

    /**
     * Provides an AngularJS based view scope.
     * @memberof ursa.view.angular
     * @name AngularViewScope
     * @public
     * @class
     * @extends {ursa.view.IViewScope}
     */
    var AngularViewScope = namespace.AngularViewScope = {};

    /**
     * Extens a standard AngularJS scope with URSA abstraction.
     * @memberof ursa.view.angular
     * @public
     * @static
     * @method createInstance
     * @param {angular.IScope} $scope The scope.
     * @returns {ursa.view.IViewScope} The URSA view scope.
     */
    AngularViewScope.createInstance = function($scope) {
        if (($scope === null) || ($scope.rootScope) && ($scope.parentScope) && ($scope.onEvent) && ($scope.emitEvent) && ($scope.broadcastEvent)) {
            return $scope;
        }

        $scope.rootScope = ($scope.$root === $scope ? this : AngularViewScope.createInstance($scope.$root));
        $scope.parentScope = ($scope.$parent === $scope.$root ? $scope.rootScope : AngularViewScope.createInstance($scope.$parent));
        if (typeof($scope.updateView) !== "function") {
            $scope.updateView = function() { if (!$scope.$root.$$phase) { $scope.$root.$apply(); } };
        }

        if (typeof($scope.onEvent) !== "function") {
            $scope.onEvent = function() { return $scope.$on.apply($scope, arguments); };
        }

        if (typeof($scope.emitEvent) !== "function") {
            $scope.emitEvent = function() { return $scope.$emit.apply($scope, arguments); };
        }

        if (typeof($scope.broadcastEvent) !== "function") {
            $scope.broadcastEvent = function() { return $scope.$broadcast.apply($scope, arguments); };
        }

        return $scope;
    };
    
    AngularViewScope.toString = function() { return "ursa.web.AngularViewScope"; };
}(namespace("ursa.view.angular")));