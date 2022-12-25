﻿using Iratrips.MapKit.Models;
using Iratrips.MapKit.Overlays;

namespace Iratrips.MapKit.Interfaces
{
    /// <summary>
    /// Containing functions which are called from the renderer to the PCL
    /// </summary>
    public interface IMapFunctions
    {
        /// <summary>
        /// Sets the renderer functions
        /// </summary>
        /// <param name="renderer">The renderer</param>
        void SetRenderer(IRendererFunctions renderer);
        
        /// <summary>
        /// Raises the pin selected event and command
        /// </summary>
        /// <param name="pin">The selected pin</param>
        void RaisePinSelected(MKCustomMapPin pin);
        
        /// <summary>
        /// Raises the pin drag end event and command
        /// </summary>
        /// <param name="pin">The pin which got dragged</param>
        void RaisePinDragEnd(MKCustomMapPin pin);
        
        /// <summary>
        /// Raises the map clicked event and command
        /// </summary>
        /// <param name="position">The position</param>
        void RaiseMapClicked(Position position);
        
        /// <summary>
        /// Raises the map long press event and command
        /// </summary>
        /// <param name="position">The position</param>
        void RaiseMapLongPress(Position position);
        
        /// <summary>
        /// Raises the user location changed event and command
        /// </summary>
        /// <param name="position">The user location</param>
        void RaiseUserLocationChanged(Position position);
        
        /// <summary>
        /// Raises the route clicked event and command
        /// </summary>
        /// <param name="route">The route which got tapped</param>
        void RaiseRouteClicked(MKRoute route);
        
        /// <summary>
        /// Raises the route calculation finished event and command
        /// </summary>
        /// <param name="route">The route</param>
        void RaiseRouteCalculationFinished(MKRoute route);
        
        /// <summary>
        /// Raises the route calculation failed event and command
        /// </summary>
        /// <param name="route">The route</param>
        void RaiseRouteCalculationFailed(RouteCalculationError route);
        
        /// <summary>
        /// Raises the pins ready event and command
        /// </summary>
        void RaisePinsReady();
        
        /// <summary>
        /// Raises the callout clicked event and command
        /// </summary>
        void RaiseCalloutClicked(MKCustomMapPin pin);
        
        /// <summary>
        /// Raises the map ready event and command
        /// </summary>
        void RaiseMapReady();

        /// <summary>
        /// Raised when camera movement has ended, there are no pending animations and the user has stopped interacting with the map.
        /// </summary>
        void RaiseCameraIdeal();

        /// <summary>
        /// Raised when the camera starts moving after it has been idle or when the reason for camera motion has changed.
        /// </summary>
        void RaiseCameraMoveStarted();
    }
}
