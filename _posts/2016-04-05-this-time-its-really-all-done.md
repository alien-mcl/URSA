---
layout: post
title: This time it's really all done!
---

OK, so these are now done and available:

[x] Server-side support for _hydra:Collection_ in API Documentation
[x] Server-side support for _hydra:Collection_ and _hydra:PartialCollectionView_ in in-band data payload (RDF only)
[x] Client-side support for _hydra:Collection_ in API Documentation
[x] Client-side support for _hydra:Collection_ and _hydra:PartialCollectionView_ in in-band data payload (RDF only)
[x] Support for _hydra:Collection_ in XSLT API Documentation transformation

The worst nightmare was API documentation client-side analysis, especially the data model - OWL is wonderful for description, but terrible for analysis.

I need to think on the next steps. One of them would be finally to put my hands on the _Hydra_console_ and test it against my API documentation.
I'm somehow discouraged from doing it as I feel that it won't work due to not-that-simple OWL description.
I'd also try to fix the known (for me at least) bug in the framework that won't match a controller action if a unpredicted query string parameter is introduced (i.e. random cache-disabling one).
I'd also try to move the client code to a separate _node.js_ project and think about pushing it to the _NPM_.

I need to write those down and pick one. But hey, where is my rest?