using System.Collections.Generic;
using Foundation;
using Google.MobileAds;
using KeyboardOverlap.Forms.Plugin.iOSUnified;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Distribute;
using Plugin.Toasts;
using StayTogether;
using StayTogether.Classes;
using StayTogether.Helpers;
using StayTogether.iOS.NotificationCenter;
using StayTogether.Models;
using TK.CustomMap.iOSUnified;
using UIKit;
using UserNotifications;
using WheresChris.Helpers;
using WheresChris.iOS.Classes;
using WheresChris.iOS.NotificationCenter;
using WheresChris.ViewModels;
using Xamarin.Forms;
using Device = Xamarin.Forms.Device;


namespace WheresChris.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate //XFormsApplicationDelegate //
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

            AppCenter.Start("2cd11ff1-c5b1-47d8-ac96-9fa5b74a47bd", typeof(Analytics), typeof(Crashes), typeof(Distribute));
            
		    KeyboardOverlapRenderer.Init ();
            Xamarin.FormsMaps.Init();
            TKCustomMapRenderer.InitMapRenderer();
		    DependencyService.Register<ToastNotification>();
            ToastNotification.Init();
            InitializeToastPlugin(app);

            MobileAds.Configure("ca-app-pub-5660348862902976~4046598647");

            LoadApplication(new App());

            NotificationManager.RegisterNotifications(app);
            NotificationManager.InitializeNotifications(options, UIApplication.SharedApplication.KeyWindow);

            Analytics.TrackEvent("AppDelegate_InitializingTimer");
            _interval = new Interval();
            _interval.SetInterval(TryToStartLocationService, 10000);

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
            //Crashes.TrackError("AppDelegate_InitializeBackgroundLocation_StartLocationUpdates");
            LocationManager.StartLocationUpdates();

            //Crashes.TrackError("AppDelegate_InitializeBackgroundLocation_InitializeEvents");
            InitializeEvents(LocationManager);

            //Crashes.TrackError("AppDelegate_InitializeBackgroundLocation_Finished");
        }

	    public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
        {
            //application.KeyWindow
            NotificationStrategyHandler.ReceiveNotification(notification, UIApplication.SharedApplication.KeyWindow);
        }

	    private void TryToStartLocationService()
	    {
	        try
	        {
                //var count = 0;
                //while (count < 3)
                //{

                    //var phonePermissionGranted = await PermissionHelper.HasOrRequestPhonePermission();
                    //var locationPermissionGranted = await PermissionHelper.HasOrRequestLocationPermission();
                    //var contactPermissionGranted = await PermissionHelper.HasOrRequestContactPermission();

                    //if (locationPermissionGranted && phonePermissionGranted && contactPermissionGranted)
                    //{
                InitializeBackgroundLocation();
                        //return;
                    //}
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
                Crashes.TrackError(ex, new Dictionary<string, string>
                {
                    {"Source", ex.Source },
                    { "stackTrace",ex.StackTrace}
                });
            }
        }

	    private void InitializeEvents(LocationManager manager)
        {

            if (manager?.LocationSender == null || _eventsInitialized) return;

            MessagingCenter.Subscribe<LocationSender>(this, LocationSender.PhoneNumberMissingMsg,
            (sender) =>
            {
                Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                {
                    PhoneNumberMissingNotification.DisplayGroupInvitationNotification();
                });
            });

            MessagingCenter.Subscribe<LocationSender, GroupMemberVm>(this, LocationSender.SomeoneIsLostMsg,
            (sender, groupMember) =>
            {
                Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                {
                    LostNotification.DisplayLostNotification(groupMember);
                });
            });

            MessagingCenter.Subscribe<LocationSender, InvitationVm>(this, LocationSender.GroupInvitationReceivedMsg,
            (sender, invitationVm) =>
            {
                Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                {
                    GroupInvitationNotification.DisplayGroupInvitationNotification(invitationVm.PhoneNumber, invitationVm.Name);
                });
            });

            MessagingCenter.Subscribe<LocationSender, GroupMemberSimpleVm>(this, LocationSender.SomeoneLeftMsg,
            (sender, groupMemberSimpleVm) =>
            {
                Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                {
                    LeftGroupNotification.DisplayGroupInvitationNotification(groupMemberSimpleVm.PhoneNumber, groupMemberSimpleVm.Name);
                });
            });

            MessagingCenter.Subscribe<LocationSender, GroupMemberSimpleVm>(this, LocationSender.MemberAlreadyInGroupMsg,
            (sender, groupMemberSimpleVm) =>
            {
                Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                {
                    InAnotherGroupNotification.DisplayInAnotherGroupNotification(groupMemberSimpleVm.PhoneNumber, groupMemberSimpleVm.Name);
                });
            });

            MessagingCenter.Subscribe<LocationSender, List<GroupMemberSimpleVm>>(this, LocationSender.AdditionalMembersRequestJoinGroup,
                (sender, groupMemberSimpleListVm) =>
                {
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                    {
                        RequestToJoinGroupNotification.RequestToJoinThisGroup(groupMemberSimpleListVm);
                    });
                });

            MessagingCenter.Subscribe<LocationSender, ChatMessageSimpleVm>(this, LocationSender.ChatReceivedMsg,
                (sender, chatMessageVm) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ChatMessageReceivedNotification.DisplayChatMessageReceivedNotification(chatMessageVm);
                    });
                });

            MessagingCenter.Subscribe<MessagingCenterSender>(this, LocationSender.ChatNotificationCancelMsg, (sender) =>
            {
                Device.BeginInvokeOnMainThread(ChatMessageReceivedNotification.CancelNotification);                
            });

            //Crashes.TrackError("IPhoneLocationEventsInitialized");
            _eventsInitialized = true;
        }
    }
}
