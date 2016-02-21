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

    Object.defineProperty(Function, "requiresArgument", { enumerable: false, configurable: false, value:function (argumentName, argumentValue, argumentType) {
        if (argumentValue === undefined) {
            throw new ArgumentException(argumentName);
        }

        if (argumentValue === null) {
            throw new ArgumentNullException(argumentName);
        }

        if (!argumentType) {
            return;
        }

        if (((typeof(argumentType) === "string") && (typeof (argumentValue) !== argumentType)) ||
            ((argumentType instanceof Function) && ((argumentValue !== argumentType) && 
                (!(argumentValue.prototype instanceof argumentType)) && (!(argumentValue instanceof argumentType))))) {
            throw new ArgumentOutOfRangeException(argumentName);
        }
    } });

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
    Object.defineProperty(Registration.prototype, "_serviceType", { enumerable: false, configurable: true, writable: true, value: null });
    Object.defineProperty(Registration.prototype, "_implementationType", { enumerable: false, configurable: true, writable: true, value: null });
    Object.defineProperty(Registration.prototype, "_instance", { enumerable: false, configurable: true, writable: true, value: null });
    Object.defineProperty(Registration.prototype, "_scope", { enumerable: false, configurable: true, writable: true, value: null });
    Object.defineProperty(Registration.prototype, "_name", { enumerable: false, configurable: true, writable: true, value: null });
    Object.defineProperty(Registration.prototype, "_dependencies", { enumerable: false, configurable: true, writable: true, value: null });
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
    Object.defineProperty(RegistrationsCollection.prototype, "_owner", { enumerable: false, configurable: true, writable: true, value: null });
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

    var arrayIndicators = ["arrayOf", "collectionOf", "enumerationOf"];

    var Container = namespace.Container = function () {
        this._registrations = new RegistrationsCollection(this);
    };
    Object.defineProperty(Container.prototype, "_registrations", { enumerable: false, configurable: true, writable: true, value: null });
    Container.prototype.register = function(registration) {
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
    var _Container = {};
    _Container.resolveInternal = function(registration, dependencyStack) {
        if (dependencyStack.indexOf(registration) !== -1) {
            throw new InvalidOperationException(String.format("Dependency loop detected for type '{0}'.", registration._implementationType));
        }

        dependencyStack.push(registration);
        var args = _Container.resolveArguments.call(this, registration, dependencyStack);
        return _Container.resolveInstance.call(this, registration, args);
    };
    _Container.resolveArguments = function(registration, dependencyStack) {
        var args = [];
        for (var index = 0; index < registration._dependencies.length; index++) {
            var dependency = registration._dependencies[index];
            var isArray = false;
            for (var indicatorIndex = 0; indicatorIndex < arrayIndicators.length; indicatorIndex++) {
                var arrayIndicator = arrayIndicators[indicatorIndex];
                if (dependency.indexOf(arrayIndicator) === 0) {
                    isArray = true;
                    dependency = dependency.substring(arrayIndicator.length);
                }
            }

            var argumentInstance = (isArray ? [] : null);
            var argumentRegistration = _Container.findType.call(this, dependency);
            if (argumentRegistration !== null) {
                if (isArray) {
                    argumentInstance.push(_Container.resolveInternal.call(this, argumentRegistration, dependencyStack));
                }
                else {
                    argumentInstance = _Container.resolveInternal.call(this, argumentRegistration, dependencyStack);
                }
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
    _Container.findType = function(dependency) {
        dependency = dependency.toLowerCase();
        for (var index = 0; index < this._registrations.length; index++) {
            var registration = this._registrations[index];
            var typeNames = [registration._name];
            if (registration._name !== registration._implementationType.toString()) {
                typeNames.push(registration._implementationType.toString());
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
    };
}(namespace("ursa")));