using System;
using StayTogether;
using StayTogether.Helpers;
using Xamarin.Forms;

namespace WheresChris.Messaging
{
    public class LocationSentEvent : MessageEventBase
    {
        public event System.EventHandler OnLocationSentMsg;

        public LocationSentEvent(TimeSpan? interval = null) : base(interval ?? new TimeSpan(0, 0, 2))
        {
            MessagingCenter.Subscribe<LocationSender>(this, LocationSender.LocationSentMsg, (sender) =>
            {
                if (MessageTimeGate.CanProcess(true))
                {
                    OnLocationSentMsg?.Invoke(this, EventArgs.Empty);
                }
            });
        }
    }
}
