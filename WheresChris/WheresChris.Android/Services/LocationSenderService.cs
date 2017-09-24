using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using StayTogether;
using StayTogether.Classes;
using StayTogether.Droid.NotificationCenter;
using StayTogether.Droid.Services;
using StayTogether.Models;
using WheresChris.Helpers;
using WheresChris.Views;
using Xamarin.Forms;

namespace WheresChris.Droid.Services
{
    [Service]
    // ReSharper disable once RedundantExplicitArrayCreation
    [IntentFilter(new string[] {"com.StayTogether.Droid.LocationSenderService"})]
    public class LocationSenderService : Service
    {
        private LocationSenderBinder _binder;
        private LocationSender _locationSender;//This Reference keeps the sender alive when app is backgrounded

        public void StartForeground()
        {
            var notification = DisplayServiceNotification();
            StartForeground(1337, notification);
        }

        public void StopForeground()
        {
            StopForeground(true);
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
                .SetContentText("Wheres Chris")
                .SetContentIntent(pendingIntent).Build();
            return notification;
        }

        private void StartLocationSender()
        {
            Task.Run(InitializeLocationSender).Wait();            
        }

        private async Task InitializeLocationSender()
        {
            var locationPermissionGranted = await PermissionHelper.HasLocationPermission();
            if (!locationPermissionGranted) return;

            var phonePermissionGranted = await PermissionHelper.HasPhonePermission();
            if (!phonePermissionGranted) return;

            _locationSender = await LocationSender.GetInstanceAsync();

            MessagingCenter.Subscribe<LocationSender, GroupMemberVm>(this, LocationSender.SomeoneIsLostMsg,
            (sender, groupMember) =>
            {
                Device.BeginInvokeOnMainThread( () =>
                {
                    LostNotification.DisplayLostNotification(groupMember);
                });
            });

            MessagingCenter.Subscribe<LocationSender, InvitationVm>(this, LocationSender.GroupInvitationReceivedMsg,
            (sender, invitationVm) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    GroupInvitationNotification.DisplayGroupInvitationNotification(invitationVm.PhoneNumber, invitationVm.Name);
                });
            });

            MessagingCenter.Subscribe<LocationSender, GroupMemberSimpleVm>(this, LocationSender.SomeoneLeftMsg,
            (sender, groupMemberSimpleVm) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    LeftGroupNotification.DisplayLostNotification(groupMemberSimpleVm.PhoneNumber, groupMemberSimpleVm.Name);
                });
            });

            MessagingCenter.Subscribe<LocationSender, GroupMemberSimpleVm>(this, LocationSender.MemberAlreadyInGroupMsg,
            (sender, groupMemberSimpleVm) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    InAnotherGroupNotification.DisplayInAnotherGroupNotification(groupMemberSimpleVm.PhoneNumber, groupMemberSimpleVm.Name);
                });
            });
        }

        public override IBinder OnBind(Intent intent)
        {
            _binder = new LocationSenderBinder(this);
            return _binder;
        }
    }
}