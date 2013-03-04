using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;

namespace EventBrite.WinStore
{
	public class EventBriteClient : DynamicObject
	{
		private IDictionary<string, object> AuthTokens { get; set; }
		private const string ROOT_URI = "https://www.eventbrite.com";

		public EventBriteClient(IEnumerable<KeyValuePair<string, object>> auth_tokens)
		{
			AuthTokens = new Dictionary<string, object>();
			foreach (var prop in auth_tokens)
			{
				AuthTokens.Add(prop);
			}
		}

		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			var dictionary = new Dictionary<string, object>();
			result = null;

			try
			{
				for (var i = 0; i < args.Count(); i++)
				{
					dictionary.Add(binder.CallInfo.ArgumentNames[i], args[i]);
				}

				using (var webClient = new HttpClient())
				{
					if (AuthTokens.ContainsKey("access_token"))
					{
						webClient.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer #{{{0}}}", dictionary["access_token"]));
					}
					else if (AuthTokens.ContainsKey("app_key"))
					{
						dictionary.Add("app_key", AuthTokens["app_key"]);

						if (AuthTokens.ContainsKey("user_key"))
						{
							dictionary.Add("user_key", AuthTokens["user_key"]);
						}
						else if (AuthTokens.ContainsKey("user") && AuthTokens.ContainsKey("password"))
						{
							dictionary.Add("user", AuthTokens["user"]);
							dictionary.Add("password", AuthTokens["password"]);
						}
					}
					else
					{
						throw new InvalidOperationException("Sufficient valid authorization tokens have not been provided.");
					}

					var query = dictionary.Aggregate(
						string.Empty,
						(current, item) => current + string.Format("{0}={1}&", item.Key, item.Value)
					);
					query = query.TrimEnd('&');

					var requestUrl = string.Format("{0}/{1}/{2}?{3}", 
						ROOT_URI,
						dictionary.ContainsKey("data_type") ? dictionary["data_type"] : "json",
						binder.Name, 
						query
					);
					var response = webClient.GetAsync(Uri.EscapeUriString(requestUrl));

					result = response.Result.Content.ReadAsStringAsync().Result;
					return true;
				}
			}
			catch (HttpRequestException ex)
			{
				result = null;
				return false;
			}
		}

		public override bool TryCreateInstance(CreateInstanceBinder binder, object[] args, out object result)
		{
			AuthTokens = new Dictionary<string, object>();
			for (var i = 0; i < args.Count(); i++)
			{
				AuthTokens.Add(binder.CallInfo.ArgumentNames[i], args[i]);
			}

			result = this;
			return true;
		}
	}
}
