using System.Threading.Tasks;
using StayTogether;

namespace WheresChris
{
    public class LocationSenderFactory
    {
        public static async Task<LocationSender> GetLocationSender()
        {
            return await LocationSender.GetInstanceAsync();
        }
    }
}
