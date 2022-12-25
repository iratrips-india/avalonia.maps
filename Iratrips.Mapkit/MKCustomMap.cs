using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Iratrips.MapKit.Interfaces;
using Iratrips.MapKit.Models;
using Iratrips.MapKit.Overlays;
using Avalonia;
using Avalonia.Controls;

namespace Iratrips.MapKit
{
    /// <summary>
    /// An extensions of the <see cref="Avalonia.Controls.NativeControlHost"/>
    /// </summary>
    public class MKCustomMap : NativeControlHost, IMapFunctions
    {
        /// <summary>
        /// Event raised when a pin gets selected
        /// </summary>
        public event EventHandler<GenericEventArgs<MKCustomMapPin>> PinSelected;
        /// <summary>
        /// Event raised when a drag of a pin ended
        /// </summary>
        public event EventHandler<GenericEventArgs<MKCustomMapPin>> PinDragEnd;
        /// <summary>
        /// Event raised when an area of the map gets clicked
        /// </summary>
        public event EventHandler<GenericEventArgs<Position>> MapClicked;
        /// <summary>
        /// Event raised when an area of the map gets long-pressed
        /// </summary>
        public event EventHandler<GenericEventArgs<Position>> MapLongPress;
        /// <summary>
        /// Event raised when the location of the user changes
        /// </summary>
        public event EventHandler<GenericEventArgs<Position>> UserLocationChanged;
        /// <summary>
        /// Event raised when a route gets tapped
        /// </summary>
        public event EventHandler<GenericEventArgs<MKRoute>> RouteClicked;
        /// <summary>
        /// Event raised when a route calculation finished successfully
        /// </summary>
        public event EventHandler<GenericEventArgs<MKRoute>> RouteCalculationFinished;
        /// <summary>
        /// Event raised when a route calculation failed
        /// </summary>
        public event EventHandler<GenericEventArgs<RouteCalculationError>> RouteCalculationFailed;
        /// <summary>
        /// Event raised when all pins are added to the map initially
        /// </summary>
        public event EventHandler PinsReady;
        /// <summary>
        /// Event raised when a callout got tapped
        /// </summary>
        public event EventHandler<GenericEventArgs<MKCustomMapPin>> CalloutClicked;
        /// <summary>
        /// Event raised when map is ready
        /// </summary>
        public event EventHandler MapReady;
        /// <summary>
        /// Event raised when camera movement has ended, there are no pending animations and the user has stopped interacting with the map.
        /// </summary>
        public event EventHandler CameraIdeal;

        /// <summary>
        /// Event raised when the camera starts moving after it has been idle or when the reason for camera motion has changed.
        /// </summary>
        public event EventHandler CameraMoveStarted;

        public static readonly DirectProperty<MKCustomMap, IRendererFunctions> MapFunctionsProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, IRendererFunctions>(nameof(MapFunctions),
                o => o.MapFunctions,
                (o, v) => o.MapFunctions = v,
                defaultBindingMode: Avalonia.Data.BindingMode.OneWayToSource);

        /// <summary>
        /// Bindable Property of <see cref="Pins" />
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, IEnumerable<MKCustomMapPin>> PinsProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, IEnumerable<MKCustomMapPin>>(nameof(Pins),
            o => o.Pins,
            (o, v) => o.Pins = v);

        /// <summary>
        /// Bindable Property of <see cref="SelectedPin" />
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, MKCustomMapPin> SelectedPinProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, MKCustomMapPin>(nameof(SelectedPin),
            o => o.SelectedPin,
            (o, v) => o.SelectedPin = v,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        /// <summary>
        /// Bindable Property of <see cref="PinSelectedCommand" />
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, ICommand> PinSelectedCommandProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, ICommand>(nameof(PinSelectedCommand),
            o => o.PinSelectedCommand,
            (o, v) => o.PinSelectedCommand = v);

        /// <summary>
        /// Bindable Property of <see cref="MapClickedCommand"/>
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, ICommand> MapClickedCommandProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, ICommand>(nameof(MapClickedCommand),
            o => o.MapClickedCommand,
            (o, v) => o.MapClickedCommand = v);

        /// <summary>
        /// Bindable Property of <see cref="MapLongPressCommand"/>
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, ICommand> MapLongPressCommandProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, ICommand>(nameof(MapLongPressCommand),
            o => o.MapLongPressCommand,
            (o, v) => o.MapLongPressCommand = v);

        /// <summary>
        /// Bindable Property of <see cref="PinDragEndCommand"/>
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, ICommand> PinDragEndCommandProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, ICommand>(nameof(PinDragEndCommand),
            o => o.PinDragEndCommand,
            (o, v) => o.PinDragEndCommand = v);

        /// <summary>
        /// Bindable Property of <see cref="PinsReadyCommand"/>
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, ICommand> PinsReadyCommandProperty 
            = AvaloniaProperty.RegisterDirect<MKCustomMap, ICommand>(nameof(PinsReadyCommand),
            o => o.PinsReadyCommand,
            (o, v) => o.PinsReadyCommand = v);


        /// <summary>
        /// Bindable Property of <see cref="MapCenter"/>
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, Position> MapCenterProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, Position>(nameof(MapCenter),
            o => o.MapCenter,
            unsetValue: default(Position));

        /// <summary>
        /// Bindable Property of <see cref="IsRegionChangeAnimated"/>
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, bool> IsRegionChangeAnimatedProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, bool>(nameof(IsRegionChangeAnimated),
            o => o.IsRegionChangeAnimated,
            (o, v) => o.IsRegionChangeAnimated = v,
            unsetValue: default(bool));

        /// <summary>
        /// Bindable Property of <see cref="ShowTraffic"/>
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, bool> ShowTrafficProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, bool>(nameof(ShowTraffic),
            o => o.IsRegionChangeAnimated,
            (o, v) => o.IsRegionChangeAnimated = v,
            unsetValue: default(bool));

        /// <summary>
        /// Bindable Property of <see cref="Routes"/>
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, IEnumerable<MKPolyline>?> PolylinesProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, IEnumerable<MKPolyline>?>(nameof(Polylines),
            o => o.Polylines,
            (o, v) => o.Polylines = v,
            unsetValue: null);

        /// <summary>
        /// Bindable Property of <see cref="Circles"/>
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, IEnumerable<MKCircle>> CirclesProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, IEnumerable<MKCircle>>(nameof(Circles),
            o => o.Circles,
            (o, v) => o.Circles = v,
            unsetValue: new List<MKCircle>());

        /// <summary>
        /// Bindable Property of <see cref="CalloutClickedCommand"/>
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, ICommand> CalloutClickedCommandProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, ICommand>(nameof(CalloutClickedCommand),
            o => o.CalloutClickedCommand,
            (o, v) => o.CalloutClickedCommand = v);

        /// <summary>
        /// Bindable Property of <see cref="Polygons"/>
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, IEnumerable<MKPolygon>> PolygonsProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, IEnumerable<MKPolygon>>(nameof(Polygons),
            o => o.Polygons,
            (o, v) => o.Polygons = v);

        /// <summary>
        /// Bindable Property of <see cref="MapRegion"/>
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, MapSpan> MapRegionProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, MapSpan>(nameof(MapRegion),
            o => o.MapRegion,
            (o, v) => o.MapRegion = v,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        /// <summary>
        /// Bindable Property of <see cref="Routes"/>
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, IEnumerable<MKRoute>> RoutesProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, IEnumerable<MKRoute>>(nameof(Routes),
            o => o.Routes,
            (o, v) => o.Routes = v);

        /// <summary>
        /// Bindable Property of <see cref="RouteClickedCommand"/>
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, ICommand> RouteClickedCommandProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, ICommand>(nameof(RouteClickedCommand),
            o => o.RouteClickedCommand,
            (o, v) => o.RouteClickedCommand = v);

        /// <summary>
        /// Bindable Property of <see cref="RouteCalculationFinishedCommand"/>
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, ICommand> RouteCalculationFinishedCommandProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, ICommand>(nameof(RouteCalculationFinishedCommand),
            o => o.RouteCalculationFinishedCommand,
            (o, v) => o.RouteCalculationFinishedCommand = v);

        /// <summary>
        /// Bindable Property of <see cref="RouteCalculationFailedCommand"/>
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, ICommand> RouteCalculationFailedCommandProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, ICommand>(nameof(RouteCalculationFailedCommand),
                o => o.RouteCalculationFinishedCommand,
                (o, v) => o.RouteCalculationFinishedCommand = v);

        /// <summary>
        /// Bindable Property of <see cref="TilesUrlOptions"/>
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, MKTileUrlOptions> TilesUrlOptionsProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, MKTileUrlOptions>(nameof(TilesUrlOptions),
                o => o.TilesUrlOptions,
                (o, v) => o.TilesUrlOptions = v);

        /// <summary>
        /// Bindable Property of <see cref="UserLocationChangedCommand"/>
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, ICommand> UserLocationChangedCommandProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, ICommand>(nameof(UserLocationChangedCommand),
                o => o.RouteCalculationFinishedCommand,
                (o, v) => o.RouteCalculationFinishedCommand = v);

        /// <summary>
        /// Bindable Property of <see cref="GetCalloutView"/>
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, Func<MKCustomMapPin, NativeControlHost>> GetCalloutViewProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, Func<MKCustomMapPin, NativeControlHost>>(nameof(GetCalloutView),
                o => o.GetCalloutView,
                    (o, v) => o.GetCalloutView = v);

        /// <summary>
        /// Bindable Property of <see cref="GetClusteredPin"/>
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, Func<string, IEnumerable<MKCustomMapPin>, MKCustomMapPin>> GetClusteredPinProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, Func<string, IEnumerable<MKCustomMapPin>, MKCustomMapPin>>(nameof(GetClusteredPin),
                o => o.GetClusteredPin,
                (o, v) => o.GetClusteredPin = v);

        /// <summary>
        /// Bindable property of <see cref="IsClusteringEnabled"/>
        /// </summary>
        public static DirectProperty<MKCustomMap, bool> IsClusteringEnabledProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, bool>(nameof(IsClusteringEnabled),
                o => o.IsClusteringEnabled,
                (o, v) => o.IsClusteringEnabled = v,
                unsetValue: true);
        /// <summary>
        /// Binadble property of <see cref="MapType"/>
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, MapType> MapTypeProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, MapType>(nameof(MapType),
                o => o.MapType,
                (o, v) => o.MapType = v,
                unsetValue: default(MapType));
        /// <summary>
        /// Binadble property of <see cref="IsShowingUser"/>
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, bool> IsShowingUserProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, bool>(nameof(IsShowingUser),
                o => o.IsShowingUser,
                (o, v) => o.IsShowingUser = v,
                unsetValue: default(bool));

        /// <summary>
        /// Binadble property of <see cref="HasScrollEnabled"/>
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, bool> HasScrollEnabledProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, bool>(nameof(HasScrollEnabled),
                o => o.HasScrollEnabled,
                (o, v) => o.HasScrollEnabled = v,
                unsetValue: true);

        /// <summary>
        /// Binadble property of <see cref="HasZoomEnabled"/>
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, bool> HasZoomEnabledProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, bool>(nameof(HasZoomEnabled),
                o => o.HasZoomEnabled,
                (o, v) => o.HasZoomEnabled = v,
                unsetValue: true);
        /// <summary>
        /// Binadble property of <see cref="MapReadyCommand"/>
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, ICommand> MapReadyCommandProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, ICommand>(nameof(MapReadyCommand),
                o => o.MapReadyCommand,
                (o, v) => o.MapReadyCommand = v,
                unsetValue: default(ICommand));
        /// <summary>
        /// Binadble property of <see cref="CameraIdealCommand"/>
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, ICommand> CameraIdealCommandProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, ICommand>(nameof(CameraIdealCommand),
                o => o.CameraIdealCommand,
                (o, v) => o.CameraIdealCommand = v,
        unsetValue: default(ICommand));

        /// <summary>
        /// Binadble property of <see cref="CameraMoveStartedCommand"/>
        /// </summary>
        public static readonly DirectProperty<MKCustomMap, ICommand> CameraMoveStartedCommandProperty
            = AvaloniaProperty.RegisterDirect<MKCustomMap, ICommand>(nameof(CameraMoveStartedCommand),
                o => o.CameraMoveStartedCommand,
                (o, v) => o.CameraMoveStartedCommand = v,
                unsetValue: default(ICommand));

        private ICommand _cameraIdealCommand;
        /// <summary>
        /// Gets/Sets the command which is raised when the camera is ideal
        /// </summary>
        public ICommand CameraIdealCommand
        {
            get => _cameraIdealCommand;
            set => SetAndRaise(CameraIdealCommandProperty, ref _cameraIdealCommand, value);
        }

        private ICommand _cameraMoveStartedCommand;
        /// <summary>
        /// Gets/Sets the command which is raised when the camera move started
        /// </summary>
        public ICommand CameraMoveStartedCommand
        {
            get => _cameraMoveStartedCommand;
            set => SetAndRaise(CameraMoveStartedCommandProperty, ref _cameraMoveStartedCommand, value);
        }

        private ICommand _mapReadyCommand;
        /// <summary>
        /// Gets/Sets the command which is raised when the map is ready
        /// </summary>
        public ICommand MapReadyCommand
        {
            get => _mapReadyCommand;
            set => SetAndRaise(MapReadyCommandProperty, ref _mapReadyCommand, value);
        }

        private MapType _mapType;
        /// <summary>
        /// Gets/Sets the current <see cref="MapType"/>
        /// </summary>
        public MapType MapType
        {
            get => _mapType;
            set => SetAndRaise(MapTypeProperty, ref _mapType, value);
        }

        private bool _isShowingUser;
        /// <summary>
        /// Gets/Sets if the user should be displayed on the map
        /// </summary>
        public bool IsShowingUser
        {
            get => _isShowingUser;
            set => SetAndRaise(IsShowingUserProperty, ref _isShowingUser, value);
        }

        private bool _hasScrollEnabled;
        /// <summary>
        /// Gets/Sets whether scrolling is enabled or not
        /// </summary>
        public bool HasScrollEnabled
        {
            get => _hasScrollEnabled;
            set => SetAndRaise(HasScrollEnabledProperty, ref _hasScrollEnabled, value);
        }

        private bool _hasZoomEnabled;
        /// <summary>
        /// Gets/Sets whether zooming is enabled or not
        /// </summary>
        public bool HasZoomEnabled
        {
            get => _hasZoomEnabled;
            set => SetAndRaise(HasZoomEnabledProperty, ref _hasZoomEnabled, value);
        }

        private IEnumerable<MKCustomMapPin> _pins;
        /// <summary>
        /// Gets/Sets the custom pins of the Map
        /// </summary>
        public IEnumerable<MKCustomMapPin> Pins
        {
            get => _pins;
            set => SetAndRaise(PinsProperty, ref _pins, value);
        }

        private MKCustomMapPin _selectedPin;
        /// <summary>
        /// Gets/Sets the currently selected pin on the map
        /// </summary>
        public MKCustomMapPin SelectedPin
        {
            get => _selectedPin;
            set => SetAndRaise(SelectedPinProperty, ref _selectedPin, value);
        }

        private ICommand _mapClickedCommand;
        /// <summary>
        /// Gets/Sets the command when the map was clicked/tapped
        /// </summary>
        public ICommand MapClickedCommand
        {
            get => _mapClickedCommand;
            set => SetAndRaise(MapClickedCommandProperty, ref _mapClickedCommand, value);
        }

        private ICommand _mapLongPressCommand;
        /// <summary>
        /// Gets/Sets the command when a long press was performed on the map
        /// </summary>
        public ICommand MapLongPressCommand
        {
            get => _mapLongPressCommand;
            set => SetAndRaise(MapLongPressCommandProperty, ref _mapLongPressCommand, value);
        }

        private ICommand _pinDragEndCommand;
        /// <summary>
        /// Gets/Sets the command when a pin drag ended. The pin already has the updated position set
        /// </summary>
        public ICommand PinDragEndCommand
        {
            get => _pinDragEndCommand;
            set => SetAndRaise(PinDragEndCommandProperty, ref _pinDragEndCommand, value);
        }

        private ICommand _pinSelectedCommand;
        /// <summary>
        /// Gets/Sets the command when a pin got selected
        /// </summary>
        public ICommand PinSelectedCommand
        {
            get => _pinSelectedCommand;
            set => SetAndRaise(PinSelectedCommandProperty, ref _pinSelectedCommand, value);
        }

        private ICommand _pinsReadyCommand;
        /// <summary>
        /// Gets/Sets the command when the pins are ready
        /// </summary>
        public ICommand PinsReadyCommand
        {
            get => _pinsReadyCommand;
            set => SetAndRaise(PinsReadyCommandProperty, ref _pinsReadyCommand, value);
        }

        /// <summary>
        /// Gets/Sets the current center of the map.
        /// </summary>
        public Position MapCenter => MapRegion.Center;

        private bool _isRegionChangeAnimated;
        /// <summary>
        /// Gets/Sets if a change <see cref="MapRegion"/> should be animated
        /// </summary>
        public bool IsRegionChangeAnimated
        {
            get => _isRegionChangeAnimated;
            set => SetAndRaise(IsRegionChangeAnimatedProperty, ref _isRegionChangeAnimated, value);
        }

        private IEnumerable<MKPolyline> _polylines;
        /// <summary>
        /// Gets/Sets the lines to display on the map
        /// </summary>
        public IEnumerable<MKPolyline> Polylines
        {
            get => _polylines;
            set => SetAndRaise(PolylinesProperty, ref _polylines, value);
        }

        private IEnumerable<MKCircle> _circles;
        /// <summary>
        /// Gets/Sets the circles to display on the map
        /// </summary>
        public IEnumerable<MKCircle> Circles
        {
            get => _circles;
            set => SetAndRaise(CirclesProperty, ref _circles, value);
        }

        private ICommand _calloutClickedCommand;
        /// <summary>
        /// Gets/Sets the command when a callout gets clicked. When this is set, there will be an accessory button visible inside the callout on iOS.
        /// Android will simply raise the command by clicking anywhere inside the callout, since Android simply renders a bitmap
        /// </summary>
        public ICommand CalloutClickedCommand
        {
            get => _calloutClickedCommand;
            set => SetAndRaise(CalloutClickedCommandProperty, ref _calloutClickedCommand, value);
        }

        private IEnumerable<MKPolygon> _polygons;
        /// <summary>
        /// Gets/Sets the rectangles to display on the map
        /// </summary>
        public IEnumerable<MKPolygon> Polygons
        {
            get => _polygons;
            set => SetAndRaise(PolygonsProperty, ref _polygons, value);
        }

        private MapSpan _mapRegion;
        /// <summary>
        /// Gets/Sets the visible map region
        /// </summary>
        public MapSpan MapRegion
        {
            get => _mapRegion;
            set => SetAndRaise(MapRegionProperty, ref _mapRegion, value);
        }

        private IEnumerable<MKRoute> _routes;
        /// <summary>
        /// Gets/Sets the routes to calculate and display on the map
        /// </summary>
        public IEnumerable<MKRoute> Routes
        {
            get => _routes;
            set => SetAndRaise(RoutesProperty, ref _routes, value);
        }

        private ICommand _routeClickedCommand;
        /// <summary>
        /// Gets/Sets the command when a route gets tapped
        /// </summary>
        public ICommand RouteClickedCommand
        {
            get => _routeClickedCommand;
            set => SetAndRaise(RouteClickedCommandProperty, ref _routeClickedCommand, value);
        }

        private ICommand _routeCalculationFinishedCommand;
        /// <summary>
        /// Gets/Sets the command when a route calculation finished successfully
        /// </summary>
        public ICommand RouteCalculationFinishedCommand
        {
            get => _routeCalculationFinishedCommand;
            set => SetAndRaise(RouteCalculationFinishedCommandProperty, ref _routeCalculationFinishedCommand, value);
        }

        private ICommand _routeCalculationFailedCommand;
        /// <summary>
        /// Gets/Sets the command when a route calculation failed
        /// </summary>
        public ICommand RouteCalculationFailedCommand
        {
            get => _routeCalculationFailedCommand;
            set => SetAndRaise(RouteCalculationFailedCommandProperty, ref _routeCalculationFailedCommand, value);
        }

        private MKTileUrlOptions _tilesUrlOptions;
        /// <summary>
        /// Gets/Sets the options for displaying custom tiles via an url
        /// </summary>
        public MKTileUrlOptions TilesUrlOptions
        {
            get => _tilesUrlOptions;
            set => SetAndRaise(TilesUrlOptionsProperty, ref _tilesUrlOptions, value);
        }

        private ICommand _userLocationChangedCommand;
        /// <summary>
        /// Gets/Sets the command when the user location changed
        /// </summary>
        public ICommand UserLocationChangedCommand
        {
            get => _userLocationChangedCommand;
            set => SetAndRaise(UserLocationChangedCommandProperty, ref _userLocationChangedCommand, value);
        }

        private IRendererFunctions _mapFunctions;
        /// <summary>
        /// Gets/Sets the avaiable functions on the map/renderer
        /// </summary>
        public IRendererFunctions MapFunctions
        {
            get => _mapFunctions;
            set => SetAndRaise(MapFunctionsProperty, ref _mapFunctions, value);
        }

        private bool _showTraffic;
        /// <summary>
        /// Gets/Sets if traffic information should be displayed
        /// </summary>
        public bool ShowTraffic
        {
            get => _showTraffic;
            set => SetAndRaise(ShowTrafficProperty, ref _showTraffic, value);
        }

        private Func<MKCustomMapPin, NativeControlHost> _getCalloutView;
        /// <summary>
        /// Gets/Sets function to retrieve a callout view. 
        /// </summary>
        public Func<MKCustomMapPin, NativeControlHost> GetCalloutView
        {
            get => _getCalloutView;
            set => SetAndRaise(GetCalloutViewProperty, ref _getCalloutView, value);
        }

        private Func<string, IEnumerable<MKCustomMapPin>, MKCustomMapPin> _getClusteredPin;
        /// <summary>
        /// Gets/Sets function to retrieve a pin for clustering. You receive the group name and all pins getting clustered. 
        /// </summary>
        public Func<string, IEnumerable<MKCustomMapPin>, MKCustomMapPin> GetClusteredPin
        {
            get => _getClusteredPin;
            set => SetAndRaise(GetClusteredPinProperty, ref _getClusteredPin, value);
        }

        private bool _isClusteringEnabled;
        /// <summary>
        /// Gets/Sets whether clustering is enabled or not
        /// </summary>
        public bool IsClusteringEnabled
        {
            get => _isClusteringEnabled;
            set => SetAndRaise(IsClusteringEnabledProperty, ref _isClusteringEnabled, value);
        }
        
        /// <summary>
        /// Creates a new instance of <c>MKCustomMap</c>
        /// </summary>
        public MKCustomMap()
            : base()
        {
            MapRegion = MapSpan.FromCenterAndRadius(new Position(40.7142700, -74.0059700), Distance.FromKilometers(2));
        }
        
        /// <summary>
        /// Creates a new instance of <c>MKCustomMap</c>
        /// </summary>
        /// <param name="region">The initial region of the map</param>
        public MKCustomMap(MapSpan region)
        {
            MapRegion = region;
        }
        
        /// <summary>
        /// Creates a new instance of <see cref="MKCustomMap"/>
        /// </summary>
        /// <param name="initialLatitude">The initial latitude value</param>
        /// <param name="initialLongitude">The initial longitude value</param>
        /// <param name="distanceInKilometers">The initial zoom distance in kilometers</param>
        public MKCustomMap(double initialLatitude, double initialLongitude, double distanceInKilometers) :
            this(MapSpan.FromCenterAndRadius(new Position(initialLatitude, initialLongitude), Distance.FromKilometers(distanceInKilometers)))
        {
        }
        
        /// <summary>
        /// Returns the currently visible map as a PNG image
        /// </summary>
        /// <returns>Map as image</returns>
        public async Task<byte[]> GetSnapshot() => await MapFunctions.GetSnapshot();
        
        /// <summary>
        /// Moves the visible region to the specified <see cref="MapSpan"/>
        /// </summary>
        /// <param name="region">Region to move the map to</param>
        /// <param name="animate">If the region change should be animated or not</param>
        public void MoveToMapRegion(MapSpan region, bool animate = false) => MapFunctions.MoveToMapRegion(region, animate);
        
        /// <summary>
        /// Fits the map region to make all given positions visible
        /// </summary>
        /// <param name="positions">Positions to fit inside the MapRegion</param>
        /// <param name="animate">If the camera change should be animated</param>
        public void FitMapRegionToPositions(IEnumerable<Position> positions, bool animate = false, int padding = 0) => MapFunctions.FitMapRegionToPositions(positions, animate, padding);
        
        /// <summary>
        /// Fit all regions on the map
        /// </summary>
        /// <param name="regions">The regions to fit to the map</param>
        /// <param name="animate">Animation on/off</param>
        public void FitToMapRegions(IEnumerable<MapSpan> regions, bool animate = false, int padding = 0) => MapFunctions.FitToMapRegions(regions, animate, padding);
        
        /// <summary>
        /// Converts an array of <see cref="Point"/> into geocoordinates
        /// </summary>
        /// <param name="screenLocations">The screen locations(pixel)</param>
        /// <returns>A collection of <see cref="Position"/></returns>
        public IEnumerable<Position> ScreenLocationsToGeocoordinates(params Point[] screenLocations) => MapFunctions.ScreenLocationsToGeocoordinates(screenLocations);
       
        /// <summary>
        /// Raises <see cref="PinSelected"/>
        /// </summary>
        /// <param name="pin">The selected pin</param>
        protected void OnPinSelected(MKCustomMapPin pin)
        {
            PinSelected?.Invoke(this, new GenericEventArgs<MKCustomMapPin>(pin));

            RaiseCommand(PinSelectedCommand, pin);
        }
        
        /// <summary>
        /// Raises <see cref="PinDragEnd"/>
        /// </summary>
        /// <param name="pin">The dragged pin</param>
        protected void OnPinDragEnd(MKCustomMapPin pin)
        {
            PinDragEnd?.Invoke(this, new GenericEventArgs<MKCustomMapPin>(pin));

            RaiseCommand(PinDragEndCommand, pin);
        }
        
        /// <summary>
        /// Raises <see cref="MapClicked"/>
        /// </summary>
        /// <param name="position">The position on the map</param>
        protected void OnMapClicked(Position position)
        {
            MapClicked?.Invoke(this, new GenericEventArgs<Position>(position));

            RaiseCommand(MapClickedCommand, position);
        }
        
        /// <summary>
        /// Raises <see cref="MapLongPress"/>
        /// </summary>
        /// <param name="position">The position on the map</param>
        protected void OnMapLongPress(Position position)
        {
            MapLongPress?.Invoke(this, new GenericEventArgs<Position>(position));

            RaiseCommand(MapLongPressCommand, position);
        }
        
        /// <summary>
        /// Raises <see cref="RouteClicked"/>
        /// </summary>
        /// <param name="route">The tapped route</param>
        protected void OnRouteClicked(MKRoute route)
        {
            RouteClicked?.Invoke(this, new GenericEventArgs<MKRoute>(route));

            RaiseCommand(RouteClickedCommand, route);
        }
        
        /// <summary>
        /// Raises <see cref="RouteCalculationFinished"/>
        /// </summary>
        /// <param name="route">The route</param>
        protected void OnRouteCalculationFinished(MKRoute route)
        {
            RouteCalculationFinished?.Invoke(this, new GenericEventArgs<MKRoute>(route));

            RaiseCommand(RouteCalculationFinishedCommand, route);
        }
        
        /// <summary>
        /// Raises <see cref="RouteCalculationFailed"/>
        /// </summary>
        /// <param name="error">The error</param>
        protected void OnRouteCalculationFailed(RouteCalculationError error)
        {
            RouteCalculationFailed?.Invoke(this, new GenericEventArgs<RouteCalculationError>(error));

            RaiseCommand(RouteCalculationFailedCommand, error);
        }
        
        /// <summary>
        /// Raises <see cref="UserLocationChanged"/>
        /// </summary>
        /// <param name="position">The position of the user</param>
        protected void OnUserLocationChanged(Position position)
        {
            UserLocationChanged?.Invoke(this, new GenericEventArgs<Position>(position));

            RaiseCommand(UserLocationChangedCommand, position);
        }
        
        /// <summary>
        /// Raises <see cref="PinsReady"/>
        /// </summary>
        protected void OnPinsReady()
        {
            PinsReady?.Invoke(this, new EventArgs());

            RaiseCommand(PinsReadyCommand, null);
        }
        
        /// <summary>
        /// Raises <see cref="CalloutClicked"/>
        /// </summary>
        protected void OnCalloutClicked(MKCustomMapPin pin)
        {
            CalloutClicked?.Invoke(this, new GenericEventArgs<MKCustomMapPin>(pin));

            RaiseCommand(CalloutClickedCommand, pin);
        }
        
        /// <summary>
        /// Raises <see cref="MapReady"/>
        /// </summary>
        protected void OnMapReady()
        {
            MapReady?.Invoke(this, EventArgs.Empty);
            RaiseCommand(MapReadyCommand, null);
        }

        /// <summary>
        /// Raises <see cref="CameraIdeal"/>
        /// </summary>
        protected void OnCameraIdeal()
        {
            CameraIdeal?.Invoke(this, EventArgs.Empty);
            RaiseCommand(CameraIdealCommand, null);
        }

        /// <summary>
        /// Raises <see cref="CameraMoveStarted"/>
        /// </summary>
        protected void OnCameraMoveStarted()
        {
            CameraMoveStarted?.Invoke(this, EventArgs.Empty);
            RaiseCommand(CameraMoveStartedCommand, null);
        }

        /// <summary>
        /// Raises a specific command
        /// </summary>
        /// <param name="command">The command to raise</param>
        /// <param name="parameter">Addition command parameter</param>
        void RaiseCommand(ICommand command, object parameter)
        {
            if (command != null && command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }
        }
        
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IMapFunctions.SetRenderer(IRendererFunctions renderer) => MapFunctions = renderer;
        
        /// <inheritdoc/>
        void IMapFunctions.RaisePinSelected(MKCustomMapPin pin) => OnPinSelected(pin);
        
        /// <inheritdoc/>
        void IMapFunctions.RaisePinDragEnd(MKCustomMapPin pin) => OnPinDragEnd(pin);
        
        /// <inheritdoc/>
        void IMapFunctions.RaiseMapClicked(Position position) => OnMapClicked(position);
        
        /// <inheritdoc/>
        void IMapFunctions.RaiseMapLongPress(Position position) => OnMapLongPress(position);
        
        /// <inheritdoc/>
        void IMapFunctions.RaiseUserLocationChanged(Position position) => OnUserLocationChanged(position);
       
        /// <inheritdoc/>
        void IMapFunctions.RaiseRouteClicked(MKRoute route) => OnRouteClicked(route);
        
        /// <inheritdoc/>
        void IMapFunctions.RaiseRouteCalculationFinished(MKRoute route) => OnRouteCalculationFinished(route);
        
        /// <inheritdoc/>
        void IMapFunctions.RaiseRouteCalculationFailed(RouteCalculationError route) => OnRouteCalculationFailed(route);
        
        /// <inheritdoc/>
        void IMapFunctions.RaisePinsReady() => OnPinsReady();
        
        /// <inheritdoc/>
        void IMapFunctions.RaiseCalloutClicked(MKCustomMapPin pin) => OnCalloutClicked(pin);
        
        /// <inheritdoc/>
        void IMapFunctions.RaiseMapReady() => OnMapReady();

        /// <inheritdoc/>
        void IMapFunctions.RaiseCameraIdeal() => OnCameraIdeal();

        /// <inheritdoc/>
        void IMapFunctions.RaiseCameraMoveStarted() => OnCameraMoveStarted();
    }
}
