using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using MessageBox = System.Windows.MessageBox;
using MessageBoxOptions = System.Windows.MessageBoxOptions;

namespace com.andrewbennet.istockcheck {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow {
		public MainWindow() {
			InitializeComponent();
			WindowMessageLabel.Content = "Checking iPhone stock";

			// Restoration from minimisation
			NotifyIcon ni = new NotifyIcon {
				Icon = new Icon("Resources/Phone-64.ico"),
				Visible = true
			};
			ni.DoubleClick += delegate {
				Show();
				WindowState = WindowState.Normal;
			};
		}

		protected override void OnStateChanged(EventArgs e) {
			// Hide when minimised
			if(WindowState == WindowState.Minimized) {
				Hide();
			}

			base.OnStateChanged(e);
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
				await _pushBulletClient.PostAsync("https://api.pushbullet.com/v2/pushes", content);
			}
			catch(Exception) {
				// don't crash
			}
		}
	}
}
