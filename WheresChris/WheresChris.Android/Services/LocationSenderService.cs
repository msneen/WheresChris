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
        public LocationSender LocationSender;

        public static LocationSenderService Instance;

        public void StartForeground()
        {
#if (DEBUG)
            var notification = DisplayServiceNotification();
#endif
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
                .SetContentText("Watching Chris...")
                .SetContentIntent(pendingIntent).Build();
            return notification;
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


        private async void SendFirstPositionUpdate(string phoneNumber)
        {
            var mapPosition = await PositionHelper.GetMapPosition();//var position = GpsService.GetLocation();
            if (!mapPosition.HasValue) return;

            var position = PositionConverter.Convert(mapPosition.Value);
            if (position == null) return;

            var groupMemberVm = GroupMemberConverter.Convert(position);
            groupMemberVm.PhoneNumber = phoneNumber;
            LocationSender.SendUpdatePosition(groupMemberVm);
        }

        private void InitializeLocationSender(string phoneNumber)
        {
            LocationSender = new LocationSender();
            LocationSender.InitializeSignalRAsync();
            LocationSender.OnSomeoneIsLost += (sender, args) =>
            {
                LostNotification.DisplayLostNotification(args.GroupMember);//OnNotifySomeoneIsLost(args.GroupMember);
            };
            LocationSender.OnGroupInvitationReceived += (sender, args) => 
            {
                GroupInvitationNotification.DisplayGroupInvitationNotification(args.GroupId, args.Name);
            };
            //LocationSender.OnGroupJoined += (sender, args) =>
            //{

            //};
            //LocationSender.OnGroupDisbanded +=(sender, args) =>
            //{

            //};
            LocationSender.OnSomeoneLeft += (sender, args) =>
            {
                LeftGroupNotification.DisplayLostNotification(args.PhoneNumber, args.Name);
            };
            LocationSender.OnSomeoneAlreadyInAnotherGroup += (sender, args) =>
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