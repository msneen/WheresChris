using System;
using System.Collections.Generic;
using System.Text;
using StayTogether;
using StayTogether.Classes;
using Xamarin.Forms;

namespace WheresChris.Messaging
{
    public delegate void GroupEventHandler(object sender, GroupEventArgs e);

    public class GroupEventArgs : EventArgs
    {
        public List<GroupMemberVm> GroupMembers { get; set; }
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
            MessagingCenter.Subscribe<LocationSender, List<GroupMemberVm>>(this, LocationSender.GroupPositionUpdateMsg,
                (sender, groupMembers) =>
                {
                    OnGroupPositionChangedMsg?.Invoke(this, new GroupEventArgs
                    {
                        GroupMembers = groupMembers
                    });
                });
            _isSubscribed = true;
        }
    }
}
