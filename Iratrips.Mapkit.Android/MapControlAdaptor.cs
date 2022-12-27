using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Iratrips.MapKit;
using Iratrips.MapKit.Api.Google;
using Iratrips.MapKit.Droid;
using Iratrips.MapKit.Interfaces;
using Iratrips.MapKit.Models;
using Iratrips.MapKit.Overlays;
using Iratrips.MapKit.Utilities;
using Color = Avalonia.Media.Color;
using System.Collections;
using System.Reflection;
using Android.OS;
using Android.Content;
using Android.Views;
using Android.Widget;
using Avalonia;
using Avalonia.Platform;
using Java.IO;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using File = Java.IO.File;

namespace Iratrips.MapKit.Droid
{
    /// <summary>
    /// Android Renderer of <see cref="MapControl"/>
    /// </summary>
    public class MapControlAdaptor : Java.Lang.Object, IRendererFunctions,
        GoogleMap.ISnapshotReadyCallback, GoogleMap.IOnCameraIdleListener, IOnMapReadyCallback,
        GoogleMap.IInfoWindowAdapter, GoogleMap.IOnCameraMoveStartedListener
    {
        private Context Context { get; set; }

        private object _lockObj = new object();

        private bool _isInitialized = false;
        private bool _isLayoutPerformed = false;

        private readonly List<MapRoute> _tempRouteList = new List<MapRoute>();

        private readonly Dictionary<MapRoute, Polyline> _routes = new Dictionary<MapRoute, Polyline>();
        private readonly Dictionary<MapPolyline, Polyline> _polylines = new Dictionary<MapPolyline, Polyline>();
        private readonly Dictionary<MapCircle, Circle> _circles = new Dictionary<MapCircle, Circle>();
        private readonly Dictionary<MapPolygon, Polygon> _polygons = new Dictionary<MapPolygon, Polygon>();
        private readonly Dictionary<MapPin, MapMarker> _markers = new Dictionary<MapPin, MapMarker>();

        private Marker? _selectedMarker;
        private bool _isDragging = false;
        private bool _disposed = false;
        private byte[]? _snapShot;

        private TileOverlay? _tileOverlay;
        private GoogleMap? _googleMap = null;


        private static Bundle s_bundle;
        internal static Bundle Bundle { set { s_bundle = value; } }

        private MapView Control { get; set; }
        private GoogleMap? Map => _googleMap;
        internal MapControl FormsMapControl { get; set; }

        private IMapFunctions MapFunctions => this.FormsMapControl as IMapFunctions;

        public IPlatformHandle PlatformHandle => new PlatformHandle(Control.Handle, "HWND");

        /// <summary>
        /// Creates a new instance of <see cref="MapControlAdaptor"/>
        /// </summary>
        /// <param name="context">Android context</param>
        public MapControlAdaptor(MapControl mapControl, Context context) 
        {
            this.Context = context;
            this.FormsMapControl = mapControl;
            
            if (!GoogleMaps.IsInitialized) throw new Exception("Call MKGoogleMaps.Init first");

            this.Control = new MapView(Context);
            this.Control.OnCreate(s_bundle);
            this.Control.OnResume();
            this.Control.LayoutChange += ControlOnLayoutChange;

            lock (_lockObj)
            {
                //if (e.OldElement != null)
                //{
                //    e.OldElement.PropertyChanged -= FormsMapPropertyChanged;
                //    UnregisterCollections((MKCustomMap)e.OldElement);

                //    if (_googleMap != null)
                //    {
                //        _googleMap.MarkerClick -= OnMarkerClick;
                //        _googleMap.MapClick -= OnMapClick;
                //        _googleMap.MapLongClick -= OnMapLongClick;
                //        _googleMap.MarkerDragEnd -= OnMarkerDragEnd;
                //        _googleMap.MarkerDrag -= OnMarkerDrag;
                //        _googleMap.MarkerDragStart -= OnMarkerDragStart;
                //        _googleMap.InfoWindowClick -= OnInfoWindowClick;
                //        _googleMap.MyLocationChange -= OnUserLocationChange;
                //        _googleMap.SetOnCameraIdleListener(null);
                //        _googleMap.SetOnCameraMoveStartedListener(null);
                //        _googleMap.SetInfoWindowAdapter(null);
                //        _googleMap = null;
                //    }

                //}

                MapFunctions.SetRenderer(this);
                this.Control.GetMapAsync(this);
                FormsMapControl.PropertyChanged += FormsMapPropertyChanged;

            }

        }

        private void ControlOnLayoutChange(object? sender, View.LayoutChangeEventArgs e)
        {
            UpdateMapRegion();
        }
        
        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;

            _disposed = true;

            if (disposing)
            {
                //Control.LayoutChange -= ControlOnLayoutChange;
                FormsMapControl.PropertyChanged -= FormsMapPropertyChanged;
                UnregisterCollections(FormsMapControl);
                
                if (_googleMap != null)
                {
                    _googleMap.MarkerClick -= OnMarkerClick;
                    _googleMap.MapClick -= OnMapClick;
                    _googleMap.MapLongClick -= OnMapLongClick;
                    _googleMap.MarkerDragEnd -= OnMarkerDragEnd;
                    _googleMap.MarkerDrag -= OnMarkerDrag;
                    _googleMap.MarkerDragStart -= OnMarkerDragStart;
                    _googleMap.InfoWindowClick -= OnInfoWindowClick;
                    _googleMap.MyLocationChange -= OnUserLocationChange;
                    _googleMap.SetOnCameraIdleListener(null);
                    _googleMap.SetOnCameraMoveStartedListener(null);
                    _googleMap.SetInfoWindowAdapter(null);
                    _googleMap.Dispose();
                    _googleMap = null;
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// When a property of the Forms map changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void FormsMapPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (_googleMap == null) return;

            switch (e.Property.Name)
            {
                case nameof(MapControl.Pins):
                    UpdatePins();
                    break;
                case nameof(MapControl.SelectedPin):
                    SetSelectedItem();
                    break;
                case nameof(MapControl.Polylines):
                    UpdateLines();
                    break;
                case nameof(MapControl.Circles):
                    UpdateCircles();
                    break;
                case nameof(MapControl.Polygons):
                    UpdatePolygons();
                    break;
                case nameof(MapControl.Routes):
                    UpdateRoutes();
                    break;
                case nameof(MapControl.TilesUrlOptions):
                    UpdateTileOptions();
                    break;
                case nameof(MapControl.ShowTraffic):
                    UpdateShowTraffic();
                    break;
                case nameof(MapControl.MapRegion):
                    UpdateMapRegion();
                    break;
                case nameof(MapControl.MapType):
                    UpdateMapType();
                    break;
                case nameof(MapControl.IsShowingUser):
                    UpdateIsShowingUser();
                    break;
                case nameof(MapControl.HasScrollEnabled):
                    UpdateHasScrollEnabled();
                    break;
                case nameof(MapControl.HasZoomEnabled):
                    UpdateHasZoomEnabled();
                    break;
            }
        }
        /// <summary>
        /// When the map is ready to use
        /// </summary>
        /// <param name="googleMap">The map instance</param>
        public virtual void OnMapReady(GoogleMap googleMap)
        {
            lock (_lockObj)
            {
                _googleMap = googleMap;

                _googleMap.MarkerClick += OnMarkerClick;
                _googleMap.MapClick += OnMapClick;
                _googleMap.MapLongClick += OnMapLongClick;
                _googleMap.MarkerDragEnd += OnMarkerDragEnd;
                _googleMap.MarkerDrag += OnMarkerDrag;
                _googleMap.MarkerDragStart += OnMarkerDragStart;
                _googleMap.InfoWindowClick += OnInfoWindowClick;
                _googleMap.MyLocationChange += OnUserLocationChange;

                _googleMap.SetOnCameraIdleListener(this);
                _googleMap.SetOnCameraMoveStartedListener(this);
                _googleMap.SetInfoWindowAdapter(this);

                UpdateTileOptions();
                UpdateMapRegion();
                UpdatePins();
                UpdateRoutes();
                UpdateLines();
                UpdateCircles();
                UpdatePolygons();
                UpdateShowTraffic();
                UpdateMapType();
                UpdateIsShowingUser();
                UpdateHasZoomEnabled();
                UpdateHasScrollEnabled();
                
                MapFunctions?.RaiseMapReady();
            }
        }
        /// <summary>
        /// When the location of the user changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnUserLocationChange(object? sender, GoogleMap.MyLocationChangeEventArgs e)
        {
            if (e.Location == null || FormsMapControl == null) return;

            var newPosition = new Position(e.Location.Latitude, e.Location.Longitude);
            MapFunctions.RaiseUserLocationChanged(newPosition);
        }
        /// <summary>
        /// When the info window gets clicked
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnInfoWindowClick(object? sender, GoogleMap.InfoWindowClickEventArgs e)
        {
            var pin = GetPinByMarker(e.Marker);

            if (pin == null || pin.Callout == null) return;

            if (pin.Callout.IsClickable)
                MapFunctions.RaiseCalloutClicked(pin);
        }
        /// <summary>
        /// Dragging process
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnMarkerDrag(object? sender, GoogleMap.MarkerDragEventArgs e)
        {
            var item = _markers.SingleOrDefault(i => true == i.Value.Marker?.Id.Equals(e.Marker.Id));
            if (item.Key == null) return;

            item.Key.Position = e.Marker.Position.ToPosition();
        }
        /// <summary>
        /// When a dragging starts
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnMarkerDragStart(object? sender, GoogleMap.MarkerDragStartEventArgs e)
        {
            _isDragging = true;
        }
        /// <summary>
        /// When the camera position changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnCameraChange(object? sender, GoogleMap.CameraChangeEventArgs e)
        {
            if (FormsMapControl == null) return;
            FormsMapControl.MapRegion = GetCurrentMapRegion(e.Position.Target);
        }
        /// <summary>
        /// When a pin gets clicked
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnMarkerClick(object? sender, GoogleMap.MarkerClickEventArgs e)
        {
            if (FormsMapControl == null) return;
            var item = _markers.SingleOrDefault(i => true == i.Value.Marker?.Id.Equals(e.Marker.Id));
            if (item.Key == null) return;

            _selectedMarker = e.Marker;
            FormsMapControl.SelectedPin = item.Key;
            if (item.Key.Callout != null)
            {
                item.Value.Marker.ShowInfoWindow();
            }
        }
        /// <summary>
        /// When a drag of a marker ends
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnMarkerDragEnd(object? sender, GoogleMap.MarkerDragEndEventArgs e)
        {
            _isDragging = false;

            if (FormsMapControl == null) return;

            var pin = _markers.SingleOrDefault(i => true == i.Value.Marker?.Id.Equals(e.Marker.Id));
            if (pin.Key == null) return;

            MapFunctions.RaisePinDragEnd(pin.Key);
        }
        /// <summary>
        /// When a long click was performed on the map
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnMapLongClick(object? sender, GoogleMap.MapLongClickEventArgs e)
        {
            if (FormsMapControl == null) return;

            var position = e.Point.ToPosition();
            MapFunctions.RaiseMapLongPress(position);
        }
        /// <summary>
        /// When the map got tapped
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnMapClick(object? sender, GoogleMap.MapClickEventArgs e)
        {
            if (FormsMapControl == null) return;

            var position = e.Point.ToPosition();

            if (FormsMapControl.Routes != null)
            {
                foreach (var route in FormsMapControl.Routes.Where(i => i.Selectable))
                {
                    var internalRoute = _routes[route];

                    if (GmsPolyUtil.IsLocationOnPath(
                        position,
                        internalRoute.Points.Select(i => i.ToPosition()),
                        true,
                        (int)_googleMap.CameraPosition.Zoom,
                        FormsMapControl.MapCenter.Latitude))
                    {
                        MapFunctions.RaiseRouteClicked(route);
                        return;
                    }
                }
            }
            MapFunctions.RaiseMapClicked(position);
        }
        /// <summary>
        /// Updates the markers when a pin gets added or removed in the collection
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnCustomPinsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (MapPin pin in e.NewItems)
                {
                    AddPin(pin);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (MapPin pin in e.OldItems)
                {
                    if (!FormsMapControl.Pins.Contains(pin))
                    {
                        RemovePin(pin);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                UpdatePins(false);
            }
        }
        /// <summary>
        /// When a property of a pin changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        async void OnPinPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var pin = sender as MapPin;
            if (pin == null) return;

            MapMarker marker = null;
            if (!_markers.ContainsKey(pin) || (marker = _markers[pin]) == null) return;
            await marker.HandlePropertyChangedAsync(e, _isDragging);
        }
        
        /// <summary>
        /// Collection of routes changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnLineCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (MapPolyline line in e.NewItems)
                {
                    AddLine(line);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (MapPolyline line in e.OldItems)
                {
                    if (!FormsMapControl.Polylines.Contains(line))
                    {
                        _polylines[line].Remove();
                        line.PropertyChanged -= OnLinePropertyChanged;
                        _polylines.Remove(line);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                UpdateLines(false);
            }
        }
        /// <summary>
        /// A property of a route changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnLinePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var line = (MapPolyline)sender;

            if (e.PropertyName == nameof(MapPolyline.LineCoordinates))
            {
                if (line.LineCoordinates != null && line.LineCoordinates.Count > 1)
                {
                    _polylines[line].Points = new List<LatLng>(line.LineCoordinates.Select(i => i.ToLatLng()));
                }
                else
                {
                    _polylines[line].Points = null;
                }
            }
            else if (e.PropertyName == nameof(MapPolyline.Color))
            {
                _polylines[line].Color = line.Color.ToAndroid().ToArgb();
            }
            else if (e.PropertyName == nameof(MapPolyline.LineWidth))
            {
                _polylines[line].Width = line.LineWidth;
            }
        }
        /// <summary>
        /// Creates all Markers on the map
        /// </summary>
        void UpdatePins(bool firstUpdate = true)
        {
            if (_googleMap == null) return;

            foreach (var i in _markers)
            {
                RemovePin(i.Key, false);
            }
            _markers.Clear();
            if (FormsMapControl.Pins != null)
            {
                foreach (var pin in FormsMapControl.Pins)
                {
                    AddPin(pin);
                }
                if (firstUpdate)
                {
                    var observAble = FormsMapControl.Pins as INotifyCollectionChanged;
                    if (observAble != null)
                    {
                        observAble.CollectionChanged += OnCustomPinsCollectionChanged;
                    }
                }
                MapFunctions.RaisePinsReady();
            }
        }
        /// <summary>
        /// Adds a marker to the map
        /// </summary>
        /// <param name="pin">The Forms Pin</param>
        void AddPin(MapPin pin)
        {
            if (_markers.Keys.Contains(pin)) return;

            pin.PropertyChanged += OnPinPropertyChanged;

            var tkMarker = new MapMarker(pin, Context);
            var markerWithIcon = new MarkerOptions();
            tkMarker.InitializeMarkerOptions(markerWithIcon);

            _markers.Add(pin, tkMarker);
            tkMarker.Marker = _googleMap.AddMarker(markerWithIcon);
        }

        /// <summary>
        /// Remove a pin from the map and the internal dictionary
        /// </summary>
        /// <param name="pin">The pin to remove</param>
        /// <param name="removeMarker">true to remove the marker from the map</param>
        void RemovePin(MapPin pin, bool removeMarker = true)
        {
            if (!_markers.TryGetValue(pin, out var item)) return;

            if (_selectedMarker != null)
            {
                if (item.Marker.Id.Equals(_selectedMarker.Id))
                {
                    FormsMapControl.SelectedPin = null;
                }
            }

            item.Marker?.Remove();
            pin.PropertyChanged -= OnPinPropertyChanged;

            if (removeMarker)
            {
                _markers.Remove(pin);
            }
        }
        /// <summary>
        /// Set the selected item on the map
        /// </summary>
        void SetSelectedItem()
        {
            if (_selectedMarker != null)
            {
                _selectedMarker.HideInfoWindow();
                _selectedMarker = null;
            }
            if (FormsMapControl.SelectedPin != null)
            {
                if (!_markers.ContainsKey(FormsMapControl.SelectedPin)) return;

                var selectedPin = _markers[FormsMapControl.SelectedPin];
                _selectedMarker = selectedPin.Marker;
                if (FormsMapControl.SelectedPin.Callout != null)
                {
                    selectedPin.Marker.ShowInfoWindow();
                }

                MapFunctions.RaisePinSelected(FormsMapControl.SelectedPin);
            }
        }

        /// <summary>
        /// Creates the routes on the map
        /// </summary>
        void UpdateLines(bool firstUpdate = true)
        {
            if (_googleMap == null) return;

            foreach (var i in _polylines)
            {
                i.Key.PropertyChanged -= OnLinePropertyChanged;
                i.Value.Remove();
            }
            _polylines.Clear();

            if (FormsMapControl.Polylines != null)
            {
                foreach (var line in FormsMapControl.Polylines)
                {
                    AddLine(line);
                }

                if (firstUpdate)
                {
                    var observAble = FormsMapControl.Polylines as INotifyCollectionChanged;
                    if (observAble != null)
                    {
                        observAble.CollectionChanged += OnLineCollectionChanged;
                    }
                }
            }
        }
        /// <summary>
        /// Updates all circles
        /// </summary>
        void UpdateCircles(bool firstUpdate = true)
        {
            if (_googleMap == null) return;

            foreach (var i in _circles)
            {
                i.Key.PropertyChanged -= CirclePropertyChanged;
                i.Value.Remove();
            }
            _circles.Clear();
            if (FormsMapControl.Circles != null)
            {
                foreach (var circle in FormsMapControl.Circles)
                {
                    AddCircle(circle);
                }
                if (firstUpdate)
                {
                    var observAble = FormsMapControl.Circles as INotifyCollectionChanged;
                    if (observAble != null)
                    {
                        observAble.CollectionChanged += CirclesCollectionChanged;
                    }
                }
            }
        }
        /// <summary>
        /// Creates the polygones on the map
        /// </summary>
        /// <param name="firstUpdate">If the collection updates the first time</param>
        void UpdatePolygons(bool firstUpdate = true)
        {
            if (_googleMap == null) return;

            foreach (var i in _polygons)
            {
                i.Key.PropertyChanged -= OnPolygonPropertyChanged;
                i.Value.Remove();
            }
            _polygons.Clear();
            if (FormsMapControl.Polygons != null)
            {
                foreach (var i in FormsMapControl.Polygons)
                {
                    AddPolygon(i);
                }
                if (firstUpdate)
                {
                    var observAble = FormsMapControl.Polygons as INotifyCollectionChanged;
                    if (observAble != null)
                    {
                        observAble.CollectionChanged += OnPolygonsCollectionChanged;
                    }
                }
            }
        }
        /// <summary>
        /// Create all routes
        /// </summary>
        /// <param name="firstUpdate">If first update of collection or not</param>
        void UpdateRoutes(bool firstUpdate = true)
        {
            _tempRouteList.Clear();

            if (_googleMap == null) return;

            foreach (var i in _routes)
            {
                if (i.Key != null)
                    i.Key.PropertyChanged -= OnRoutePropertyChanged;
                i.Value.Remove();
            }
            _routes.Clear();

            if (FormsMapControl == null || FormsMapControl.Routes == null) return;

            foreach (var i in FormsMapControl.Routes)
            {
                AddRoute(i);
            }

            if (firstUpdate)
            {
                var observAble = FormsMapControl.Routes as INotifyCollectionChanged;
                if (observAble != null)
                {
                    observAble.CollectionChanged += OnRouteCollectionChanged;
                }
            }
        }
        /// <summary>
        /// When the collection of routes changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnRouteCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (MapRoute route in e.NewItems)
                {
                    AddRoute(route);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (MapRoute route in e.OldItems)
                {
                    if (!FormsMapControl.Routes.Contains(route))
                    {
                        _routes[route].Remove();
                        route.PropertyChanged -= OnRoutePropertyChanged;
                        _routes.Remove(route);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                UpdateRoutes(false);
            }
        }
        /// <summary>
        /// When a property of a route changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnRoutePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var route = (MapRoute)sender;

            if (e.PropertyName == nameof(MapRoute.Source) ||
                e.PropertyName == nameof(MapRoute.Destination) ||
                e.PropertyName == nameof(MapRoute.TravelMode))
            {
                route.PropertyChanged -= OnRoutePropertyChanged;
                _routes[route].Remove();
                _routes.Remove(route);

                AddRoute(route);
            }
            else if (e.PropertyName == nameof(MapPolyline.Color))
            {
                _routes[route].Color = route.Color.ToAndroid().ToArgb();
            }
            else if (e.PropertyName == nameof(MapPolyline.LineWidth))
            {
                _routes[route].Width = route.LineWidth;
            }
        }
        /// <summary>
        /// When the polygon collection changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnPolygonsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (MapPolygon poly in e.NewItems)
                {
                    AddPolygon(poly);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (MapPolygon poly in e.OldItems)
                {
                    if (!FormsMapControl.Polygons.Contains(poly))
                    {
                        _polygons[poly].Remove();
                        poly.PropertyChanged -= OnPolygonPropertyChanged;
                        _polygons.Remove(poly);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                UpdatePolygons(false);
            }
        }
        /// <summary>
        /// Adds a polygon to the map
        /// </summary>
        /// <param name="polygon">The polygon to add</param>
        void AddPolygon(MapPolygon polygon)
        {
            polygon.PropertyChanged += OnPolygonPropertyChanged;

            var polygonOptions = new PolygonOptions();

            if (polygon.Coordinates != null && polygon.Coordinates.Any())
            {
                polygonOptions.Add(polygon.Coordinates.Select(i => i.ToLatLng()).ToArray());
            }
            if (polygon.Color != default(Color))
            {
                polygonOptions.InvokeFillColor(polygon.Color.ToAndroid().ToArgb());
            }
            if (polygon.StrokeColor != default(Color))
            {
                polygonOptions.InvokeStrokeColor(polygon.StrokeColor.ToAndroid().ToArgb());
            }
            polygonOptions.InvokeStrokeWidth(polygon.StrokeWidth);

            _polygons.Add(polygon, _googleMap.AddPolygon(polygonOptions));
        }
        /// <summary>
        /// When a property of a <see cref="MapPolygon"/> changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnPolygonPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var tkPolygon = (MapPolygon)sender;

            switch (e.PropertyName)
            {
                case nameof(MapPolygon.Coordinates):
                    _polygons[tkPolygon].Points = tkPolygon.Coordinates.Select(i => i.ToLatLng()).ToList();
                    break;
                case nameof(MapPolygon.Color):
                    _polygons[tkPolygon].FillColor = tkPolygon.Color.ToAndroid().ToArgb();
                    break;
                case nameof(MapPolygon.StrokeColor):
                    _polygons[tkPolygon].StrokeColor = tkPolygon.StrokeColor.ToAndroid().ToArgb();
                    break;
                case nameof(MapPolygon.StrokeWidth):
                    _polygons[tkPolygon].StrokeWidth = tkPolygon.StrokeWidth;
                    break;
            }
        }
        /// <summary>
        /// When the circle collection changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void CirclesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (MapCircle circle in e.NewItems)
                {
                    AddCircle(circle);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (MapCircle circle in e.OldItems)
                {
                    if (!FormsMapControl.Circles.Contains(circle))
                    {
                        circle.PropertyChanged -= CirclePropertyChanged;
                        _circles[circle].Remove();
                        _circles.Remove(circle);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                UpdateCircles(false);
            }
        }
        /// <summary>
        /// Adds a circle to the map
        /// </summary>
        /// <param name="circle">The circle to add</param>
        void AddCircle(MapCircle circle)
        {
            circle.PropertyChanged += CirclePropertyChanged;

            var circleOptions = new CircleOptions();

            circleOptions.InvokeRadius(circle.Radius);
            circleOptions.InvokeCenter(circle.Center.ToLatLng());

            if (circle.Color != default(Color))
            {
                circleOptions.InvokeFillColor(circle.Color.ToAndroid().ToArgb());
            }
            if (circle.StrokeColor != default(Color))
            {
                circleOptions.InvokeStrokeColor(circle.StrokeColor.ToAndroid().ToArgb());
            }
            circleOptions.InvokeStrokeWidth(circle.StrokeWidth);
            _circles.Add(circle, _googleMap.AddCircle(circleOptions));
        }
        /// <summary>
        /// When a property of a <see cref="MapCircle"/> changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void CirclePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var tkCircle = (MapCircle)sender;
            var circle = _circles[tkCircle];

            switch (e.PropertyName)
            {
                case nameof(MapCircle.Radius):
                    circle.Radius = tkCircle.Radius;
                    break;
                case nameof(MapCircle.Center):
                    circle.Center = tkCircle.Center.ToLatLng();
                    break;
                case nameof(MapCircle.Color):
                    circle.FillColor = tkCircle.Color.ToAndroid().ToArgb();
                    break;
                case nameof(MapCircle.StrokeColor):
                    circle.StrokeColor = tkCircle.StrokeColor.ToAndroid().ToArgb();
                    break;
            }
        }
        /// <summary>
        /// Adds a route to the map
        /// </summary>
        /// <param name="line">The route to add</param>
        void AddLine(MapPolyline line)
        {
            line.PropertyChanged += OnLinePropertyChanged;

            var polylineOptions = new PolylineOptions();
            if (line.Color != default(Color))
            {
                polylineOptions.InvokeColor(line.Color.ToAndroid().ToArgb());
            }
            if (line.LineWidth > 0)
            {
                polylineOptions.InvokeWidth(line.LineWidth);
            }

            if (line.LineCoordinates != null)
            {
                polylineOptions.Add(line.LineCoordinates.Select(i => i.ToLatLng()).ToArray());
            }

            _polylines.Add(line, _googleMap.AddPolyline(polylineOptions));
        }
        /// <summary>
        /// Calculates and adds the route to the map
        /// </summary>
        /// <param name="route">The route to add</param>
        async void AddRoute(MapRoute route)
        {
            if (route == null) return;

            _tempRouteList.Add(route);

            route.PropertyChanged += OnRoutePropertyChanged;

            GmsDirectionResult routeData = null;
            string errorMessage = null;

            routeData = await GmsDirection.Instance.CalculateRoute(route.Source, route.Destination, route.TravelMode.ToGmsTravelMode());

            if (FormsMapControl == null || Map == null || !_tempRouteList.Contains(route)) return;

            if (routeData != null && routeData.Routes != null)
            {
                if (routeData.Status == GmsDirectionResultStatus.Ok)
                {
                    var r = routeData.Routes.FirstOrDefault();
                    if (r != null && r.Polyline.Positions != null && r.Polyline.Positions.Any())
                    {
                        SetRouteData(route, r);

                        var routeOptions = new PolylineOptions();

                        if (route.Color != default(Color))
                        {
                            routeOptions.InvokeColor(route.Color.ToAndroid().ToArgb());
                        }
                        if (route.LineWidth > 0)
                        {
                            routeOptions.InvokeWidth(route.LineWidth);
                        }
                        routeOptions.Add(r.Polyline.Positions.Select(i => i.ToLatLng()).ToArray());

                        _routes.Add(route, _googleMap.AddPolyline(routeOptions));

                        MapFunctions.RaiseRouteCalculationFinished(route);
                    }
                    else
                    {
                        errorMessage = "Unexpected result";
                    }
                }
                else
                {
                    errorMessage = routeData.Status.ToString();
                }
            }
            else
            {
                errorMessage = "Could not connect to api";
            }
            if (!string.IsNullOrEmpty(errorMessage))
            {
                var routeCalculationError = new RouteCalculationError(route, errorMessage);

                MapFunctions.RaiseRouteCalculationFailed(routeCalculationError);
            }
        }
        /// <summary>
        /// Sets the route calculation data
        /// </summary>
        /// <param name="route">The PCL route</param>
        /// <param name="routeResult">The rourte api result</param>
        void SetRouteData(MapRoute route, GmsRouteResult routeResult)
        {
            var latLngBounds = new LatLngBounds(
                    new LatLng(routeResult.Bounds.SouthWest.Latitude, routeResult.Bounds.SouthWest.Longitude),
                    new LatLng(routeResult.Bounds.NorthEast.Latitude, routeResult.Bounds.NorthEast.Longitude));

            var apiSteps = routeResult.Legs.First().Steps;
            var steps = new MapRouteStep[apiSteps.Count()];
            var routeFunctions = (IRouteFunctions)route;


            for (int i = 0; i < steps.Length; i++)
            {
                steps[i] = new MapRouteStep();
                var stepFunctions = (IRouteStepFunctions)steps[i];
                var apiStep = apiSteps.ElementAt(i);

                stepFunctions.SetDistance(apiStep.Distance.Value);
                stepFunctions.SetInstructions(apiStep.HtmlInstructions);
            }
            routeFunctions.SetSteps(steps);
            routeFunctions.SetDistance(routeResult.Legs.First().Distance.Value);
            routeFunctions.SetTravelTime(routeResult.Legs.First().Duration.Value);

            routeFunctions.SetBounds(
                MapSpan.FromCenterAndRadius(
                    latLngBounds.Center.ToPosition(),
                    Distance.FromKilometers(
                        new Position(latLngBounds.Southwest.Latitude, latLngBounds.Southwest.Longitude)
                        .DistanceTo(
                            new Position(latLngBounds.Northeast.Latitude, latLngBounds.Northeast.Longitude)) / 2)));
            routeFunctions.SetIsCalculated(true);
        }
        /// <summary>
        /// Updates the image of a pin
        /// </summary>
        /// <param name="pin">The forms pin</param>
        /// <param name="markerOptions">The native marker options</param>
        Task UpdateImage(MapPin pin, MarkerOptions markerOptions)
        {
            BitmapDescriptor bitmap;
            try
            {
                if (pin.Image != null)
                    bitmap = Droid.BitmapUtil.GetBitmapDescriptorFromStream(pin.Image);
                else
                {
                    if (pin.DefaultPinColor != default(Color))
                    {
                        var hue = pin.DefaultPinColor.ToAndroid().GetHue();
                        bitmap = BitmapDescriptorFactory.DefaultMarker(Math.Min(hue, 359.99f));
                    }
                    else
                    {
                        bitmap = BitmapDescriptorFactory.DefaultMarker();
                    }
                }
            }
            catch (Exception)
            {
                bitmap = BitmapDescriptorFactory.DefaultMarker();
            }
            
            markerOptions.SetIcon(bitmap);
            return Task.CompletedTask;
        }
        /// <summary>
        /// Updates the image on a marker
        /// </summary>
        /// <param name="pin">The forms pin</param>
        /// <param name="marker">The native marker</param>
        Task UpdateImage(MapPin pin, Marker marker)
        {
            BitmapDescriptor bitmap;
            try
            {
                if (pin.Image != null)
                {
                    bitmap = Droid.BitmapUtil.GetBitmapDescriptorFromStream(pin.Image);
                }
                else
                {
                    if (pin.DefaultPinColor != default(Color))
                    {
                        var hue = pin.DefaultPinColor.ToAndroid().GetHue();
                        bitmap = BitmapDescriptorFactory.DefaultMarker(hue);
                    }
                    else
                    {
                        bitmap = BitmapDescriptorFactory.DefaultMarker();
                    }
                }
            }
            catch (Exception)
            {
                bitmap = BitmapDescriptorFactory.DefaultMarker();
            }

            marker.SetIcon(bitmap);
            return Task.CompletedTask;

        }
        /// <summary>
        /// Updates the custom tile provider 
        /// </summary>
        void UpdateTileOptions()
        {
            if (_tileOverlay != null)
            {
                _tileOverlay.Remove();
                _googleMap.MapType = GoogleMap.MapTypeNormal;
            }

            if (FormsMapControl == null || _googleMap == null) return;

            if (FormsMapControl.TilesUrlOptions != null)
            {
                _googleMap.MapType = GoogleMap.MapTypeNone;

                _tileOverlay = _googleMap.AddTileOverlay(
                    new TileOverlayOptions()
                        .InvokeTileProvider(
                            new MapTileProvider(FormsMapControl.TilesUrlOptions))
                        .InvokeZIndex(-1));
            }
        }
        /// <summary>
        /// Updates the visible map region
        /// </summary>
        void UpdateMapRegion()
        {
            if (FormsMapControl == null || _googleMap == null || !_isLayoutPerformed || FormsMapControl.MapRegion == null || !FormsMapControl.IsVisible) return;

            if (!FormsMapControl.MapRegion.Equals(GetCurrentMapRegion(_googleMap.CameraPosition.Target)))
            {
                MoveToMapRegion(FormsMapControl.MapRegion, FormsMapControl.IsRegionChangeAnimated);
            }
        }
        /// <summary>
        /// Sets traffic enabled on the google map
        /// </summary>
        void UpdateShowTraffic()
        {
            if (FormsMapControl == null || _googleMap == null) return;

            _googleMap.TrafficEnabled = FormsMapControl.ShowTraffic;
        }

        /// <summary>
        /// Updates the map type
        /// </summary>
        void UpdateMapType()
        {
            if (FormsMapControl == null || _googleMap == null) return;

            switch (FormsMapControl.MapType)
            {
                case MapType.Hybrid:
                    Map.MapType = GoogleMap.MapTypeHybrid;
                    break;
                case MapType.Satellite:
                    Map.MapType = GoogleMap.MapTypeSatellite;
                    break;
                case MapType.Street:
                    Map.MapType = GoogleMap.MapTypeNormal;
                    break;
            }
        }
        /// <summary>
        /// Updates if the user location and user location button are displayed
        /// </summary>
        void UpdateIsShowingUser()
        {
            if (FormsMapControl == null || _googleMap == null) return;
            Map.MyLocationEnabled = Map.UiSettings.MyLocationButtonEnabled = FormsMapControl.IsShowingUser;
        }
        /// <summary>
        /// Updates scroll gesture
        /// </summary>
        void UpdateHasScrollEnabled()
        {
            if (FormsMapControl == null || _googleMap == null) return;
            Map.UiSettings.ScrollGesturesEnabled = FormsMapControl.HasScrollEnabled;
        }
        /// <summary>
        /// Updates zoom gesture/control
        /// </summary>
        void UpdateHasZoomEnabled()
        {
            if (FormsMapControl == null || _googleMap == null) return;

            Map.UiSettings.ZoomGesturesEnabled = Map.UiSettings.ZoomControlsEnabled = FormsMapControl.HasZoomEnabled;
        }
        /// <summary>
        /// Creates a <see cref="LatLngBounds"/> from a collection of <see cref="MapSpan"/>
        /// </summary>
        /// <param name="spans">The spans to get calculate the bounds from</param>
        /// <returns>The bounds</returns>
        LatLngBounds BoundsFromMapSpans(params MapSpan[] spans)
        {
            LatLngBounds.Builder builder = new LatLngBounds.Builder();

            foreach (var region in spans)
            {
                builder
                    .Include(GmsSphericalUtil.ComputeOffset(region.Center, region.Radius.Meters, 0).ToLatLng())
                    .Include(GmsSphericalUtil.ComputeOffset(region.Center, region.Radius.Meters, 90).ToLatLng())
                    .Include(GmsSphericalUtil.ComputeOffset(region.Center, region.Radius.Meters, 180).ToLatLng())
                    .Include(GmsSphericalUtil.ComputeOffset(region.Center, region.Radius.Meters, 270).ToLatLng());
            }
            return builder.Build();
        }
        /// <summary>
        /// Unregisters all collections
        /// </summary>
        void UnregisterCollections(MapControl mapControl)
        {
            UnregisterCollection(mapControl.Pins, OnCustomPinsCollectionChanged, OnPinPropertyChanged);
            UnregisterCollection(mapControl.Routes, OnRouteCollectionChanged, OnRoutePropertyChanged);
            UnregisterCollection(mapControl.Polylines, OnLineCollectionChanged, OnLinePropertyChanged);
            UnregisterCollection(mapControl.Circles, CirclesCollectionChanged, CirclePropertyChanged);
            UnregisterCollection(mapControl.Polygons, OnPolygonsCollectionChanged, OnPolygonPropertyChanged);
        }
        /// <summary>
        /// Unregisters one collection and all of its items
        /// </summary>
        /// <param name="collection">The collection to unregister</param>
        /// <param name="observableHandler">The <see cref="NotifyCollectionChangedEventHandler"/> of the collection</param>
        /// <param name="propertyHandler">The <see cref="PropertyChangedEventHandler"/> of the collection items</param>
        void UnregisterCollection(
           IEnumerable collection,
           NotifyCollectionChangedEventHandler observableHandler,
           PropertyChangedEventHandler propertyHandler)
        {
            if (collection == null) return;

            var observable = collection as INotifyCollectionChanged;
            if (observable != null)
            {
                observable.CollectionChanged -= observableHandler;
            }
            foreach (INotifyPropertyChanged obj in collection)
            {
                obj.PropertyChanged -= propertyHandler;
            }
        }
        /// <summary>
        /// Gets the current mapregion
        /// </summary>
        /// <param name="center">Center point</param>
        /// <returns>The map region</returns>
        MapSpan GetCurrentMapRegion(LatLng center)
        {
            var map = _googleMap;
            if (map == null)
                return null;

            var projection = map.Projection;
            var width = Control.Width;
            var height = Control.Height;
            var ul = projection.FromScreenLocation(new global::Android.Graphics.Point(0, 0));
            var ur = projection.FromScreenLocation(new global::Android.Graphics.Point(width, 0));
            var ll = projection.FromScreenLocation(new global::Android.Graphics.Point(0, height));
            var lr = projection.FromScreenLocation(new global::Android.Graphics.Point(width, height));
            var dlat = Math.Max(Math.Abs(ul.Latitude - lr.Latitude), Math.Abs(ur.Latitude - ll.Latitude));
            var dlong = Math.Max(Math.Abs(ul.Longitude - lr.Longitude), Math.Abs(ur.Longitude - ll.Longitude));

            return new MapSpan(new Position(center.Latitude, center.Longitude), dlat, dlong);
        }

        /// <inheritdoc/>
        public async Task<byte[]> GetSnapshot()
        {
            if (_googleMap == null) return null;

            _snapShot = null;
            _googleMap.Snapshot(this);

            while (_snapShot == null) await Task.Delay(10);

            return _snapShot;
        }
        ///<inheritdoc/>
        public void OnSnapshotReady(Bitmap snapshot)
        {
            using (var strm = new MemoryStream())
            {
                snapshot.Compress(Bitmap.CompressFormat.Png, 100, strm);
                _snapShot = strm.ToArray();
            }
        }
        ///<inheritdoc/>
        public void FitMapRegionToPositions(IEnumerable<Position> positions, bool animate = false, int padding = 0)
        {
            if (_googleMap == null) throw new InvalidOperationException("Map not ready");
            if (positions == null) throw new InvalidOperationException("positions can't be null");

            LatLngBounds.Builder builder = new LatLngBounds.Builder();

            positions.ToList().ForEach(i => builder.Include(i.ToLatLng()));

            if (animate)
                _googleMap.AnimateCamera(CameraUpdateFactory.NewLatLngBounds(builder.Build(), padding));
            else
                _googleMap.MoveCamera(CameraUpdateFactory.NewLatLngBounds(builder.Build(), padding));
        }
        ///<inheritdoc/>
        public void MoveToMapRegion(MapSpan region, bool animate)
        {
            if (_googleMap == null) return;

            if (region == null) return;

            var bounds = BoundsFromMapSpans(region);
            if (bounds == null) return;
            var cam = CameraUpdateFactory.NewLatLngBounds(bounds, 0);

            if (animate && _isInitialized)
                _googleMap.AnimateCamera(cam);
            else
                _googleMap.MoveCamera(cam);
        }
        ///<inheritdoc/>
        public void FitToMapRegions(IEnumerable<MapSpan> regions, bool animate = false, int padding = 0)
        {
            if (_googleMap == null || regions == null || !regions.Any()) return;

            var bounds = BoundsFromMapSpans(regions.ToArray());
            if (bounds == null) return;
            var cam = CameraUpdateFactory.NewLatLngBounds(bounds, padding);

            if (animate)
                _googleMap.AnimateCamera(cam);
            else
                _googleMap.MoveCamera(cam);
        }
        ///<inheritdoc/>
        public IEnumerable<Position> ScreenLocationsToGeocoordinates(params Avalonia.Point[] screenLocations)
        {
            if (_googleMap == null)
                throw new InvalidOperationException("Map not initialized");
            return screenLocations.Select(i => _googleMap.Projection.FromScreenLocation(i.ToAndroidPoint()).ToPosition());
        }
        
        /// <summary>
        /// Gets the <see cref="MapPin"/> by the native <see cref="Marker"/>
        /// </summary>
        /// <param name="marker">The marker to search the pin for</param>
        /// <returns>The forms pin</returns>
        protected MapPin GetPinByMarker(Marker marker)
        {
            return _markers.SingleOrDefault(i => i.Value.Marker?.Id == marker.Id).Key;
        }

        public void OnCameraIdle()
        {
            if (FormsMapControl == null) return;

            FormsMapControl.MapRegion = GetCurrentMapRegion(Map.CameraPosition.Target);
            MapFunctions.RaiseCameraIdeal();
        }

        public void OnCameraMoveStarted(int reason)
        {
            if (FormsMapControl == null) return;
            MapFunctions.RaiseCameraMoveStarted();
        }

        
        
        public Android.Views.View GetInfoWindow(Marker marker)
        {
            return null;
        }

        public Android.Views.View GetInfoContents(Marker baseMarker)
        {
            // var inflater = Android.App.Application.Context.GetSystemService(Context.LayoutInflaterService) as Android.Views.LayoutInflater;
            // if (inflater != null && FormsMap.SelectedPin != null && FormsMap.SelectedPin.Callout != null && FormsMap.SelectedPin.Callout.HasCustomView)
            // {
            //     if (FormsMap.SelectedPin.Callout == null || FormsMap.GetCalloutView == null) return null;
            //
            //     Xamarin.Forms.View xfView = FormsMap.GetCalloutView?.Invoke(FormsMap.SelectedPin);
            //
            //     var renderer = Xamarin.Forms.Platform.Android.Platform.CreateRendererWithContext(xfView, this.Context);
            //     var nativeView = renderer.View;
            //     renderer.Tracker.UpdateLayout();
            //     xfView.Layout(new Xamarin.Forms.Rectangle(0, 0, this.Context.ToPixels(xfView.WidthRequest), this.Context.ToPixels(xfView.HeightRequest)));
            //
            //     LinearLayout layout = new LinearLayout(this.Context);
            //     layout.LayoutParameters = new LayoutParams((int)this.Context.ToPixels(xfView.WidthRequest), (int)this.Context.ToPixels(xfView.HeightRequest));
            //     layout.AddView(nativeView);
            //
            //     return layout;
            // }

            return null;
        }
    }
}
