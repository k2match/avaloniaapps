using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;

namespace devtools.Android;

[Activity(
    Label = "devtools.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
}
