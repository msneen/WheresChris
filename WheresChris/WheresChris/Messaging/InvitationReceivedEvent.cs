using System;
using StayTogether;
using StayTogether.Helpers;
using Xamarin.Forms;
using EventHandler = System.EventHandler;

namespace WheresChris.Messaging
{
    public class InvitationReceivedEvent
    {
        public event EventHandler OnInvitationReceivedMsg;

        private readonly TimeGate _invitationTimeGate= new TimeGate(1000);

        public InvitationReceivedEvent(TimeSpan? interval = null)
        {
            if (interval.HasValue)
            {
                _invitationTimeGate = new TimeGate(interval.Value);
            }

            MessagingCenter.Subscribe<LocationSender>(this, LocationSender.GroupInvitationReceivedMsg,
            (sender) =>
            {
                if (_invitationTimeGate.CanProcess(true))
                {
                    OnInvitationReceivedMsg?.Invoke(this, EventArgs.Empty);
                }
            });
        }
    }
}
