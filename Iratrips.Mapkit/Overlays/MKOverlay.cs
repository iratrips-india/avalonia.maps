﻿using Avalonia.Media;
using System;

namespace Iratrips.MapKit.Overlays
{
    /// <summary>
    /// Base overlay class
    /// </summary>
    public abstract class MKOverlay : MKBase
    {
        Color _color;

        /// <summary>
        /// Gets/Sets the main color of the overlay.
        /// </summary>
        public Color Color 
        {
            get => _color;
            set => SetField(ref _color, value);
        }
        /// <summary>
        /// Gets the id of the <see cref="MKOverlay"/>
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();
        /// <summary>
        /// Checks whether the <see cref="Id"/> of the overlays match
        /// </summary>
        /// <param name="obj">The <see cref="MKOverlay"/> to compare</param>
        /// <returns>true of the ids match</returns>
        public override bool Equals(object obj) => 
            obj is MKOverlay overlay && Id.Equals(overlay.Id);

        /// <inheritdoc />
        public override int GetHashCode() => Id.GetHashCode();
    }
}
