using System;
using System.Collections.Generic;
using System.Text;
using StayTogether;
using StayTogether.Helpers;
using Xamarin.Forms;
using EventHandler = System.EventHandler;

namespace WheresChris.Messaging
{
    public class GroupLeftEvent
    {
        public event EventHandler OnGroupLeftMsg;

        private readonly TimeGate _leftGroupTimeGate = new TimeGate(1000);

        public GroupLeftEvent(TimeSpan? interval = null)
        {
            if (interval.HasValue)
            {
                _leftGroupTimeGate = new TimeGate(interval.Value);
            }
            //If the group is disbanded, it means this user also left the group with everyone else
            MessagingCenter.Subscribe<LocationSender>(this, LocationSender.GroupDisbandedMsg,
            (sender) =>
            {
                if (_leftGroupTimeGate.CanProcess(true))
                {
                    OnGroupLeftMsg?.Invoke(this, EventArgs.Empty);
                }
            });
            //This user left the group
            MessagingCenter.Subscribe<LocationSender>(this, LocationSender.ThisUserLeftGroupMsg,
            (sender) =>
            {
                if (_leftGroupTimeGate.CanProcess(true))
                {
                    OnGroupLeftMsg?.Invoke(this, EventArgs.Empty);
                }
            });
        }
    }
}
