using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
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
using WheresChris.Droid.Services;
using WheresChris.Helpers;
using Permission = Android.Content.PM.Permission;

namespace WheresChris.Droid
{
    [Activity(Label = "WheresChris.Android", Theme = "@style/splashscreen", MainLauncher = true, 
        LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        ScreenOrientation = ScreenOrientation.Portrait)]
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

        protected override async void OnCreate(Bundle bundle)
        {
            MobileCenter.LogLevel = LogLevel.Verbose;
            MobileCenter.Start("14162ca6-0c56-4822-9d95-f265b524bd98", typeof(Analytics), typeof(Crashes), typeof(Distribute));

            NotificationStrategyController.GetNotificationHandler(Intent)?.OnNotify(Intent);

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            global::Xamarin.Forms.Forms.Init(this, bundle);
            Xamarin.FormsMaps.Init(this, bundle);
            ToastNotification.Init(this);

            SetTheme(Resource.Style.MyTheme);
            base.OnCreate(bundle);

            await TryToStartLocationService();
        }

        private async Task TryToStartLocationService()
        {
            var phonePermissionGranted = await PermissionHelper.HasPhonePermission();
            var locationPermissionGranted = await PermissionHelper.HasLocationPermission();
            var contactPermissionGranted = await PermissionHelper.HasContactPermission();

            if (locationPermissionGranted && phonePermissionGranted && contactPermissionGranted)
            {
                LoadApplication(new App());
                //await App.InitializeContacts();
                Task.Run(()=>StartLocationService()).Wait();
                
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

        public async Task CleanupGroupsForExit()
        {
            var locationSender = await LocationSender.GetInstanceAsync();
            await locationSender.LeaveGroup();
            await locationSender.EndGroup();
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

        protected override async void OnDestroy()
        {
            base.OnDestroy();
            await CleanupGroupsForExit();
            Binder?.GetLocationSenderService()?.StopSelf();
            Process.KillProcess(Process.MyPid());
            System.Environment.Exit(0);
        }

        public override async void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            await TryToStartLocationService();
        }
    }
}