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

    var listRdfTemplatedLink = { "@id": "http://temp.uri/api/list-rdf#GETwith$take", "@type": [hydra.TemplatedLink] };
    var setRolesTemplatedLink = { "@id": "http://temp.uri/api/person/{id}/roles#SETwithId", "@type": [hydra.TemplatedLink] };

    var topMapping = { "@id": "_:takeMapping", "@type": [hydra.IriTemplateMapping] };
    topMapping[hydra.variable] = [{ "@value": "$top" }];
    topMapping[hydra.property] = [{ "@id": odata.top }];

    var skipMapping = { "@id": "_:skipMapping", "@type": [hydra.IriTemplateMapping] };
    skipMapping[hydra.variable] = [{ "@value": "$skip" }];
    skipMapping[hydra.property] = [{ "@id": odata.skip }];

    var idMapping = { "@id": "_:idMapping", "@type": [hydra.IriTemplateMapping] };
    idMapping[hydra.variable] = [{ "@value": "id" }];

    var listRdfIriTemplate = { "@id": "_:listRdfIriTemplate", "@type": [hydra.IriTemplate] };
    listRdfIriTemplate[hydra.template] = [{ "@value": "/api/list-rdf{?$top,$skip}" }];
    listRdfIriTemplate[hydra.mapping] = [{ "@id": topMapping["@id"] }, { "@id": skipMapping["@id"] }];

    var setRolesIriTemplate = { "@id": "_:setRolesIriTemplate", "@type": [hydra.IriTemplate] };
    setRolesIriTemplate[hydra.template] = [{ "@value": "/api/person/{id}/roles" }];
    setRolesIriTemplate[hydra.mapping] = [{ "@id": idMapping["@id"] }];

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

    var stringsCollectionRestriction = { "@id": "_:stringsCollectionRestriction", "@type": [owl.Restriction] };
    stringsCollectionRestriction[owl.onProperty] = [{ "@id": hydra.member }];
    stringsCollectionRestriction[owl.allValuesFrom] = [{ "@id": xsd.string }];

    var stringsCollection = { "@id": "javascript:collectionOfString", "@type": [hydra.Class] };
    stringsCollection[rdfs.subClassOf] = [{ "@id": hydra.Collection }, { "@id": stringsCollectionRestriction["@id"] }];

    var rolesProperty = { "@id": "http://temp.uri/vocab#role", "@type": [rdf.Property] };
    rolesProperty[rdfs.range] = [{ "@id": stringsCollection["@id"] }];
    rolesProperty[rdfs.label] = [{ "@value": "Roles" }];
    idMapping[hydra.property] = [{ "@id": rolesProperty["@id"] }];

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

    var setRolesOperation = { "@id": "http://temp.uri/api/person/id/roles", "@type": [hydra.Operation] };
    setRolesOperation[ursa.mediaType] = [{ "@value": "application/ld+json" }];
    setRolesOperation[hydra.method] = [{ "@value": "POST" }];
    setRolesTemplatedLink[hydra.supportedOperation] = [{ "@id": setRolesOperation["@id"] }];

    rolesSupportedProperty[hydra.property] = [{ "@id": rolesProperty["@id"] }];
    rolesSupportedProperty[hydra.required] = [{ "@value": false }];
    rolesSupportedProperty[hydra.readable] = [{ "@value": true }];
    rolesSupportedProperty[hydra.writeable] = [{ "@value": true }];
    rolesSupportedProperty[setRolesTemplatedLink["@id"]] = [{ "@id": setRolesIriTemplate["@id"] }];

    var personSubClass = { "@id": "_:personSubClass", "@type": [hydra.Class] };
    personSubClass[rdfs.comment] = [{ "@value": "Person of given key." }];
    personSubClass[rdfs.subClassOf] = [{ "@id": personClass["@id"] }];

    getRdfOperation[ursa.mediaType] = [{ "@value": "application/ld+json" }];
    getRdfOperation[hydra.returns] = [{ "@id": personSubClass["@id"] }];
    getRdfOperation[hydra.expects] = [{ "@id": personClass["@id"] }];
    getRdfOperation[hydra.method] = [{ "@value": "GET" }];

    var personCollectionRestriction = { "@id": "_:personCollectionRestriction", "@type": [owl.Restriction] };
    personCollectionRestriction[owl.onProperty] = [{ "@id": hydra.member }];
    personCollectionRestriction[owl.allValuesFrom] = [{ "@id": personClass["@id"] }];

    var personListRestriction = { "@id": "_:personListRestriction", "@type": [owl.Restriction] };
    personListRestriction[owl.onProperty] = [{ "@id": rdf.first }];
    personListRestriction[owl.allValuesFrom] = [{ "@id": personClass["@id"] }];

    var personCollection = { "@id": "javascript:collectionOfPerson", "@type": [hydra.Class] };
    personCollection[rdfs.subClassOf] = [{ "@id": hydra.Collection }, { "@id": rdf.List }, { "@id": personCollectionRestriction["@id"] }, { "@id": personListRestriction["@id"] }];

    listRdfTemplatedLink[hydra.supportedOperation] = [{ "@id": listRdfOperation["@id"] }];
    listRdfOperation[ursa.mediaType] = [{ "@value": "application/ld+json" }];
    listRdfOperation[hydra.returns] = [{ "@id": personCollection["@id"] }];
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
    window.apiDocumentation.push(window.apiDocumentation.personCollectionRestriction = personCollectionRestriction);
    window.apiDocumentation.push(window.apiDocumentation.personListRestriction = personListRestriction);
    window.apiDocumentation.push(window.apiDocumentation.listRdfOperation = listRdfOperation);
    window.apiDocumentation.push(window.apiDocumentation.personCollection = personCollection);
    window.apiDocumentation.push(window.apiDocumentation.topMapping = topMapping);
    window.apiDocumentation.push(window.apiDocumentation.skipMapping = skipMapping);
    window.apiDocumentation.push(window.apiDocumentation.listRdfIriTemplate = listRdfIriTemplate);
    window.apiDocumentation.push(window.apiDocumentation.listRdfTemplatedLink = listRdfTemplatedLink);
    window.apiDocumentation.push(window.apiDocumentation.idMapping = idMapping);
    window.apiDocumentation.push(window.apiDocumentation.setRolesIriTemplate = setRolesIriTemplate);
    window.apiDocumentation.push(window.apiDocumentation.setRolesTemplatedLink = setRolesTemplatedLink);
    window.apiDocumentation.push(window.apiDocumentation.setRolesOperation = setRolesOperation);
    window.apiDocumentation.push(window.apiDocumentation.stringsCollectionRestriction = stringsCollectionRestriction);
    window.apiDocumentation.push(window.apiDocumentation.stringsCollection = stringsCollection);
    window.apiDocumentation.push(window.apiDocumentation.xsdString = xsdString);
    window.apiDocumentation.push(window.apiDocumentation.xsdInt = xsdInt);
    window.apiDocumentation.push(window.apiDocumentation.rdfList = { "@id": rdf.List });
    window.apiDocumentation.push(window.apiDocumentation.hydraCollection = { "@id": hydra.Collection });
    window.apiDocumentation.getById = function (id) { return getById.call(window.apiDocumentation, id); };
}());