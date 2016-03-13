---
layout: post
title: Opps, I did it again
---

Well, it was not to well after all. I forgot that I've got yet another client in the project.

Actually two more!

First is the proxy code generation tool and the other is an XSLT for the RDF/XML generated API documentation that shows a nice looking API documentation page.
I've started to modify the first of them and I've already stumbled on complex OWL data type descriptions.
I've got sub-classes and sub-classes of sub-classes in a tree and visiting all of these cases becomes a drag.
I wonder for how long people behind Hydra Core Vocabulary will refuse to use something less complex than RDFS/OWL.

Anyway, I'm almost done with the proxy, but the XSLT still remains untouched. It's getting more time-consuming to maintain all those changes.

Maybe dropping the XSLT in favour of the URSA JavaScript client would be the option here? I should consider it as it touches same topic...