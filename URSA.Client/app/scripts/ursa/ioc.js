(function(namespace) {
    var ArgumentException = (namespace.ArgumentException = function(argumentName) {
        Error.prototype.constructor.call(this, String.format("Argument '{0}' is invalid.", argumentName));
    })[":"](Error);
    ArgumentException.toString = function() { return "ursa.ArgumentException"; };
    var ArgumentNullException = (namespace.ArgumentNullException = function(argumentName) {
        Error.prototype.constructor.call(this, String.format("Argument '{0}' cannot be null.", argumentName));
    })[":"](ArgumentException);
    ArgumentNullException.toString = function() { return "ursa.ArgumentNullException"; };
    var ArgumentOutOfRangeException = (namespace.ArgumentOutOfRangeException = function (argumentName) {
        Error.prototype.constructor.call(this, String.format("Argument '{0}' is out of range.", argumentName));
    })[":"](ArgumentException);
    ArgumentOutOfRangeException.toString = function() { return "ursa.ArgumentOutOfRangeException"; };
    var InvalidOperationException = (namespace.InvalidOperationException = function () {
        Error.prototype.constructor.apply(this, arguments);
    })[":"](Error);
    InvalidOperationException.toString = function() { return "ursa.InvalidOperationException"; };

    var Scope = namespace.Scope = { Singleton: "singleton", Transient: "transient" };

    var Registration = namespace.Registration = function(serviceType) {
        Function.requiresArgument("serviceType", serviceType, Function);
        this._serviceType = serviceType;
        this._implementationType = null;
        this._instance = null;
        this._scope = Scope.Transient;
        this._name = null;
        this._dependencies = [];
    };
    Object.defineProperty(Registration.prototype, "_serviceType", { enumerable: false, configurable: false, writable: true, value: null });
    Object.defineProperty(Registration.prototype, "_implementationType", { enumerable: false, configurable: false, writable: true, value: null });
    Object.defineProperty(Registration.prototype, "_instance", { enumerable: false, configurable: false, writable: true, value: null });
    Object.defineProperty(Registration.prototype, "_scope", { enumerable: false, configurable: false, writable: true, value: null });
    Object.defineProperty(Registration.prototype, "_name", { enumerable: false, configurable: false, writable: true, value: null });
    Object.defineProperty(Registration.prototype, "_dependencies", { enumerable: false, configurable: false, writable: true, value: null });
    Registration.prototype.implementedBy = function(implementationType) {
        Function.requiresArgument("implementationType", implementationType, this._serviceType);
        this._implementationType = implementationType;
        this._name = implementationType.toString();
        return this;
    };
    Registration.prototype.instance = function(instance) {
        Function.requiresArgument("instance", instance, this._serviceType);
        this._instance = instance;
        this._implementationType = instance.prototype;
        return this;
    };
    Registration.prototype.lifestyleSingleton = function() {
        this._scope = Scope.Singleton;
        return this;
    };
    Registration.prototype.lifestyleTransient = function() {
        this._scope = Scope.Transient;
        return this;
    };
    Registration.prototype.named = function(name) {
        this._name = name;
        return this;
    };
    Registration.toString = function() { return "ursa.Registration"; };
    var _Registration = {}
    _Registration.initialize = function() {
        var index;
        if ((this._implementationType.dependencies) && (this._implementationType.dependencies instanceof Array)) {
            for (index = 0; index < this._implementationType.dependencies.length; index++) {
                this._dependencies.push(this._implementationType.dependencies[index]);
            }
        }
        else {
            var code = Function.prototype.toString.call(this._implementationType);
            var parameters = code.match(/\(([^)]*)\)/);
            if ((parameters !== null) && (parameters.length > 1)) {
                parameters = parameters[1].split(",");
                for (index = 0; index < parameters.length; index++) {
                    var parameter = parameters[index].trim();
                    if (parameter.length === 0) {
                        continue;
                    }

                    this._dependencies.push(parameter);
                }
            }
        }
    };

    var RegistrationsCollection = (namespace.RegistrationsCollection = function(owner) {
        Array.prototype.constructor.apply(this, Array.prototype.slice.call(arguments, 1));
        this._owner = owner || null;
    })[":"](Array);
    Object.defineProperty(RegistrationsCollection.prototype, "_owner", { enumerable: false, configurable: false, writable: true, value: null });
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

    var Component = namespace.Component = {};
    Component.for = function(type) { return new Registration(type); }

    var Resolver = namespace.Resolver = function() {};
    Resolver.prototype.isApplicableTo = function(dependency) {
        Function.requiresArgument("dependency", dependency, "string");
        return false;
    };
    Resolver.prototype.resolve = function(dependency, dependencyStack) {
        Function.requiresArgument("dependency", dependency, "string");
        Function.requiresArgument("dependencyStack", dependencyStack, Array);
        return null;
    };
    Object.defineProperty(Resolver.prototype, "container", { enumerable: false, configurable: false, writable: true, value: null });

    var arrayIndicators = ["arrayOf", "collectionOf", "enumerationOf"];
    var ArrayResolver = (namespace.ArrayResolver = function() {})[":"](Resolver);
    ArrayResolver.prototype.isApplicableTo = function(dependency) {
        Resolver.prototype.isApplicableTo.apply(this, arguments);
        var normalizedDependency = _ArrayResolver.normalize.call(this, dependency);
        return (normalizedDependency !== dependency);
    };
    ArrayResolver.prototype.resolve = function(dependency, dependencyStack) {
        dependency = _ArrayResolver.normalize.call(this, dependency);
        var result = [];
        var registrationTypes = this.container.findType(dependency, true);
        for (var index = 0; index < registrationTypes.length; index++) {
            var instance = _Container.resolveInternal.call(this.container, registrationTypes[index], dependencyStack);
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

    var InstanceResolver = (namespace.InstanceResolver = function() {})[":"](Resolver);
    InstanceResolver.prototype.isApplicableTo = function(dependency) {
        Function.requiresArgument("dependency", dependency, "string");
        var normalizedDependency = _ArrayResolver.normalize.call(this, dependency);
        return (normalizedDependency === dependency);
    };
    InstanceResolver.prototype.resolve = function(dependency, dependencyStack) {
        var argumentRegistration = this.container.findType(dependency);
        if (argumentRegistration !== null) {
            return _Container.resolveInternal.call(this.container, argumentRegistration, dependencyStack);
        }

        return null;
    };

    var Container = namespace.Container = function() {
        this._registrations = new RegistrationsCollection(this);
        this._resolvers = [];
        this.withResolver(new InstanceResolver()).withResolver(new ArrayResolver());
    };
    Object.defineProperty(Container.prototype, "_registrations", { enumerable: false, configurable: false, writable: true, value: null });
    Object.defineProperty(Container.prototype, "_resolvers", { enumerable: false, configurable: false, writable: true, value: null });
    Container.prototype.withResolver = function(resolver) {
        Function.requiresArgument("resolver", resolver, Resolver);
        this._resolvers.push(resolver);
        resolver.container = this;
        return this;
    };
    Container.prototype.register = function (registration) {
        Function.requiresArgument("registration", registration, Registration);
        if (this._registrations.indexOf(registration._name) !== -1) {
            throw new Error(String.format("Registration with name of '{0}' already exists.", registration._name));
        }

        _Registration.initialize.call(registration);
        this._registrations.push(registration);
    };
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

        return _Container.resolveInternal.call(this, registration, []);
    };
    Container.toString = function() { return "ursa.Container"; };
    Object.defineProperty(Container.prototype, "findType", { enumerable: false, configurable: false, writeable: false, value: function(dependency, useServiceType) {
        useServiceType = (typeof(useServiceType) === "boolean" ? useServiceType : false);
        var result = [];
        dependency = dependency.toLowerCase();
        for (var index = 0; index < this._registrations.length; index++) {
            var registration = this._registrations[index];
            var targetName = (useServiceType ? registration._serviceType.toString() : registration._name);
            var typeNames = [targetName];
            if ((!useServiceType) && (targetName !== registration._implementationType.toString())) {
                typeNames.push(registration._implementationType.toString());
            }

            for (var typeNameIndex = 0; typeNameIndex < typeNames.length; typeNameIndex++) {
                var typeName = typeNames[typeNameIndex];
                if (dependency === typeName) {
                    if (useServiceType) {
                        result.push(registration);
                    }
                    else {
                        return registration;
                    }
                }

                typeName = typeName.split(".");
                typeName = typeName[typeName.length - 1];
                if (typeName.toLowerCase().indexOf(dependency) !== -1) {
                    if (useServiceType) {
                        result.push(registration);
                    }
                    else {
                        return registration;
                    }
                }
            }
        }

        return (useServiceType ? result : null);
    } });
    var _Container = {};
    _Container.resolveInternal = function(registration, dependencyStack) {
        if (dependencyStack.indexOf(registration) !== -1) {
            throw new InvalidOperationException(String.format("Dependency loop detected for type '{0}'.", registration._implementationType));
        }

        dependencyStack.push(registration);
        var args = _Container.resolveArguments.call(this, registration, dependencyStack);
        dependencyStack.pop();
        return _Container.resolveInstance.call(this, registration, args);
    };
    _Container.resolveArguments = function(registration, dependencyStack) {
        var args = [];
        for (var index = 0; index < registration._dependencies.length; index++) {
            var argumentInstance = null;
            var dependency = registration._dependencies[index];
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
        var instance = new (Function.prototype.bind.apply(registration._implementationType, args));
        if (registration._scope === Scope.Singleton) {
            return registration._instance = instance;
        }

        return instance;
    };
}(namespace("ursa")));