using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plugin.Geolocator;
using StayTogether.Classes;
using StayTogether.Models;
using TK.CustomMap;
using WheresChris.Helpers;
using Position = Plugin.Geolocator.Abstractions.Position;

namespace StayTogether.Helpers
{
    public static class PositionHelper
    {
        public static double MinAccuracy { get; set; }
        public static double MaxAccuracy { get; set; }
        public static double AvgAccuracy { get; set; }

        public static event EventHandler OnAccuracyChanged;

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
            lock (groupMembers)
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
        }

        public static Xamarin.Forms.Maps.Position GetMapCenter(List<GroupMemberSimpleVm> groupMembers)
        {
            return PositionConverter.Convert(GetCentralGeoCoordinate(groupMembers));
        }

        public static async Task<Xamarin.Forms.Maps.Position?> GetMapPosition()
        {
            var positionList = new List<Plugin.Geolocator.Abstractions.Position>();
            CrossGeolocator.Current.DesiredAccuracy = 30;
            for (var i = 0; i < 4; i++)
            {
                var position = await CrossGeolocator.Current.GetPositionAsync(new TimeSpan(0, 0, 10));
                AddToMinMaxAgg(position);
                positionList.Add(position);
                await Task.Delay(100);
            }

            var userPosition = GetMedianPosition(positionList);
            return GetMapPosition(userPosition);
        }

        public static Xamarin.Forms.Maps.Position? GetMapPosition(Position userPosition)
        {
            if (userPosition == null) return null;
            if (!LocationValid(userPosition)) return null;
            var mapPosition = PositionConverter.Convert(userPosition);
            return mapPosition;
        }

        private static void AddToMinMaxAgg(Position position)
        {
            if (position.Accuracy < MinAccuracy)
            {
                MinAccuracy = position.Accuracy;
            }
            if (position.Accuracy > MaxAccuracy)
            {
                MaxAccuracy = position.Accuracy;
            }

            var lastAvg = (AvgAccuracy + position.Accuracy)/2;
            AvgAccuracy = lastAvg;
            OnAccuracyChanged?.Invoke(null, new EventArgs());
        }

        private static  Position GetMedianPosition(List<Position> positionListAll)
        {
            if (positionListAll == null) return null;
            if (positionListAll.Count < 1) return null;
            //.Accuracy is in meters
            //I'm taking about 20 location readings, then sorting from most to least accurate, and taking the top 3 most accurate.
            var positionList = positionListAll.Where(x=>x.Accuracy < 100.0).OrderBy(p => p.Accuracy).Skip(0).Take(3).ToList();
            if  (!positionList.Any()) return null;

            var middlePosition = (positionList.Count < 3) ? 0 : 1;//mws:should I be taking the top one always(ie 0), since I'm sorting by accuracy?
            
            var medianLatitude = positionList.OrderBy(l => l.Latitude).ToArray()[middlePosition].Latitude;
            var medianLongitude = positionList.OrderBy(l => l.Longitude).ToArray()[middlePosition].Longitude;
            var userPosition = new Plugin.Geolocator.Abstractions.Position
            {
                Latitude = medianLatitude,
                Longitude = medianLongitude
            };
            return userPosition;
        }

        public static bool LocationValid(this Plugin.Geolocator.Abstractions.Position position)
        {
            return LocationValid(position.Latitude, position.Longitude);
        }

        public static bool LocationValid(this Xamarin.Forms.Maps.Position position)
        {
            return LocationValid(position.Latitude, position.Longitude);
        }

        public static bool LocationValid(this TK.CustomMap.Position position)
        {
            return LocationValid(position.Latitude, position.Longitude);
        }

        private static bool LocationValid(double latitude, double longitude)
        {
            return !(Math.Abs(latitude) < 0.1) || !(Math.Abs(longitude) < 0.1);
        }
    }
}
