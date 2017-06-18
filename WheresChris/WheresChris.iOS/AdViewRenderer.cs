using Google.MobileAds;
using System;
using StayTogether.Helpers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(WheresChris.Controls.AdView), typeof(WheresChris.iOS.AdViewRenderer))]
namespace WheresChris.iOS
{
    public class AdViewRenderer : ViewRenderer<WheresChris.Controls.AdView, BannerView>
    {
        private static readonly Interval Interval = new Interval();
        string bannerId = "ca-app-pub-5660348862902976/5523331842";
        BannerView adView;
        BannerView CreateNativeControl()
        {
            if (adView != null)
                return adView;


            // Setup your BannerView, review AdSizeCons class for more Ad sizes. 
            adView = new BannerView(size: AdSizeCons.Banner)
            {
                AdUnitID = bannerId,
                RootViewController = GetVisibleViewController()
            };

            //// Wire AdReceived event to know when the Ad is ready to be displayed
            //adView.AdReceived += (object sender, EventArgs e) =>
            //{
            //    //ad has come in
            //};

            Interval.SetInterval(LoadAdd, 30000);//adView.LoadRequest(GetRequest());

            return adView;
        }

        //Call this from AppDelegate or android service
        public void LoadAdd()
        {
            Device.BeginInvokeOnMainThread(() => {
                adView.LoadRequest(GetRequest());
            });
        }

        Request GetRequest()
        {
            var request = Request.GetDefaultRequest();
            // Requests test ads on devices you specify. Your test device ID is printed to the console when
            // an ad request is made. GADBannerView automatically returns test ads when running on a
            // simulator. After you get your device ID, add it here
            //request.TestDevices = new [] { Request.SimulatorId.ToString () };
            return request;
        }

        /// 
        /// Gets the visible view controller.
        /// 
        /// The visible view controller.
        UIViewController GetVisibleViewController()
        {
            var rootController = UIApplication.SharedApplication.KeyWindow.RootViewController;

            if (rootController.PresentedViewController == null)
                return rootController;

            if (rootController.PresentedViewController is UINavigationController)
            {
                return ((UINavigationController)rootController.PresentedViewController).VisibleViewController;
            }

            if (rootController.PresentedViewController is UITabBarController)
            {
                return ((UITabBarController)rootController.PresentedViewController).SelectedViewController;
            }

            return rootController.PresentedViewController;
        }

        protected void OnElementChanged(ElementChangedEventArgs<WheresChris.Controls.AdView> elementChangedEventArgs)
        {
            base.OnElementChanged(elementChangedEventArgs);
            if (Control == null)
            {
                CreateNativeControl();
                SetNativeControl(adView);
            }
        }
    }
}