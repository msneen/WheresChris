
using Foundation;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;
using Microsoft.Azure.Mobile.Crashes;
using UIKit;
using WheresChris.iOS.Classes;

namespace WheresChris.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
	    public static LocationManager LocationManager = null;

		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init();
			LoadApplication(new App());

            MobileCenter.Start("2cd11ff1-c5b1-47d8-ac96-9fa5b74a47bd", typeof(Analytics), typeof(Crashes));

            Xamarin.FormsMaps.Init();

            LocationManager = new LocationManager();
            LocationManager.StartLocationUpdates();

            return base.FinishedLaunching(app, options);
		}
	}
}
