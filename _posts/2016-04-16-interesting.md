---
layout: post
title: Interesting
---

That was an interesting experience. I started reviewing my code and unit tests.
I tried to use code coverage to find out places that would need some testing.

As I am using a community version of Visual Studio I had no tools to measure code coverage.

I did have in the solution OpenCover UI configured, but id had it's issues. After few updates it still has, but seems it less faulty and results looks promising.

Unfortunately, this plugin has a tiny issue with MSTest runner - it does not discover tests that are marked with proper attributes in a base class.
Indeed, native MSTest runner from Visual Studio has an issue with discovering/running tests from a different assembly, but base class tests from same assembly runs OK.

Good thing that the plugin is open source and I was able to pin-point the culprit. The fix actually ended up with three-or-so lines of extra code in the test discovery utility.

It did improve the overal coverage thus I'm not chasing classes that were fully tested. I think I'm gonna make a pull request with that fix.