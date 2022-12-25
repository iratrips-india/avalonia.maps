﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace Iratrips.MapKit.Api.Google
{
    /// <summary>
    /// Google Polyline class
    /// </summary>
    public class GmsPolyline
    {
        /// <summary>
        /// Gets the points as string
        /// </summary>
        public string Points { get; set; }
        /// <summary>
        /// Gets the converted positions
        /// </summary>
        [JsonIgnore]
        public IEnumerable<Position> Positions
        {
            get
            {
                return GooglePoints.Decode(Points);
            }
        }
    }
}
