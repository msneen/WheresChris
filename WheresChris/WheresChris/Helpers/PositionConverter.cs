using Xamarin.Forms.Maps;

namespace WheresChris.Helpers
{
    public class PositionConverter
    {
        public static Position Convert(Plugin.Geolocator.Abstractions.Position geolocatorPosition)
        {
            Position mapPosition = new Position(geolocatorPosition.Latitude, geolocatorPosition.Longitude);
            return mapPosition;
        }

        public static Plugin.Geolocator.Abstractions.Position GetValidGeoLocatorPosition(Plugin.Geolocator.Abstractions.Position userPosition)
        {
            if (userPosition == null || (userPosition.Latitude < .1 && userPosition.Longitude < .1))
            {
                return new Plugin.Geolocator.Abstractions.Position
                {
                    Latitude = 32.7157,
                    Longitude = -117.1611
                };

            }
            return userPosition;
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
