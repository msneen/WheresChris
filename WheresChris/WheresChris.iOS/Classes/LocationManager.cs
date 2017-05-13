using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreLocation;
using Microsoft.Azure.Mobile.Analytics;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using StayTogether;
using StayTogether.Classes;
using StayTogether.Group;
using StayTogether.Helpers;
using StayTogether.Location;
using UIKit;
using WheresChris.Helpers;

namespace WheresChris.iOS.Classes
{
    public class LocationManager
    {

        CLLocationManager _clLocationManager;
        private CLLocation _lastLocation;
        private LocationSender _locationSender;
        private SendMeter _sendMeter;
        public string UserPhoneNumber { get; set; }

        public LocationSender LocationSender => _locationSender;

        public CLLocationManager ClLocationManager
        {
            get
            {
                return _clLocationManager;               
            }
            private set
            {
                _clLocationManager = value;
            }
        }

        public LocationManager()
        {
            _sendMeter = new SendMeter(150, TimeSpan.FromSeconds(30));
            _clLocationManager = new CLLocationManager
            {
                PausesLocationUpdatesAutomatically = false
            };

            // iOS 8 has additional permissions requirements
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                _clLocationManager.RequestAlwaysAuthorization(); // works in background
                
            }

            if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
            {
                _clLocationManager.AllowsBackgroundLocationUpdates = true;
            }

        }

        private async Task InitializeLocationSender()
        {
            _locationSender = new LocationSender();
            await _locationSender.InitializeSignalRAsync();
        }

        public async Task StartLocationUpdates()
        {
            if (CLLocationManager.LocationServicesEnabled)
            {
                //set the desired accuracy, in meters
                _clLocationManager.DesiredAccuracy = 1;
                _clLocationManager.LocationsUpdated += async (object sender, CLLocationsUpdatedEventArgs e) =>
                {
                    // fire our custom Location Updated event
                    if (e.Locations == null || e.Locations.Length <= -1) return;

                    var locationList = e.Locations.ToList();
                    var count = locationList.Count;
                    var medianLatitude = locationList.OrderBy(l => l.Coordinate.Latitude).ToArray()[count / 2].Coordinate.Latitude;
                    var medianLongitude = locationList.OrderBy(l => l.Coordinate.Longitude).ToArray()[count / 2].Coordinate.Longitude;

                    _lastLocation = new CLLocation(medianLatitude, medianLongitude); //e.Locations[e.Locations.Length - 1];
                    //if more than 2 minutes or 100 feet from last location, send update to server
                    await SendPositionUpdate();
                };

                _clLocationManager?.StartUpdatingLocation();

                await InitializeLocationSender();
            }
        }

        private async Task SendPositionUpdate()
        {
            var position = await GetPosition();
            UserPhoneNumber = SettingsHelper.GetPhoneNumber();

            //Analytics.TrackEvent("IPhone_SendPositionUpdate_Entered", new Dictionary<string, string>
            //        {
            //            { "PhoneNumber", UserPhoneNumber},
            //            {"Latitude", position?.Latitude.ToString() },
            //            {"Longitude", position?.Longitude.ToString() },
            //            {"LastLatitude", _lastLocation?.Coordinate.Latitude.ToString() },
            //            {"LastLongitude", _lastLocation?.Coordinate.Longitude.ToString() }
            //        });

            if (string.IsNullOrWhiteSpace(UserPhoneNumber) || !_sendMeter.CanSend(position)) return;

            //Send position update
            var groupMemberVm = new GroupMemberVm
            {
                //Get Group Member Properties
                Name = "iPhoneTester",
                PhoneNumber = UserPhoneNumber,
                Latitude = _lastLocation.Coordinate.Latitude,
                Longitude = _lastLocation.Coordinate.Longitude
            };
            _locationSender?.SendUpdatePosition(groupMemberVm);

            //Analytics.TrackEvent("IPhone_SendPositionUpdate_Sent", new Dictionary<string, string>
            //        {
            //            { "PhoneNumber", UserPhoneNumber},
            //            {"Latitude", position?.Latitude.ToString() },
            //            {"Longitude", position?.Longitude.ToString() },
            //            {"LastLatitude", _lastLocation?.Coordinate.Latitude.ToString() },
            //            {"LastLongitude", _lastLocation?.Coordinate.Longitude.ToString() }
            //        });
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