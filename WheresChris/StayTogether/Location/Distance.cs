using System;
using System.Runtime.CompilerServices;

namespace StayTogether.Location
{
	internal static class Distance
	{
		private static readonly Func<double, double> ConvertDegreesToRadians = (double degrees) => Math.PI * degrees / 180.0;
		private static readonly Func<double, double> ConvertRadiansToDegrees = (double radians) => 180.0 * radians / Math.PI;
	
		/// <summary>
		/// Calculate distance between two points, identified by their latitude and longitude.
		/// Parameters should be submitted in degrees, not radians.
		/// </summary>
		public static double Calculate(double latitude1, double longitude1, double latitude2, double longitude2)
		{
			var longitudeDegrees = longitude1 - longitude2;
			var latitude1Radians = ConvertDegreesToRadians(latitude1);
			var latitude2Radians = ConvertDegreesToRadians(latitude2);
			var longitudeRadians = ConvertDegreesToRadians(longitudeDegrees);
			var distance = Math.Sin(latitude1Radians) * Math.Sin(latitude2Radians) +
						   Math.Cos(latitude1Radians) * Math.Cos(latitude2Radians) * Math.Cos(longitudeRadians);
			distance = Math.Acos(distance);
			distance = ConvertRadiansToDegrees(distance);
            var statuteMiles = distance * 60 * 1.1515; // Imperial measurement (in statute miles)
            var feet = statuteMiles * 5280;
            return feet;
        }
	}
}