﻿namespace Iratrips.MapKit.Overlays
{
    /// <summary>
    /// A route to display on the map
    /// </summary>
    public class MKRoute : MKOverlay, IRouteFunctions
    {

         Position _source;
         Position _destination;
         float _lineWidth;
         bool _selectAble;
         MKRouteTravelMode _travelMode;
         MapSpan _bounds;
         MKRouteStep[] _steps;
         double _distance;
         double _travelTime;
         bool _isCalculated;
        /// <summary>
        /// Gets/Sets the source of the route
        /// </summary>
        public Position Source
        {
            get { return _source; }
            set { SetField(ref _source, value); }
        }
        /// <summary>
        /// Gets/Sets the destination of the route
        /// </summary>
        public Position Destination
        {
            get { return _destination; }
            set { SetField(ref _destination, value); }
        }
        /// <summary>
        /// Gets/Sets the width of the line
        /// </summary>
        public float LineWidth
        {
            get { return _lineWidth; }
            set { SetField(ref _lineWidth, value); }
        }
        /// <summary>
        /// Gets/Sets if a route is selectable. If this is <value>false</value> <see cref="Iratrips.MapKit.MKCustomMap.RouteClickedCommand"/> will not get raised for this route
        /// </summary>
        public bool Selectable
        {
            get { return _selectAble; }
            set { SetField(ref _selectAble, value); }
        }
        /// <summary>
        /// Gets/Sets the travel mode for a route calculation.
        /// </summary>
        public MKRouteTravelMode TravelMode
        {
            get { return _travelMode; }
            set { SetField(ref _travelMode, value); }
        }
        /// <summary>
        /// Gets the bounds of the route. This is set automatically by the renderer during route calculation.
        /// </summary>
        public MapSpan Bounds
        {
            get { return _bounds; }
             set { SetField(ref _bounds, value); }
        }
        /// <summary>
        /// Gets the steps of the route
        /// </summary>
        public MKRouteStep[] Steps
        {
            get { return _steps; }
             set { SetField(ref _steps, value); }
        }
        /// <summary>
        /// Gets the distance of the route in meters
        /// </summary>
        public double Distance
        {
            get { return _distance; }
             set { SetField(ref _distance, value); }
        }
        /// <summary>
        /// Gets the travel time of the route in seconds
        /// </summary>
        public double TravelTime
        {
            get { return _travelTime; }
             set { SetField(ref _travelTime, value); }
        }
        public bool IsCalculated
        {
            get { return _isCalculated; }
             set { SetField(ref _isCalculated, value); }
        }
        /// <summary>
        /// Creates a new instance of <see cref="MKRoute"/>
        /// </summary>
        public MKRoute()
        {
            LineWidth = 2.5f;
            Selectable = true;
            TravelMode = MKRouteTravelMode.Driving;
        }
        ///<inheritdoc/>
        void IRouteFunctions.SetBounds(MapSpan bounds)
        {
            Bounds = bounds;
        }
        ///<inheritdoc/>
        void IRouteFunctions.SetSteps(MKRouteStep[] steps)
        {
            Steps = steps;
        }
        ///<inheritdoc/>
        void IRouteFunctions.SetTravelTime(double travelTime)
        {
            TravelTime = travelTime;   
        }
        ///<inheritdoc/>
        void IRouteFunctions.SetDistance(double distance)
        {
            Distance = distance;
        }
        ///<inheritdoc/>
        void IRouteFunctions.SetIsCalculated(bool calculated)
        {
            IsCalculated = calculated;
        }
    }
}
