using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.AnyContainer;
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

		public double WindowScale { get; set; } = 1;

		public static MainWindowViewModel Instance { get; set; }

		private MainGame _game;

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
			_ = Task.Run(() => {
				//Get ServerAddress and ServerPort from TopDownMainServer
				Status = "Searching server...";
				(string, int) server;
				try {
					server = GetServer();
				} catch (Exception e) {
					DebugLog += e.Message;
					Status = "Error!";
					return;
				}

				Messages.ServerAddress = server.Item1;
				Messages.ServerPort = server.Item2;
				if (string.IsNullOrEmpty(Messages.ServerAddress) || Messages.ServerPort < 0) {
					Status = "MainServer didn't give correct GameServerAddress or GameServerPort";
					return;
				}

				Status = "Server found!";
				var mainWindow = StaticResolver.Resolve<IWindowManager>().GetView(this);
				mainWindow.Dispatcher.Invoke(() => {
					mainWindow.Visibility = Visibility.Collapsed;
					_game = new MainGame();
					_game.Scale = WindowScale;
					_game.Run();
					_game.Dispose();
					mainWindow.Visibility = Visibility.Visible;
					mainWindow.Focus();
				});
				Status = "";
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
				mainWindow.Dispatcher.Invoke(() => {
					mainWindow.Visibility = Visibility.Collapsed;
					_game = new MainGame();
					_game.Scale = WindowScale;
					_game.Run();
					_game.Dispose();
					mainWindow.Visibility = Visibility.Visible;
					mainWindow.Focus();
				});
			}).ContinueWith((t) => {
				GUIEnabled = true;
			});
		}

		private (string, int) GetServer()
        {
	        try {
		        using TcpClient client = new TcpClient(ConfigurationManager.AppSettings["ServerMatchmakingAddress"]!, int.Parse(ConfigurationManager.AppSettings["ServerMatchmakingPort"]!));
		        using var streamReader = new BinaryReader(client.GetStream());
				using var streamWriter = new BinaryWriter(client.GetStream());
				streamWriter.Write(1);
				string address = streamReader.ReadString();
				int port = streamReader.ReadInt32();
				return (address, port);
	        }
	        catch (Exception e)
	        {
		        Console.WriteLine(e);
	        }

	        return (null, -1);
        }
	}
}