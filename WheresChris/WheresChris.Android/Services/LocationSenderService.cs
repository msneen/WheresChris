using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using StayTogether;
using StayTogether.Droid.NotificationCenter;
using StayTogether.Droid.Services;
using WheresChris.Helpers;

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
            var locationPermissionGranted = await PermissionHelper.HasOrRequestLocationPermission();
            if (!locationPermissionGranted) return;

            var phonePermissionGranted = await PermissionHelper.HasOrRequestPhonePermission();
            if (!phonePermissionGranted) return;

            _locationSender = await LocationSender.GetInstanceAsync();
            if (_locationSender == null) return;

            _locationSender.OnSomeoneIsLost += (sender, args) =>
            {
                LostNotification.DisplayLostNotification(args.GroupMember);
            };
            _locationSender.OnGroupInvitationReceived += (sender, args) => 
            {
                GroupInvitationNotification.DisplayGroupInvitationNotification(args.GroupId, args.Name);
            };

            _locationSender.OnSomeoneLeft += (sender, args) =>
            {                
                LeftGroupNotification.DisplayLostNotification(args.PhoneNumber, args.Name);
            };
            _locationSender.OnSomeoneAlreadyInAnotherGroup += (sender, args) =>
            {
                InAnotherGroupNotification.DisplayInAnotherGroupNotification(args.PhoneNumber, args.Name);
            };
        }

        public override IBinder OnBind(Intent intent)
        {
            _binder = new LocationSenderBinder(this);
            return _binder;
        }
    }
}