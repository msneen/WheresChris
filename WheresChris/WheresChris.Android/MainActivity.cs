using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;
using Microsoft.Azure.Mobile.Crashes;
using Plugin.Permissions;
using StayTogether.Droid.Services;

namespace WheresChris.Droid
{
    [Activity(Label = "WheresChris.Android", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, GroupJoinedCallback
    {
        public LocationSenderBinder Binder;
        public bool IsBound;
        private LocationSenderServiceConnection _locationSenderServiceConnection;

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            MobileCenter.LogLevel = Microsoft.Azure.Mobile.LogLevel.Verbose;
            MobileCenter.Start("f9f28a5e-6d54-4a4a-a1b4-e51f8da8e8c7",
                typeof(Analytics), typeof(Crashes));

            Xamarin.FormsMaps.Init(this, bundle);

            StartService(new Intent(this, typeof(LocationSenderService)));

            LoadApplication(new App());
        }

        public void GroupJoined()
        {
            
        }

        public void GroupDisbanded()
        {
            //Finish();//Todo: figure out what to do here
            //Notes from StayTogether:  Eventually keep running and reshow the Start Group Button and Contacts List
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
            if (locationSender != null)
            {
                inAGroup = locationSender.InAGroup;
            }

            if (inAGroup)
            {
                GroupJoined();
            }
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
        }
    }
}