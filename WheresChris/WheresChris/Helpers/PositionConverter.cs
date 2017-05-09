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

        public static Plugin.Geolocator.Abstractions.Position GetInitialPosition(Plugin.Geolocator.Abstractions.Position userPosition)
        {
            return new Plugin.Geolocator.Abstractions.Position
            {
                Latitude = 32.7157,
                Longitude = -117.1611
            };
        }

        public static Position GetValidMapPosition(Position? userPosition)
        {
            if (userPosition == null || (userPosition.Value.Latitude < .1 && userPosition.Value.Longitude < .1))
            {
                return new Position(32.7157, -117.1611);

            }
            return userPosition.Value;
        }
    }
}
