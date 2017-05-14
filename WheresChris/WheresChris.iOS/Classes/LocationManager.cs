using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreLocation;
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

        public async Task StartLocationUpdates()
        {
            if (CLLocationManager.LocationServicesEnabled)
            {
                //set the desired accuracy, in meters
                ClLocationManager.DesiredAccuracy = 1;
                ClLocationManager.LocationsUpdated += async (sender, e) =>
                {
                    // fire our custom Location Updated event
                    if (e.Locations == null || e.Locations.Length <= -1) return;

                    var locationList = e.Locations.ToList();
                    var count = locationList.Count;
                    var medianLatitude = locationList.OrderBy(l => l.Coordinate.Latitude).ToArray()[count / 2].Coordinate.Latitude;
                    var medianLongitude = locationList.OrderBy(l => l.Coordinate.Longitude).ToArray()[count / 2].Coordinate.Longitude;

                    _lastLocation = new CLLocation(medianLatitude, medianLongitude); 
                    await SendPositionUpdate();
                };

                ClLocationManager?.StartUpdatingLocation();

                _locationSender = await LocationSender.GetInstance();
            }
        }

        private async Task SendPositionUpdate()
        {
            var position = await GetPosition();
            UserPhoneNumber = SettingsHelper.GetPhoneNumber();
            //                                                if more than x seconds or x feet from last location, send update to server
            if (string.IsNullOrWhiteSpace(UserPhoneNumber) || !_sendMeter.CanSend(position)) return;

            
            var groupMemberVm = new GroupMemberVm
            {
                //Get Group Member Properties
                Name = "iPhoneTester",
                PhoneNumber = UserPhoneNumber,
                Latitude = _lastLocation.Coordinate.Latitude,
                Longitude = _lastLocation.Coordinate.Longitude
            };

            //Send position update
            var sendUpdatePosition = _locationSender?.SendUpdatePosition(groupMemberVm);
            if (sendUpdatePosition != null)
                await sendUpdatePosition;
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