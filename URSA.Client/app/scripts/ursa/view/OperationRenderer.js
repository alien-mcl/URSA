/*globals namespace, ursa, hydra, confirm */
(function (namespace) {
    "use strict";

    var bodylessVerbs = ["TRACE"];
    var _OperationRenderer = {};

    /**
     * Default renderer for {@link ursa.model.Operation}.
     * @memberof ursa.view
     * @name OperationRenderer
     * @protected
     * @class
     * @extends ursa.view.ViewRenderer
     */
    var OperationRenderer = (namespace.OperationRenderer = function(httpService, jsonLdProcessor, authenticationProvider, filterProvider) {
        ursa.view.ViewRenderer.prototype.constructor.call(this, httpService, jsonLdProcessor, authenticationProvider, filterProvider);
    })[":"](ursa.view.ViewRenderer);

    OperationRenderer.prototype.isApplicableTo = function(apiMember) {
        ursa.view.ViewRenderer.prototype.isApplicableTo.apply(this, arguments);
        return apiMember instanceof ursa.model.Operation;
    };

    OperationRenderer.prototype.initialize = function(apiMember) {
        Function.requiresArgument("apiMember", apiMember, ursa.model.Operation);
        ursa.view.ViewRenderer.prototype.initialize.apply(this, arguments);
    };

    OperationRenderer.prototype.render = function(scope, classNames) {
        ursa.view.ViewRenderer.prototype.render.apply(this, arguments);
        if ((this.apiMember.returns.length === 1) && (this.apiMember.returns[0].maxOccurances === Number.MAX_VALUE)) {
            return _OperationRenderer.renderEntityList.call(this, scope, classNames);
        }

        return "";
    };

    OperationRenderer.toString = function () { return "ursa.view.OperationRenderer"; };

    _OperationRenderer.renderEntityList = function(scope, classNames) {
        if (!scope.supportedProperties) {
            _OperationRenderer.setupListScope.call(this, scope);
        }

        var pager = "";
        var pages = "Action";
        if (scope.itemsPerPageList) {
            pager = _OperationRenderer.generatePager.call(this, scope);
            pages = "<select class=\"form-control\" ng-model=\"filters.itemsPerPage\" ng-change=\"list(1)\" ng-options=\"take for take in itemsPerPageList track by take\"></select>";
        }

        var result = String.format(
                "<table class=\"table table-condensed table-bordered table-hover{0}\">", (classNames !== undefined) && (classNames !== null) ? " " + classNames : "") + String.format(
                "<tr>" +
                "<th ng-repeat=\"supportedProperty in supportedProperties track by supportedProperty.id\" ng-hide=\"supportedProperty.key\">" +
                "<span class=\"form-control\" ng-show=\"supportedProperties.getById(supportedProperty.id) === null\">{{ supportedProperty.label }}</span>" +
                "<input class=\"form-control\" ng-hide=\"supportedProperties.getById(supportedProperty.id) == null\" " +
                "type=\"text\" ng-model=\"filters[supportedProperty.property]\" placeholder=\"{{ supportedProperty.label }}\" " +
                "ng-change=\"list()\"/>" +
                "</th>" +
                "<th>{0}</th>" +
                "</tr>", pages) + String.format(
                "<tr ng-repeat-start=\"entity in entities{1}\" ng-hide=\"entityEquals(entity)\"" + (scope.keyProperty !== "" ? " title=\"{{ entity['{0}'] | asId }}\"" : "") + ">" +
                "<td ng-repeat=\"supportedProperty in supportedProperties track by supportedProperty.id\" ng-hide=\"supportedProperty.key\">{{ getPropertyValue(entity, supportedProperty, operation) | stringify }}</td>" +
                "<td><div class=\"btn-block\">" +
                (scope.getOperation !== null ? "<button class=\"btn btn-default\" title=\"Edit\" ng-click=\"get(entity)\"><span class=\"glyphicon glyphicon-pencil\"></span></button>" : "") +
                (scope.deleteOperation !== null ? "<button class=\"btn btn-default\" title=\"Delete\" ng-click=\"delete(entity, $event)\"><span class=\"glyphicon glyphicon-remove\"></span></button>" : "") +
                "</div></td>" +
                "</tr>", scope.keyProperty, (scope.keyProperty !== "" ? " track by entity['" + scope.keyProperty + "']" : "")) + String.format(
                "<tr ng-repeat-end ng-show=\"entityEquals(entity)\" ng-init=\"initialize($index)\">" +
                "<td colspan=\"{0}\"><div><ursa-api-member-view api-member=\"operation.owner\" target-instance=\"editedEntity\" unique-id=\"{{ uniqueId[$index] }}\"></ursa-api-member-view></div></td>" +
                "<td><div class=\"btn-block\">" +
                (scope.updateOperation !== null ? "<button class=\"btn btn-default\" title=\"Update\" ng-disabled=\"isFormDisabled(uniqueId[$index])\" ng-click=\"update(editedEntity)\"><span class=\"glyphicon glyphicon-floppy-disk\"></span></button>" : "") +
                "<button class=\"btn btn-default\" title=\"Cancel\" ng-click=\"cancel()\"><span class=\"glyphicon glyphicon-repeat\"></span></button>" +
                "</div></td>" +
                "</tr>", scope.supportedProperties.length - 1) + String.format(
                "<tr ng-hide=\"editedEntity !== null\" ng-init=\"initialize(-1)\">" +
                "<td colspan=\"{0}\"><div ng-show=\"footerVisible\"><ursa-api-member-view api-member=\"operation.owner\" target-instance=\"newInstance\" unique-id=\"{{ uniqueId.footer }}\"></ursa-api-member-view></div></td>" +
                "<td><div class=\"btn-block\">" +
                (scope.createOperation !== null ? "<button class=\"btn btn-default\" title=\"Create\" ng-disabled=\"isFormDisabled(uniqueId.footer)\" ng-click=\"create(newInstance)\"><span class=\"glyphicon glyphicon-plus\"></span></button>" : "") +
                "</div></td>" +
                "</tr>", scope.supportedProperties.length - 1) +
            pager +
            "</table>";
        scope.list();
        return result;
    };

    _OperationRenderer.generatePager = function(scope) {
        return String.format(
            "<tr>" +
            "<td colspan=\"{0}\"><nav><ul class=\"pagination\">" +
            "<li ng-hide=\"filters.currentPage <= 1\"><a href ng-click=\"list(filters.currentPage - 1)\">&laquo;</a></li>" +
            "<li ng-repeat=\"page in pages\"><a href ng-click=\"list(page)\">{{ page }}</a></li>" +
            "<li ng-hide=\"filters.currentPage >= pages.length\"><a href ng-click=\"list(filters.currentPage + 1)\">&raquo;</a></li>" +
            "</ul></nav>" +
            "</td>" +
            "</tr>", scope.supportedProperties.length);
    };

    _OperationRenderer.setupListScope = function(scope) {
        var that = this;
        scope.uniqueId = [];
        scope.operation = this.apiMember;
        scope.deleteOperation = namespace.findEntityCrudOperation.call(this, "DELETE");
        scope.updateOperation = namespace.findEntityCrudOperation.call(this, "PUT");
        scope.getOperation = namespace.findEntityCrudOperation.call(this, "GET");
        if ((scope.createOperation = namespace.findEntityCrudOperation.call(this, "POST")) !== null) {
            scope.newInstance = this.apiMember.owner.createInstance(this.apiMember);
            scope.footerVisible = false;
        }

        scope.keyProperty = (this.apiMember.isRdf ? "@id" : "");
        scope.supportedProperties = new ursa.model.ApiMemberCollection();
        for (var index = 0; index < this.apiMember.owner.supportedProperties.length; index++) {
            var supportedProperty = this.apiMember.owner.supportedProperties[index];
            if (supportedProperty.key) {
                scope.keyProperty = supportedProperty.propertyName(this.apiMember) + (this.apiMember.isRdf ? "'][0]['@value" : "");
            }

            if ((supportedProperty.maxOccurances === 1) && (supportedProperty.readable) && (supportedProperty.range instanceof ursa.model.DataType)) {
                scope.supportedProperties.push(supportedProperty);
            }
        }

        scope.filters = new ursa.model.Filter(scope.supportedProperties);
        if ((scope.operation.mappings !== null) && (this.filterProvider.providesCapabilities(this.apiMember.mappings, ursa.model.FilterExpressionProvider.SupportsPaging))) {
            scope.itemsPerPageList = [scope.filters.itemsPerPage = 10, 20, 50, 100];
            scope.pages = [];
            scope.totalEntities = 0;
        }

        scope.entities = null;
        scope.editedEntity = null;
        scope.getPropertyValue = function (entity, supportedProperty, operation) { return entity[supportedProperty.propertyName(operation)]; };
        scope.cancel = function () { scope.editedEntity = null; };
        scope.list = function (page) { _OperationRenderer.loadEntityList.call(that, scope, scope.operation, null, page); };
        scope.get = function (instance) { _OperationRenderer.loadEntity.call(that, scope, scope.getOperation, instance); };
        scope.create = function (instance) { _OperationRenderer.createEntity.call(that, scope, scope.createOperation, instance); };
        scope.update = function (instance) { _OperationRenderer.updateEntity.call(that, scope, scope.updateOperation, instance); };
        scope.delete = function (instance, e) { _OperationRenderer.deleteEntity.call(that, scope, scope.deleteOperation, instance, e); };
        scope.entityEquals = function (entity) { return _OperationRenderer.entityEquals.call(that, entity, scope.editedEntity, scope.operation); };
        scope.initialize = function (index) { _OperationRenderer.initialize.call(that, scope, index); };
        scope.isFormDisabled = function (name) { return _OperationRenderer.isFormDisabled.call(that, scope, name); };
    };

    _OperationRenderer.onLoadEntitySuccess = function(scope, createOperation, instance, request, response) {
        var that = this;
        if ((response.headers["Content-Type"] || "*/*").indexOf(ursa.model.EntityFormat.ApplicationLdJson) === 0) {
            that.jsonLdProcessor.expand(response.data).
                then(function (expanded) {
                    scope.rootScope.broadcastEvent(ursa.view.Events.EntityLoaded, scope.editedEntity = (expanded.length > 0 ? expanded[0] : null), createOperation.owner);
                    scope.updateView();
                    return expanded;
                });
        }
        else {
            scope.rootScope.broadcastEvent(ursa.view.Events.EntityLoaded, scope.editedEntity = response.data, createOperation.owner);
        }
    };

    _OperationRenderer.loadEntity = function(scope, getOperation, instance) {
        _OperationRenderer.handleOperation.call(this, scope, getOperation, null, instance, _OperationRenderer.onLoadEntitySuccess, null, _OperationRenderer.loadEntity);
    };

    _OperationRenderer.createEntitySuccess = function(scope, createOperation, instance) {
        scope.newInstance = createOperation.owner.createInstance(createOperation);
        scope.rootScope.broadcastEvent(ursa.view.Events.EntityCreated, instance, createOperation.owner);
        scope.list(1);
        scope.footerVisible = false;
    };

    _OperationRenderer.ammendEntityFailure = function(scope, createOperation, instance, request) {
        if (request.initialInstanceId !== undefined) {
            if (request.initialInstanceId) {
                delete instance["@id"];
            }
            else {
                instance["@id"] = request.initialInstanceId;
            }
        }
    };

    _OperationRenderer.createEntity = function(scope, createOperation, instance) {
        if (!scope.footerVisible) {
            scope.footerVisible = true;
            return;
        }

        _OperationRenderer.handleOperation.call(this, scope, createOperation, null, instance, _OperationRenderer.createEntitySuccess, _OperationRenderer.ammendEntityFailure, _OperationRenderer.createEntity);
    };

    _OperationRenderer.onUpdateEntitySuccess = function(scope, updateOperation, instance) {
        scope.editedEntity = null;
        scope.rootScope.broadcastEvent(ursa.view.Events.EntityModified, instance, updateOperation.owner);
        scope.list();
    };

    _OperationRenderer.updateEntity = function(scope, updateOperation, instance) {
        _OperationRenderer.handleOperation.call(this, scope, updateOperation, null, instance, _OperationRenderer.onUpdateEntitySuccess, _OperationRenderer.ammendEntityFailure, _OperationRenderer.updateEntity);
    };

    _OperationRenderer.onDeleteEntitySuccess = function(scope, deleteOperation, instance) {
        scope.rootScope.broadcastEvent(ursa.view.Events.EntityRemoved, instance, deleteOperation);
        scope.list(1);
    };

    _OperationRenderer.deleteEntity = function(scope, deleteOperation, instance, e) {
        if ((e) && (!confirm("Are you sure you want to delete this item?"))) {
            e.preventDefault();
            e.stopPropagation();
            return;
        }

        _OperationRenderer.handleOperation.call(this, scope, deleteOperation, null, instance, _OperationRenderer.onDeleteEntitySuccess, null, _OperationRenderer.deleteEntity);
    };

    _OperationRenderer.setupPaging = function(scope) {
        scope.pages = [];
        for (var pageIndex = 1; pageIndex <= Math.ceil(scope.totalEntities / scope.filters.itemsPerPage) ; pageIndex++) {
            scope.pages.push(pageIndex);
        }
    };

    _OperationRenderer.parseContentRange = function(scope, response) {
        var contentRange = response.headers["Content-Range"];
        if ((contentRange !== undefined) && (contentRange !== null)) {
            var matches = contentRange.match(/^members ([0-9]+)-([0-9]+)\/([0-9]+)/);
            if ((matches !== null) && (matches.length === 4)) {
                var startIndex = parseInt(matches[1]);
                scope.totalEntities = parseInt(matches[3]);
                scope.filters.currentPage = Math.ceil(startIndex / scope.filters.itemsPerPage) + 1;
                _OperationRenderer.setupPaging.call(this, scope);
            }
        }
    };

    _OperationRenderer.gatherMembers = function(scope, graph, members) {
        var result = [];
        for (var memberIndex = 0; memberIndex < members.length; memberIndex++) {
            for (var graphIndex = 0; graphIndex < graph.length; graphIndex++) {
                var graphNode = graph[graphIndex];
                if (members[memberIndex]["@id"] === graphNode["@id"]) {
                    result.push(graphNode);
                    break;
                }
            }
        }

        _OperationRenderer.setupPaging.call(this, scope);
        return result;
    };

    _OperationRenderer.parseHypermediaControls = function(scope, response, graph) {
        var result = graph;
        for (var index = graph.length - 1; index >= 0; index--) {
            var resource = graph[index];
            if (!resource["@type"]) {
                continue;
            }

            if (resource["@type"].indexOf(hydra.Collection) !== -1) {
                graph.splice(index, 1);
                if (resource[hydra.totalItems]) {
                    if ((typeof (scope.totalEntities = resource[hydra.totalItems][0]["@value"])) === "string") {
                        scope.totalEntities = parseInt(scope.totalEntities);
                    }
                }

                if (resource[hydra.member]) {
                    return _OperationRenderer.gatherMembers.call(this, scope, graph, resource[hydra.member]);
                }
            }
            else if (resource["@type"].indexOf(hydra.PartialCollectionView) !== -1) {
                graph.splice(index, 1);
            }
        }

        _OperationRenderer.setupPaging.call(this, scope);
        return result;
    };

    _OperationRenderer.onLoadEntityListSuccess = function(scope, listOperation, instance, request, response) {
        var that = this;
        _OperationRenderer.parseContentRange.call(this, scope, response);
        if ((response.headers["Content-Type"] || "*/*").indexOf(ursa.model.EntityFormat.ApplicationLdJson) === 0) {
            that.jsonLdProcessor.expand(response.data).
                then(function (expanded) {
                    scope.entities = _OperationRenderer.parseHypermediaControls.call(this, scope, response, expanded);
                    scope.updateView();
                    return expanded;
                });
        }
        else {
            scope.entities = response.data;
        }
    };

    _OperationRenderer.loadEntityList = function(scope, listOperation, instance, page) {
        var candidateMethod = _OperationRenderer.findEntityListMethod.call(this, listOperation);
        if (candidateMethod === null) {
            return;
        }

        if (typeof (page) === "number") {
            scope.filters.currentPage = page;
        }

        if (listOperation.mappings !== null) {
            for (var index = 0; index < listOperation.mappings.length; index++) {
                var mapping = listOperation.mappings[index];
                var filterExpressionProvider = this.filterProvider.resolve(mapping.property);
                if (filterExpressionProvider === null) {
                    continue;
                }

                var filterExpression = filterExpressionProvider.createFilter(mapping, scope.filters);
                if (filterExpression === null) {
                    continue;
                }

                instance = instance || {};
                instance[mapping.propertyName(listOperation)] = (listOperation.isRdf ? [{ "@value": filterExpression }] : filterExpression);
            }
        }

        _OperationRenderer.handleOperation.call(this, scope, listOperation, candidateMethod, instance, _OperationRenderer.onLoadEntityListSuccess, null, _OperationRenderer.loadEntityList, page, true);
    };

    _OperationRenderer.entityEquals = function(leftOperand, rightOperand, operation) {
        if ((!leftOperand) || (!rightOperand)) {
            return false;
        }

        if (leftOperand === rightOperand) {
            return true;
        }

        if (!operation.isRdf) {
            var supportedProperties = operation.owner.supportedProperties;
            for (var index = 0; index < supportedProperties.length; index++) {
                var supportedProperty = supportedProperties[index];
                if (supportedProperty.key) {
                    return leftOperand[supportedProperty.propertyName(operation)] === rightOperand[supportedProperty.propertyName(operation)];
                }
            }

            return false;
        }

        return (leftOperand["@id"] === rightOperand["@id"]);
    };

    _OperationRenderer.findEntityListMethod = function(listOperation) {
        var candidateMethod = null;
        for (var index = 0; index < listOperation.methods.length; index++) {
            if ((candidateMethod !== null) && (listOperation.methods[index] !== "GET")) {
                continue;
            }

            candidateMethod = listOperation.methods[index];
            if (listOperation.methods[index] === "GET") {
                break;
            }
        }

        return candidateMethod;
    };

    _OperationRenderer.sanitizeEntity = function(instance, operation) {
        if (!operation.isRdf) {
            return instance;
        }

        for (var property in instance) {
            if ((instance.hasOwnProperty(property)) && (instance[property] instanceof Array)) {
                if (instance[property.length] === 0) {
                    delete instance[property];
                }
                else {
                    for (var index = 0; instance < instance[property].length; index++) {
                        _OperationRenderer.sanitizeEntity.call(this, instance[property][index], operation);
                    }
                }
            }
        }

        return instance;
    };

    _OperationRenderer.initialize = function(scope, index) {
        if (index === -1) {
            scope.uniqueId.footer = "instance_" + Math.random().toString().replace(".", "").substr(1);
        }
        else {
            scope.uniqueId[index] = "instance_" + Math.random().toString().replace(".", "").substr(1);
        }
    };

    _OperationRenderer.isFormDisabled = function(scope, name) {
        var forms = document.getElementsByName(name);
        return (forms.length > 0 ? angular.element(forms[0]).scope()[name].$invalid : true) && (scope.footerVisible);
    };

    _OperationRenderer.handleUnauthorized = function(scope, challenge, callback) {
        if (this._authenticationEventHandler) {
            this._authenticationEventHandler();
            delete this._authenticationEventHandler;
        }

        var that = this;
        this._authenticationEventHandler = scope.onEvent(ursa.view.Events.Authenticate, function (e, userName, password) {
            that.authenticationProvider.authenticate(challenge, userName, password).
                then(function (authorization) {
                    that._authorization = authorization;
                    callback();
                    return authorization;
                });
        });

        scope.rootScope.broadcastEvent(ursa.view.Events.AuthenticationRequired);
    };

    _OperationRenderer.handleAuthorized = function(scope) {
        if (this._authenticationEventHandler) {
            this._authenticationEventHandler();
            delete this._authenticationEventHandler;
        }

        if (this._authorization) {
            this.authenticationProvider.use(this._authorization);
            delete this._authorization;
        }

        scope.rootScope.broadcastEvent(ursa.view.Events.Authenticated);
    };

    _OperationRenderer.prepareRequest = function (operation, methodOverride, instance, isList) {
        var url = operation.createCallUrl(instance);
        var request = new ursa.web.HttpRequest(methodOverride || operation.methods[0], url, { Accept: operation.mediaTypes.join() });
        request.initialInstanceId = undefined;
        if ((operation.mediaTypes.length > 0) && (bodylessVerbs.indexOf(operation.methods[0]) === -1)) {
            request.headers["Content-Type"] = operation.mediaTypes[0];
        }

        if (isList) {
            request.headers["Accept-Ranges"] = "members";
        }

        if (instance) {
            if (operation.isRdf) {
                if (instance["@id"] === undefined) {
                    request.initialInstanceId = true;
                    instance["@id"] = url;
                }
                else if ((instance["@id"] === null) || (instance["@id"] === "") || (instance["@id"].indexOf("_:") === 0)) {
                    request.initialInstanceId = instance["@id"];
                    instance["@id"] = url;
                }
            }

            request.data = JSON.stringify(_OperationRenderer.sanitizeEntity.call(this, instance, operation));
        }

        if (this._authorization) {
            request.headers.Authorization = this._authorization;
        }

        return request;
    };

    _OperationRenderer.handleOperation = function(scope, operation, methodOverride, instance, success, failure, callbackMethod, context, isList) {
        var that = this;
        var request = _OperationRenderer.prepareRequest.call(this, operation, methodOverride, instance, isList);
        var promise = this.httpService.sendRequest(request).
            then(function (response) {
                if (typeof (success) === "function") {
                    success.call(that, scope, operation, instance, request, response);
                }

                _OperationRenderer.handleAuthorized.call(that, scope);
                scope.updateView();
                return response;
            });

        promise.catch(function (response) {
            if (typeof (failure) === "function") {
                failure.call(that, scope, operation, instance, request, response);
            }

            if (response.status === ursa.model.HttpStatusCodes.Unauthorized) {
                if (that._authenticationEventHandler) {
                    scope.rootScope.broadcastEvent(ursa.view.Events.AuthenticationFailed, response.statusText);
                }
                else {
                    var callback = function () { callbackMethod.call(that, scope, operation, instance, context); };
                    var challenge = ((response.headers["WWW-Authenticate"]) || (response.headers["X-WWW-Authenticate"])).split(/[ ;]/g)[0].toLowerCase();
                    _OperationRenderer.handleUnauthorized.call(that, scope, challenge, callback);
                }
            }

            scope.updateView();
            return response;
        });
    };
}(namespace("ursa.view")));