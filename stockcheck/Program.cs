using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace stockcheck
{
	class Program
	{
		static void Main(string[] args)
		{

			string type = ConfigurationManager.AppSettings["type"];
			string colour = ConfigurationManager.AppSettings["colour"];
			string model = ConfigurationManager.AppSettings["model"];
			int sleepTime = int.Parse(ConfigurationManager.AppSettings["sleeptime"]);

			List<string> storeNames = ConfigurationManager.AppSettings["stores"].Split(';').Select(str => str.Trim()).ToList();
			List<string> storeIds = null;


			Uri queryUrl = new Uri($"http://www.istocknow.com/live/live.php?type={type}&operator=&color={colour}&model={model}&ajax=1&nocache={UnixTimestamp.Now}&nobb=false&notarget=false&noradioshack=false&nostock=false");
			HttpClient httpClient = new HttpClient();

			while (true)
			{
				// Ping the API
				Task<HttpResponseMessage> result = httpClient.GetAsync(queryUrl);
				result.Wait();

				// Load the response
				Task<string> readResponseTask = result.Result.Content.ReadAsStringAsync();
				readResponseTask.Wait();

				// Load the whole response into a string
				string webContent = readResponseTask.Result;

				JObject jObject;
				if (JsonTools.TryParseJsonObject(webContent, out jObject))
				{
					JToken dataz = jObject["dataz"];
					if (dataz != null)
					{
						// First time we query, we haven't cached the store IDs
						if (storeIds == null)
						{
							storeIds = dataz.Values<JProperty>()
								.Where(jProperty => storeNames.Contains(jProperty.Value["title"].Value<string>()))
								.Select(jProp => jProp.Name)
								.ToList();
						}

						List<JProperty> relevantStoreJProperties =
							dataz.Values<JProperty>().Where(prop => storeIds.Contains(prop.Name)).ToList();

						IEnumerable<JProperty> stockedStores = relevantStoreJProperties.Where(prop => prop.Value["live"].Value<string>() != "0");
						if (stockedStores.Any())
						{
							string stockFound =
								$"Stock found at {string.Join(", ", stockedStores.Select(stockedStore => stockedStore.Value["title"].Value<string>()))}";
							Console.WriteLine($"Stock Found: {stockFound}");
							MessageBox.Show(stockFound, $"{DateTime.Now.ToLongTimeString()}: Stock Found", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
						}
						else
						{
							Console.WriteLine($"{DateTime.Now.ToLongTimeString()}: No stock found.");
						}
					}
				}
				Thread.Sleep(sleepTime);
			}
		}
	}

	static class UnixTimestamp
	{
		public static long Now => (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
	}

	class JsonTools
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