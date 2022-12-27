using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Iratrips.MapKit;

namespace MapSample.ViewModels;

public class MainViewModel : ViewModelBase
{
    public string Greeting => "Welcome to Avalonia!";

    public ObservableCollection<MapPin> Pins => new ObservableCollection<MapPin>()
    {
        new MapPin()
        {
            ID = "1",
            Image =  LoadAsset("location_pin.png"),
            Position = new Position(18.5204, 73.8567),
            DefaultPinColor = Colors.Yellow
        }
    };

    private static Stream LoadAsset(string file)
    {
        var assemblyName = Assembly.GetAssembly(typeof(MainViewModel))?.GetName()?.Name;
        if (assemblyName == null)
            throw new Exception("Unable to find current assembly.");
        
        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
        if (assets == null)
            throw new Exception("Unable to find assets loader.");
        
        var uri = new Uri($"avares://{assemblyName}/{AssetPath(file)}");
        return assets.Open(uri);
    }
    
    private static string AssetPath(string file) => $"Assets/{file}";
}