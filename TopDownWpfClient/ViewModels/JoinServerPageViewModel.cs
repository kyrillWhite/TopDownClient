using System;
using System.Windows.Input;
using TopDownWpfClient.Models;

namespace TopDownWpfClient.ViewModels {
	public class JoinServerPageViewModel : BaseViewModel {
		public ICommand BackCommand { get; set; }

		public event Action BackEvent;

		public MainWindowViewModel MainWindowViewModel => MainWindowViewModel.Instance;

		public ICommand JoinCommand { get; set; }

		public event EventHandler<(string, int)> JoinedServer;

		public string IP { get; set; }
		public int Port { get; set; }

		public JoinServerPageViewModel()
		{
			BackCommand = new DelegateCommand<object>(_ => BackEvent?.Invoke());
			JoinCommand = new DelegateCommand<object>(_ => JoinedServer?.Invoke(this, (IP, Port)), _ => !string.IsNullOrEmpty(IP));
		}
	}
}