using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Gms.Common.Apis;
using Android.Gms.Location.Places;
using Android.Gms.Maps.Model;
using Iratrips.Mapkit.Android;
using Iratrips.MapKit.Api;

namespace Iratrips.MapKit.Droid
{
    /// <inheritdoc />
    public class NativePlacesApi : INativePlacesApi
    {

         GoogleApiClient _apiClient;
         AutocompletePredictionBuffer _buffer;

        ///<inheritdoc/>
        public async Task<IEnumerable<IPlaceResult>> GetPredictions(string query, MapSpan bounds)
        {
            if (_apiClient == null || !_apiClient.IsConnected) Connect();

            List<IPlaceResult> result = new List<IPlaceResult>();

            double mDistanceInMeters = bounds.Radius.Meters;
            
            double latRadian = bounds.LatitudeDegrees;

            double degLatKm = 110.574235;
            double degLongKm = 110.572833 * Math.Cos(latRadian);
            double deltaLat = mDistanceInMeters / 1000.0 / degLatKm;
            double deltaLong = mDistanceInMeters / 1000.0 / degLongKm;

            double minLat = bounds.Center.Latitude - deltaLat;
            double minLong = bounds.Center.Longitude - deltaLong;
            double maxLat = bounds.Center.Latitude + deltaLat;
            double maxLong = bounds.Center.Longitude + deltaLong;

            if (_buffer != null)
            {
                _buffer.Dispose();
                _buffer = null;
            }

            _buffer = await PlacesClass.GeoDataApi.GetAutocompletePredictionsAsync(
                _apiClient, 
                query, 
                new LatLngBounds(new LatLng(minLat, minLong), new LatLng(maxLat, maxLong)), 
                null);
            
            if (_buffer != null)
            {
                result.AddRange(_buffer.Select(i => 
                    new NativeAndroidPlaceResult
                    {
                        Description = i.GetPrimaryText(null),
                        Subtitle = i.GetSecondaryText(null),
                        PlaceId = i.PlaceId,
                    }));
            }
            return result;
        }
        ///<inheritdoc/>
        public void Connect()
        {
            if(_apiClient == null)
            {
                _apiClient = new GoogleApiClient.Builder(Platform.AppContext)
                    .AddApi(PlacesClass.GEO_DATA_API)
                    .Build();
            }
            if(!_apiClient.IsConnected && !_apiClient.IsConnecting)
            {
                _apiClient.Connect();
            }
        }
        ///<inheritdoc/>
        public void DisconnectAndRelease()
        {
            if (_apiClient == null) return;

            if (_apiClient.IsConnected)
                _apiClient.Disconnect();

            _apiClient.Dispose();
            _apiClient = null;

            if (_buffer != null)
            {
                _buffer.Dispose();
                _buffer = null;
            }
        }
        /// <inheritdoc/>
        public async Task<PlaceDetails> GetDetails(string id)
        {
            if (_apiClient == null || !_apiClient.IsConnected) Connect();

            var nativeResult = await PlacesClass.GeoDataApi.GetPlaceByIdAsync(_apiClient, id);

            if (nativeResult == null || !nativeResult.Any()) return null;

            var nativeDetails = nativeResult.First();

            return new PlaceDetails
            {
                Coordinate = nativeDetails.LatLng.ToPosition(),
                FormattedAddress = nativeDetails.AddressFormatted.ToString(),
                InternationalPhoneNumber = nativeDetails.PhoneNumberFormatted?.ToString(),
                Website = nativeDetails.WebsiteUri?.ToString()
            };
        }
    }
}