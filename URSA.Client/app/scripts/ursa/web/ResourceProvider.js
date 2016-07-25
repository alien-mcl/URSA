/*globals ursa, namespace */
(function(namespace) {
    "use strict";

    var _ResourceProvider = {};

    /**
     * Provides an abstraction API over restful resources.
     * @memberof ursa.web
     * @name ResourceProvider
     * @public
     * @class
     * @param supportedClass {ursa.model.Type} Supported class of this resource provider.
     * @param http {ursa.web.HttpService} HTTP cummunication facility.
     */
    var ResourceProvider = namespace.ResourceProvider = function(supportedClass, http, filterProvider, promiseProvider) {
        Function.requires("supportedClass", supportedClass, ursa.model.Type);
        Function.requires("http", http, ursa.web.HttpService);
        Function.requiresArgument("filterProvider", filterProvider, ursa.model.FilterProvider);
        Function.requiresArgument("promiseProvider", promiseProvider, ursa.IPromiseProvider);
        this.supportedClass = supportedClass;
        this.http = http;
        this.filterProvider = filterProvider;
        this.promiseProvider = promiseProvider;
        var allCandidate = null;
        var getCandidate = null;
        var putCandidate = null;
        var postCandidate = null;
        var deleteCandidate = null;
        for (var operationIndex = 0; operationIndex < supportedClass.supportedOperations.length; operationIndex++) {
            var supportedOperation = supportedClass.supportedOperations[operationIndex];
            if ((supportedOperation.returns.length > 0) && (supportedOperation.returns[0].maxOccurances === Number.MAX_VALUE) && 
                (allCandidate === null) && (supportedOperation.methods.indexOf("GET") !== -1)) {
                    allCandidate = supportedOperation;
            }
            else if ((supportedOperation.returns.length > 0) && (supportedOperation.returns[0].maxOccurances === 1) && 
                (getCandidate === null) && (supportedOperation.methods.indexOf("GET") !== -1)) {
                getCandidate = supportedOperation;
            }
            else if ((putCandidate === null) && (supportedOperation.methods.indexOf("PUT") !== -1)) {
                putCandidate = supportedOperation;
            }
            else if ((postCandidate === null) && (supportedOperation.methods.indexOf("POST") !== -1)) {
                postCandidate = supportedOperation;
            }
            else if ((deleteCandidate === null) && (supportedOperation.methods.indexOf("DELETE") !== -1)) {
                deleteCandidate = supportedOperation;
            }
        }

        this._all = allCandidate;
        this._get = getCandidate;
        this._put = putCandidate;
        this._post = postCandidate;
        this._delete = deleteCandidate;
    };

    /**
     * Gets or sets an authorization header.
     * @memberof ursa.web.ResourceProvider
     * @public
     * @property {string}
     */
    ResourceProvider.prototype.authorization = null;

    /**
     * Gets all the resources.
     * @memberof ursa.web.ResourceProvider
     * @public
     * @method all
     * @param {string} filters Optional filters and modifiers.
     * @returns {ursa.IPromise<ursa.web.HttpResponse>} The resources obtained.
     */
    ResourceProvider.prototype.all = function(filters) {
        if (this._all === null) {
            throw new ursa.NotSupportedException("Listing resources is not supported");
        }

        var instance = _ResourceProvider.resolveMappings.call(this, this._all, filters);
        var request = _ResourceProvider.prepareRequest.call(this, this._all, instance, true);
        var result = this.promiseProvider.defer();
        this.http.sendRequest(request).
            then(function(response) {
                result.resolve(response);
            }).
            catch(function(response) {
                result.reject(response);
            });
        return result;
    };

    /**
     * Gets a specific resource.
     * @memberof ursa.web.ResourceProvider
     * @public
     * @method get
     * @param {string} url Resource url.
     * @returns {ursa.IPromise<ursa.web.HttpResponse>} The resource obtained.
     */
    /**
     * Gets a specific resource.
     * @memberof ursa.web.ResourceProvider
     * @public
     * @method get
     * @param {object} instance The resource.
     * @returns {ursa.IPromise<ursa.web.HttpResponse>} The resource obtained.
     */
    ResourceProvider.prototype.get = function(urlOrInstance) {
        if (this._get === null) {
            throw new ursa.NotSupportedException("Retrieving resources is not supported.");
        }

        var request = _ResourceProvider.prepareRequest.call(this,
            this._get,
            (typeof(urlOrInstance) === "string" ? null : urlOrInstance),
            false,
            (typeof(urlOrInstance) === "string" ? urlOrInstance : null));
        var result = this.promiseProvider.defer();
        this.http.sendRequest(request).
            then(function(response) {
                result.resolve(response);
            }).
            catch(function(response) {
                result.reject(response);
            });
        return result;
    };

    /**
     * Either creates or updates a specific resource.
     * @memberof ursa.web.ResourceProvider
     * @public
     * @method get
     * @param {object} object The resource.
     * @param {string} [url] Resource url determining whether the resource is to be updated or created if no url is provided.
     * @returns {ursa.IPromise<ursa.web.HttpResponse>} Location of the resource.
     */
    ResourceProvider.prototype.set = function(resource, url) {
        if (((!url) && (this._post === null)) || ((url) && (this._put === null))) {
            throw new ursa.NotSupportedException("Neither updating nor creating resources is not supported.");
        }

        var request = _ResourceProvider.prepareRequest.call(this, (url ? this._put : this._post), resource, false);
        var result = this.promiseProvider.defer();
        this.http.sendRequest(request).
            then(function(response) {
                result.resolve(response);
            }).
            catch(function(response) {
                result.reject(response);
            });
        return result;
    };

    /**
     * Deletes a specific resource.
     * @memberof ursa.web.ResourceProvider
     * @public
     * @method delete
     * @param {string} url Resource url.
     * @returns {ursa.IPromise<ursa.web.HttpResponse>} Task of the deletion.
     */
    /**
     * Deletes a specific resource.
     * @memberof ursa.web.ResourceProvider
     * @public
     * @method delete
     * @param {object} instance The resource.
     * @returns {ursa.IPromise<ursa.web.HttpResponse>} Task of the deletion.
     */
    ResourceProvider.prototype.delete = function(urlOrInstance) {
        if (this._delete === null) {
            throw new ursa.NotSupportedException("Deleting resources is not supported.");
        }

        var request = _ResourceProvider.prepareRequest.call(this,
            this._delete,
            (typeof(urlOrInstance) === "string" ? null : urlOrInstance),
            false,
            (typeof(urlOrInstance) === "string" ? urlOrInstance : null));
        var result = this.promiseProvider.defer();
        this.http.sendRequest(request).
            then(function(response) {
                result.resolve(response);
            }).
            catch(function(response) {
                result.reject(response);
            });
        return result;
    };

    /**
     * Gets a supported class;
     * @memberof ursa.web.ResourceProvider
     * @public
     * @readonly
     * @property supportedClass {ursa.model.Type}
     */
    ResourceProvider.prototype.supportedClass = null;

    Object.defineProperty(ResourceProvider.prototype, "http", { enumerable: false, configurable: false, writable: true, value: null });

    Object.defineProperty(ResourceProvider.prototype, "filterProvider", { enumerable: false, configurable: false, writable: true, value: null });

    Object.defineProperty(ResourceProvider.prototype, "promiseProvider", { enumerable: false, configurable: false, writable: true, value: null });

    Object.defineProperty(ResourceProvider.prototype, "_all", { enumerable: false, configurable: false, writable: true, value: null });

    Object.defineProperty(ResourceProvider.prototype, "_get", { enumerable: false, configurable: false, writable: true, value: null });

    Object.defineProperty(ResourceProvider.prototype, "_put", { enumerable: false, configurable: false, writable: true, value: null });

    Object.defineProperty(ResourceProvider.prototype, "_post", { enumerable: false, configurable: false, writable: true, value: null });

    Object.defineProperty(ResourceProvider.prototype, "_delete", { enumerable: false, configurable: false, writable: true, value: null });

    _ResourceProvider.resolveMappings = function(operation, filters, instance) {
        if (operation.mappings !== null) {
            for (var index = 0; index < operation.mappings.length; index++) {
                var mapping = operation.mappings[index];
                var filterExpressionProvider = this.filterProvider.resolve(mapping.property);
                if (filterExpressionProvider === null) {
                    continue;
                }

                var filterExpression = filterExpressionProvider.createFilter(mapping, filters);
                if (filterExpression === null) {
                    continue;
                }

                instance = instance || {};
                instance[mapping.propertyName(operation)] = (operation.isRdf ? [{ "@value": filterExpression }] : filterExpression);
            }
        }

        return instance;
    };

    _ResourceProvider.sanitizeEntity = function(operation, instance) {
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
                        _ResourceProvider.sanitizeEntity.call(this, operation, instance[property][index]);
                    }
                }
            }
        }

        return instance;
    };

    _ResourceProvider.prepareRequest = function(operation, instance, isList, url) {
        url = url || operation.createCallUrl(instance);
        var request = new ursa.web.HttpRequest(operation.methods[0], url, { Accept: operation.mediaTypes.join() });
        request.initialInstanceId = undefined;
        if (operation.mediaTypes.length > 0) {
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
                else if ((instance["@id"] === null) || (instance["@id"] === "") || (instance["@id"].indexOf("_:") === 0)
                ) {
                    request.initialInstanceId = instance["@id"];
                    instance["@id"] = url;
                }
            }

            request.data = JSON.stringify(_ResourceProvider.sanitizeEntity.call(this, operation, instance));
        }

        if (this.authorization) {
            request.headers.Authorization = this.authorization;
        }

        return request;
    };

    ResourceProvider.toString = function () { return "ursa.web.ResourceProvider"; };
}(namespace("ursa.web")));