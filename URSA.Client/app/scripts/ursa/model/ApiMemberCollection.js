/*globals namespace */
(function (namespace) {
    "use strict";

    /**
     * Collection of {@link ursa.model.ApiMember} instances.
     * @memberof ursa.model
     * @name ApiMemberCollection
     * @public
     * @extends {Array}
     * @class
     * @param {ursa.model.ApiMember} owner Owner if this instance being created.
     */
    var ApiMemberCollection = namespace.ApiMemberCollection = function(owner) {
        this._owner = owner || null;
    };
    ApiMemberCollection.prototype = [];
    ApiMemberCollection.prototype._owner = null;

    /**
     * Searches the collection for a member with a given id.
     * @memberof ursa.model.ApiMemberCollection
     * @instance
     * @public
     * @method getById
     * @param {string} id Id of the member to be found.
     * @returns {ursa.model.ApiMember} Instance of the {@link ursa.model.ApiMember} with given id if found; otherwise null.
     */
    ApiMemberCollection.prototype.getById = function(id) { return this.getByProperty(id, "id"); };

    /**
     * Searches the collection for a member with a given property value.
     * @memberof ursa.model.ApiMemberCollection
     * @instance
     * @public
     * @method getByProperty
     * @param {string} value Value of the member's property to be found.
     * @param {string} [property] Property to be compared to of the member to be found. If not value is provided a default of 'id' is used.
     * @returns {ursa.model.ApiMember} Instance of the {@link ursa.model.ApiMember} with given id if found; otherwise null.
     */
    ApiMemberCollection.prototype.getByProperty = function(value, property) {
        if ((property === undefined) || (property === null) || (typeof(property) !== "string")) {
            property = "id";
        }

        if ((value === undefined) || (value === null) || (typeof(property) !== "string")) {
            return null;
        }

        for (var index = 0; index < this.length; index++) {
            if (this[index][property] === value) {
                return this[index];
            }
        }

        return null;
    };

    ApiMemberCollection.toString = function () { return "ursa.model.ApiMemberCollection"; };
}(namespace("ursa.model")));