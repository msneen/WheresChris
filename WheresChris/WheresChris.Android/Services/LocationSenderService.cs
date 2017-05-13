using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using StayTogether;
using StayTogether.Droid.NotificationCenter;
using StayTogether.Droid.Services;
using StayTogether.Helpers;
using StayTogether.Location;
using WheresChris.Helpers;

namespace WheresChris.Droid.Services
{

    [Service]
    // ReSharper disable once RedundantExplicitArrayCreation
    [IntentFilter(new string[] {"com.StayTogether.Droid.LocationSenderService"})]
    public class LocationSenderService : Service
    {
        private LocationSenderBinder _binder;
        public LocationSender _LocationSender;

        public static LocationSenderService Instance;

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
                .SetContentText("Wheres Chris")
                .SetContentIntent(pendingIntent).Build();
            return notification;
        }

        private void StartLocationSender()
        {
            InitializeLocationSender();
        }

        private void InitializeLocationSender()
        {
            Task.Run(async () => { _LocationSender = await LocationSender.GetInstance(); }).Wait();

            _LocationSender.OnSomeoneIsLost += (sender, args) =>
            {
                LostNotification.DisplayLostNotification(args.GroupMember);//OnNotifySomeoneIsLost(args.GroupMember);
            };
            _LocationSender.OnGroupInvitationReceived += (sender, args) => 
            {
                GroupInvitationNotification.DisplayGroupInvitationNotification(args.GroupId, args.Name);
            };
            //_LocationSender.OnGroupJoined += (sender, args) =>
            //{

            //};
            //_LocationSender.OnGroupDisbanded +=(sender, args) =>
            //{

            //};
            _LocationSender.OnSomeoneLeft += (sender, args) =>
            {                
                LeftGroupNotification.DisplayLostNotification(args.PhoneNumber, args.Name);
            };
            _LocationSender.OnSomeoneAlreadyInAnotherGroup += (sender, args) =>
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