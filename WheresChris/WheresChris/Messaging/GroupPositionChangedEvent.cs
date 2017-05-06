using System;
using System.Collections.Generic;
using System.Linq;
using StayTogether;
using StayTogether.Helpers;
using StayTogether.Models;
using Xamarin.Forms;

namespace WheresChris.Messaging
{
    public delegate void GroupEventHandler(object sender, GroupEventArgs e);

    public class GroupPositionChangedEvent : MessageEventBase
    {
        public event GroupEventHandler OnGroupPositionChangedMsg;

        public GroupPositionChangedEvent(TimeSpan? interval = null) : base(interval ?? new TimeSpan(0, 0, 30))
        {
            MessagingCenter.Subscribe<LocationSender>(this, LocationSender.GroupPositionUpdateMsg,
            (sender) =>
            {
                var locationSender = LocationSenderFactory.GetLocationSender();
                if ((locationSender?.GroupMembers?.Any() ?? false) && MessageTimeGate.CanProcess(true))
                {
                    OnGroupPositionChangedMsg?.Invoke(this, new GroupEventArgs
                    {
                        GroupMembers = locationSender.GroupMembers
                    });
                }
            });
        }
    }
    public class GroupEventArgs : EventArgs
    {
        public List<GroupMemberSimpleVm> GroupMembers { get; set; }
    }
}
