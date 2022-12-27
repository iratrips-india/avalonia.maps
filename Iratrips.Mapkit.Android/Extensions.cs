using System.Threading.Tasks;
using Android.Content;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Iratrips.MapKit.Overlays;
using Iratrips.MapKit.Api.Google;
using Iratrips.MapKit.Utilities;
using System;
using Avalonia.Media;
using Color = Avalonia.Media.Color;
using Point = Android.Graphics.Point;

namespace Iratrips.MapKit.Droid
{
    /// <summary>
    /// Extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Convert <see cref="LatLng" /> to <see cref="Position"/>
        /// </summary>
        /// <param name="self">Self instance</param>
        /// <returns>Forms Position</returns>
        public static Position ToPosition(this LatLng self)
        {
            return new Position(self.Latitude, self.Longitude);
        }
        /// <summary>
        /// Convert <see cref="Position" /> to <see cref="LatLng"/>
        /// </summary>
        /// <param name="self">Self instance</param>
        /// <returns>Android Position</returns>
        public static LatLng ToLatLng(this Position self)
        {
            return new LatLng(self.Latitude, self.Longitude);
        }
        /// <summary>
        /// Convert <see cref="MapRouteTravelMode"/> to <see cref="GmsDirectionTravelMode"/>
        /// </summary>
        /// <param name="self">Self instance</param>
        /// <returns>Gms Direction API travel mode</returns>
        public static GmsDirectionTravelMode ToGmsTravelMode(this MapRouteTravelMode self)
        {
            switch (self)
            {
                case MapRouteTravelMode.Driving:
                    return GmsDirectionTravelMode.Driving;
                case MapRouteTravelMode.Walking:
                    return GmsDirectionTravelMode.Walking;
                case MapRouteTravelMode.Any:
                    return GmsDirectionTravelMode.Driving;
                default:
                    return GmsDirectionTravelMode.Driving;
            }
        }
        
        /// <summary>
        /// Convert a <see cref="Xamarin.Forms.Point"/> to <see cref="Android.Graphics.Point"/>
        /// </summary>
        /// <param name="point">Self</param>
        /// <returns>A Android point</returns>
        public static Point ToAndroidPoint(this Avalonia.Point point)
        {
            return new Android.Graphics.Point((int)point.X, (int)point.Y);
        }
        
        /// <summary>
        /// Convert a <see cref="Xamarin.Forms.Point"/> to <see cref="Android.Graphics.Point"/>
        /// </summary>
        /// <param name="point">Self</param>
        /// <returns>A Android point</returns>
        public static Android.Graphics.Color ToAndroid(this Color color)
        {
            return new Android.Graphics.Color(color.R, color.G, color.B, color.A);
        }

        /// <summary>
        /// Converts a <see cref="MapSpan"/> to a <see cref="LatLngBounds"/>
        /// </summary>
        /// <param name="self">Self instance</param>
        /// <returns>The <see cref="LatLngBounds"/></returns>
        public static LatLngBounds ToBounds(this MapSpan self)
        {
            var southWest = GmsSphericalUtil.ComputeOffset(self.Center, self.Radius.Meters * Math.Sqrt(2), 225).ToLatLng();
            var northEast = GmsSphericalUtil.ComputeOffset(self.Center, self.Radius.Meters * Math.Sqrt(2), 45).ToLatLng();
            return new LatLngBounds(southWest, northEast);
        }
        /// <summary>
        /// Converts a <see cref="LatLngBounds"/> to a <see cref="MapSpan"/>
        /// </summary>
        /// <param name="self">Self instance</param>
        /// <returns>The <see cref="MapSpan"/></returns>
        public static MapSpan ToMapSpan(this LatLngBounds self)
        {
            var radius = self.Northeast.ToPosition().DistanceTo(self.Southwest.ToPosition()) / 2;
            return MapSpan.FromCenterAndRadius(self.Center.ToPosition(), Distance.FromKilometers(radius));
        }
    }
}