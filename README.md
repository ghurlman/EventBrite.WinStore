#EventBrite.WinStore#

##Description##
A wee lil' dynamic, C#-based web client for the Eventbrite API.

Inspired by the [eventbrite-client.rb project](https://github.com/ryanjarvinen/eventbrite-client.rb).

##Usage Examples##

###Initializing the client###
Your API / Application key is required to initialize the client - http://eventbrite.com/api/key

Set your user_key if you want to access private data - http://eventbrite.com/userkeyapi

	dynamic auth_tokens = new ExpandoObject(); // any IEnumerable will do here
	auth_tokens.app_key = "YOUR_APP_KEY";
	auth_tokens.user_key = "YOUR_USER_KEY";
	dynamic client = new EventBriteClient(auth_tokens);

###Initializing the client using an OAuth2.0 access_token###
You can also initialize our API client using an OAuth2.0 `access_token`, like this:

	dynamic auth_tokens = new ExpandoObject(); // any IEnumerable will do here
	auth_tokens.access_token = "YOUR_OAUTH2_ACCESS_TOKEN";
	dynamic client = new EventBriteClient(auth_tokens);

###Calling API methods###

To call an EventBrite API method, simply invoke the method right from the client, and pass in named parameters that match the API parameters.

See [Eventbrite's API method documentation](http://developer.eventbrite.com/doc/) for more information about the list of available client methods.

Here is an example using the API's [event\_list\_attendees](http://developer.eventbrite.com/doc/users/event_list_attendees/) method:

    var result = client.event_list_attendees();

The [event_get](http://developer.eventbrite.com/doc/events/event_get/) API call should look like this:

    var result = client.event_get(id: 1848891083);

The [event_search](http://developer.eventbrite.com/doc/events/event_search/) API call might look like this:

    var result = client.event_search(keywords: "Yankees OR Rangers", category: "sports", region: "NY");

##Resources##
* API Documentation - <http://developer.eventbrite.com/doc/>
* API QuickStart Guide - <http://developer.eventbrite.com/doc/getting-started/>
* Eventbrite Open Source - <http://eventbrite.github.com/>
* Eventbrite App Showcase - <http://eventbrite.appstores.com/>