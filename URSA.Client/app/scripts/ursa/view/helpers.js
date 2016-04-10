/*globals namespace, ursa */
(function(namespace) {
    "use strict";

    var expectedBodyVerbs = ["POST", "PUT"];

    var findEntityCrudOperation = function (method, listing) {
        listing = (typeof (listing) === "boolean" ? listing : false);
        var supportedOperations = (this.apiMember instanceof ursa.model.Class ? this.apiMember : this.apiMember.owner).supportedOperations;
        for (var index = 0; index < supportedOperations.length; index++) {
            var operation = supportedOperations[index];
            if ((operation.methods.indexOf(method) !== -1) &&
                (((!listing) && (((expectedBodyVerbs.indexOf(method) === -1) && (operation.returns.length > 0) && (operation.returns[0].maxOccurances === 1)) ||
                    ((expectedBodyVerbs.indexOf(method) !== -1) && (operation.expects.length > 0) && (operation.expects[0].maxOccurances === 1)) ||
                    ((method === "DELETE") && (operation.expects.length === 0) && (operation.returns.length === 0)))) ||
                ((listing) && (((expectedBodyVerbs.indexOf(method) === -1) && (operation.returns.length > 0) && (operation.returns[0].maxOccurances === Number.MAX_VALUE)) ||
                    ((expectedBodyVerbs.indexOf(method) !== -1) && (operation.expected.length > 0) && (operation.expected[0].maxOccurances === Number.MAX_VALUE)))))) {
                return operation;
            }
        }

        return null;
    };

    Object.defineProperty(namespace, "findEntityCrudOperation", { enumerable: false, configurable: false, writable: true, value: findEntityCrudOperation });
}(namespace("ursa.view")));