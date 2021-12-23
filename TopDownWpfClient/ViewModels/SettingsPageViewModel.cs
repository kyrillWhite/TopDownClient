using System;
using System.Windows.Input;
using TopDownWpfClient.Models;

namespace TopDownWpfClient.ViewModels {
	public class SettingsPageViewModel : BaseViewModel {
		public ICommand BackCommand { get; set; }

		public event Action BackEvent;

		public MainWindowViewModel MainWindowViewModel => MainWindowViewModel.Instance;

		public SettingsPageViewModel()
		{
			BackCommand = new DelegateCommand<object>(_ => BackEvent?.Invoke());
		}
	}
}