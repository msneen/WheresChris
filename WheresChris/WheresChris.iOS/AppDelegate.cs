
using Foundation;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;
using Microsoft.Azure.Mobile.Crashes;
using StayTogether.iOS.NotificationCenter;
using UIKit;
using WheresChris.iOS.Classes;

namespace WheresChris.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
	    public static LocationManager LocationManager = null;
        private bool _eventsInitialized;

        public override UIWindow Window
        {
            get;
            set;
        }

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init();
			LoadApplication(new App());

            MobileCenter.Start("2cd11ff1-c5b1-47d8-ac96-9fa5b74a47bd", typeof(Analytics), typeof(Crashes));

            Xamarin.FormsMaps.Init();

            LocationManager = new LocationManager();
            InitializeEvents(LocationManager);
            LocationManager.StartLocationUpdates();
            NotificationManager.RegisterNotifications(app);
            NotificationManager.InitializeNotifications(options, Window);

            return base.FinishedLaunching(app, options);
		}

        public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
        {
            NotificationStrategyHandler.ReceiveNotification(notification, Window);
        }

        private void InitializeEvents(LocationManager manager)
        {

            if (manager?.LocationSender == null || _eventsInitialized) return;

            manager.LocationSender.OnSomeoneIsLost += (sender, args) =>
            {
                LostNotification.DisplayLostNotification(args.GroupMember);
            };

            manager.LocationSender.OnGroupInvitationReceived += (sender, args) =>
            {
                GroupInvitationNotification.DisplayGroupInvitationNotification(args.GroupId, args.Name);
            };
            manager.LocationSender.OnGroupJoined += (sender, args) =>
            {

            };
            manager.LocationSender.OnSomeoneLeft += (sender, args) =>
            {
                LeftGroupNotification.DisplayGroupInvitationNotification(args.PhoneNumber, args.Name);
            };
            manager.LocationSender.OnSomeoneAlreadyInAnotherGroup += (sender, args) =>
            {
                InAnotherGroupNotification.DisplayInAnotherGroupNotification(args.PhoneNumber, args.Name);
            };
            manager.LocationSender.OnGroupDisbanded += (sender, args) =>
            {

            };
            _eventsInitialized = true;
        }
    }
}
