using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Iratrips.MapKit.Api.Google
{
    /// <summary>
    /// Calls the Google Place API
    /// </summary>
    public sealed class GmsPlace
    {
        static string _apiKey;

        static GmsPlace _instance;

        readonly HttpClient _httpClient;

        const string BaseUrl = "https://maps.googleapis.com/maps/api/";
        const string UrlPredictions = "place/autocomplete/json"; // ?input=SEARCHTEXT&key=API_KEY
        const string UrlDetails = "place/details/json";

        /// <summary>
        /// Google Maps Place API instance
        /// </summary>
        public static GmsPlace Instance
        {
            get { return _instance ?? (_instance = new GmsPlace()); }
        }
        /// <summary>
        /// Creates a new instance of <see cref="GmsPlace"/> 
        /// </summary>
        GmsPlace()
        {
            if (_apiKey == null) throw new InvalidOperationException("NO API KEY PROVIDED");

            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri(BaseUrl)
            };
        }
        /// <summary>
        /// Initialize the Google Maps Places API with the api key
        /// </summary>
        /// <param name="apiKey">The api key</param>
        public static void Init(string apiKey)
        {
            _apiKey = apiKey;
        }
        /// <summary>
        /// Performs the API call to the Google Places API to get place predictions 
        /// </summary>
        /// <param name="searchText">Search text</param>
        /// <returns>Result containing place predictions</returns>
        public async Task<GmsPlaceResult> GetPredictions(string searchText)
        {
            var result = await _httpClient.GetAsync(BuildQueryPredictions(searchText));

            if (result.IsSuccessStatusCode)
            {
                var json = await result.Content.ReadAsStringAsync();
                var placeResult = JsonConvert.DeserializeObject<GmsPlaceResult>(json);
                placeResult.SearchTerm = searchText;
                return placeResult;
            }

            return null;
        }
        /// <summary>
        /// Performs the API call to the Google Places API to get place details 
        /// </summary>
        /// <param name="placeId">The google place Id</param>
        /// <returns>Result containing place details</returns>
        public async Task<GmsDetailsResult> GetDetails(string placeId)
        {
            var result = await _httpClient.GetAsync(BuildQueryDetails(placeId));

            if (result.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<GmsDetailsResult>(await result.Content.ReadAsStringAsync());
            }

            return null;
        }
        /// <summary>
        /// Build the query string for predictions
        /// </summary>
        /// <param name="searchText">The search text</param>
        /// <returns>The Query string</returns>
        string BuildQueryPredictions(string searchText)
        {
            return string.Format("{0}?input={1}&key={2}", UrlPredictions, searchText, _apiKey);
        }
        /// <summary>
        /// Build the query string for detail request
        /// </summary>
        /// <param name="placeId">The google place id</param>
        /// <returns>The Query string</returns>
        string BuildQueryDetails(string placeId)
        {
            return string.Format("{0}?placeid={1}&key={2}", UrlDetails, placeId, _apiKey);
        }
    }
}
