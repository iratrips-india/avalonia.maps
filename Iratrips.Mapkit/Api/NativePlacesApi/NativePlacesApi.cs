namespace Iratrips.MapKit.Api
{
    /// <summary>
    /// Manages instance of <see cref="INativePlacesApi"/>
    /// </summary>
    public static class NativePlacesApi
    {
        /// <summary>
        /// Gets an instance of <see cref="INativePlacesApi"/>
        /// </summary>
        public static INativePlacesApi Instance { get; set; }
    }
}
