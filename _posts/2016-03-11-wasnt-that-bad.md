---
layout: post
title: Wasn't that bad
---

It wasn't that bad after all. I was able to make several ammendments so the API documentation uses _hydra:Collection_.

Especially, using a dedicated data model on the client side instead of repacked JSON-LD payloads payed off!
Because Hydra doesn't use any specific way for describing data structures, RDFS/OWL constructs are in use.
This complicates analysis, as in theory client should be equipped with a ... reasoner. This won't happen any time soon (and actually who would go that way!).
Few changes while paring the payload were required, but the model used later didn't change at all.

It's quite unfortunate that Hydra doesn't provide all the means necessary to describe data types, neither by having a built-in solution, not by using external vocabulary (like SHACL).

Still, it's getting close to the moment when I test my API with Hydra console. I'm aftaid it won't work due to some complicated OWL'ish stuff, but we'll see.