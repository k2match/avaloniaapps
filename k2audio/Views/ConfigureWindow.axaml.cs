using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using k2audio.ViewModels;

namespace k2audio.Views;

public partial class ConfigureWindow : Window
{
	public ConfigureWindow()
	{
		//InitializeComponent();

		// メッセージを受信してWindowを閉じる
		WeakReferenceMessenger.Default.Register<CloseWindowMessage>(this, (recipient, message) =>
		{
			//WeakReferenceMessenger.Default.Unregister<CloseWindowMessage>(message);
			this.Close();
		});
	}
}
