using System;
using System.Collections.Generic;
using System.Text;
using StayTogether;
using WheresChris.Droid.Services;
#if __ANDROID__
using StayTogether.Droid.Services;
#endif
#if __IOS__
using WheresChris.iOS;
#endif

namespace WheresChris
{
    public class LocationSenderFactory
    {
        public static LocationSender GetLocationSender()
        {
#if __ANDROID__
                return LocationSenderService.Instance.LocationSender;
#endif
#if __IOS__
            return AppDelegate.LocationManager.LocationSender;
#endif
        }
    }
}
