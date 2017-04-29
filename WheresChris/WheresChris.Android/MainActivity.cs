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
using StayTogether.Droid.Services;

namespace WheresChris.Droid
{
    [Activity(Label = "WheresChris.Android", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public LocationSenderBinder Binder;
        public bool IsBound;
        private LocationSenderServiceConnection _locationSenderServiceConnection;

        private static readonly int REQUEST_LOCATION = 1;

        private static string[] PERMISSIONS_CONTACT = {
            Manifest.Permission.ReadContacts,
            Manifest.Permission.WriteContacts
        };
        private static string[] PERMISSIONS_LOCATION= {
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation
        };

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
           
            MobileCenter.LogLevel = Microsoft.Azure.Mobile.LogLevel.Verbose;
            MobileCenter.Start("14162ca6-0c56-4822-9d95-f265b524bd98",    //f9f28a5e-6d54-4a4a-a1b4-e51f8da8e8c7
                typeof(Analytics), typeof(Crashes));

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            global::Xamarin.Forms.Forms.Init(this, bundle);
            Xamarin.FormsMaps.Init(this, bundle);
            LoadApplication(new App());

            TryToStartLocationService();
        }

        private void TryToStartLocationService()
        {
            if (HasLocationPermission())
            {
                StartLocationService();
            }
            else
            {
                RequestPermissions();
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
            BindToService();
            var locationSenderService = Binder?.GetLocationSenderService();
            var locationSender = locationSenderService?.LocationSender;
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

        private void RequestPermissions()
        {
            if (!HasLocationPermission())
            {
                ActivityCompat.RequestPermissions(this, PERMISSIONS_LOCATION, REQUEST_LOCATION);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            TryToStartLocationService();
        }

        public bool HasLocationPermission()
        {
            return HasPermission(Manifest.Permission.AccessFineLocation);
        }
        private bool HasPermission(string permission)
        {
            return ActivityCompat.CheckSelfPermission(this, permission) ==
                   (int)Permission.Granted;
        }
    }
}