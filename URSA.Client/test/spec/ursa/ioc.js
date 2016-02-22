/// <reference path="/scripts/_references.js"/>
//ReSharperReporter.prototype.jasmineDone = function() { };
/*globals ursa */
(function() {
    "use strict";

    var ServiceType = window.ServiceType = function() { };
    ServiceType.toString = function() { return "ursa.ServiceType"; };
    var ImplementationType = window.ImplementationType = function() { };
    ImplementationType[":"](ServiceType);
    ImplementationType.toString = function() { return "ursa.ImplementationType"; };
    var AnotherImplementationType = window.AnotherImplementationType = function() { };
    AnotherImplementationType[":"](ServiceType);
    AnotherImplementationType.toString = function() { return "ursa.AnotherImplementationType"; };
    var SomeType = function (implementationType) { this.implementationType = implementationType; };
    SomeType.prototype.implementationType = null;
    SomeType.toString = function() { return "ursa.SomeType"; };
    var SomeOtherType = function(someType) { this.someType = someType; };
    SomeOtherType.prototype.someType = null;
    SomeOtherType.toString = function() { return "ursa.SomeOtherType"; };
    var YetAnotherType = function(arrayOfServiceType) { this.serviceTypes = arrayOfServiceType; };
    YetAnotherType.prototype.serviceTypes = null;
    YetAnotherType.toString = function() { return "ursa.YetAnotherType"; };

    describe("Component registration", function() {
        var registration;
        beforeEach(function() {
            jasmine.addMatchers(matchers);
            registration = ursa.Component.for(ServiceType).implementedBy(ImplementationType);
        });

        describe("when creating a registration", function() {
            it("should be created correctly", function () {
                expect(registration._serviceType).toBe(ServiceType);
                expect(registration._implementationTypes.length).toBe(1);
                expect(registration._implementationTypes[0]).toBe(ImplementationType);
                expect(registration._name).toBe(ImplementationType.toString());
                expect(registration._scope).toBe(ursa.Scope.Transient);
            });
            it("should use a given name", function() {
                var expectedName = "test";
                registration.named(expectedName);

                expect(registration._name).toBe(expectedName);
            });
            it("should use a given lifestyle", function() {
                registration.lifestyleSingleton();

                expect(registration._scope).toBe(ursa.Scope.Singleton);
            });
        });
    });

    describe("Given instance of the RegistrationsCollection", function() {
        var collection;
        beforeEach(function() {
            jasmine.addMatchers(matchers);
            collection = new ursa.RegistrationsCollection(null);
            collection.push(new ursa.Registration(ServiceType).implementedBy(ImplementationType));
        });
        it("it should contain a named registration", function() {
            expect(collection.indexOf(ImplementationType.toString())).not.toBe(-1);
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
                container.register(ursa.Component.for(ServiceType).implementedBy(ImplementationType).lifestyleSingleton());
                container.register(ursa.Component.for(ServiceType).implementedBy(AnotherImplementationType));
                container.register(ursa.Component.for(SomeType).implementedBy(SomeType));
                container.register(ursa.Component.for(SomeOtherType).implementedBy(SomeOtherType));
                container.register(ursa.Component.for(YetAnotherType).implementedBy(YetAnotherType));
            });
            it("it should resolve an instance with dependencies", function() {
                var instance = container.resolve(SomeType);

                expect(instance.implementationType).not.toBe(null);
                expect(instance.implementationType).toBeOfType(ImplementationType);
            });
            it("it should resolve an instance with nested dependencies", function() {
                var instance = container.resolve(SomeOtherType);

                expect(instance.someType).not.toBe(null);
                expect(instance.someType).toBeOfType(SomeType);
                expect(instance.someType.implementationType).not.toBe(null);
                expect(instance.someType.implementationType).toBeOfType(ImplementationType);
            });
            it("it should resolve an instance with array of dependencies", function() {
                var instance = container.resolve(YetAnotherType);

                expect(instance.serviceTypes).not.toBe(null);
                expect(instance.serviceTypes).toBeOfType(Array);
                expect(instance.serviceTypes.length).toBe(2);
                expect(instance.serviceTypes[0]).toBeOfType(ImplementationType);
                expect(instance.serviceTypes[1]).toBeOfType(AnotherImplementationType);
            });
            it("it should resolve same instances for singletons", function() {
                var firstCallResult = container.resolve(ServiceType);
                var secondCallResult = container.resolve(ServiceType);

                expect(firstCallResult).toBe(secondCallResult);
            });
            it("it should resolve different instances for transient scope", function() {
                var firstCallResult = container.resolve(SomeType);
                var secondCallResult = container.resolve(SomeType);

                expect(firstCallResult).not.toBe(secondCallResult);
            });
        });

        describe("when registering services by convention", function() {
            beforeEach(function() {
                container = new ursa.Container();
                container.register(ursa.Classes.implementing(ServiceType));
            });

            it("it should have correct types registered", function() {
                expect(container._registrations.length).toBe(2);
            });
        });
    });
}());