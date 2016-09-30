using System;
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

		private StockRelayer _stockRelayer;
		private int _sleepTime;

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e) {
			try {
				_stockRelayer = new StockRelayer();
				_sleepTime = int.Parse(ConfigurationManager.AppSettings["sleeptime"]);
			}
			catch(Exception) {
				WindowMessageLabel.Content = "Config file is invalid. Please fix and restart application.";
				return;
			}

			DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(_sleepTime) };
			timer.Tick += (dispatchSender, args) => {
				_stockRelayer.RelayStock();
				WindowMessageLabel.Content = _stockRelayer.DisplayMessage;
			};
			timer.Start();
		}
	}
}
