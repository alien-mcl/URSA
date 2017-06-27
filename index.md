---
layout: default
title: Home
weight: 1
---
### Meet the bear - Ultimate ReSt Api.
Welcome to the home site of the URSA project. Here you can find documentation pieces that would allow you to get started with creating your ReSTful APIs easily.

### Why URSA?
I've worked with several ReSTful implementations written in C# and none of them felt ... ReSTful.
There is WCF or Web API, Nancy FX, but I had an impression that the only difference from SOAP was a nice looking Url, JSON instead of an XML and sometimes HTTP POST instead of a HTTP GET.

If you wanted to treat i.e. errors the ReSTful way you had to spend some extra time implementing various extensions.

Finally, all of them have nothing to do with Hypermedia As The Engine Of Application State (HATOAS - is that a shortcut, really?!?).

### URSA to the rescue
I decided to play a little with HYpermedia DRiven Application (HYDRA) Core Vocabulary - a promising solution for documenting ReST APIs
so a client can be provided with hypermedia details telling him on what, when and how it can communicate with the server.

Quickly this tiny project started to grow and became an alternate approach for the mentioned solutions when it comes to ReSTful API written in C#.
While there is still plenty of work it is already a working solution that could be good starting point for other developers.
If you're still interested pieces of documentation can be found <a href="doc.html">here</a>.

### Supported features
* OWIN/IIS connectors
* multiple body (de)serializers
* url/query string parameters
* auto-url generated API
* API self-generated documentation
* angularJS generic client
* RDF and POJsO data type handling
* automatic ReST and HATOAS behaviours
