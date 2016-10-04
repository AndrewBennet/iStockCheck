using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace com.andrewbennet.istockcheck {
	class Notifier {
		private readonly string _pushbulletToken;
		private readonly string _telegramBotId;
		private readonly string _telegramChatId;
		private readonly bool _enableWindowsAlerts;
		
		private readonly HttpClient _pushBulletClient = new HttpClient();
		private readonly HttpClient _telegramClient = new HttpClient();

		public Notifier() {
			_enableWindowsAlerts = bool.Parse(ConfigurationManager.AppSettings["windows-alerts"]);

			string pushBulletToken = ConfigurationManager.AppSettings["pushbullet-token"]?.Trim();
			_pushbulletToken = string.IsNullOrWhiteSpace(pushBulletToken) ? null : pushBulletToken;
			if(_pushbulletToken != null) {
				_pushBulletClient.DefaultRequestHeaders.Add("Access-Token", _pushbulletToken);
			}

			string telegramBotId = ConfigurationManager.AppSettings["telegram-bot-id"]?.Trim();
			string telegramChatId = ConfigurationManager.AppSettings["telegram-chat-id"]?.Trim();
			_telegramBotId = string.IsNullOrWhiteSpace(telegramBotId) ? null : telegramBotId;
			_telegramChatId = string.IsNullOrWhiteSpace(telegramChatId) ? null : telegramChatId;
		}

		public async void Notify(DateTime timeOfCheck, Dictionary<IphoneModel, List<string>> stock) {
			if(stock.Any()) {
				if(_enableWindowsAlerts) {
					StringBuilder messageBuilder = new StringBuilder();
					foreach(IphoneModel model in stock.Keys) {
						messageBuilder.AppendLine($"{model.ToDisplayName()} available at {string.Join(", ", stock[model])}.");
					}
					string message = messageBuilder.ToString();
					MessageBox.Show(message, $"Stock found at {timeOfCheck}!", MessageBoxButton.OK, MessageBoxImage.Exclamation,
						MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
				}
				if(_pushbulletToken != null) {
					foreach(IphoneModel model in stock.Keys) {
						await SendPushbullet($"{model.ToDisplayName()} available at {string.Join(", ", stock[model])}.", "");
					}
				}
				if(_telegramBotId != null && _telegramChatId != null) {
					foreach(IphoneModel model in stock.Keys) {
						await SendTelegram($"{model.ToDisplayName()} available at {string.Join(", ", stock[model])}.");
					}
				}
			}
		}

		async Task SendPushbullet(string title, string body) {
			try {
				JObject con = new JObject(new JProperty("body", body), new JProperty("title", title), new JProperty("type", "note"));
				HttpContent content = new StringContent(con.ToString(), Encoding.UTF8, "application/json");
				await _pushBulletClient.PostAsync("https://api.pushbullet.com/v2/pushes", content);
			}
			catch(Exception) {
				
			}
		}

		async Task SendTelegram(string message) {
			try {
				await _telegramClient.GetAsync($"https://api.telegram.org/bot{_telegramBotId}/sendMessage?chat_id={_telegramChatId}&text={message}");
			}
			catch (Exception) {

			}
		}
	}
}