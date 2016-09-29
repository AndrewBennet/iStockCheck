using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace stockcheck {
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
		private readonly string _pushbulletToken = ConfigurationManager.AppSettings["pushbullet-token"];

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(_sleepTime) };
			timer.Tick += (dispatchSender, args) => CheckStock();
			timer.Start();
		}

		async void CheckStock()
		{
			string message = await _stockChecker.CheckForStockAsync();
			LastStockCheck = DateTime.Now;
			WindowMessageLabel.Content = $"Last stock check at {DateTime.Now.ToLongTimeString()}";
			if (message != null)
			{
				MessageBox.Show(message, $"Stock found at {LastStockCheck}!", MessageBoxButton.OK, MessageBoxImage.Exclamation,
					MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
				if(_pushbulletToken != null) {
					await SendPush(message);
				}
			}
		}

		async Task SendPush(string message) {
			try {
				byte[] webContent = Encoding.ASCII.GetBytes($"{{ \"type\": \"note\", \"title\": \"stockcheck\", \"body\": \"{message}\" }}");

				HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("https://api.pushbullet.com/v2/pushes");
				webRequest.Method = "POST";
				webRequest.ContentType = "application/json";
				webRequest.Credentials = new NetworkCredential(_pushbulletToken, "");
				webRequest.ContentLength = webContent.Length;
				using(Stream requestStream = webRequest.GetRequestStream()) {
					await requestStream.WriteAsync(webContent, 0, webContent.Length);
					requestStream.Close();
				}

				using(HttpWebResponse response = webRequest.GetResponse() as HttpWebResponse) {
					using(StreamReader reader = new System.IO.StreamReader(response.GetResponseStream())) {
						string responseContent = reader.ReadToEnd();
					}
				}
			}
			catch {
				// don't crash
			}
		}
	}
}
