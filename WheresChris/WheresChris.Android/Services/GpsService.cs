using System;
using System.Diagnostics;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Locations;
using Android.Util;
// using Plugin.Geolocator.Abstractions;

namespace WheresChris.Droid.Services
{
	[Service]
	public class GpsService
	{
		public static bool IsGpsEnabled()
		{
			var enabled = false;
			var locationManager = (LocationManager)Application.Context.GetSystemService(Context.LocationService);
			if (locationManager != null)
			{
				var providerName = GetGpsProviderName(locationManager);
				if (!string.IsNullOrEmpty(providerName))
				{
					enabled = locationManager?.IsProviderEnabled(providerName) ?? false;
				}
			}
			return enabled;
		}

		public static bool EnableGps()
		{
			var enabled = false;
			try
			{
				var locationManager = (LocationManager)Application.Context.GetSystemService(Context.LocationService);
				if (locationManager != null)
				{
					var providerName = GetGpsProviderName(locationManager);
					if (!string.IsNullOrEmpty(providerName))
					{
						enabled = locationManager?.IsProviderEnabled(providerName) ?? false;
						if (!enabled)
						{
							EnableGpsService();
							enabled = locationManager?.IsProviderEnabled(providerName) ?? false;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.WriteLine(LogPriority.Info, "GetLocation", "GPS:  Unable to turn on - are location services turned off?  " + ex.StackTrace);
			}
			return enabled;
		}

		private static void EnableGpsService()
		{
			var poke = new Intent();
			poke.SetClassName("com.android.settings", "com.android.settings.widget.SettingsAppWidgetProvider");
			poke.AddCategory(Intent.CategoryAlternative);
			var data = Android.Net.Uri.Parse("3");
			poke.SetData(data);
			Application.Context.SendBroadcast(poke); // TODO: check for broadcast privileges
		}

		/// <summary>
		/// Require that the GPS provider is enabled.
		/// Assume that there's only one GPS provider.
		/// </summary>
		private static string GetGpsProviderName(LocationManager locationManager)
		{
			const string gpsProvider = "gps";
			var providerName = locationManager.AllProviders.FirstOrDefault(n => n == gpsProvider);
			try
			{
				if (!string.IsNullOrWhiteSpace(providerName))
				{
					var provider = locationManager.GetProvider(providerName);
					providerName = (provider.Accuracy == Accuracy.High || provider.Accuracy == Accuracy.Fine) ? gpsProvider : "";
				}
			}
			catch (Exception ex)
			{
				var m = ex.ToString();
				if (Debugger.IsAttached) Debugger.Break();
				throw;
			}
			return providerName;
		}
	}
}
