using Android.App;
using Android.Content.PM;
using Android.OS;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;
using Microsoft.Azure.Mobile.Crashes;

namespace WheresChris.Droid
{
    [Activity(Label = "WheresChris.Android", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
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

            LoadApplication(new App());
        }
    }
}