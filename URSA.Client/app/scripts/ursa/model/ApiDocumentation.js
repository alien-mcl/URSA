/*globals namespace, ursa, hydra */
(function(namespace) {
    "use strict";

    var getById = function (id) {
        for (var index = 0; index < this.length; index++) {
            if (this[index]["@id"] === id) {
                return this[index];
            }
        }

        return null;
    };

    /**
     * Describes an API documentation.
     * @memberof ursa.model
     * @name ApiDocumentation
     * @public
     * @extends {ursa.model.ApiMember}
     * @class
     * @param {object} graph JSON-LD graph of resources.
     */
    var ApiDocumentation = (namespace.ApiDocumentation = function(graph) {
        Function.requiresArgument("graph", graph, Array);
        ursa.model.ApiMember.prototype.constructor.call(this, null, null);
        var index;
        this.owner = null;
        this.entryPoints = [];
        this.supportedClasses = new namespace.ApiMemberCollection(this);
        this.knownTypes = new namespace.ApiMemberCollection(this);
        graph.getById = function(id) { return getById.call(graph, id); };
        var apiDocumentation = null;
        for (index = 0; index < graph.length; index++) {
            var resource = graph[index];
            if (resource["@type"].indexOf(hydra.ApiDocumentation) !== -1) {
                apiDocumentation = resource;
                this.id = resource["@id"];
                this.title = namespace.getValue.call(resource, hydra.title) || "";
                this.description = namespace.getValue.call(resource, hydra.description) || "";
                this.entryPoints = namespace.getValues.call(resource, hydra.entrypoint);
                if (this.entryPoints.length === 0) {
                    this.entryPoints.push(this.id.match(/^http[s]*:\/\/[^\/]+\//)[0]);
                }

                break;
            }
        }

        if ((apiDocumentation !== null) && (apiDocumentation[hydra.supportedClass] instanceof Array)) {
            for (index = 0; index < apiDocumentation[hydra.supportedClass].length; index++) {
                var supportedClassDefinition = graph.getById(apiDocumentation[hydra.supportedClass][index]["@id"]);
                var ref = { lazyInitializer: null };
                var supportedClass = new ursa.model.Class(this, supportedClassDefinition, graph, ref);
                this.supportedClasses.push(supportedClass);
                this.knownTypes.push(supportedClass);
                ref.lazyInitializer(supportedClassDefinition, graph);
            }
        }
    })[":"](ursa.model.ApiMember);

    /**
     * List of supported classes.
     * @memberof ursa.model.ApiDocumentation
     * @instance
     * @public
     * @member {Array.<ursa.model.Class>} supportedClasses
     */
    ApiDocumentation.prototype.supportedClasses = null;

    /**
     * List of known types.
     * @memberof ursa.model.ApiDocumentation
     * @instance
     * @public
     * @member {Array.<ursa.model.Class>} knownTypes
     */
    ApiDocumentation.prototype.knownTypes = null;

    /**
     * List of entry point Urls.
     * @memberof ursa.model.ApiDocumentation
     * @instance
     * @public
     * @member {Array.<string>} entryPoints
     */
    ApiDocumentation.prototype.entryPoints = null;
}(namespace("ursa.model")));