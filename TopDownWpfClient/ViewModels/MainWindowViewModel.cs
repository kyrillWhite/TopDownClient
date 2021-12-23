using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

		public Page CurrentPage { get; set; }

		MyServiceClient client = null;

		public MainWindowViewModel() {
			var pageManager = StaticResolver.Resolve<IPageManager>();
			_startPageVM = new StartPageViewModel();
			_searchGamePageVM = new SearchGamePageViewModel();
			_startPageVM.PlayEvent += SwitchPageToSearchGamePage;
			_startPageVM.ExitEvent += KillApp;
			_searchGamePageVM.BackEvent += SwitchPageToStartPage;
			_searchGamePageVM.SearchGameEvent += SearchGame;

			StartPage = pageManager.CreatePage(_startPageVM);
			SearchGamePage = pageManager.CreatePage(_searchGamePageVM);

			CurrentPage = StartPage;
		}

		private void SwitchPageToSearchGamePage() {
			CurrentPage = SearchGamePage;
		}

		private void SwitchPageToStartPage() {
			CurrentPage = StartPage;
		}

		private void KillApp() {
			Application.Current.Shutdown();
		}

		private void SearchGame() {
			//Get ServerAddress and ServerPort from TopDownMainServer
			(string, int) server;
			try {
				server = GetServer();
			} catch {
				MessageBox.Show("Can't find server");
				return;
			}

			Messages.ServerAddress = server.Item1;
			// Messages.ServerAddress = "26.202.152.148";
			Messages.ServerPort = server.Item2;
			Messages.ServerPort = 5000; //TODO: доделать
			// Messages.ServerPort = "5000";
			//Open game with acquired ServerAddress and ServerPort 
			using (var game = new MainGame()) {
				StaticResolver.Resolve<IWindowManager>().GetView(this).Visibility = Visibility.Collapsed;
				game.Run();
				StaticResolver.Resolve<IWindowManager>().GetView(this).Visibility = Visibility.Visible;
			}
		}

		

        private (string, int) GetServer()
        {
            try
            {
				var serviceClient = new MyServiceClient(MyServiceClient.EndpointConfiguration.BasicHttpBinding_IMyService);
				var serverResult = serviceClient.GetAvailableServerAsync();
				serverResult.Wait();
				return serverResult.Result;
            }
            catch (Exception ex)
            {
	            Console.WriteLine(ex);
	            Console.WriteLine(ex.InnerException);
	            MessageBox.Show(ex.Message);
                client = null;
            }

            throw new Exception();
        }
    }
}