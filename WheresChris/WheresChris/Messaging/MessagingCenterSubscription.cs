using System;
using System.Collections.Generic;
using System.Linq;
using StayTogether;
using StayTogether.Helpers;
using StayTogether.Models;
using Xamarin.Forms;

namespace WheresChris.Messaging
{

    public class MessagingCenterSubscription
    {
        public LocationSentEvent LocationSentEvent;
        public GroupPositionChangedEvent GroupPositionChangedEvent;


        public MessagingCenterSubscription()
        {
            LocationSentEvent = new LocationSentEvent();
            GroupPositionChangedEvent = new GroupPositionChangedEvent(new TimeSpan(0, 0, 30));
        }
    }
}
