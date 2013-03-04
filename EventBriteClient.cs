using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;

namespace EventBrite.WinStore
{
	public class EventBriteClient : DynamicObject
	{
		private IDictionary<string, object> Dictionary { get; set; }
		private IDictionary<string, object> AuthTokens { get; set; }
		private const string ROOT_URI = "https://www.eventbrite.com";

		public EventBriteClient(IEnumerable<KeyValuePair<string, object>> auth_tokens)
		{
			Dictionary = new Dictionary<string, object>();
			AuthTokens = new Dictionary<string, object>();
			foreach (var prop in auth_tokens)
			{
				AuthTokens.Add(prop);
			}
		}

		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			result = null;

			try
			{
				for (var i = 0; i < args.Count(); i++)
				{
					Dictionary.Add(binder.CallInfo.ArgumentNames[i], args[i]);
				}

				using (var webClient = new HttpClient())
				{
					if (AuthTokens.ContainsKey("access_token"))
					{
						webClient.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer #{{{0}}}", Dictionary["access_token"]));
					}
					else if (AuthTokens.ContainsKey("app_key"))
					{
						Dictionary.Add("app_key", AuthTokens["app_key"]);

						if (AuthTokens.ContainsKey("user_key"))
						{
							Dictionary.Add("user_key", AuthTokens["user_key"]);
						}
						else if (AuthTokens.ContainsKey("user") && AuthTokens.ContainsKey("password"))
						{
							Dictionary.Add("user", AuthTokens["user"]);
							Dictionary.Add("password", AuthTokens["password"]);
						}
					}
					else
					{
						throw new InvalidOperationException("Sufficient valid authorization tokens have not been provided.");
					}

					var query = Dictionary.Aggregate(
						string.Empty,
						(current, item) => current + string.Format("{0}={1}&", item.Key, item.Value)
					);
					query = query.TrimEnd('&');

					var requestUrl = string.Format("{0}/{1}/{2}?{3}", 
						ROOT_URI, 
						Dictionary.ContainsKey("data_type") ? Dictionary["data_type"] : "json",
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
			Dictionary = new Dictionary<string, object>();
			for (var i = 0; i < args.Count(); i++)
			{
				Dictionary.Add(binder.CallInfo.ArgumentNames[i], args[i]);
			}

			result = this;
			return true;
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			if (Dictionary.ContainsKey(binder.Name))
			{
				result = Dictionary[binder.Name];
				return true;
			}
			else
			{
				result = null;
				return false;
			}
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			if (Dictionary.ContainsKey(binder.Name))
			{
				Dictionary[binder.Name] = value;
			}
			else
			{
				Dictionary.Add(binder.Name, value);
			}

			return true;
		}

		public override bool TryDeleteMember(DeleteMemberBinder binder)
		{
			if (Dictionary.ContainsKey(binder.Name))
			{
				Dictionary.Remove(binder.Name);
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
