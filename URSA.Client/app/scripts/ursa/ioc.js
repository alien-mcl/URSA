/*globals namespace */
(function(namespace) {
    "use strict";

    /**
     * Represents an expression where an argument was faulty for some reason.
     * @memberof ursa
     * @name ArgumentException
     * @public
     * @class
     * @extends Error
     * @param {string} argumentName Name of the faulty argument.
     */
    var ArgumentException = (namespace.ArgumentException = function(argumentName) {
        Error.prototype.constructor.call(this, String.format("Argument '{0}' is invalid.", argumentName));
    })[":"](Error);
    ArgumentException.toString = function() { return "ursa.ArgumentException"; };

    /**
     * Represents an expression where an argument was null.
     * @memberof ursa
     * @name ArgumentNullException
     * @public
     * @class
     * @extends ursa.ArgumentException
     * @param {string} argumentName Name of the argument that was null.
     */
    var ArgumentNullException = (namespace.ArgumentNullException = function(argumentName) {
        Error.prototype.constructor.call(this, String.format("Argument '{0}' cannot be null.", argumentName));
    })[":"](ArgumentException);
    ArgumentNullException.toString = function() { return "ursa.ArgumentNullException"; };

    /**
     * Represents an expression where an argument was out of range of valid values.
     * @memberof ursa
     * @name ArgumentOutOfRangeException
     * @public
     * @class
     * @extends ursa.ArgumentException
     * @param {string} argumentName Name of the argument that was out of range.
     */
    var ArgumentOutOfRangeException = (namespace.ArgumentOutOfRangeException = function(argumentName) {
        Error.prototype.constructor.call(this, String.format("Argument '{0}' is out of range.", argumentName));
    })[":"](ArgumentException);
    ArgumentOutOfRangeException.toString = function() { return "ursa.ArgumentOutOfRangeException"; };

    /**
     * Represents an expression where an operation was invalid.
     * @memberof ursa
     * @name InvalidOperationException
     * @public
     * @class
     * @extends Error
     * @param {string} message Message of the exception.
     */
    var InvalidOperationException = (namespace.InvalidOperationException = function() {
        Error.prototype.constructor.apply(this, arguments);
    })[":"](Error);
    InvalidOperationException.toString = function() { return "ursa.InvalidOperationException"; };

    /**
     * Enumerates possible instance scopes.
     * @memberof ursa
     * @name Scope
     * @public
     * @class
     * @param {string} name Name of the scope.
     */
    var Scope = namespace.Scope = function(name) {
        Function.requiresArgument("name", name, "string");
        this.toString = function() { return name; };
    };
    /**
     * Defines a singleton scope.
     * @public
     * @static
     * @member {ursa.Scope} Singleton
     */
    Scope.Singleton = new Scope("singleton");
    /**
     * Defines a transient scope. Instances are created on each resolution attempt.
     * @memberof ursa.Scope
     * @public
     * @static
     * @member {ursa.Scope} Transient
     */
    Scope.Transient = new Scope("transient");

    /**
     * Represents a service registration.
     * @memberof ursa
     * @name Registration
     * @public
     * @class
     * @param {Function} serviceType Type of service to be registered.
     */
    var Registration = namespace.Registration = function(serviceType) {
        Function.requiresArgument("serviceType", serviceType, Function);
        this._serviceType = serviceType;
        this._implementationTypes = [];
        this._instance = null;
        this._scope = Scope.Transient;
        this._name = null;
        this._dependencies = [];
    };
    Object.defineProperty(Registration.prototype, "_serviceType", { enumerable: false, configurable: false, writable: true, value: null });
    Object.defineProperty(Registration.prototype, "_implementationTypes", { enumerable: false, configurable: false, writable: true, value: null });
    Object.defineProperty(Registration.prototype, "_instance", { enumerable: false, configurable: false, writable: true, value: null });
    Object.defineProperty(Registration.prototype, "_scope", { enumerable: false, configurable: false, writable: true, value: null });
    Object.defineProperty(Registration.prototype, "_name", { enumerable: false, configurable: false, writable: true, value: null });
    Object.defineProperty(Registration.prototype, "_dependencies", { enumerable: false, configurable: false, writable: true, value: null });
    /**
     * Defines a type to be registered as an implementation of the service type.
     * @memberof ursa
     * @public
     * @instance
     * @member {ursa.Registration} implementedBy
     * @param {Function} implementationType Type implementing a service.
     */
    Registration.prototype.implementedBy = function(implementationType) {
        Function.requiresArgument("implementationType", implementationType, this._serviceType);
        this._implementationTypes = [implementationType];
        this._name = implementationType.toString();
        return this;
    };
    /**
     * Defines an instance to be registered as an implementation of the service type.
     * @memberof ursa
     * @public
     * @instance
     * @member {ursa.Registration} instance
     * @param {object} instance Instance implementing a service.
     */
    Registration.prototype.instance = function(instance) {
        Function.requiresArgument("instance", instance, this._serviceType);
        this._instance = instance;
        this._implementationTypes = [instance.prototype];
        this._name = instance.prototype.toString();
        return this;
    };
    /**
     * Defines a singleton scope of the instances being resolved for this registration.
     * @memberof ursa
     * @public
     * @instance
     * @member {ursa.Registration} lifestyleSingleton
     */
    Registration.prototype.lifestyleSingleton = function() {
        this._scope = Scope.Singleton;
        return this;
    };
    /**
     * Defines a transient scope of the instances being resolved for this registration.
     * @memberof ursa
     * @public
     * @instance
     * @member {ursa.Registration} lifestyleTransient
     */
    Registration.prototype.lifestyleTransient = function() {
        this._scope = Scope.Transient;
        return this;
    };
    /**
     * Defines a name for this registration.
     * @memberof ursa
     * @public
     * @instance
     * @member {ursa.Registration} named
     */
    Registration.prototype.named = function(name) {
        Function.requiresArgument("name", name, "string");
        if (name.length === 0) {
            throw new ArgumentOutOfRangeException("name");
        }

        this._name = name;
        return this;
    };
    Registration.toString = function() { return "ursa.Registration"; };
    var _Registration = {};
    _Registration.initialize = function() {
        if (this._implementationTypes.length === 0) {
            throw new InvalidOperationException(String.format("Cannot finalize registration '{0}' of the '{1}' service.", this._name, this._serviceType));
        }

        for (var implementationTypeIndex = 0; implementationTypeIndex < this._implementationTypes.length; implementationTypeIndex ++) {
            var index;
            var implementationType = this._implementationTypes[implementationTypeIndex];
            var dependencies = [];
            this._dependencies.push(dependencies);
            if ((implementationType.dependencies) && (implementationType.dependencies instanceof Array)) {
                for (index = 0; index < implementationType.dependencies.length; index++) {
                    dependencies.push(implementationType.dependencies[index]);
                }
            }
            else {
                var code = Function.prototype.toString.call(implementationType);
                var parameters = code.match(/\(([^)]*)\)/);
                if ((parameters !== null) && (parameters.length > 1)) {
                    parameters = parameters[1].split(",");
                    for (index = 0; index < parameters.length; index++) {
                        var parameter = parameters[index].trim();
                        if (parameter.length === 0) {
                            continue;
                        }

                        dependencies.push(parameter);
                    }
                }
            }
        }
    };

    /**
     * Collects registrations.
     * @memberof ursa
     * @name RegistrationsCollection
     * @public
     * @class
     * @param {ursa.Container} owner Owner of this collection.
     */
    var RegistrationsCollection = (namespace.RegistrationsCollection = function(owner) {
        Array.prototype.constructor.apply(this, Array.prototype.slice.call(arguments, 1));
        this._owner = owner || null;
    })[":"](Array);
    Object.defineProperty(RegistrationsCollection.prototype, "_owner", { enumerable: false, configurable: false, writable: true, value: null });
    /**
     * Returns an index matching given criteria or -1.
     * @memberof ursa.RegistrationsCollection
     * @public
     * @instance
     * @member {number} indexOf
     * @param {ursa.Registration|string} item Name of the registration or the registration instance to be searched for.
     */
    RegistrationsCollection.prototype.indexOf = function(registrationOrName) {
        if (registrationOrName instanceof Registration) {
            return Array.prototype.indexOf.apply(this, arguments);
        }

        if (typeof(registrationOrName) === "string") {
            for (var index = 0; index < this.length; index++) {
                var item = this[index];
                if (item._name === registrationOrName) {
                    return index;
                }
            }

            return -1;
        }
    };
    RegistrationsCollection.toString = function() { return "ursa.RegistrationsCollection"; };

    /**
     * Entry point for fluent API for explicit service registrations.
     * @memberof ursa
     * @name Component
     * @public
     * @static
     * @class
     */
    var Component = namespace.Component = {};
    /**
     * Returns a registration for given type.
     * @memberof ursa.Component
     * @public
     * @static
     * @member {ursa.Registration} for
     * @param {Function} type Type of the service for which the registration is being defined.
     */
    Component.for = function(type) { return new Registration(type); };

    /**
     * Entry point for fluent API for service registrations by convention.
     * @memberof ursa
     * @name Classes
     * @public
     * @static
     * @class
     */
    var Classes = namespace.Classes = {};
    /**
     * Returns a registration for given type.
     * @memberof ursa.Classes
     * @public
     * @static
     * @member {ursa.Registration} implementing
     * @param {Function} type Type of the service to find implementations for.
     */
    Classes.implementing = function(type) {
        Function.requiresArgument("type", type, Function);
        var result = new Registration(type);
        result._implementationTypes = _Component.resolve.call(this, type, window, [], 0);
        return result;
    };
    /**
     * Gets a max resolution depth, which is 4 by default.
     * @memberof ursa.Classes
     * @public
     * @static
     * @member {number} MaxResolutionDepth
     */
    Component.MaxResolutionDepth = 4;
    var _Component = {};
    _Component.is = function(type) {
        if ((this.prototype === undefined) || (this.prototype === null)) {
            return false;
        }

        if (this.prototype instanceof type) {
            return true;
        }

        return _Component.is.call(this.prototype, type);
    };
    _Component.forbiddenProperties = [/^webkit.*/];
    _Component.forbiddenProperties.matches = function(propertyName) {
        for (var index = 0; index < _Component.forbiddenProperties.length; index++) {
            var forbiddenProperty = _Component.forbiddenProperties[index];
            if ((forbiddenProperty === propertyName) || (forbiddenProperty.test(propertyName))) {
                return true;
            }
        }

        return false;
    };
    _Component.resolve = function(type, target, result, depth) {
        if (depth > Component.maxResolutionDepth) {
            return result;
        }

        for (var property in target) {
            if (_Component.forbiddenProperties.matches(property)) {
                continue;
            }

            if ((target.hasOwnProperty(property)) && (target[property] !== undefined) && (target[property] !== null)) {
                if ((typeof(target[property]) === "object") && (target[property].__namespace)) {
                    _Component.resolve.call(this, type, target[property], result, depth + 1);
                }
                else if ((typeof(target[property]) === "function") && (result.indexOf(target[property]) === -1) && (_Component.is.call(target[property], type))) {
                    result.push(target[property]);
                }
            }
        }

        return result;
    };

    /**
     * An abstract of the type resolver.
     * @memberof ursa
     * @name Resolver
     * @public
     * @class
     */
    var Resolver = namespace.Resolver = function() {};
    /**
     * Checks if the resolver is applicable for a given dependency name.
     * @memberof ursa.Resolver
     * @public
     * @instance
     * @member {boolean} isApplicableTo
     * @param {string} dependency Name of the dependency to check applicability.
     */
    Resolver.prototype.isApplicableTo = function(dependency) {
        Function.requiresArgument("dependency", dependency, "string");
        return false;
    };
    /**
     * Resolves an instance of a given depedency.
     * @memberof ursa.Resolver
     * @public
     * @instance
     * @member {boolean} resolve
     * @param {string} dependency Name of the dependency to check applicability.
     * @param {Array<string>} dependencyStack Stack of dependencies in current context.
     */
    Resolver.prototype.resolve = function(dependency, dependencyStack) {
        Function.requiresArgument("dependency", dependency, "string");
        Function.requiresArgument("dependencyStack", dependencyStack, Array);
        return null;
    };
    /**
     * Contains a reference to the owning container.
     * @memberof ursa.Resolver
     * @protected
     * @instance
     * @member {ursa.Container} container
     */
    Object.defineProperty(Resolver.prototype, "container", { enumerable: false, configurable: false, writable: true, value: null });

    var arrayIndicators = ["arrayOf", "collectionOf", "enumerationOf"];
    /**
     * Resolves arrays of types.
     * @memberof ursa
     * @name ArrayResolver
     * @public
     * @class
     */
    var ArrayResolver = (namespace.ArrayResolver = function() {})[":"](Resolver);
    ArrayResolver.prototype.isApplicableTo = function(dependency) {
        Resolver.prototype.isApplicableTo.apply(this, arguments);
        var normalizedDependency = _ArrayResolver.normalize.call(this, dependency);
        return (normalizedDependency !== dependency);
    };
    ArrayResolver.prototype.resolve = function(dependency, dependencyStack) {
        dependency = _ArrayResolver.normalize.call(this, dependency);
        var result = [];
        var registrationTypes = this.container.findServices(dependency);
        for (var index = 0; index < registrationTypes.length; index++) {
            var instance = this.container.resolveInternal(registrationTypes[index], dependencyStack);
            if (instance !== null) {
                result.push(instance);
            }
        }

        return result;
    };
    var _ArrayResolver = {};
    _ArrayResolver.normalize = function(dependency) {
        for (var indicatorIndex = 0; indicatorIndex < arrayIndicators.length; indicatorIndex++) {
            var arrayIndicator = arrayIndicators[indicatorIndex];
            if (dependency.indexOf(arrayIndicator) === 0) {
                dependency = dependency.substring(arrayIndicator.length);
                break;
            }
        }

        return dependency;
    };

    /**
     * Resolves single instances of types.
     * @memberof ursa
     * @name InstanceResolver
     * @public
     * @class
     */
    var InstanceResolver = (namespace.InstanceResolver = function() {})[":"](Resolver);
    InstanceResolver.prototype.isApplicableTo = function(dependency) {
        Function.requiresArgument("dependency", dependency, "string");
        var normalizedDependency = _ArrayResolver.normalize.call(this, dependency);
        return (normalizedDependency === dependency);
    };
    InstanceResolver.prototype.resolve = function(dependency, dependencyStack) {
        var argumentRegistration = this.container.findType(dependency);
        if (argumentRegistration !== null) {
            return this.container.resolveInternal(argumentRegistration, dependencyStack);
        }

        return null;
    };

    /**
     * Inverse of Control container for resolving instances of given types.
     * @memberof ursa
     * @name Container
     * @public
     * @class
     */
    var Container = namespace.Container = function() {
        this._registrations = new RegistrationsCollection(this);
        this._resolvers = [];
        this.withResolver(new InstanceResolver()).withResolver(new ArrayResolver());
    };
    Object.defineProperty(Container.prototype, "_registrations", { enumerable: false, configurable: false, writable: true, value: null });
    Object.defineProperty(Container.prototype, "_resolvers", { enumerable: false, configurable: false, writable: true, value: null });
    /**
     * Allows to add a custom instance resolver into the pipeline.
     * @memberof ursa
     * @public
     * @instance
     * @member {ursa.Container} withResolver
     * @param {ursa.Resolver} resolver The resolver to be added.
     */
    Container.prototype.withResolver = function(resolver) {
        Function.requiresArgument("resolver", resolver, Resolver);
        this._resolvers.push(resolver);
        resolver.container = this;
        return this;
    };
    /**
     * Adds a given registration.
     * @memberof ursa
     * @public
     * @instance
     * @member {ursa.Container} register
     * @param {ursa.Registration} registration Registration to be added.
     */
    Container.prototype.register = function(registration) {
        Function.requiresArgument("registration", registration, Registration);
        _Registration.initialize.call(registration);
        if (registration._implementationTypes.length === 1) {
            if (this._registrations.indexOf(registration._name) !== -1) {
                throw new Error(String.format("Registration with name of '{0}' already exists.", registration._name));
            }

            this._registrations.push(registration);
            return this;
        }

        for (var index = 0; index < registration._implementationTypes.length; index++) {
            var implementationType = registration._implementationTypes[index];
            var newRegistration = new Registration(registration._serviceType);
            newRegistration._implementationTypes = [implementationType];
            newRegistration._scope = registration._scope;
            newRegistration._name = implementationType.toString();
            newRegistration._dependencies = registration._dependencies[index];
            if (this._registrations.indexOf(newRegistration._name) !== -1) {
                throw new Error(String.format("Registration with name of '{0}' already exists.", newRegistration._name));
            }

            this._registrations.push(newRegistration);
        }

        return this;
    };
    /**
     * Resolves an instance of a given type.
     * @memberof ursa
     * @public
     * @instance
     * @member {object} resolve
     * @param {Function} type Type to resolve instance of.
     */
    Container.prototype.resolve = function(type) {
        Function.requiresArgument("type", type, Function);
        var registration = null;
        for (var index = 0; index < this._registrations.length; index++) {
            var currentRegistration = this._registrations[index];
            if ((currentRegistration._serviceType.prototype instanceof type) || (currentRegistration._serviceType === type)) {
                registration = currentRegistration;
                break;
            }
        }

        if (registration === null) {
            throw new InvalidOperationException(String.format("There are no components registered for service type of '{0}'.", type.toString()));
        }

        return this.resolveInternal(registration, []);
    };
    Container.toString = function() { return "ursa.Container"; };
    /**
     * Searches registration for a given dependency name.
     * @memberof ursa
     * @protected
     * @instance
     * @member {ursa.Registration} findType
     * @param {string} dependency Dependency name to find the registration for.
     */
    Object.defineProperty(Container.prototype, "findType", { enumerable: false, configurable: false, writeable: false, value: function(dependency) {
        dependency = dependency.toLowerCase();
        for (var index = 0; index < this._registrations.length; index++) {
            var registration = this._registrations[index];
            var typeNames = [registration._name];
            if (typeNames[0] !== registration._implementationTypes[0].toString()) {
                typeNames.push(registration._implementationTypes[0].toString());
            }

            for (var typeNameIndex = 0; typeNameIndex < typeNames.length; typeNameIndex++) {
                var typeName = typeNames[typeNameIndex];
                if (dependency === typeName) {
                    return registration;
                }

                typeName = typeName.split(".");
                typeName = typeName[typeName.length - 1];
                if (typeName.toLowerCase().indexOf(dependency) !== -1) {
                    return registration;
                }
            }
        }

        return null;
    } });
    /**
     * Searches registration for a given service name.
     * @memberof ursa
     * @protected
     * @instance
     * @member {ursa.Registration} findServices
     * @param {string} dependency Service name to find the registration for.
     */
    Object.defineProperty(Container.prototype, "findServices", { enumerable: false, configurable: false, writeable: false, value: function(dependency) {
        var result = [];
        dependency = dependency.toLowerCase();
        for (var index = 0; index < this._registrations.length; index++) {
            var registration = this._registrations[index];
            var typeName = registration._serviceType.toString();
            if (dependency === typeName) {
                result.push(registration);
                continue;
            }

            typeName = typeName.split(".");
            typeName = typeName[typeName.length - 1];
            if (typeName.toLowerCase().indexOf(dependency) !== -1) {
                result.push(registration);
            }
        }

        return result;
    } });
    /**
     * Resolves a given registration.
     * @memberof ursa
     * @protected
     * @instance
     * @member {object} resolveInternal
     * @param {ursa.Registration} registration Registration to resolve.
     * @param {Array<string>} dependencyStack Stack of dependencies in current context.
     */
    Object.defineProperty(Container.prototype, "resolveInternal", { enumerable: false, configurable: false, writeable: false, value: function(registration, dependencyStack) {
        if (dependencyStack.indexOf(registration) !== -1) {
            throw new InvalidOperationException(String.format("Dependency loop detected for type '{0}'.", registration._implementationTypes[0]));
        }

        dependencyStack.push(registration);
        var args = _Container.resolveArguments.call(this, registration, dependencyStack);
        dependencyStack.pop();
        return _Container.resolveInstance.call(this, registration, args);
    } });
    var _Container = {};
    _Container.resolveArguments = function(registration, dependencyStack) {
        var args = [];
        for (var index = 0; index < registration._dependencies[0].length; index++) {
            var argumentInstance = null;
            var dependency = registration._dependencies[0][index];
            var resolver = null;
            for (var resolverIndex = 0; resolverIndex < this._resolvers.length; resolverIndex++) {
                if (this._resolvers[resolverIndex].isApplicableTo(dependency)) {
                    resolver = this._resolvers[resolverIndex];
                    break;
                }
            }

            if (resolver !== null) {
                argumentInstance = resolver.resolve(dependency, dependencyStack);
            }

            args.push(argumentInstance);
        }

        return args;
    };
    _Container.resolveInstance = function(registration, args) {
        if ((registration._scope === Scope.Singleton) && (registration._instance !== null)) {
            return registration._instance;
        }

        args.splice(0, 0, null);
        var BoundType = Function.prototype.bind.apply(registration._implementationTypes[0], args);
        var instance = new BoundType();
        if (registration._scope === Scope.Singleton) {
            return (registration._instance = instance);
        }

        return instance;
    };
}(namespace("ursa")));