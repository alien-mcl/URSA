// Karma configuration
// http://karma-runner.github.io/0.12/config/configuration-file.html
// Generated on 2015-10-10 using
// generator-karma 1.0.0

module.exports = function(config) {
    "use strict";

    config.set({
        // enable / disable watching file and executing tests whenever any file changes
        autoWatch: true,

        // base path, that will be used to resolve files and exclude
        basePath: "../",

        // testing framework to use (jasmine/mocha/qunit/...)
        // as well as any additional frameworks (requirejs/chai/sinon/...)
        frameworks: [
            "jasmine"
        ],

        // list of files / patterns to load in the browser
        files: [
            // bower:js
            "bower_components/jquery/dist/jquery.js",
            "bower_components/angular/angular.js",
            "bower_components/bootstrap-sass-official/assets/javascripts/bootstrap.js",
            "bower_components/angular-animate/angular-animate.js",
            "bower_components/angular-cookies/angular-cookies.js",
            "bower_components/angular-messages/angular-messages.js",
            "bower_components/angular-resource/angular-resource.js",
            "bower_components/angular-route/angular-route.js",
            "bower_components/angular-sanitize/angular-sanitize.js",
            "bower_components/angular-touch/angular-touch.js",
            "bower_components/angular-bootstrap/ui-bootstrap-tpls.js",
            "bower_components/es6-promise/promise.js",
            "bower_components/jsonld/js/jsonld.js",
            "bower_components/uri-templates/uri-templates.js",
            "bower_components/joice/dist/joice.min.js",
            "bower_components/angular-mocks/angular-mocks.js",
            // endbower
            "app/scripts/app.js",
            "app/scripts/ursa/namespaces.js",
            "app/scripts/ursa/IPromise.js",
            "app/scripts/ursa/IDeferred.js",
            "app/scripts/ursa/IPromiseProvider.js",
            "app/scripts/ursa/NotSupportedException.js",
            "app/scripts/ursa/Encoder.js",
            "app/scripts/ursa/Base64Encoder.js",
            "app/scripts/ursa/IGraph.js",
            "app/scripts/ursa/web/AuthenticationProvider.js",
            "app/scripts/ursa/web/HttpRequest.js",
            "app/scripts/ursa/web/HttpResponse.js",
            "app/scripts/ursa/web/HttpService.js",
            "app/scripts/ursa/web/angular/AngularHttpService.js",
            "app/scripts/ursa/web/angular/AngularAuthenticationProvider.js",
            "app/scripts/ursa/model/helpers.js",
            "app/scripts/ursa/model/RegExpArray.js",
            "app/scripts/ursa/model/DateTimeRegExp.js",
            "app/scripts/ursa/model/EntityFormat.js",
            "app/scripts/ursa/model/ApiMember.js",
            "app/scripts/ursa/model/ApiMemberCollection.js",
            "app/scripts/ursa/model/SupportedProperty.js",
            "app/scripts/ursa/model/Mapping.js",
            "app/scripts/ursa/model/Operation.js",
            "app/scripts/ursa/model/Type.js",
            "app/scripts/ursa/model/DataType.js",
            "app/scripts/ursa/model/Class.js",
            "app/scripts/ursa/model/Filter.js",
            "app/scripts/ursa/model/FilterProvider.js",
            "app/scripts/ursa/model/FilterExpressionProvider.js",
            "app/scripts/ursa/model/ODataFilterExpressionProvider.js",
            "app/scripts/ursa/model/ApiDocumentation.js",
            "app/scripts/ursa/model/HttpStatusCodes.js",
            "app/scripts/ursa/model/JsonLdProcessor.js",
            "app/scripts/ursa/model/ApiDocumentationProvider.js",
            "app/scripts/ursa/view/helpers.js",
            "app/scripts/ursa/view/IViewScope.js",
            "app/scripts/ursa/view/angular/AngularViewScope.js",
            "app/scripts/ursa/view/Events.js",
            "app/scripts/ursa/view/ViewRenderer.js",
            "app/scripts/ursa/view/ViewRendererProvider.js",
            "app/scripts/ursa/view/DatatypeDescriptor.js",
            "app/scripts/ursa/view/SupportedPropertyRenderer.js",
            "app/scripts/ursa/view/ClassRenderer.js",
            "app/scripts/ursa/view/OperationRenderer.js",
            "app/scripts/ursa/angular/module.js",
            "app/scripts/services/configuration.js",
            "app/scripts/filters/as-id.js",
            "app/scripts/filters/stringify.js",
            "app/scripts/directives/ursa.js",
            "app/scripts/controllers/main.js",
            "test/matchers/toBeOfType.js",
            "test/fixtures/api-documentation.js",
            "test/spec/**/*.js"
        ],

        // list of files / patterns to exclude
        exclude: [
        ],

        preprocessors: {
            "app/scripts/ursa/*.js": "coverage"
        },

        reporters: ["coverage", "teamcity"],

        coverageReporter: {
            dir: "build/reports/coverage",
            reporters: [
                { type: "html", subdir: "html" },
                { type: "teamcity" }
            ]
        },

        // web server port
        port: 8080,

        // Start these browsers, currently available:
        // - Chrome
        // - ChromeCanary
        // - Firefox
        // - Opera
        // - Safari (only Mac)
        // - PhantomJS
        // - IE (only Windows)
        browsers: [
            "PhantomJS"
        ],

        plugins: [
            "karma-teamcity-reporter",
            "karma-coverage",
            "karma-phantomjs-launcher",
            "karma-jasmine"
        ],

        // Continuous Integration mode
        // if true, it capture browsers, run tests and exit
        singleRun: true,
        colors: true,

        // level of logging
        // possible values: LOG_DISABLE || LOG_ERROR || LOG_WARN || LOG_INFO || LOG_DEBUG
        logLevel: config.LOG_INFO

        // Uncomment the following lines if you are using grunt"s server to run the tests
        // proxies: {
        //   "/": "http://localhost:9000/"
        // },
        // URL root prevent conflicts with the site root
        // urlRoot: "_karma_"
    });
};