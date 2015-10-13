---
layout: default
title: Roadmap
weight: 3
---
### Roadmap
This is an under-development project and hard work is still ahead. Also Hydra itself is under development, thus there are few things that are not compliant for now.

These would be:

- **hydra:expects** and **hydra:returns** currently have anonymous subclass with few custom predicates describing things like cardinality as Hydra is not yet there
- few custom predicates in IRI template mappings for offset/limit collection operations are used. There is also neither **hydra:Collection** nor **hydra:PagedCollection** notion used anywhere.
- Property marked with **KeyAttribute** are expressed via owl:InverseFunctionalProperty. While for reference types it is pretty valid, for literals whole RDF becomes valid only with OWL Full.
- Implement SHACL data shapes description
- Fine-tune the proxy generator to be pluggable to Visual Studio.
- Create an angularJS generic client
- ... we'll see how it will end up :)