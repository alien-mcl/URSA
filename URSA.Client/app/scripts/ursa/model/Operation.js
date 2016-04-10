/*globals namespace, ursa, hydra, UriTemplate */
(function (namespace) {
    "use strict";

    var _Operation = {};

    /**
     * Describes an ReST operation.
     * @memberof ursa.model
     * @name Operation
     * @public
     * @extends {ursa.model.ApiMember}
     * @class
     * @param {ursa.model.ApiMember} owner Owner if this instance being created.
     * @param {object} [supportedOperation] JSON-LD resource describing this API member.
     * @param {object} [template] JSON-LD resource describing this API member.
     * @param {ursa.IGraph} [graph] JSON-LD graph of resources.
     */
    var Operation = (namespace.Operation = function(owner, supportedOperation, template, graph) {
        Function.requiresArgument("graph", graph, ursa.IGraph);
        ursa.model.ApiMember.prototype.constructor.apply(this, arguments);
        if ((template) && (template[hydra.template])) {
            if ((this.url = template[hydra.template][0]["@value"]).match(/^[a-zA-Z][a-zA-Z0-9\+\-\.]*/) === null) {
                var apiDocumentation = this.apiDocumentation;
                var entryPoint = ((apiDocumentation.entryPoints) && (apiDocumentation.entryPoints.length > 0) ? apiDocumentation.entryPoints[0] : window.location.href);
                this.url = (entryPoint.charAt(entryPoint.length - 1) === "/" ? entryPoint.substr(0, entryPoint.length - 1) : entryPoint) + this.url;
            }

            this.mappings = new ursa.model.ApiMemberCollection(this);
            for (var index = 0; index < template[hydra.mapping].length; index++) {
                this.mappings.push(new ursa.model.Mapping(this, graph.getById(template[hydra.mapping][index]["@id"]), graph));
            }
        }
        else {
            this.url = this.id;
            this.mappings = null;
        }

        this.returns = [];
        this.expects = [];
        this.methods = namespace.getValues.call(supportedOperation, hydra.method);
        this.statusCodes = namespace.getValues.call(supportedOperation, hydra.statusCode);
        if ((this.mediaTypes = namespace.getValues.call(supportedOperation, ursa.mediaType)).length === 0) {
            this.mediaTypes.push(ursa.model.EntityFormat.ApplicationLdJson);
            this.mediaTypes.push(ursa.model.EntityFormat.ApplicationJson);
        }
        else {
            this.mediaTypes.sort(function(leftOperand, rightOperand) {
                return (leftOperand.indexOf("json") !== -1 ? -1 : (rightOperand.indexOf("json") !== -1 ? 1 : 0));
            });
        }

        _Operation.setupTypeCollection.call(this, supportedOperation[hydra.returns], this.returns, graph);
        _Operation.setupTypeCollection.call(this, supportedOperation[hydra.expects], this.expects, graph);
    })[":"](ursa.model.ApiMember);

    /**
     * List of allowed HTTP verbs for this operation.
     * @memberof ursa.model.Mapping
     * @instance
     * @public
     * @member {Array.<string>} methods
     */
    Operation.prototype.methods = null;

    /**
     * List of expected types.
     * @memberof ursa.model.Mapping
     * @instance
     * @public
     * @member {Array.<ursa.model.Type>} expects
     */
    Operation.prototype.expects = null;

    /**
     * List of returned types.
     * @memberof ursa.model.Mapping
     * @instance
     * @public
     * @member {Array.<ursa.model.Type>} returns
     */
    Operation.prototype.returns = null;

    /**
     * List of returned HTTP status codes.
     * @memberof ursa.model.Mapping
     * @instance
     * @public
     * @member {Array.<number>} statusCodes
     */
    Operation.prototype.statusCodes = null;

    /**
     * List of returned acceptable media types.
     * @memberof ursa.model.Mapping
     * @instance
     * @public
     * @member {Array.<string>} mediaTypes
     */
    Operation.prototype.mediaTypes = null;

    /**
     * Url of this operation.
     * @memberof ursa.model.Mapping
     * @instance
     * @public
     * @member {string} url
     */
    Operation.prototype.url = null;

    /**
     * Collection of IRI template mappings if any. If there is no template associated this member is null.
     * @memberof ursa.model.Mapping
     * @instance
     * @public
     * @member {string} mappings
     */
    Operation.prototype.mappings = null;

    /**
     * Creates an operation call URL. This method parses the url for any template mappings and fills it with values.
     * @memberof ursa.model.Operation
     * @instance
     * @public
     * @method createCallUrl
     * @param {object} instance Object with values to be used when expanding the template.
     * @returns {string} Operation's call URL.
     */
    Operation.prototype.createCallUrl = function(instance) {
        var result = this.url;
        if (this.mappings !== null) {
            var input = {};
            for (var index = 0; index < this.mappings.length; index++) {
                var mapping = this.mappings[index];
                var propertyValue = null;
                if ((instance !== undefined) && (instance !== null)) {
                    if (mapping.property !== null) {
                        propertyValue = instance[mapping.propertyName(this)];
                    }

                    if ((propertyValue !== undefined) && (propertyValue !== null) && (propertyValue instanceof Array)) {
                        propertyValue = propertyValue[0];
                    }

                    if ((this.isRdf) && (propertyValue !== undefined) && (propertyValue !== null)) {
                        propertyValue = propertyValue["@value"] || propertyValue["@id"];
                    }
                }

                if ((propertyValue !== undefined) && (propertyValue !== null)) {
                    input[mapping.variable] = propertyValue;
                }
            }

            result = new UriTemplate(result).fill(input);
        }

        var indexOf;
        if ((indexOf = result.indexOf("#")) !== -1) {
            result = result.substr(0, indexOf);
        }

        return result;
    };

    /**
     * Gets a flag indicating whether the operation accepts JSON-LD or not.
     * @memberof ursa.model.Operation
     * @instance
     * @public
     * @readonly
     * @member {boolean} isRdf
     */
    Object.defineProperty(Operation.prototype, "isRdf", { get: function() { return this.mediaTypes.indexOf(ursa.model.EntityFormat.ApplicationLdJson) !== -1; } });

    Operation.toString = function () { return "ursa.model.Operation"; };

    _Operation.setupTypeCollection = function(source, target, graph) {
        if (!source) {
            return;
        }

        for (var index = 0; index < source.length; index++) {
            target.push(namespace.getClass.call(graph.getById(source[index]["@id"]), this, graph));
        }
    };
}(namespace("ursa.model")));