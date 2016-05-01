---
layout: post
title: Nice excercise
---

Well, I didn't think too much about having own URL representation. Actually first lines of code appeared few days ago.
I thought it as an interesting excercise and refresh my knowledge of how are these handled.

Knowing all of these I feel like _System.Uri_ seems ... strange, i.e. property _Segments_ is supposed to have scheme specific parts of the path, but actually each scheme may have it handled on it's own.
Indeed, common internet protocol resource locators uses _/_ char to build hierarchy in the address path but it's not the case for other schemes.
This means that the authors of _System.Uri_ decided to somehow acknowledge IP based protocol addresses in a very special way.
Unfortunately, they didn't decide to acknowledge in a very special way HTTP based URLs as creating an instance of the _System.Uri_ with a relative without specifying it explicitely fails.

Pity. Anyway - I decided to stick to generic parser that tries to detect URL's scheme and find which of the internal parsers registered can do the job.
I also decided to build a hierarchy of URL classes, of which the URL would be the most generic with just few common properties.

We'll see how it will work with the rest of the infrastructure.