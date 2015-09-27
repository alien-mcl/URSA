---
layout: default
---
URSA is designed to do as much as possible automatically. While there is always an option of explicitely defining the behaviour, in most of the cases you'd like to stick to the defaults.

### Building API
Build an API is as easy as 01, 10, 11. Define a class implementing any of the interfaces listed below and this should make your class up and running:

* URSA.Web.IController - the simplest and with least support for ReST behaviour.
* URSA.Web.IController&lt;T&gt; - basic entity of type *T* based controller with list of items.
* URSA.Web.IReadController&lt;T, I&gt; - basic entity of type *T* based controller with access to single item.
* URSA.Web.IWriteController&lt;T, I&gt; - basic entity of type *T* based controller with identifier of type *I* and abilities to create, update and delete an item.

Aim is to have as least of the interference in the ordinary code as possible, thus interfaces are used instead of a class.

The framework uses several naming conventions, thus you can have an influence on how the URLs and HTTP methods are assigned.
In general, mapping of HTTP methods to class methods is as follows (method should contain in it's name these words):

* Query, All, List - GET
* Create, Build, Make, Set - POST
* Update - PUT
* Remove, Teardown - DELETE

Additionally, methods containing HTTP method names (Get, Put, Post, Delete) are mapped to their HTTP equivalents.

As for URLs generated, these are the basic rules:

* if not overriden with *URSA.Web.Mapping.RouteAttribute*, controller class name with 'Controller' word stripped of is taken as a first URL segment
* unless overriden with *URSA.Web.Mapping.OnVerbAttribute* (or derivative) methods are mapped to HTTP verbs according to the map descibed above
* arguments being *out* or *ref* are returned either in the response body (multipart/mixed if necessary) or into a *Location* HTTP headre when a value is of type matching *T* in *URSA.Web.IReadController&lt;T, I&gt;*
* unless overriden with *URSA.Web.Mapping.FromQueryStringAttribute* or *URSA.Web.Mapping.FromBodyAttribute* arguments of type *int* and *long* named *id*, *identifier*, *identity* or *key* are mapped to URL by their name
* unless overriden with *URSA.Web.Mapping.FromQueryStringAttribute* or *URSA.Web.Mapping.FromBodyAttribute* arguments of type *System.Guid* and *System.DateTime* are mapped to URL by their name
* unless overriden with *URSA.Web.Mapping.FromUriAttribute* or *URSA.Web.Mapping.FromBodyAttribute* arguments being *System.Collections.Generic.IEnumerable&lt;T&gt;* of numeric types are mapped to query string by their name
* unless overriden with *URSA.Web.Mapping.FromUriAttribute* or *URSA.Web.Mapping.FromBodyAttribute* arguments being *string* are mapped to query string by their name
* unless overriden with *URSA.Web.Mapping.FromQueryStringAttribute* or *URSA.Web.Mapping.FromUriAttribute* arguments of all other types are mapped to request body
* methods accepting an argument matching a target type *T* of the *URSA.Web.IController&lt;T&gt;* are mapped as sub-hierarchy

To visualize, below are few examples of how the methods are mapped to URLs:

* URSA.Web.Test.PersonController.Get(int id) - GET /person/id/{?id}
* URSA.Web.Test.PersonController.List(int skip = 0, int take = 0) - GET /person?skip={?skip}&take={?take}
* URSA.Web.Test.PersonController.SetRoles(int id, IEnumerable<string> roles) - POST /person/id/{?id}/roles
* URSA.Web.Test.TestController.Add(int operandA = 0, int operandB = 0) - GET /test/add?operandA={?operandA}&operandB={?operandB}

### Converters
URSA comes with several convertes transforming incomming parameters and request bodies into strongly typed values. These includes:

* binary converter - converts *application/octet-stream* media types into *byte[]*
* boolean converter - converts *text/plain* media types into *bool* or *System.Collections.Generic.IEnumerable&lt;bool&gt;*
* number converter - converts *text/plain* media types into numeric values or *System.Collections.Generic.IEnumerable&lt;&gt;* of them
* string converter - converts *text/plain* media types into *string* or *System.Collections.Generic.IEnumerable&lt;string&gt;*
* *System.DateTime* converter - converts *text/plain* media types into *System.DateTime* or *System.Collections.Generic.IEnumerable&lt;System.DateTime&gt;*
* *System.TimeSpan* converter - converts *text/plain* media types into *System.TimeSpan* or *System.Collections.Generic.IEnumerable&lt;System.TimeSpan&gt;*
* *System.Guid* converter - converts *text/plain* media types into *System.Guid* or *System.Collections.Generic.IEnumerable&lt;System.Guid&gt;*
* *System.Uri* converter - converts either *text/plain* or *text/uri-list* media types into *System.Uri* or *System.Collections.Generic.IEnumerable&lt;System.Uri&gt;*
* JSON converter - converts *application/json* media types into strongly typed instances
* XML converter - converts either *text/xml* or *application/xml* into strongly typed instances
* RDF converter - converts *text/turtle*, *application/rdf+xml*, *application/owl+xml*, *application/ld+json* media types into strony typed instances

Practically, it would be possible i.e. to implement a converter accepting *image/jpeg* which would provide a strongly typed *System.Graphics.Image* instance out of it.

### API Documentation
In order to obtain a automatically generated HYDRA documentation a proper request to controller's base URL with HTTP OPTIONS and supported HTTP Accept header.
It is also possible to manually invoke a specific RDF serialization by sending to controller's base URL a HTTP GET request with ?format=[XML|RDF|JSONLD|Turtle].

This is still experimental thus it might still not be fully compliant with HYDRA itself.

### Initialization
In order to have everything setup, all you need to do is to add:

{% highlight csharp %}
using URSA.Web;

public class Global : System.Web.HttpApplication
{
	protected void Application_Start(object sender, EventArgs e)
	{
		this.RegisterApis();
	}
}
{% endhighlight %}

or in whatever initialization code you have out there. This will discover all implementations of *URSA.Web.IController* type, 
register in the route tables, assign handlers, automatically generated HTTP OPTIONS handler, etc.