/// <reference path="/scripts/_references.js"/>
//ReSharperReporter.prototype.jasmineDone = function() { };
/*globals ursa */
(function() {
    "use strict";

    var IServiceType = window.IService = function() {
        throw new ursa.InvalidOperationException("Cannot instantiate interface ursa.IServiceType.");
    };
    IServiceType.action = function() {};
    IServiceType.toString = function() { return "ursa.IServiceType"; };

    var Service = window.Service = function() { };
    Service.prototype.action = function() {};
    Service.toString = function() { return "ursa.Service"; };
    var Implementation = window.Implementation = function() { };
    Implementation[":"](Service);
    Implementation.prototype.action = function() {};
    Implementation.toString = function() { return "ursa.Implementation"; };
    var AnotherImplementation = window.AnotherImplementation = function() { };
    AnotherImplementation[":"](Service);
    AnotherImplementation.prototype.action = function() {};
    AnotherImplementation.toString = function() { return "ursa.AnotherImplementation"; };
    var Some = function(implementation) { this.implementation = implementation; };
    Some.prototype.implementation = null;
    Some.toString = function() { return "ursa.Some"; };
    var SomeOther = function(some) { this.some = some; };
    SomeOther.prototype.some = null;
    SomeOther.toString = function() { return "ursa.SomeOther"; };
    var YetAnother = function(arrayOfService) { this.services = arrayOfService; };
    YetAnother.prototype.services = null;
    YetAnother.toString = function() { return "ursa.YetAnother"; };
    var Whatever = function(service) { this.service = service; };
    Whatever.prototype.service = null;
    Whatever.toString = function() { return "ursa.Whatever"; };
    var FactoryRequiring = function(serviceFactory) { this.serviceFactory = serviceFactory; };
    FactoryRequiring.prototype.serviceFactory = null;
    FactoryRequiring.toString = function() { return "ursa.FactoryRequiring"; };

    describe("Component registration", function() {
        var registration;
        beforeEach(function() {
            jasmine.addMatchers(matchers);
            registration = ursa.Component.for(Service).implementedBy(Implementation);
        });

        describe("when creating a registration", function() {
            it("should be created correctly", function () {
                expect(registration.serviceType).toBe(Service);
                expect(registration._implementationType).toBe(Implementation);
                expect(registration._name).toBe(Implementation.toString());
                expect(registration.scope).toBe(ursa.Scope.Transient);
            });
            it("should use a given name", function() {
                var expectedName = "test";
                registration.named(expectedName);

                expect(registration._name).toBe(expectedName);
            });
            it("should use a given lifestyle", function() {
                registration.lifestyleSingleton();

                expect(registration.scope).toBe(ursa.Scope.Singleton);
            });
        });
    });

    describe("Given instance of the RegistrationsCollection", function() {
        var collection;
        beforeEach(function() {
            jasmine.addMatchers(matchers);
            collection = new ursa.RegistrationsCollection(null);
            collection.push(new ursa.Registration(Service, Implementation));
        });
        it("it should contain a named registration", function() {
            expect(collection.indexOf(Implementation.toString())).not.toBe(-1);
        });
    });

    describe("Given instance of the Container", function() {
        var container;
        beforeEach(function() {
            jasmine.addMatchers(matchers);
        });

        describe("when registering services explicitely", function() {
            beforeEach(function() {
                container = new ursa.Container();
                container.register(ursa.Component.for(Service).implementedBy(Implementation).lifestyleSingleton());
                container.register(ursa.Component.for(Service).implementedBy(AnotherImplementation));
                container.register(ursa.Component.for(Some).implementedBy(Some));
                container.register(ursa.Component.for(SomeOther).implementedBy(SomeOther));
                container.register(ursa.Component.for(YetAnother).implementedBy(YetAnother));
                container.register(ursa.Component.for(FactoryRequiring).implementedBy(FactoryRequiring));
            });
            describe("and resolving a factory", function() {
                var factory;
                beforeEach(function() {
                    factory = container.resolve(FactoryRequiring);
                });

                it("it should resolve that factory correctly", function() {
                    expect(factory).not.toBe(null);
                    expect(factory.serviceFactory).not.toBe(null);
                });
                it("it should implement that factory to resolve single instance correctly", function() {
                    var result = factory.serviceFactory.resolve();

                    expect(result).toBeOfType(Implementation);
                });
                it("it should implement that factory to resolve all instances correctly", function() {
                    var result = factory.serviceFactory.resolveAll();

                    expect(result).toBeOfType(Array);
                    expect(result.length).toBe(2);
                    expect(result[0]).toBeOfType(Implementation);
                    expect(result[1]).toBeOfType(AnotherImplementation);
                });
            });
            it("it should resolve an instance with dependencies", function() {
                var instance = container.resolve(Some);

                expect(instance.implementation).not.toBe(null);
                expect(instance.implementation).toBeOfType(Implementation);
            });
            it("it should resolve an instance with nested dependencies", function() {
                var instance = container.resolve(SomeOther);

                expect(instance.some).not.toBe(null);
                expect(instance.some).toBeOfType(Some);
                expect(instance.some.implementation).not.toBe(null);
                expect(instance.some.implementation).toBeOfType(Implementation);
            });
            it("it should resolve an instance with array of dependencies", function() {
                var instance = container.resolve(YetAnother);

                expect(instance.services).not.toBe(null);
                expect(instance.services).toBeOfType(Array);
                expect(instance.services.length).toBe(2);
                expect(instance.services[0]).toBeOfType(Implementation);
                expect(instance.services[1]).toBeOfType(AnotherImplementation);
            });
            it("it should resolve same instances for singletons", function() {
                var firstCallResult = container.resolve(Service);
                var secondCallResult = container.resolve(Service);

                expect(firstCallResult).toBe(secondCallResult);
            });
            it("it should resolve different instances for transient scope", function() {
                var firstCallResult = container.resolve(Some);
                var secondCallResult = container.resolve(Some);

                expect(firstCallResult).not.toBe(secondCallResult);
            });
            it("it should resolve service type", function() {
                var result = container.resolveType(Some);

                expect(result).not.toBe(Implementation);
            });
        });

        describe("when registering services by convention", function() {
            beforeEach(function() {
                container = new ursa.Container();
                container.register(ursa.Classes.implementing(Service));
            });

            it("it should have correct types registered", function() {
                expect(container._registrations.length).toBe(2);
            });
        });

        describe("when registering service by factory method", function() {
            var calls = 0;
            var instance = null;
            beforeEach(function() {
                container = new ursa.Container();
                container.register(ursa.Component.for(Service).usingFactoryMethod(function() {
                    calls++;
                    return instance = {};
                }).named("implementationType"));
                container.register(ursa.Component.for(Some).implementedBy(Some));
            });

            it("it should resolve instance correctly", function() {
                var resolved = container.resolve(Some);

                expect(calls).toBe(1);
                expect(resolved.implementation).toBe(instance);
            });
        });

        describe("when registering interface service implementation", function() {
            beforeEach(function() {
                container = new ursa.Container();
                container.register(ursa.Component.for(IServiceType).implementedBy(Implementation));
                container.register(ursa.Component.for(Whatever).implementedBy(Whatever));
            });

            it("it should resolve instance correctly", function() {
                var resolved = container.resolve(Whatever);

                expect(resolved.service).toBeOfType(Implementation);
            });
        });
    });
}());