using System;
using StayTogether;
using StayTogether.Helpers;
using Xamarin.Forms;

namespace WheresChris.Messaging
{
    public class LocationSentEvent
    {
        public event System.EventHandler OnLocationSentMsg;

        private readonly TimeGate _locationSentTimeGate = new TimeGate(2000);

        public LocationSentEvent()
        {
            MessagingCenter.Subscribe<LocationSender>(this, LocationSender.LocationSentMsg, (sender) =>
            {
                if (_locationSentTimeGate.CanProcess(true))
                {
                    OnLocationSentMsg?.Invoke(this, EventArgs.Empty);
                }
            });
        }
    }
}
