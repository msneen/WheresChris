using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Telephony;
using Plugin.Geolocator.Abstractions;
using Plugin.Settings;
using StayTogether.Classes;
using StayTogether.Droid.Classes;
//using StayTogether.Droid.Activities;
//using StayTogether.Droid.Classes;
//using StayTogether.Droid.Helpers;
//using StayTogether.Droid.NotificationCenter;
using StayTogether.Group;
using StayTogether.Location;
using WheresChris.Droid;
using StayTogether.Droid.NotificationCenter;
using WheresChris.Helpers;

namespace StayTogether.Droid.Services
{
    public interface GroupJoinedCallback
    {
        void GroupJoined();
        void GroupDisbanded();
    }


    [Service]
    // ReSharper disable once RedundantExplicitArrayCreation
    [IntentFilter(new string[] {"com.StayTogether.Droid.LocationSenderService"})]
    public class LocationSenderService : Service
    {
        private GroupJoinedCallback _groupJoinedCallback;

        private LocationSenderBinder _binder;
        public LocationSender LocationSender;

        public static LocationSenderService Instance;

        public void SetGroupJoinedCallback(GroupJoinedCallback groupJoinedCallback)
        {
            _groupJoinedCallback = groupJoinedCallback;
        }

        public void StartForeground()
        {
            var notification = DisplayServiceNotification();

            StartForeground(1337, notification);
        }


        public void StopForeground()
        {
            StopForeground(true);
        }

        public override void OnCreate()
        {
            base.OnCreate();
            Instance = this;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            StartLocationSender();

            return StartCommandResult.Sticky;
        }

        private Notification DisplayServiceNotification()
        {
            var notificationIntent = new Intent(this, typeof(MainActivity));
            var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, 0);
            var notification = new Notification.Builder(this)
                .SetSmallIcon(Resource.Drawable.ic_speaker_dark)
                .SetContentTitle("Where's Chris")
                .SetContentText("Tracking...")
                .SetContentIntent(pendingIntent).Build();
            return notification;
        }

        //Todo: Delete me.  Replaced by WheresChris.InvitePage.xaml.cs.StartGroup, or whereever it's been refactored to
        public async void StartGroup(List<GroupMemberVm> contactList, int expireInHours)
        {
            var position = GetPosition();
            if (LocationSender == null || position == null) return;

            await CreateGroup(contactList, position, expireInHours);
        }

        public Position GetPosition()
        {
            var position = GpsService.GetLocation();
            return position;
        }

        private async Task CreateGroup(List<GroupMemberVm> contactList, Position position, int expireInHours)
        {
            var groupVm = GroupHelper.InitializeGroupVm(contactList, position, SettingsHelper.GetPhoneNumber(), expireInHours);

            await LocationSender.StartGroup(groupVm);
        }

        public async Task SendError(string message)
        {
            await LocationSender.SendError(message);
        }

        private void StartLocationSender()
        {
            var phoneNumber = SettingsHelper.GetPhoneNumber();
            InitializeLocationSender(phoneNumber);
            SendFirstPositionUpdate(phoneNumber);
        }

        public async void EndGroup()
        {
            await LocationSender.EndGroup();
        }

        public async void LeaveGroup()
        {
            await LocationSender.LeaveGroup();
        }


        private void SendFirstPositionUpdate(string phoneNumber)
        {
            var position = GpsService.GetLocation();
            if (position == null) return;

            var groupMemberVm = GroupMemberPositionAdapter.Adapt(position);
            groupMemberVm.PhoneNumber = phoneNumber;
            LocationSender.SendUpdatePosition(groupMemberVm);
        }

        private void InitializeLocationSender(string phoneNumber)
        {
            LocationSender = new LocationSender();
            LocationSender.InitializeSignalRAsync();
            LocationSender.OnSomeoneIsLost += (sender, args) =>
            {
                OnNotifySomeoneIsLost(args.GroupMember);
            };
            LocationSender.OnGroupInvitationReceived += (sender, args) => 
            {
                GroupInvitationNotification.DisplayGroupInvitationNotification(args.GroupId, args.Name);
            };
            LocationSender.OnGroupJoined += (sender, args) =>
            {
                //When the location sender fires the group joined event, call the callback in the activity 
                //so we can disable the joinGroup button and hide the contact list
                _groupJoinedCallback?.GroupJoined();
            };
            LocationSender.OnGroupDisbanded +=(sender, args) =>
            {
                _groupJoinedCallback?.GroupDisbanded();
            };
            LocationSender.OnSomeoneLeft += (sender, args) =>
            {
                LeftGroupNotification.DisplayLostNotification(args.PhoneNumber, args.Name);
            };
            LocationSender.OnSomeoneAlreadyInAnotherGroup += (sender, args) =>
            {
                InAnotherGroupNotification.DisplayInAnotherGroupNotification(args.PhoneNumber, args.Name);
            };
        }

        private void OnNotifySomeoneIsLost(GroupMemberVm groupMember)
        {
            LostNotification.DisplayLostNotification(groupMember);
        }

        public override IBinder OnBind(Intent intent)
        {
            _binder = new LocationSenderBinder(this);
            return _binder;
        }

    }
}