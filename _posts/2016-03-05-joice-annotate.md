---
layout: post
title: JoICe Annotate
---

Finally I got it covered (mostly). I was able to craft in haste a _Grunt_ tool that annotates classes with their dependencies.
Well, not dependencies, but the constructor variables, but still it prevents the minification process from breaking my IoC.

Anyway, I've got no more excuses to think about moving to the part that I implement ... _hydra:Collection_.

I was quite reluctant to do so as I feel that the way it's specified doesn'f fit non-RDF APIs,
but maybe with using pure HTTP headers I could have similar behaviour for those situations.

In general, RDF API responses for collections should return a _hydra:Collection_ hypermedia control which tells about it's members and few other things.
The issue I've got with that that I've got a mix of data and hypermedia controls which enforces client to do extra processing.
I'd prefer something MVC'ish approach where a distinction between data and other controls is easier to pin-point.

Anyway, I've got to stop fooling aroung the project and finally work on that part.