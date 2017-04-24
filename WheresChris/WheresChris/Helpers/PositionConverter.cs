using Xamarin.Forms.Maps;

namespace WheresChris.Helpers
{
    public class PositionConverter
    {
        public static Xamarin.Forms.Maps.Position Convert(Plugin.Geolocator.Abstractions.Position geolocatorPosition)
        {
            Xamarin.Forms.Maps.Position mapPosition = new Position(geolocatorPosition.Latitude, geolocatorPosition.Longitude);
            return mapPosition;
        }
    }
}
