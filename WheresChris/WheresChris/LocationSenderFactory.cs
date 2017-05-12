using StayTogether;

namespace WheresChris
{
    public class LocationSenderFactory
    {
        public static LocationSender GetLocationSender()
        {
            return LocationSender.Instance;
        }
    }
}
