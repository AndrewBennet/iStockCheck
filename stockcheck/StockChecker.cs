using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace stockcheck {

    public class StockChecker {
        public List<IphoneModel> Models { get; }
        public string PostCode { get; }

        public HttpClient HttpClient { get; } = new HttpClient();
        public const string BaseUrl = "http://www.apple.com/uk/shop/retail/pickup-message";//?parts.0=MN4U2B%2FA&location=E2+0DN&little=true";

        public StockChecker() {
            var IphoneSizes = ConfigurationManager.AppSettings["iphone-size"].Split(',').Select(typ => (IphoneSize)Enum.Parse(typeof(IphoneSize), typ.Trim())).ToList();
            var Colours = ConfigurationManager.AppSettings["colour"].Split(',').Select(typ => (Colour)Enum.Parse(typeof(Colour), typ.Trim())).ToList();
            var StorageSizes = ConfigurationManager.AppSettings["storage-size"].Split(',').Select(typ => (StorageSize)Enum.Parse(typeof(StorageSize), typ.Trim())).ToList();
            Models = IphoneModel.GetModels(IphoneSizes, StorageSizes, Colours).ToList();

            PostCode = ConfigurationManager.AppSettings["post-code"];
        }


        public async Task<string> CheckForStockAsync() {
            StringBuilder message = new StringBuilder();

            foreach(var iphoneModel in Models) {
                var QueryUrl = new Uri($"{BaseUrl}?parts.0={iphoneModel.ToIdentifier()}&location={PostCode.Replace(' ', '+')}&little=false");

                // Ping the API
                HttpResponseMessage result = await HttpClient.GetAsync(QueryUrl);

                // Load the response
                string webContent = await result.Content.ReadAsStringAsync();

                JObject jObject;
                if(JsonTools.TryParseJsonObject(webContent, out jObject)) {
                    JArray stores = jObject["body"]["stores"].Value<JArray>();
                    if(stores != null) {
                        foreach(var store in stores) {
                            string pickupQuote = store["partsAvailability"][iphoneModel.ToIdentifier()]["storePickupQuote"].Value<string>();
                            if(pickupQuote != "Currently unavailable") {
                                message.AppendLine($"{iphoneModel.ToDisplayName()} available at {store["storeName"].Value<string>()}!");
                            }
                        }
                    }
                }
            }
            if(message.Length != 0) {
                return message.ToString();
            }
            return null;
        }
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