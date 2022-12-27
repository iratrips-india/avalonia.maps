namespace Iratrips.MapKit.Interfaces;

public interface IMapPlatform
{
    IRendererFunctions GetAdaptor(MapControl mapControl);
}