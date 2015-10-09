---
layout: default
---
### Roadmap
This is an under-development project and hard work is still ahead. Also Hydra itself is under development, thus there are few things that are not compliant for now.

These would be:
- [ ] **hydra:expects** and **hydra:returns** currently have an anonymous resource assigned with few custom predicates describing things like cardinality as Hydra is not yet there
- [ ] Property marked with **KeyAttribute** are expressed via owl:InverseFunctionalProperty. While for reference types it is pretty valid, for literals whole RDF becomes valid only with OWL Full.
- [ ] Implement SHACL data shapes description
- [ ] ... we'll see how it will end up :)