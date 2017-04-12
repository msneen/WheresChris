
using Foundation;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;
using Microsoft.Azure.Mobile.Crashes;
using UIKit;

namespace WheresChris.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init();
			LoadApplication(new App());

            MobileCenter.Start("8fb14343-2648-42ad-acdc-1acf2e6d8c0f",
                typeof(Analytics), typeof(Crashes));

            return base.FinishedLaunching(app, options);
		}
	}
}
