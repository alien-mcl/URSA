---
layout: post
title: Not so fast
---

My success with JavaScript client changes regarding introduction of the _hydra:Collection_ was a bit to premature.

While my fixture and unit tests works OK, the client failed to work properly with the server generated API documentation.
I have to revisit the model parser again and tweak the fixture to have it reflect the server code properly.

The funny thing is that the fixture seems OK - maybe I should confront both of them.
I may find that the server is actually a culprit and will have to adjust it instead of the client.

We'll see.