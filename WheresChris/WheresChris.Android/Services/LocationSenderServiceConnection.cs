using System;
using Android.Content;
using Android.OS;
using WheresChris.Droid;
using WheresChris.Droid.Services;

namespace StayTogether.Droid.Services
{
    public class LocationSenderServiceConnection : Java.Lang.Object, IServiceConnection
    {
        private readonly MainActivity _activity;

        public LocationSenderServiceConnection(MainActivity activity)
        {
            _activity = activity;
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            var locationSenderBinder = service as LocationSenderBinder;
            if (locationSenderBinder == null) return;

            _activity.Binder = locationSenderBinder;
            _activity.IsBound = true;
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            _activity.IsBound = false;            
        }
    }
}