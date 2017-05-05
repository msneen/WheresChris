using System;
using System.Collections.Generic;
using System.Text;
using StayTogether;
using StayTogether.Helpers;
using Xamarin.Forms;
using EventHandler = System.EventHandler;

namespace WheresChris.Messaging
{
    public class GroupJoinedEvent : MessageEventBase
    {
        public event EventHandler OnGroupJoinedMsg;
       
        public GroupJoinedEvent(TimeSpan? interval = null):base(interval ?? new TimeSpan(0, 0, 1))
        {
            MessagingCenter.Subscribe<LocationSender>(this, LocationSender.GroupJoinedMsg,
            (sender) =>
            {
                if (MessageTimeGate.CanProcess(true))
                {
                    OnGroupJoinedMsg?.Invoke(this, EventArgs.Empty);
                }
            });
        }
    }
}
