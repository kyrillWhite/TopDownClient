using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.AnyContainer;
using TopDownWpfClient.Services.Pages;

namespace TopDownWpfClient.ViewModels {
	public class MainWindowViewModel : BaseViewModel {
		private StartPageViewModel _startPageVM;
		public Page StartPage { get; set; }

		private SearchGamePageViewModel _searchGamePageVM;
		public Page SearchGamePage { get; set; }

		public Page CurrentPage { get; set; }

		public MainWindowViewModel() {
			var pageManager = StaticResolver.Resolve<IPageManager>();
			_startPageVM = new StartPageViewModel();
			_searchGamePageVM = new SearchGamePageViewModel();
			_startPageVM.PlayEvent += SwitchPageToSearchGamePage;
			_searchGamePageVM.BackEvent += SwitchPageToStartPage;

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
	}
}