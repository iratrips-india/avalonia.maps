using Android.App;
using Android.Content;
using Iratrips.MapKit;
using Iratrips.MapKit.Droid;
using Iratrips.MapKit.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Iratrips.Mapkit.Android;

public static class Platform
{
    internal static Context AppContext { get; set; }
    
    public static void Init(Context context)
    {
        Platform.AppContext = context;
        PlatformInitialization.Platform = new MapPlatform();
    }
}

public class MapPlatform : IMapPlatform
{
    public IRendererFunctions GetAdaptor(MapControl mapControl)
    {
        GoogleMaps.Init(Platform.AppContext, null);
        return new MapControlAdaptor(mapControl, Platform.AppContext);
    }
}