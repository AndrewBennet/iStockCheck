using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace stockcheck
{
	public class StockChecker
	{
		public string Type { get; }
		public string Colour { get; }
		public string Model { get; }
		public List<string> StoreNames { get; }
		public Uri QueryUrl { get; }

		public HttpClient HttpClient { get; } = new HttpClient();
		public const string BaseUrl = "http://www.istocknow.com/live/live.php";

		private List<string> _storeIds;

		public StockChecker()
		{
			Type = ConfigurationManager.AppSettings["type"];
			Colour = ConfigurationManager.AppSettings["colour"];
			Model = ConfigurationManager.AppSettings["model"];
			StoreNames = ConfigurationManager.AppSettings["stores"].Split(';').Select(str => str.Trim()).ToList();
			QueryUrl = new Uri($"{BaseUrl}?type={Type}&color={Colour}&model={Model}&nocache={UnixTimestamp.Now}&ajax=1&operator=&nobb=false&notarget=false&noradioshack=false&nostock=false");
		}


		public async Task<string> CheckForStockAsync()
		{
			// Ping the API
			HttpResponseMessage result = await HttpClient.GetAsync(QueryUrl);

			// Load the response
			string webContent = await result.Content.ReadAsStringAsync();
			
			JObject jObject;
			if (JsonTools.TryParseJsonObject(webContent, out jObject))
			{
				JToken dataz = jObject["dataz"];
				if (dataz != null)
				{
					// First time we query, we haven't cached the store IDs
					if (_storeIds == null)
					{
						_storeIds = dataz.Values<JProperty>()
							.Where(jProperty => StoreNames.Contains(jProperty.Value["title"].Value<string>()))
							.Select(jProp => jProp.Name)
							.ToList();
					}

					List<JProperty> stockedStores =
						dataz.Values<JProperty>()
							.Where(prop => _storeIds.Contains(prop.Name) && prop.Value["live"].Value<string>() != "0")
							.ToList();

					if (stockedStores.Any())
					{
						return $"Stock found at {string.Join(", ", stockedStores.Select(stockedStore => stockedStore.Value["title"].Value<string>()))}";
					}
				}
			}
			return null;
		}
	}

	static class UnixTimestamp
	{
		public static long Now => (int) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
	}

	static class JsonTools
	{
		public static bool TryParseJsonObject(string value, out JObject jsonObject)
		{
			jsonObject = null;
			try
			{
				using (JsonReader reader = new JsonTextReader(new StringReader(value)))
				{
					jsonObject = JObject.Load(reader);
					return true;
				}
			}
			catch (JsonReaderException)
			{
				return false;
			}
			catch
			{
				//yes, the JsonTextReader throws Exceptions, despite advertising JsonReaderException.
				return false;
			}
		}
	}
}