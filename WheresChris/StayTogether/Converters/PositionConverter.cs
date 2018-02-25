using Xamarin.Forms.Maps;

namespace WheresChris.Helpers
{
    public static class PositionConverter
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

        public static TK.CustomMap.Position ToTkPosition(this Position position)
        {
            return new TK.CustomMap.Position(position.Latitude, position.Longitude);
        }

        public static Plugin.Geolocator.Abstractions.Position ToGeolocatorPosition(this TK.CustomMap.Position position)
        {
            var geoPosition = new Plugin.Geolocator.Abstractions.Position();
            geoPosition.Latitude = position.Latitude;
            geoPosition.Longitude = position.Longitude;
            return geoPosition;
        }
    }
}
