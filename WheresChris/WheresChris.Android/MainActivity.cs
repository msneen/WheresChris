using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;
using Microsoft.Azure.Mobile.Crashes;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using StayTogether.Droid.NotificationCenter;
using StayTogether.Droid.Services;
using Permission = Android.Content.PM.Permission;

namespace WheresChris.Droid
{
    [Activity(Label = "WheresChris.Android", Theme = "@style/MyTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public LocationSenderBinder Binder;
        public bool IsBound;
        private LocationSenderServiceConnection _locationSenderServiceConnection;

        private static readonly int REQUEST_LOCATION = 1;
        public const int SdkVersionMarshmallow = 23;

        private static string[] PERMISSIONS_LOCATION= {
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation
        };

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            NotificationStrategyController.GetNotificationHandler(intent)?.OnNotify(intent);
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
           
            MobileCenter.LogLevel = LogLevel.Verbose;
            MobileCenter.Start("14162ca6-0c56-4822-9d95-f265b524bd98", typeof(Analytics), typeof(Crashes));

            NotificationStrategyController.GetNotificationHandler(Intent)?.OnNotify(Intent);

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            global::Xamarin.Forms.Forms.Init(this, bundle);
            Xamarin.FormsMaps.Init(this, bundle);

            TryToStartLocationService();
            LoadApplication(new App());
        }

        private async void TryToStartLocationService()
        {
            var phonePermissionGranted = HasPhonePermission();
            var locationPermissionGranted = HasLocationPermission();

            if (locationPermissionGranted && phonePermissionGranted)
            {
                StartLocationService();
            }
            else if (!locationPermissionGranted)
            {
                ActivityCompat.RequestPermissions(this, PERMISSIONS_LOCATION, REQUEST_LOCATION);
            }
            else 
            {
                await CrossPermissions.Current.RequestPermissionsAsync(new[] { Plugin.Permissions.Abstractions.Permission.Phone });
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
            LocationSenderService.Instance.LeaveGroup();
            LocationSenderService.Instance.EndGroup();
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
            var inAGroup = false;
            Binder?.GetLocationSenderService()?.StopForeground();
            BindToService();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CleanupGroupsForExit();
            Binder?.GetLocationSenderService()?.SetGroupJoinedCallback(null);
            Binder?.GetLocationSenderService()?.StopSelf();
            Process.KillProcess(Process.MyPid());
            System.Environment.Exit(0);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            TryToStartLocationService();
        }

        private static bool HasPhonePermission()
        {
            var phonePermission =
                CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Phone).Result;
            var phonePermissionGranted = phonePermission == PermissionStatus.Granted;
            return phonePermissionGranted;
        }

        public bool HasLocationPermission()
        {
            //replace this with plugin?
            return HasPermission(Manifest.Permission.AccessFineLocation);
        }
        private bool HasPermission(string permission)
        {
            return ActivityCompat.CheckSelfPermission(this, permission) ==
                   (int)Permission.Granted;
        }
    }
}