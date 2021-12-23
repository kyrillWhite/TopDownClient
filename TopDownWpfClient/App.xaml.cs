using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.AnyContainer;
using TopDownWpfClient.Models;
using TopDownWpfClient.Services.Windows;
using TopDownWpfClient.ViewModels;

namespace TopDownWpfClient
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			Ioc.SetUp();

			var mainVM = new MainWindowViewModel();
			StaticResolver.Resolve<IWindowManager>().OpenWindow(mainVM);

			base.OnStartup(e);
		}
	}
}
