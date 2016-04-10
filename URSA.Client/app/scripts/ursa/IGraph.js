/*globals namespace, joice */
(function (namespace) {
    "use strict";

    /**
     * RDF graph abstract.
     * @memberof ursa
     * @name IGraph
     * @public
     * @abstract
     * @class
     */
    var IGraph = namespace.IGraph = function() {
        throw new joice.InvalidOperationException("Cannot instantiate interface ursa.IGraph.");
    };

    /**
     * Gets a resource from a graph by it's IRI.
     * @memberof ursa.IGraph
     * @public
     * @abstract
     * @method getById
     * @param {string} id Resource identifier to be found.
     * @returns {object} Resource of a given IRI.
     */
    IGraph.getById = function () { };

    IGraph.toString = function() { return "ursa.IGraph"; };
}(namespace("ursa")));