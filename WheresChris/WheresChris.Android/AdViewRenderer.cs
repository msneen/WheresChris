using Android.Widget;
using Android.Gms.Ads;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(WheresChris.Controls.AdView), typeof(WheresChris.Droid.AdViewRenderer))]
namespace WheresChris.Droid
{
    public class AdViewRenderer : ViewRenderer<Controls.AdView, AdView>
    {

        string adUnitId = string.Empty;
        AdSize adSize = AdSize.SmartBanner;
        AdView adView;
        AdView CreateNativeControl()
        {
            if (adView != null)
                return adView;

            adUnitId = Forms.Context.Resources.GetString(Resource.String.banner_ad_unit_id);
            adView = new AdView(Forms.Context);
            adView.AdSize = adSize;
            adView.AdUnitId = adUnitId;

            var adParams = new LinearLayout.LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent);

            adView.LayoutParameters = adParams;

            var adMobListener = new AdMobListener();
            adMobListener.AdLoaded += () =>
            {
                Toast.MakeText(this.Context, "Ad Loaded", ToastLength.Long).Show();
            };
            adMobListener.AdFailedLoading += code =>
            {
                Toast.MakeText(this.Context, $"Ad Loading Failed {code}", ToastLength.Long).Show();
            };
            adView.AdListener = adMobListener;

            var builder = new AdRequest
                .Builder();

#if DEBUG
            // Google requires the usage of test ads while debugging
            builder.AddTestDevice(AdRequest.DeviceIdEmulator);
#endif
            var adRequest = builder.Build();

            adView.LoadAd(adRequest);
            return adView;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Controls.AdView> elementChangedEventArgs)
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