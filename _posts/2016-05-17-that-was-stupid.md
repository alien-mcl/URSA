---
layout: post
title: That was stupid
---

Indeed, that was stupid. While working on the hypermedia controls injection mechanism I've finally noticed that virtual stupidity I implemented. I had shared in-memory triple store for both request/response payload and for the demo site!

Why it was stupid? Because in order to have the graphs synchronized and maintained properly between requests I introduced not that simple mechanism strictly bound to both request and _RomanticWeb_'_s_ entity contexts.

And the simplest solution is to ... have the triple store for actual server-side data separated from the in-memory one that can be recreated from scratch just to serve the request/response. Simple as that.

Damn, I need some rest. Gladly The Witcher DLC is comming soon :).