
using Foundation;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;
using Microsoft.Azure.Mobile.Crashes;
using Microsoft.Azure.Mobile.Distribute;
using Plugin.Toasts;
using StayTogether.iOS.NotificationCenter;
using TK.CustomMap.iOSUnified;
using UIKit;
using UserNotifications;
using WheresChris.Helpers;
using WheresChris.iOS.Classes;
using XLabs.Forms;

namespace WheresChris.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : XFormsApplicationDelegate //global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
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
			//LoadApplication(new App());

            MobileCenter.Start("2cd11ff1-c5b1-47d8-ac96-9fa5b74a47bd", typeof(Analytics), typeof(Crashes), typeof(Distribute));

            Xamarin.FormsMaps.Init();
            TKCustomMapRenderer.InitMapRenderer();

            NotificationManager.RegisterNotifications(app);
            NotificationManager.InitializeNotifications(options, Window);
            ToastNotification.Init();

            InitializeToastPlugin(app);

            TryToStartLocationService();

            return base.FinishedLaunching(app, options);
		}

	    private static void InitializeToastPlugin(UIApplication app)
	    {
	        if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
	        {
	            // Request Permissions
	            UNUserNotificationCenter.Current.RequestAuthorization(
	                UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound,
	                (granted, error) =>
	                {
	                    // Do something if needed
	                });
	        }
	        else if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
	        {
	            var notificationSettings = UIUserNotificationSettings.GetSettingsForTypes(
	                UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound, null);

	            app.RegisterUserNotificationSettings(notificationSettings);
	        }
	    }

	    private void InitializeBackgroundLocation()
	    {
	        var phoneNumber = SettingsHelper.GetPhoneNumber();
	        if (!string.IsNullOrWhiteSpace(phoneNumber))
	        {
	            LocationManager = new LocationManager();
	            InitializeEvents(LocationManager);
	            LocationManager.UserPhoneNumber = phoneNumber;
	            LocationManager.StartLocationUpdates();
	        }
	    }

	    public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
        {
            NotificationStrategyHandler.ReceiveNotification(notification, Window);
        }

	    private async void TryToStartLocationService()
	    {
	        var count = 0;
	        while (count < 3)
	        {
	            var phonePermissionGranted = PermissionHelper.HasPhonePermission();
	            var locationPermissionGranted = PermissionHelper.HasLocationPermission();
	            var contactPermissionGranted = PermissionHelper.HasContactPermission();

	            if (locationPermissionGranted && phonePermissionGranted && contactPermissionGranted)
	            {
                    InitializeBackgroundLocation();
                    LoadApplication(new App());
	                return;
	            }
	            if (!locationPermissionGranted)
	            {
	                await PermissionHelper.RequestLocationPermission();
	            }
	            else if (!phonePermissionGranted)
	            {
	                await PermissionHelper.RequestPhonePermission();
	            }
	            else
	            {
	                await PermissionHelper.RequestContactPermission();
	            }
	            count++;
	        }
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
