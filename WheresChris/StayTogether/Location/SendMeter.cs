using System;
using Plugin.Geolocator.Abstractions;

namespace StayTogether.Location
{
    public class SendMeter
    {
        private readonly int _distanceChangeInFeet;
        private readonly TimeSpan _timeChange;
        private Position _lastLocation;
        private DateTime _lastUpDateTime;

        public SendMeter(int distanceChangeInFeet, TimeSpan timeChange)
        {
            _distanceChangeInFeet = distanceChangeInFeet;
            _timeChange = timeChange;
        }

        public bool CanSend(Position currentLocation)
        {
            if (_lastLocation == null)
            {
                //if we have never set the last position or time, set it now
                UpdateLastLocationAndTime(currentLocation);
                return true;
            }

            //get the distance from the last position update
            var distanceInFeet = Helpers.DistanceCalculator.Distance.CalculateFeet(_lastLocation.Latitude, 
                                              _lastLocation.Longitude, 
                                              currentLocation.Latitude,
                                              currentLocation.Longitude);

            //get the time since last position update
            var intervalSinceLastUpdate = DateTime.Now - _lastUpDateTime;


            if (!(distanceInFeet > _distanceChangeInFeet) && intervalSinceLastUpdate <= _timeChange) return false;
            
            //and set the current time and position as the last position for future comparison
            UpdateLastLocationAndTime(currentLocation);

            //if time or distance is greater than values, tell caller to send an update
            return true;
        }

        private void UpdateLastLocationAndTime(Position currentLocation)
        {
            _lastLocation = currentLocation;
            _lastUpDateTime = DateTime.Now;
        }
    }
}
