using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

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

		private Notifier _notifier;
		private StockChecker _stockChecker;
		private int _sleepTime;

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e) {
			try {
				_stockChecker = new StockChecker();
				_notifier = new Notifier();
				_sleepTime = int.Parse(ConfigurationManager.AppSettings["sleeptime"]);
			}
			catch(Exception) {
				WindowMessageLabel.Content = "Config file is invalid. Please fix and restart application.";
				return;
			}

			DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(_sleepTime) };
			timer.Tick += (dispatchSender, args) => RelayStock();
			timer.Start();
		}

		async void RelayStock() {
			Dictionary<IphoneModel, List<string>> stock;
			DateTime now = DateTime.Now;
			try {
				 stock = await _stockChecker.CheckForStockAsync();
			}
			catch(AppleConnectivityException) {
				WindowMessageLabel.Content = $"Apple connectivity issue at {now.ToLongTimeString()}";
				return;
			}
			catch(AppleFormatException) {
				WindowMessageLabel.Content = $"Unexpected Apple response issue at {now.ToLongTimeString()}";
				return;
			}

			try {
				_notifier.Notify(now, stock);
				WindowMessageLabel.Content = $"Last stock check at {now.ToLongTimeString()}";
			}
			catch {
				WindowMessageLabel.Content = $"Error sending notification at {now.ToLongTimeString()}";
			}
			
		}
	}
}
