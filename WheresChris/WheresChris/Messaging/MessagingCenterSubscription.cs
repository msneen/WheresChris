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

    public class GroupEventArgs : EventArgs
    {
        public List<GroupMemberSimpleVm> GroupMembers { get; set; }
    }

    public class MessagingCenterSubscription
    {
        public event System.EventHandler OnLocationSentMsg;
        public event GroupEventHandler OnGroupPositionChangedMsg;
        
        private readonly TimeGate _locationSentTimeGate = new TimeGate(2000);
        private readonly TimeGate _groupPositionUpdateTimeGate = new TimeGate(0, 0, 30);

        private readonly bool _isSubscribed;

        public MessagingCenterSubscription()
        {
            if (_isSubscribed) return;
            MessagingCenter.Subscribe<LocationSender>(this, LocationSender.LocationSentMsg, (sender) =>
            {
                if (_locationSentTimeGate.CanProcess(true))
                {
                    OnLocationSentMsg?.Invoke(this, EventArgs.Empty);
                }

            });
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
            _isSubscribed = true;
        }
    }
}
