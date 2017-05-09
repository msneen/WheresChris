using Plugin.Geolocator.Abstractions;
using StayTogether.Classes;

namespace StayTogether.Location
{
    public class GroupMemberConverter
    {
        public static GroupMemberVm Convert(Position position)
        {
            return new GroupMemberVm
            {
                Latitude = position.Latitude,
                Longitude = position.Longitude
            };
        }

        public static GroupMemberVm Convert(Xamarin.Forms.Maps.Position position)
        {
            return new GroupMemberVm
            {
                Latitude = position.Latitude,
                Longitude = position.Longitude
            };
        }
    }
}
