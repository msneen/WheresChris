using System;
using System.Collections.Generic;
using System.Text;
using StayTogether;
using Xamarin.Forms;

namespace WheresChris.Messaging
{
    public class MessagingCenterSubscription
    {
        public event System.EventHandler OnLocationSentMsg;

        private DateTime LastMessageTime = DateTime.Now;
        private bool _isSubscribed = false;

        public MessagingCenterSubscription()
        {
            if (_isSubscribed) return;
            MessagingCenter.Subscribe<LocationSender>(this, LocationSender.LocationSentMsg, (sender) =>
            {
                if (DateTime.Now.Subtract(LastMessageTime).Seconds <= 2) return;

                OnLocationSentMsg?.Invoke(this, EventArgs.Empty);
                LastMessageTime = DateTime.Now;
            });
            _isSubscribed = true;
        }
    }
}
