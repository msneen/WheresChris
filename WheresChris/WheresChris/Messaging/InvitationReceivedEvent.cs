using System;
using StayTogether;
using StayTogether.Helpers;
using Xamarin.Forms;
using EventHandler = System.EventHandler;

namespace WheresChris.Messaging
{
    public class InvitationReceivedEvent : MessageEventBase
    {
        public event EventHandler OnInvitationReceivedMsg;

        public InvitationReceivedEvent(TimeSpan? interval = null) : base(interval ?? new TimeSpan(0, 0, 1))
        {
            MessagingCenter.Subscribe<LocationSender>(this, LocationSender.GroupInvitationReceivedMsg,
            (sender) =>
            {
                if (MessageTimeGate.CanProcess(true))
                {
                    OnInvitationReceivedMsg?.Invoke(this, EventArgs.Empty);
                }
            });
        }
    }
}
