using Xamarin.Forms.Maps;

namespace WheresChris.Helpers
{
    public class PositionConverter
    {
        public static Position Convert(Plugin.Geolocator.Abstractions.Position geolocatorPosition)
        {
            return new Xamarin.Forms.Maps.Position(geolocatorPosition.Latitude, geolocatorPosition.Longitude);
        }

        public static Plugin.Geolocator.Abstractions.Position Convert(Xamarin.Forms.Maps.Position geolocatorPosition)
        {
            return new Plugin.Geolocator.Abstractions.Position
            {
                Latitude = geolocatorPosition.Latitude,
                Longitude = geolocatorPosition.Longitude
            };
        }
    }
}
