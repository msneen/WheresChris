using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.OS;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;
using Microsoft.Azure.Mobile.Crashes;
using Microsoft.Azure.Mobile.Distribute;
using Plugin.Permissions;
using Plugin.Toasts;
using StayTogether;
using StayTogether.Droid.NotificationCenter;
using StayTogether.Droid.Services;
using StayTogether.Helpers;
using StayTogether.Models;
using TK.CustomMap.Droid;
using WheresChris.Droid.Services;
using Xamarin.Forms;
using Permission = Android.Content.PM.Permission;

namespace WheresChris.Droid
{
	[Activity(Label = "WheresChris", Theme = "@style/splashscreen", MainLauncher = true,
		LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
		ScreenOrientation = ScreenOrientation.Portrait)]
	public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		public LocationSenderBinder Binder;
		public bool IsBound;
		private LocationSenderServiceConnection _locationSenderServiceConnection;
        Interval _backgroundServiceInterval = new Interval();
        Interval _backgroundStartupInterval = new Interval();

        public const int SdkVersionMarshmallow = 23;

		protected override void OnNewIntent(Intent intent)
		{
			base.OnNewIntent(intent);
			NotificationStrategyController.GetNotificationHandler(intent)?.OnNotify(intent);
		}

		protected override async void OnCreate(Bundle bundle)
		{
		    try
		    {
		        MobileCenter.LogLevel = LogLevel.Verbose;
		        MobileCenter.Start("14162ca6-0c56-4822-9d95-f265b524bd98", typeof(Analytics), typeof(Crashes),
		            typeof(Distribute));

#pragma warning disable 618
		        MobileAds.Initialize(ApplicationContext, "ca-app-pub-5660348862902976~9593604641");
#pragma warning restore 618

		        NotificationStrategyController.GetNotificationHandler(Intent)?.OnNotify(Intent);

		        TabLayoutResource = Resource.Layout.Tabbar;
		        ToolbarResource = Resource.Layout.Toolbar;
		        Xamarin.Forms.Forms.Init(this, bundle);
		        Xamarin.FormsMaps.Init(this, bundle);
		        TKGoogleMaps.Init(this, bundle);
		        ToastNotification.Init(this);

		        SetTheme(Resource.Style.MyTheme);
		        base.OnCreate(bundle);

                LoadApplication(new App());


                await TryToStartLocationService();
            }
		    catch (Exception ex)
		    {
                Analytics.TrackEvent("Permissions", new Dictionary<string, string>
                {
                    {"MainActivity.cs_OnCreate_Error" , ex.Message}
                });
            }
		}

		private async Task TryToStartLocationService()
		{
             _backgroundServiceInterval.SetInterval(StartLocationService, 60000);
		}

		private void TryStartGps()
		{
			var isGpsEnabled = GpsService.IsGpsEnabled();
			if (!isGpsEnabled)
			{
				isGpsEnabled = GpsService.EnableGps();
				if (!isGpsEnabled)
				{
					ShowAlert(); // Prompt user to turn on GPS
				}
			}
		}

		private bool _locationServiceStarted = false;
		private void StartLocationService()
		{
			if (_locationServiceStarted) return;

            StartService(new Intent(this, typeof(LocationSenderService)));

			_locationServiceStarted = true;
		}

		protected void BindToService()
		{
			_locationSenderServiceConnection = new LocationSenderServiceConnection(this);

			BindService(new Intent(this, typeof(LocationSenderService)), _locationSenderServiceConnection, Bind.AutoCreate);
			IsBound = true;
		}

		protected void UnbindFromService()
		{
			if (!IsBound) return;
			UnbindService(_locationSenderServiceConnection);
			IsBound = false;
		}

		public void CleanupGroupsForExit()
		{
            MessagingCenter.Send<MessagingCenterSender>(new MessagingCenterSender(), LocationSender.LeaveGroupMsg);
            MessagingCenter.Send<MessagingCenterSender>(new MessagingCenterSender(), LocationSender.EndGroupMsg);
        }

		protected override void OnPause()
		{
			base.OnPause();
			Binder?.GetLocationSenderService()?.StartForeground();
			UnbindFromService();
		}

		protected override void OnResume()
		{
			base.OnResume();
			Binder?.GetLocationSenderService()?.StopForeground();
			BindToService();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			CleanupGroupsForExit();
			Binder?.GetLocationSenderService()?.StopSelf();
			Process.KillProcess(Process.MyPid());
			System.Environment.Exit(0);
		}

		public void ShowAlert()
		{
			var alertDialogBuilder =	InitializeAlertDialog();
			RunOnUiThread(() => alertDialogBuilder.Show());
		}

		private AlertDialog.Builder InitializeAlertDialog()
		{
			const string message = "You must enable GPS Location Services in your device settings in order to use this application.";
			const string title = "Notice";
			const string buttonText = "OK";
			var alertDialogBuilder = new AlertDialog.Builder(this)
				.SetTitle(title)
				.SetMessage(message)
				.SetPositiveButton(buttonText, (s2, e2) => { });
			return alertDialogBuilder;
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
		{
		    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
			PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}
	}
}