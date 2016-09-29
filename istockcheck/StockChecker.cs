using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace com.andrewbennet.istockcheck {

	public class StockChecker {
		public List<IphoneModel> Models { get; }
		public string PostCode { get; }
		public HttpClient HttpClient { get; } = new HttpClient();

		public const string BaseUrl = "http://www.apple.com/uk/shop/retail/pickup-message";

		public StockChecker() {
			List<PhoneSize> iphoneSizes = ConfigTools.EnumsFromListString<PhoneSize>(ConfigurationManager.AppSettings["iphone-size"]);
			List<Colour> colours = ConfigTools.EnumsFromListString<Colour>(ConfigurationManager.AppSettings["colour"]);
			List<StorageSize> storageSizes = ConfigTools.IntsFromStringList(ConfigurationManager.AppSettings["storage-size"]).Select(IphoneModel.StorageSizeFromInt).ToList();
			PostCode = ConfigurationManager.AppSettings["post-code"];
			
			Models = IphoneModel.GetModels(iphoneSizes, storageSizes, colours).ToList();
		}

		public async Task<Dictionary<IphoneModel, List<string>>> CheckForStockAsync() {
			Dictionary<IphoneModel, List<string>> storesByIphone = new Dictionary<IphoneModel, List<string>>();

			foreach(List<IphoneModel> modelBatch in Models.Batch(10)) {

				UriBuilder uriBuilder = new UriBuilder(BaseUrl) {
					Query = string.Join("&", modelBatch.Select((model, index) => $"parts.{index}={model.ToIdentifier()}").Concat(new[] {$"location={PostCode.Replace(' ', '+')}", "little=false"}))
				};

				string webContent = null;
				try {
					// Ping the API
					HttpResponseMessage result = await HttpClient.GetAsync(uriBuilder.Uri);

					// Load the response
					webContent = await result.Content.ReadAsStringAsync();
				}
				catch {
					// don't crash if there's no internet
				}

				try {
					JObject jObject;
					if(JsonTools.TryParseJsonObject(webContent, out jObject)) {
						JArray stores = jObject["body"]["stores"].Value<JArray>();
						if(stores != null) {
							foreach(JToken store in stores) {
								foreach(IphoneModel iphoneModel in modelBatch) {
									string pickupQuote = store["partsAvailability"][iphoneModel.ToIdentifier()]["storePickupQuote"].Value<string>();
									if(!pickupQuote.ToLower().Contains("unavailable")) {
										if(storesByIphone.ContainsKey(iphoneModel)) {
											storesByIphone[iphoneModel].Add(store["storeName"].Value<string>());
										}
										else {
											storesByIphone.Add(iphoneModel, new List<string> {store["storeName"].Value<string>()});
										}
									}
								}

							}
						}
					}
				}
				catch {
					// don't crash if the JSON isn't what we expect
				}
			}

			return storesByIphone;
		}
	}

	static class ConfigTools {
		public static List<TEnum> EnumsFromListString<TEnum>(string commaSeparatedList) {
			return commaSeparatedList.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
				.Select(str => str.Trim())
				.Where(str => !string.IsNullOrWhiteSpace(str))
				.Select(EnumTools.EnumFromString<TEnum>)
				.ToList();
		}

		public static List<int> IntsFromStringList(string commaSeparatedList) {
			return commaSeparatedList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(str => str.Trim())
				.Where(str => !string.IsNullOrWhiteSpace(str))
				.Select(int.Parse)
				.ToList();
		}
	}

	static class EnumerableTools {
		public static IEnumerable<List<T>> Batch<T>(this IEnumerable<T> collection, int batchSize) {
			List<T> nextbatch = new List<T>(batchSize);
			foreach(T item in collection) {
				nextbatch.Add(item);
				if(nextbatch.Count == batchSize) {
					yield return nextbatch;
					nextbatch = new List<T>(batchSize);
				}
			}
			if(nextbatch.Count > 0) yield return nextbatch;
		}
	}

	static class EnumTools {
		public static TEnum EnumFromString<TEnum>(string input) => (TEnum)Enum.Parse(typeof(TEnum), input);
	}

	static class UnixTimestamp {
		public static long Now => (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
	}

	static class JsonTools {
		public static bool TryParseJsonObject(string value, out JObject jsonObject) {
			jsonObject = null;
			try {
				using(JsonReader reader = new JsonTextReader(new StringReader(value))) {
					jsonObject = JObject.Load(reader);
					return true;
				}
			}
			catch(JsonReaderException) {
				return false;
			}
			catch {
				//yes, the JsonTextReader throws Exceptions, despite advertising JsonReaderException.
				return false;
			}
		}
	}
}