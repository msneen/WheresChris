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
        public static Plugin.Geolocator.Abstractions.Position GetCentralGeoCoordinate(List<Plugin.Geolocator.Abstractions.Position> geoCoordinates)
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

            var position = new Plugin.Geolocator.Abstractions.Position
            {
                Longitude = centralLongitude*180/Math.PI,
                Latitude = centralLatitude*180/Math.PI
            };

            return position; //new Position(centralLatitude * 180 / Math.PI, centralLongitude * 180 / Math.PI);
        }

        public static Plugin.Geolocator.Abstractions.Position GetCentralGeoCoordinate(List<GroupMemberVm> groupMembers)
        {
            var positions = groupMembers.Select(x => new Plugin.Geolocator.Abstractions.Position
            {
                Latitude = x.Latitude,
                Longitude = x.Longitude
            }).ToList();
            return GetCentralGeoCoordinate(positions);
        }

        public static Plugin.Geolocator.Abstractions.Position GetCentralGeoCoordinate(List<GroupMemberSimpleVm> groupMembers)
        {
            var positions = groupMembers.Where(x=> !(x.Latitude.Equals(0d ) || x.Longitude.Equals(0d) )).Select(x => new Plugin.Geolocator.Abstractions.Position
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

        public static async Task<Xamarin.Forms.Maps.Position?> GetMapPosition()
        {
            var positionList = new List<Plugin.Geolocator.Abstractions.Position>();
            CrossGeolocator.Current.DesiredAccuracy = 100;
            for (var i = 0; i < 3; i++)
            {
                var position = await CrossGeolocator.Current.GetPositionAsync(new TimeSpan(0, 0, 10));
                positionList.Add(position);
                await Task.Delay(10000);
            }

            var userPosition = GetMedianPosition(positionList);
            if (!LocationValid(userPosition)) return null;
            var mapPosition = PositionConverter.Convert(userPosition);
            return mapPosition;
        }

        private static Position GetMedianPosition(List<Position> positionList)
        {
            var medianLatitude = positionList.OrderBy(l => l.Latitude).ToArray()[1].Latitude;
            var medianLongitude = positionList.OrderBy(l => l.Longitude).ToArray()[1].Longitude;
            var userPosition = new Plugin.Geolocator.Abstractions.Position
            {
                Latitude = medianLatitude,
                Longitude = medianLongitude
            };
            return userPosition;
        }

        public static bool LocationValid(Plugin.Geolocator.Abstractions.Position position)
        {
            return LocationValid(position.Latitude, position.Longitude);
        }

        public static bool LocationValid(Xamarin.Forms.Maps.Position position)
        {
            return LocationValid(position.Latitude, position.Longitude);
        }

        private static bool LocationValid(double latitude, double longitude)
        {
            return !(Math.Abs(latitude) < 0.1) || !(Math.Abs(longitude) < 0.1);
        }
    }
}
