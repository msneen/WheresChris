using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StayTogether;
using StayTogether.Classes;
using StayTogether.Models;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;

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

        private DateTime _lastLocationMessageSentTime = DateTime.Now;

        private readonly bool _isSubscribed = false;

        public MessagingCenterSubscription()
        {
            if (_isSubscribed) return;
            MessagingCenter.Subscribe<LocationSender>(this, LocationSender.LocationSentMsg, (sender) =>
            {
                if (DateTime.Now.Subtract(_lastLocationMessageSentTime).Seconds <= 2) return;

                OnLocationSentMsg?.Invoke(this, EventArgs.Empty);
                _lastLocationMessageSentTime = DateTime.Now;
            });
            MessagingCenter.Subscribe<LocationSender>(this, LocationSender.GroupPositionUpdateMsg,
                (sender) =>
                {
                    var locationSender = LocationSenderFactory.GetLocationSender();
                    // if (locationSender.GroupMembers != null && locationSender.GroupMembers.Any())
                    if (locationSender?.GroupMembers?.Any() ?? false)
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
