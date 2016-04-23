---
layout: post
title: Damn you, Uri!
---

I'm still wondering why am I using _System.Uri_! While in general this works perfectly, but has a really really annoying implementation detail that drives me crazy! Why? Try this:

```csharp
var uri = new System.Uri("http://temp.uri/document");
var anotherUri = new System.Uri("http://temp.uri/document#fragment");
Assert.Equals(uri.Equals(anotherUri));
```

It'll pass! Why? Implementation assumes that you're a client, thus no fragment should be sent to the server.
This enforces the fact that the Uri used identifies a document regardles it's internal parts idenfied by the fragment itself.
But in URSA's case we're server here! I remember that when writing [Romantic.Web](http://romanticweb.net/) we decided to introduce EntityId for that (and few other) reason as we needed literal comparison rather than _smart_ one.
Another reason would be fact that most of the properties doesn't work for relative Uris.

I'm more and more convinced that I should drop _System.Uri_ in favour of some other structure. I do remember such structures i.e. from Nancy FX, but I remember also that it lack few features known from _System.Uri_.

I must consider it carefuly as creating such a structure is not that easy as it sounds. Why? Read RFC 3986 - its about 55 pages of specification, excluding index.