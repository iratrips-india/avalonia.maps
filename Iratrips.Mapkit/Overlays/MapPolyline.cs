﻿using System.Collections.Generic;

namespace Iratrips.MapKit.Overlays
{
    public class MapPolyline : MapOverlay
    {
        List<Position> _lineCoordinates;
        float _lineWidth;

        /// <summary>
        /// Coordinates of the line
        /// </summary>
        public List<Position> LineCoordinates
        {
            get => _lineCoordinates;
            set => SetField(ref _lineCoordinates, value);
        }
        /// <summary>
        /// Gets/Sets the width of the line
        /// </summary>
        public float LineWidth
        {
            get => _lineWidth;
            set => SetField(ref _lineWidth, value);
        }
        /// <summary>
        /// Creates a new instance of <see cref="MapPolyline"/>
        /// </summary>
        public MapPolyline() => 
            _lineCoordinates = new List<Position>();
    }
}
