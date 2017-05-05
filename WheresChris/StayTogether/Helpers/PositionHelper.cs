using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Plugin.Geolocator.Abstractions;
using StayTogether.Classes;
using StayTogether.Models;

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
            var centralSquareRoot = Math.Sqrt(x * x + y * y);
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
            var positions = groupMembers.Select(x => new Position
            {
                Latitude = x.Latitude,
                Longitude = x.Longitude
            }).ToList();
            return GetCentralGeoCoordinate(positions);
        }

        public static Xamarin.Forms.Maps.Position ConvertPluginPositionToMapPosition(
            Position plugInPosition)
        {
            return new Xamarin.Forms.Maps.Position(plugInPosition.Latitude, plugInPosition.Longitude);
        }
    }
}
