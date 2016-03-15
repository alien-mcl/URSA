---
layout: post
title: Almost there
---

I'm actually done with necessary upgrades to the proxy generation tool. The code seems to be correct now, but my demo ceased to work.
It fails on the authentication. I wonder which my recent (or even older) modification broke that.

As for the collections - I still need to modify my JavaScript client to correctly parse the hypermedia controls embeded in the body.
I may try to see what would happen without doing it - maybe this procedure won'be needed, but honestly - I doubt it.

I also need to make HTTP headers alternative for non-RDF payloads. I think that HTTP Link header will do the trick as it allows to define own relation types.
Something like this might actually be pretty valid:

```
GET /api/person
...
HTTP/1.1 200 OK
Link: <http://www.w3.org/ns/hydra/core#Collection>; rel="http://www.w3.org/1999/02/22-rdf-syntax-ns#type"
```

I'll have to remember to include these in the Access-Control headers as the browser won't let me touch those in the script.

Still plenty of work ahead.