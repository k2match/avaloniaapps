using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using k2audio.ViewModels;
using k2audio.Views;

namespace k2audio;

public partial class App : Application
{
	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);
	}

	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			MainViewModel vm = new MainViewModel();
			desktop.MainWindow = new MainWindow
			{
				DataContext = vm,
			};

			var window = desktop.MainWindow;
			window.Closing += (s, e) =>
			{
				//e.Cancel = true;
				vm.Terminate();
			};
			vm.Initialize();
		}

		base.OnFrameworkInitializationCompleted();
	}
}
