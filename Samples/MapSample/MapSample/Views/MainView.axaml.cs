using System;
using Avalonia.Controls;
using Iratrips.MapKit;

namespace MapSample.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void MapControl_OnMapReady(object? sender, EventArgs e)
    {
        if (sender is not MapControl map) return;
        
        var mapSpan = MapSpan.FromCenterAndRadius(new Position(18.5204, 73.8567), Distance.FromMiles(0.25));
        map.MoveToMapRegion(mapSpan);
    }

    private void MapControl_OnPinSelected(object? sender, GenericEventArgs<MapPin> e)
    {
        var pin = e.Value;
    }
}