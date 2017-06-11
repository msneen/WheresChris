using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Ads;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace WheresChris.Droid
{
    public class AdMobListener : AdListener
    {
        public delegate void AdLoadedEvent();
        public delegate void AdClosedEvent();
        public delegate void AdOpenedEvent();

        public delegate void AdFailedToLoad(int errorCode);
        // Declare the event.
        public event AdLoadedEvent AdLoaded;
        public event AdClosedEvent AdClosed;
        public event AdOpenedEvent AdOpened;
        public event AdFailedToLoad AdFailedLoading;

        public override void OnAdLoaded()
        {
            if (AdLoaded != null) this.AdLoaded();
            base.OnAdLoaded();
        }
        public override void OnAdClosed()
        {
            if (AdClosed != null) this.AdClosed();
            base.OnAdClosed();
        }
        public override void OnAdOpened()
        {
            if (AdOpened != null) this.AdOpened();
            base.OnAdOpened();
        }

        public override void OnAdFailedToLoad(int errorCode)
        {
            AdFailedLoading?.Invoke(errorCode);
            base.OnAdFailedToLoad(errorCode);
        }
    }
}