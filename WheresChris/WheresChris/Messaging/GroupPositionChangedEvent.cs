using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StayTogether;
using StayTogether.Helpers;
using StayTogether.Models;
using Xamarin.Forms;

namespace WheresChris.Messaging
{
    public delegate void GroupEventHandler(object sender, GroupEventArgs e);

    public class GroupPositionChangedEvent
    {
        public event GroupEventHandler OnGroupPositionChangedMsg;

        private readonly TimeGate _groupPositionUpdateTimeGate = new TimeGate(0, 0, 30);

        public GroupPositionChangedEvent(TimeSpan? interval = null)
        {
            if (interval.HasValue)
            {
                _groupPositionUpdateTimeGate = new TimeGate(interval.Value);
            }

            MessagingCenter.Subscribe<LocationSender>(this, LocationSender.GroupPositionUpdateMsg,
            (sender) =>
            {
                var locationSender = LocationSenderFactory.GetLocationSender();
                if (_groupPositionUpdateTimeGate.CanProcess(true) && (locationSender?.GroupMembers?.Any() ?? false))
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
