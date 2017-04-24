using System;
using Android.App;
using Android.Content;
using Android.Locations;
using Android.Util;
using Plugin.Geolocator.Abstractions;

namespace StayTogether.Droid.Classes
{
    [Service]
    public class GpsService
    {
        public static DateTime LastLocationCheck;

        public static Position GetLocation()
        {
            try
            {
                //if (LastLocationUpdateWasMoreThanAnHourAgo())
                //{
                    var criteriaForGpsService = new Criteria
                    {
                        //A constant indicating an approximate accuracy  
                        Accuracy = Accuracy.Fine,
                        PowerRequirement = Power.Low
                    };
                    var locationManager =
                        (LocationManager) Application.Context.GetSystemService(Context.LocationService);

                    var locationProvider = locationManager.GetBestProvider(criteriaForGpsService, true);

                    if (locationManager.IsProviderEnabled(locationProvider))
                    {
                        var location = locationManager.GetLastKnownLocation(locationProvider);
                        var position = new Position
                        {
                            Longitude = location.Longitude,
                            Latitude = location.Latitude
                        };
                        return position;
                    }
                    try
                    {
                        TurnGpsOn();
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(LogPriority.Info, "GetLocation", "GPS:  Unable to turn on.  " + ex.StackTrace);
                    }
                //}
            }
            catch(Exception ex)
            {
                Log.WriteLine(LogPriority.Info, "GetLocation", ex.StackTrace);
            
            }
            return null;

        }

        private static bool LastLocationUpdateWasMoreThanAnHourAgo()
        {
            try
            {
                var interval = DateTime.Now - LastLocationCheck;
                var isMoreThanAnHour = interval > TimeSpan.FromHours(1);
                return isMoreThanAnHour;
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogPriority.Info, "LastLocationUpdateWasMoreThanAnHourAgo", ex.StackTrace);
            }
            return true;
        }

        public static void TurnGpsOn()
        {
            try
            {
                Intent intent = new Intent("android.location.GPS_ENABLED_CHANGE");
                intent.PutExtra("enabled", true);
                Application.Context.SendBroadcast(intent);
                
                LocationManager manager = (LocationManager)Application.Context.GetSystemService(Context.LocationService);
                if (!manager.IsProviderEnabled(LocationManager.GpsProvider))
                {
                    ToggleGps();
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogPriority.Info, "TurnGPSOn", ex.StackTrace);
            }
        }


        private static void ToggleGps()
        {
            try
            {
                Intent poke = new Intent();
                poke.SetClassName("com.android.settings", "com.android.settings.widget.SettingsAppWidgetProvider");
                poke.AddCategory(Intent.CategoryAlternative);
                poke.SetData(Android.Net.Uri.Parse("3"));
                Application.Context.SendBroadcast(poke);
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogPriority.Info, "ToggleGps", ex.StackTrace);
            }

        }
    }
}
