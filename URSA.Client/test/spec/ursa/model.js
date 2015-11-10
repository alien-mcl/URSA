/// <reference path="/scripts/_references.js"/>
//ReSharperReporter.prototype.jasmineDone = function() { };
/*globals ursa, rdfs, hydra, owl, matchers */
(function() {
    "use strict";

    var apiDocumentation = new ursa.model.ApiDocumentation(window.apiDocumentation);

    describe("Given instance of the API documentation model's", function() {
        beforeEach(function() {
            jasmine.addMatchers(matchers);
        });

        describe("ursa.model.ApiDocumentation class", function() {
            it("it should initialize an instance correctly", function() {
                expect(apiDocumentation.entryPoints.length).toBe(1);
                expect(apiDocumentation.supportedClasses.length).toBe(1);
            });
        });

        describe("ursa.model.Class class", function() {
            var $class = null;
            var operation = null;
            describe("which is rdfs:Class", function () {
                beforeEach(function() {
                    $class = apiDocumentation.supportedClasses[0];
                    operation = $class.supportedOperations[0];
                });

                it("it should initialize an instance correctly", function() {
                    expect($class.id).toBe(window.apiDocumentation.personClass["@id"]);
                    expect($class.supportedProperties.length).toBe(4);
                    expect($class.supportedProperties[0].id).toBe(window.apiDocumentation.firstNameSupportedProperty["@id"]);
                    expect($class.supportedOperations.length).toBe(2);
                    expect($class.supportedOperations[0].id).toBe(window.apiDocumentation.getRdfOperation["@id"]);
                    expect($class.supportedOperations[1].id).toBe(window.apiDocumentation.listRdfOperation["@id"]);
                });

                it("it should create a class instance correctly", function() {
                    var instance = $class.createInstance(operation);

                    expect(instance).not.toBe(null);
                    expect(instance[$class.supportedProperties[0].property]).toBeOfType(Array);
                    expect(instance[$class.supportedProperties[0].property].length).toBe(1);
                    expect(instance[$class.supportedProperties[0].property][0]).not.toBe(null);
                    expect(instance[$class.supportedProperties[0].property][0]["@value"]).toBe("");
                });
            });
        });

        describe("ursa.model.Operation class", function() {
            var operation = null;
            describe("which is RDF based and no mappings are required", function() {
                beforeEach(function() {
                    operation = apiDocumentation.supportedClasses[0].supportedOperations[0];
                });

                it("it should initialize an instance correctly", function () {
                    expect(operation.isRdf).toBeTruthy();
                    expect(operation.methods.length).toBe(1);
                    expect(operation.methods[0]).toBe("GET");
                    expect(operation.expects.length).toBe(1);
                    expect(operation.expects[0]).toBeOfType(ursa.model.Class);
                    expect(operation.expects[0].id).toBe(window.apiDocumentation.personClass["@id"]);
                    expect(operation.returns.length).toBe(1);
                    expect(operation.returns[0]).toBeOfType(ursa.model.Class);
                    expect(operation.returns[0].id).toBe(window.apiDocumentation.personClass["@id"]);
                    expect(operation.returns[0].description).toBe(window.apiDocumentation.personSubClass[rdfs.comment][0]["@value"]);
                });

                it("it should create a correct call URL", function() {
                    var callUrl = operation.createCallUrl(null);

                    expect(callUrl).toBe(operation.id);
                });
            });

            describe("which is RDF based and with IRI template mappings provided", function() {
                beforeEach(function() {
                    operation = apiDocumentation.supportedClasses[0].supportedOperations[1];
                });

                it("it should initialize an instance correctly", function () {
                    expect(operation.isRdf).toBeTruthy();
                    expect(operation.url).toBe(apiDocumentation.entryPoints[0] + window.apiDocumentation.listRdfIriTemplate[hydra.template][0]["@value"].substr(1));
                    expect(operation.mappings.length).toBe(2);
                    expect(operation.methods[0]).toBe("GET");
                    expect(operation.methods.length).toBe(1);
                    expect(operation.methods[0]).toBe("GET");
                    expect(operation.expects.length).toBe(1);
                    expect(operation.expects[0]).toBeOfType(ursa.model.Class);
                    expect(operation.expects[0].id).toBe(window.apiDocumentation.personClass["@id"]);
                    expect(operation.returns.length).toBe(1);
                    expect(operation.returns[0]).toBeOfType(ursa.model.Class);
                    expect(operation.returns[0].id).toBe(window.apiDocumentation.personClass["@id"]);
                });

                it("it should create a correct call URL", function() {
                    var instance = {};
                    instance[ursa + "take"] = [{ "@value": 10 }];
                    instance[ursa + "skip"] = [{ "@value": 1 }];
                    var callUrl = operation.createCallUrl(instance);

                    expect(callUrl).toBe(operation.url.replace("{?take,skip}", "?take=" + instance[ursa + "take"][0]["@value"] + "&skip=" + instance[ursa + "skip"][0]["@value"]));
                });
            });
        });

        describe("ursa.model.Mapping class", function() {
            var operation = apiDocumentation.supportedClasses[0].supportedOperations[1];
            var mapping = null;
            describe("which is mapped to RDF property", function() {
                beforeEach(function() {
                    mapping = operation.mappings[0];
                });

                it("it should initialize an instance correctly", function () {
                    expect(mapping.variable).toBe(window.apiDocumentation.takeMapping[hydra.variable][0]["@value"]);
                    expect(mapping.property).toBe(window.apiDocumentation.takeMapping[hydra.property][0]["@id"]);
                });

                it("it should provide a property name correctly", function() {
                    expect(mapping.propertyName()).toBe(mapping.property);
                });
            });
        });

        describe("ursa.model.SupportedProperty class", function() {
            var supportedProperty = null;
            describe("which defines a values range", function() {
                beforeEach(function() {
                    supportedProperty = apiDocumentation.supportedClasses[0].supportedProperties[0];
                });

                it("it should initialize an instance correctly", function () {
                    expect(supportedProperty.required).toBe(window.apiDocumentation.firstNameSupportedProperty[hydra.required][0]["@value"]);
                    expect(supportedProperty.readable).toBe(window.apiDocumentation.firstNameSupportedProperty[hydra.readable][0]["@value"]);
                    expect(supportedProperty.writeable).toBe(window.apiDocumentation.firstNameSupportedProperty[hydra.writeable][0]["@value"]);
                    expect(supportedProperty.property).toBe(window.apiDocumentation.firstNameProperty["@id"]);
                    expect(supportedProperty.minOccurances).toBe(window.apiDocumentation.firstNameRestriction[owl.minCardinality][0]["@value"]);
                    expect(supportedProperty.maxOccurances).toBe(window.apiDocumentation.firstNameRestriction[owl.maxCardinality][0]["@value"]);
                });

                it("it should create an empty value instance correctly", function() {
                    expect(supportedProperty.createInstance()).toBe("");
                });
            });

            describe("which defines a dedicated operation", function() {
                beforeEach(function() {
                    supportedProperty = apiDocumentation.supportedClasses[0].supportedProperties[2];
                });

                it("it should initialize an instance correctly", function () {
                    expect(supportedProperty.supportedOperations.length).toBe(1);
                    expect(supportedProperty.supportedOperations[0].id).toBe(window.apiDocumentation.setRolesOperation["@id"]);
                });
            });
        });
    });
}());