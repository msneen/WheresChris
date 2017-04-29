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

        private static readonly int REQUEST_CONTACTS = 1;
        private static readonly int REQUEST_LOCATION = 2;
        private static readonly int REQUEST_PHONE= 3;
        private static readonly int REQUEST_SMS = 4;
        private static readonly int REQUEST_STORAGE = 4;

        private static string[] PERMISSIONS_CONTACT = {
            Manifest.Permission.ReadContacts,
            Manifest.Permission.WriteContacts
        };
        private static string[] PERMISSIONS_LOCATION= {
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation
        };
        private static string[] PERMISSIONS_PHONE = {
            Manifest.Permission.CallPhone,
            Manifest.Permission.ReadPhoneState
        };
        private static string[] PERMISSIONS_SMS= {
            Manifest.Permission.WriteSms,
            Manifest.Permission.SendSms
        };
        private static string[] PERMISSIONS_STORAGE = {
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage
        };

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            MobileCenter.LogLevel = Microsoft.Azure.Mobile.LogLevel.Verbose;
            MobileCenter.Start("14162ca6-0c56-4822-9d95-f265b524bd98",    //f9f28a5e-6d54-4a4a-a1b4-e51f8da8e8c7
                typeof(Analytics), typeof(Crashes));

            RequestPermissions();

            Xamarin.FormsMaps.Init(this, bundle);

            StartService(new Intent(this, typeof(LocationSenderService)));

            LoadApplication(new App());
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
            //Refactor me
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.ReadContacts) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, PERMISSIONS_CONTACT, REQUEST_CONTACTS);
            }
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, PERMISSIONS_LOCATION, REQUEST_LOCATION);
            }
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.ReadPhoneState) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, PERMISSIONS_PHONE, REQUEST_PHONE);
            }
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.SendSms) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, PERMISSIONS_SMS, REQUEST_SMS);
            }
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, PERMISSIONS_STORAGE, REQUEST_STORAGE);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}