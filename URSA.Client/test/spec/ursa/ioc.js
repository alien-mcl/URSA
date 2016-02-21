﻿/// <reference path="/scripts/_references.js"/>
//ReSharperReporter.prototype.jasmineDone = function() { };
/*globals ursa */
(function() {
    "use strict";

    var ServiceType = function() { };
    ServiceType.toString = function() { return "ursa.ServiceType"; };
    var ImplementationType = function() { };
    ImplementationType[":"](ServiceType);
    ImplementationType.toString = function() { return "ursa.ImplementationType"; };
    var AnotherImplementationType = function() { };
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
                expect(registration._implementationType).toBe(ImplementationType);
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
            container = new ursa.Container();
            container.register(ursa.Component.for(ServiceType).implementedBy(ImplementationType));
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
    });
}());