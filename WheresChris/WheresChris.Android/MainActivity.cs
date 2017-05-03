using System.Threading.Tasks;
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
using WheresChris.Helpers;
using Permission = Android.Content.PM.Permission;

namespace WheresChris.Droid
{
    [Activity(Label = "WheresChris.Android", Theme = "@style/MyTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public LocationSenderBinder Binder;
        public bool IsBound;
        private LocationSenderServiceConnection _locationSenderServiceConnection;

        public const int SdkVersionMarshmallow = 23;


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

        }

        private async void TryToStartLocationService()
        {
            var phonePermissionGranted = PermissionHelper.HasPhonePermission();
            var locationPermissionGranted = PermissionHelper.HasLocationPermission();
            var contactPermissionGranted = PermissionHelper.HasContactPermission();

            if (locationPermissionGranted && phonePermissionGranted && contactPermissionGranted)
            {
                StartLocationService();
                LoadApplication(new App());
            }
            else if (!locationPermissionGranted)
            {
                await PermissionHelper.RequestLocationPermission();
            }
            else if (!phonePermissionGranted)
            {
                await PermissionHelper.RequestPhonePermission();
            }
            else
            {
                await PermissionHelper.RequestContactPermission();
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

    }
}