﻿using Avalonia.Media;
using System.Collections.Generic;

namespace Iratrips.MapKit.Overlays
{
    /// <summary>
    ///     A polygon to display on the map
    /// </summary>
    public class MKPolygon : MKOverlay
    {
        private List<Position> _coordinates;
        private Color _strokeColor;
        private float _strokeWidth;

        /// <summary>
        ///     Creates a new instance of <c>MKPolygon</c>
        /// </summary>
        public MKPolygon()
        {
            _coordinates = new List<Position>();
        }

        /// <summary>
        ///     List of positions of the polygon
        /// </summary>
        public List<Position> Coordinates
        {
            get => _coordinates;
            set => SetField(ref _coordinates, value);
        }

        /// <summary>
        ///     Gets/Sets the stroke color of the polygon
        /// </summary>
        public Color StrokeColor
        {
            get => _strokeColor;
            set => SetField(ref _strokeColor, value);
        }

        /// <summary>
        ///     Gets/Sets the width of the stroke
        /// </summary>
        public float StrokeWidth
        {
            get => _strokeWidth;
            set => SetField(ref _strokeWidth, value);
        }
    }
}