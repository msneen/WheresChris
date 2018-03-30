using System.Collections.Generic;
using System.Linq;
using CoreLocation;
using Microsoft.AppCenter.Analytics;
using Plugin.Geolocator.Abstractions;
using StayTogether;
using StayTogether.Models;
using UIKit;
using WheresChris.Helpers;
using Xamarin.Forms;

namespace WheresChris.iOS.Classes
{
    public class LocationManager
    {
        //private CLLocation _lastLocation;
        private LocationSender _locationSender;
        //private readonly SendMeter _sendMeter;
        public string UserPhoneNumber { get; set; }

        public LocationSender LocationSender => _locationSender;

        public CLLocationManager ClLocationManager { get; }

        public LocationManager()
        {
            //_sendMeter = new SendMeter(15, TimeSpan.FromSeconds(15));
            ClLocationManager = new CLLocationManager
            {
                PausesLocationUpdatesAutomatically = false
            };

            // iOS 8 has additional permissions requirements
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                ClLocationManager.RequestAlwaysAuthorization(); // works in background
                
            }

            if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
            {
                ClLocationManager.AllowsBackgroundLocationUpdates = true;
            }

        }

        public void StartLocationUpdates()
        {
            var locationPermissionGranted = PermissionHelper.HasOrRequestLocationPermission().Result;
            if (!locationPermissionGranted) return;


            if (!CLLocationManager.LocationServicesEnabled) return;

            //set the desired accuracy, in meters
            ClLocationManager.DesiredAccuracy = 1;
            ClLocationManager.LocationsUpdated += (sender, e) =>
            {
                Analytics.TrackEvent("LocationManager_LocationsUpdated_started");
                // fire our custom Location Updated event
                if (e.Locations == null || e.Locations.Length <= -1) return;

                Analytics.TrackEvent("LocationManager_LocationsUpdated", new Dictionary<string, string>
                {
                    {"Latitude", e.Locations[0].Coordinate.Latitude.ToString()},
                    {"Longitude", e.Locations[0].Coordinate.Longitude.ToString()},
                });

                var locationList = e.Locations.ToList();
                var count = locationList.Count;
                var medianLatitude = locationList.OrderBy(l => l.Coordinate.Latitude).ToArray()[count / 2].Coordinate.Latitude;
                var medianLongitude = locationList.OrderBy(l => l.Coordinate.Longitude).ToArray()[count / 2].Coordinate.Longitude;

                Analytics.TrackEvent("LocationManager_LocationsUpdated_MedianCalculated", new Dictionary<string, string>
                {
                    {"Latitude", medianLatitude.ToString()},
                    {"Longitude", medianLongitude.ToString()},
                });

                //_lastLocation = new CLLocation(medianLatitude, medianLongitude);

                MessagingCenter.Send(new MessagingCenterSender(), LocationSender.PositionUpdatedMsg, new Position
                {
                    Latitude = medianLatitude,
                    Longitude = medianLongitude,
                    Accuracy = ClLocationManager.DesiredAccuracy
                });

                //await SendPositionUpdate();//If I os doesn't work, this could be the problem
                Analytics.TrackEvent("LocationManager_LocationsUpdated_SendPositionUpdate_Sent");
            };

            _locationSender = LocationSender.GetInstance();

            ClLocationManager.StartUpdatingLocation();
            //Crashes.TrackError("LocationManager_ClLocationManager_StartUpdatingLocation");
        }
    }
}