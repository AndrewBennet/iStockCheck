using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;

namespace com.andrewbennet.istockcheck {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow {
		public MainWindow() {
			InitializeComponent();
			WindowMessageLabel.Content = "Checking iPhone stock";
		}

		public DateTime LastStockCheck { get; private set; }
		readonly StockChecker _stockChecker = new StockChecker();
		private readonly int _sleepTime = int.Parse(ConfigurationManager.AppSettings["sleeptime"]);
		private readonly string _pushbulletToken = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["pushbullet-token"]) ? null : ConfigurationManager.AppSettings["pushbullet-token"];
		private readonly HttpClient _pushBulletClient = new HttpClient();

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			if(_pushbulletToken != null) {
				_pushBulletClient.DefaultRequestHeaders.Add("Access-Token", _pushbulletToken);
			}
			DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(_sleepTime) };
			timer.Tick += (dispatchSender, args) => CheckStock();
			timer.Start();
		}

		async void CheckStock() {
			Dictionary<IphoneModel, List<string>> stock = await _stockChecker.CheckForStockAsync();
			LastStockCheck = DateTime.Now;
			WindowMessageLabel.Content = $"Last stock check at {DateTime.Now.ToLongTimeString()}";
			if(stock.Any()) {
				if(_pushbulletToken == null) {
					StringBuilder messageBuilder = new StringBuilder();
					foreach(IphoneModel model in stock.Keys) {
						messageBuilder.AppendLine($"{model.ToDisplayName()} available at {string.Join(", ", stock[model])}.");
					}
					string message = messageBuilder.ToString();
					MessageBox.Show(message, $"Stock found at {LastStockCheck}!", MessageBoxButton.OK, MessageBoxImage.Exclamation,
						MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
				}
				else {
					foreach(IphoneModel model in stock.Keys) {
						await SendPush($"{model.ToDisplayName()} available at {string.Join(", ", stock[model])}.");
					}
				}
			}
		}

		async Task SendPush(string message) {
			try {
				JObject con = new JObject(new JProperty("body", ""), new JProperty("title", message), new JProperty("type", "note"));
				HttpContent content = new StringContent(con.ToString(), Encoding.UTF8, "application/json");
				HttpResponseMessage response = await _pushBulletClient.PostAsync("https://api.pushbullet.com/v2/pushes", content);
			}
			catch(Exception e) {
				// don't crash
			}
		}
	}
}
