using Android.App;
using Android.Content.PM;
using Android.OS;
using Avalonia.Android;

[assembly: MetaData("com.google.android.maps.v2.API_KEY", Value = "AIzaSyAeQUFd_NSAa7OrrNvVqz6XDTTXe2zrQe8")]

namespace MapSample.Android;

[Activity(Label = "MapSample.Android", Theme = "@style/MyTheme.NoActionBar", Icon = "@drawable/icon",
    LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
public class MainActivity : AvaloniaMainActivity
{
}