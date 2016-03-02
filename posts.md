---
layout: default
title: Log
weight: 4
---
<ul class="posts">
{% for post in site.posts %}
	<li>
		<h3><a href="{{ site.baseurl }}{{ post.url }}">{{post.date | date:"%B %d, %Y"}}, {{ post.title }}</a></h3>
		<p>{{ post.excerpt | remove: "<p>" | remove: "</p>" }}<a href="{{ site.baseurl }}{{ post.url }}">...</a></p>
	</li>
{% endfor %}
</ul>