using System;
using System.Collections.Generic;
using System.Text;
using StayTogether;
using StayTogether.Helpers;
using Xamarin.Forms;
using EventHandler = System.EventHandler;

namespace WheresChris.Messaging
{
    public class GroupJoinedEvent
    {
        public event EventHandler OnGroupJoinedMsg;

        private readonly  TimeGate _groupJoinedTimeGate = new TimeGate(1000);

        public GroupJoinedEvent(TimeSpan? interval = null)
        {
            if (interval.HasValue)
            {
                _groupJoinedTimeGate = new TimeGate(interval.Value);
            }
            MessagingCenter.Subscribe<LocationSender>(this, LocationSender.GroupJoinedMsg,
            (sender) =>
            {
                if (_groupJoinedTimeGate.CanProcess(true))
                {
                    OnGroupJoinedMsg?.Invoke(this, EventArgs.Empty);
                }
            });
        }
    }
}
