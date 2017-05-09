using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using StayTogether.Classes;
using StayTogether.Models;
using WheresChris.Helpers;

namespace StayTogether.Helpers
{
    public class PositionHelper
    {
        public static Position GetCentralGeoCoordinate(List<Position> geoCoordinates)
        {
            if (geoCoordinates.Count == 1)
            {
                return geoCoordinates.Single();
            }

            double x = 0;
            double y = 0;
            double z = 0;

            foreach (var geoCoordinate in geoCoordinates)
            {
                var latitude = geoCoordinate.Latitude * Math.PI / 180;
                var longitude = geoCoordinate.Longitude * Math.PI / 180;

                x += Math.Cos(latitude) * Math.Cos(longitude);
                y += Math.Cos(latitude) * Math.Sin(longitude);
                z += Math.Sin(latitude);
            }

            var total = geoCoordinates.Count;

            x = x / total;
            y = y / total;
            z = z / total;

            var centralLongitude = Math.Atan2(y, x);
            var centralSquareRoot = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
            var centralLatitude = Math.Atan2(z, centralSquareRoot);

            var position = new Position
            {
                Longitude = centralLongitude*180/Math.PI,
                Latitude = centralLatitude*180/Math.PI
            };

            return position; //new Position(centralLatitude * 180 / Math.PI, centralLongitude * 180 / Math.PI);
        }

        public static Position GetCentralGeoCoordinate(List<GroupMemberVm> groupMembers)
        {
            var positions = groupMembers.Select(x => new Position
            {
                Latitude = x.Latitude,
                Longitude = x.Longitude
            }).ToList();
            return GetCentralGeoCoordinate(positions);
        }

        public static Position GetCentralGeoCoordinate(List<GroupMemberSimpleVm> groupMembers)
        {
            var positions = groupMembers.Where(x=> !(x.Latitude.Equals(0d ) || x.Longitude.Equals(0d) )).Select(x => new Position
            {
                Latitude = x.Latitude,
                Longitude = x.Longitude
            }).ToList();
            return GetCentralGeoCoordinate(positions);
        }



        public static double GetRadius(List<GroupMemberSimpleVm> groupMembers, Xamarin.Forms.Maps.Position mapCenterPosition)
        {
            var radius = .1;
            if (groupMembers.Count > 1)
            {
                var minLatitude = groupMembers.Min(x => x.Latitude);
                var minLongitude = groupMembers.Min(x => x.Longitude);
                radius = DistanceCalculator.Distance.CalculateMiles(mapCenterPosition.Latitude,
                    mapCenterPosition.Longitude, minLatitude, minLongitude);
            }
            radius = radius < .03 ? .03 : radius;
            radius = radius > 5 ? 5 : radius;
            return radius;
        }

        public static Xamarin.Forms.Maps.Position GetMapCenter(List<GroupMemberSimpleVm> groupMembers)
        {
            return PositionConverter.Convert(GetCentralGeoCoordinate(groupMembers));
        }

        public static async Task<Xamarin.Forms.Maps.Position> GetMapPosition()
        {
            var positionList = new List<Position>();
            CrossGeolocator.Current.DesiredAccuracy = 1;
            for (var i = 0; i < 3; i++)
            {
                var position = await CrossGeolocator.Current.GetPositionAsync(new TimeSpan(0, 0, 10));
                positionList.Add(position);
            }

            var medianLatitude = positionList.OrderBy(l => l.Latitude).ToArray()[1].Latitude;
            var medianLongitude = positionList.OrderBy(l => l.Longitude).ToArray()[1].Longitude;
            var userPosition = new Position
            {
                Latitude = medianLatitude,
                Longitude = medianLongitude
            };

            userPosition = PositionConverter.GetValidGeoLocatorPosition(userPosition);

            var mapPosition = PositionConverter.Convert(userPosition);
            return mapPosition;
        }

        public static bool LocationValid(Xamarin.Forms.Maps.Position position)
        {
            if (Math.Abs(position.Latitude) < 0.1) return false;
            if (Math.Abs(position.Longitude) < 0.1) return false;
            return true;
        }
    }
}
