using System;
using System.Windows.Controls;
using System.Windows.Input;
using TopDownWpfClient.Models;
using TopDownWpfClient.Views;

namespace TopDownWpfClient.ViewModels {
	public class StartPageViewModel : BaseViewModel {

		public ICommand PlayCommand { get; set; }

		public event Action PlayEvent;

		public ICommand ExitCommand { get; set; }

		public event Action ExitEvent;

		public ICommand SettingsCommand { get; set; }

		public event Action SettingsEvent;

		public StartPageViewModel() {
			// PlayCommand = new DelegateCommand<object>(_ => Play());
			PlayCommand = new DelegateCommand<object>(_ => PlayEvent?.Invoke());
			ExitCommand = new DelegateCommand<object>(_ => ExitEvent?.Invoke());
			SettingsCommand = new DelegateCommand<object>(_ => SettingsEvent?.Invoke());
		}
	}
}