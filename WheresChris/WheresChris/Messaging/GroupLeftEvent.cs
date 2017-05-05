using System;
using System.Collections.Generic;
using System.Text;
using StayTogether;
using StayTogether.Helpers;
using Xamarin.Forms;
using EventHandler = System.EventHandler;

namespace WheresChris.Messaging
{
    public class GroupLeftEvent : MessageEventBase
    {
        public event EventHandler OnGroupLeftMsg;

        public GroupLeftEvent(TimeSpan? interval = null) : base(interval ?? new TimeSpan(0, 0, 1))
        {
            //If the group is disbanded, it means this user also left the group with everyone else
            MessagingCenter.Subscribe<LocationSender>(this, LocationSender.GroupDisbandedMsg,
            (sender) =>
            {
                if (MessageTimeGate.CanProcess(true))
                {
                    OnGroupLeftMsg?.Invoke(this, EventArgs.Empty);
                }
            });
            //This user left the group
            MessagingCenter.Subscribe<LocationSender>(this, LocationSender.ThisUserLeftGroupMsg,
            (sender) =>
            {
                if (MessageTimeGate.CanProcess(true))
                {
                    OnGroupLeftMsg?.Invoke(this, EventArgs.Empty);
                }
            });
        }
    }
}
