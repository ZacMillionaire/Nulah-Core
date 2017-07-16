# Major TODO
* Fix OAuth to use https correctly
	* This'll probably involve fixing kestrel to correctly use https
		* Which is its own barrel of fun
	* Should only need to work in Startup.cs + ScreamingInterceptionFilter.cs to sort that out