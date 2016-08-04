/*globals namespace, ursa, hydra, confirm */
(function(namespace) {
    "use strict";

    var _OperationRenderer = {};

    /**
     * Default renderer for {@link ursa.model.Operation}.
     * @memberof ursa.view
     * @name OperationRenderer
     * @protected
     * @class
     * @extends ursa.view.ViewRenderer
     */
    var OperationRenderer = (namespace.OperationRenderer = function(resourceProviderBuilder, jsonLdProcessor, authenticationProvider) {
        ursa.view.ViewRenderer.prototype.constructor.call(this, resourceProviderBuilder.httpService, jsonLdProcessor, authenticationProvider, resourceProviderBuilder.filterProvider);
        this.resourceProviderBuilder = resourceProviderBuilder;
    })[":"](ursa.view.ViewRenderer);

    OperationRenderer.prototype.isApplicableTo = function(apiMember) {
        ursa.view.ViewRenderer.prototype.isApplicableTo.apply(this, arguments);
        return apiMember instanceof ursa.model.Operation;
    };

    OperationRenderer.prototype.initialize = function(apiMember) {
        Function.requiresArgument("apiMember", apiMember, ursa.model.Operation);
        ursa.view.ViewRenderer.prototype.initialize.apply(this, arguments);
        this.resourceProvider = this.resourceProviderBuilder.createFor(apiMember);
    };

    OperationRenderer.prototype.render = function(scope, classNames) {
        ursa.view.ViewRenderer.prototype.render.apply(this, arguments);
        if ((this.apiMember.returns.length === 1) && (this.apiMember.returns[0].maxOccurances === Number.MAX_VALUE)) {
            return _OperationRenderer.renderEntityList.call(this, scope, classNames);
        }

        return "";
    };

    OperationRenderer.toString = function() { return "ursa.view.OperationRenderer"; };

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
                (this.resourceProvider.canRead ? "<button class=\"btn btn-default\" title=\"Edit\" ng-click=\"get(entity)\"><span class=\"glyphicon glyphicon-pencil\"></span></button>" : "") +
                (this.resourceProvider.canDelete ? "<button class=\"btn btn-default\" title=\"Delete\" ng-click=\"delete(entity, $event)\"><span class=\"glyphicon glyphicon-remove\"></span></button>" : "") +
                "</div></td>" +
                "</tr>", scope.keyProperty, (scope.keyProperty !== "" ? " track by entity['" + scope.keyProperty + "']" : "")) + String.format(
                "<tr ng-repeat-end ng-show=\"entityEquals(entity)\" ng-init=\"initialize($index)\">" +
                "<td colspan=\"{0}\"><div><ursa-api-member-view api-member=\"operation.owner\" target-instance=\"editedEntity\" unique-id=\"{{ uniqueId[$index] }}\"></ursa-api-member-view></div></td>" +
                "<td><div class=\"btn-block\">" +
                (this.resourceProvider.canUpdate ? "<button class=\"btn btn-default\" title=\"Update\" ng-disabled=\"isFormDisabled(uniqueId[$index])\" ng-click=\"update(editedEntity)\"><span class=\"glyphicon glyphicon-floppy-disk\"></span></button>" : "") +
                "<button class=\"btn btn-default\" title=\"Cancel\" ng-click=\"cancel()\"><span class=\"glyphicon glyphicon-repeat\"></span></button>" +
                "</div></td>" +
                "</tr>", scope.supportedProperties.length - 1) + String.format(
                "<tr ng-hide=\"editedEntity !== null\" ng-init=\"initialize(-1)\">" +
                "<td colspan=\"{0}\"><div ng-show=\"footerVisible\"><ursa-api-member-view api-member=\"operation.owner\" target-instance=\"newInstance\" unique-id=\"{{ uniqueId.footer }}\"></ursa-api-member-view></div></td>" +
                "<td><div class=\"btn-block\">" +
                (this.resourceProvider.canCreate ? "<button class=\"btn btn-default\" title=\"Create\" ng-disabled=\"isFormDisabled(uniqueId.footer)\" ng-click=\"create(newInstance)\"><span class=\"glyphicon glyphicon-plus\"></span></button>" : "") +
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
        if (this.resourceProvider.canCreate) {
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
        scope.getPropertyValue = function(entity, supportedProperty, operation) { return entity[supportedProperty.propertyName(operation)]; };
        scope.cancel = function() { scope.editedEntity = null; };
        scope.list = function(page) { _OperationRenderer.loadEntityList.call(that, scope, null, page); };
        scope.get = function(instance) { _OperationRenderer.loadEntity.call(that, scope, instance); };
        scope.create = function(instance) { _OperationRenderer.createEntity.call(that, scope, instance); };
        scope.update = function(instance) { _OperationRenderer.updateEntity.call(that, scope, instance); };
        scope.delete = function(instance, e) { _OperationRenderer.deleteEntity.call(that, scope, instance, e); };
        scope.entityEquals = function(entity) { return _OperationRenderer.entityEquals.call(that, entity, scope.editedEntity, scope.operation); };
        scope.initialize = function(index) { _OperationRenderer.initialize.call(that, scope, index); };
        scope.isFormDisabled = function(name) { return _OperationRenderer.isFormDisabled.call(that, scope, name); };
    };

    _OperationRenderer.onLoadEntitySuccess = function(scope, instance, response) {
        var that = this;
        if ((response.headers["Content-Type"] || "*/*").indexOf(ursa.model.EntityFormat.ApplicationLdJson) === 0) {
            that.jsonLdProcessor.expand(response.data).
                then(function(expanded) {
                    scope.rootScope.broadcastEvent(ursa.view.Events.EntityLoaded, scope.editedEntity = (expanded.length > 0 ? expanded[0] : null), that.apiMember.owner);
                    scope.updateView();
                    return expanded;
                });
        }
        else {
            scope.rootScope.broadcastEvent(ursa.view.Events.EntityLoaded, scope.editedEntity = response.data, that.apiMember.owner);
        }
    };

    _OperationRenderer.loadEntity = function(scope, instance) {
        _OperationRenderer.handleOperation.call(this, scope, this.resourceProvider.get, instance, _OperationRenderer.onLoadEntitySuccess, null, _OperationRenderer.loadEntity);
    };

    _OperationRenderer.createEntitySuccess = function(scope, instance) {
        scope.newInstance = this.apiMember.owner.createInstance(this.apiMember);
        scope.rootScope.broadcastEvent(ursa.view.Events.EntityCreated, instance, this.apiMember.owner);
        scope.list(1);
        scope.footerVisible = false;
    };

    _OperationRenderer.ammendEntityFailure = function(scope, instance, response) {
        if (response.request.initialInstanceId !== undefined) {
            if (response.request.initialInstanceId) {
                delete instance["@id"];
            }
            else {
                instance["@id"] = response.request.initialInstanceId;
            }
        }
    };

    _OperationRenderer.createEntity = function(scope, instance) {
        if (!scope.footerVisible) {
            scope.footerVisible = true;
            return;
        }

        _OperationRenderer.handleOperation.call(this, scope, this.resourceProvider.set, instance, _OperationRenderer.createEntitySuccess, _OperationRenderer.ammendEntityFailure, _OperationRenderer.createEntity);
    };

    _OperationRenderer.onUpdateEntitySuccess = function(scope, instance) {
        scope.editedEntity = null;
        scope.rootScope.broadcastEvent(ursa.view.Events.EntityModified, instance, this.apiMember.owner);
        scope.list();
    };

    _OperationRenderer.updateEntity = function(scope, instance) {
        _OperationRenderer.handleOperation.call(this, scope, this.resourceProvider.set, instance, _OperationRenderer.onUpdateEntitySuccess, _OperationRenderer.ammendEntityFailure, _OperationRenderer.updateEntity);
    };

    _OperationRenderer.onDeleteEntitySuccess = function(scope, instance) {
        scope.rootScope.broadcastEvent(ursa.view.Events.EntityRemoved, instance, this.apiMember.owner);
        scope.list(1);
    };

    _OperationRenderer.deleteEntity = function(scope, instance, e) {
        if ((e) && (!confirm("Are you sure you want to delete this item?"))) {
            e.preventDefault();
            e.stopPropagation();
            return;
        }

        _OperationRenderer.handleOperation.call(this, scope, this.resourceProvider.delete, instance, _OperationRenderer.onDeleteEntitySuccess, null, _OperationRenderer.deleteEntity);
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

    _OperationRenderer.onLoadEntityListSuccess = function(scope, instance, response) {
        var that = this;
        _OperationRenderer.parseContentRange.call(this, scope, response);
        if ((response.headers["Content-Type"] || "*/*").indexOf(ursa.model.EntityFormat.ApplicationLdJson) === 0) {
            that.jsonLdProcessor.expand(response.data).
                then(function(expanded) {
                    scope.entities = _OperationRenderer.parseHypermediaControls.call(this, scope, response, expanded);
                    scope.updateView();
                    return expanded;
                });
        }
        else {
            scope.entities = response.data;
        }
    };

    _OperationRenderer.loadEntityList = function(scope, instance, page) {
        if (typeof(page) === "number") {
            scope.filters.currentPage = page;
        }

        _OperationRenderer.handleOperation.call(this, scope, this.resourceProvider.all, instance, _OperationRenderer.onLoadEntityListSuccess, null, _OperationRenderer.loadEntityList, page);
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
        this._authenticationEventHandler = scope.onEvent(ursa.view.Events.Authenticate, function(e, userName, password) {
            that.authenticationProvider.authenticate(challenge, userName, password).
                then(function(authorization) {
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

    _OperationRenderer.handleOperation = function(scope, instance, operation, success, failure, callbackMethod, context) {
        var that = this;
        var promise = operation(operation === this.resourceProvider.all ? scope.filters : instance).
            then(function(response) {
                if (typeof(success) === "function") {
                    success.call(that, scope, instance, response);
                }

                _OperationRenderer.handleAuthorized.call(that, scope);
                scope.updateView();
                return response;
            });

        promise.catch(function(response) {
            if (typeof(failure) === "function") {
                failure.call(that, scope, operation, instance, response);
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