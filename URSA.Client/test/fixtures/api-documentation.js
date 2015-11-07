(function () {
    "use strict";
    var getById = function(id) {
        for (var index = 0; index < this.length; index++) {
            if (this[index]["@id"] === id) {
                return this[index];
            }
        }

        return null;
    };

    var listRdfTemplatedLink = { "@id": "http://temp.uri/api/list-rdf#GETwithTake", "@type": [hydra.TemplatedLink] };

    var takeMapping = { "@id": "_:takeMapping", "@type": [hydra.IriTemplateMapping] };
    takeMapping[hydra.variable] = [{ "@value": "take" }];
    takeMapping[hydra.property] = [{ "@id": ursa + "take" }];

    var skipMapping = { "@id": "_:skipMapping", "@type": [hydra.IriTemplateMapping] };
    skipMapping[hydra.variable] = [{ "@value": "skip" }];
    skipMapping[hydra.property] = [{ "@id": ursa + "skip" }];

    var listRdfIriTemplate = { "@id": "_:listRdfIriTemplate", "@type": [hydra.IriTemplate] };
    listRdfIriTemplate[hydra.template] = [{ "@value": "/api/list-rdf{?take,skip}" }];
    listRdfIriTemplate[hydra.mapping] = [{ "@id": takeMapping["@id"] }, { "@id": skipMapping["@id"] }];

    var personClass = { "@id": "http://temp.uri/vocab#Person", "@type": [hydra.Class] };
    var emailSupportedProperty = { "@id": "urn:eMail", "@type": [hydra.SupportedProperty] };
    var firstNameSupportedProperty = { "@id": "urn:firstName", "@type": [hydra.SupportedProperty] };
    var ageSupportedProperty = { "@id": "urn:age", "@type": [hydra.SupportedProperty] };
    var rolesSupportedProperty = { "@id": "urn:roles", "@type": [hydra.SupportedProperty] };
    var getRdfOperation = { "@id": "http://temp.uri/api/get-rdf", "@type": [hydra.Operation] };
    var listRdfOperation = { "@id": "http://temp.uri/api/list-rdf", "@type": [hydra.Operation] };
    var firstNameRestriction = { "@id": "_:firstNameRestriction", "@type": [owl.Restriction] };
    var ageRestriction = { "@id": "_:ageRestriction", "@type": [owl.Restriction] };
    var emailRestriction = { "@id": "_:emailRestriction", "@type": [owl.Restriction] };
    personClass[rdfs.subClassOf] = [{ "@id": firstNameRestriction["@id"] }, { "@id": ageRestriction["@id"] }, { "@id": emailRestriction["@id"] }];
    personClass[hydra.supportedProperty] = [{ "@id": firstNameSupportedProperty["@id"] }, { "@id": ageSupportedProperty["@id"] }, { "@id": rolesSupportedProperty["@id"] }, { "@id": emailSupportedProperty["@id"] }];
    personClass[hydra.supportedOperation] = [{ "@id": getRdfOperation["@id"] }];
    personClass[listRdfTemplatedLink["@id"]] = [{ "@id": listRdfIriTemplate["@id"] }];

    var emailProperty = { "@id": "http://temp.uri/vocab#email", "@type": [owl.InverseFunctionalProperty] };
    emailProperty[rdfs.range] = [{ "@id": xsd.string }];
    emailProperty[rdfs.label] = [{ "@value": "e-Mail" }];

    var firstNameProperty = { "@id": "http://temp.uri/vocab#firstName", "@type": [rdf.Property] };
    firstNameProperty[rdfs.range] = [{ "@id": xsd.string }];
    firstNameProperty[rdfs.label] = [{ "@value": "First name" }];

    var ageProperty = { "@id": "http://temp.uri/vocab#age", "@type": [rdf.Property] };
    ageProperty[rdfs.range] = [{ "@id": xsd.int }];
    ageProperty[rdfs.label] = [{ "@value": "Age" }];

    var rolesProperty = { "@id": "http://temp.uri/vocab#role", "@type": [rdf.Property] };
    rolesProperty[rdfs.range] = [{ "@id": xsd.string }];
    rolesProperty[rdfs.label] = [{ "@value": "Roles" }];

    var xsdString = { "@id": xsd.string, "@type": [hydra.Class] };
    var xsdInt = { "@id": xsd.int, "@type": [hydra.Class] };

    emailSupportedProperty[hydra.property] = [{ "@id": emailProperty["@id"] }];
    emailSupportedProperty[hydra.required] = [{ "@value": true }];
    emailSupportedProperty[hydra.readable] = [{ "@value": true }];
    emailSupportedProperty[hydra.writeable] = [{ "@value": true }];

    firstNameSupportedProperty[hydra.property] = [{ "@id": firstNameProperty["@id"] }];
    firstNameSupportedProperty[hydra.required] = [{ "@value": true }];
    firstNameSupportedProperty[hydra.readable] = [{ "@value": true }];
    firstNameSupportedProperty[hydra.writeable] = [{ "@value": true }];

    ageSupportedProperty[hydra.property] = [{ "@id": ageProperty["@id"] }];
    ageSupportedProperty[hydra.required] = [{ "@value": false }];
    ageSupportedProperty[hydra.readable] = [{ "@value": true }];
    ageSupportedProperty[hydra.writeable] = [{ "@value": true }];

    rolesSupportedProperty[hydra.property] = [{ "@id": rolesProperty["@id"] }];
    rolesSupportedProperty[hydra.required] = [{ "@value": false }];
    rolesSupportedProperty[hydra.readable] = [{ "@value": true }];
    rolesSupportedProperty[hydra.writeable] = [{ "@value": true }];

    var personSubClass = { "@id": "_:personSubClass", "@type": [hydra.Class] };
    personSubClass[rdfs.comment] = [{ "@value": "Person of given key." }];
    personSubClass[rdfs.subClassOf] = [{ "@id": personClass["@id"] }];

    getRdfOperation[ursa.mediaType] = [{ "@value": "application/ld+json" }];
    getRdfOperation[hydra.returns] = [{ "@id": personSubClass["@id"] }];
    getRdfOperation[hydra.expects] = [{ "@id": personClass["@id"] }];
    getRdfOperation[hydra.method] = [{ "@value": "GET" }];

    listRdfTemplatedLink[hydra.supportedOperation] = [{ "@id": listRdfOperation["@id"] }];
    listRdfOperation[ursa.mediaType] = [{ "@value": "application/ld+json" }];
    listRdfOperation[hydra.returns] = [{ "@id": personClass["@id"] }];
    listRdfOperation[hydra.expects] = [{ "@id": personClass["@id"] }];
    listRdfOperation[hydra.method] = [{ "@value": "GET" }];

    emailRestriction[owl.onProperty] = [{ "@id": emailProperty["@id"] }];
    emailRestriction[owl.maxCardinality] = [{ "@value": 1 }];
    emailRestriction[owl.minCardinality] = [{ "@value": 1 }];

    firstNameRestriction[owl.onProperty] = [{ "@id": firstNameProperty["@id"] }];
    firstNameRestriction[owl.maxCardinality] = [{ "@value": 1 }];
    firstNameRestriction[owl.minCardinality] = [{ "@value": 1 }];

    ageRestriction[owl.onProperty] = [{ "@id": ageProperty["@id"] }];
    ageRestriction[owl.maxCardinality] = [{ "@value": 1 }];
    ageRestriction[owl.minCardinality] = [{ "@value": 0 }];

    var apiDocumentation = { "@id": "http://temp.uri/api", "@type": [hydra.ApiDocumentation] };
    apiDocumentation[hydra.supportedClass] = [{ "@id": personClass["@id"] }];
    window.apiDocumentation = [];
    window.apiDocumentation.push(window.apiDocumentation.apiDocumentation = apiDocumentation);
    window.apiDocumentation.push(window.apiDocumentation.personClass = personClass);
    window.apiDocumentation.push(window.apiDocumentation.personSubClass = personSubClass);
    window.apiDocumentation.push(window.apiDocumentation.firstNameRestriction = firstNameRestriction);
    window.apiDocumentation.push(window.apiDocumentation.firstNameSupportedProperty = firstNameSupportedProperty);
    window.apiDocumentation.push(window.apiDocumentation.firstNameProperty = firstNameProperty);
    window.apiDocumentation.push(window.apiDocumentation.ageRestriction = ageRestriction);
    window.apiDocumentation.push(window.apiDocumentation.ageSupportedProperty = ageSupportedProperty);
    window.apiDocumentation.push(window.apiDocumentation.ageProperty = ageProperty);
    window.apiDocumentation.push(window.apiDocumentation.rolesSupportedProperty = rolesSupportedProperty);
    window.apiDocumentation.push(window.apiDocumentation.rolesProperty = rolesProperty);
    window.apiDocumentation.push(window.apiDocumentation.emailRestriction = emailRestriction);
    window.apiDocumentation.push(window.apiDocumentation.emailSupportedProperty = emailSupportedProperty);
    window.apiDocumentation.push(window.apiDocumentation.emailProperty = emailProperty);
    window.apiDocumentation.push(window.apiDocumentation.getRdfOperation = getRdfOperation);
    window.apiDocumentation.push(window.apiDocumentation.listRdfOperation = listRdfOperation);
    window.apiDocumentation.push(window.apiDocumentation.takeMapping = takeMapping);
    window.apiDocumentation.push(window.apiDocumentation.skipMapping = skipMapping);
    window.apiDocumentation.push(window.apiDocumentation.listRdfIriTemplate = listRdfIriTemplate);
    window.apiDocumentation.push(window.apiDocumentation.listRdfTemplatedLink = listRdfTemplatedLink);
    window.apiDocumentation.push(window.apiDocumentation.xsdString = xsdString);
    window.apiDocumentation.push(window.apiDocumentation.xsdInt = xsdInt);
    window.apiDocumentation.getById = function(id) { return getById.call(window.apiDocumentation, id); };
}());