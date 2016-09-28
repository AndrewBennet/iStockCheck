using System;
using System.Configuration;
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
			}
		}
	}
}
