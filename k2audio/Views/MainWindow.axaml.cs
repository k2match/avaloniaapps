using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Messaging;
using k2audio.Models;
using k2audio.ViewModels;

namespace k2audio.Views;

public partial class MainWindow : Window
{
	private MainViewModel? _vm = null;

	public MainWindow()
	{
		InitializeComponent();

		// メッセージを受信してWindowを閉じる
		WeakReferenceMessenger.Default.Register<CloseWindowMessage>(this, (recipient, message) =>
		{
			this.Close();
		});

		// 設定ウィンドウを表示
		WeakReferenceMessenger.Default.Register<OpenConfigureWindowMessage>(this, (recipient, message) =>
		{
			ConfigureWindow wnd = new();
			wnd.ShowDialog(this);
		});

		// XAML またはコードビハインドで設定
		//_playPosition.AddHandler(InputElement.PointerCaptureLostEvent, (sender, e) =>
		//{
		//	// ユーザーがドラッグ・クリックを離した瞬間に実行
		//	System.Console.WriteLine($"ユーザー操作完了: {_playPosition.Value}");
		//}, RoutingStrategies.Bubble);
	}

	protected override void OnOpened(EventArgs e)
	{
		base.OnOpened(e);

		System.Console.WriteLine("MainWindow OnOpened");

		_vm = this.DataContext as MainViewModel;

		// 状態を復元
		ThisState state = ThisApp.Instance.State;
		if (state.Enabled)
		{
			this.Position = new Avalonia.PixelPoint(state.Left, state.Top);
			this.Width = state.Width;
			this.Height = state.Height;
			if (Enum.TryParse<WindowState>(state.WindowState, out WindowState st))
			{
				this.WindowState = st;
			}
			_vm?.RestoreState(state);
		}
	}

	protected override void OnClosing(WindowClosingEventArgs e)
	{
		base.OnClosing(e);

		System.Console.WriteLine("MainWindow OnClosing");

		// 状態を保存
		ThisState state = ThisApp.Instance.State;
		state.WindowState = this.WindowState.ToString();
		state.Left = this.Position.X;
		state.Top = this.Position.Y;
		state.Width = this.Width;
		state.Height = this.Height;
		_vm?.SaveState(state);
		ThisState.SaveToFile(state);
	}

	private void Position_PointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
	{
		System.Console.WriteLine($"Slider PointerCaptureLost: {_playPosition.Value}");

		_vm?.Seek(_playPosition.Value);
	}

	private void Volume_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
	{
		// e.OldValue gives you the previous number
		// e.NewValue gives you the current number
		double newValue = e.NewValue;

		System.Console.WriteLine($"Volume Changed: {newValue}");
	}

	/*private void Slider_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
	{
		if (_vm is null || _vm.IsPositionUpdating) return;

		// e.OldValue gives you the previous number
		// e.NewValue gives you the current number
		double newValue = e.NewValue;

		System.Console.WriteLine($"Slider value changed to: {newValue}");
		//_vm?.Seek(newValue);
	}*/
}
