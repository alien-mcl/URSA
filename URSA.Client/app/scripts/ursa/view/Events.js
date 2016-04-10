/*globals namespace */
(function(namespace) {
    "use strict";

    /**
     * Enumeration of URSA events.
     * @public
     * @readonly 
     * @enum {string}
     */
    namespace.Events = {
        /*Notifies that the authentication is required.*/
        AuthenticationRequired: "AuthenticationRequired",

        /*Notifies that the authentication failed.*/
        AuthenticationFailed: "AuthenticationFailed",

        /*Notifies that the authentication should occur.*/
        Authenticate: "Authenticate",

        /*Notifies that the authentication was successful.*/
        Authenticated: "Authenticated",

        /*Notifies that an entity has been created. */
        EntityCreated: "EntityCreated",

        /*Notifies that an entity has been removed. */
        EntityRemoved: "EntityRemoved",

        /*Notifies that an entity has been modified. */
        EntityModified: "EntityModified",

        /*Notifies that an entity has been loaded. */
        EntityLoaded: "EntityLoaded"
    };
}(namespace("ursa.view")));