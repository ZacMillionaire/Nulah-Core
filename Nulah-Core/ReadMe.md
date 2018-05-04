# THIS PROJECT HAS MOVED
The newest version of this can be found at https://github.com/ZacMillionaire/Nulah.Blog

This repo remains so you can look at poor code.





# Major TODO
* Fix OAuth to use https correctly - think I've got this sorted
	* This'll probably involve fixing kestrel to correctly use https - kestrel is http only, but nginx sits in front of it for my set up. This isn't good or ideal, but I'm just a bit to time poor to figure it out fully.
		* ~Which is its own barrel of fun~
	* Should only need to work in Startup.cs + ScreamingInterceptionFilter.cs to sort that out
* Work out how to provide multiple user profile handles for different providers
	* I want to avoid reflection if possible, even though ASP is basically a hallway of mirrors in that regard.
	* Would like to have it so it works out the user profile provider to call based on the provider defined in appsettings.json
	* So for now the profile stuff is hard coded to GitHub specific.
	* Other thing: Need to make a base profile data for common user details across providers, and then a provider specific dictionary that can be referenced in views and so on.
		* The provider specific dictionary is the easy part of this at least.
	* Would like to divine the profile class to use from the provider name.
		* not sure on overhead for this.

# Design Notes
The idea of this is to create a quick and easy blog platform that relates to an OAuth provider, allowing likeminded people to login with the same details, and keeps the content related.

Currently I only support GitHub, but I have plans to make a provider for Discord for gaming related blogs.

I don't use https://github.com/aspnet-contrib/AspNet.Security.OAuth.Providers because the idea is to keep the provider details in appsettings.json (skeleton coming soon), generic.