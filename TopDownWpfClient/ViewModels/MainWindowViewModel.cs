using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.AnyContainer;
using ServiceReference1;
using TopDown;
using TopDownGrpcClient;
using TopDownWpfClient.Services.Pages;
using TopDownWpfClient.Services.Windows;
using TopDownWpfClient.Views;

namespace TopDownWpfClient.ViewModels {
	public class MainWindowViewModel : BaseViewModel {
		private StartPageViewModel _startPageVM;
		public Page StartPage { get; set; }

		private SearchGamePageViewModel _searchGamePageVM;
		public Page SearchGamePage { get; set; }
		
		private SettingsPageViewModel _settingsPageVM;
		public Page SettingsPage { get; set; }

		private JoinServerPageViewModel _joinServerPagePageVM;
		public Page JoinServerPage { get; set; }

		public Page CurrentPage { get; set; }

		public string DebugLog { get; set; }

		public Visibility DebugVisibility => !string.IsNullOrEmpty(DebugLog) ? Visibility.Visible : Visibility.Hidden;

		public bool GUIEnabled { get; set; }

		public string Status { get; set; }

		public double WindowScale { get; set; }

		public static MainWindowViewModel Instance { get; set; }

		public MainWindowViewModel() {
			Instance = this;
			var pageManager = StaticResolver.Resolve<IPageManager>();
			_startPageVM = new StartPageViewModel();
			_searchGamePageVM = new SearchGamePageViewModel();
			_settingsPageVM = new SettingsPageViewModel();
			_joinServerPagePageVM = new JoinServerPageViewModel();
			_startPageVM.PlayEvent += SwitchPageToSearchGamePage;
			_startPageVM.SettingsEvent += SwitchPageToSettingsPage;
			_startPageVM.ExitEvent += KillApp;
			_searchGamePageVM.BackEvent += SwitchPageToStartPage;
			_searchGamePageVM.SearchGameEvent += SearchGame; 
			_searchGamePageVM.JoinServerEvent += SwitchPageToJoinServerPage; 
			_settingsPageVM.BackEvent += SwitchPageToStartPage;
			_joinServerPagePageVM.BackEvent += SwitchPageToSearchGamePage;
			_joinServerPagePageVM.JoinedServer += LaunchGame;
			

			StartPage = pageManager.CreatePage(_startPageVM);
			SearchGamePage = pageManager.CreatePage(_searchGamePageVM);
			SettingsPage = pageManager.CreatePage(_settingsPageVM);
			JoinServerPage = pageManager.CreatePage(_joinServerPagePageVM);

			CurrentPage = StartPage;
			GUIEnabled = true;
		}

		private void SwitchPageToSearchGamePage() {
			CurrentPage = SearchGamePage;
		}

		private void SwitchPageToStartPage() {
			CurrentPage = StartPage;
		}

		private void SwitchPageToSettingsPage() {
			CurrentPage = SettingsPage;
		}

		private void SwitchPageToJoinServerPage()
		{
			CurrentPage = JoinServerPage;
		}


		private void KillApp() {
			Application.Current.Shutdown();
		}

		private void SearchGame() {
			GUIEnabled = false;
			Status = "";
			_ = Task.Run(() => {
				//Get ServerAddress and ServerPort from TopDownMainServer
				Status = "Searching server...";
				(string, int) server;
				try
				{
					server = GetServer();
				}
				catch (Exception e)
				{
					DebugLog += "\n" + e.Message;
					// DebugLog += e.InnerException?.Message;
					Status = "Error!";
					return;
				}

				Messages.ServerAddress = server.Item1;
				// Messages.ServerAddress = "26.202.152.148";
				Messages.ServerPort = server.Item2;
				// Messages.ServerPort = 5000; //TODO: доделать
											// Messages.ServerPort = "5000";
											//Open game with acquired ServerAddress and ServerPort 

				if (string.IsNullOrEmpty(Messages.ServerAddress) || Messages.ServerPort < 0) {
					Status = "Server didn't give correct ServerAddress or ServerPort";
					return;
				}

				Status = "Server found!";
				var mainWindow = StaticResolver.Resolve<IWindowManager>().GetView(this);
				using (var game = new MainGame())
				{
					mainWindow.Dispatcher.Invoke(() => {
						mainWindow.Visibility = Visibility.Collapsed;
					});
					game.Run();
					mainWindow.Dispatcher.Invoke(() => {
						mainWindow.Visibility = Visibility.Visible;
						mainWindow.Focus();
					});
				}
			}).ContinueWith((t) => {
				GUIEnabled = true;
			});
		}

		private void LaunchGame(object sender, (string, int) server)
		{
			GUIEnabled = false;
			_ = Task.Run(() => {
				//Get ServerAddress and ServerPort from TopDownMainServer

				Messages.ServerAddress = server.Item1;
				Messages.ServerPort = server.Item2;
				var mainWindow = StaticResolver.Resolve<IWindowManager>().GetView(this);
				using (var game = new MainGame())
				{
					mainWindow.Dispatcher.Invoke(() => {
						mainWindow.Visibility = Visibility.Collapsed;
					});
					game.Run();
					mainWindow.Dispatcher.Invoke(() => {
						mainWindow.Visibility = Visibility.Visible;
						mainWindow.Focus();
					});
				}
			}).ContinueWith((t) => {
				GUIEnabled = true;
			});
		}

		private (string, int) GetServer()
        {
			var serviceClient = new MyServiceClient(MyServiceClient.EndpointConfiguration.BasicHttpBinding_IMyService);
			var serverResult = serviceClient.GetAvailableServerAsync();
			serverResult.Wait();
			return serverResult.Result;
        }
	}
}