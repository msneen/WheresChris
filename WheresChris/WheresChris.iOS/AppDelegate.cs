using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Google.MobileAds;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;
using Microsoft.Azure.Mobile.Crashes;
using Microsoft.Azure.Mobile.Distribute;
using Plugin.Toasts;
using StayTogether.Helpers;
using StayTogether.iOS.NotificationCenter;
using TK.CustomMap.iOSUnified;
using UIKit;
using UserNotifications;
using WheresChris.Helpers;
using WheresChris.iOS.Classes;
using WheresChris.iOS.NotificationCenter;
using XLabs.Forms;

namespace WheresChris.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : XFormsApplicationDelegate //global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
	    public static LocationManager LocationManager = null;
        private bool _eventsInitialized;
	    private Interval _interval;

	    public override UIWindow Window
        {
            get;
            set;
        }

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init();

            MobileCenter.Start("2cd11ff1-c5b1-47d8-ac96-9fa5b74a47bd", typeof(Analytics), typeof(Crashes)/*, typeof(Distribute)*/);
            

            Xamarin.FormsMaps.Init();
            TKCustomMapRenderer.InitMapRenderer();
            ToastNotification.Init();
            InitializeToastPlugin(app);
            MobileAds.Configure("ca-app-pub-5660348862902976~4046598647");

            LoadApplication(new App());

            NotificationManager.RegisterNotifications(app);
            NotificationManager.InitializeNotifications(options, UIApplication.SharedApplication.KeyWindow);

            //Analytics.TrackEvent("AppDelegate_InitializingTimer");
            _interval = new Interval();
            _interval.SetInterval(TryToStartLocationService().Wait , 10000);

            return base.FinishedLaunching(app, options);
		}

	    public override async void WillTerminate(UIApplication application)
	    {
	        var leaveGroup = LocationManager?.LocationSender?.LeaveGroup();
	        if (leaveGroup != null) await leaveGroup;
	        var endGroup = LocationManager?.LocationSender?.EndGroup();
	        if (endGroup != null) await endGroup;

	        await PermissionHelper.GetNecessaryPermissionInformation();//for troubleshooting wayne's phone crash
	        base.WillTerminate(application);
	    }
        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            Distribute.OpenUrl(url);

            return true;
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
            LocationManager = new LocationManager();

            var phoneNumber = SettingsHelper.GetPhoneNumber();
            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                LocationManager.UserPhoneNumber = phoneNumber;
            }
            //Analytics.TrackEvent("AppDelegate_InitializeBackgroundLocation_StartLocationUpdates");
            LocationManager.StartLocationUpdates();

            //Analytics.TrackEvent("AppDelegate_InitializeBackgroundLocation_InitializeEvents");
            InitializeEvents(LocationManager);

            //Analytics.TrackEvent("AppDelegate_InitializeBackgroundLocation_Finished");
        }

	    public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
        {
            //application.KeyWindow
            NotificationStrategyHandler.ReceiveNotification(notification, UIApplication.SharedApplication.KeyWindow);
        }

	    private async Task TryToStartLocationService()
	    {
	        try
	        {
                //var count = 0;
                //while (count < 3)
                //{

                    var phonePermissionGranted = await PermissionHelper.HasPhonePermission();
                    var locationPermissionGranted = await PermissionHelper.HasLocationPermission();
                    var contactPermissionGranted = await PermissionHelper.HasContactPermission();

                    if (locationPermissionGranted && phonePermissionGranted && contactPermissionGranted)
                    {
                        InitializeBackgroundLocation();
                        //return;
                    }
                    //if (!locationPermissionGranted)
                    //{
                    //    PermissionHelper.RequestLocationPermission().Wait();
                    //}
                    //if (!phonePermissionGranted)
                    //{
                    //    PermissionHelper.RequestPhonePermission().Wait();
                    //}
                    //else
                    //{
                    //    PermissionHelper.RequestContactPermission().Wait();
                    //}
                    //Task.Delay(10000);
                    //count++;
               // }
            }
            catch (System.Exception ex)
            {
                Analytics.TrackEvent("TryToStartLocationService", new Dictionary<string, string>
                {
                    { "Error",  ex.Message}
                });
            }
        }

	    private void InitializeEvents(LocationManager manager)
        {

            if (manager?.LocationSender == null || _eventsInitialized) return;

	        manager.LocationSender.OnPhoneNumberMissing += (sender, args) =>
	        {
                PhoneNumberMissingNotification.DisplayGroupInvitationNotification();
	        };

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
            //Analytics.TrackEvent("IPhoneLocationEventsInitialized");
            _eventsInitialized = true;
        }
    }
}
