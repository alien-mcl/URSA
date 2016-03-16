---
layout: post
title: Template - Shameplate
---

While figuring it out on why the authentication in the console client demo ceased to work property, I found several other issues.

These were related generally to switching list parameter mapping for Iri templates to OData. This spec uses variables lie _$top_ or _$skip_ for queries.
Unfortunately, it came out that the _$_ char is not compatible with the Url template spec - it had to be escaped with _%_ as a hex number.
It was also incompatible with _ExpandoObject_ I used for passing variables for the template, which I had to replace with the _IDictionary_.
I don't have to mention that these variables were not covered in the tests.

Few other minor pips and boobs here and there and finally it started to work.

I have to have some rest as the final thing is left here - hypermedia controls handling.