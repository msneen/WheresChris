using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreLocation;
using Microsoft.Azure.Mobile.Analytics;
using Plugin.Geolocator.Abstractions;
using StayTogether;
using StayTogether.Classes;
using StayTogether.Helpers;
using StayTogether.Location;
using UIKit;
using WheresChris.Helpers;

namespace WheresChris.iOS.Classes
{
    public class LocationManager
    {
        private CLLocation _lastLocation;
        private LocationSender _locationSender;
        private readonly SendMeter _sendMeter;
        public string UserPhoneNumber { get; set; }

        public LocationSender LocationSender => _locationSender;

        public CLLocationManager ClLocationManager { get; }

        public LocationManager()
        {
            _sendMeter = new SendMeter(15, TimeSpan.FromSeconds(15));
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
            Analytics.TrackEvent("LocationManager_StartLocationUpdates_Start");
            if (!CLLocationManager.LocationServicesEnabled) return;

            //set the desired accuracy, in meters
            ClLocationManager.DesiredAccuracy = 1;
            ClLocationManager.LocationsUpdated += async (sender, e) =>
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

                _lastLocation = new CLLocation(medianLatitude, medianLongitude);
                await SendPositionUpdate();
                Analytics.TrackEvent("LocationManager_LocationsUpdated_SendPositionUpdate_Sent");
            };

            _locationSender = LocationSender.GetInstance();
            Analytics.TrackEvent("LocationManager_LocationSender_GotInstance", new Dictionary<string, string>
                {
                    {"IsInitialized", _locationSender.IsInitialized.ToString()},
                });


            ClLocationManager.StartUpdatingLocation();
            Analytics.TrackEvent("LocationManager_ClLocationManager_StartUpdatingLocation");
        }

        private async Task SendPositionUpdate()
        {
            var position = await GetPosition();
            UserPhoneNumber = SettingsHelper.GetPhoneNumber();
            var nickname = SettingsHelper.GetNickname();

            Analytics.TrackEvent("LocationManager_SendPositionUpdate_Start", new Dictionary<string, string>
            {
                { "UserPhoneNumber",  UserPhoneNumber},
            });

            //                                                if more than x seconds or x feet from last location, send update to server
            if (string.IsNullOrWhiteSpace(UserPhoneNumber) || !_sendMeter.CanSend(position)) return;

            
            var groupMemberVm = new GroupMemberVm
            {
                //Get Group Member Properties
                Name = nickname,
                PhoneNumber = UserPhoneNumber,
                Latitude = position.Latitude,
                Longitude = position.Longitude
            };

            //Send position update
            _locationSender = await LocationSender.GetInstanceAsync();
            await _locationSender.SendUpdatePosition(groupMemberVm);

            Analytics.TrackEvent("Sent", new Dictionary<string, string>
            {
                { "LocationSenderInitialized",  _locationSender.IsInitialized.ToString()},
            });
        }

        public async Task<Position> GetPosition()
        {
            var currentPosition = await PositionHelper.GetMapPosition();
            if (currentPosition.HasValue &&  PositionHelper.LocationValid(currentPosition.Value))
            {
                var averageList = new List<CLLocation>
                {
                    new CLLocation(currentPosition.Value.Latitude, currentPosition.Value.Longitude),
                    _lastLocation
                };
                var averageLatitude = averageList.Average(x => x.Coordinate.Latitude);
                var averageLongitude = averageList.Average(x => x.Coordinate.Longitude);

                _lastLocation = new CLLocation(averageLatitude, averageLongitude);
            }

            if (_lastLocation == null) return null;

            var position = new Position
            {
                Latitude = _lastLocation.Coordinate.Latitude,
                Longitude = _lastLocation.Coordinate.Longitude
            };
            return position;
        }
    }
}