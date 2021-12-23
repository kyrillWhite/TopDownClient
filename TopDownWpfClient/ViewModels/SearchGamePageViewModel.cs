using System;
using System.Windows.Controls;
using System.Windows.Input;
using TopDownWpfClient.Models;

namespace TopDownWpfClient.ViewModels {
	public class SearchGamePageViewModel : BaseViewModel {
		public ICommand SearchGameCommand { get; set; }

		public event Action SearchGameEvent;

		public ICommand BackCommand { get; set; }

		public event Action BackEvent;


		public SearchGamePageViewModel() {
			SearchGameCommand = new DelegateCommand<object>(_ => SearchGameEvent?.Invoke());
			BackCommand = new DelegateCommand<object>(_ => BackEvent?.Invoke());
		}
	}
}